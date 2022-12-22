open Fable.Remoting.Giraffe
open Fable.Remoting.Server
open Saturn
open Shared

let webApp =
    Remoting.createApi ()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.fromValue Api.dojoApi
    |> Remoting.buildHttpHandler

let app = application {
    url "http://0.0.0.0:8085"
    use_router webApp
    use_static "public"
    use_gzip
}

run app