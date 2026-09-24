namespace SuperMetroid.Core.Assets;

/// <summary>
/// Kraid's two visual BG2 source maps. These are tile references, not the boss's
/// attack, collision, or growth instructions; the engine composes the working map.
/// </summary>
public sealed record KraidBackgroundArtwork(
    RoomBackgroundTilemapAtlas Upper,
    RoomBackgroundTilemapAtlas Lower);

/// <summary>Stable installed filenames for Kraid's private BG2 source maps.</summary>
public static class KraidBackgroundArtworkFormat
{
    public const string UpperFileName = "kraid-upper-bg2.json";
    public const string LowerFileName = "kraid-lower-bg2.json";
}
