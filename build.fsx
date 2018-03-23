#r @"packages/build/FAKE/tools/FakeLib.dll"

open System

open Fake

let appPath = "./src/SaturnSample/" |> FullName

let dotnetcliVersion = DotNetCli.GetDotNetSDKVersionFromGlobalJson()

Target "Clean" DoNothing

Target "InstallDotNetCore" (fun _ ->
  DotNetCli.InstallDotNetSDK dotnetcliVersion |> ignore
)


Target "Restore" (fun _ ->
    DotNetCli.Restore (fun p -> {p with WorkingDir = appPath})
)

Target "Build" (fun _ ->
    DotNetCli.Build(fun p -> {p with WorkingDir = appPath})
)

Target "Run" (fun () ->
  let server = async {
    DotNetCli.RunCommand (fun p -> {p with WorkingDir = appPath}) "watch run"
  }
  let browser = async {
    Threading.Thread.Sleep 5000
    Diagnostics.Process.Start "http://localhost:8085" |> ignore
  }

  [ server; browser]
  |> Async.Parallel
  |> Async.RunSynchronously
  |> ignore
)

"Clean"
  ==> "InstallDotNetCore"
  ==> "Build"

"Clean"
  ==> "Restore"
  ==> "Run"

RunTargetOrDefault "Build"