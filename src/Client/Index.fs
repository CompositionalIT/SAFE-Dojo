module Index

open Elmish

open Feliz
open Feliz.Bulma
open Feliz.Recharts
open Feliz.PigeonMaps
open Elmish.SweetAlert

open Fable.Remoting.Client
open Shared

/// The different elements of the completed report.
type Report =
    { Location: LocationResponse
      Crimes: CrimeResponse array
      Weather: WeatherResponse}

type ServerState =
    | Idle
    | Loading
    | ServerError of string

/// The overall data model driving the view.
type Model =
    { Postcode: string
      ValidationError: string option
      ServerState: ServerState
      Report: Report option }

/// The different types of messages in the system.
type Msg =
    | GetReport
    | Clear
    | PostcodeChanged of string
    | GotReport of Report
    | ErrorMsg of exn

/// The init function is called to start the message pump with an initial view.
let init () =
    { Postcode = ""
      Report = None
      ValidationError = None
      ServerState = Idle }, Cmd.none

let dojoApi =
    Remoting.createApi()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.buildProxy<IDojoApi>

let getResponse postcode = async {
    let! location = dojoApi.GetDistance postcode
    let! crimes = dojoApi.GetCrimes postcode
    let! weather = dojoApi.GetWeather postcode
    (* Task 4.4 WEATHER: Fetch the weather from the API endpoint you created.
       Then, save its value into the Report below. You'll need to add a new
       field to the Report type first, though! *)

    return
        { Location = location
          Crimes = crimes
          Weather = weather}
}

/// The update function knows how to update the model given a message.
let update msg model =
    match msg with
    | GetReport ->
        match model.ValidationError with
        | None ->
            { model with ServerState = Loading }, Cmd.OfAsync.either getResponse model.Postcode GotReport ErrorMsg
        | _  ->
            model, Cmd.none

    | GotReport response ->
        { model with
            ValidationError = None
            Report = Some response
            ServerState = Idle }, Cmd.none

    | Clear ->
        init ()       

    | PostcodeChanged p ->
        { model with
            Postcode = p            
            (* Task 2.2 Validation. Use the Validation.isValidPostcode function to implement client-side form validation.
               Note that the validation is the same shared code that runs on the server! *)
            ValidationError = if Validation.isValidPostcode p then None else Some $"'{p}' is not a valid postcode" }, Cmd.none

    | ErrorMsg e ->
        let errorAlert =
            SimpleAlert(e.Message)
                .Title("Try another postcode")
                .Type(AlertType.Error)
        { model with ServerState = ServerError e.Message }, SweetAlert.Run errorAlert

[<AutoOpen>]
module ViewParts =
    let widget (title: string) (content: ReactElement list) =
        Bulma.box [
            prop.children [
                Bulma.subtitle title
                yield! content
            ]
        ]

    let crimeWidget crimes =
        let cleanData =
            crimes |> Array.map (fun c ->
                let cleansed = c.Crime.[0..0].ToUpper() + c.Crime.[1..].Replace('-', ' ')
                { c with Crime = cleansed } )

        widget "Crime"  [
            Recharts.barChart [
                barChart.layout.vertical
                barChart.data cleanData
                barChart.width 600
                barChart.height 500
                barChart.children [
                    Recharts.cartesianGrid [ cartesianGrid.strokeDasharray(4, 4) ]
                    Recharts.xAxis [ xAxis.number ]
                    Recharts.yAxis [
                        yAxis.dataKey (fun point -> point.Crime)
                        yAxis.width 200
                        yAxis.category ]
                    Recharts.tooltip []
                    Recharts.bar [
                        bar.legendType.star
                        bar.isAnimationActive true
                        bar.animationEasing.ease
                        bar.dataKey (fun point -> point.Incidents)
                        bar.fill "#8884d8"
                    ]
                ]
            ]
        ]

    let makeMarker latLong =
        PigeonMaps.marker [
            marker.anchor latLong
            marker.render (fun marker -> [
                Html.i [
                    if marker.hovered
                    then prop.style [ style.color.red; style.cursor.pointer ]
                    prop.className [ "fa"; "fa-map-marker"; "fa-2x" ]
                ]
            ])
        ]

    let mapWidget (lr:LocationResponse) =
        widget "Map"  [
                PigeonMaps.map [
                    let latLong = (lr.Location.LatLong.Latitude,lr.Location.LatLong.Longitude)
                    map.center latLong
                    map.zoom 15
                    map.height 500
                    map.markers [makeMarker latLong]
            ]
        ]

    let weatherWidget weatherReport =
        widget "Weather"  [
            Html.div [
                Bulma.image [
                    prop.children [
                        Html.img [
                            prop.style [ style.height 100]
                            prop.src (sprintf "https://raw.githubusercontent.com/erikflowers/weather-icons/master/svg/wi-%s.svg" weatherReport.WeatherType.IconName) ]
                        ]
                    ]
                Bulma.table [
                    table.isNarrow
                    table.isFullWidth
                    prop.style [ style.marginTop 37 ]
                    prop.children [
                        Html.tbody [
                            Html.tr [
                                Html.th $"Temp %.2f{weatherReport.Temperature}"
                                 (* Task 4.7 WEATHER: Get the temperature from the given weather report
                                   and display it here instead of an empty string.
                                   Hint: Use sprintf with "%.2f" to round the temperature to 2 decimal points
                                   (look at the locationWidget for an example) *)
                                Html.td ""
                            ]
                        ]
                    ]
                ]
            ]
        ]

    let locationWidget model =
        widget "Location" [
            Bulma.table [
                table.isNarrow
                table.isFullWidth
                prop.children [
                    Html.tbody [
                        Html.tr [
                            Html.th "Region"
                            Html.td model.Location.Location.Region
                        ]
                        Html.tr [
                            Html.th "Town"
                            Html.td model.Location.Location.Town
                        ]
                        Html.tr [
                            Html.th "Distance to London"
                            Html.td (sprintf "%.2fKM" model.Location.DistanceToLondon)
                        ]
                    ]
                ]
            ]
        ]

