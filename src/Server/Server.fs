open Fable.Remoting.Giraffe
open Fable.Remoting.Server
open Saturn

let webApp =
    Remoting.createApi()
    |> Remoting.fromValue Api.apiRouter
    |> Remoting.buildHttpHandler

let app = application {
    url "http://0.0.0.0:8085"
    use_router webApp
    use_static "public"
    use_gzip
}

run app