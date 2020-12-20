[![Gitpod ready-to-code](https://img.shields.io/badge/Gitpod-ready--to--code-blue?logo=gitpod)](https://gitpod.io/#https://github.com/CompositionalIT/SAFE-Dojo)

# SAFE Dojo

This self-study repository is designed to allow you to experience the SAFE stack based on a ready-made application that you can build on top of. It will take around 90 minutes for you to complete if you have no experience in any of these technologies.

The `master` branch has the "incomplete" solution; please read the [instructions.md](Instructions.md) for a guide on completing this dojo to learn all about the [SAFE Stack](https://safe-stack.github.io/) and F#. There is a "completed" version in the `suggested-solution` branch.

## Prerequisites

* [dotnet SDK 3.1.1 or higher](https://dotnet.microsoft.com/download). The .NET Core SDK including CLI tools
* [Yarn](https://yarnpkg.com/lang/en/docs/install/) or [NPM](https://www.npmjs.com/get-npm) package manager
* [Node 14.x](https://nodejs.org/en/download/) installed for the front-end components
* An F# code editor such as:
   * [VS Code](https://code.visualstudio.com/) + [Ionide](https://github.com/ionide/ionide-vscode-fsharp) extension
   * [Visual Studio](https://www.visualstudio.com/downloads/)
   * [Jetbrains Rider](https://www.jetbrains.com/rider/)

## Running the app
1. If this is your first time starting the app, run `dotnet tool restore`.
2. Run `dotnet fake build` to launch the application. If you're in VS Code, you can also hit `F5` to build and run the application.

If using Visual Studio, do NOT attempt to build the solution directly in VS. Instead you *must* use `dotnet fake build` to compile and run the application.
