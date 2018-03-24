module Client

open Elmish
open Elmish.React

open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fable.PowerPack.Fetch

open Shared

open Fulma
open Fulma.Layouts
open Fulma.Elements
open Fulma.Elements.Form
open Fulma.Extra.FontAwesome
open Fulma.Components
open Fulma.BulmaClasses

type Model = obj option

type Msg =
| Increment
| Decrement
| Init

let init () = 
  None, Cmd.ofMsg Init

let update msg (model : Model) =
  let model' =
    match model,  msg with
    | Some _, Increment -> Some (obj())
    | Some _, Decrement -> Some (obj())
    | None, Init -> Some (obj())
    | _ -> None
  model', Cmd.none

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
    [ strong [] [ str "SAFE Template" ]
      str " powered by: "
      components ]

let show = function
| Some x -> string x
| None -> "Loading..."

let button txt onClick = 
  Button.button
    [ Button.IsFullwidth
      Button.Color IsPrimary
      Button.OnClick onClick ] 
    [ str txt ]

let label txt = Label.label [] [ str txt ]

let view model dispatch =
  div []
    [ Navbar.navbar [ Navbar.Color IsPrimary ]
        [ Navbar.Item.div [ ]
            [ Heading.h2 [ ]
                [ str "Location Review!" ] ] ]

      Container.container []
        [
              Field.div [ ]
                   [ Label.label [ ] [ str "Postcode" ]
                     Control.div [ Control.HasIconLeft
                                   Control.HasIconRight ]
                                 [ Input.text [ Input.Placeholder "Ex: EC2A4NE" ]
                                   Icon.faIcon [ Icon.Size IsSmall; Icon.IsLeft ] [ Fa.icon Fa.I.Building ]
                                   Icon.faIcon [ Icon.Size IsSmall; Icon.IsRight ] [ Fa.icon Fa.I.Check ] ]
                     Help.help [ Help.Color IsSuccess ] [ str "This postcode is valid!" ] ]
              // Control area (submit, cancel, etc.)
              Field.div [ Field.IsGrouped ]
                [ Control.div [ ] [ button "Submit" ignore ] ]
        ]
    
      Footer.footer [ ]
        [ Content.content [ Content.CustomClass Bulma.Properties.Alignment.HasTextCentered ]
            [ safeComponents ] ] ]

  
#if DEBUG
open Elmish.Debug
open Elmish.HMR
#endif

Program.mkProgram init update view
#if DEBUG
|> Program.withConsoleTrace
|> Program.withHMR
#endif
|> Program.withReact "elmish-app"
#if DEBUG
|> Program.withDebugger
#endif
|> Program.run
