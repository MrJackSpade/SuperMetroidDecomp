using SuperMetroid.AssetExtraction.Areas.Ceres;
using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Areas.Escape;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend.GameOver.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Options.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Escape.Artwork;
using SuperMetroid.AssetExtraction.Frontend.FileSelect.Artwork;
using SuperMetroid.Core.Frontend.Map;
using SuperMetroid.AssetExtraction.Frontend;
using SuperMetroid.AssetExtraction.Frontend.Ending;
using SuperMetroid.AssetExtraction.Frontend.Intro;
using SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Frontend.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.Frontend.Pause;
using SuperMetroid.AssetExtraction.Hud;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.Title;

namespace SuperMetroid.AssetExtraction.Installation;

/// <summary>
/// Installs the pre-coloured sprite sheets (#1392, #1635): for each sheet its frame manifest, with a provenance manifest of
/// their hashes, its pictures held in the installation's picture store (<see cref="PictureStoreFiles"/>). A player edits the
/// installed files in place.
/// </summary>
public static class SpriteSheetFiles
{
    /// <summary>The provenance manifest's file name.</summary>
    public const string ManifestFileName = "sprite-sheets.json";

    /// <summary>
    /// Composes every sheet as the installation holds it, in the sheets' own order, for the picture store to write
    /// (<see cref="PictureStoreFiles"/>, #1635).
    /// </summary>
    /// <param name="bus">Cartridge source for frames no installed artwork holds and for the ending's colours (#1544).</param>
    /// <param name="content">Installed stock content holding the characters, compositions and colours the sheets compose from.</param>
    /// <param name="directory">The family's directory, which the picture store writes the sheet manifests into.</param>
    internal static InstalledSheetFamily Extract(ISnesAddressSpace bus, string content, string directory)
    {
        Ensure.NotNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(content);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        // The ending's colours and the escape timer's sheet read the cartridge bus, so they are read on this thread; every
        // other sheet composes from the installed sources and those colours in parallel (#1530), each into its own place,
        // so the order stays the sheets' own.
        var sources = new SpriteSheetSourceCatalog(content, EndingPaletteImport.Read(bus));
        IReadOnlyList<SpriteSheetId> sheets = SpriteSheetDefinitions.Sheets;
        var composed = new ComposedSpriteSheet[sheets.Count];
        Parallel.For(0, sheets.Count, index =>
        {
            if (sheets[index] != SpriteSheetId.EscapeTimer)
                composed[index] = sources.Compose(sheets[index]);
        });
        for (int index = 0; index < sheets.Count; index++)
        {
            if (sheets[index] == SpriteSheetId.EscapeTimer)
                composed[index] = EscapeTimerSheetComposer.Compose(bus);
        }
        return InstalledSheetFamily.Install(directory, ManifestFileName,
            [.. sheets.Select((sheet, index) => new NamedComposedSheet(SpriteSheetDefinitions.FileNames[sheet], composed[index]))]);
    }

