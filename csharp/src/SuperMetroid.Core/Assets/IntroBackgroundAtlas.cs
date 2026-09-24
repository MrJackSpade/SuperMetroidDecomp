namespace SuperMetroid.Core.Assets;

/// <summary>
/// Indexed opening-cinematic background characters, compiled to the original SNES
/// four-bit tile format. Placement, scroll and palette selection remain game logic.
/// </summary>
public sealed class IntroBackgroundAtlas
{
    private readonly RoomCharacterAtlas tiles;

    private IntroBackgroundAtlas(RoomCharacterAtlas tiles) => this.tiles = tiles;

    /// <summary>The exact bytes uploaded to cinematic background-character VRAM.</summary>
    public ReadOnlyMemory<byte> Transfer => tiles.Transfer;

    /// <summary>Loads and validates a complete palette-indexed PNG sheet.</summary>
    public static IntroBackgroundAtlas Load(Stream png) =>
        new(RoomCharacterAtlas.Load(png, IntroBackgroundAtlasFormat.NativeByteCount));
}

/// <summary>File and physical tile geometry for the opening-cinematic background sheet.</summary>
public static class IntroBackgroundAtlasFormat
{
    public const string FileName = "intro-background-characters.png";
    public const int NativeByteCount = 0x8000;
    public const int TileColumns = 32;
}