let navbar =
    Bulma.navbar [
        color.isInfo
        prop.children [
            Bulma.navbarBrand.a [
                prop.href "https://safe-stack.github.io/docs/"
                prop.target "_"
                prop.children [
                    Bulma.navbarItem.div [
                        Bulma.title [
                            Bulma.icon [
                                icon.isLarge
                                prop.style [ style.color.white; style.transform.scaleX -1 ]
                                prop.children [
                                    Html.i [ prop.className "fas fa-unlock-alt" ]
                                ]
                            ]
                            Html.span [
                                prop.style [ style.color.white ]
                                prop.text "My SAFE Dojo"
                            ]
                        ]
                    ]
                ]
            ]
        ]
    ]

/// The view function knows how to render the UI given a model, as well as to dispatch new messages based on user actions.
let view (model: Model) dispatch =
    Html.div [
        prop.style [ style.backgroundColor "#eeeeee57"; style.minHeight (length.vh 100) ]
        prop.children [
            navbar
            Bulma.section [
                Bulma.box [
                    Bulma.level [
                        Bulma.levelLeft [
                            Bulma.levelItem [ Bulma.label "Postcode" ]
                            Bulma.levelItem [
                                Html.span [
                                    prop.children [
                                        Html.text "Example postcodes: "
                                        let postcodes = [ "EC2A 4NE"; "E1 7QA"; "NG7 4ED"; "TR11 4GE"; "EH1 2EU" ]
                                        yield! [
                                            for postcode in postcodes do
                                                Html.text ", "
                                                Html.a [
                                                    prop.text postcode
                                                    prop.onClick (fun _ -> dispatch (PostcodeChanged postcode))
                                                ]
                                        ] |> List.tail
                                    ]
                                ]
                            ]
                        ]
                    ]
                    Bulma.field.div  [
                        field.hasAddons
                        prop.children [
                            Bulma.control.div [
                                control.hasIconsLeft
                                prop.style [ style.width (length.percent 100)]
                                control.hasIconsRight
                                prop.children [
                                    Bulma.input.text [
                                        if model.ValidationError.IsSome then color.isDanger else color.isInfo
                                        prop.style [ style.textTransform.uppercase ]
                                        prop.value model.Postcode
                                        prop.onChange (PostcodeChanged >> dispatch)
                                    ]
                                    Bulma.icon [
                                        icon.isLeft
                                        prop.children [
                                            Html.i [ prop.className "fas fa-home"]
                                        ]
                                    ]
                                    match model.ValidationError with
                                    | Some _ ->
                                        Bulma.icon [
                                            icon.isRight
                                            prop.children [
                                                Html.i [ prop.className "fas fa-times"]
                                            ]
                                        ]
                                    | None ->
                                        Bulma.icon [
                                            icon.isRight
                                            prop.children [
                                                Html.i [ prop.className "fas fa-check"]
                                            ]
                                        ]
                                    Bulma.help [
                                        if model.ValidationError.IsNone then color.isPrimary else color.isDanger
                                        prop.text (model.ValidationError |> Option.defaultValue "")
                                    ]
                                ]
                            ]
                            Bulma.control.div [
                                        prop.children [
                                    Bulma.button.a [
                                        color.isInfo
                                        prop.onClick (fun _ -> dispatch GetReport)
                                        prop.disabled (model.ValidationError.IsSome)
                                        if (model.ServerState = Loading) then button.isLoading
                                        prop.text "Fetch"
                                    ]
                                    Bulma.button.a [
                                        color.isInfo
                                        prop.onClick (fun _ -> dispatch Clear)
                                        if (model.ServerState = Loading) then button.isLoading
                                        prop.text "Clear"
                                        ]
                                ]
                                    ]
                            ]
                        ]
                    ]
            ]

            match model.Report, model.ServerState with
            | None, (Idle | Loading) -> ()
            | _, ServerError _ -> ()
            | Some report, _ ->
                Bulma.section [
                    Bulma.columns [
                        Bulma.column [
                            prop.children [
                                Bulma.columns [
                                    Bulma.column [
                                        column.isThreeFifths
                                        prop.children [
                                            locationWidget report
                                        ]
                                    ]
                                    Bulma.column [
                                        (* Task 4.5 WEATHER: Generate the view code for the weather tile
                                           using the weatherWidget function, supplying the weather data
                                           from the report value, and include it here as part of the list *)
                                    ]
                                ]
                                (* Task 3.1 MAP: Call the mapWidget function here, which creates a
                                   widget to display a map using the React ReCharts component. The function
                                   takes in a LocationResponse value as input and returns a ReactElement. *)
                            ]
                        ]
                        Bulma.column [
                            column.is6
                            prop.children [
                                mapWidget report.Location
                            ]
                        ]
                        Bulma.column [
                            column.is7
                            prop.children [
                                crimeWidget report.Crimes
                                weatherWidget report.Weather
                            ]
                        ]
                    ]
                ]

        ]
    ]
