# Instructions

## Prologue: Build and Run
1. Clone this repo
1. Ensure that you have installed all the pre-requisites in `README.md`
1. If this is your first time doing this, run `dotnet tool restore` command in your terminal.
1. Start the server and client by navigating to the repository root in your terminal and run `dotnet run`. The first time you run this, it may take a few seconds - it needs to download all the NPM dependencies as well as compile and run both client and server.
1. Open the web page by navigating to http://localhost:8080 to see the app running
1. You should see a search entry box with a Submit button. Enter the UK postcode `EC2A 4NE` and hit Submit: you will see several empty spaces and some basic Location details on the postcode.

Also note the following:

1. When you run the application, the [dotnet watch](https://docs.microsoft.com/en-us/aspnet/core/tutorials/dotnet-watch) tool is also run which detects file changes and automatically recompiles and reloads the server-side application. For example, in `DataAccess.fs`, try temporarily changing the value of `Town` (line 15) to a string such as `"A Town"` instead of `postcode.Result.AdminDistrict`. The server  application will automatically restart, and the next time you make a query you will see your hard-coded text appear.
1. The front-end application also supports [hot module reloading](https://webpack.js.org/concepts/hot-module-replacement/). Try changing the text `"SAFE Dojo"` to something else in the `src/Client/Index.fs` file; save the file and see how the front-end automatically updates in the browser whilst still retaining the application state. There's no need to rebuild the application.

This method of rapid, iterative development is a powerful tool for SAFE apps.

## 1. Add a new endpoint
In this task, you'll add a new endpoint to the backend application which provides crime statistics. By the end of this task you will have an understanding of how routes are defined in the Saturn web framework.

1.1 Navigate to `src/Server/Api.fs` and find where the endpoints are defined in the `dojoApi` record.

1.2 Observe how we bind a function to the GetDistance endpoint. Also, note the function name and the signature of the `getDistanceFromLondon` function.

1.3 In the same manner as the GetDistance endpoint above, delete the anonymous function that's bound to the GetCrimes endpoint which currently returns an empty array and bind the `getCrimeReport` function to it instead.

1.4 Navigate to the application in the web browser and run a postcode search; observe how the chart has now appeared rendering the crime statistics returned from your new API endpoint.

1.5 Look at `src/Client/Index.fs` to see how data from the API flows through to the chart. Which function is used to retrieve the data from the API? Where does the data go once the front-end receives the network response?

## 2. Add shared code

In this task, you'll take some validation code which already exists on the backend service and use it within your front-end application. By the end of this task, you'll have an understanding of how code can be shared between an F# service and a Fable web application

2.1 Navigate to `src/Client/Index.fs`. Look in the `update` function and examine the `PostcodeChanged` message handler. This code runs whenever the user changes the postcode field.

2.2 Using the `Validation.isValidPostcode` function, implement some validation logic to confirm that the postcode is valid. You'll need to update the `ValidationError` field in the model appropriate with either `Some` error or `None`.

2.3 Observe how the `Validation.isValidPostcode` function is also being used in the back-end as well as the front-end code.

2.4 Navigate to the web application and try typing in an invalid postcode into the text box. Notice how your validation error is now being propagated into the UI.

## 3. Add map

In this task, you'll add a map to the UI which shows the area surrounding the postcode on a map. By the end of this task, you'll understand how the view code in Elmish interacts with the Elmish data model and how to add "properties" onto elements.

3.1 In `src/Client/Index.fs` update the `view` function to add in the map tile using the `mapWidget` function. You will find a more detailed explanation in the function (search for Task 3.1).

3.2 Now that you have an empty map tile, navigate to the `mapWidget` function itself and add a new item to the `map` props list, `map.center`. It takes a `LatLong` as input. You should now be able to see the map with the correct location!

3.3 Change the zoom level to `15`. Observe how the map updates almost immediately and smoothly zooms in.

3.4 Add a marker to display on the centre point of the map. There is already a helper function, `makeMarker`, that you can use. You can build up an appropriate description for the second argument, such as "{postcode} - {region}" or similar, using fields on the `Location` record that is passed in.

## 4. Implement weather endpoint

In this task, you need to add another widget to the UI including all the associated work on the backend of the system as well. We'll be adding a widget which shows the current weather for the given postcode as an image and then updating the widget to add in the current temperature. By the end of this task you should start to understand how a full stack SAFE application is built.

4.1 In `src/Server/Api.fs` implement the `getWeather` function following the same pattern as the other endpoints. Be sure to add the likes of postcode validation. Hint: The `asWeatherResponse` function will help to simplify the process of mapping the data.

4.2 As in the task earlier, you need an endpoint in the `dojoAPI` which will have the `getWeather` function bound to it. However, since this endpoint doesn't exist yet unlike in the earlier task, you first need to change the definition of `IDojoAPI`. Find the type `IDojoAPI` in `src/Shared/Shared.fs` and add to it a new field named GetWeather which has the type signature of `string -> WeatherResponse Async`. Then, navigate back to `src/Server/Api.fs` and change the implementation of `dojoAPI` to bind the `getWeather` function to the GetWeather endpoint.

4.3 On the front end in the file `src/Client/Index.fs`, update the `Report` type to include the `WeatherResponse`

4.4 In `src/Client/Index.fs` in the `getResponse` function, call your endpoint using the `dojoApi` per the other API calls, and populate the `Report` field with the returned data

4.5 Update the `view` function to include a call to the `weatherWidget` function

4.6 Navigate to the web application and observe how the weather tile is now being rendered in the UI

4.7 Update the weather widget to include the temperature as well as the icon for the weather

## 5. Adding a clear button

This task is left as an exercise for the reader. In addition to the Submit button in the UI, add a Clear button, which clears the UI state and returns the UI to its initial state.
