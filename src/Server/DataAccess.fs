module DataAccess

open FSharp.Data
open Shared

[<AutoOpen>]
module GeoLocation =
    open FSharp.Data.UnitSystems.SI.UnitNames
    type PostcodesIO = JsonProvider<"http://api.postcodes.io/postcodes/EC2A4NE">

    let getLocation postcode = async {
        let! postcode = postcode |> sprintf "http://api.postcodes.io/postcodes/%s" |> PostcodesIO.AsyncLoad
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
        let inline degreesToRadians degrees = System.Math.PI * float degrees / 180.0
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
    type PoliceUkCrime = JsonProvider<"https://data.police.uk/api/crimes-street/all-crime?lat=51.5074&lng=0.1278">
    let getCrimesNearPosition location =
        (location.Latitude, location.Longitude)
        ||> sprintf "https://data.police.uk/api/crimes-street/all-crime?lat=%f&lng=%f"
        |> PoliceUkCrime.AsyncLoad

[<AutoOpen>]
module Weather =
    type OpenMeteoCurrentWeather = JsonProvider<"https://api.open-meteo.com/v1/forecast?latitude=51.5074&longitude=0.1278&current_weather=true">
    let getWeatherForPosition location = async {
        let! weatherInfo =
            (location.Latitude, location.Longitude)
            ||> sprintf "https://api.open-meteo.com/v1/forecast?latitude=%f&longitude=%f&current_weather=true"
            |> OpenMeteoCurrentWeather.AsyncLoad
        return
            weatherInfo.CurrentWeather }
