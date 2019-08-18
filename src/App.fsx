open Fable.React

#load "Shared.fsx"

open Fable.Core.JsInterop
open Fable.React
open Fable.React.Props
open Elmish
open Elmish.React
open Elmish.Navigation
open Elmish.Debug
open Shared
let superFriends =
    [ 1, "The Man of Steel"
      2, "The Dark Knight"
      3, "The Amazing Amazon"
      4, "The Emerald Guardian"
      5, "The Scarlet Speedster"
      6, "The King of the Seven Seas"
      7, "The Martian Manhunter" ]
    |> Map.ofList

module Routing =
    open Elmish.UrlParser

    let private route =
        oneOf [
            map Route.Root (s "")
            map Route.Dashboard (s "dashboard")
            map Route.Heroes (s "heroes")
            map Route.Detail (s "detail" </> i32)
        ]

    let parsePath location = UrlParser.parsePath route location

let toRouteUrl route =
    match route with
    | Route.Root -> "/"
    | Route.Dashboard -> "/dashboard"
    | Route.Heroes -> "/heroes"
    | Route.Detail id -> sprintf "/detail/%d" id

let urlUpdate (route: Route option) (model: Model) =
    match route with
    | Some(Route.Root) ->
        model, Cmd.ofMsg (Navigate Route.Dashboard)
    | Some(Route.Detail id) ->
        if Map.containsKey id model.Heroes then
            { model with SelectedHero = Some id
                         CurrentRoute = route }, Cmd.none
        else
            { model with CurrentRoute = None }, Cmd.none
    | _ ->
        { model with CurrentRoute = route }, Cmd.none
let init _ =
    let model =
        { Heroes = superFriends
          CurrentRoute = None
          SelectedHero = None }
    let route = Routing.parsePath Browser.Dom.document.location
    urlUpdate route model

let update msg model =
    match msg with
    | Navigate route ->
        model, Navigation.newUrl (toRouteUrl route)
    | RemoveHero id ->
        let heroes = Map.remove id model.Heroes
        { model with Heroes = heroes }, Cmd.none
    | AddHero hero ->
        let id =
            Map.toList model.Heroes
            |> List.map fst
            |> List.max
            |> (+) 1
        let heroes = Map.add id hero model.Heroes
        { model with Heroes = heroes }, Cmd.ofMsg (Navigate (Route.Detail id))
    | UpdateHero (id,hero) ->
        let heroes =
            Map.add id hero model.Heroes
        { model with Heroes = heroes }, Cmd.ofMsg (Navigate Route.Heroes)

let suspense fallback children =
    let props = createObj [ "fallback" ==> fallback ]
    ofImport "Suspense" "react" props children

let fallback =
    p [] [ i [ClassName "spin"; DangerouslySetInnerHTML { __html = "&orarr;" }] []
           str "loading your page..." ]

let layout page =
    div [] [
        h1 [] [str "Tour of Heroes"]
        nav [] [
            A Route.Dashboard [ str "Dashboard" ]
            A Route.Heroes [ str "Heroes" ]
        ]
        suspense fallback [page]
    ]

let DashboardPage props : ReactElement =
    let dashboard = ReactBindings.React.``lazy`` (fun () -> importDynamic "./DashboardPage.fsx")
    ReactBindings.React.createElement(dashboard, props, [])

let HeroesPage props : ReactElement =
    let heroesPage = ReactBindings.React.``lazy`` (fun () -> importDynamic "./HeroesPage.fsx")
    ReactBindings.React.createElement(heroesPage, props, [])

let DetailPage props : ReactElement =
    let detailPage = ReactBindings.React.``lazy`` (fun () -> importDynamic "./DetailPage.fsx")
    ReactBindings.React.createElement(detailPage, props, [])

let App =
    FunctionComponent.Of (fun () ->
        let model = useModel()

        match model.CurrentRoute with
        | Some(Route.Root) -> str "redirecting..."
        | Some(Route.Dashboard) -> DashboardPage()
        | Some(Route.Heroes) -> HeroesPage()
        | Some(Route.Detail _) -> DetailPage()
        | None -> h1 [] [str "404 - Hero not found"]
        |> layout
    , "App")

let ElmishCapture =
    FunctionComponent.Of (
        fun (props:AppContext) ->
            contextProvider appContext props [ App() ]
        , "ElmishCapture", memoEqualsButFunctions)

let view model dispatch =
    ElmishCapture { Model = model; Dispatch = dispatch }

Program.mkProgram init update view
#if DEBUG
|> Program.withDebugger
#endif
|> Program.toNavigable Routing.parsePath urlUpdate
|> Program.withReactBatched "app"
|> Program.run