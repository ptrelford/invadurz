namespace Invadurz

open System.Windows.Shapes

type Cannon (width:float, height:float, missiles:Weapons, onFire:unit->unit) =
    let cannonWidth = 24.0
    let mutable cannonX, cannonY = 100.0, height-32.0
    let mutable cannonReload = 0
    let cannon = Sprite(laserCannon |> toBitmap |> toImage, cannonX, cannonY,[])

    let updateCannon actions =
        let invoked action = actions |> Set.contains action
        if invoked Left && cannonX > 0.0 then cannonX <- cannonX - 2.0
        if invoked Right && cannonX < width - cannonWidth then cannonX <- cannonX + 2.0
        cannon.MoveTo(cannonX,cannonY)
        if cannonReload > 0 then cannonReload <- cannonReload - 1
        if invoked Fire && cannonReload = 0 then 
            onFire()
            let missile = Rectangle(Width=2.0,Height=4.0, Fill=whiteBrush)          
            missiles.Fire(missile, cannonX+10.0,cannonY,-4.0)
            cannonReload <- 8
    member this.X = cannon.X
    member this.Control = cannon.Control
    member this.Update actions = updateCannon actions
    member this.IsHit (bombs:Weapons) =
        bombs.Items |> Seq.exists (fun bomb -> cannon.HitTest(bomb.X+1.0,bomb.Y+6.0))
    member this.IsHit (originX, originY, aliens:Sprite seq) =
        aliens |> Seq.exists (fun alien -> cannon.HitTest(originX, originY,alien))
