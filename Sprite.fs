namespace Invadurz

open System.Windows
open System.Windows.Controls

[<AutoOpen>]
module Canvas =
    let setPosition element (x,y) = 
        Canvas.SetLeft(element,x)
        Canvas.SetTop(element,y)

[<AutoOpen>]
module FrameworkElement =
    let getWidth (element:UIElement) = 
        element.GetValue(FrameworkElement.WidthProperty) :?> float
    let getHeight (element:UIElement) = 
        element.GetValue(FrameworkElement.HeightProperty) :?> float

[<AutoOpen>]
module Seq =
    let yieldFor n = seq { for x = 1 to n do yield () }

type Sprite(element:UIElement,x,y,animation:seq<unit>) =
    let e = animation.GetEnumerator()
    do  Canvas.setPosition element (x,y)
    let isCollision (x,y,w,h) (x',y',w',h') =
        let insideX x' = x' >= x && x' < x + w
        let insideY y' = y' >= y && y' < y + h
        (insideX x' || insideX (x'+w')) && (insideY y' || insideY (y'+h'))
    new (elements:UIElement seq,x,y,animation) = 
        let canvas = Canvas() 
        let width ,height = 
            elements |> Seq.fold (fun (w,h) element ->
                let w', h' = getWidth element, getHeight element 
                max w w', max h h'
            ) (0.0,0.0)
        canvas.Width <- width
        canvas.Height <- height
        elements |> Seq.iter canvas.Children.Add
        Sprite(canvas,x,y,animation)
    static member toControl (sprite:Sprite) = sprite.Control
    member this.Control = element
    member this.Animate() = e.MoveNext() |> ignore
    member this.MoveTo (x,y) = Canvas.setPosition element (x,y)
    member this.X = Canvas.GetLeft(element)
    member this.Y = Canvas.GetTop(element)
    member this.HitTest(x,y) =
        let x',y' = this.X, this.Y
        let w, h = getWidth element, getHeight element
        x >= x' && x < x' + w &&
        y >= y' && y < y' + h
    member this.HitTest (originX,originY,sprite:Sprite) =
        let x, y  = this.X, this.Y
        let w, h = getWidth element, getHeight element
        let x', y' = sprite.X + originX, sprite.Y + originY
        let w', h' = getWidth sprite.Control, getHeight sprite.Control
        isCollision (x,y,w,h) (x',y',w',h')
