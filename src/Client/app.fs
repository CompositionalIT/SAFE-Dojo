module App

open Elmish

open Fable.Core.JsInterop
open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fable.PowerPack
open Fable.Recharts
open Fable.Recharts.Props

open Fulma
open Fulma.Layouts
open Fulma.Elements
open Fulma.Elements.Form
open Fulma.Extra.FontAwesome
open Fulma.Components
open Fulma.BulmaClasses

open Shared

/// The data model driving the view
type Report =
    { Location : LocationResponse
      Crimes : CrimeResponse array
      Weather : WeatherResponse }

type ServerState = Idle | Loading | ServerError of string

type Model =
    { Postcode : string
      ValidationError : string option
      ServerState : ServerState
      Report : Report option }

/// The different types of messages in the system
type Msg =
    | GetReport
    | PostcodeChanged of string
    | GotReport of Report
    | ErrorMsg of exn

/// The init function is called to start the message pump with an initial view.
let init () = 
    { Postcode = null
      Report = None
      ValidationError = None
      ServerState = Idle }, Cmd.ofMsg (PostcodeChanged "")

let getResponse postcode = promise {
    let! location = Fetch.fetchAs<LocationResponse> (sprintf "/api/distance/%s" postcode) []
    let! crimes = Fetch.tryFetchAs<CrimeResponse array> (sprintf "api/crime/%s" postcode) [] |> Promise.map (Result.defaultValue [||])
    let! weather = Fetch.fetchAs<WeatherResponse> (sprintf "api/weather/%s" postcode) []
    return { Location = location; Crimes = crimes; Weather = weather } }
 
/// The update function knows how to update the model given a message.
let update msg model =
    match model, msg with
    | { ValidationError = None; Postcode = postcode }, GetReport ->
        { model with ServerState = Loading }, Cmd.ofPromise getResponse postcode GotReport ErrorMsg
    | _, GetReport -> model, Cmd.none
    | _, GotReport response ->
        { model with
            ValidationError = None
            Report = Some response
            ServerState = Idle }, Cmd.none
    | _, PostcodeChanged p ->
        let p = p.ToUpper()
        { model with
            Postcode = p
            ValidationError =
              if Validation.validatePostcode p then None
              else Some "Invalid postcode." }, Cmd.none
    | _, ErrorMsg e -> { model with ServerState = ServerError e.Message }, Cmd.none

[<AutoOpen>]
module ViewParts =
    let crimeTile crimes =
        let cleanData = crimes |> Array.map (fun c -> { c with Crime = c.Crime.[0..0].ToUpper() + c.Crime.[1..].Replace('-', ' ') } )
        Tile.tile [ ] [
          Notification.notification [ Notification.Props [ Style [ Height "100%"; Width "100%" ] ] ] [
            div [] [
              Heading.h2 [] [ str "Crime" ]
              barChart
                [ Chart.Data cleanData
                  Chart.Width 600.
                  Chart.Height 500.
                  Chart.Layout Vertical ]
                [ xaxis [ Cartesian.Type "number" ] []
                  yaxis [ Cartesian.Type "category"; Cartesian.DataKey "Crime"; Cartesian.Width 200. ] []
                  bar [ Cartesian.DataKey "Incidents" ] [] ] ] ] ]

    let bingMapTile latLong =
      Tile.tile [ Tile.Size Tile.Is12 ] [
        Notification.notification [ Notification.Props [ Style [ Height "100%"; Width "100%" ] ] ] [
          Heading.p [ ] [ str "Map" ]
          iframe [
            Style [ Height 410; Width 810 ]
            Src (sprintf "https://www.bing.com/maps/embed?h=400&w=800&cp=%f~%f&lvl=11&typ=s&sty=h&src=SHELL&FORM=MBEDV8" latLong.Latitude latLong.Longitude)
          ] [ ]
        ]
      ]

    let weatherTile weatherReport =
        Tile.child [ ] [
          Notification.notification [ Notification.Props [ Style [ Height "100%"; Width "100%" ] ] ] [
            Heading.h2 [ ] [ str "Weather" ]
            Level.level [ ] [
              Level.item [ Level.Item.HasTextCentered ] [
                div [ ] [
                  Level.heading [ ] [
                    Image.image [ Image.Is128x128 ] [
                      img [ Src(sprintf "https://www.metaweather.com/static/img/weather/%s.svg" (WeatherType.Parse weatherReport.Description).Abbreviation) ]
                    ]
                  ]
                  Level.title [ ] [
                    Heading.h3 [ Heading.Is4; Heading.Props [ Style [ Width "100%" ] ] ] [ sprintf "%.1f°C" weatherReport.AverageTemperature |> str ]
                  ]
                ]
              ]
            ]
          ]
        ]
    let locationTile model =
        Tile.child [ ] [
          Notification.notification [ Notification.Props [ Style [ Height "100%"; Width "100%" ] ] ] [
            Heading.h2 [ ] [ str "Location" ]
          ]
        ]
             

