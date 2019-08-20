#load "Shared.fsx"

open Fable.Core.JsInterop
open Fable.React
open Fable.React.Props
open Elmish
open Elmish.React
open Elmish.Navigation
open Elmish.Debug
open Shared
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
            { model with CurrentRoute = route }, Cmd.none
    | _ ->
        { model with CurrentRoute = route }, Cmd.none

open Thoth.Json

let getHeroes (dispatch:Dispatch<Msg>) =
    let decoder =
        Decode.object (fun get ->
            get.Required.Field "id" Decode.int, get.Required.Field "name" Decode.string
        )
        |> Decode.array

    Fetch.fetch "/heroes.json" []
    |> Promise.bind (fun response -> response.text())
    |> Promise.map (fun json ->
        // We don't trust the incoming JSON, so we use a decode function to verify it is what we think it is.
        Decode.fromString decoder json
        |> fun result ->
            match result with
            | Ok heroes ->
                heroes
                |> Map.ofArray
                |> Msg.HeroesLoaded
                |> dispatch
            | Error err ->
                Msg.HeroesFailedToLoad err
                |> dispatch
    )
    |> Promise.catchEnd (fun ex ->
        Msg.HeroesFailedToLoad (ex.ToString())
        |> dispatch
    )


let init _ =
    let model =
        { Heroes = Map.empty
          CurrentRoute = None
          SelectedHero = None
          IsLoadingHeroes = true }
    let route = Routing.parsePath Browser.Dom.document.location
    let cmd = Cmd.ofSub getHeroes
    let model', cmd' = urlUpdate route model
    // We combine the possible Commands from the urlUpdate with our own create Cmd of getHeroes
    model', Cmd.batch [cmd;cmd']

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
        { model with Heroes = heroes
                     IsLoadingHeroes = false }, Cmd.ofMsg (Navigate Route.Heroes)
    | HeroesLoaded heroes ->
        // if needed force a reload of the Detail page, in urlUpdate the correct heroes will be set if found.
        let cmd =
            match model.CurrentRoute with
            | Some(Route.Detail id) ->Cmd.ofMsg (Navigate (Route.Detail id))
            | _ -> Cmd.none

        { model with Heroes = heroes
                     IsLoadingHeroes = false }, cmd
    | HeroesFailedToLoad err ->
        printfn "Error while loading the heroes: %s" err
        // In a typical application you would add something to the Model and display to user that something went wrong.
        // For now we are going to just log the error
        model, Cmd.none

// See https://reactjs.org/docs/code-splitting.html#suspense
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


// See https://reactjs.org/docs/code-splitting.html#reactlazy
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

        if model.IsLoadingHeroes then
            fallback
        else
            match model.CurrentRoute with
            | Some(Route.Root) -> str "redirecting..."
            | Some(Route.Dashboard) -> DashboardPage()
            | Some(Route.Heroes) -> HeroesPage()
            | Some(Route.Detail _) -> DetailPage()
            | None -> h1 [] [str "404 - Hero not found"]
        |> layout
    , "App")

// See https://reactjs.org/docs/context.html
let ElmishCapture =
    FunctionComponent.Of (
        fun (props:AppContext) ->
            contextProvider appContext props [ App() ]
        , "ElmishCapture", memoEqualsButFunctions)

let view model dispatch =
    ElmishCapture { Model = model; Dispatch = dispatch }

Program.mkProgram init update view
#if DEBUG // with the hash directive, the debug functionality does not end up in our production bundle
|> Program.withDebugger
#endif
|> Program.toNavigable Routing.parsePath urlUpdate
|> Program.withReactBatched "app"
|> Program.run