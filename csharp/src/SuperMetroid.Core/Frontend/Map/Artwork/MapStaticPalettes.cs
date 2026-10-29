using SuperMetroid.Core.Progression;

using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Frontend.Map.Artwork;

/// <summary>
/// Named complete presentation colours by screen owner (#1464), which the import pipeline decodes; no native offsets,
/// palette programs or selection rules.
/// </summary>
public sealed class MapStaticPalettes
{
    /// <summary>Six Zebes area maps' screen colours keyed by area identity.</summary>
    private readonly Dictionary<AreaId, ScreenColours> world;
    /// <summary>Takes ownership of validated complete map colours.</summary>
    /// <param name="pause">The pause screen's colours.</param>
    /// <param name="fileSelect">The file-select screens' colours.</param>
    /// <param name="world">Each Zebes area map's colours.</param>
    internal MapStaticPalettes(ScreenColours pause, ScreenColours fileSelect, Dictionary<AreaId, ScreenColours> world)
    {
        Pause = Ensure.NotNull(pause);
        FileSelect = Ensure.NotNull(fileSelect);
        this.world = Ensure.NotNull(world);
    }

    /// <summary>The pause screen's colours.</summary>
    public ScreenColours Pause { get; }

    /// <summary>The file-select screens' colours.</summary>
    public ScreenColours FileSelect { get; }

    /// <summary>The world-map colours for one of the six Zebes areas.</summary>
    /// <param name="selectedArea">Zebes area whose selected-map colours are requested.</param>
    public ScreenColours World(AreaId selectedArea) => world.TryGetValue(selectedArea, out ScreenColours? colours)
        ? colours : throw new ArgumentOutOfRangeException(nameof(selectedArea), "World map palettes cover the six Zebes areas.");
}
