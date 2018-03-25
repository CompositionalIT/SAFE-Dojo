module App

open Elmish

open Fable.Core.JsInterop
open Fable.Helpers.React
open Fable.Helpers.React.Props

open Fulma
open Fulma.Layouts
open Fulma.Elements
open Fulma.Elements.Form
open Fulma.Extra.FontAwesome
open Fulma.Components
open Fulma.BulmaClasses

open Shared

/// The data model driving the view
type Model =
  { Postcode : string
    Response : Location option }

/// The different types of messages in the system
type Msg =
| GetReport
| PostcodeChanged of string

/// The update function knows how to update the model given a message
let update msg (model : Model) =
  let model =
    match model,  msg with
    | _, GetReport _ -> model
    | m, PostcodeChanged p -> { m with Postcode = p.ToUpper() }
  model, Cmd.none

[<AutoOpen>]
module ViewHelpers =
    let button txt onClick = 
      Button.button
        [ Button.IsFullwidth
          Button.Color IsPrimary
          Button.OnClick onClick ] 
        [ str txt ]

    let label txt = Label.label [] [ str txt ]

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
        [ strong [] [ a [ Href "https://http://safe-stack.github.io/" ] [ str "SAFE Template" ] ]
          str " powered by: "
          components ]

/// The view function knows how to render the UI given a model, as well as to dispatch new messages based on user actions.
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
                                 [ Input.text [ Input.Placeholder "Ex: EC2A4NE"; Input.Value model.Postcode; Input.Props [ OnChange (fun ev -> dispatch (PostcodeChanged !!ev.target?value)) ] ]
                                   Icon.faIcon [ Icon.Size IsSmall; Icon.IsLeft ] [ Fa.icon Fa.I.Building ]
                                   Icon.faIcon [ Icon.Size IsSmall; Icon.IsRight ] [ Fa.icon Fa.I.Check ] ]
                     Help.help [ Help.Color IsSuccess ] [ str "This postcode is valid!" ] ]

              Field.div [ Field.IsGrouped ]
                [ Control.div [ ] [ button "Submit" (fun _ -> dispatch GetReport) ] ]
        ]
    
      Footer.footer [ ]
        [ Content.content [ Content.CustomClass Bulma.Properties.Alignment.HasTextCentered ]
            [ safeComponents ] ] ]