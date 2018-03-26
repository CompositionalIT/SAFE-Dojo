[<AutoOpen>]
module ViewHelpers

open Fable.Helpers.React
open Fable.Helpers.React.Props

open Fulma
open Fulma.Elements
open Fulma.Elements.Form

let btn txt onClick = 
  Button.button
    [ Button.IsFullwidth
      Button.Color IsPrimary
      Button.OnClick onClick ] 
    [ str txt ]

let lbl txt = Label.label [] [ str txt ]

let safeComponents =
  let intersperse sep ls =
    List.foldBack (fun x -> function
      | [] -> [x]
      | xs -> x::sep::xs) ls []

  let components =
    [
      "Saturn", "https://saturnframework.github.io/docs/"
      "Fable", "http://fable.io"
      "Elmish", "https://fable-elmish.github.io/"
      "Fulma", "https://mangelmaxime.github.io/Fulma" 
    ]
    |> List.map (fun (desc,link) -> a [ Href link ] [ str desc ] )
    |> intersperse (str ", ")
    |> span [ ]

  p [ ]
    [ strong [] [ a [ Href "https://safe-stack.github.io/" ] [ str "SAFE Template" ] ]
      str " powered by: "
      components ]
