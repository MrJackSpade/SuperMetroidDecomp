namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// Phantoon's presentation as the display latch captured it: the wave of his body (#1353, #1468) and where his body lies,
/// which the wave ripples about, whether his body shows, hidden or translucent (#1545), and his body's pixelation. Later
/// passes cannot change them. His fades show through the body colours the frame's colour snapshot carries, which are exact.
/// </summary>
/// <param name="Wave">The active wave rippling his body, or null when no wave is active.</param>
/// <param name="BodyPlacement">Where his body is placed, the room pixel each of its frames lies against and his wave ripples about (#2031).</param>
/// <param name="BodyVisibility">Whether his body shows as the room shows it, hidden or translucent.</param>
/// <param name="PixelationBlockSize">His body's pixelation block size in screen pixels latched for display, 1..16; one means none.</param>
public sealed record PhantoonDisplaySnapshot(
    PhantoonWave? Wave,
    Position16<RoomPixels> BodyPlacement,
    PhantoonBodyVisibility BodyVisibility,
    int PixelationBlockSize)
{
    /// <summary>Copies the boss's display-latched wave, his body's placement, visibility and pixelation.</summary>
    public static PhantoonDisplaySnapshot Capture(PhantoonEnemyState phantoon)
    {
        Ensure.NotNull(phantoon);
        return new(phantoon.BodyWave.DisplayedWave, phantoon.Body.Position.Whole, phantoon.BodyVisibility.DisplayedVisibility,
            phantoon.BodyVisibility.DisplayedPixelationBlockSize);
    }
}
