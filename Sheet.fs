namespace Invadurz

open System
open System.Windows
open System.Windows.Controls
open System.Windows.Shapes

type Sheet (container:UIElementCollection,width:float,bombs:Weapons) =
    let createRow (invader:(int * int[]) [],i) (x',y') =
        let bitmaps = invader |> Array.map toBitmap
        [|0..11|]
        |> Array.map (fun x ->
            let images = bitmaps |> Array.map toImage
            images |> Seq.iter (fun image -> image.Visibility <- Visibility.Collapsed)
            images.[i].Visibility <- Visibility.Visible
            let animation = seq {
                let i = ref i
                while true do
                    yield! yieldFor 6
                    images.[!i].Visibility <- Visibility.Collapsed
                    i := (!i+1) % images.Length
                    images.[!i].Visibility <- Visibility.Visible
            }
            Sprite (images |> Seq.cast, x' + 32.0*float x,y',animation)
        )
    let createAliens () =
        [|  alien1,0
            alien2,0
            alien2,1
            alien3,0
            alien3,1 |]
        |> Array.mapi (fun row alien -> (row,alien))
        |> Array.collect (fun (row,alien) -> createRow alien (0.0, 24.0*float row))

    let mutable sheetX, sheetY = 64.0, 24.0
    let mutable direction = 1.0
    let mutable aliens = [||]

    let createSheetControl() =
        let canvas = Canvas()
        aliens |> Seq.map Sprite.toControl |> Seq.iter canvas.Children.Add
        Canvas.setPosition canvas (sheetX,sheetY)
        canvas

    let mutable sheet = createSheetControl()

    let createSheet () = 
        aliens <- createAliens()
        sheetX <- 64.0
        sheetY <- 32.0
        sheet <- createSheetControl()

    let newSheet () = 
        container.Remove sheet |> ignore
        createSheet()
        container.Add sheet

    let update () =
        let startX, endX, count = 
            aliens |> Seq.fold (fun (lo,hi,count) alien ->
                let x = alien.X
                min x lo, max x hi, count+1
            ) (Double.MaxValue, Double.MinValue,0)
        if direction > 0.0 then (sheetX + direction) > (width - (endX+24.0))
        else sheetX + startX - direction < 0.0
        |> (function
            | true -> sheetY <- sheetY + 8.0; direction <- -direction
            | false -> sheetX <- sheetX + (((float (60 - count))/15.0 + 1.0) * direction)
        )
        Canvas.setPosition sheet (sheetX,sheetY)
        aliens |> Seq.iter (fun alien -> alien.Animate())
        count

    let fireBombs count =
        aliens |> Seq.iter (fun alien ->
            if rand.Next(count * 16) = 0 then
                let rectangle = Rectangle(Width=2.0,Height=4.0, Fill=whiteBrush)
                let lines = 
                    [0.0,0.0,2.0,2.0;2.0,2.0,0.0,4.0;0.0,4.0,2.0,6.0]
                    |> List.map (fun (x1,y1,x2,y2) -> Line(X1=x1,Y1=y1,X2=x2,Y2=y2,Stroke=whiteBrush))
                let bomb = Sprite(lines |> Seq.cast,0.0,0.0,[])
                bombs.Fire(bomb,sheetX + alien.X+12.0,sheetY + alien.Y+12.0,3.0)
        )

    member this.Aliens= aliens
    member this.X = sheetX
    member this.Y = sheetY
    member this.Height =
        match aliens |> Seq.length with
        | 0 -> 0.0 
        | _ -> 
            aliens 
            |> Seq.map (fun alien -> alien.Y + 24.0)
            |> Seq.max
    member this.Clean() = 
        direction <- 1.0
        newSheet()
    member this.Renew () = 
        direction <- direction + 0.1
        newSheet()
    member this.Update (canFire) = 
        let count = update ()
        if canFire then fireBombs count
    member this.Remove (dead:Sprite list) =
        dead |> List.iter (fun invader -> sheet.Children.Remove invader.Control |> ignore)
        aliens <- aliens |> Array.filter (fun alien -> dead |> List.exists((=) alien) |> not)
