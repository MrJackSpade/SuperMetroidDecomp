using System.Collections.Concurrent;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend;
using SuperMetroid.AssetExtraction.Frontend.Ending;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.TestSupport.RepositoryInstallation.Verification;

/// <summary>
/// Verification access (#1392): binds a fixture's ending artwork as the game binds an installation's, with the ending
/// sheets that artwork composes over the installed palettes and font, registered so OAM comparisons still apply to them.
/// </summary>
internal static class EndingSheetFixtures
{
    /// <summary>The installed ending artwork's identity, whose sheets are the installed ones.</summary>
    private static readonly Lazy<string> stockIdentity = new(() => RepositoryInstallation.Installation.LoadEndingObjectArt().ContentIdentity);

    /// <summary>The installed ending palettes the composed sheets are coloured with.</summary>
    private static readonly Lazy<EndingPaletteSet> palettes = new(() => RepositoryInstallation.Installation.LoadEndingPalettes());

    /// <summary>The ending's background picture sheets its object artwork composes (#1392, #1467).</summary>
    private static readonly SpriteSheetId[] EndingPictureSheets = [SpriteSheetId.EndingPostCreditsPictures,
        SpriteSheetId.EndingWavySamusPicture, SpriteSheetId.EndingExplosionPictures];

    /// <summary>Installed sheets with the ending sheets already composed, by ending artwork identity.</summary>
    private static readonly ConcurrentDictionary<string, InstalledSpriteSheets> composed = new();

    /// <summary>The installed sheets with the ending sheets <paramref name="objects"/> composes.</summary>
    internal static InstalledSpriteSheets WithEnding(EndingObjectArtwork objects)
    {
        ArgumentNullException.ThrowIfNull(objects);
        string identity = objects.ContentIdentity;
        return identity == stockIdentity.Value ? InstalledFixtures.SpriteSheets : composed.GetOrAdd(identity, key =>
        {
            var sheets = SpriteSheetDefinitions.Sheets.ToDictionary(id => id, InstalledFixtures.SpriteSheets.Get);
            foreach (SpriteSheetId sheet in EndingSpriteSheetsConstants.Sheets)
                sheets[sheet] = CinematicSheetFixtures.Load(EndingSpriteSheetComposer.Sources(sheet, objects, palettes.Value, InstalledFixtures.Maps.EndingFont),
                    $"{SpriteSheetDefinitions.FileNames[sheet]}-fixture-{key[..8]}");
            // The ending's background pictures are pre-coloured from these objects (#1392, #1467).
            var tileSources = new ScreenTileSources(() => InstalledFixtures.Maps, () => InstalledFixtures.IntroArtwork, () => objects, () => palettes.Value,
                RepositoryInstallation.Installation.LoadEndingMode7Art);
            foreach (SpriteSheetId sheet in EndingPictureSheets)
            {
                ComposedSpriteSheet tiles = EndingPictureSheetComposer.Compose(sheet, tileSources);
                sheets[sheet] = SpriteSheet.Load(new MemoryStream(SpriteSheet.Write(tiles.Manifest), writable: false),
                    $"{SpriteSheetDefinitions.FileNames[sheet]}-fixture-{key[..8]}", tiles.Size, tiles.Pixels, tiles.PaletteIndices, tiles.Priorities, tiles.PartGroups);
            }
            return new InstalledSpriteSheets(sheets);
        });
    }

    /// <summary>The installed sheets with the ending Mode 7 sheets edited <paramref name="mode7"/> art composes (#1392).</summary>
    internal static InstalledSpriteSheets WithEndingMode7(EndingFloorArtwork mode7)
    {
        ArgumentNullException.ThrowIfNull(mode7);
        var sheets = SpriteSheetDefinitions.Sheets.ToDictionary(id => id, InstalledFixtures.SpriteSheets.Get);
        var tileSources = new ScreenTileSources(() => InstalledFixtures.Maps, () => InstalledFixtures.IntroArtwork,
            RepositoryInstallation.Installation.LoadEndingObjectArt, () => palettes.Value, () => mode7);
        foreach (SpriteSheetId sheet in new[] { SpriteSheetId.EndingEscapeAFloor, SpriteSheetId.EndingEscapeBFloor,
                     SpriteSheetId.EndingPlanetExplosionFloor, SpriteSheetId.EndingRewardIconFloor })
        {
            ComposedSpriteSheet tiles = FrontendFloorSheetComposer.Compose(sheet, tileSources);
            sheets[sheet] = SpriteSheet.Load(new MemoryStream(SpriteSheet.Write(tiles.Manifest), writable: false),
                $"{SpriteSheetDefinitions.FileNames[sheet]}-fixture", tiles.Size, tiles.Pixels, tiles.PaletteIndices, tiles.Priorities, tiles.PartGroups);
        }
        return new InstalledSpriteSheets(sheets);
    }

    /// <summary>
    /// Draws <paramref name="family"/> from the sheet <paramref name="ending"/> draws <paramref name="objectSheet"/>'s page
    /// from now, as its own frames draw it.
    /// </summary>
    internal static CinematicSpriteSheet Drawer(EndingCreditsState ending, EndingObjectSheet objectSheet, CinematicSpriteFamily family)
    {
        ArgumentNullException.ThrowIfNull(ending);
        return new CinematicSpriteSheet(ending.InstalledArtwork.SpriteSheet(EndingSpriteSheets.For(objectSheet, ending.Stage.EndingReward,
            ending.screen.LogoChunksShown)), family);
    }

    extension(EndingArtwork self)
    {
        /// <summary>Binds <paramref name="objects"/> and the ending sheets it composes, as the game rebinds both.</summary>
        internal void BindFixtureObjectArtwork(EndingObjectArtwork objects)
        {
            self.BindObjectArtwork(objects);
            self.BindSpriteSheets(WithEnding(objects));
        }
    }
}
