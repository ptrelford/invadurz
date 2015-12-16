namespace Invadurz

open System
open System.Windows.Controls
open System.Windows.Shapes

type Particles (container:UIElementCollection, width, height) =
    let mutable particles = []
    let setPositions lines =
        lines |> List.iter (fun (line,x,y,_,_,_) -> Canvas.setPosition line (x,y))
    let explode (x,y) =        
        let newTrajectory _ =
            let r = rand.NextDouble() * 2.0 * Math.PI
            let v = 4.0 *  rand.NextDouble() + 2.0
            v * cos r, v * sin r 
        let toLine (dx,dy) = 
            let line = Line(X1=dx,Y1=dy,X2=0.0,Y2=0.0,Stroke=whiteBrush)            
            (line,x,y,dx,dy,100)        
        let explosion = [1..100] |> List.map (newTrajectory >> toLine)
        setPositions explosion
        explosion |> List.iter (fun (line,_,_,_,_,_) -> line |> container.Add |> ignore)
        particles <- explosion @ particles
    let updateParticles () =
        let alive, dead =
            particles 
            |> List.map (fun (p,x,y,dx,dy,count) -> p,x+dx,y+dy,dx,dy,count-1)
            |> List.partition (fun (_,x,y,_,_,count) -> 
                count > 0 && 
                x >= 0.0 && x <= width &&
                y >= 0.0 && y <= height
            )
        dead |> List.iter (fun (p:Line,_,_,_,_,_) -> container.Remove p |> ignore)
        particles <- alive        
        setPositions particles
    member thuis.Update() = updateParticles()
    member this.Explode(x,y) = explode (x,y)        