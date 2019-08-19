#load "../.paket/load/main.group.fsx"

open Browser.Types
open Fable.React
open Fable.React.Props
open Elmish
open Elmish.Navigation
open Fable.React

let DashboardPage =
    FunctionComponent.Of (fun () -> str "Dashboard"
                          , "Dashboard")

let HeroesPage =
    FunctionComponent.Of (fun () -> str "Heroes"
                          ,"Heroes")

let DetailPage =
    FunctionComponent.Of (fun () -> str "Detail"
                          , "Detail")

type Hero = string

type Route =
    | Root
    | Dashboard
    | Heroes
    | Detail of int

let toRouteUrl route =
    match route with
    | Route.Root -> "/"
    | Route.Dashboard -> "/dashboard"
    | Route.Heroes -> "/heroes"
    | Route.Detail id -> sprintf "/detail/%i" id

type Model =
    { Heroes: Map<int,Hero>
      CurrentRoute: Route option }

type Msg =
    | Navigate of Route

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

let urlUpdate (route: Route option) (model: Model) =
    { model with CurrentRoute = route }, Cmd.none

let init _ =
    let model = { Heroes = Map.empty; CurrentRoute = None }
    let route = Routing.parsePath Browser.Dom.document.location
    urlUpdate route model


let update msg model =
    match msg with
    | Navigate route ->
        model, Navigation.newUrl (toRouteUrl route)

[<NoComparison>]
type AppContext = { Model: Model; Dispatch: Dispatch<Msg> }

let defaultContextValue : AppContext = Fable.Core.JS.undefined
let appContext = ReactBindings.React.createContext(defaultContextValue)

let useModel(): Model =
    let ac = Hooks.useContext(appContext)
    ac.Model

let useDispatch():Dispatch<Msg> =
    let ac = Hooks.useContext(appContext)
    ac.Dispatch

type AProps = { Children: ReactElement seq; Route: Route }
let A route children =
    FunctionComponent.Of(fun (props:AProps) ->
            let dispatch = useDispatch()
            let onClick (ev:Event) =
                ev.preventDefault()
                dispatch (Msg.Navigate props.Route)
            a [OnClick onClick] props.Children
        , "A") ({ Children = children; Route = route })

let layout page =
    div [] [
        h1 [] [str "Tour of heroes"]
        nav [] [
            A Route.Dashboard [str "Dashboard"]
            A Route.Heroes [str "Heroes"]
        ]
        page
    ]

let App =
    FunctionComponent.Of(fun () ->
        let model = useModel()
        match model.CurrentRoute with
        | Some(Route.Root) -> str "should redirect"
        | Some(Route.Dashboard) -> DashboardPage()
        | Some(Route.Heroes) -> HeroesPage()
        | Some(Route.Detail id) -> DetailPage()
        | None -> str "page not found"
        |> layout
    , "App")

let ElmishCapture =
    FunctionComponent.Of(fun (props:AppContext) ->
        contextProvider appContext props [ App() ]
    , "ElmishCapture")

let view model dispatch =
    ElmishCapture ({ Model = model; Dispatch = dispatch })

open Elmish.React
open Elmish.Navigation

Program.mkProgram init update view
|> Program.toNavigable Routing.parsePath urlUpdate
|> Program.withReactBatched "app"
|> Program.run