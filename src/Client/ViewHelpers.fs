[<AutoOpen>]
module ViewHelpers

open Fable.React
open Fable.React.Props
open Fulma

module Key =
    let enter = "Enter"

let onKeyDown key action =
    OnKeyDown (fun (ev:Browser.Types.KeyboardEvent) ->
        if ev.key = key then
            ev.preventDefault()
            action ev)

let intersperse sep ls =
    List.foldBack (fun x -> function
        | [] -> [x]
        | xs -> x::sep::xs) ls []

let safeComponents =
    let components =
        [ "Saturn", "https://saturnframework.org/"
          "Fable", "http://fable.io"
          "Elmish", "https://elmish.github.io/"
          "Fulma", "https://fulma.github.io/Fulma/" ]
        |> List.map (fun (desc,link) -> a [ Href link ] [ str desc ] )
        |> intersperse (str ", ")
        |> span []

    p [] [
        strong [] [ a [ Href "https://safe-stack.github.io/" ] [ str "SAFE Template" ] ]
        str " powered by: "
        components
    ]

module Result =
    let defaultValue v r = match r with Ok x -> x | Error _ -> v