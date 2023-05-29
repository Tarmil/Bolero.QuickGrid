namespace Bolero

open Elmish
open Microsoft.AspNetCore.Components.QuickGrid

type QuickGridMessage<'item> =
    | GetItems of GridItemsProviderRequest<'item> * (GridItemsProviderResult<'item> -> unit)

module Cmd =

    module QuickGrid =

        let either
                (message: QuickGridMessage<'item>)
                (getItems: GridItemsProviderRequest<'item> -> Async<GridItemsProviderResult<'item>>)
                (ofSuccess: GridItemsProviderResult<'item> -> 'msg)
                (ofError: exn -> 'msg)
                : Cmd<'msg> =
            match message with
            | GetItems (request, callback) ->
                Cmd.OfTask.either (fun request -> task {
                    let! result = getItems request
                    callback result
                    return result
                }) request ofSuccess ofError

        let perform
                (message: QuickGridMessage<'item>)
                (getItems: GridItemsProviderRequest<'item> -> Async<GridItemsProviderResult<'item>>)
                (ofSuccess: GridItemsProviderResult<'item> -> 'msg)
                : Cmd<'msg> =
            match message with
            | GetItems (request, callback) ->
                Cmd.OfTask.perform (fun request -> task {
                    let! result = getItems request
                    callback result
                    return result
                }) request ofSuccess

        let attempt
                (message: QuickGridMessage<'item>)
                (getItems: GridItemsProviderRequest<'item> -> Async<GridItemsProviderResult<'item>>)
                (ofError: exn -> 'msg)
                : Cmd<'msg> =
            match message with
            | GetItems (request, callback) ->
                Cmd.OfTask.attempt (fun request -> task {
                    let! result = getItems request
                    callback result
                }) request ofError

        let run
                (message: QuickGridMessage<'item>)
                (getItems: GridItemsProviderRequest<'item> -> Async<GridItemsProviderResult<'item>>)
                : Cmd<'msg> =
            match message with
            | GetItems (request, callback) ->
                [ fun _ -> Async.Start (async { let! x = getItems request in callback x }) ]
