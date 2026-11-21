using System.Text.Json;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Title.Artwork;
using SuperMetroid.AssetExtraction.Fx.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Compatibility;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>
/// Installs the scene colours (#1544) in the map presentation: the solid colour the title and each menu screen shows
/// where nothing is drawn, and the Ceres haze's tints, as one JSON document a player edits in place. The import takes each
/// backdrop from the first colour of the screen's own colours when it extracts them; the game reads these colours and no
/// palette document, as every picture it draws is pre-coloured.
/// </summary>
public static class SceneColourFiles
{
    /// <summary>The installed document's file name.</summary>
    public const string FileName = "scene-colours.json";

    /// <summary>
    /// Revision 3 of the scene colour document, the only revision the reader accepts: 2 names the backdrop the menus share
    /// for the menus, not for file select (#2203), and 3 writes each colour as <c>#rrggbb</c> (#2564).
    /// </summary>
    public const int Version = 3;

    /// <summary>The stock tint the Ceres haze fades in to while Ceres Ridley lives.</summary>
    private static RgbColor StockCeresHazeBlue => StockCeresHaze(CeresHazeColor.Blue);

    /// <summary>The stock tint the Ceres haze fades in to once Ceres Ridley is dead.</summary>
    private static RgbColor StockCeresHazeRed => StockCeresHaze(CeresHazeColor.Red);

    /// <summary>
    /// The stock tint the Ceres haze of <paramref name="color"/> fades in to: its channel at the last amplitude the fade
    /// writes before it stops. The import records it in the scene colours; the game reads only the installed tint (#1596).
    /// </summary>
    private static RgbColor StockCeresHaze(CeresHazeColor color)
    {
        const int finalAmplitude = CeresHazeDefinitions.FadeSteps - 1;
        return color switch
        {
            CeresHazeColor.Blue => new FadeLevelColour(0, 0, finalAmplitude).ToColour(),
            CeresHazeColor.Red => new FadeLevelColour(finalAmplitude, 0, 0).ToColour(),
            _ => throw new ArgumentOutOfRangeException(nameof(color), color, "The Ceres haze is blue or red."),
        };
    }

    /// <summary>The scene colours the screens' extracted colours show, as the document's bytes.</summary>
    /// <param name="title">The title's extracted colours, whose first colour is its backdrop.</param>
    /// <param name="menus">The menu screens' extracted colours, whose first colours are their backdrops.</param>
    internal static byte[] Extract(TitlePalettePresentation title, MapStaticPalettes menus)
    {
        Ensure.NotNull(title);
        MenuBackdrops backdrops = Ensure.NotNull(menus).Backdrops();
        return JsonSerializer.SerializeToUtf8Bytes(new SceneColourDocument
        {
            Version = Version,
            TitleBackdrop = InstalledColourText.Format(title.Backdrop),
            PauseBackdrop = InstalledColourText.Format(backdrops.Pause),
            MenusBackdrop = InstalledColourText.Format(backdrops.Menus),
            WorldMapBackdrops = ZebesAreas().ToDictionary(area => AreaIdKeys.Installed.Of(area), area => InstalledColourText.Format(backdrops.World(area))),
            CeresHazeWhileRidleyLives = InstalledColourText.Format(StockCeresHazeBlue),
            CeresHazeAfterRidleyDies = InstalledColourText.Format(StockCeresHazeRed),
        }, ArtworkJson.Options);
    }

    /// <summary>Reads the installed scene colours.</summary>
    /// <param name="json">The installed document, read from its current position.</param>
    /// <exception cref="InvalidDataException">
    /// The document is invalid or names another version, a Zebes area's world-map backdrop is missing or another area is
    /// named, or a colour is missing, is not written as <c>#rrggbb</c> or lies off the game's fade scale.
    /// </exception>
    public static SceneColours Read(Stream json)
    {
        SceneColourDocument document = JsonAssetDocument.Read<SceneColourDocument>(json, ArtworkJson.Options, "scene colours");
        if (document.Version != Version)
            throw new InvalidDataException($"Scene colours require version {Version}.");
        AreaId[] areas = [.. ZebesAreas()];
        if (document.WorldMapBackdrops is null || document.WorldMapBackdrops.Count != areas.Length)
            throw new InvalidDataException("Scene colours require the world map's backdrop for each of the six Zebes areas.");
        var world = new Dictionary<AreaId, RgbColor>();
        foreach (AreaId area in areas)
        {
            if (!document.WorldMapBackdrops.TryGetValue(AreaIdKeys.Installed.Of(area), out string? backdrop))
                throw new InvalidDataException($"Scene colours are missing the world map's backdrop for {area}.");
            world.Add(area, Colour(backdrop, $"world map's {area} backdrop"));
        }
        return new SceneColours(Colour(document.TitleBackdrop, "title backdrop"),
            new MenuBackdrops(Colour(document.PauseBackdrop, "pause backdrop"), Colour(document.MenusBackdrop, "menus' backdrop"), world),
            new CeresHazeTints(Colour(document.CeresHazeWhileRidleyLives, "Ceres haze while Ridley lives"),
                Colour(document.CeresHazeAfterRidleyDies, "Ceres haze once Ridley is dead")));

        static RgbColor Colour(string? colour, string name) => InstalledColourText.ParseOnFadeScale(colour, $"Scene colours' {name}");
    }

    /// <summary>The six Zebes areas, whose world maps each have a backdrop.</summary>
    private static IEnumerable<AreaId> ZebesAreas() => AreaIds.Zebes;
}
