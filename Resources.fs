namespace Invadurz

open System
open System.Windows.Controls
open System.Windows.Media

[<AutoOpen>]
module Resources =
    open System.Windows
    let rand = Random()
    let settings = System.IO.IsolatedStorage.IsolatedStorageSettings.ApplicationSettings
    let toGradientStops stops =
        let collection = GradientStopCollection()
        stops
        |> List.map (fun (color,offset) -> GradientStop(Color=color,Offset=offset)) 
        |> List.iter collection.Add
        collection
    let skyBrush = 
        let darkBlue = Color.FromArgb(255uy,0uy,0uy,64uy)
        let stops = [Colors.Black,0.0; darkBlue,1.0] |> toGradientStops
        LinearGradientBrush(stops, 90.0)
    let whiteBrush = SolidColorBrush Colors.White
    let greenBrush = SolidColorBrush Colors.Green
    let createTextBlock () =
            TextBlock(
                Foreground=whiteBrush, 
                FontSize=15.0, 
                FontWeight=FontWeights.ExtraBold,
                Effect=Effects.BlurEffect(Radius=10.0/3.0))
    let createMessage s =
        let block = createTextBlock()
        block.Text <- s
        block.HorizontalAlignment <- HorizontalAlignment.Center
        block.VerticalAlignment <- VerticalAlignment.Center
        block


