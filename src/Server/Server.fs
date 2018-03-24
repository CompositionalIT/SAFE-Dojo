open System.IO
open Apis
open FSharp.Data.UnitSystems.SI.UnitNames
open Giraffe
open Microsoft.AspNetCore.Http
open Saturn
open Shared

let clientPath = Path.Combine("..","Client") |> Path.GetFullPath
let port = 8085us

module ApiRouter =
    open Apis.GeoLocation
    [<CLIMutable>]
    type LocationRequest = { Postcode : string }
    type LocationResponse = { Postcode : string; DistanceToLondon : float }
    let private london = { Latitude = 51.5074; Longitude = 0.1278 }

    // Replicate this function but for the crime endpoint and the weather endpoint
    let getDistanceToLondon next (ctx : HttpContext) = task {
        let! location = ctx.BindModelAsync<LocationRequest>()
        let! latLngForPostcode = getLatLngForPostcode (Postcode location.Postcode) |> Async.StartAsTask
        let distanceToLondon = getDistanceBetweenPositions latLngForPostcode london
        return! json { Postcode = location.Postcode; DistanceToLondon = (distanceToLondon / 1000.<meter>) } next ctx }    

    let apiRouter = scope {
        pipe_through (pipeline { set_header "x-pipeline-type" "Api" })
        get "/distance" getDistanceToLondon
        // Add in a new endpoint here which returns crime data for a postcode
        // Add in a new endpoint here which returns weather data for a postcode
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