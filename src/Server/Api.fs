module Api

open DataAccess
open FSharp.Data.UnitSystems.SI.UnitNames
open Giraffe
open Microsoft.AspNetCore.Http
open Saturn
open Shared
open FSharp.Control.Tasks

let private london = { Latitude = 51.5074; Longitude = 0.1278 }
let invalidPostcode next (ctx:HttpContext) =
    ctx.SetStatusCode 400
    text "Invalid postcode" next ctx

let getDistanceFromLondon postcode = async {
    if not (Validation.isValidPostcode postcode) then failwith "Invalid postcode"

    let! location = getLocation postcode
    let distanceToLondon = getDistanceBetweenPositions location.LatLong london
    return { Postcode = postcode; Location = location; DistanceToLondon = (distanceToLondon / 1000.<meter>) }
}

let getCrimeReport postcode next ctx = task {
    if Validation.isValidPostcode postcode then
        let! location = getLocation postcode
        let! reports = Crime.getCrimesNearPosition location.LatLong
        let crimes =
            reports
            |> Array.countBy(fun r -> r.Category)
            |> Array.sortByDescending snd
            |> Array.map(fun (k, c) -> { Crime = k; Incidents = c })
        return! json crimes next ctx
    else return! invalidPostcode next ctx }

let private asWeatherResponse (weather:DataAccess.Weather.MetaWeatherLocation.Root) =
    { WeatherType =
        weather.ConsolidatedWeather
        |> Array.countBy(fun w -> w.WeatherStateName)
        |> Array.maxBy snd
        |> fst
        |> WeatherType.Parse
      AverageTemperature = weather.ConsolidatedWeather |> Array.averageBy(fun r -> float r.TheTemp) }

let getWeather postcode next ctx = task {
    (* Task 4.1 WEATHER: Implement a function that retrieves the weather for
       the given postcode. Use the GeoLocation.getLocation, Weather.getWeatherForPosition and
       asWeatherResponse functions to create and return a WeatherResponse instead of the stub.
       Don't forget to use let! instead of let to "await" the Task. *)
    return! json { WeatherType = WeatherType.Clear; AverageTemperature = 0. } next ctx }

let apiRouter =
    { GetDistance = getDistanceFromLondon
      GetCrimes = fun postcode -> async { return Array.empty }
    }

    (* Task 1.0 CRIME: Replace the dummy GetCrimes implementation to return crime
       data using the getCrimeReport function. Use the above GetDistance function
       as an example of how to add a new route. *)

    (* Task 4.2 WEATHER: Hook up the weather endpoint to the getWeather function. *)