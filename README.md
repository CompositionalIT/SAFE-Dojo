# SAFE Dojo

This self-study repository is designed to allow you to experience the SAFE stack based on a ready-made application that you can build on top of. It will take around 90 minutes for you to complete if you have no experience in any of these technologies.

The `master` branch has the "incomplete" solution; please read the [instructions.md](Instructions.md) for a guide on completing this dojo, to learn all about the [SAFE Stack](https://safe-stack.github.io/) and F#. There is a "completed" version in the `suggested-solution` branch.

## Install pre-requisites

You'll need to install the following pre-requisites in order to build SAFE applications

* [.NET Core SDK](https://www.microsoft.com/net/download) 6.0 or higher
* [Node LTS](https://nodejs.org/en/download/)

## Starting the application

Before you run the project **for the first time only** you must install dotnet "local tools" with this command:

```bash
dotnet tool restore
```

Open the editor:

```bash
code .
```

Build and run in watch mode use the following command:

```bash
dotnet run
```

> NOTE: You may have to allow `dotnet` or `Server` access to your public and/or private network.

Then open `http://localhost:8080` in your browser. Arrange the windows so you can see both Code editor and the web browser.

## Use the app

Type a UK postcode into the web app, e.g. "SW1A 2AA". Press "Fetch"

## Completing the tasks

Search files (Ctrl+Shift+F or Edit --> Find in Files) and search through for "Task 1" to start completing the Dojo.

See [Instructions.md](Instructions.md) for further details and hints about the tasks.

## Going further: bundling your app

There is `Bundle` to package your app:

```bash
dotnet run -- Bundle
```
## Going further: deploying to Azure

This requires these prerequisites:
* [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)

First time run

    az login

Then set the name of your app in Build.fs:

```
    let web = webApp {
        name "feiew02"  // set the name of your app here
    ...
```

To deploy to Azure:

```bash
dotnet run -- Azure
```

## SAFE Stack Documentation

If you want to know more about the full Azure Stack and all of it's components (including Azure) visit the official [SAFE documentation](https://safe-stack.github.io/docs/).

You will find more documentation about the used F# components at the following places:

* [Saturn](https://saturnframework.org/)
* [Fable](https://fable.io/docs/)
* [Elmish](https://elmish.github.io/elmish/)
