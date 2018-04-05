module Api

open DataAccess
open FSharp.Data.UnitSystems.SI.UnitNames
open Giraffe
open Microsoft.AspNetCore.Http
open Saturn
open Shared

let private london = { Latitude = 51.5074; Longitude = 0.1278 }
let invalidPostcode next (ctx:HttpContext) =
    ctx.SetStatusCode 400
    text "Invalid postcode" next ctx

let getDistanceFromLondon postcode next (ctx:HttpContext) = task {
    if Validation.validatePostcode postcode then
        let! location = getLocation postcode
        let distanceToLondon = getDistanceBetweenPositions location.LatLong london
        return! json { Postcode = postcode; Location = location; DistanceToLondon = (distanceToLondon / 1000.<meter>) } next ctx
    else return! invalidPostcode next ctx }

let getCrimeReport postcode next ctx = task {
    if Validation.validatePostcode postcode then
        let! location = getLocation postcode
        let! reports = Crime.getCrimesNearPosition location.LatLong
        let crimes =
            reports
            |> Array.countBy(fun r -> r.category)
            |> Array.sortByDescending snd
            |> Array.map(fun (k, c) -> { Crime = k; Incidents = c })
        return! json crimes next ctx
    else return! invalidPostcode next ctx }

let private asWeatherResponse weather =
    { WeatherResponse.Description =
        weather.consolidated_weather
        |> Array.maxBy(fun w -> w.weather_state_name)
        |> fun w -> w.weather_state_name
      AverageTemperature = weather.consolidated_weather |> Array.averageBy(fun r -> r.the_temp) }


let getWeather postcode next ctx = task {  
    (* Task 4.1 WEATHER: Implement a function that retrieves the weather for
       the given postcode. Use the getLocation, getWeatherForPosition and
       asWeatherResponse functions to get a proper response, and then plug it
       in below instead of the stub. *)
    return! json { Description = ""; AverageTemperature = 0. } next ctx }

let apiRouter = scope {
    pipe_through (pipeline { set_header "x-pipeline-type" "Api" })
    getf "/distance/%s" getDistanceFromLondon
    
    (* Task 1.0 CRIME: Add a new /crime/{postcode} endpoint to return crime data
       using the getCrimeReport web part function. Use the above distance
       route as an example of how to add a new route. *)    
        
    (* Task 4.2 WEATHER: Hook up the weather endpoint to the getWeatherForPosition function. *)
    
    }
