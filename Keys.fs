namespace Invadurz

open System
open System.Windows.Input

type Action = Left | Right | Fire

type Keys (keyDown:IObservable<KeyEventArgs>,keyUp:IObservable<KeyEventArgs>,remember) =
    let mutable actions = set []
    let toAction = function
        | Key.Z | Key.Left -> Some Left
        | Key.X | Key.Right -> Some Right
        | Key.Space -> Some Fire
        | _ -> None
    let listen() =  
        keyDown
        |> Observable.choose (fun ke -> toAction ke.Key)
        |> Observable.subscribe(fun action -> 
            actions <- Set.add action actions)
        |> remember
        keyUp
        |> Observable.choose (fun ke -> toAction ke.Key)
        |> Observable.subscribe (fun action -> 
            actions <- Set.remove action actions)
        |> remember
    member this.StartListening () = listen()
    member this.Down action =
        actions.Contains action

