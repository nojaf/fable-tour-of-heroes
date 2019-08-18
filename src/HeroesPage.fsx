#load "Shared.fsx"

open Fable.Core.JsInterop
open Fable.React
open Fable.React.Props
open Shared

let private HeroesPage =
    FunctionComponent.Of (
        (fun () ->
            let model = useModel()
            let dispatch = useDispatch()
            let heroes =
                model.Heroes
                |> Map.toList
                |> List.map (fun (id,hero) ->
                    li [Key !!id] [
                        A (Route.Detail id) [
                            span [ClassName "badge"] [ofInt id]
                            em [] [str hero]
                        ]
                        button [ ClassName "delete"
                                 OnClick (fun _ -> Msg.RemoveHero id |> dispatch)
                                 DangerouslySetInnerHTML { __html = "&cross;" } ] []
                    ]
                )

            let onSubmit newHero =
                Msg.AddHero newHero
                |> dispatch

            div [Id "heroes" ] [
                h2 [] [str "My Heroes"]
                HeroForm ({ InitialHero = ""; OnSubmit = onSubmit; LabelText = "Hero name:"; ButtonText = "add" })
                ul [ClassName "heroes"] [ ofList heroes ]
            ]
        )
        ,"HeroesPage"
    )

exportDefault HeroesPage