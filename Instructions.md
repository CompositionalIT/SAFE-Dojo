# Instructions

## Prologue: Build and Run
1. Clone this repo
1. Ensure that you have installed all the pre-requisites in `README.md`
1. If this is your first time doing this run `dotnet tool restore` command in your terminal.
1. Start the server and client by navigating to the repository root in your terminal and run `dotnet fake build`. The first time you run this, it may take a few seconds - it needs to download all the NPM dependencies as well as compile and run both client and server.
1. Your web browser should automatically be opened to the correct page and after a short wait, you will see the application running. You can also open the web page by navigating to http://localhost:8080
1. You should see a search entry box with a Submit button. Enter the UK postcode `EC2A 4NE` and hit Submit: you will see several empty panels and some basic Location details on the postcode.

Also note the following:

1. When you run the application, the [dotnet watch](https://docs.microsoft.com/en-us/aspnet/core/tutorials/dotnet-watch) tool is also run which detects file changes and automatically recompiles and reloads the server-side application. For example, in `DataAccess.fs`, try temporarily changing the value of `Town` (line 16) to a string such as `"A Town"` instead of `postcode.Result.AdminDistrict`. The server  application will automatically restarts, and the next time you make a query you will see your hard-coded text appear.
1. The front end application also supports [hot module reloading](https://webpack.js.org/concepts/hot-module-replacement/). Try changing the text `"UK Location Data Mashup"` to something else in the `App.fs` file; save the file and see how the front end automatically updates in the browser whilst still retaining the application state. There's no need to rebuild the application.

This method of rapid, iterative development is a powerful tool for SAFE apps.

## 1. Add a new endpoint
In this task, you'll add a new endpoint to the backend application which provides crime statistics. By the end of this task you will have an understanding of how routes are defined in the Saturn web framework.

1.1 Navigate to `src/Server/Api.fs` and find where the routes are defined in the `apiRouter` value.

1.2 Observe how we define a route for the distance endpoint. Also, note the function name and the signature of the `getDistanceFromLondon` function.

1.3 In the same manner as the distance endpoint above, add a new endpoint `/crime` which also accepts a postcode. Connect it to the `getCrimeReport` function.

1.4 Navigate to the application in the web browser and run a postcode search; observe how the chart has now appeared rendering the crime statistics returned from your new API endpoint.

1.5 Look at the `app.fs` in the Client to see how data from the API flows through to the chart. Which function is used to retrieve the data from the API? Where does the data go once the front end receives the network response?

## 2. Add shared code

In this task, you'll take some validation code which already exists on the backend service and use it within your front-end application. By the end of this task, you'll have an understanding of how code can be shared between an F# service and a Fable web application

2.1 Navigate to `src/Client/App.fs`. Look in the `update` function and examine the `PostcodeChanged` message handler. This code runs whenever the user changes the postcode field.

2.2 Using the `Validation.isValidPostcode` function, implement some validation logic to confirm that the postcode is valid. You'll need to update the `ValidationError` field in the model appropriate with either `Some` error or `None`.

2.3 Observe how the `Validation.isValidPostcode` function is also being used in the back-end as well as the front-end code

2.4 Navigate to the web application and try typing in an invalid postcode into the text box. Notice how your validation error is now being propagated into the UI

## 3. Add map

In this task, you'll add a map to the UI which shows the area surrounding the postcode on a map. By the end of this task, you'll understand how the view code in Elmish interacts with the Elmish data model and how to add "properties" onto elements.

3.1 In `src/Client/App.fs` update the `view` function to add in the map tile using the `mapTile` function. You will find a more detailed explanation in the function (search for Task 3.1).

3.2 Now that you have an empty map tile, navigate to the `mapTile` function itself and add a new item to the `map` props list, `MapProps.Center`. It takes a `LatLong` as input. You should now be able to see the map with the correct location!

3.3 Change the zoom level to `15.0` (note the `.0` - `Zoom` expects a float). Observe how the map updates almost immediately and smoothly zooms in.

3.4 Add a marker to display on the center point of the map, just after the `tileLayer`. There is already a helper function, `makeMarker`, that you can use. You can build up an appropriate description for the second argument, such as "{postcode} - {region}" or similar, using fields on the `Location` record that is passed in.

## 4. Implement weather endpoint

In this task, you need to add another tile to the UI including all the associated work on the backend of the system as well. We'll be adding a tile which shows the current weather for the given postcode as an image and then updating the tile to add in the current temperature. By the end of this task you should start to understand how a full stack SAFE application is built.

4.1 In `src/Server/Api.fs` implement the `getWeather` function following the same pattern as the other endpoints. Be sure to add the likes of postcode validation. Hint: The `asWeatherResponse` function will help to simplify the process of mapping the data

4.2 As in the task earlier, add a new route to the router which routes traffic to the `getWeather` function

4.3 Verify that your endpoint works and returns data either using CURL or your web browser

4.4 On the front end in the file `src/Client/App.fs`, update the `Report` type to include the `WeatherResponse`

4.5 In `src/Client/App.fs` in the `getResponse` function, call your endpoint using `Fetch.get` per the other API call, and populate the `Report` field with the returned data

4.6 Update the `view` function to include a call to the `weatherTile` function

4.7 Navigate to the web application and observe how the weather tile is now being rendered in the UI

4.8 Update the weather tile to include the temperature as well as the icon for the weather

## 5. Adding a clear button

This task is left as an exercise for the reader. In addition to the Submit button in the UI, add a Clear button, which clears the UI state and returns the UI to its initial state.

## 6. Trying the POST verb

Change a route to be POST based, rather than GET. You'll need to do the following:

1. Create a new `PostcodeRequest` record in `Shared.fs` which will store the Postcode sent to the server as the body of the request, instead of in the query string.

### On the client
1. Update the code that sends a request to the server to use `Fetch.post` instead of `Fetch.get`.
1. Change the url so that it is not parameterised any longer, and supply a `PostcodeRequest` record as the second argument (the payload).

### On the server
1. Modify the associated route in `apiRouter` to use `post` rather than `getF`.
1. Update the handler function to no longer retrieve the postcode via the query string. Instead, use `ctx.BindModelAsync<PostcodeRequest>()` to retrieve the body as a `PostcodeRequest` (you might need to add a type annotation to `ctx` to get intellisense to "show up"!).
