namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// Phantoon's presentation as the display latch captured it: the wave of his body (#1353, #1468), whether his body
/// shows, hidden or translucent (#1545), and his body's pixelation. Later passes cannot change them. His fades show
/// through the body palette the frame's colour snapshot carries, which is exact palette state.
/// </summary>
/// <param name="Wave">The active wave rippling his body, or null when no wave is active.</param>
/// <param name="BodyVisibility">Whether his body shows as the room shows it, hidden or translucent.</param>
/// <param name="MosaicBlockSize">His body's pixelation block size in screen pixels latched for display, 1..16; one means none.</param>
public sealed record PhantoonDisplaySnapshot(
    PhantoonWave? Wave,
    PhantoonBodyVisibility BodyVisibility,
    int MosaicBlockSize)
{
    /// <summary>Copies the boss's display-latched wave, visibility and pixelation.</summary>
    public static PhantoonDisplaySnapshot Capture(PhantoonEnemyState phantoon)
    {
        Ensure.NotNull(phantoon);
        return new(phantoon.BodyWave.DisplayedWave, phantoon.BodyVisibility.DisplayedVisibility, phantoon.BodyVisibility.DisplayedMosaicBlockSize);
    }
}
