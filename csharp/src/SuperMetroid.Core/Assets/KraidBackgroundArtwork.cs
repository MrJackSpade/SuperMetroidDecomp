namespace SuperMetroid.Core.Assets;

/// <summary>
/// Kraid's two visual BG2 source maps. These are tile references, not the boss's
/// attack, collision, or growth instructions; the engine composes the working map.
/// </summary>
public sealed class KraidBackgroundArtwork
{
    private readonly Dictionary<ushort, KraidHeadTilemapAtlas> heads;

    public KraidBackgroundArtwork(RoomBackgroundTilemapAtlas upper,
        RoomBackgroundTilemapAtlas lower,
        IReadOnlyDictionary<ushort, KraidHeadTilemapAtlas> heads)
    {
        Upper = upper;
        Lower = lower;
        this.heads = new Dictionary<ushort, KraidHeadTilemapAtlas>(heads);
    }

    public RoomBackgroundTilemapAtlas Upper { get; }
    public RoomBackgroundTilemapAtlas Lower { get; }

    /// <summary>Resolves one cartridge-selected frame without changing its timing or hitboxes.</summary>
    public ReadOnlySpan<ushort> HeadWords(ushort sourcePointer) =>
        heads.TryGetValue(sourcePointer, out KraidHeadTilemapAtlas? frame)
            ? frame.Words.Span
            : throw new InvalidDataException(
                $"Kraid head frame $A7:{sourcePointer:X4} has no installed tilemap.");
}

/// <summary>Stable installed filenames for Kraid's private BG2 source maps.</summary>
public static class KraidBackgroundArtworkFormat
{
    public const string UpperFileName = "kraid-upper-bg2.json";
    public const string LowerFileName = "kraid-lower-bg2.json";

    public static string HeadFileName(ushort sourcePointer) =>
        $"kraid-head-{sourcePointer:X4}.json";
}
