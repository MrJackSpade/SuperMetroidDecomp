namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// Phantoon's presentation as the display latch captured it: the wave on his body layer (#1353, #1468), the blend
/// configuration, and the body layer's mosaic size. Later passes cannot change them. His fades show through the body
/// palette the frame's colour snapshot carries, which is exact palette state.
/// </summary>
/// <param name="Wave">The active wave rippling the background, or null when the wave channel is inactive.</param>
/// <param name="Configuration">Latched body blend: shown as the room shows it, hidden, or translucent.</param>
/// <param name="MosaicBlockSize">Background mosaic block size in screen pixels latched for display, 1..16; one means no quantization.</param>
public sealed record PhantoonDisplaySnapshot(
    PhantoonWave? Wave,
    PhantoonBodyBlend Configuration,
    int MosaicBlockSize)
{
    /// <summary>Copies the boss's display-latched wave and blend state.</summary>
    public static PhantoonDisplaySnapshot Capture(PhantoonEnemyState phantoon)
    {
        Ensure.NotNull(phantoon);
        return new(phantoon.Wave.DisplayedWave, phantoon.Blending.DisplayedConfiguration, phantoon.Blending.DisplayedMosaicBlockSize);
    }
}