/// The view function knows how to render the UI given a model, as well as to dispatch new messages based on user actions.
let view model dispatch =
    div [] [
        Navbar.navbar [ Navbar.Color IsPrimary ] [
            Navbar.Item.div [] [
                Heading.h1 [] [ str "Location Review!" ] ]
            ]
        
        Container.container [] [
            yield 
                Field.div [] [
                    Label.label [] [ str "Postcode" ]
                    Control.div [ Control.HasIconLeft; Control.HasIconRight ] [
                        Input.text
                            [ Input.Placeholder "Ex: EC2A 4NE"
                              Input.Value model.Postcode
                              Input.Props [ OnChange (fun ev -> dispatch (PostcodeChanged !!ev.target?value)); onKeyDown KeyCode.enter (fun _ -> dispatch GetReport) ] ]
                        Icon.faIcon [ Icon.Size IsSmall; Icon.IsLeft ] [ Fa.icon Fa.I.Building ]
                        (match model with
                         | { ServerState = Loading } -> span [ Class "icon is-small is-right" ] [ i [ Class "fa fa-spinner faa-spin animated" ] [] ] 
                         | { ValidationError = Some _ } -> Icon.faIcon [ Icon.Size IsSmall; Icon.IsRight ] [ Fa.icon Fa.I.Exclamation ]
                         | { ValidationError = None } -> Icon.faIcon [ Icon.Size IsSmall; Icon.IsRight ] [ Fa.icon Fa.I.Check ])
                    ]
                    Help.help
                       [ Help.Color (if model.ValidationError.IsNone then IsSuccess else IsDanger) ]
                       [ str (model.ValidationError |> Option.defaultValue "") ]
                ]
            yield
                Field.div [ Field.IsGrouped ] [
                    Control.div [] [
                        Button.button
                            [ Button.IsFullwidth; Button.Color IsPrimary; Button.OnClick (fun _ -> dispatch GetReport); Button.Disabled model.ValidationError.IsSome ]
                            [ str "Submit" ] ] 
                ]

            match model with
            | { Report = None; ServerState = (Idle | Loading) } -> ()
            | { ServerState = ServerError error } -> yield Field.div [] [ str error ]
            | { Report = Some model } ->
                yield
                    Tile.ancestor [ ] [
                      Tile.parent [ Tile.Size Tile.Is12 ] [
                        bingMapTile model.Location.Location.LatLong
                      ]
                    ]
                yield
                    Tile.ancestor [ ] [
                      Tile.parent [ Tile.IsVertical; Tile.Size Tile.Is4 ] [ 
                        locationTile model
                        weatherTile model.Weather
                      ]
                      Tile.parent [ Tile.Size Tile.Is8 ] [
                        crimeTile model.Crimes
                      ]                   
                  ]        
        ]

        br [ ]

        Footer.footer [] [
            Content.content
                [ Content.CustomClass Bulma.Properties.Alignment.HasTextCentered ]
                [ safeComponents ]
        ]
    ]