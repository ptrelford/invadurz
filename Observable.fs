module Observable

open System

let run (f:unit -> unit) (observable:IObservable<_>) =
    let disposable = ref null
    disposable :=
        observable |> Observable.subscribe(fun _ ->
            f ()
            disposable.Value.Dispose()
        )
