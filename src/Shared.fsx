#load "../.paket/load/main.group.fsx"

open Fable.React
open Fable.React.Props
open Elmish
open Browser.Types

type Hero = string

type Route =
    | Root
    | Dashboard
    | Heroes
    | Detail of int

type Msg =
    | Navigate of Route
    | RemoveHero of int
    | AddHero of Hero
    | UpdateHero of int * Hero
    | HeroesLoaded of Map<int,Hero>
    | HeroesFailedToLoad of string

type Model =
    { Heroes: Map<int, Hero>
      CurrentRoute: Route option
      SelectedHero: int option
      IsLoadingHeroes: bool }

[<NoComparison>]
type AppContext =
    { Model: Model
      Dispatch : Dispatch<Msg> }

let defaultContextValue : AppContext = Fable.Core.JS.undefined
let appContext = ReactBindings.React.createContext(defaultContextValue)

let useModel() : Model =
    let ac = Hooks.useContext(appContext)
    ac.Model

let useDispatch() : Dispatch<Msg> =
    let ac = Hooks.useContext(appContext)
    ac.Dispatch

type AProps = { Children: ReactElement seq; Route: Route }

let A route children =
    FunctionComponent.Of(
        fun (props:AProps) ->
            let dispatch = useDispatch()
            let onClick (ev:Event) =
                ev.preventDefault()
                Msg.Navigate props.Route
                |> dispatch
            a [OnClick onClick; Href "#"] children
        , "A", memoEqualsButFunctions) ({ Route = route; Children = children })

type HeroFormProps =
    { InitialHero: string
      OnSubmit: (Hero -> unit)
      LabelText: string
      ButtonText: string }
let HeroForm =
    FunctionComponent.Of (fun props ->
        // We use Reacts use-state here in order to prevent to keep change of the value of the input in the Model
        // Because it would bloat the history of Msg's and we only really care in respect to the Model when it is submitted.
        let hero = Hooks.useState(props.InitialHero)
        let onSubmit (ev:Event) =
            ev.preventDefault()
            props.OnSubmit hero.current

        form [OnSubmit onSubmit] [
            label [] [str props.LabelText]
            input [Value hero.current; OnChange (fun ev -> ev.Value |> hero.update)]
            button [] [str props.ButtonText]
        ]
    , "HeroForm")



