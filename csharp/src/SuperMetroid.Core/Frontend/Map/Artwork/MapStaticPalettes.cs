using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Map.Artwork;

/// <summary>Named complete presentation palettes; no native offsets, palette programs or selection rules.</summary>
public sealed class MapStaticPalettes
{
    /// <summary>Complete pause-screen CGRAM selection copied from the asset document.</summary>
    private readonly RgbColor[] pause;
    /// <summary>Complete file-select CGRAM selection copied from the asset document.</summary>
    private readonly RgbColor[] fileSelect;
    /// <summary>Six Zebes area-map CGRAM selections keyed by area identity.</summary>
    private readonly Dictionary<AreaId, RgbColor[]> world;
    /// <summary>Takes ownership of validated complete map palettes.</summary>
    internal MapStaticPalettes(RgbColor[] pause, RgbColor[] fileSelect, Dictionary<AreaId, RgbColor[]> world)
    { this.pause = pause; this.fileSelect = fileSelect; this.world = world; }

    /// <summary>Gets the complete 256-color pause-screen palette as packed SNES RGB555 words.</summary>
    public ReadOnlySpan<RgbColor> Pause => pause;

    /// <summary>Gets the complete 256-color file-select palette as packed SNES RGB555 words.</summary>
    public ReadOnlySpan<RgbColor> FileSelect => fileSelect;

    /// <summary>Gets the complete 256-color world-map palette for one of the six Zebes areas.</summary>
    /// <param name="selectedArea">Zebes area whose selected-map palette is requested.</param>
    /// <returns>The area's packed SNES RGB555 palette.</returns>
    public ReadOnlySpan<RgbColor> World(AreaId selectedArea) => world.TryGetValue(selectedArea, out var colors)
        ? colors : throw new ArgumentOutOfRangeException(nameof(selectedArea), "World map palettes cover the six Zebes areas.");
}
