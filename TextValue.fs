namespace Invadurz

open System.Windows
open System.Windows.Controls
open System.Windows.Media

type TextValue (label,value,alignment,width) =
    let mutable value = value
    let textBlock = createTextBlock()
    do  textBlock.Width <- width
    do  textBlock.TextAlignment <- alignment
    let update () =  textBlock.Text <- sprintf "%s  %d" label value
    do  update()
    member this.Control = textBlock
    member this.Value 
        with get () = value
        and set newValue = value <- newValue; update ()