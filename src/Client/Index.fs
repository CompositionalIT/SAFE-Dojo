module App

open Elmish
open Fable.FontAwesome
open Fable.Core.JsInterop
open Fable.React
open Fable.React.Props
open Fable.Recharts
open Fable.Recharts.Props
open Fable.Remoting.Client
open Fulma
open Leaflet
open ReactLeaflet

open Shared

/// The different elements of the completed report.
type Report =
    { Location : LocationResponse
      Crimes : CrimeResponse array }

type ServerState = Idle | Loading | ServerError of string

/// The overall data model driving the view.
type Model =
    { Postcode : string
      ValidationError : string option
      ServerState : ServerState
      Report : Report option }

/// The different types of messages in the system.
type Msg =
    | GetReport
    | PostcodeChanged of string
    | GotReport of Report
    | ErrorMsg of exn

/// The init function is called to start the message pump with an initial view.
let init () =
    { Postcode = ""
      Report = None
      ValidationError = None
      ServerState = Idle }, Cmd.ofMsg (PostcodeChanged "")

let dojoApi =
    Remoting.createApi()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.buildProxy<IDojoApi>

let getResponse postcode = async {
    let! location = dojoApi.GetDistance postcode
    let! crimes = dojoApi.GetCrimes postcode
    (* Task 4.4 WEATHER: Fetch the weather from the API endpoint you created.
       Then, save its value into the Report below. You'll need to add a new
       field to the Report type first, though! *)

    return
        { Location = location
          Crimes = crimes }
}

/// The update function knows how to update the model given a message.
let update msg model =
    match model, msg with
    | { ValidationError = None; Postcode = postcode }, GetReport ->
        { model with ServerState = Loading }, Cmd.OfAsync.either getResponse postcode GotReport ErrorMsg
    | _, GetReport ->
        model, Cmd.none
    | _, GotReport response ->
        { model with
            ValidationError = None
            Report = Some response
            ServerState = Idle }, Cmd.none
    | _, PostcodeChanged p ->
        { model with
            Postcode = p
            (* Task 2.2 Validation. Use the Validation.isValidPostcode function to implement client-side form validation.
               Note that the validation is the same shared code that runs on the server! *)
            ValidationError = None }, Cmd.none
    | _, ErrorMsg e ->
        { model with ServerState = ServerError e.Message }, Cmd.none

[<AutoOpen>]
module ViewParts =
    let basicTile title options content =
        Tile.tile options [
            Notification.notification [ Notification.Props [ Style [ Height "100%"; Width "100%" ] ] ]
                (Heading.h2 [] [ str title ] :: content)
        ]
    let childTile title content =
        Tile.child [ ] [
            Notification.notification [ Notification.Props [ Style [ Height "100%"; Width "100%" ] ] ]
                (Heading.h2 [ ] [ str title ] :: content)
        ]

    let crimeTile crimes =
        let cleanData =
            crimes |> Array.map (fun c -> { c with Crime = c.Crime.[0..0].ToUpper() + c.Crime.[1..].Replace('-', ' ') } )
        basicTile "Crime" [ ] [
            barChart [
                Chart.Data cleanData
                Chart.Width 600.
                Chart.Height 500.
                Chart.Layout Vertical ] [

                xaxis [ Cartesian.Type "number" ] []
                yaxis [ Cartesian.Type "category"; Cartesian.DataKey "Crime"; Cartesian.Width 200. ] []
                bar [ Cartesian.DataKey "Incidents"; Cartesian.Custom("fill", "#8884d8") ] []
                cartesianGrid [ Cartesian.Custom("strokeDasharray", "3 3") ] [ ]
                legend [] []
                tooltip [] []
            ]
        ]

    let makeMarker latLong description =
        marker [ MarkerProps.Position latLong ] [
            tooltip [ ] [ str description ]
        ]

    let mapTile (lr:LocationResponse) =
        let latLong = LatLngExpression.Case3(lr.Location.LatLong.Latitude, lr.Location.LatLong.Longitude)
        basicTile "Map" [ Tile.Size Tile.Is12 ] [
            map [
                (* Task 3.2 MAP: Set the center of the map using MapProps.Center, supply the lat/long value as input.
                   Task 3.3 MAP: Update the Zoom to 15. *)
                MapProps.Zoom 11.
                MapProps.Style [ Height 500 ]
            ] [
                tileLayer [ TileLayerProps.Url "https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png" ] []
                (* Task 3.4 MAP: Create a marker for the map. Use the makeMarker function above. *)
            ]
        ]

    let weatherTile weatherReport =
        childTile "Weather" [
            Level.level [ ] [
                Level.item [ Level.Item.HasTextCentered ] [
                    div [ ] [
                        Level.heading [ ] [
                            Image.image [ Image.Is128x128 ] [
                                img [ Src(sprintf "https://www.metaweather.com/static/img/weather/%s.svg" weatherReport.WeatherType.Abbreviation) ]
                            ]
                        ]
                        Level.title [ ] [
                            Heading.h3 [ Heading.Is4; Heading.Props [ Style [ Width "100%" ] ] ] [
                                (* Task 4.7 WEATHER: Get the temperature from the given weather report
                                   and display it here instead of an empty string. *)
                                str ""
                            ]
                        ]
                    ]
                ]
            ]
        ]
    let locationTile model =
        childTile "Location" [
            div [ ] [
                Heading.h3 [ ] [ str model.Location.Location.Town ]
                Heading.h4 [ ] [ str model.Location.Location.Region ]
                Heading.h4 [ ] [ sprintf "%.1fKM to London" model.Location.DistanceToLondon |> str ]
            ]
        ]


