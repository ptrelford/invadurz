namespace Invadurz

open System.Windows

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