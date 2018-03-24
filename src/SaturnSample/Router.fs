module Router

open Apis
open Giraffe
open Microsoft.AspNetCore.Http
open Saturn

module ApiRouter =
    open Apis.GeoLocation
    [<CLIMutable>]
    type LocationQuery =
        { Postcode : string }

    // Replicate this function but for the crime endpoint and the weather endpoint
    let getDistanceToLondon next (ctx : HttpContext) = task {
        let! location = ctx.BindModelAsync<LocationQuery>()
        let london = { Latitude = 51.5074; Longitude = 0.1278 }
        let! latLngForPostcode = getLatLngForPostcode (Postcode location.Postcode) |> Async.StartAsTask
        let distanceToLondon = getDistanceBetweenPositions latLngForPostcode london
        return! json (sprintf "Distance to London from %s is %.2fKM" location.Postcode (distanceToLondon / 1000.)) next ctx }    

    let apiRouter = scope {
        pipe_through (pipeline { set_header "x-pipeline-type" "Api" })
        get "/distance" getDistanceToLondon
        // Add in a new endpoint here which returns crime data for a postcode
        // Add in a new endpoint here which returns weather data for a postcode
    }


module ViewRouter =
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

let router = scope {
    forward "/api" ApiRouter.apiRouter
    forward "" ViewRouter.browserRouter
}