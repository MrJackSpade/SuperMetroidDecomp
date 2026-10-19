using SuperMetroid.Core.Fx;
using System.Collections.Immutable;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// Phantoon's presentation as an accepted NMI latched it: the wavy BG2 scroll data, the blend
/// configuration, and the BG2 mosaic size. Owned copies; later HDMA passes cannot change them.
/// </summary>
/// <param name="Scrolls">Gameplay-viewport BG2 horizontal scrolls of the active wave, or null when the wave channel is inactive.</param>
/// <param name="Configuration">Latched blend configuration: room default, BG2 hidden, or BG2 semitransparent.</param>
/// <param name="MosaicBlockSize">BG2 mosaic block size in screen pixels accepted by the NMI, 1..16; one means no quantization.</param>
public sealed record PhantoonDisplaySnapshot(
    ImmutableArray<ushort>? Scrolls,
    LayerBlendingConfiguration Configuration,
    int MosaicBlockSize)
{
    /// <summary>Copies the boss's NMI-latched wave and blend state.</summary>
    public static PhantoonDisplaySnapshot Capture(PhantoonEnemyState phantoon)
    {
        Ensure.NotNull(phantoon);
        ushort[]? scrolls = phantoon.Wave.DisplayedScrolls;
        return new(scrolls is null ? null : ImmutableArray.Create(scrolls),
            phantoon.Blending.DisplayedConfiguration, phantoon.Blending.DisplayedMosaicBlockSize);
    }
}
