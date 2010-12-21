namespace Invadurz

open System.Windows.Controls

type Weapons(container:UIElementCollection,height) =
    let mutable weapons = []
    member this.Fire(weapon:Sprite,x,y,dy) =
        container.Add weapon.Control
        weapons <- (weapon,x,y,dy) :: weapons
    member this.Update(hitTest:(float * float * float) -> bool) = 
        let current, expired =
            weapons
            |> List.map (fun (m,x,y,dy) -> (m,x,y+dy,dy))
            |> List.partition (fun (_,x,y,dy) -> 
                y>0.0 && y<height && not (hitTest(x,y,dy))
            )
        expired |> List.iter (fun (weapon:Sprite,_,_,_) -> container.Remove weapon.Control |> ignore)
        current |> List.iter (fun (weapon,x,y,_) -> weapon.MoveTo(x,y))
        weapons <- current
    member this.Remove(expired:_ list) =
        expired |> List.iter (fun (weapon:Sprite) -> container.Remove weapon.Control |> ignore)
        weapons <- weapons |> List.filter (fun (weapon,_,_,_) -> expired |> List.exists((=) weapon) |> not)
    member this.Items = weapons