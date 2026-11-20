using SuperMetroid.Core.Saves;

namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>
/// The slots and arrow of each <see cref="FileCopyRoute"/> (#2553): the arrow is short between neighbouring slots and long
/// between A and C, pointing down or up toward the destination, at the height between the two slots.
/// </summary>
internal static class FileCopyRoutes
{
    /// <summary>The slot each route copies from.</summary>
    private static IdentityTable<FileCopyRoute, SaveSlot> Sources => field ??= new(new Dictionary<FileCopyRoute, SaveSlot>
    {
        [FileCopyRoute.AToB] = SaveSlot.A,
        [FileCopyRoute.AToC] = SaveSlot.A,
        [FileCopyRoute.BToA] = SaveSlot.B,
        [FileCopyRoute.BToC] = SaveSlot.B,
        [FileCopyRoute.CToA] = SaveSlot.C,
        [FileCopyRoute.CToB] = SaveSlot.C,
    });

    /// <summary>The slot each route copies onto.</summary>
    private static IdentityTable<FileCopyRoute, SaveSlot> Destinations => field ??= new(new Dictionary<FileCopyRoute, SaveSlot>
    {
        [FileCopyRoute.AToB] = SaveSlot.B,
        [FileCopyRoute.AToC] = SaveSlot.C,
        [FileCopyRoute.BToA] = SaveSlot.A,
        [FileCopyRoute.BToC] = SaveSlot.C,
        [FileCopyRoute.CToA] = SaveSlot.A,
        [FileCopyRoute.CToB] = SaveSlot.B,
    });

    /// <summary>The arrow each route shows on the copy confirmation page.</summary>
    private static IdentityTable<FileCopyRoute, FileCopyArrowPlacement> Arrows => field ??= new(new Dictionary<FileCopyRoute, FileCopyArrowPlacement>
    {
        [FileCopyRoute.AToB] = new(FileCopyArrowShape.ShortDown, FileCopyArrowDefinitions.UpperShortArrowY),
        [FileCopyRoute.AToC] = new(FileCopyArrowShape.LongDown, FileCopyArrowDefinitions.LongArrowY),
        [FileCopyRoute.BToA] = new(FileCopyArrowShape.ShortUp, FileCopyArrowDefinitions.UpperShortArrowY),
        [FileCopyRoute.BToC] = new(FileCopyArrowShape.ShortDown, FileCopyArrowDefinitions.LowerShortArrowY),
        [FileCopyRoute.CToA] = new(FileCopyArrowShape.LongUp, FileCopyArrowDefinitions.LongArrowY),
        [FileCopyRoute.CToB] = new(FileCopyArrowShape.ShortUp, FileCopyArrowDefinitions.LowerShortArrowY),
    });

    /// <summary>The two routes from each slot, in the order the copy-destination menu lists their destinations.</summary>
    private static IdentityTable<SaveSlot, IReadOnlyList<FileCopyRoute>> RoutesFrom => field ??= new(new Dictionary<SaveSlot, IReadOnlyList<FileCopyRoute>>
    {
        [SaveSlot.A] = [FileCopyRoute.AToB, FileCopyRoute.AToC],
        [SaveSlot.B] = [FileCopyRoute.BToA, FileCopyRoute.BToC],
        [SaveSlot.C] = [FileCopyRoute.CToA, FileCopyRoute.CToB],
    });

    /// <summary>The slot <paramref name="route"/> copies from.</summary>
    internal static SaveSlot SourceOf(FileCopyRoute route) => Sources[route];

    /// <summary>The slot <paramref name="route"/> copies onto.</summary>
    internal static SaveSlot DestinationOf(FileCopyRoute route) => Destinations[route];

    /// <summary>The arrow <paramref name="route"/> shows.</summary>
    internal static FileCopyArrowPlacement ArrowOf(FileCopyRoute route) => Arrows[route];

    /// <summary>The two routes from <paramref name="source"/>, in the copy-destination menu's order.</summary>
    internal static IReadOnlyList<FileCopyRoute> From(SaveSlot source) => RoutesFrom[source];
}
