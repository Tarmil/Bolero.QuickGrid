﻿namespace Microsoft.AspNetCore.Components.QuickGrid

open System
open System.Linq
open System.Linq.Expressions
open Bolero
open Bolero.Html
open Microsoft.AspNetCore.Components.QuickGrid
open Microsoft.AspNetCore.Components.Web.Virtualization

module QuickGrid =
    
    /// <summary>
    /// Component builder for a QuickGrid using a queryable source of data for the grid.
    /// </summary>
    let withItems<'item> (items: seq<'item>) =
        "Items" => items.AsQueryable()
        |> Builders.ComponentWithAttrsBuilder<QuickGrid<'item>>

    /// <summary>
    /// Component builder for a QuickGrid using a callback that supplies data for the grid.
    /// </summary>
    let inline withItemsProvider<'item> (provider: GridItemsProvider<'item>) =
        "ItemsProvider" => provider
        |> Builders.ComponentWithAttrsBuilder<QuickGrid<'item>>

    /// <summary>
    /// An optional CSS class name. If given, this will be included in the class attribute of the rendered table.
    /// </summary>
    let class' (value: string) =
        "Class" => value
        
    /// <summary>
    /// A theme name, with default value "default". This affects which styling rules match the table.
    /// </summary>
    let theme (value: string) =
        "Theme" => value

    /// <summary>
    /// If true, the grid will be rendered with virtualization. This is normally used in conjunction with
    /// scrolling and causes the grid to fetch and render only the data around the current scroll viewport.
    /// This can greatly improve the performance when scrolling through large data sets.
    ///
    /// If you use <see cref="virtualize"/>, you should supply a value for <see cref="itemSize"/> and must
    /// ensure that every row renders with the same constant height.
    ///
    /// Generally it's preferable not to use <see cref="virtualize"/> if the amount of data being rendered
    /// is small or if you are using pagination.
    /// </summary>
    let virtualize (value: bool) =
        "Virtualize" => value
        
    /// <summary>
    /// This is applicable only when using <see cref="virtualize"/>. It defines an expected height in pixels for
    /// each row, allowing the virtualization mechanism to fetch the correct number of items to match the display
    /// size and to ensure accurate scrolling.
    /// </summary>
    let itemSize (value: float32) =
        "ItemSize" => value
        
    /// <summary>
    /// If true, renders draggable handles around the column headers, allowing the user to resize the columns
    /// manually. Size changes are not persisted.
    /// </summary>
    let resizableColumns (value: bool) =
        "ResizableColumns" => value

    /// <summary>
    /// Optionally defines a value for @key on each rendered row. Typically this should be used to specify a
    /// unique identifier, such as a primary key value, for each data item.
    ///
    /// This allows the grid to preserve the association between row elements and data items based on their
    /// unique identifiers, even when the <see cref="item"/> instances are replaced by new copies (for
    /// example, after a new query against the underlying data store).
    ///
    /// If not set, the @key will be the <see cref="item"/> instance itself.
    /// </summary>
    let itemKey (value: 'item -> obj) =
        "ItemKey" => Func<_, _>(value)

    /// <summary>
    /// Optionally links this <see cref="QuickGrid{item}"/> instance with a <see cref="PaginationState"/> model,
    /// causing the grid to fetch and render only the current page of data.
    ///
    /// This is normally used in conjunction with a <see cref="Paginator"/> component or some other UI logic
    /// that displays and updates the supplied <see cref="PaginationState"/> instance.
    /// </summary>
    let pagination (value: PaginationState) =
        "Pagination" => value
    
    type Column =

        /// <summary>
        /// Represents a <see cref="QuickGrid{item}"/> column whose cells display a single value.
        /// </summary>
        /// <typeparam name="item">The type of data represented by each row in the grid.</typeparam>
        /// <typeparam name="prop">The type of the value being displayed in the column's cells.</typeparam>
        /// <param name="property">
        /// Defines the value to be displayed in this column's cells.
        /// </param>
        static member property<'item, 'prop>(property: Expression<Func<'item, 'prop>>) =
            "Property" => property
            |> Builders.ComponentWithAttrsBuilder<PropertyColumn<'item, 'prop>>

    module Column =

        /// <summary>
        /// Title text for the column. This is rendered automatically if <see cref="HeaderTemplate" /> is not used.
        /// </summary>
        let title (value: string) =
            "Title" => value

        /// <summary>
        /// An optional CSS class name. If specified, this is included in the class attribute of table header and body cells
        /// for this column.
        /// </summary>
        let class' (value: string) =
            "Class" => value

        /// <summary>
        /// If specified, controls the justification of table header and body cells for this column.
        /// </summary>
        let align (value: Align) =
            "Align" => value

        /// <summary>
        /// An optional template for this column's header cell. If not specified, the default header template
        /// includes the <see cref="Title" /> along with any applicable sort indicators and options buttons.
        /// </summary>
        let headerTemplate (value: ColumnBase<'item> -> Node) =
            attr.fragmentWith "HeaderTemplate" value

        /// <summary>
        /// If specified, indicates that this column has this associated options UI. A button to display this
        /// UI will be included in the header cell by default.
        ///
        /// If <see cref="HeaderTemplate" /> is used, it is left up to that template to render any relevant
        /// "show options" UI and invoke the grid's <see cref="QuickGrid{item}.ShowColumnOptions(ColumnBase{item})" />).
        /// </summary>
        let columnOptions (value: Node) =
            attr.fragment "ColumnOptions" value

        /// <summary>
        /// Indicates whether the data should be sortable by this column.
        ///
        /// The default value may vary according to the column type (for example, a <see cref="TemplateColumn{item}" />
        /// is sortable by default if any <see cref="TemplateColumn{item}.SortBy" /> parameter is specified).
        /// </summary>
        let sortable (value: bool) =
            "Sortable" => value

        /// <summary>
        /// If specified and not null, indicates that this column represents the initial sort order
        /// for the grid. The supplied value controls the default sort direction.
        /// </summary>
        let isDefaultSort (value: SortDirection) =
            "IsDefaultSort" => value

        /// <summary>
        /// If specified, virtualized grids will use this template to render cells whose data has not yet been loaded.
        /// </summary>
        let placeholderTemplate (value: PlaceholderContext -> Node) =
            attr.fragmentWith "PlaceholderTemplate" value
            
        /// <summary>
        /// Optionally specifies a format string for the value.
        ///
        /// Using this requires the <typeparamref name="prop"/> type to implement <see cref="IFormattable" />.
        /// </summary>
        let inline format (value: string) =
            "Format" => value

        /// <summary>
        /// Represents a <see cref="QuickGrid{item}"/> column whose cells are based on a template.
        /// </summary>
        let template<'item> (template: 'item -> Node) =
            attr.fragmentWith "ChildContent" template
            |> Builders.ComponentWithAttrsBuilder<TemplateColumn<'item>>

    module Paginator =
        
        /// <summary>
        /// A component that provides a user interface for <see cref="PaginationState"/>.
        /// </summary>
        /// <param name="value">
        /// Specifies the associated <see cref="PaginationState"/>.
        /// This should be a field in a containing component, and also passed to <see cref="QuickGrid.Pagination" />.
        /// </param>
        let withState (value: PaginationState) =
            "Value" => value
            |> Builders.ComponentWithAttrsBuilder<Paginator>

        /// <summary>
        /// Optionally supplies a template for rendering the page count summary.
        /// </summary>
        let summaryTemplate (value: Node) =
            attr.fragment "SummaryTemplate" value
