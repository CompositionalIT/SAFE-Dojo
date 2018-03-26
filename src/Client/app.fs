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
type Report =
  { Location : LocationResponse
    Crimes : CrimeResponse array
    Weather : WeatherResponse }

type Model =
  { Postcode : string
    ValidationError : string option
    ServerError : string option
    Report : Report option }

/// The different types of messages in the system
type Msg =
| GetReport
| PostcodeChanged of string
| GotReport of Report
| ErrorMsg of exn

let getResponse postcode = promise {
  let! location = Fetch.fetchAs<LocationResponse> (sprintf "/api/distance/%s" postcode) []
  let! crimes = Fetch.fetchAs<CrimeResponse array> (sprintf "api/crime/%s" postcode) []
  let! weather = Fetch.fetchAs<WeatherResponse> (sprintf "api/weather/%s" postcode) []
  return { Location = location; Crimes = crimes; Weather = weather } }

/// The update function knows how to update the model given a message
let update msg model =
  match model, msg with
  | { ValidationError = None; Postcode = postcode }, GetReport -> model, Cmd.ofPromise getResponse postcode GotReport ErrorMsg
  | _, GetReport -> model, Cmd.none
  | _, GotReport response ->
    { model with
        ValidationError = None
        Report = Some response
        ServerError = None }, Cmd.none
  | _, PostcodeChanged p ->
    let p = p.ToUpper()
    { model with
        Postcode = p
        ValidationError =
          if Shared.Validation.validatePostcode p then None
          else Some "Invalid postcode."
        ServerError = None        
        Report = None }, Cmd.none
  | _, ErrorMsg e -> { model with ServerError = Some e.Message }, Cmd.none

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
                [ Control.div [ ]
                    [ Button.button
                        [ Button.IsFullwidth; Button.Color IsPrimary; Button.OnClick (fun _ -> dispatch GetReport); Button.Disabled model.ValidationError.IsSome ]
                        [ str "Submit" ] ] ]

              Field.div []
                (match model with
                 | { Report = None; ServerError = None; } -> []
                 | { ServerError = Some error } -> [ lbl error ]
                 | { Report = Some model } ->
                  [
                    div [] [
                      iframe [
                        Style [ Width 500; Height 400 ]
                        Src (sprintf "https://www.bing.com/maps/embed?h=400&w=500&cp=%f~%f&lvl=11&typ=s&sty=r&src=SHELL&FORM=MBEDV8" model.Location.Location.Latitude model.Location.Location.Longitude) ] [ ]
                    ]
                  ])
        ]
    
      Footer.footer [ ]
        [ Content.content [ Content.CustomClass Bulma.Properties.Alignment.HasTextCentered ]
            [ safeComponents ] ] ]