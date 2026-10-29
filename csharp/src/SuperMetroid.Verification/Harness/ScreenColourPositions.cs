using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core;

namespace SuperMetroid.Verification.Harness;

/// <summary>
/// Where a frontend screen's colour groups lie in the cartridge's 256-colour order (#1404, #1464): the native boundary at
/// which checks compare a screen's colours with native snapshots and recorded colour memory. A group's owner set and
/// colour decode to the image position the import pipeline reads them from.
/// </summary>
internal static class ScreenColourPositions
{
    /// <summary>The native position of colour <paramref name="colour"/> of <paramref name="group"/>.</summary>
    internal static int Of(ScreenColourGroup group, int colour = 0)
    {
        ScreenColourPlacement placement = ScreenColourGroupDefinitions.Groups[group];
        Ensure.BetweenInclusive(0, placement.ColourCount - 1, colour);
        int ownerColour = placement.Set * ColourSets.SetColours + placement.Colour + colour;
        return placement switch
        {
            ScreenBackgroundSetPlacement => ScreenColourImage.BackgroundPosition(ownerColour),
            ScreenSpriteSetPlacement => ScreenColourImage.SpritePosition(ownerColour),
            _ => throw new ArgumentOutOfRangeException(nameof(group), group, "Undefined screen colour owner."),
        };
    }
}
