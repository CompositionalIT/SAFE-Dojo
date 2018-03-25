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
open Fable.PowerPack

/// The data model driving the view
type Model =
  { Postcode : string
    Response : Location option }

/// The different types of messages in the system
type Msg =
| GetReport
| PostcodeChanged of string
| GotReport of LocationResponse
| Error of exn

/// The update function knows how to update the model given a message
let update msg (model : Model) =
  match model,  msg with
  | _, GetReport _ -> model, Cmd.ofPromise (Fetch.fetchAs<LocationResponse> "/api/distance/EC2A4NE") [] GotReport Error
  | m, PostcodeChanged p -> { m with Postcode = p.ToUpper() }, Cmd.none
  | _, Error _ -> model, Cmd.none
  | m, GotReport response -> { m with Response = Some response.Location }, Cmd.none

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
                [ Control.div [ ] [ btn "Submit" (fun _ -> dispatch GetReport) ] ]

              Field.div []
                (match model with
                 | { Response = None } -> []
                 | { Response = Some location } -> [ lbl (location.ToString()) ] )
        ]
    
      Footer.footer [ ]
        [ Content.content [ Content.CustomClass Bulma.Properties.Alignment.HasTextCentered ]
            [ safeComponents ] ] ]