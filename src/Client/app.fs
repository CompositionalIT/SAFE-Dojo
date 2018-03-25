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
    ValidationError : string option
    Response : Location option }

/// The different types of messages in the system
type Msg =
| GetReport
| PostcodeChanged of string
| GotReport of LocationResponse
| ErrorMsg of exn

/// The update function knows how to update the model given a message
let update msg model =
  match model, msg with
  | { ValidationError = None; Postcode = postcode }, GetReport -> model, Cmd.ofPromise (Fetch.fetchAs<LocationResponse> (sprintf "/api/distance/%s" postcode)) [] GotReport ErrorMsg
  | _, GetReport -> model, Cmd.none
  | _, GotReport response -> { model with Response = Some response.Location }, Cmd.none
  | _, PostcodeChanged p ->
    let p = p.ToUpper()
    { model with
        Postcode = p
        ValidationError =
          if Shared.Validation.validatePostcode p then None
          else Some "Invalid postcode." }, Cmd.none
  | _, ErrorMsg _ -> model, Cmd.none

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
                                 [ Input.text [ Input.Placeholder "Ex: EC2A 4NE"; Input.Value model.Postcode; Input.Props [ OnChange (fun ev -> dispatch (PostcodeChanged !!ev.target?value)) ] ]
                                   Icon.faIcon [ Icon.Size IsSmall; Icon.IsLeft ] [ Fa.icon Fa.I.Building ]
                                   Icon.faIcon [ Icon.Size IsSmall; Icon.IsRight ] [ (match model.ValidationError with Some _ -> Fa.icon Fa.I.Exclamation | _ -> Fa.icon Fa.I.Check) ] ]
                     Help.help
                      [ Help.Color (if model.ValidationError.IsNone then IsSuccess else IsDanger) ]
                      [ str (model.ValidationError |> Option.defaultValue "") ] ]

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