/// The view function knows how to render the UI given a model, as well as to dispatch new messages based on user actions.
let view (model:Model) dispatch =
    section [] [
        Hero.hero [ Hero.Color Color.IsInfo ] [
            Hero.body [ ] [
                Container.container [
                    Container.IsFluid
                    Container.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ]
                ] [
                    Heading.h1 [ ] [
                        str "UK Location Data Mashup"
                    ]
                ]
            ]
        ]

        Container.container [] [
            Field.div [] [
                Label.label [] [ str "Postcode" ]
                Control.div [ Control.HasIconLeft; Control.HasIconRight ] [
                    Input.text
                        [ Input.Placeholder "Ex: EC2A 4NE"
                          Input.Value model.Postcode
                          Input.Modifiers [ Modifier.TextTransform TextTransform.UpperCase ]
                          Input.Color (if model.ValidationError.IsSome then Color.IsDanger else Color.IsSuccess)
                          Input.Props [ OnChange (fun ev -> dispatch (PostcodeChanged !!ev.target?value)); onKeyDown KeyCode.enter (fun _ -> dispatch GetReport) ] ]
                    Fulma.Icon.icon [ Icon.Size IsSmall; Icon.IsLeft ] [ Fa.i [ Fa.Solid.Home ] [] ]
                    (match model with
                     | { ValidationError = Some _ } ->
                        Icon.icon [ Icon.Size IsSmall; Icon.IsRight ] [ Fa.i [ Fa.Solid.Exclamation ] [] ]
                     | { ValidationError = None } ->
                        Icon.icon [ Icon.Size IsSmall; Icon.IsRight ] [ Fa.i [ Fa.Solid.Check ] [] ])
                ]
                Help.help
                   [ Help.Color (if model.ValidationError.IsNone then IsSuccess else IsDanger) ]
                   [ str (model.ValidationError |> Option.defaultValue "") ]
            ]
            Field.div [ Field.IsGrouped ] [
                Level.level [ ] [
                    Level.left [] [
                        Level.item [] [
                            Button.button
                                [ Button.IsFullWidth
                                  Button.Color IsPrimary
                                  Button.OnClick (fun _ -> dispatch GetReport)
                                  Button.Disabled (model.ValidationError.IsSome)
                                  Button.IsLoading (model.ServerState = ServerState.Loading) ]
                                [ str "Submit" ]
                        ]
                    ]
                ]
            ]

            match model with
            | { Report = None; ServerState = (Idle | Loading) } -> ()
            | { ServerState = ServerError error } ->
                Field.div [] [
                    Tag.list [ Tag.List.HasAddons; Tag.List.IsCentered ] [
                        Tag.tag [ Tag.Color Color.IsDanger; Tag.Size IsMedium ] [
                            str error
                        ]
                    ]
                ]
            | { Report = Some report } ->
                Tile.ancestor [
                    Tile.Size Tile.Is12
                ] [
                    Tile.parent [ Tile.Size Tile.Is12 ] [
                        (* Task 3.1 MAP: Call the mapTile function here, which creates a
	                       tile to display a map using the React Leaflet component. The function
	                       takes in a LocationResponse value as input and returns a ReactElement. *)
                    ]
                ]
                Tile.ancestor [ ] [
                    Tile.parent [ Tile.IsVertical; Tile.Size Tile.Is4 ] [
                        locationTile report
                            (* Task 4.5 WEATHER: Generate the view code for the weather tile
                               using the weatherTile function, supplying the weather data
                               from the report value, and include it here as part of the list *)
                    ]
                    Tile.parent [ Tile.Size Tile.Is8 ] [
                        crimeTile report.Crimes
                    ]
                ]
        ]

        br [ ]

        Footer.footer [] [
            Content.content
                [ Content.Modifiers [ Fulma.Modifier.TextAlignment(Screen.All, TextAlignment.Centered) ] ]
                [ safeComponents ]
        ]
    ]
