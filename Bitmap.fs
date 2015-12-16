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
    open System.Windows
    open System.Windows.Media
    open System.Windows.Media.Imaging
    let toBitmap (width,xs:int []) =
        let white = Colors.White |> Color.toInt
        let black = 0x00000000
        let toColor = function true -> white | false -> black
        #if SILVERLIGHT
        let bitmap = WriteableBitmap(width,xs.Length)
        let pixels = bitmap.Pixels
        xs |> Array.iteri (fun y xs ->
            for x = 0 to width-1 do
                let bit = 1 <<< (width - 1 - x) 
                pixels.[x+y*width] <- xs &&& bit = bit |> toColor
        )
        #else
        let bitmap = WriteableBitmap(width, xs.Length, 300.0, 300.0, PixelFormats.Bgra32, null)
        xs |> Array.iteri (fun y xs ->
            let line = 
                Array.init width (fun x ->
                    let bit = 1 <<< (width - 1 - x) 
                    xs &&& bit = bit |> toColor
                )
            bitmap.WritePixels(Int32Rect(0,0,width,1), line, width*4, 0 , y)
        )  
        #endif
        bitmap

[<AutoOpen>]
module Bitmap =
    open System.Windows
    open System.Windows.Controls
    open System.Windows.Media
    open System.Windows.Media.Imaging
    let toImage (bitmap:#BitmapSource) =
        #if SILVERLIGHT
        let w = bitmap.GetValue(BitmapSource.PixelWidthProperty) :?> int
        let h = bitmap.GetValue(BitmapSource.PixelHeightProperty) :?> int
        #else
        let w = bitmap.PixelWidth
        let h = bitmap.PixelHeight
        #endif
        Image(Source=bitmap,Stretch=Stretch.Fill,Width=float w*2.0,Height=float h*2.0) 

