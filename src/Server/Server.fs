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
    memory_cache
    use_router webApp
    use_static "public"
    use_gzip
}

run app