    /// <summary>Checks the installed files against their manifest and loads every sheet, its pictures from the picture store.</summary>
    /// <exception cref="InvalidDataException">The manifest, a frame manifest or the picture store is invalid.</exception>
    public static InstalledSpriteSheets Load(string directory)
    {
        SpriteSheetStore store = SpriteSheetStore.Open(directory, ManifestFileName, SpriteSheetDefinitions.Sheets.Count);
        var sheets = new Dictionary<SpriteSheetId, SpriteSheet>();
        // The body sheet is named from the bodies Samus can show with the body installed beside it (#1513).
        string content = Path.GetDirectoryName(Path.GetFullPath(directory))
            ?? throw new ArgumentException($"Directory {directory} has no installation above it.", nameof(directory));
        var body = new Lazy<IReadOnlyList<string>>(() => SamusBodySheetComposer.FrameNames(content));
        // The pause sheet is named from the stock selector presentation installed beside it.
        var maps = new Lazy<InstalledAreaMapPresentation>(() => MapPresentationFiles.Load(
            Path.Combine(content, GameInstallationLayout.MapDirectoryName)));
        // The intro sheet is named from the stock intro compositions installed beside it.
        var intro = new Lazy<IntroCinematicSourceArtwork>(() => IntroCinematicArtworkFiles.Load(
            Path.Combine(content, GameInstallationLayout.IntroCinematicDirectoryName)));
        // The ending sheets are named from the stock ending compositions installed beside them.
        var ending = new Lazy<EndingObjectSourceArtwork>(() => EndingObjectArtworkFiles.Load(
            Path.Combine(content, GameInstallationLayout.EndingObjectDirectoryName)));
        // The projectile sheets are named from the frame bindings installed beside them.
        var projectiles = new Lazy<InstalledProjectileFrameBindings>(() => ProjectileFrameBindingFile.LoadInstalled(
            Path.Combine(content, GameInstallationLayout.ProjectileDirectoryName)));
        foreach (SpriteSheetId sheet in SpriteSheetDefinitions.Sheets)
        {
            SpriteSheet loaded = store.Read(SpriteSheetDefinitions.FileNames[sheet]);
            if (!loaded.HoldsExactly(FrameNames(sheet, body, maps, intro, ending, projectiles)))
                throw new InvalidDataException($"Sprite sheet {FrameFile(sheet)} does not hold exactly the frames its family draws.");
            sheets.Add(sheet, loaded);
        }
        var installed = new InstalledSpriteSheets(sheets);
        // Every frontend floor picture is the whole floor (#1467), except the title's, which is one whole floor with each
        // pose of the Baby Metroid drawn over it (#1538).
        foreach (SpriteSheetId sheet in FrontendFloorSheetComposer.Sheets)
        {
            if (sheet == SpriteSheetId.TitleFloor)
            {
                foreach (TitleBabyPose pose in Enum.GetValues<TitleBabyPose>())
                    _ = TitleFloors.Of(installed, pose);
                continue;
            }
            foreach (string frame in FrameNames(sheet, body, maps, intro, ending, projectiles))
                _ = FloorShots.Of(installed, sheet, frame);
        }
        return installed;
    }


