module Index

open Giraffe.GiraffeViewEngine

let index =
    [
        section [_class "hero is-primary"] [
            div [_class "hero-body"] [
                div [_class "container"] [
                    div [_class "columns is-vcentered"] [
                        div [_class "column"] [
                            p [_class "title"] [rawText "Welcome to Saturn!"]
                            p [_class "subtitle"] [rawText "Opinionated, web development framework for F# which implements the server-side, functional MVC pattern"]
                        ]
                    ]
                ]
            ]
        ]
        section [_class "section"] [
            h1 [_class "title"] [rawText "Resources"]
            div [_class "tile is-ancestor"] [
                div [_class "tile is-parent is-4"] [
                    article [_class "tile is-child notification is-primary box"] [
                        a [_class "title "] [rawText "Guides (WIP)"]
                    ]
                ]
                div [_class "tile is-parent is-4"] [
                    article [_class "tile is-child notification is-info box"] [
                        a [_class "title"] [rawText "Documentation (WIP)"]
                    ]
                ]
                div [_class "tile is-parent is-4"] [
                    article [_class "tile is-child notification is-warning box"] [
                        a [_class "title"; _href "https://github.com/SaturnFramework/Saturn" ] [rawText "Source"]
                    ]
                ]

            ]
        ]
        section [_class "section"] [
            h1 [_class "title"] [rawText "Help"]
            div [_class "tile is-ancestor"] [
                div [_class "tile is-parent is-4"] [
                    article [_class "tile is-child notification is-link box"] [
                        a [_class "title"; _href "https://github.com/SaturnFramework/Saturn/issues"] [rawText "GitHub issues"]
                    ]
                ]
                div [_class "tile is-parent is-4"] [
                    article [_class "tile is-child notification is-danger box"] [
                        a [_class "title"; _href "https://gitter.im/SaturnFramework/Saturn"] [rawText "Gitter"]
                    ]
                ]
                div [_class "tile is-parent is-4"] [
                    article [_class "tile is-child notification is-success box"] [
                        a [_class "title"; _href "https://safe-stack.github.io/"] [rawText "SAFE Stack"]
                    ]
                ]

            ]
        ]
    ]

let layout =
    App.layout index