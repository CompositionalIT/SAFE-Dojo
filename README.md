# SAFE Dojo

This self-study repository is designed to allow you to experience the SAFE stack based on an ready-made application that you can build on top of. It will take around 90 minutes for you to complete if you have no experience in any of these technologies.

The `master` branch has the "incomplete" solution; please read the [instructions.md](Instructions.md) for a guide on completing this dojo to learn all about the [SAFE Stack](https://safe-stack.github.io/) and F#. There is a "completed" version in the `suggested-solution` branch.

## Prerequisites

* [dotnet SDK 2.1.4](https://github.com/dotnet/cli/releases/tag/v2.1.4) The .NET Core SDK
* [Yarn](https://yarnpkg.com/lang/en/docs/install/) package manager
* [Node 8.x](https://nodejs.org/en/download/) installed for the front end components
* [Mono](https://www.mono-project.com/docs/getting-started/install/) if you're running on Linux or OSX
* An F# code editor such as:
   * [VS Code](https://code.visualstudio.com/) + [Ionide](https://github.com/ionide/ionide-vscode-fsharp) extension
   * [Visual Studio 2017](https://www.visualstudio.com/downloads/)
   * [Jetbrains Rider](https://www.jetbrains.com/rider/)

## Building
`build.cmd run` or `./build.sh run`. You can optionally use npm instead of yarn by supplying the `jsPackageManager=npm` argument e.g. `build jsPackageManager=npm run`.

If you're in VS Code, you can simply hit `CTRL`+`SHIFT`+`B` to build and run the application.

If using Visual Studio 2017, [do NOT attempt to build the solution directly in VS](https://github.com/CompositionalIT/SAFE-Dojo/issues/24). You *must* use `build.cmd` to compile and run the application.

