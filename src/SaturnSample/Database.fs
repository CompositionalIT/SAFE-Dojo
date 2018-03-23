namespace ApiWrappers

open Newtonsoft.Json
type Position =
    { Latitude : float
      Longitude : float }

type Postcode = Postcode of string

module PostcodeApi =

    type PostcodeApiPayload =
        { Latitude : float
          Longitude : float }

    type PostcodeApiWrapper =
        { Status : string
          Result : PostcodeApiPayload }

    let getLatLngForPostcode (Postcode postcode) =
        async {
            let url = sprintf "http://api.postcodes.io/postcodes/%s" postcode
            let wc = new System.Net.WebClient()
            let! data = wc.DownloadStringTaskAsync(url) |> Async.AwaitTask
            let postcode = Newtonsoft.Json.JsonConvert.DeserializeObject<PostcodeApiWrapper>(data)
            return { Position.Latitude = postcode.Result.Latitude; Longitude = postcode.Result.Longitude }
        }

module PoliceApi =
    type CrimeIncident =
        { category : string
          id : int
          month : string
          Location : Position }

    let getCrimesNearPosition { Position.Latitude = lat; Longitude = lng } =
        async {
            let url = sprintf "https://data.police.uk/api/crimes-street/all-crime?lat=%f&lng=%f" lat lng
            let wc = new System.Net.WebClient()
            let! data = wc.DownloadStringTaskAsync(url) |> Async.AwaitTask
            let crimes = Newtonsoft.Json.JsonConvert.DeserializeObject<CrimeIncident []>(data)
            return crimes
        }

module WeatherApi =
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

    let getWeatherForPosition { Position.Latitude = lat; Longitude = lng } =
        async {
            let locationUrl = sprintf "https://www.metaweather.com/api/location/search/?lattlong=%f,%f" lat lng
            let wc = new System.Net.WebClient()
            let! locationData = wc.DownloadStringTaskAsync(locationUrl) |> Async.AwaitTask
            let locations = Newtonsoft.Json.JsonConvert.DeserializeObject<WeatherLocationResponse []>(locationData)
            let bestLocationId = locations |> Array.sortBy (fun t -> t.Distance) |> Array.map (fun o -> o.WoeId) |> Array.head
            let weatherUrl = sprintf "https://www.metaweather.com/api/location/%s" bestLocationId
            let! weatherData = wc.DownloadStringTaskAsync(weatherUrl) |> Async.AwaitTask
            return Newtonsoft.Json.JsonConvert.DeserializeObject<WeatherApiResponse>(weatherData)
        }

module DistanceCalculator =
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
        R * c