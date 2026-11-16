namespace SuperMetroid.Core.Rendering;

/// <summary>Cartridge values and helpers of <see cref="ScrollBands"/> that only verification reads.</summary>
internal static class ScrollBandsConstants
{
    /// <summary>The lines above <paramref name="line"/> at <paramref name="scrollAbove"/>, and the lines from it down at <paramref name="scroll"/>.</summary>
    /// <param name="scrollAbove">The scroll the lines above show.</param>
    /// <param name="line">The screen line the second scroll begins on; a line outside the screen leaves one scroll on every line.</param>
    /// <param name="scroll">The scroll the lines from <paramref name="line"/> down show.</param>
    public static ScrollBands Split(int scrollAbove, int line, int scroll) =>
        new(scrollAbove, [new(Math.Clamp(line, 0, ScreenGeometry.ScreenHeightPixels), scroll)]);
}
