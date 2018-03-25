module Client

open Elmish
open Elmish.React
open App

let init () = 
  { Postcode = null; Response = None; ValidationError = None }, Cmd.none
 
#if DEBUG
open Elmish.Debug
open Elmish.HMR
#endif

Program.mkProgram init update view
#if DEBUG
|> Program.withConsoleTrace
|> Program.withHMR
#endif
|> Program.withReact "elmish-app"
#if DEBUG
|> Program.withDebugger
#endif
|> Program.run
