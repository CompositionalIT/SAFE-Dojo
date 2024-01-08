namespace Shared

type LatLong = { Latitude: float; Longitude: float }

type Location = {
    Town: string
    Region: string
    LatLong: LatLong
}

type LocationResponse = {
    Postcode: string
    Location: Location
    DistanceToLondon: float
}

type CrimeResponse = { Crime: string; Incidents: int }

type WeatherType =
    | Clear
    | SandStorm
    | Fog
    | Thunderstorm
    | Drizzle
    | Rain
    | Snow
    | Sleet
    | Showers
    | Hail

    // WMO codes in open-meteo response seem to be WMO 4677
    // See eg: https://www.nodc.noaa.gov/archive/arc0021/0002199/1.1/data/0-data/HTML/WMO-CODE/WMO4677.HTM
    static member FromCode(weathercode: int) =
        match weathercode with
        | w when w = 9 -> SandStorm
        | w when w = 11 || w = 12 -> Fog
        | w when w = 17 -> Thunderstorm
        | w when w >= 0 && w <= 19 -> Clear
        | w when w = 20 -> Drizzle
        | w when w = 21 -> Rain
        | w when w = 22 -> Snow
        | w when w = 23 || w = 24 -> Sleet
        | w when w >= 25 && w <= 27 -> Showers
        | w when w = 28 -> Fog
        | w when w = 29 -> Thunderstorm
        | w when w >= 30 && w <= 35 -> SandStorm
        | w when w >= 36 && w <= 39 -> Snow
        | w when w >= 40 && w <= 49 -> Fog
        | w when w >= 50 && w <= 59 -> Drizzle
        | w when w >= 60 && w <= 69 -> Rain
        | w when w >= 70 && w <= 78 -> Snow
        | w when w = 79 -> Hail
        | w when w >= 80 && w <= 90 -> Showers
        | w when w = 91 || w = 92 -> Rain
        | w when w = 93 || w = 94 -> Snow
        | w when w >= 95 && w <= 99 -> Thunderstorm
        | _ -> Clear

    member this.IconName =
        match this with
        | Clear -> "day-sunny" // improvement: return "night-clear" at night time
        | SandStorm -> "sandstorm"
        | Fog -> "fog"
        | Thunderstorm -> "thunderstorm"
        | Drizzle -> "raindrops"
        | Rain -> "rain"
        | Snow -> "snow"
        | Sleet -> "sleet"
        | Showers -> "showers"
        | Hail -> "hail"

type WeatherResponse = {
    WeatherType: WeatherType
    Temperature: float
}

module Route =
    let builder = sprintf "/api/%s/%s"

type IDojoApi = {
    GetDistance: string -> LocationResponse Async
    GetCrimes: string -> CrimeResponse array Async
    GetWeather: string -> WeatherResponse Async
}

/// Provides validation on data. Shared across both client and server.
module Validation =
    open System.Text.RegularExpressions

    let isValidPostcode (postcode: string) =
        Regex.IsMatch(
            postcode,
            @"([Gg][Ii][Rr] 0[Aa]{2})|((([A-Za-z][0-9]{1,2})|(([A-Za-z][A-Ha-hJ-Yj-y][0-9]{1,2})|(([A-Za-z][0-9][A-Za-z])|([A-Za-z][A-Ha-hJ-Yj-y][0-9]?[A-Za-z]))))\s?[0-9][A-Za-z]{2})"
        )