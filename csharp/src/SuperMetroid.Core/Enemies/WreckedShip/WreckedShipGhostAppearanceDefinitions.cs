using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Enemies.WreckedShip;

/// <summary>Appearance geometry, colours and visibility timing for Wrecked Ship ghost appearances.</summary>
public static class WreckedShipGhostAppearanceDefinitions
{
    /// <summary>
    /// The ghost's sixteen colours, which its materializing fade moves toward (#1544). The first, a blue, is never drawn,
    /// but the fade moves it with the others, so it takes part in deciding when the fade ends (WreckedShipGhostFade).
    /// The next eight are olive paint whose red and green share a categorical level, with a blue seven lower except the
    /// highlight, the first of them, and the middle tint, the sixth. The last seven are warm paint: orange, three pure reds, ochre
    /// with its faint blue, and two brown bug shades whose green is three below their red. These are chosen artwork
    /// content, retained as authored; colours no picture draws are still faded toward.
    /// </summary>
    internal static readonly RgbColor[] OwnColours =
    [
        new(0, 0, 115),
        new(255, 255, 173), new(189, 189, 132), new(74, 74, 16), new(41, 41, 0),
        new(214, 214, 156), new(173, 173, 107), new(132, 132, 74), new(115, 115, 57),
        new(255, 115, 0), new(255, 0, 0), new(198, 0, 0), new(82, 0, 0),
        new(206, 173, 8), new(82, 57, 0), new(41, 16, 0),
    ];

    /// <summary>
    /// Where the ghost appears around Samus for each way she has been moving: 64 pixels the way she moved on each
    /// axis, and level with her on an axis she held still. A Samus moving up and right is the exception: the ghost
    /// appears level with her, to her right.
    /// </summary>
    internal static readonly IReadOnlyDictionary<SamusMotion, Offset16<RoomPixels>> SpawnOffsets =
        new Dictionary<SamusMotion, Offset16<RoomPixels>>
        {
            [new(SamusAxisMotion.TowardSmaller, SamusAxisMotion.TowardSmaller)] = new(-64, -64),
            [new(SamusAxisMotion.Still, SamusAxisMotion.TowardSmaller)] = new(0, -64),
            [new(SamusAxisMotion.TowardLarger, SamusAxisMotion.TowardSmaller)] = new(64, 0),
            [new(SamusAxisMotion.TowardSmaller, SamusAxisMotion.Still)] = new(-64, 0),
            [new(SamusAxisMotion.Still, SamusAxisMotion.Still)] = new(0, 0),
            [new(SamusAxisMotion.TowardLarger, SamusAxisMotion.Still)] = new(64, 0),
            [new(SamusAxisMotion.TowardSmaller, SamusAxisMotion.TowardLarger)] = new(-64, 64),
            [new(SamusAxisMotion.Still, SamusAxisMotion.TowardLarger)] = new(0, 64),
            [new(SamusAxisMotion.TowardLarger, SamusAxisMotion.TowardLarger)] = new(64, 64),
        };

    /// <summary>
    /// The steps of a ghost's flicker as it appears, in order; flickering ends when the last step does, leaving the ghost
    /// visible. The hidden steps shorten from eight updates to five and the shown steps lengthen from one to three, each
    /// length held for two hidden and two shown steps; the ghost starts hidden. Once a step has shown the ghost, the hidden
    /// steps that follow leave it shown, so the steps' lengths decide when the ghost first appears and when its flicker
    /// ends, which the fade's completion waits for.
    /// </summary>
    public static IReadOnlyList<WreckedShipGhostFlickerStep> FlickerSteps { get; } =
    [
        new(8, WreckedShipGhostFlickerVisibility.Unchanged), new(1, WreckedShipGhostFlickerVisibility.Shown),
        new(8, WreckedShipGhostFlickerVisibility.Unchanged), new(1, WreckedShipGhostFlickerVisibility.Shown),
        new(7, WreckedShipGhostFlickerVisibility.Unchanged), new(1, WreckedShipGhostFlickerVisibility.Shown),
        new(7, WreckedShipGhostFlickerVisibility.Unchanged), new(2, WreckedShipGhostFlickerVisibility.Shown),
        new(6, WreckedShipGhostFlickerVisibility.Unchanged), new(2, WreckedShipGhostFlickerVisibility.Shown),
        new(6, WreckedShipGhostFlickerVisibility.Unchanged), new(3, WreckedShipGhostFlickerVisibility.Shown),
        new(5, WreckedShipGhostFlickerVisibility.Unchanged), new(3, WreckedShipGhostFlickerVisibility.Shown),
        new(5, WreckedShipGhostFlickerVisibility.Unchanged),
    ];
}
