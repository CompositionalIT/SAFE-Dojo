#r "paket: groupref Build //"
#load "./.fake/build.fsx/intellisense.fsx"
#if !FAKE
  #r "netstandard"
#endif

open Fake.Core
open Fake.DotNet
open Fake.IO

let serverPath = "./src/Server" |> Path.getFullName
let clientPath = "./src/Client" |> Path.getFullName
let deployDir = "./deploy" |> Path.getFullName

let platformTool tool winTool =
  let tool = if Environment.isUnix then tool else winTool
  tool
  |> ProcessUtils.tryFindFileOnPath
  |> function Some t -> t | _ -> failwithf "%s not found" tool

let nodeTool = platformTool "node" "node.exe"

type JsPackageManager = 
  | NPM
  | YARN
  member this.Tool =
    match this with
    | NPM -> platformTool "npm" "npm.cmd"
    | YARN -> platformTool "yarn" "yarn.cmd"
   member this.ArgsInstall =
    match this with
    | NPM -> "install"
    | YARN -> "install --frozen-lockfile"

let jsPackageManager =
  let ctx = Context.forceFakeContext ()
  let arg =
    ctx.Arguments
    |> List.tryFind (fun s -> s.StartsWith("jsPackageManager"))
    |> Option.map (fun s -> s.Split('=') |> Array.last)
  match arg with
  | Some "npm" -> NPM
  | Some "yarn" | None -> YARN
  | Some _ -> failwith "Invalid JS package manager"

let runTool cmd args workingDir =
    let arguments = args |> String.split ' ' |> Arguments.OfArgs
    Command.RawCommand (cmd, arguments)
    |> CreateProcess.fromCommand
    |> CreateProcess.withWorkingDirectory workingDir
    |> CreateProcess.ensureExitCode
    |> Proc.run
    |> ignore

let runDotNet cmd workingDir =
    let result =
        DotNet.exec (DotNet.Options.withWorkingDirectory workingDir) cmd ""
    if result.ExitCode <> 0 then failwithf "'dotnet %s' failed in %s" cmd workingDir

let openBrowser url =
    //https://github.com/dotnet/corefx/issues/10361
    Command.ShellCommand url
    |> CreateProcess.fromCommand
    |> CreateProcess.ensureExitCodeWithMessage "opening browser failed"
    |> Proc.run
    |> ignore

Target.create "Clean" (fun _ -> 
  Shell.cleanDirs [deployDir]
)

Target.create "InstallClient" (fun _ ->
  Trace.tracefn "Node version:"
  runTool nodeTool "--version" __SOURCE_DIRECTORY__
  runTool jsPackageManager.Tool jsPackageManager.ArgsInstall  __SOURCE_DIRECTORY__
  runDotNet "restore" clientPath
)

Target.create "Build" (fun _ ->
    runDotNet "build" serverPath
    runDotNet "fable webpack-cli -- --config src/Client/webpack.config.js -p" clientPath
)

Target.create "Run" (fun _ ->
  let server = async { runDotNet "watch run" serverPath }
  let client = async { runDotNet "fable webpack-dev-server -- --config webpack.config.js" clientPath }
  let browser = async {
    do! Async.Sleep 5000
    openBrowser "http://localhost:8080"
  }

  [ server; client; browser]
  |> Async.Parallel
  |> Async.RunSynchronously
  |> ignore
)

open Fake.Core.TargetOperators

"Clean"
  ==> "InstallClient"
  ==> "Build"

"Clean"
    ==> "InstallClient"
    ==> "Run"

Target.runOrDefaultWithArguments "Build"