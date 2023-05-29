namespace Bolero.QuickGrid.Tests

open System
open Elmish
open Microsoft.AspNetCore.Components
open Microsoft.AspNetCore.Components.QuickGrid
open Microsoft.AspNetCore.Components.QuickGrid.Elmish
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

    let cursedPage = 13 // Arbitrarily fail on this page, to demonstrate how failure works

    let getItems (request: GridItemsProviderRequest<Item>) =
        async {
            let count = min (request.Count.GetValueOrDefault()) (items.Length - request.StartIndex)
            printfn $"REQUESTED {request.StartIndex} | {request.Count} | {count}"
            do! Async.Sleep(TimeSpan.FromSeconds 1)

            if request.StartIndex = count * (cursedPage - 1) then
                raise (exn "This page is cursed! 😨")

            let segment = ArraySegment(items, request.StartIndex, count)
            return GridItemsProviderResult.From(segment, items.Length)
        }

    type Model =
        { pagination: PaginationState
          gridModel: QuickGrid.Model<Item>
          error: exn option }

    type Msg =
        | GridMsg of QuickGrid.Msg<Item>
        | GotoPage of int
        | SetError of exn option

    let init _ =
        let gridModel, gridCmd = QuickGrid.initItemProvider getItems
        { pagination = PaginationState(ItemsPerPage = 5)
          gridModel = gridModel
          error = None },
        Cmd.map GridMsg gridCmd

    let update message model =
        match message with
        | GridMsg msg ->
            let gridModel, gridCmd, gridExtMsg = QuickGrid.update msg model.gridModel
            let gridExtCmd =
                match gridExtMsg with
                | QuickGrid.NoOp -> None
                | QuickGrid.Error error -> Some error
                |> SetError
                |> Cmd.ofMsg
            { model with gridModel = gridModel },
            Cmd.batch [ gridExtCmd; Cmd.map GridMsg gridCmd ]
        | GotoPage page ->
            model,
            Cmd.OfAsync.attempt (model.pagination.SetCurrentPageIndexAsync >> Async.AwaitTask) (page - 1) (Some >> SetError)
        | SetError error ->
            { model with error = error }, Cmd.none

    let paginatedGrid model dispatch =
        div {
            QuickGrid.withModel model.gridModel {
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
                    "Go to page: "
                    input { bind.change.int (model.pagination.CurrentPageIndex + 1) (dispatch << GotoPage) }
                    $" (total items: {model.pagination.TotalItemCount})"
                })
            }
            cond model.error <| function
                | None -> empty()
                | Some exn -> p { attr.``class`` "error"; exn.Message }
        }

    let virtualizedGrid model dispatch =
        QuickGrid.withModel model.gridModel {
            QuickGrid.virtualize true
            QuickGrid.itemSize 32.f
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

    let view model dispatch =
        div {
            paginatedGrid model dispatch
            div {
                attr.``class`` "virtualized-grid"
                virtualizedGrid model dispatch
            }
        }

open Main

type TestGrid() =
    inherit ProgramComponent<Model, Msg>()

    override _.CssScope = CssScopes.TestGrid

    override this.Program =
        Program.mkProgram init update view
        |> Program.withConsoleTrace