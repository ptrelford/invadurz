namespace Invadurz

open System
open System.Windows
open System.Windows.Controls
open System.Windows.Threading

type GameControl () as control =
    inherit UserControl ()
        
    #if SILVERLIGHT
    #if WP7
    #else
    let uri = Uri("/Invadurz;component/GameControl.xaml", UriKind.Relative)
    do  Application.LoadComponent(control, uri)
    #endif
    #else // WPF specific workaround for keyboard input
    do  control.IsTabStop <- true
    do  control.Focusable <- true
    do  control.Focus() |> ignore
    #endif

    let mutable disposables = []
    let remember disposable = disposables <- disposable :: disposables
    let forget () =
        disposables |> List.iter (fun (d:IDisposable) -> d.Dispose())
        disposables <- []

    let width, height = 512.0, 384.0
    let screen = Canvas(Background=skyBrush)
    do  screen.Width <- width; screen.Height <- height
    let add (x:#UIElement) = screen.Children.Add x |> ignore
    let remove (x:#UIElement) = screen.Children.Remove x |> ignore

    let playMedia name =
        #if SILVERLIGHT
        let info = System.Windows.Application.GetResourceStream(Uri(name + ".wav", UriKind.Relative))
        let effect = Microsoft.Xna.Framework.Audio.SoundEffect.FromStream(info.Stream)
        effect.Play() |> ignore
        #else
        let player = System.Windows.Media.MediaPlayer()
        player.Open(Uri(name + ".mp3", UriKind.Relative))
        player.Play()
        #endif

    let onFire () = playMedia "shoot"
    let onKill () = playMedia "invaderkilled"
    let onExplode () = playMedia "explosion"

    let layout = Grid()
    do  layout.RowDefinitions.Add(RowDefinition(Height=GridLength(height)))
    do  layout.RowDefinitions.Add(RowDefinition())
    do  layout.ColumnDefinitions.Add(ColumnDefinition())
    do  layout.ColumnDefinitions.Add(ColumnDefinition(Width=GridLength(width)))
    do  layout.ColumnDefinitions.Add(ColumnDefinition())
    let left = Button(Content="<",Width=96.0, Height=80.0)
    do  Grid.SetColumn(screen,1)
    do  layout.Children.Add screen |> ignore
    let right = Button(Content=">", Width=96.0, Height=80.0)
    do  Grid.SetColumn(right,2)
    let fire = Button(Content="Fire", Width=256.0, Height=80.0)
    do  Grid.SetColumn(fire,1)
    do  Grid.SetRow(fire,1)
    do  control.Content <- layout

    let keys = Keys(control.KeyDown,control.KeyUp,remember)
    let mouse = Mouse(screen, screen.MouseLeftButtonDown, screen.MouseLeftButtonUp, remember)

    let particles = Particles(screen.Children, width, height)
    let missiles = Weapons(screen.Children,height)
    let cannon = Cannon(width,height,missiles,onFire)
    let bunkers = Bunkers(screen.Children,width,height)
    let bombs = Weapons(screen.Children,height)
    let sheet = Sheet(screen.Children,width,bombs)
    do  sheet.Clean()

    let score = TextValue("SCORE",0,TextAlignment.Left,width)
    let highScore = if settings.Contains "HighScore" then settings.["HighScore"] :?> int else 0
    let highScore = TextValue("HIGH",highScore,TextAlignment.Center,width)
    let lives = TextValue("LIVES",3,TextAlignment.Right,width)
    do  [score.Control;highScore.Control;lives.Control] |> List.iter add

    let updateScore () =
        let missilesHit, invadersHit =
            sheet.Aliens |> Seq.fold (fun acc alien ->
                missiles.Items |> Seq.tryFind (fun missile ->
                    alien.HitTest(missile.X - sheet.X, missile.Y - sheet.Y)                    
                )
                |> (function 
                    | Some missile -> (missile,alien)::acc 
                    | None -> acc)
            ) []
            |> List.unzip
        sheet.Remove invadersHit
        missilesHit |> List.iter (fun (missile) -> particles.Explode(missile.X,missile.Y))
        missiles.Remove (missilesHit |> List.map (fun missile -> missile.Shape))
        if invadersHit.Length > 0 then
            onKill()
            score.Value <- score.Value + invadersHit.Length * 10

    let updateScreen () =        
        sheet.Update(lives.Value>0)
        missiles.Update(bunkers.HitTest)
        bombs.Update(bunkers.HitTest)
        particles.Update()

    let buttonActions () =
        set [
            if left.IsPressed then yield Action.Left
            if right.IsPressed then yield Action.Right
            if fire.IsPressed then yield Action.Fire
        ]

    let getActions () =
        keys.Actions + mouse.Actions cannon.X + buttonActions ()

    let play () =
        getActions() |> cannon.Update
        updateScreen ()
        updateScore()

    let isOverrun () = 
        sheet.Y + sheet.Height >= height ||
        cannon.IsHit (sheet.X,sheet.Y,sheet.Aliens)

    let game () = seq {
        add cannon.Control
        while lives.Value > 0 do
            cannon.Control.Opacity <- 0.5
            for i = 1 to 25 do play() |> ignore; yield ()
            cannon.Control.Opacity <- 1.0
            while (play(); 
                    not (cannon.IsHit bombs) && 
                    not (isOverrun ()) &&
                    sheet.Aliens.Length>0) do yield ()
            if cannon.IsHit bombs then
                onExplode ()
                lives.Value <- lives.Value - 1
                remove cannon.Control
                for i = 1 to 25 do updateScreen(); yield ()
                if lives.Value > 0 then add cannon.Control
            elif isOverrun() then
                onExplode ()
                lives.Value <- lives.Value - 1
                remove cannon.Control
                let mess = createMessage "OVERRUN"
                Grid.SetColumn(mess,1)
                layout.Children.Add mess |> ignore
                for i = 1 to 50 do updateScreen(); yield()
                layout.Children.Remove mess |> ignore
                if lives.Value > 0 then 
                    sheet.Renew()
                    add cannon.Control
            elif sheet.Aliens.Length = 0 then
                let mess = createMessage "COMPLETED"
                Grid.SetColumn(mess,1)
                layout.Children.Add mess |> ignore
                for i = 1 to 100 do updateScreen(); yield()
                layout.Children.Remove mess |> ignore
                sheet.Renew()

        }

    let updateLoop () = seq {
        while true do
            layout.Children.Add(left) |> ignore
            layout.Children.Add(right) |> ignore
            layout.Children.Add(fire) |> ignore
            yield! game ()
            layout.Children.Remove(left) |> ignore
            layout.Children.Remove(right) |> ignore
            layout.Children.Remove(fire) |> ignore
            if score.Value > highScore.Value then
                highScore.Value <- score.Value
                settings.["HighScore"] <- highScore.Value
            let mess = createMessage "GAME OVER"
            Grid.SetColumn(mess,1)
            layout.Children.Add mess |> ignore
            for i = 1 to 100 do updateScreen(); yield()
            while not (getActions() |> Set.contains Fire) do yield()
            layout.Children.Remove mess |> ignore
            bunkers.Reset ()
            lives.Value <- 3
            score.Value <- 0
            sheet.Clean()
        }

    let startGame () =
        let e = (updateLoop ()).GetEnumerator()
        let timer = DispatcherTimer()
        timer.Interval <- TimeSpan.FromMilliseconds(1000.0/50.0)
        timer.Tick
        |> Observable.subscribe (fun _ -> e.MoveNext() |> ignore)
        |> remember
        timer.Start()

    do  let mess = createMessage "Click to Start"
        Grid.SetColumn(mess,1)
        layout.Children.Add mess |> ignore
        control.MouseLeftButtonUp
        |> Observable.subscribe (fun _ ->
            forget()
            keys.StartListening()
            mouse.StartListening()
            layout.Children.Remove mess |> ignore
            startGame() 
        )
        |> remember

    interface System.IDisposable with
        member this.Dispose() = forget()