    /// <summary>The frames a sheet must hold: the ones its family draws.</summary>
    /// <param name="sheet">The sheet.</param>
    /// <param name="body">The body sheet's frames, the bodies Samus can show with the installed body.</param>
    /// <param name="maps">The menus' presentation, whose selector frames name the pause sheet's.</param>
    /// <param name="intro">The opening cinematic's artwork, whose compositions name the intro sheet's frames.</param>
    /// <param name="ending">The ending's sprite artwork, whose compositions name the ending sheets' frames.</param>
    /// <param name="projectiles">The projectile frame bindings, whose compositions name the projectile sheets' frames.</param>
    internal static IEnumerable<string> FrameNames(SpriteSheetId sheet, Lazy<IReadOnlyList<string>> body,
        Lazy<InstalledAreaMapPresentation> maps, Lazy<IntroCinematicSourceArtwork> intro, Lazy<EndingObjectSourceArtwork> ending,
        Lazy<InstalledProjectileFrameBindings> projectiles) => sheet switch
    {
        SpriteSheetId.IntroScenePictures => IntroScenePictureSheetComposer.FrameNames,
        SpriteSheetId.IntroNarrationPictures => IntroNarrationPictureSheetComposer.FrameNames,
        SpriteSheetId.CeresCinematicPictures => CeresCinematicPictureSheetComposer.FrameNames,
        SpriteSheetId.EndingCreditsRows or SpriteSheetId.EndingPostCreditsPictures or
            SpriteSheetId.EndingWavySamusPicture or SpriteSheetId.EndingExplosionPictures =>
            EndingPictureSheetComposer.FrameNames(sheet),
        SpriteSheetId.TitleFloor => Enum.GetValues<TitleBabyPose>().Select(TitleFloors.BabyFrameName).Prepend(TitleFloors.FloorFrameName),
        SpriteSheetId.CeresFloor => Enum.GetValues<CeresFloorShot>().Select(CeresFloors.FrameName),
        SpriteSheetId.EndingEscapeAFloor or SpriteSheetId.EndingEscapeBFloor or
            SpriteSheetId.EndingPlanetExplosionFloor or SpriteSheetId.EndingFlyawayFloor or
            SpriteSheetId.EndingRewardIconFloor =>
            [EndingFloorDefinitions.FrameName],
        SpriteSheetId.EscapeTimer => EscapeTimerLayoutDefinitions.Glyphs.Select(EscapeTimerComposition.FrameName),
        SpriteSheetId.Grapple => GrappleSheetComposer.Frames,
        SpriteSheetId.SamusBody => body.Value,
        SpriteSheetId.SamusOverlays => SamusOverlaySheetComposer.Frames,
        SpriteSheetId.SamusAtmosphere => SamusAtmosphereSheetComposer.Frames,
        SpriteSheetId.SamusDeath => SamusDeathSheetComposer.Frames,
        SpriteSheetId.GameOver => GameOverPresentationFormat.SpriteNames.ToArray(),
        SpriteSheetId.FileSelect => FileSelectPresentationFormat.SheetFrames,
        SpriteSheetId.GameOptions => GameOptionsPresentationFormat.SheetFrames,
        SpriteSheetId.MapSprites => MapSpriteArtwork.SheetFrames,
        SpriteSheetId.PauseSprites => PauseSheetComposer.FrameNames(maps.Value),
        SpriteSheetId.PauseHud => HudImageSheet.PauseFrameNames(maps.Value.Game),
        SpriteSheetId.MenuPictures => MenuPictureSheet.FrameNames(maps.Value),
        SpriteSheetId.RoomMapPictures => MapPictureSheets.RoomMapFrameNames(maps.Value),
        SpriteSheetId.PausePictures => PausePictureSheet.FrameNames(maps.Value),
        SpriteSheetId.AreaMapForegroundCrateria or SpriteSheetId.AreaMapForegroundBrinstar or
            SpriteSheetId.AreaMapForegroundNorfair or SpriteSheetId.AreaMapForegroundWreckedShip or
            SpriteSheetId.AreaMapForegroundMaridia or SpriteSheetId.AreaMapForegroundTourian => [AreaMapPictures.WorldForegroundFrame],
        SpriteSheetId.AreaMapBackgroundCrateria or SpriteSheetId.AreaMapBackgroundBrinstar or
            SpriteSheetId.AreaMapBackgroundNorfair or SpriteSheetId.AreaMapBackgroundWreckedShip or
            SpriteSheetId.AreaMapBackgroundMaridia or SpriteSheetId.AreaMapBackgroundTourian => [AreaMapPictures.WorldBackgroundFrame],
        SpriteSheetId.Title => TitleSheetComposer.FrameNames(),
        SpriteSheetId.IntroSprites => IntroSpriteSheetComposer.FrameNames(intro.Value),
        SpriteSheetId.CeresFlightSprites => IntroSpriteSheetComposer.CeresFlightFrameNames(intro.Value.CeresFlight),
        SpriteSheetId.CeresDestructionSprites => IntroSpriteSheetComposer.CeresDestructionFrameNames(intro.Value),
        SpriteSheetId.ZebesRevealSprites => IntroSpriteSheetComposer.ZebesRevealFrameNames(intro.Value),
        SpriteSheetId.EndingCloudSprites or SpriteSheetId.EndingExplosionSprites or
            SpriteSheetId.EndingSuitedRewardSprites or SpriteSheetId.EndingSuitlessRewardSprites or
            SpriteSheetId.EndingShootingSprites =>
            EndingSpriteSheetComposer.FrameNames(sheet, ending.Value),
        SpriteSheetId.IntroProjectiles or SpriteSheetId.ProjectilesPower or SpriteSheetId.ProjectilesWave or
            SpriteSheetId.ProjectilesIce or SpriteSheetId.ProjectilesIceWave or SpriteSheetId.ProjectilesSpazer or
            SpriteSheetId.ProjectilesSpazerWave or SpriteSheetId.ProjectilesSpazerIce or
            SpriteSheetId.ProjectilesSpazerIceWave or SpriteSheetId.ProjectilesPlasma or
            SpriteSheetId.ProjectilesPlasmaWave or SpriteSheetId.ProjectilesPlasmaIce or
            SpriteSheetId.ProjectilesPlasmaIceWave or SpriteSheetId.ProjectilesSpazerPlasmaWave or
            SpriteSheetId.ProjectilesSpazerPlasmaIce =>
            ProjectileSheetComposer.Frames(sheet, projectiles.Value),
        _ => throw new ArgumentOutOfRangeException(nameof(sheet), sheet, "Undefined sprite sheet."),
    };

    /// <summary>A sheet's frame manifest file name.</summary>
    internal static string FrameFile(SpriteSheetId sheet) => SpriteSheetStore.FrameFile(SpriteSheetDefinitions.FileNames[sheet]);
}
