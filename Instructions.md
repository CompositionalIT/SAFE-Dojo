# Instructions

## Prologue: Build and Run

i. Clone this repo

ii. Start the server and client by navigating to this directory in your terminal and run ```fake build -t Run```

iii. Your web browser should automatically be opened to the correct page and after a short wait, you will see the application running. You can also open the web page by navigating to http://localhost:8080

iv. When you run the application, the [dotnet watch](https://docs.microsoft.com/en-us/aspnet/core/tutorials/dotnet-watch) tool is also run which detects file changes and automatically recompiles and reloads the server-side application.

v. In `Api.fs` in the Server, try changing an endpoint response and see in the console window the application restarts, and the change is automatically reflected in the running application.

vi. The front end application also supports [hot module reloading](https://webpack.js.org/concepts/hot-module-replacement/). Try changing some of the view code in `App.fs` in the Client and see how the front end automatically updates in the browser whilst still retaining the application state.

## 1. Add a new endpoint

In this task, you'll add a new endpoint to the backend application which provides crime statistics. By the end of this task you will have an understanding of how routes are defined in the Saturn web framework.

1.1 Navigate to ```src/Server/Api.fs``` and find where the routes are defined in the `apiRouter` value.

1.2 Observe how we define a route for the distance endpoint. Also, note the function name and the signature of the ```getDistanceFromLondon``` function.

1.3 In the same manner as the distance endpoint above, add a new endpoint ```/crime``` which also accepts a postcode. Connect it to the ```getCrimeReport``` function.

1.4 Navigate to the application in the web browser and observe how the chart has now appeared rendering the crime statistics returned from your new API endpoint.

1.5 Look at the `app.fs` in the Client to see how data from the API flows through to the chart. Which function is used to retrieve the data from the API? Where does the data go once the front end receives the network response?

## 2. Add shared code

In this task, you'll take some validation code which already exists on the backend service and use it within your front end application. By the end of this task, you'll have an understanding of how code can be shared between an F# service and a Fable web application

2.1 Navigate to ```src/Client/App.fs```. Look in the ```update``` function and examine the ```PostcodeChanged``` message handler. This code runs whenever the user changes the postcode field.

2.2 Using the ```Validation.isValidPostcode``` function, implement some validation logic to confirm that the postcode is valid. You'll need to update the ```ValidationError``` field in the model appropriate with either ```Some``` error or ```None```.

2.3 Observe how the ```Validation.isValidPostcode``` function is also being used in the back-end as well as the front-end code

2.4 Navigate to the web application and try typing in an invalid postcode into the text box. Notice how your validation error is now being propagated into the UI

## 3. Add map

In this task, you'll add a map to the UI which shows the area surrounding the postcode on a map. By the end of this task, you'll understand how the view code in Elmish interacts with the Elmish data model.

3.1 In ```src/Client/App.fs``` update the ```bingMapTile``` function to call the ```getBingMapUrl``` function and get a valid URL to embed a Bing map into the UI

3.2 Using your retrieved URL, update the Src attribute on the iframe to retrieve embed the Bing map

3.3 Navigate to the web application and observe how the Bing map is now being rendered in the UI

3.4 Start to explore how data flows from the updated model through to the UI. What causes the map to update?

## 4. Implement weather endpoint

In this task, you need to add another tile to the UI including all the associated work on the backend of the system as well. We'll be adding a tile which shows the current weather for the given postcode as an image and then updating the tile to add in the current temperature. By the end of this task you should start to understand how a full stack SAFE application is built.

4.1 In ```src/Server/Api.fs``` implement the ```getWeather``` function following the same pattern as the other endpoints. Be sure to add the likes of postcode validation. Hint: The ```asWeatherResponse``` function will help to simplify the process of mapping the data

4.2 As in the task earlier, add a new route to the router which routes traffic to the ```getWeather``` function

4.3 Verify that your endpoint works and returns data either using CURL or your web browser

4.4 On the front end in the file ```src/Client/App.fs```, update the ```Report``` type to include the ```WeatherResponse```

4.5 In ```src/Client/App.fs``` in the ```getResponse``` function, call your endpoint using Fetch as the other APIs and populate your Report with the returned data

4.6 Update the ```view``` function to include a call to the ```weatherTile``` function

4.7 Navigate to the web application and observe how the weather tile is now being rendered in the UI

4.8 Update the weather tile to include the temperature as well as the icon for the weather

## 5. Adding a clear button

This task is left as an exercise for the reader. In addition to the Submit button in the UI, add a Clear button, which clears the UI state and returns the UI to its initial state.

## 6. Trying the POST verb

Change a routes to be POST based, rather than GET. You'll need to do the following:

i. Create a new ```PostcodeRequest``` record in ```Shared.fs``` which will store the Postcode sent to the server as the body of the request, instead of in the query string.

### On the client
ii. Update the code that sends a request to the server to use ```postRecord``` instead of ```fetchAs```. Notice that the return type is different though when using ```postRecord```. To get the data returned from the server you can use a combination ```text``` method on the response and the ```Decode.Auto.unsafeFromString<'T>``` function to safely retrieve the typed result from the server. Do not use ```json``` on the response as it does not work with more complex F# types e.g. DUs. Tip: you can easily create a helper function to compose ```text()``` and ```Decode.Auto.unsafeFromString<'T>```. If you take this approach, then note the addition of the ``inline`` modifier, this is needed to ensure that the type information is not lost when we create this function:

```fsharp
let inline getJson<'T> (response:Fetch.Fetch_types.Response) = response.text() |> Promise.map Decode.Auto.unsafeFromString<'T>
```


### On the server
c. Modify the associated route in ```apiRouter``` to use ```post``` rather than ```getF```.

d. Update the handler function to no longer retrieve the postcode via the query string. Instead, use ```ctx.BindModelAsync()``` to retrieve the body as a ```PostcodeRequest```.
