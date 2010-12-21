namespace Invadurz

open System.Windows.Controls

type Bunkers (container:UIElementCollection,width,height) =
    let add,remove = container.Add,(fun item -> container.Remove item |> ignore)
    let mutable bunkers = []
    let createBunkers () =
        bunkers <-
            [1..4] 
            |> List.map (fun x -> Bunker((float x*width/5.0)-8.0,height-64.0))
        bunkers |> List.map (fun bunker -> bunker.Sprite.Control) |> List.iter add
    do createBunkers ()
    let removeBunkers () = 
        bunkers |> List.map (fun bunker -> bunker.Sprite.Control) |> List.iter remove
    let bunkerHitTest (x,y,dy) =
        bunkers |> List.exists (fun bunker -> 
            let y = if dy > 0.0 then y+6.0 else y
            bunker.Busted(x+1.0,y,dy)
        )
    member this.HitTest (x,y,dy) = bunkerHitTest(x,y,dy)
    member this.Reset () =
        removeBunkers ()
        createBunkers ()

