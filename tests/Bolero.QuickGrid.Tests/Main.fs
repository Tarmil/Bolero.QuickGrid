namespace Bolero.QuickGrid.Tests

open System
open System.Runtime.CompilerServices
open Elmish
open Microsoft.AspNetCore.Components
open Microsoft.AspNetCore.Components.QuickGrid
open Bolero
open Bolero.Html

module Main =

    type Item =
        { name: string
          value: int }

    let items =
        Array.append
            [| { name = "Bolero"; value = 1 }
               { name = "Bolero.Build"; value = 2 }
               { name = "Bolero.Server"; value = 3 }
               { name = "Bolero.Templates"; value = 4 }
               { name = "Bolero.HotReload"; value = 5 } |]
            [| for i in 6..100 do { name = $"Item {i}"; value = i } |]

    let getItems (request: GridItemsProviderRequest<Item>) =
        async {
            let count = min (request.Count.GetValueOrDefault()) (items.Length - request.StartIndex)
            printfn $"REQUESTED {request.StartIndex} | {request.Count} | {count}"
            do! Async.Sleep(TimeSpan.FromSeconds 1)
            let segment = ArraySegment(items, request.StartIndex, count)
            return GridItemsProviderResult.From(segment, items.Length)
        }

    type Model =
        { pagination: PaginationState }

    type Msg =
        | GridMsg of QuickGridMessage<Item>
        | Exn of exn

    let init _ =
        { pagination = PaginationState(ItemsPerPage = 5) },
        Cmd.none

    let update message model =
        match message with
        | GridMsg msg ->
            model, Cmd.QuickGrid.run msg getItems
        | Exn exn ->
            eprintfn $"{exn}"
            model, Cmd.none

    let view model dispatch =
        div {
            QuickGrid.withElmishItemsProvider (dispatch << GridMsg) {
                QuickGrid.pagination model.pagination
                QuickGrid.Column.property (fun item -> item.name) {
                    QuickGrid.Column.sortable true
                }
                QuickGrid.Column.template<Item> (fun (item: Item) ->
                    span {
                        attr.``class`` "value-cell"
                        $"{item.value}"
                    }
                ) {
                    QuickGrid.Column.title "<blank>"
                    QuickGrid.Column.sortable true
                }
                QuickGrid.Column.property (fun item -> item.value) {
                    QuickGrid.Column.sortable true
                    QuickGrid.Column.title "index"
                }
            }
            QuickGrid.Paginator.withState model.pagination {
                QuickGrid.Paginator.summaryTemplate (concat {
                    $"Total: {model.pagination.TotalItemCount}"
                })
            }
        }

open Main

type TestGrid() =
    inherit ProgramComponent<Model, Msg>()

    override _.CssScope = CssScopes.TestGrid

    override _.Program =
        Program.mkProgram init update view