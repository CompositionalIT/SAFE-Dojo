namespace Shared

type LatLong =
    { Latitude: float
      Longitude: float }

type Location =
    { Town: string
      Region: string
      LatLong: LatLong }

type LocationResponse =
    { Postcode: string
      Location: Location
      DistanceToLondon: float }

type CrimeResponse =
    { Crime: string
      Incidents: int }

type WeatherType =
    | Snow
    | Sleet
    | Hail
    | Thunder
    | HeavyRain
    | LightRain
    | Showers
    | HeavyCloud
    | LightCloud
    | Clear

    static member Parse (s: string) =
        let weatherTypes = Reflection.FSharpType.GetUnionCases typeof<WeatherType>
        let case =
            weatherTypes
            |> Array.find(fun w -> w.Name = s.Replace(" ", ""))
        FSharp.Reflection.FSharpValue.MakeUnion(case, [| |]) :?> WeatherType

    member this.Abbreviation =
        match this with
        | Snow -> "sn" | Sleet -> "sl" | Hail -> "h" | Thunder -> "t" | HeavyRain -> "hr"
        | LightRain -> "lr" | Showers -> "s" | HeavyCloud -> "hc" | LightCloud -> "lc" | Clear -> "c"

type WeatherResponse =
    { WeatherType: WeatherType
      AverageTemperature: float }

module Route =
    let builder = sprintf "/api/%s/%s"

type IDojoApi =
    { GetDistance: string -> LocationResponse Async
      GetCrimes: string -> CrimeResponse array Async
      GetWeather: string -> WeatherResponse Async }

/// Provides validation on data. Shared across both client and server.
module Validation =
    open System.Text.RegularExpressions
    let isValidPostcode postcode =
        Regex.IsMatch(postcode, @"([Gg][Ii][Rr] 0[Aa]{2})|((([A-Za-z][0-9]{1,2})|(([A-Za-z][A-Ha-hJ-Yj-y][0-9]{1,2})|(([A-Za-z][0-9][A-Za-z])|([A-Za-z][A-Ha-hJ-Yj-y][0-9]?[A-Za-z]))))\s?[0-9][A-Za-z]{2})")