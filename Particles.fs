namespace Invadurz

open System
open System.Windows.Controls
open System.Windows.Shapes

type Particles (container:UIElementCollection) =
    let mutable particles = []
    let explode (x,y) =
        let boom (dx,dy) = 
            let line = Line(X1=dx,Y1=dy,X2=0.0,Y2=0.0,Stroke=whiteBrush)
            let sprite = Sprite(line,x+dx,y+dy,[])
            (sprite,x,y,dx,dy,100)
        [1..100] 
        |> List.map (fun _ ->
            let r = rand.NextDouble() * 2.0 * Math.PI
            let v = 4.0 *  rand.NextDouble() + 2.0
            v * cos r, v * sin r 
        )
        |> List.map boom
    let updateParticles () =
        let alive, dead =
            particles 
            |> List.map (fun (p,x,y,dx,dy,count) -> p,x+dx,y+dy,dx,dy,count-1)
            |> List.partition (fun (p,x,y,dx,dy,count) -> count>0)
        dead |> List.iter (fun (p:Sprite,x,y,dx,dy,count) -> container.Remove p.Control |> ignore)
        particles <- alive
        particles |> List.iter (fun (p,x,y,_,_,_) -> p.MoveTo(x,y))
    member thuis.Update() = updateParticles()
    member this.Explode(x,y) =
        let fragments = explode(x,y)
        fragments |> List.iter (fun (sprite,_,_,_,_,_) -> sprite.Control |> container.Add)
        particles <- particles @ fragments