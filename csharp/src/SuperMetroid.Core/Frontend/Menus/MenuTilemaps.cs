namespace SuperMetroid.Core.Frontend.Menus;

/// <summary>
/// The tilemaps a menu screen shows (#1358): BG1, the room map's two-screen area map over BG1's screen and the one
/// after it, BG2, and the world map's area background. The screen owns their words; its capture places them.
/// </summary>
internal sealed class MenuTilemaps
{
    /// <summary>Words in one 32x32 screen.</summary>
    internal const int ScreenWords = SnesPpuLayout.TilemapPageWordCount;

    /// <summary>The four screens, in their order: BG1, the area map's second screen, BG2, the area background.</summary>
    internal readonly ushort[] words = new ushort[4 * ScreenWords];

    /// <summary>The menu BG1 screen.</summary>
    internal Span<ushort> Bg1 => words.AsSpan(0, ScreenWords);

    /// <summary>The room map's 64x32 area map: BG1's screen and the one after it.</summary>
    internal Span<ushort> AreaMap => words.AsSpan(0, 2 * ScreenWords);

    /// <summary>The menu BG2 screen.</summary>
    internal Span<ushort> Bg2 => words.AsSpan(2 * ScreenWords, ScreenWords);

    /// <summary>The world map's area-background screen.</summary>
    internal Span<ushort> AreaBackground => words.AsSpan(3 * ScreenWords, ScreenWords);

    /// <summary>Loads little-endian tilemap words into <paramref name="destination"/>.</summary>
    internal static void Load(Span<ushort> destination, ReadOnlySpan<byte> littleEndianWords)
    {
        Ensure.LengthEqual(littleEndianWords, destination.Length * sizeof(ushort));
        for (int word = 0; word < destination.Length; word++)
            destination[word] = SnesLittleEndian.Word(littleEndianWords[word * 2], littleEndianWords[word * 2 + 1]);
    }
}
