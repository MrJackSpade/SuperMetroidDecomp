namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// What a room's explicit scroll layout writes over the kept scroll cells past the room's own screens (#2356). Loading an
/// explicit layout fills every kept cell, so a room with fewer screens than the game keeps cells fills the cells after
/// its own with what follows its screens in the game's data. The camera reads those cells beside the room's bottom row
/// and right column, and a door setup or a scroll trigger may set a row past the room's own and leave the rest as the
/// load wrote them, so whether the camera keeps following Samus past a room's edge depends on them. An implicit layout
/// writes only the room's own screens and has none.
/// </summary>
/// <remarks>
/// Consulted as a room's scroll grid loads, right after its own screens are written (<see cref="ScrollCellsKeptAcrossRooms{TCell, TArea}.LoadAll"/>).
/// Lifetime: the installation's, shared by every room load of the session. Evidence: the original explicit scroll loader
/// (<c>$82:E878</c>) copies fifty bytes from the room state's scroll table whatever the room's size, so a table holding
/// only the room's screens is followed by the next table or other data in bank <c>$8F</c>, each byte read as the camera
/// reads a cell; the import decodes those bytes once for every explicit layout, and the room scroll definitions check
/// compares each layout's screens followed by these cells with the fifty bytes of every retail table.
/// </remarks>
/// <typeparam name="TCell">A scroll cell's state.</typeparam>
[CompatibilityService("a room's explicit scroll layout fills the kept cells past its own screens with what follows them in the game's data")]
public sealed partial class ScrollCellsPastRoomScreens<TCell>
{
    /// <summary>The cells each explicit layout's load writes past the room's screens, by the layout's name.</summary>
    private readonly IReadOnlyDictionary<string, IReadOnlyList<TCell>> _byLayout;

    /// <summary>Keeps the decoded cells of every explicit layout.</summary>
    /// <param name="byLayout">The cells each explicit layout's load writes past the room's screens, in order, by the layout's name.</param>
    public ScrollCellsPastRoomScreens(IReadOnlyDictionary<string, IReadOnlyList<TCell>> byLayout) =>
        _byLayout = Ensure.NotNull(byLayout);

    /// <summary>The cells the explicit layout named <paramref name="layout"/> writes past the room's screens, in order.</summary>
    /// <exception cref="ArgumentOutOfRangeException">No explicit layout has the name.</exception>
    public IReadOnlyList<TCell> After(string layout) => Ensure.Listed(_byLayout, layout);
}
