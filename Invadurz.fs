namespace Invadurz

open System
open System.Windows
open System.Windows.Controls
open System.Windows.Threading

type GameControl () as control =
    inherit UserControl ()

    let uri = Uri("/Invadurz;component/GameControl.xaml", UriKind.Relative)
    do  Application.LoadComponent(control, uri)

    let mutable disposables = []
    let remember disposable = disposables <- disposable :: disposables
    let forget () =
        disposables |> List.iter (fun (d:IDisposable) -> d.Dispose())
        disposables <- []

    let width, height = 512.0, 384.0
    do  control.Width <- width; control.Height <- height

    let screen = Canvas(Background=skyBrush)
    let add (x:#UIElement) = screen.Children.Add x
    let remove (x:#UIElement) = screen.Children.Remove x |> ignore

    let playMedia name =
        let me = MediaElement(AutoPlay=true)
        me.Source <- Uri(name, UriKind.Relative)
        add me
        me.CurrentStateChanged
        |> Observable.filter (fun _  -> me.CurrentState = Media.MediaElementState.Paused)
        |> Observable.run (fun _ -> remove me)

    let onFire () = playMedia "/shoot.mp3"
    let onKill () = playMedia "/invaderkilled.mp3"
    let onExplode () = playMedia "/explosion.mp3"

    let layout = Grid()
    do  layout.Children.Add screen
    do  control.Content <- layout

    let keys = Keys(control.KeyDown,control.KeyUp,remember)

    let particles = Particles(screen.Children)
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
                missiles.Items |> Seq.tryFind (fun (_,x,y,_) ->
                    alien.HitTest(x - sheet.X, y - sheet.Y)
                )
                |> (function 
                    | Some (missile,_,_,_) -> (missile,alien)::acc 
                    | None -> acc)
            ) []
            |> List.unzip
        sheet.Remove invadersHit
        missilesHit |> List.iter (fun missile -> particles.Explode(missile.X,missile.Y))
        missiles.Remove missilesHit
        if invadersHit.Length > 0 then
            onKill()
            score.Value <- score.Value + invadersHit.Length * 10

    let updateScreen () =
        sheet.Update(lives.Value>0)
        missiles.Update(bunkers.HitTest)
        bombs.Update(bunkers.HitTest)
        particles.Update()

    let play () =
        keys |> cannon.Update
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
                layout.Children.Add mess
                for i = 1 to 50 do updateScreen(); yield()
                layout.Children.Remove mess |> ignore
                if lives.Value > 0 then 
                    sheet.Renew()
                    add cannon.Control
            elif sheet.Aliens.Length = 0 then
                let mess = createMessage "COMPLETED"
                layout.Children.Add mess
                for i = 1 to 100 do updateScreen(); yield()
                layout.Children.Remove mess |> ignore
                sheet.Renew()
        }

    let updateLoop () = seq {
        while true do
            yield! game ()
            if score.Value > highScore.Value then
                highScore.Value <- score.Value
                settings.["HighScore"] <- highScore.Value
            let mess = createMessage "GAME OVER"
            layout.Children.Add mess
            for i = 1 to 100 do updateScreen(); yield()
            while not (keys.Down Fire) do yield()
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
        layout.Children.Add mess
        control.MouseLeftButtonUp
        |> Observable.subscribe (fun _ ->
            forget()
            keys.StartListening()
            layout.Children.Remove mess |> ignore
            startGame() 
        )
        |> remember

    interface System.IDisposable with
        member this.Dispose() = forget()