namespace SuperMetroid.Core.Fx;

/// <summary>
/// The room effect's part of the BG3 plane a gameplay frame shows (#1348, #1466): 32 columns by 64 rows, every cell the
/// cleared picture, with the picture of the effect's page from row 31 down. The HUD's images cover its top four rows: the
/// frame's plane takes those rows' pictures from the HUD, so the HUD band, liquids (the whole plane) and atmospheres (its
/// lower 32 rows) all read one plane, and nothing is written into VRAM for them.
/// </summary>
public sealed class Layer3Plane
{
    /// <summary>Tile columns; the plane repeats every 256 pixels across.</summary>
    public const int Columns = 32;

    /// <summary>Tile rows.</summary>
    public const int Rows = 64;

    /// <summary>The row the effect's page begins on.</summary>
    public const int EffectPageFirstRow = 31;

    /// <summary>
    /// The name of the picture, in the room's BG3 picture sheet, of the cell the plane is cleared to wherever no effect's
    /// page shows.
    /// </summary>
    public const string ClearedPicture = "Cleared";

    /// <summary>The plane of a room whose effect shows no page: every cell the cleared picture.</summary>
    public static Layer3Plane Cleared { get; } = new(null);

    /// <summary>The plane showing each effect's page, shared so a frame showing the same page reuses its resolved pictures.</summary>
    private static readonly Dictionary<RoomFxType, Layer3Plane> Pages =
        Enum.GetValues<RoomFxType>().ToDictionary(type => type, type => new Layer3Plane(type));

    /// <summary>Takes the effect whose page the plane shows.</summary>
    private Layer3Plane(RoomFxType? effectPage) => EffectPage = effectPage;

    /// <summary>The plane showing the page of <paramref name="effectPage"/>, or <see cref="Cleared"/> for none.</summary>
    /// <param name="effectPage">The effect whose page the plane shows from row <see cref="EffectPageFirstRow"/>, or null.</param>
    public static Layer3Plane Of(RoomFxType? effectPage) => effectPage is { } type ? Pages[type] : Cleared;

    /// <summary>The effect whose page the plane shows from row <see cref="EffectPageFirstRow"/>; null when it shows none.</summary>
    public RoomFxType? EffectPage { get; }

    /// <summary>The name of the picture, in the room's BG3 picture sheet, of the page room effect <paramref name="type"/> shows.</summary>
    public static string EffectPagePicture(RoomFxType type) => $"Fx{type}";
}
