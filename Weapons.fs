namespace Invadurz

open System.Windows.Controls
open System.Windows.Media
open System.Windows.Shapes

type Weapons(container:UIElementCollection,height) =
    let mutable weapons = []
    member this.Fire(weapon:Shape,x,y,dy) =        
        container.Add weapon
        let transform = TranslateTransform(X=x,Y=y)
        weapon.RenderTransform <- transform
        weapons <- (weapon,transform,x,y,dy) :: weapons
    member this.Update(hitTest:(float * float * float) -> bool) = 
        let current, expired =
            weapons
            |> List.map (fun (m,t,x,y,dy) -> (m,t,x,y+dy,dy))
            |> List.partition (fun (_,_,x,y,dy) -> 
                y>0.0 && y<height && not (hitTest(x,y,dy))
            )
        expired |> List.iter (fun (weapon:Shape,_,_,_,_) -> 
            container.Remove weapon |> ignore
        )
        current |> List.iter (fun (_,t,x,y,_) -> 
            t.X <- x; t.Y <- y
        )
        weapons <- current
    member this.Remove(expired:_ list) =
        expired |> List.iter (fun (weapon:Shape) -> 
            container.Remove weapon |> ignore
        )
        weapons <- weapons |> List.filter (fun (weapon,_,_,_,_) -> 
            expired |> List.exists((=) weapon) |> not
        )
    member this.Items = weapons