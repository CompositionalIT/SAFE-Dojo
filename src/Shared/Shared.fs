namespace Shared

type Location =
    { Latitude : float
      Longitude : float }

type LocationResponse = { Postcode : string; Location : Location; DistanceToLondon : float }
type CrimeResponse = { Crime : string; Incidents : int }
type WeatherResponse = { Description : string; AverageTemperature : float }
type WeatherType =
    | Snow
    | Sleet
    | Hail
    | Thunderstorm
    | HeavyRain
    | LightRain
    | Showers
    | HeavyCloud
    | LightCloud
    | Clear
    static member Parse =
        let weatherTypes = FSharp.Reflection.FSharpType.GetUnionCases typeof<WeatherType>
        fun s -> weatherTypes |> Array.find(fun w -> w.Name = s) |> fun u -> FSharp.Reflection.FSharpValue.MakeUnion(u, [||]) :?> WeatherType
    member this.Abbreviation =
        match this with
        | Snow -> "sn" | Sleet -> "s" | Hail -> "h" | Thunderstorm -> "t" | HeavyRain -> "hr"
        | LightRain -> "lr" | Showers -> "s" | HeavyCloud -> "hc" | LightCloud -> "lc" | Clear -> "c"

module Validation =
    open System.Text.RegularExpressions
    let validatePostcode postcode =
        Regex.IsMatch(postcode, @"(GIR 0AA)|((([A-Z-[QVX]][0-9][0-9]?)|(([A-Z-[QVX]][A-Z-[IJZ]][0-9][0-9]?)|(([A-Z-[QVX]][0-9][A-HJKSTUW])|([A-Z-[QVX]][A-Z-[IJZ]][0-9][ABEHMNPRVWXY])))) [0-9][A-Z-[CIKMOV]]{2})")