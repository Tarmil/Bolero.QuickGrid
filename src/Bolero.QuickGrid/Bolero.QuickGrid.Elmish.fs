namespace Microsoft.AspNetCore.Components.QuickGrid.Elmish

open System.Threading.Tasks
open Elmish
open Microsoft.AspNetCore.Components.QuickGrid

[<RequireQualifiedAccess>]
module QuickGrid =

    /// The model for an ItemsProvider-based QuickGrid.
    type Model<'item> =
        { itemsProvider: GridItemsProvider<'item>
          getItems: GridItemsProviderRequest<'item> -> Async<GridItemsProviderResult<'item>> }

    /// The internal message for an ItemsProvider-based QuickGrid.
    [<RequireQualifiedAccess>]
    type Msg<'item> =
        | GetItems of GridItemsProviderRequest<'item> * (GridItemsProviderResult<'item> -> unit) * (exn -> unit)
        | InitItemsProvider of GridItemsProvider<'item>
        | InternalError of exn

    /// The external message for an ItemsProvider-based QuickGrid.
    type ExternalMsg<'item> =
        | NoOp
        | Error of exn

    /// Initialize an ItemsProvider-based QuickGrid with its items provider function.
    let initItemProvider getItems : Model<'item> * Cmd<Msg<'item>> =
        { itemsProvider = null; getItems = getItems },
        [ fun dispatch ->
            GridItemsProvider<'item>(fun request ->
                let tcs = TaskCompletionSource<_>()
                dispatch (Msg.GetItems (request, tcs.SetResult, tcs.SetException))
                ValueTask<GridItemsProviderResult<_>>(tcs.Task))
            |> Msg.InitItemsProvider
            |> dispatch
        ]

    /// <summary>
    /// Update an ItemsProvider-based QuickGrid.
    /// </summary>
    let update (message: Msg<'item>) (model: Model<'item>) : Model<'item> * Cmd<Msg<'item>> * ExternalMsg<'item> =
        match message with
        | Msg.InitItemsProvider p ->
            { model with itemsProvider = p }, Cmd.none, NoOp
        | Msg.GetItems (request, callback, exnCallback) ->
            model,
            [ fun dispatch -> Async.Start (async {
                try
                    let! x = model.getItems request
                    callback x
                with exn ->
                    exnCallback exn
                    dispatch (Msg.InternalError exn)
            }) ],
            NoOp
        | Msg.InternalError exn ->
            model, Cmd.none, Error exn

    /// <summary>
    /// Component builder for a QuickGrid using an Elmish items provider.
    /// </summary>
    let withModel (model: Model<'item>) =
        QuickGrid.withItemsProvider model.itemsProvider
