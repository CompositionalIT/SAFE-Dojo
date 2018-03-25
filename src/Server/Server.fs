open Api
open Giraffe
open Saturn
open System.IO

let clientPath = Path.Combine("..","Client") |> Path.GetFullPath
let port = 8085us

let browserRouter = scope {
  get "/" (htmlFile (Path.Combine(clientPath, "/index.html"))) }

let mainRouter = scope {
  forward "/api" apiRouter
  forward "" browserRouter }

let app = application {
    router mainRouter
    url ("http://0.0.0.0:" + port.ToString() + "/")
    memory_cache 
    use_static clientPath
    use_gzip }

run app