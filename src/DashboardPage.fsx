#load "Shared.fsx"

open Fable.Core
open Fable.Core.JsInterop
open Fable.React
open Fable.React.Props
open Shared

[<RequireQualifiedAccess>]
module private ReactSelect =
    let selectImport : ReactElement = importDefault "react-select"
    type SelectOption<'t> = { label:string; value:'t }
    type SelectProps<'t> =
        | OnChange of (SelectOption<'t> -> unit)
        | Options of SelectOption<'t> array

    let Select (props: SelectProps<'t> seq) =
        let props = keyValueList CaseRules.LowerFirst props
        ReactBindings.React.createElement(selectImport, props, [])

let private heroSearch onChange heroes =
    let options =
        heroes
        |> Map.toList
        |> List.map (fun (id,name) -> { label = name; value = !!id }:ReactSelect.SelectOption<int>)
        |> List.toArray

    div [ClassName "w50"] [ ReactSelect.Select [ReactSelect.Options options;ReactSelect.OnChange onChange] ]

let private DashboardPage =
    FunctionComponent.Of
        ((fun () ->
            let model = useModel()
            let dispatch = useDispatch()

            let topHeroes =
                model.Heroes
                |> Map.toList
                |> fun heroes -> if List.length heroes >= 4 then List.take 4 heroes else heroes
                |> List.map (fun (id,hero) ->
                    fragment [FragmentProp.Key (id.ToString())] [
                        A (Route.Detail id) [
                            h4 [] [str hero]
                        ]
                    ]
                )

            let onChange ({ value = id }:ReactSelect.SelectOption<int>) =
                Route.Detail id
                |> Msg.Navigate
                |> dispatch

            div [Id "dashboard"] [
                h2 [] [str "Top Heroes"]
                div [ClassName "grid"] [ ofList topHeroes ]
                heroSearch onChange model.Heroes
            ]
        )
        ,"DashboardPage")

exportDefault DashboardPage