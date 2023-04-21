module DataAccess

open System
open FSharp.Data
open Shared

[<AutoOpen>]
module GeoLocation =
    open FSharp.Data.UnitSystems.SI.UnitNames
    let [<Literal>] Sample = """{"status":200,"result":{"postcode":"EC2A 4NE","quality":1,"eastings":533041,"northings":182485,"country":"England","nhs_ha":"London","longitude":-0.083628,"latitude":51.525615,"european_electoral_region":"London","primary_care_trust":"City and Hackney Teaching","region":"London","lsoa":"Hackney 027G","msoa":"Hackney 027","incode":"4NE","outcode":"EC2A","parliamentary_constituency":"Hackney South and Shoreditch","admin_district":"Hackney","parish":"Hackney, unparished area","admin_county":null,"admin_ward":"Hoxton East & Shoreditch","ced":null,"ccg":"NHS North East London","nuts":"Hackney and Newham","codes":{"admin_district":"E09000012","admin_county":"E99999999","admin_ward":"E05009377","parish":"E43000202","parliamentary_constituency":"E14000721","ccg":"E38000255","ccg_id":"A3A8R","ced":"E99999999","nuts":"TLI41","lsoa":"E01033708","msoa":"E02000371","lau2":"E09000012"}}}"""

    type PostcodesIO = JsonProvider<Sample>

    let getLocation postcode = async {
        let! postcode = async {
            try
                return! postcode |> sprintf "http://api.postcodes.io/postcodes/%s" |> PostcodesIO.AsyncLoad
            with ex ->
            printfn "Failed to fetch location data: %O" ex
            return PostcodesIO.Parse Sample
        }
        let latLong =
            { Latitude = float postcode.Result.Latitude
              Longitude = float postcode.Result.Longitude }
        let location =
            { LatLong = latLong
              Town = postcode.Result.AdminDistrict
              Region = postcode.Result.Nuts }
        return location }

    let getDistanceBetweenPositions pos1 pos2 =
        let lat1, lng1 = pos1.Latitude, pos1.Longitude
        let lat2, lng2 = pos2.Latitude, pos2.Longitude
        let inline degreesToRadians degrees = Math.PI * float degrees / 180.0
        let radius = 6371000.0
        let phi1 = degreesToRadians lat1
        let phi2 = degreesToRadians lat2
        let deltaPhi = degreesToRadians (lat2 - lat1)
        let deltaLambda = degreesToRadians (lng2 - lng1)
        let a = sin (deltaPhi / 2.0) * sin (deltaPhi / 2.0) + cos phi1 * cos phi2 * sin (deltaLambda / 2.0) * sin (deltaLambda / 2.0)
        let c = 2.0 * atan2 (sqrt a) (sqrt (1.0 - a))
        radius * c * 1.<meter>

[<AutoOpen>]
module Crime =
    let [<Literal>] Sample = """[{"category":"anti-social-behaviour","location_type":"Force","location":{"latitude":"51.504888","street":{"id":955002,"name":"On or near Booth Close"},"longitude":"0.115119"},"context":"","outcome_status":null,"persistent_id":"","id":102508605,"location_subtype":"","month":"2022-06"}]"""
    type PoliceUkCrime = JsonProvider<Sample>
    let getCrimesNearPosition location = async {
        try
            return!
                (location.Latitude, location.Longitude)
                ||> sprintf "https://data.police.uk/api/crimes-street/all-crime?lat=%f&lng=%f"
                |> PoliceUkCrime.AsyncLoad
        with ex ->
            printfn "Failed to fetch crime data: %O" ex
            return PoliceUkCrime.Parse Sample
    }

[<AutoOpen>]
module Weather =
    open System.Net.Http

    let [<Literal>] Sample = """{"latitude":51.52,"longitude":-0.08000016,"generationtime_ms":0.27298927307128906,"utc_offset_seconds":0,"timezone":"GMT","timezone_abbreviation":"GMT","elevation":25.0,"current_weather":{"temperature":23.8,"windspeed":13.8,"winddirection":277.0,"weathercode":3.0,"time":"2022-08-19T14:00"}}"""

    type OpenMeteoCurrentWeather = JsonProvider<Sample>

    let getWeatherForPosition location = async {
        try
            use client = new HttpClient()

            let uri = Uri($"https://api.open-meteo.com/v1/forecast?latitude=%f{location.Latitude}&longitude=%f{location.Longitude}&current_weather=true")

            let! response = client.GetAsync(uri) |> Async.AwaitTask

            if response.IsSuccessStatusCode then
                let! json = response.Content.ReadAsStringAsync() |> Async.AwaitTask
                return (OpenMeteoCurrentWeather.Parse json).CurrentWeather
            else
                printfn "Failed to fetch weather data: %O" response.ReasonPhrase
                return (OpenMeteoCurrentWeather.Parse Sample).CurrentWeather
        with ex ->
            printfn "Failed to fetch weather data: %O" ex
            return (OpenMeteoCurrentWeather.Parse Sample).CurrentWeather
    }