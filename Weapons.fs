namespace Invadurz

open System.Collections.Generic
open System.Windows.Controls
open System.Windows.Media
open System.Windows.Shapes

type Weapon = {
    Shape : Shape
    Transform : TranslateTransform
    X : float
    mutable Y : float
    DY : float
    }

type Weapons(container:UIElementCollection,height) =
    let mutable weapons = List<_>(capacity=100)
    member this.Fire(shape:Shape,x,y,dy) =        
        container.Add shape
        let transform = TranslateTransform(X=x,Y=y)
        shape.RenderTransform <- transform               
        { Shape=shape; Transform=transform; X=x; Y=y; DY=dy }
        |> weapons.Add
    member this.Update(hitTest:(float * float * float) -> bool) = 
        for weapon in weapons do
            weapon.Y <- weapon.Y + weapon.DY
            weapon.Transform.Y <- weapon.Y            
        for index = weapons.Count-1 downto 0 do
            let weapon = weapons.[index]
            let x, y, dy = weapon.X, weapon.Y, weapon.DY
            if y<0.0 || y>height || (hitTest(x,y,dy))
            then
                weapons.RemoveAt index 
                container.Remove weapon.Shape |> ignore        
    member this.Remove(expired:_ list) =               
        expired |> List.iter (fun (weapon:Shape) -> 
            container.Remove weapon |> ignore
        )
        for index = weapons.Count-1 downto 0 do
            let weapon = weapons.[index]
            if expired |> Seq.exists ((=) weapon.Shape)
            then weapons.RemoveAt index 
    
    member this.Items = weapons