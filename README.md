# SAFE Dojo

This self-study repository is designed to allow you to experience the SAFE stack based on an ready-made application that you can build on top of. It will take around 90 minutes for you to complete if you have no experience in any of these technologies.

The `master` branch has the "incomplete" solution; please read the [instructions.md](Instructions.md) for a guide on completing this dojo to learn all about the [SAFE Stack](https://safe-stack.github.io/) and F#. There is a "completed" version in the `suggested-solution` branch.

## Prerequisites

* [dotnet SDK 3.1.1 or higher](https://dotnet.microsoft.com/download) The .NET Core SDK including CLI tools
* [FAKE 5](https://fake.build/fake-gettingstarted.html#Install-FAKE) F# build tool installed as a global tool
* [Yarn](https://yarnpkg.com/lang/en/docs/install/) NPM package manager
* [Node 10.x](https://nodejs.org/en/download/) installed for the front end components
* An F# code editor such as:
   * [VS Code](https://code.visualstudio.com/) + [Ionide](https://github.com/ionide/ionide-vscode-fsharp) extension
   * [Visual Studio](https://www.visualstudio.com/downloads/)
   * [Jetbrains Rider](https://www.jetbrains.com/rider/)

## Running the app
If this is your first time doing this, run `dotnet tool restore` first and then
`dotnet fake build`. If not, skip the first command.

If you're in VS Code, you can simply hit `F5` to build and run the application.

If using Visual Studio, do NOT attempt to build the solution directly in VS. Instead *must* use `dotnet fake build` to compile and run the application.
