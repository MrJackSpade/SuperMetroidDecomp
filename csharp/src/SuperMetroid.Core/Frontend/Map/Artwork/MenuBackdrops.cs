using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend.Map.Artwork;

/// <summary>
/// The solid colour each menu screen shows where nothing is drawn (#1512): the pause screen's, the menus' (file select
/// with its room maps, the options screens and the game-over screen, which share one) and each Zebes area's world map's. The import reads them from each screen's colours; the screens' pictures carry the
/// rest of their colours as shades the import pre-colours.
/// </summary>
public sealed partial class MenuBackdrops
{
    /// <summary>Each Zebes area's world-map backdrop, keyed by area identity.</summary>
    private readonly Dictionary<AreaId, RgbColor> world;

    /// <summary>Takes the installed backdrops.</summary>
    /// <param name="pause">The pause screen's backdrop.</param>
    /// <param name="menus">The backdrop file select, its room maps, the options screens and the game-over screen share.</param>
    /// <param name="world">Each Zebes area's world-map backdrop.</param>
    public MenuBackdrops(RgbColor pause, RgbColor menus, Dictionary<AreaId, RgbColor> world)
    {
        Pause = pause;
        Menus = menus;
        this.world = Ensure.NotNull(world);
    }

    /// <summary>The pause screen's backdrop.</summary>
    public RgbColor Pause { get; }

    /// <summary>The backdrop file select, its room maps, the options screens and the game-over screen share (#2203).</summary>
    public RgbColor Menus { get; }

    /// <summary>The world map's backdrop while <paramref name="selectedArea"/> is selected.</summary>
    /// <param name="selectedArea">The Zebes area the world map shows selected.</param>
    /// <exception cref="ArgumentOutOfRangeException">The area is not one of the six Zebes areas.</exception>
    public RgbColor World(AreaId selectedArea) => world.TryGetValue(selectedArea, out RgbColor backdrop)
        ? backdrop : throw new ArgumentOutOfRangeException(nameof(selectedArea), "World map backdrops cover the six Zebes areas.");
}
