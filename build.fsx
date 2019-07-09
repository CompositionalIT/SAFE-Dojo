#r "paket: groupref build //"
#load "./.fake/build.fsx/intellisense.fsx"

#if !FAKE
#r "netstandard"
#r "Facades/netstandard" // https://github.com/ionide/ionide-vscode-fsharp/issues/839#issuecomment-396296095
#endif

open Fake.Core
open Fake.DotNet
open Fake.IO

let serverPath = Path.getFullName "./src/Server"
let clientPath = Path.getFullName "./src/Client"
let clientDeployPath = Path.combine clientPath "deploy"
let deployDir = Path.getFullName "./deploy"

let platformTool tool winTool =
    let tool = if Environment.isUnix then tool else winTool
    match ProcessUtils.tryFindFileOnPath tool with
    | Some t -> t
    | _ ->
        let errorMsg =
            tool + " was not found in path. " +
            "Please install it and make sure it's available from your path. " +
            "See https://safe-stack.github.io/docs/quickstart/#install-pre-requisites for more info"
        failwith errorMsg

type JsPackageManager =
    | NPM
    | YARN
    member this.RestoreTool =
        match this with
        | NPM -> platformTool "npm" "npm.cmd"
        | YARN -> platformTool "yarn" "yarn.cmd"
    member this.RunTool =
        match this with
        | NPM -> platformTool "npx" "npx.cmd"
        | YARN -> platformTool "yarn" "yarn.cmd"
    member this.ArgsInstall =
        match this with
        | NPM -> "install"
        | YARN -> "install --frozen-lockfile"

let getJsPackageManager () =
    match Environment.environVarOrDefault "jsPackageManager" "yarn" with
    | "npm" -> NPM
    | "yarn" | _ -> YARN

let nodeTool = platformTool "node" "node.exe"

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
    [ deployDir
      clientDeployPath ]
    |> Shell.cleanDirs
)

Target.create "InstallClient" (fun _ ->
    let jsPackageManager = getJsPackageManager ()

    printfn "Node version:"
    runTool nodeTool "--version" __SOURCE_DIRECTORY__
    printfn "Npm version:"
    runTool jsPackageManager.RestoreTool "--version"  __SOURCE_DIRECTORY__
    runTool jsPackageManager.RestoreTool "install" __SOURCE_DIRECTORY__
    runDotNet "restore" clientPath
)

Target.create "Build" (fun _ ->
    let jsPackageManager = getJsPackageManager ()

    runDotNet "build" serverPath
    runTool jsPackageManager.RunTool "webpack-cli -p" __SOURCE_DIRECTORY__
)

Target.create "Run" (fun _ ->
    let jsPackageManager = getJsPackageManager ()

    let server = async {
        runDotNet "watch run" serverPath
    }
    let client = async {
        runTool jsPackageManager.RunTool "webpack-dev-server" __SOURCE_DIRECTORY__
    }
    let browser = async {
        do! Async.Sleep 5000
        openBrowser "http://localhost:8080"
    }

    let vsCodeSession = Environment.hasEnvironVar "vsCodeSession"
    let safeClientOnly = Environment.hasEnvironVar "safeClientOnly"

    let tasks =
        [ if not safeClientOnly then yield server
          yield client
          if not vsCodeSession then yield browser ]

    tasks
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

Target.runOrDefaultWithArguments "Run"
