#load "Shared.fsx"

open Fable.Core.JsInterop
open Fable.React
open Shared

let private DetailPage =
    FunctionComponent.Of (
        (fun () ->
            let model = useModel()
            let dispatch = useDispatch()

            model.SelectedHero
            |> Option.bind (fun id ->
                Map.tryFind id model.Heroes
                |> Option.map (fun hero -> (id,hero))
            )
            |> Option.map (fun (id, hero) ->
                let onSubmit updatedHero =
                    Msg.UpdateHero(id,updatedHero)
                    |> dispatch
                div [] [
                    h2 [] [str hero]
                    HeroForm ({ InitialHero = hero; OnSubmit = onSubmit; LabelText = "Edit"; ButtonText = "update" })
                ]
            )
            |> ofOption
        )
        ,"DetailPage")

// export default is required for React to import lazy components
exportDefault DetailPage