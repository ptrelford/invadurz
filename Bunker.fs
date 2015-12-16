namespace Invadurz

type Bunker (x,y) =
    let bunkerX, bunkerY = x,y
    let toSpans (width,xs:int []) =
        let mins = Array.create width xs.Length
        let maxs = Array.create width -1
        xs |> Array.iteri (fun y xs ->
            for x = 0 to width-1 do
                let bit = 1 <<< (width - 1 - x) 
                if xs &&& bit = bit then
                    mins.[x] <- min y mins.[x]
                    maxs.[x] <- max y maxs.[x]
        )
        Array.zip mins maxs
    let spans = bunker |> toSpans
    let bitmap = bunker |> toBitmap
    let width, height = bunker |> fst, bunker |> snd |> Seq.length
    let image = bitmap |> toImage
    do  image.Opacity <- 0.90
    let sprite = Sprite(image,bunkerX,bunkerY,[])
    member this.Sprite = sprite
    member this.Busted (x,y,dy) =
        if sprite.HitTest(x,y) then
            let x = (x - bunkerX) / 2.0 |> int            
            let y = (y - bunkerY) / 2.0 |> int
            if x<0 || x >= spans.Length then false
            else
            let s = sign dy |> int
            let top, bot = spans.[x]
            if top <= bot then
                let edge = if s > 0 then top else bot
                for i = 0 to 3 do
                    let dy = edge + i * s
                    if dy >= 0 && dy < height then
                        #if SILVERLIGHT
                        bitmap.Pixels.[x + dy * width] <- 0
                        #else
                        let rect = System.Windows.Int32Rect(x,dy,1,1)
                        bitmap.WritePixels(rect, [|0|], bitmap.BackBufferStride, 0)
                        #endif
                spans.[x] <- 
                    if s > 0 then (top+4,bot) else (top,bot-4)
                true
                else false          
        else false