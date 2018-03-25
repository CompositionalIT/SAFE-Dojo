module Api

open DataAccess
open FSharp.Data.UnitSystems.SI.UnitNames
open Giraffe
open Saturn
open Shared
open Microsoft.AspNetCore.Http

let private london = { Latitude = 51.5074; Longitude = 0.1278 }

let getDistanceFromLondon postcode next (ctx:HttpContext) = task {
    if Validation.validatePostcode postcode then
        let! location = getLocation postcode
        let distanceToLondon = getDistanceBetweenPositions location london
        return! json { Postcode = postcode; Location = location; DistanceToLondon = (distanceToLondon / 1000.<meter>) } next ctx
    else
        ctx.SetStatusCode 400
        return! text "Invalid postcode" next ctx }

let getCrimeReport postcode next ctx = task {
    let! location = getLocation postcode
    let! reports = Crime.getCrimesNearPosition location
    let crimes =
        reports
        |> Array.countBy(fun r -> r.category)
        |> Array.sortByDescending snd
        |> Array.map(fun (k, c) -> { Crime = k; Incidents = c })
    return! json crimes next ctx }

let getWeatherForPosition postcode next ctx= task {
    let! location = getLocation postcode
    let! weather = Weather.getWeatherForPosition location
    let response =
        { WeatherResponse.Description =
            weather.consolidated_weather
            |> Array.maxBy(fun w -> w.weather_state_name)
            |> fun w -> w.weather_state_name
          AverageTemperature = weather.consolidated_weather |> Array.averageBy(fun r -> r.the_temp) }
    return! json response next ctx }

let apiRouter = scope {
    pipe_through (pipeline { set_header "x-pipeline-type" "Api" })
    getf "/distance/%s" getDistanceFromLondon
    getf "/crime/%s" getCrimeReport
    getf "/weather/%s" getWeatherForPosition
}
