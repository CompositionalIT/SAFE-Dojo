open Api
open Giraffe
open Giraffe.Serialization
open Microsoft.Extensions.DependencyInjection
open Saturn
open System.IO

let clientPath = Path.Combine("..","Client") |> Path.GetFullPath
let port = 8085us

let browserRouter = scope {
  get "/" (htmlFile (Path.Combine(clientPath, "/index.html"))) }

let mainRouter = scope {
  forward "/api" apiRouter
  forward "" browserRouter }

let config (services:IServiceCollection) =
  let fableJsonSettings = Newtonsoft.Json.JsonSerializerSettings()
  fableJsonSettings.Converters.Add(Fable.JsonConverter())
  services.AddSingleton<IJsonSerializer>(NewtonsoftJsonSerializer fableJsonSettings) |> ignore
  services

let app = application {
    router mainRouter
    url ("http://0.0.0.0:" + port.ToString() + "/")
    memory_cache 
    use_static clientPath
    service_config config
    use_gzip }

run app