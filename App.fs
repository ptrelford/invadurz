namespace Invadurz

open System.Windows

#if SILVERLIGHT
type App() as app =
    inherit Application()
    let game = new GameControl()
    #if DEBUG
    do  let settings = app.Host.Settings    
        settings.EnableFrameRateCounter <- true
        settings.EnableRedrawRegions <- true
        settings.MaxFrameRate <- 60;
    #endif
    do  app.Startup.Add(fun _ -> app.RootVisual <- game)
    do  app.Exit.Add(fun _ -> (game :> System.IDisposable).Dispose())
#else
module App =
    [<System.STAThread>]
    do  let win = 
            new Window(Title="Invadurz", 
                       SizeToContent=SizeToContent.WidthAndHeight, 
                       Content=new GameControl())
        (new Application()).Run(win) |> ignore
#endif