module Apis

open Newtonsoft.Json
open System

type Location =
    { Latitude : float
      Longitude : float }

let private getData<'T> (url : string) = async {
    use  wc = new Net.WebClient()
    let! data = url |> wc.DownloadStringTaskAsync |> Async.AwaitTask
    return JsonConvert.DeserializeObject<'T> data }    

module GeoLocation =
    open FSharp.Data.UnitSystems.SI.UnitNames
    type Postcode = Postcode of string
    type PostcodeApiWrapper =
        { Status : string
          Result : Location }

    let getLatLngForPostcode (Postcode postcode) = async {
        let! postcode = postcode |> sprintf "http://api.postcodes.io/postcodes/%s" |> getData<PostcodeApiWrapper>
        return postcode.Result }

    let getDistanceBetweenPositions pos1 pos2 =
        let lat1, lng1 = pos1.Latitude, pos1.Longitude
        let lat2, lng2 = pos2.Latitude, pos2.Longitude
        let inline degreesToRadians degrees = System.Math.PI * float degrees / 180.0
        let R = 6371000.0
        let phi1 = degreesToRadians lat1
        let phi2 = degreesToRadians lat2
        let deltaPhi = degreesToRadians (lat2 - lat1)
        let deltaLambda = degreesToRadians (lng2 - lng1)
        let a = sin (deltaPhi / 2.0) * sin (deltaPhi / 2.0) + cos phi1 * cos phi2 * sin (deltaLambda / 2.0) * sin (deltaLambda / 2.0)
        let c = 2.0 * atan2 (sqrt a) (sqrt (1.0 - a))
        R * c * 1.<meter>

module Crime =
    type CrimeIncident =
        { category : string
          id : int
          month : string
          Location : Location }

    let getCrimesNearPosition location =
        sprintf "https://data.police.uk/api/crimes-street/all-crime?lat=%f&lng=%f" location.Latitude location.Longitude
        |> getData<CrimeIncident array>

module Weather =
    type WeatherLocationResponse =
        { Title : string
          WoeId : string
          Distance : int }

    type WeatherReading =
        { weather_state_name : string
          wind_direction_compass : string
          min_temp : float
          max_temp : float
          the_temp : float }

    type WeatherApiResponse =
        { sun_rise : System.DateTime
          sun_set : System.DateTime
          consolidated_weather : WeatherReading [] }      

    let getWeatherForPosition location = async {
        let! locations =
            sprintf "https://www.metaweather.com/api/location/search/?lattlong=%f,%f" location.Latitude location.Longitude
            |> getData<WeatherLocationResponse array>
        let bestLocationId = locations |> Array.sortBy (fun t -> t.Distance) |> Array.map (fun o -> o.WoeId) |> Array.head
        return!
            bestLocationId
            |> sprintf "https://www.metaweather.com/api/location/%s"
            |> getData<WeatherApiResponse> }
