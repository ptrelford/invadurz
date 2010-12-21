namespace Invadurz

module Color =
    open System.Windows.Media
    let toInt (color:Color) = 
        (int color.A <<< 24) ||| 
        (int color.R <<< 16) ||| 
        (int color.G <<< 8)  ||| 
        int color.B

[<AutoOpen>]
module Bits =
    open System.Windows.Media
    open System.Windows.Media.Imaging
    let toBitmap (width,xs:int []) =
        let white = Colors.White |> Color.toInt
        let black = 0x00000000
        let toColor = function true -> white | false -> black
        let bitmap = WriteableBitmap(width,xs.Length)
        let pixels = bitmap.Pixels
        xs |> Array.iteri (fun y xs ->
            for x = 0 to width-1 do
                let bit = 1 <<< (width - 1 - x) 
                pixels.[x+y*width] <- xs &&& bit = bit |> toColor
        )
        bitmap

[<AutoOpen>]
module Bitmap =
    open System.Windows
    open System.Windows.Controls
    open System.Windows.Media
    open System.Windows.Media.Imaging
    let toImage (bitmap:#BitmapSource) =
        let w = bitmap.GetValue(BitmapSource.PixelWidthProperty) :?> int
        let h = bitmap.GetValue(BitmapSource.PixelHeightProperty) :?> int
        Image(Source=bitmap,Stretch=Stretch.Fill,Width=float w*2.0,Height=float h*2.0) 

