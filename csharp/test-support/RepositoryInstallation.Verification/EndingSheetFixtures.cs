using SuperMetroid.AssetExtraction.Frontend.Map;
using System.Collections.Concurrent;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend;
using SuperMetroid.AssetExtraction.Frontend.Ending;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

namespace SuperMetroid.TestSupport.RepositoryInstallation.Verification;

/// <summary>
/// Verification access (#1392): binds the ending sheets a fixture's ending artwork composes over the installed palettes and
/// font, as the game binds an installation's sheets, registered so OAM comparisons still apply to them.
/// </summary>
internal static class EndingSheetFixtures
{
    /// <summary>The installed ending artwork's identity, whose sheets are the installed ones.</summary>
    private static readonly Lazy<string> stockIdentity = new(() => RepositoryInstallation.Installation.LoadEndingObjectArt().ContentIdentity);

    /// <summary>The ending's decoded colours the composed sheets are coloured with (#1544).</summary>
    private static readonly Lazy<EndingPaletteSet> palettes = new(() => InstalledFixtures.EndingColours);

    /// <summary>The ending's background picture sheets its object artwork composes (#1392, #1467).</summary>
    private static readonly SpriteSheetId[] EndingPictureSheets = [SpriteSheetId.EndingPostCreditsPictures,
        SpriteSheetId.EndingWavySamusPicture, SpriteSheetId.EndingExplosionPictures];

    /// <summary>Installed sheets with the ending sheets already composed, by ending artwork identity.</summary>
    private static readonly ConcurrentDictionary<string, InstalledSpriteSheets> composed = new();

    /// <summary>The installed sheets with the ending sheets <paramref name="objects"/> composes.</summary>
    internal static InstalledSpriteSheets WithEnding(ImportEndingObjectArtwork objects)
    {
        ArgumentNullException.ThrowIfNull(objects);
        string identity = objects.ContentIdentity;
        return identity == stockIdentity.Value ? InstalledFixtures.SpriteSheets
            : composed.GetOrAdd(identity, key => Compose(objects, palettes.Value, InstalledFixtures.ImportMaps, key[..8]));
    }

    /// <summary>
    /// The installed sheets with the ending sheets <paramref name="objects"/> composes over <paramref name="colours"/> and
    /// <paramref name="maps"/>, named by <paramref name="tag"/>: its sprite and picture sheets.
    /// </summary>
    private static InstalledSpriteSheets Compose(ImportEndingObjectArtwork objects, EndingPaletteSet colours, ImportAreaMapPresentation maps, string tag)
    {
        var sheets = SpriteSheetDefinitions.Sheets.ToDictionary(id => id, InstalledFixtures.SpriteSheets.Get);
        foreach (SpriteSheetId sheet in EndingSpriteSheetsConstants.Sheets)
            sheets[sheet] = CinematicSheetFixtures.Load(EndingSpriteSheetComposer.Sources(sheet, objects, colours, maps.EndingFont, maps.RoomPaletteFx),
                $"{SpriteSheetDefinitions.FileNames[sheet]}-fixture-{tag}");
        // The ending's background pictures are pre-coloured from these objects, colours and maps (#1392, #1467).
        var tileSources = new ScreenTileSources(() => maps, () => InstalledFixtures.IntroArtwork, () => objects, () => colours,
            () => InstalledFixtures.EndingFloors);
        foreach (SpriteSheetId sheet in EndingPictureSheets)
            sheets[sheet] = Loaded(sheet, EndingPictureSheetComposer.Compose(sheet, tileSources), tag);
        return new InstalledSpriteSheets(sheets);
    }

    /// <summary>Loads composed tile sheet <paramref name="tiles"/> as sheet <paramref name="sheet"/>, named by <paramref name="tag"/>.</summary>
    private static SpriteSheet Loaded(SpriteSheetId sheet, ComposedSpriteSheet tiles, string tag) =>
        SpriteSheetFileConstants.Load(tiles.Manifest,
            $"{SpriteSheetDefinitions.FileNames[sheet]}-fixture-{tag}", tiles.Size, tiles.Pixels, tiles.PaletteIndices);

    /// <summary>
    /// Draws <paramref name="family"/> from the sheet <paramref name="ending"/> draws <paramref name="objectSheet"/>'s page
    /// from now, as its own frames draw it.
    /// </summary>
    internal static CinematicSpriteSheet Drawer(EndingCreditsState ending, EndingObjectSheet objectSheet, CinematicSpriteFamily family)
    {
        ArgumentNullException.ThrowIfNull(ending);
        return new CinematicSpriteSheet(ending.InstalledArtwork.SpriteSheet(EndingSpriteSheets.For(objectSheet, ending.Stage.EndingReward)), family);
    }

    extension(EndingArtwork self)
    {
        /// <summary>Binds the ending sheets <paramref name="objects"/> composes, as the game binds an installation's sheets.</summary>
        internal void BindFixtureObjectArtwork(ImportEndingObjectArtwork objects) => self.BindSpriteSheets(WithEnding(objects));
    }
}
