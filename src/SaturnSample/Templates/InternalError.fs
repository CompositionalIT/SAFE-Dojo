module InternalError

open System
open Giraffe.GiraffeViewEngine
let layout (ex: Exception) =
    html [_class "has-navbar-fixed-top"] [  
        head [] [
            meta [_charset "utf-8"]
            meta [_name "viewport"; _content "width=device-width, initial-scale=1" ]
            title [] [encodedText "SaturnSample - Error #500"]
        ]
        body [] [
           h1 [] [rawText "ERROR #500"]
           h3 [] [rawText ex.Message]
           h4 [] [rawText ex.Source]
           p [] [rawText ex.StackTrace]
           a [_href "/" ] [rawText "Go back to home page"]
        ]
    ]