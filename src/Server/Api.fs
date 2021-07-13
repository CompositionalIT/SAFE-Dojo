module Api

open DataAccess
open FSharp.Data.UnitSystems.SI.UnitNames
open Shared

let private london = { Latitude = 51.5074; Longitude = 0.1278 }

let getDistanceFromLondon postcode = async {
    if not (Validation.isValidPostcode postcode) then failwith "Invalid postcode"

    let! location = getLocation postcode
    let distanceToLondon = getDistanceBetweenPositions location.LatLong london
    return { Postcode = postcode; Location = location; DistanceToLondon = (distanceToLondon / 1000.<meter>) }
}

let getCrimeReport postcode = async {
    if not (Validation.isValidPostcode postcode) then failwith "Invalid postcode"

    let! location = getLocation postcode
    let! reports = getCrimesNearPosition location.LatLong
    let crimes =
        reports
        |> Array.countBy(fun r -> r.Category)
        |> Array.sortByDescending snd
        |> Array.map(fun (k, c) -> { Crime = k; Incidents = c })
    return crimes
}

let private asWeatherResponse (weather: Weather.MetaWeatherLocation.Root) =
    { WeatherType =
        weather.ConsolidatedWeather
        |> Array.countBy(fun w -> w.WeatherStateName)
        |> Array.maxBy snd
        |> fst
        |> WeatherType.Parse
      AverageTemperature = weather.ConsolidatedWeather |> Array.averageBy(fun r -> float r.TheTemp) }

let getWeather postcode = async {
       let! location = GeoLocation.getLocation postcode
       let! weather = Weather.getWeatherForPosition location.LatLong
       return asWeatherResponse weather
}

let dojoApi =
    { GetDistance = getDistanceFromLondon
      GetCrimes = getCrimeReport
      GetWeather = getWeather
    }