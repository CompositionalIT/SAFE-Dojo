module Router

open Saturn
open Giraffe
open Giraffe.Core
open Giraffe.ResponseWriters
open Microsoft.AspNetCore.Http
open ApiWrappers

let browser = pipeline {
    plug acceptHtml
    plug putSecureBrowserHeaders
    plug fetchSession
    set_header "x-pipeline-type" "Browser"
}

let defaultView = scope {
    get "/" (htmlView Index.layout)
    get "/index.html" (redirectTo false "/")
    get "/default.html" (redirectTo false "/")
}

let browserRouter = scope {
    not_found_handler (htmlView NotFound.layout) //Use the default 404 webpage
    pipe_through browser //Use the default browser pipeline

    forward "" defaultView //Use the default view
}

[<CLIMutable>]
type LocationQuery =
    { Postcode : string }

// Replicate this function but for the crime endpoint and the weather endpoint
let getDistanceToLondon =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            let! location = ctx.BindModelAsync<LocationQuery>()
            let london = { Latitude = 51.5074; Longitude = 0.1278 }
            let! latLngForPostcode = PostcodeApi.getLatLngForPostcode (Postcode location.Postcode) |> Async.StartAsTask
            let distanceToLondonInMetres = DistanceCalculator.getDistanceBetweenPositions latLngForPostcode london
            let distanceToLondon = distanceToLondonInMetres / 1000.
            return! json (sprintf "Distance to London from %s is %.2fKM" location.Postcode distanceToLondon) next ctx
        }    

let api = pipeline {
    set_header "x-pipeline-type" "Api"
}

let apiRouter = scope {
    pipe_through api
    get "/distance" getDistanceToLondon
    // Add in a new endpoint here which returns crime data for a postcode
    // Add in a new endpoint here which returns weather data for a postcode
}

let router = scope {
    forward "/api" apiRouter
    forward "" browserRouter
}