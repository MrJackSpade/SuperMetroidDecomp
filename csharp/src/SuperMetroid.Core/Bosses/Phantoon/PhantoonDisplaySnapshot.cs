using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// Phantoon's presentation as an accepted NMI latched it: the wave on BG2 (#1353, #1468), the blend configuration, and
/// the BG2 mosaic size. Later passes cannot change them. His fades show through the body palette the frame's CGRAM
/// snapshot carries, which is exact palette state.
/// </summary>
/// <param name="Wave">The active wave rippling BG2, or null when the wave channel is inactive.</param>
/// <param name="Configuration">Latched blend configuration: room default, BG2 hidden, or BG2 semitransparent.</param>
/// <param name="MosaicBlockSize">BG2 mosaic block size in screen pixels accepted by the NMI, 1..16; one means no quantization.</param>
public sealed record PhantoonDisplaySnapshot(
    PhantoonWave? Wave,
    LayerBlendingConfiguration Configuration,
    int MosaicBlockSize)
{
    /// <summary>Copies the boss's NMI-latched wave and blend state.</summary>
    public static PhantoonDisplaySnapshot Capture(PhantoonEnemyState phantoon)
    {
        Ensure.NotNull(phantoon);
        return new(phantoon.Wave.DisplayedWave, phantoon.Blending.DisplayedConfiguration, phantoon.Blending.DisplayedMosaicBlockSize);
    }
}
