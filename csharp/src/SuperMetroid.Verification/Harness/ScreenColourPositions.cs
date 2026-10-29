using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Verification.Harness;

/// <summary>Where a frontend screen's colour groups lie among the colours a frame publishes (#1404, #1464).</summary>
internal static class ScreenColourPositions
{
    /// <summary>The published position of colour <paramref name="colour"/> of <paramref name="group"/>.</summary>
    internal static int Of(ScreenColourGroup group, int colour = 0) => new ScreenPalette().Colours(group).First + colour;
}
