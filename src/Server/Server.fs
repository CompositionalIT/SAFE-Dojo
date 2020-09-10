open Api
open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open Saturn
open Shared
open FSharp.Control.Tasks.V2

let webApp next ctx = task {
    let handler =
        Remoting.createApi()
        |> Remoting.withRouteBuilder Route.builder
        |> Remoting.fromValue dojoApi
        |> Remoting.buildHttpHandler
    return! handler next ctx }

let app = application {
    url "http://0.0.0.0:8085"
    use_router webApp
    memory_cache
    use_static "public"
    use_json_serializer(Thoth.Json.Giraffe.ThothSerializer())
    use_gzip
}

run app