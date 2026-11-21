using SuperMetroid.Core.Saves;

namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>
/// A row of the copy-destination menu (#2553): one of the two routes from the copy's source, or Exit below them. The menu of
/// each source is built once and its rows know their neighbours, so the cursor never rests on the source's own slot.
/// </summary>
internal abstract class FileCopyDestinationRow
{
    /// <summary>A row of <paramref name="source"/>'s menu, <paramref name="index"/> rows from its top.</summary>
    private FileCopyDestinationRow(SaveSlot source, int index)
    {
        Source = source;
        Index = index;
    }

    /// <summary>The slot the copy is from.</summary>
    internal SaveSlot Source { get; }

    /// <summary>The file select row the cursor shows on.</summary>
    internal abstract FileSelectItem Item { get; }

    /// <summary>How many rows above this one the menu has.</summary>
    private int Index { get; }

    /// <summary>The row above, or this one at the top.</summary>
    internal FileCopyDestinationRow Above => Index == 0 ? this : MenuFrom(Source)[Index - 1];

    /// <summary>The row below, or this one at the bottom.</summary>
    internal FileCopyDestinationRow Below => Index == MenuFrom(Source).Count - 1 ? this : MenuFrom(Source)[Index + 1];

    /// <summary>The top row of <paramref name="source"/>'s menu: the first slot other than the source.</summary>
    internal static FileCopyDestinationRow FirstFrom(SaveSlot source) => MenuFrom(source)[0];

    /// <summary>The row that chooses <paramref name="route"/>.</summary>
    internal static FileCopyDestinationRow Choosing(FileCopyRoute route) => RowsChoosing[route];

    /// <summary>The rows of <paramref name="source"/>'s menu, from the top.</summary>
    private static IReadOnlyList<FileCopyDestinationRow> MenuFrom(SaveSlot source) => Menus[source];

    /// <summary>Each source's menu: its two routes, then Exit.</summary>
    private static IdentityTable<SaveSlot, IReadOnlyList<FileCopyDestinationRow>> Menus => field ??= new(source =>
        [.. FileCopyRoutes.From(source).Select((route, index) => (FileCopyDestinationRow)new Destination(route, index)),
            new Exit(source, FileCopyRoutes.From(source).Count)]);

    /// <summary>The row choosing each route.</summary>
    private static IdentityTable<FileCopyRoute, FileCopyDestinationRow> RowsChoosing => field ??= new(
        SaveSlots.All.SelectMany(MenuFrom).OfType<Destination>().ToDictionary(row => row.Route, row => (FileCopyDestinationRow)row));

    /// <summary>A destination slot: choosing it copies along <paramref name="route"/>.</summary>
    /// <param name="route">The route chosen.</param>
    /// <param name="index">How many rows above it the menu has.</param>
    internal sealed class Destination(FileCopyRoute route, int index) : FileCopyDestinationRow(FileCopyRoutes.SourceOf(route), index)
    {
        /// <summary>The route chosen.</summary>
        internal FileCopyRoute Route { get; } = route;

        /// <inheritdoc/>
        internal override FileSelectItem Item => FileSelectItemRows.ForSaveSlot(FileCopyRoutes.DestinationOf(Route));
    }

    /// <summary>Exit, which leaves the copy without one.</summary>
    /// <param name="source">The slot the copy is from.</param>
    /// <param name="index">How many rows above it the menu has.</param>
    internal sealed class Exit(SaveSlot source, int index) : FileCopyDestinationRow(source, index)
    {
        /// <inheritdoc/>
        internal override FileSelectItem Item => FileSelectItem.Exit;
    }
}
