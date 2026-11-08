using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Enemies.WreckedShip;

/// <summary>Appearance geometry, colours and visibility timing for Wrecked Ship ghost appearances.</summary>
public static class WreckedShipGhostAppearanceDefinitions
{
    /// <summary>
    /// The ghost's sixteen colours, which its materializing fade moves toward (#1544). Colour 0, a blue, is never drawn,
    /// but the fade moves it with the others, so it takes part in deciding when the fade ends (WreckedShipGhostFade).
    /// Colours 1..8 are olive paint whose red and green share a categorical level, with a blue seven lower except the
    /// highlight in colour 1 and the middle tint in colour 6. Colours 9..15 are warm paint: orange, three pure reds, ochre
    /// with its faint blue, and two brown bug shades whose green is three below their red. These are chosen artwork
    /// content, retained as authored; colours no picture draws are still faded toward.
    /// </summary>
    internal static readonly RgbColor[] OwnColours =
    [
        new(0, 0, 14),
        new(31, 31, 21), new(23, 23, 16), new(9, 9, 2), new(5, 5, 0),
        new(26, 26, 19), new(21, 21, 13), new(16, 16, 9), new(14, 14, 7),
        new(31, 14, 0), new(31, 0, 0), new(24, 0, 0), new(10, 0, 0),
        new(25, 21, 1), new(10, 7, 0), new(5, 2, 0),
    ];

    /// <summary>The spawn table contains the nine row-major positions of a three-by-three spawn grid.</summary>
    public const int SpawnCount = 9;

    /// <summary>
    /// The signed spawn offsets, i=0..8. Horizontal and vertical movement classes select columns and
    /// rows, each spaced 64 pixels around Samus. The upper-right approach spawns level with Samus: its Y word
    /// is zero, consumed directly by the position addition.
    /// </summary>
    internal static readonly Offset16<RoomPixels>[] SpawnOffsets =
    [
        new(-64, -64), new(0, -64), new(64, 0),
        new(-64, 0), new(0, 0), new(64, 0),
        new(-64, 64), new(0, 64), new(64, 64),
    ];

    /// <summary>The flicker table contains sixteen alternating intervals followed by the -1 terminator.</summary>
    public const int FlickerCount = 17;

    /// <summary>
    /// The flicker intervals, i=0..16. Each four-interval stage repeats twice: the even interval grows
    /// from max(1, stage), the odd interval falls from 8-stage. The final -1 terminates flickering.
    /// </summary>
    internal static readonly short[] FlickerDurations = [1, 8, 1, 8, 1, 7, 1, 7, 2, 6, 2, 6, 3, 5, 3, 5, -1];
}
