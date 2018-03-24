open Apis
open FSharp.Data.UnitSystems.SI.UnitNames
open Giraffe
open Microsoft.AspNetCore.Http
open Shared
open Saturn
open System.IO

let clientPath = Path.Combine("..","Client") |> Path.GetFullPath
let port = 8085us

module ApiRouter =
    open Apis.GeoLocation
    let private london = { Latitude = 51.5074; Longitude = 0.1278 }

    let getDistanceFromLondon next (ctx : HttpContext) = task {
        let! postcode = ctx.BindModelAsync<Postcode>()
        let! location = getLocation postcode.Postcode
        let distanceToLondon = getDistanceBetweenPositions location london
        return! json { Postcode = postcode.Postcode; Location = location; DistanceToLondon = (distanceToLondon / 1000.<meter>) } next ctx }    

    let getCrimeReport next (ctx:HttpContext) = task {
        let! postcode = ctx.BindModelAsync<Postcode>()
        let! location = getLocation postcode.Postcode
        let! reports = Crime.getCrimesNearPosition location
        let crimes =
            reports
            |> Array.countBy(fun r -> r.category)
            |> Array.sortByDescending snd
            |> Array.map(fun (k, c) -> { Crime = k; Incidents = c })
        return! json crimes next ctx }

    let getWeatherForPosition next (ctx:HttpContext) = task {
        let! postcode = ctx.BindModelAsync<Postcode>()
        let! location = getLocation postcode.Postcode
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
        get "/distance" getDistanceFromLondon
        get "/crime" getCrimeReport
        get "/weather" getWeatherForPosition
    }

let browserRouter = scope {
  get "/" (htmlFile (Path.Combine(clientPath, "/index.html"))) }

let mainRouter = scope {
  forward "" browserRouter
  forward "/api" ApiRouter.apiRouter }

let app = application {
    router mainRouter
    url ("http://0.0.0.0:" + port.ToString() + "/")
    memory_cache 
    use_static clientPath
    use_gzip }

run app