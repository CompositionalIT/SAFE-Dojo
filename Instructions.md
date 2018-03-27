# Instructions

## Prologue: Build and Run

i. Clone this repo

ii. Start the server and client by navigating to this directory in your terminal and run ```build.cmd Run``` on Windows or ```./build.sh Run``` on Mac and Linux

iii. Your web browser should automatically be opened to the correct page and after some time, you should see the application running. You can also open the web page by navigating to http://localhost:8080

iv. When you run the application, the dotnet watch tool is also run which detects file changes and automatically recompiles and reloads the application

v. Try changing an endpoint response and see how it's automatically reflected in the running application

vi. The front end application also supports hot module reloading. Try changing some of the view code and see how the front end automatically updates in the browser whilst also retaining the application state

## 1. Add a new endpoint

In this task, you'll be adding a new endpoint to the backend application which provides crime statistics. By the end of this task you should have an understanding of how routes are defined in the Saturn web framework.

1.1 Navigate to the file ```src/Server/app.fs``` and find where the routes are defined in the apiRouter value

1.2 Observe how we define a route for the distance endpoint. Note the function name and the signature of the ```getDistanceFromLondon``` function

1.3 In the same manner as the distance endpoint above, add a new endpoint ```/crime``` which also accepts a postcode. Connect it to the ```getCrimeReport``` function.

1.4 Navigate to the application in the web browser and observe how the chart has now appeared rendering the crime statistics returned from your new API endpoint. 

## 2. Add shared code

In this task, you'll take some validation code which already exists on the backend service and use it within your front end application. By the end of this task, you'll have an understanding of how code can be shared between an F# service and a Fable web application

2.1 Navigate to ```src/Client/app.fs``` and look in the ```update``` function. Observe the ```PostcodeChanged event```

2.2 Using the ```Validation.validatePostcode``` function, implement some validation logic, updating the model as appropriate

2.3 Observe how the ```Validation.validatePostcode``` function is also being used in the back end code as well as the front end code

2.4 Navigate to the web application and try typing in an invalid postcode into the text box. Notice how your validation error is now being propagated into the UI

## 3. Add map

In this task, you'll add a map to the UI which shows the area surrounding the postcode on a map. By the end of this task, you'll understand how the view code in Elmish interacts with the Elmish data model.

3.1 In ```src/Client/app.fs``` update the ```mapTile``` function to call the ```getBingMapUrl``` function and get a valid URL to embed a Bing map into the UI

3.2 Using your retrieved URL, update the Src attribute on the iframe to retrieve embed the Bing map

3.3 Navigate to the web application and observe how the Bing map is now being rendered in the UI

## 4. Implement weather endpoint

In this task, you need to add another tile to the UI including all the associated work on the backend of the system as well. We'll be adding a tile which shows the current weather for the given postcode as an image and then updating the tile to add in the current temperature. By the end of this task you should start to understand how a full stack SAFE application is built.

4.1 In ```src/Server/api.fs``` implement the ```getWeatherResponse``` function following the same pattern as the other endpoints. Be sure to add the likes of postcode validation. Hint: The ```asWeatherResponse``` function will help to simplify the process of mapping the data

4.2 As in the task earlier, add a new route to the router which routes traffic to the ```getWeatherResponse``` function

4.3 Verify that your endpoint works and returns data either using CURL or your web browser

4.4 On the front end in the file ```src/Client/app.fs```, update the ```Report``` type to include the ```WeatherResponse```

4.5 In ```src/Client/app.fs``` in the ```getResponse``` function, call your endpoint using Fetch as the other APIs and populate your Report with the returned data

4.6 Update the ```view``` function to include a call to the ```weatherTile``` function

4.7 Navigate to the web application and observe how the weather tile is now being rendered in the UI

4.8 Update the weather tile to include the temperature as well as the icon for the weather

## 5. Adding a clear button

This task is left as an exercise for the reader. In addition to the Submit button in the UI, add a Clear button, which clears the UI state and returns the UI to its initial state.