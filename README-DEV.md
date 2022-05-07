1. `clone SAFE-dojo`
2. `dotnet run`
3. visit localhost:8080
4. follow instructions: https://github.com/CompositionalIT/SAFE-Dojo/blob/master/Instructions.md

# Architecture overview
**Server**
```
Saturn --> Giraffe --> ASP.Net Core
```

**Clinet**
`Fable` compiles F# to javascript
`Elmish` is a wrapper on top of `ReactJS`

**Deploy**
**Authentication**
**DB access**

# Client/Server Setup
## server setup
1. The Saturn server starts by calling `run`
2. `run` takes a `IHostBuilder` which defines: url, routes, server attributes
3. `IHostBuilder` takes routes are created with `Fable.Remoting.createApi`
4. APIs are defined in `API.dojoApi` is of type `IDojoApi`
```
let webApp =
    Remoting.createApi()
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
```

## api definition
`IDojoApi` is a type shared between Server/Client
```
type IDojoApi =
    { GetDistance: string -> LocationResponse Async
      GetCrimes: string -> CrimeResponse array Async }
```

## api implementation
api implementation is as simple as creating an instance of `IDojoApi`
```
let dojoApi = {
    GetDistance = getDistanceFromLondon
    GetCrimes = fun postcode -> async { return Array.empty }
}
```

## api client
client instantiate a api proxy client
```
let dojoApi =
    Remoting.createApi()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.buildProxy<IDojoApi>
```
then api can be invoked as
```
let crimes = dojoApi.GetCrimes "12345"
```
## Misc
 - `global.json` defines sdk version
 - `dotnet --list-sdks` list all installed sdks
 - `Build.fsproj` `Server.fsproj` `Client.fsproj` defines TargetFramework versions
 - `paket.dependencies` defines framework version
 - `donet paket install` installs all dependencies
 - Where is server address defined for Fable remoting client api proxy e.g. `Remoting.createApi()`
    - webpack-dev-server can proxy some urls to **other servers**
    - configuration of this proxying is defined by `devServer.proxy`
    - sample configuration of the proxy configuration can be
    ```
    {
      // redirect requests that start with /api/ to the server on port 8085
      '/api/**': {
          target: 'http://localhost:' + (process.env.SERVER_PROXY_PORT || "8085"),
             changeOrigin: true
         },
      // redirect websocket requests that start with /socket/ to the server on the port 8085
      '/socket/**': {
          target: 'http://localhost:' + (process.env.SERVER_PROXY_PORT || "8085"),
          ws: true
         }
     }
    ```
    - Server url is defined in `Saturn` as `IHostBuilder.url`