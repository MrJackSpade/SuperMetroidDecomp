using System.Collections.Immutable;
using SuperMetroid.Core.Hud.Artwork;

namespace SuperMetroid.Core.Hud;

/// <summary>
/// What the HUD's minimap shows (#1985): the area map's cells around Samus's, each place named by its offset in map cells
/// from her cell, two columns either side of it and one row above and below.
/// </summary>
public sealed class MinimapGrid
{
    /// <summary>What each place shows, by its offset from Samus's cell.</summary>
    private readonly ImmutableDictionary<Offset16<MapTiles>, MinimapCell> cells;

    /// <summary>Keeps <paramref name="cells"/>, which hold one cell for every place.</summary>
    private MinimapGrid(ImmutableDictionary<Offset16<MapTiles>, MinimapCell> cells) => this.cells = cells;

    /// <summary>The columns either side of Samus's cell and the rows above and below it that the minimap shows.</summary>
    public static Offset16<MapTiles> Reach =>
        new((GameplayHudPresentation.MinimapSize.X - 1) / 2, (GameplayHudPresentation.MinimapSize.Y - 1) / 2);

    /// <summary>Samus's own cell, in the minimap's middle.</summary>
    public static Offset16<MapTiles> SamusCell => Offset16<MapTiles>.Zero;

    /// <summary>Every place the minimap shows, row by row from the top and from left to right along each row.</summary>
    public static ImmutableArray<Offset16<MapTiles>> Places { get; } =
    [
        .. from row in Enumerable.Range(-Reach.Y, 2 * Reach.Y + 1)
           from column in Enumerable.Range(-Reach.X, 2 * Reach.X + 1)
           select new Offset16<MapTiles>(column, row),
    ];

    /// <summary>The minimap with every place hidden, as a boss's room shows it.</summary>
    public static MinimapGrid Hidden { get; } = Read(_ => MinimapHiddenCell.Instance);

    /// <summary>What the minimap shows at <paramref name="place"/>, an offset from Samus's cell.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="place"/> lies outside the minimap.</exception>
    public MinimapCell this[Offset16<MapTiles> place] => Ensure.Listed(cells, place);

    /// <summary>The minimap square <paramref name="place"/> shows in, counted from the minimap's upper-left square.</summary>
    /// <param name="place">The place, an offset from Samus's cell.</param>
    public static Position16<MapTiles> Square(Offset16<MapTiles> place) => new(place.X + Reach.X, place.Y + Reach.Y);

    /// <summary>
    /// Reads what each place shows, in <see cref="Places"/> order: an update that changes the map while it reads, as the
    /// slope above Samus's cell does, changes what the later places read.
    /// </summary>
    /// <param name="read">What the minimap shows at a place.</param>
    public static MinimapGrid Read(Func<Offset16<MapTiles>, MinimapCell> read)
    {
        Ensure.NotNull(read);
        var builder = ImmutableDictionary.CreateBuilder<Offset16<MapTiles>, MinimapCell>();
        foreach (Offset16<MapTiles> place in Places)
            builder.Add(place, Ensure.NotNull(read(place)));
        return new(builder.ToImmutable());
    }

    /// <summary>The minimap with Samus's cell showing <paramref name="cell"/> instead.</summary>
    public MinimapGrid WithSamusCell(MinimapCell cell) => new(cells.SetItem(SamusCell, Ensure.NotNull(cell)));
}
