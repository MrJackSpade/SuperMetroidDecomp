using System.Collections.Immutable;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The Wrecked Ship ghost's own colours, which <see cref="WreckedShipGhostFade"/> fades it toward (#1544, #2550). Evidence:
/// the original game's fade routines step all sixteen of the ghost's colours toward these.
/// </summary>
[CompatibilityService("the Wrecked Ship ghost's own colours, which its fades step all sixteen of toward")]
internal static class WreckedShipGhostFadeDefinitions
{
    /// <summary>
    /// The ghost's own sixteen colours, colour zero first, which its materializing fade moves toward (#1544). Colour zero,
    /// a blue, is never drawn, but the fade moves it with the others, so it takes part in deciding when the fade ends. The
    /// next eight are olive paint whose red and green share a level, with a blue seven lower except the highlight, the
    /// first of them, and the middle tint, the sixth; the last seven are warm paint: orange, three pure reds, ochre with its
    /// faint blue, and two brown bug shades whose green is three below their red. The ghost's pictures show its installed
    /// shades.
    /// </summary>
    internal static ImmutableArray<RgbColor> GhostColours { get; } =
    [
        new(0, 0, 115),
        new(255, 255, 173), new(189, 189, 132), new(74, 74, 16), new(41, 41, 0),
        new(214, 214, 156), new(173, 173, 107), new(132, 132, 74), new(115, 115, 57),
        new(255, 115, 0), new(255, 0, 0), new(198, 0, 0), new(82, 0, 0),
        new(206, 173, 8), new(82, 57, 0), new(41, 16, 0),
    ];
}
