namespace Invadurz

open System.Windows

type App() as app =
    inherit Application()
    let game = new GameControl()
    do  app.Startup.Add(fun _ -> app.RootVisual <- game)
    do  app.Exit.Add(fun _ -> (game :> System.IDisposable).Dispose())