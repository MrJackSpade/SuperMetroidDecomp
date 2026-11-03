using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Progression;
using SuperMetroid.AssetExtraction.Video;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>
/// The complete colours each map and menu screen loads (#1464, #1512), as whole 256-colour images in the cartridge's
/// colour order: the pause screen's, the file-select screens' and each Zebes area's world map's. Only the import reads
/// them: it pre-colours the screens' sheets from them and reads each screen's backdrop from them; the game draws none.
/// </summary>
public sealed class MapStaticPalettes
{
    /// <summary>The colour of an image that the screen shows where nothing is drawn.</summary>
    private const int BackdropColour = 0;

    /// <summary>The pause screen's colours.</summary>
    private readonly RgbColor[] pause;
    /// <summary>The file-select screens' colours.</summary>
    private readonly RgbColor[] fileSelect;
    /// <summary>Six Zebes area maps' colours keyed by area identity.</summary>
    private readonly Dictionary<AreaId, RgbColor[]> world;

    /// <summary>Takes ownership of validated complete map colours.</summary>
    /// <param name="pause">The pause screen's 256 colours.</param>
    /// <param name="fileSelect">The file-select screens' 256 colours.</param>
    /// <param name="world">Each Zebes area map's 256 colours.</param>
    /// <exception cref="ArgumentException">An image does not hold 256 colours.</exception>
    public MapStaticPalettes(RgbColor[] pause, RgbColor[] fileSelect, Dictionary<AreaId, RgbColor[]> world)
    {
        this.pause = Whole(pause);
        this.fileSelect = Whole(fileSelect);
        this.world = Ensure.NotNull(world);
        foreach (RgbColor[] image in world.Values)
            _ = Whole(image);
    }

    /// <summary>The pause screen's colours.</summary>
    public ReadOnlySpan<RgbColor> Pause => pause;

    /// <summary>The file-select screens' colours.</summary>
    public ReadOnlySpan<RgbColor> FileSelect => fileSelect;

    /// <summary>The world-map colours for one of the six Zebes areas.</summary>
    /// <param name="selectedArea">Zebes area whose selected-map colours are requested.</param>
    /// <exception cref="ArgumentOutOfRangeException">The area is not one of the six Zebes areas.</exception>
    public ReadOnlySpan<RgbColor> World(AreaId selectedArea) => world.TryGetValue(selectedArea, out RgbColor[]? colours)
        ? colours : throw new ArgumentOutOfRangeException(nameof(selectedArea), "World map palettes cover the six Zebes areas.");

    /// <summary>The solid colour each screen shows where nothing is drawn, as the game publishes it (#1512).</summary>
    public MenuBackdrops Backdrops() => new(pause[BackdropColour], fileSelect[BackdropColour],
        world.ToDictionary(pair => pair.Key, pair => pair.Value[BackdropColour]));

    /// <summary>Requires a whole 256-colour image.</summary>
    /// <exception cref="ArgumentException">The image does not hold 256 colours.</exception>
    private static RgbColor[] Whole(RgbColor[] image)
    {
        Ensure.NotNull(image);
        _ = Ensure.LengthEqual<RgbColor>(image, SnesCgram.ColorCount);
        return image;
    }
}
