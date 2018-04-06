module App

open Elmish

open Fable
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
    { Postcode = null
      Report = None
      ValidationError = None
      ServerState = Idle }, Cmd.ofMsg (PostcodeChanged "")

let getResponse postcode = promise {
    let! location = Fetch.fetchAs<LocationResponse> (sprintf "/api/distance/%s" postcode) []
    let! crimes = Fetch.tryFetchAs<CrimeResponse array> (sprintf "api/crime/%s" postcode) [] |> Promise.map (Result.defaultValue [||])
    
    (* Task 4.3 WEATHER: Fetch the weather from the API endpoint you created.
       Then, save its value into the Report below. You'll need to add a new
       field to the Report type first, though! *)
    return { Location = location; Crimes = crimes } }
 
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
            (* Task 2.2 Validation. Use the Validation.validatePostcode function to implement client-side form validation.
               Note that the validation is the same shared code that runs on the server! *)
            ValidationError = None }, Cmd.none
    | _, ErrorMsg e -> { model with ServerState = ServerError e.Message }, Cmd.none

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
        let cleanData = crimes |> Array.map (fun c -> { c with Crime = c.Crime.[0..0].ToUpper() + c.Crime.[1..].Replace('-', ' ') } )
        basicTile "Crime" [ ] [
            barChart
                [ Chart.Data cleanData
                  Chart.Width 600.
                  Chart.Height 500.
                  Chart.Layout Vertical ]
                [ xaxis [ Cartesian.Type "number" ] []
                  yaxis [ Cartesian.Type "category"; Cartesian.DataKey "Crime"; Cartesian.Width 200. ] []
                  bar [ Cartesian.DataKey "Incidents" ] [] ]
        ]

    let getBingMapUrl latLong =
        sprintf "https://www.bing.com/maps/embed?h=400&w=800&cp=%f~%f&lvl=11&typ=s&FORM=MBEDV8" latLong.Latitude latLong.Longitude

    let bingMapTile (latLong:LatLong) =
        basicTile "Map" [ Tile.Size Tile.Is12 ] [
            iframe [
                Style [ Height 410; Width 810 ]
                (* Task 3.1 MAPS: Use the getBingMapUrl function to build a valid maps URL using the supplied LatLong.
                   You can use it to add a Src attribute to this iframe. *)
            ] [ ]
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
                                (* Task 4.4 WEATHER: Get the temperature from the given weather report
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
                              Input.Color (if model.ValidationError.IsSome then Color.IsDanger else Color.IsSuccess)
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
                            [ Button.IsFullwidth; Button.Color IsPrimary; Button.OnClick (fun _ -> dispatch GetReport); Button.Disabled (model.ValidationError.IsSome || model.ServerState = ServerState.Loading) ]
                            [ str "Submit" ] ]
                ]

            match model with
            | { Report = None; ServerState = (Idle | Loading) } -> ()
            | { ServerState = ServerError error } ->
                yield
                    Field.div [] [
                        Tag.list [ Tag.List.HasAddons; Tag.List.IsCentered ] [
                            Tag.tag [ Tag.Color Color.IsDanger; Tag.Size IsMedium ] [
                                str error
                            ]
                        ]
                    ]
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
                            (* Task 4.5 WEATHER: Generate the view code for the weather tile
                               using the weatherTile function, supplying the weather report
                               from the model, and include it here as part of the list *)
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
