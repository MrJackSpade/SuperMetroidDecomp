using System.Collections.Immutable;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Phantoon's presentation as an accepted NMI latched it: the wavy BG2 scroll data, the blend
/// configuration, and the MOSAIC shadow. Owned copies; later HDMA passes cannot change them.
/// </summary>
/// <param name="Scrolls">Gameplay-viewport BG2 horizontal scrolls of the active wave, or null when the wave channel is inactive.</param>
/// <param name="Configuration">Latched blend configuration: room default, BG2 hidden, or BG2 semitransparent.</param>
/// <param name="Mosaic">Raw <c>$2106</c> shadow byte accepted by the NMI.</param>
public sealed record PhantoonDisplaySnapshot(
    ImmutableArray<ushort>? Scrolls,
    LayerBlendingConfiguration Configuration,
    byte Mosaic)
{
    /// <summary>Copies the boss's NMI-latched wave and blend state.</summary>
    public static PhantoonDisplaySnapshot Capture(PhantoonEnemyState phantoon)
    {
        Ensure.NotNull(phantoon);
        ushort[]? scrolls = phantoon.Wave.DisplayedScrolls;
        return new(scrolls is null ? null : ImmutableArray.Create(scrolls),
            phantoon.Blending.DisplayedConfiguration, phantoon.Blending.DisplayedMosaic);
    }
}
