using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Verification.Harness;

/// <summary>Where a screen palette's rows lie among the colours a frame publishes (#1404).</summary>
internal static class ScreenColourPositions
{
    /// <summary>The position of colour <paramref name="colour"/> of sprite row <paramref name="row"/>.</summary>
    internal static int Sprite(int row, int colour = 0) => new ScreenPalette().SpriteRow(row).First + colour;
}
