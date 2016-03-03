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
    member this.Actions = actions

type Mouse (relativeTo,buttonDown:IObservable<MouseButtonEventArgs>,buttonUp:IObservable<MouseButtonEventArgs>,remember) =
    let mutable actions = set []
    let mutable position = None
    let listen() = 
        buttonDown        
        |> Observable.subscribe(fun args ->
            let pos = args.GetPosition(relativeTo)
            position <- Some pos
            actions <- Set.add Fire actions)
        |> remember
        buttonUp        
        |> Observable.subscribe (fun _ -> 
            position <- None
            actions <- Set.remove Fire actions)            
        |> remember
    member this.StartListening () = listen()
    member this.Actions (x) = 
        match position with
        | Some(point) when x < point.X -> actions |> Set.add Right
        | Some(point) when x > point.X -> actions |> Set.add Left
        | _ -> actions

