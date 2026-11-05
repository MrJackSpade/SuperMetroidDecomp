using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.Core.Samus.Abilities.Artwork;
using SuperMetroid.Core.Enemies.Elevator.Artwork;
using SuperMetroid.Core.Enemies.Chozo.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Audio;
using SuperMetroid.AssetExtraction.Bosses;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.AssetExtraction.Frontend.Ending;
using SuperMetroid.AssetExtraction.Frontend.Intro;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core.Bosses.Torizo.Artwork;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Bosses.Draygon.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Bosses.SporeSpawn.Artwork;
using SuperMetroid.Core.Areas.Tourian.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;
using SuperMetroid.AssetExtraction.Rooms.Artwork;

namespace SuperMetroid.AssetExtraction.Installation;

/// <summary>App-owned immutable cartridge/audio content, separate from persistent player data.</summary>
/// <param name="Root">Installation root containing replaceable <c>game</c> content and persistent <c>overrides</c>; constructing this record does not create or validate it.</param>
/// <remarks>
/// Directory properties compute paths without accessing the filesystem. Stock loaders validate
/// the installed formats and source manifests before applying compatible replacements from
/// the override tree. Missing files, unreadable paths, and malformed stock or selected override
/// content propagate the corresponding I/O or invalid-data exception to the caller.
/// </remarks>
public sealed record GameInstallation(string Root)
{
    /// <summary>Replaceable stock-content tree published by the installer beneath the installation root.</summary>
    public string ContentDirectory => Path.Combine(Root, GameInstallationLayout.ContentDirectoryName);
    /// <summary>Private validated cartridge copy used for extraction and repair; gameplay can open complete extracted content without it.</summary>
    public string RomPath => Path.Combine(ContentDirectory, GameInstallationLayout.RomFileName);
    /// <summary>
    /// Opens mutable game memory only after validating the extracted installation.
    /// Gameplay never maps a cartridge image, even when the private import copy exists.
    /// </summary>
    public SuperMetroidAddressSpace OpenRuntimeAddressSpace()
    {
        if (GameAssetInstaller.TryOpenExtractedContent(Root) is null)
            throw new InvalidDataException(
                "Extracted game content is incomplete or invalid.");
        return SuperMetroidAddressSpace.CreateWithoutCartridge();
    }
    /// <summary>Installed audio files and manifests, which the player edits in place.</summary>
    public string AudioDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.AudioDirectoryName);
    /// <summary>Loads the installed pre-rendered audio clips (#1305) the game plays.</summary>
    /// <exception cref="InvalidDataException">The installed clips are invalid.</exception>
    public InstalledClipAudioLibrary LoadAudioClips() => InstalledAudioClips.Load(AudioDirectory);
    /// <summary>Installed area-map artwork, exploration masks and menu presentation, which the player edits in place.</summary>
    public string MapDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.MapDirectoryName);
    /// <summary>Installed starting CGRAM and room-entry sprite palettes, which the player edits in place.</summary>
    public string GameplayBasePaletteDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.GameplayBasePaletteDirectoryName);
    /// <summary>Loads the installed initial CGRAM and room-entry sprite colors.</summary>
    public GameplayBasePalette LoadGameplayBasePalettes() =>
        GameplayBasePaletteFiles.Load(GameplayBasePaletteDirectory);
    /// <summary>Stock decoded results of Samus's glitch reads; not editable, so there is no override directory.</summary>
    public string SamusOverreadDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.SamusOverreadDirectoryName);
    /// <summary>Loads the validated compatibility services reproducing Samus's glitch reads.</summary>
    public SamusOverreadServices LoadSamusOverreads() => SamusOverreadFiles.Load(SamusOverreadDirectory);
    /// <summary>Stock decoded results of what enemies read past their authored data; not editable, so there is no override directory.</summary>
    public string EnemyOverreadDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.EnemyOverreadDirectoryName);
    /// <summary>Loads the validated compatibility services reproducing what enemies read past their authored data.</summary>
    public EnemyOverreadServices LoadEnemyOverreads() => EnemyOverreadFiles.Load(EnemyOverreadDirectory);
    /// <summary>Stock decoded results of Kraid's, Crocomire's and the intro egg shell's reads past their data; not editable, so there is no override directory.</summary>
    public string BossAndIntroOverreadDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.BossAndIntroOverreadDirectoryName);
    /// <summary>Loads the validated compatibility services reproducing Kraid's, Crocomire's and the intro egg shell's reads past their data.</summary>
    public SuperMetroid.Core.Compatibility.BossAndIntroOverreadServices LoadBossAndIntroOverreads() => BossAndIntroOverreadFiles.Load(BossAndIntroOverreadDirectory);
    /// <summary>Installed pre-coloured sprite sheets and their frame manifests, which the player edits in place.</summary>
    public string SpriteSheetDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.SpriteSheetDirectoryName);
    /// <summary>Loads every installed sprite sheet.</summary>
    public InstalledSpriteSheets LoadSpriteSheets() => SpriteSheetFiles.Load(SpriteSheetDirectory);
    /// <summary>Installed pre-coloured enemy sheets and the manifest naming each graphics set's sheet for each enemy.</summary>
    public string EnemySheetDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.EnemySheetDirectoryName);
    /// <summary>Loads every installed enemy sheet.</summary>
    public InstalledEnemySheets LoadEnemySheets() => EnemySheetFiles.Load(EnemySheetDirectory);
    /// <summary>Installed indexed common gameplay OBJ sheet and its source manifest, which the player edits in place.</summary>
    public string StandardObjectDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.StandardObjectDirectoryName);
    /// <summary>Validates and loads the installed common OBJ sheet for the shared character transfer.</summary>
    public RoomCharacterAtlas LoadStandardObjects() => StandardObjectArtworkFiles.Load(StandardObjectDirectory);
    /// <summary>Installed projectile artwork and presentation selectors, which the player edits in place.</summary>
    public string ProjectileDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.ProjectileDirectoryName);
    /// <summary>Installed Samus body PNGs and visual selectors, which the player edits in place.</summary>
    public string SamusBodyDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.SamusBodyDirectoryName);
    /// <summary>Loads the validated installed Samus body artwork.</summary>
    public InstalledSamusBodyArtwork LoadSamusBodyArt() => SamusBodyArtworkFiles.Load(SamusBodyDirectory);
    /// <summary>Installed opening-cinematic artwork, which the player edits in place.</summary>
    public string IntroCinematicDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.IntroCinematicDirectoryName);
    /// <summary>Loads the installed opening-cinematic artwork.</summary>
    public IntroCinematicSourceArtwork LoadIntroCinematicArt() =>
        IntroCinematicArtworkFiles.Load(IntroCinematicDirectory);
    /// <summary>Installed ending Mode 7 scene artwork, which the player edits in place.</summary>
    public string EndingMode7Directory => Path.Combine(ContentDirectory, GameInstallationLayout.EndingMode7DirectoryName);
    /// <summary>Loads the installed ending Mode 7 artwork.</summary>
    public EndingFloorArtwork LoadEndingMode7Art() =>
        EndingMode7ArtworkFiles.Load(EndingMode7Directory);
    /// <summary>Installed ending sprite artwork, which the player edits in place.</summary>
    public string EndingObjectDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.EndingObjectDirectoryName);
    /// <summary>Loads the installed ending OBJ artwork.</summary>
    public EndingObjectSourceArtwork LoadEndingObjectArt() =>
        EndingObjectArtworkFiles.Load(EndingObjectDirectory);
    /// <summary>Installed ending-scene color palettes, which the player edits in place.</summary>
    public string EndingPaletteDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.EndingPaletteDirectoryName);
    /// <summary>Loads the installed ending color palettes.</summary>
    public EndingPaletteSet LoadEndingPalettes() =>
        EndingPaletteArtworkFiles.Load(EndingPaletteDirectory);
    /// <summary>Stock RGB5 room palettes and their source-hash manifest.</summary>
    public string RoomPaletteDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomPaletteDirectoryName);
    /// <summary>Loads each installed room palette as the player left it, keyed by its source identity.</summary>
    public RoomStaticPalettes LoadRoomPalettes() =>
        RoomStaticPaletteArtworkFiles.Load(RoomPaletteDirectory);
    /// <summary>Stock pre-coloured room block sheets under the replaceable content tree.</summary>
    public string RoomBlockSheetDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomBlockSheetDirectoryName);
    /// <summary>Loads every tileset's installed pre-coloured block sheet as the player left it (#1392).</summary>
    public InstalledRoomArtwork LoadRoomBlockSheets() => RoomBlockSheetFiles.Load(RoomBlockSheetDirectory);
    /// <summary>Installed pre-assembled boss body sheets (#1466), which the player edits in place.</summary>
    public string BossBodySheetDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.BossBodySheetDirectoryName);
    /// <summary>Loads every boss's installed pre-assembled body sheet (#1466).</summary>
    public BossBodySheets LoadBossBodySheets() => BossBodySheetFiles.Load(BossBodySheetDirectory);
    /// <summary>Stock visual room-block references under the replaceable content tree.</summary>
    public string RoomVisualLayoutDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomVisualLayoutDirectoryName);
    /// <summary>Visual room-block references independent of native collision and BTS.</summary>
    public RoomLevelArtworks LoadRoomVisualLayouts() =>
        RoomVisualLayoutFiles.Load(RoomVisualLayoutDirectory);
    /// <summary>Stock typed terrain of every room level under the replaceable content tree; it is not artwork a player edits.</summary>
    public string RoomLevelStreamDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomLevelStreamDirectoryName);
    /// <summary>Validated stock collision/BTS and streaming allocations, independent of editable visual layouts.</summary>
    public InstalledRoomLevelStreams LoadRoomLevelStreams() => RoomLevelStreamFiles.Load(RoomLevelStreamDirectory);
    /// <summary>Stock definitions for shootable block appearances under the replaceable content tree.</summary>
    public string RoomPlmShotBlockVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomPlmShotBlockVisualDirectoryName);
    /// <summary>Editable shot-block appearances; PLM timing, placement, and collision remain compiled.</summary>
    public InstalledRoomPlmShotBlockVisuals LoadRoomPlmShotBlockVisuals() =>
        RoomPlmShotBlockVisualFiles.Load(RoomPlmShotBlockVisualDirectory);
    /// <summary>Stock definitions for Grapple-reactive block appearances under the replaceable content tree.</summary>
    public string RoomPlmGrappleBlockVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomPlmGrappleBlockVisualDirectoryName);
    /// <summary>Editable Grapple-block appearances; timing, placement, and collision remain compiled.</summary>
    public InstalledRoomPlmGrappleBlockVisuals LoadRoomPlmGrappleBlockVisuals() =>
        RoomPlmGrappleBlockVisualFiles.Load(RoomPlmGrappleBlockVisualDirectory);
    /// <summary>Stock definitions for save, refill, and map station appearances under the replaceable content tree.</summary>
    public string RoomPlmStationVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomPlmStationVisualDirectoryName);
    /// <summary>Editable station appearances; activation, rewards, and collision remain compiled.</summary>
    public InstalledRoomPlmStationVisuals LoadRoomPlmStationVisuals() =>
        RoomPlmStationVisualFiles.Load(RoomPlmStationVisualDirectory);
    /// <summary>Stock definitions for blue-door cap frames under the replaceable content tree.</summary>
    public string RoomPlmBlueDoorVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomPlmBlueDoorVisualDirectoryName);
    /// <summary>Editable blue-door cap art; opening timing and physical tiles remain compiled.</summary>
    public InstalledRoomPlmBlueDoorVisuals LoadRoomPlmBlueDoorVisuals() =>
        RoomPlmBlueDoorVisualFiles.Load(RoomPlmBlueDoorVisualDirectory);
    /// <summary>Stock definitions for colored-door cap frames under the replaceable content tree.</summary>
    public string RoomPlmColoredDoorVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomPlmColoredDoorVisualDirectoryName);
    /// <summary>Editable colored-door cap art; hit rules and physical tiles remain compiled.</summary>
    public InstalledRoomPlmColoredDoorVisuals LoadRoomPlmColoredDoorVisuals() =>
        RoomPlmColoredDoorVisualFiles.Load(RoomPlmColoredDoorVisualDirectory);
    /// <summary>Stock definitions for grey-door caps and shared clear frames under the replaceable content tree.</summary>
    public string RoomPlmGreyDoorVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomPlmGreyDoorVisualDirectoryName);
    /// <summary>Editable grey caps and shared clear frames; physical door rules stay compiled.</summary>
    public InstalledRoomPlmGreyDoorVisuals LoadRoomPlmGreyDoorVisuals() =>
        RoomPlmGreyDoorVisualFiles.Load(RoomPlmGreyDoorVisualDirectory);
    /// <summary>Stock definitions for the three eye-door components' appearances under the replaceable content tree.</summary>
    public string RoomPlmEyeDoorVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomPlmEyeDoorVisualDirectoryName);
    /// <summary>Editable eye-door blocks; the three PLM components retain compiled behavior.</summary>
    public InstalledRoomPlmEyeDoorVisuals LoadRoomPlmEyeDoorVisuals() =>
        RoomPlmEyeDoorVisualFiles.Load(RoomPlmEyeDoorVisualDirectory);
    /// <summary>Stock definitions for Mother Brain glass appearances under the replaceable content tree.</summary>
    public string RoomPlmMotherBrainGlassVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomPlmMotherBrainGlassVisualDirectoryName);
    /// <summary>Editable glass appearance; shatter physics and room-object logic stay compiled.</summary>
    public RoomPlmMotherBrainGlassVisualCatalog LoadRoomPlmMotherBrainGlassVisuals() =>
        RoomPlmMotherBrainGlassVisualFiles.Load(RoomPlmMotherBrainGlassVisualDirectory);
    /// <summary>Stock definitions for intact and broken n00b-tube appearances under the replaceable content tree.</summary>
    public string RoomPlmNoobTubeVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomPlmNoobTubeVisualDirectoryName);
    /// <summary>Editable n00b-tube appearance; break logic and physical blocks stay compiled.</summary>
    public InstalledRoomPlmNoobTubeVisuals LoadRoomPlmNoobTubeVisuals() =>
        RoomPlmNoobTubeVisualFiles.Load(RoomPlmNoobTubeVisualDirectory);
    /// <summary>Stock definitions for downward gate-block appearances under the replaceable content tree.</summary>
    public string RoomPlmDownwardGateVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomPlmDownwardGateVisualDirectoryName);
    /// <summary>Editable gate-block art; activation, animation, and collision remain compiled.</summary>
    public InstalledRoomPlmDownwardGateVisuals LoadRoomPlmDownwardGateVisuals() =>
        RoomPlmDownwardGateVisualFiles.Load(RoomPlmDownwardGateVisualDirectory);
    /// <summary>Stock definitions for elevator-platform frames under the replaceable content tree.</summary>
    public string RoomPlmElevatorPlatformVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomPlmElevatorPlatformVisualDirectoryName);
    /// <summary>Editable elevator frames; physical blocks and the instruction loop stay compiled.</summary>
    public RoomPlmElevatorPlatformVisualCatalog LoadRoomPlmElevatorPlatformVisuals() =>
        RoomPlmElevatorPlatformVisualFiles.Load(RoomPlmElevatorPlatformVisualDirectory);
    /// <summary>Stock definitions for closing escape-gate appearances under the replaceable content tree.</summary>
    public string RoomPlmEscapeGateVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomPlmEscapeGateVisualDirectoryName);
    /// <summary>Editable escape-gate appearance; closing and collision stay compiled.</summary>
    public RoomPlmEscapeGateVisualCatalog LoadRoomPlmEscapeGateVisuals() =>
        RoomPlmEscapeGateVisualFiles.Load(RoomPlmEscapeGateVisualDirectory);
    /// <summary>Stock definitions for Bomb Torizo hand appearances under the replaceable content tree.</summary>
    public string RoomPlmBombTorizoHandVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomPlmBombTorizoHandVisualDirectoryName);
    /// <summary>Editable Bomb Torizo hand art; its physical block words stay compiled.</summary>
    public RoomPlmBombTorizoHandVisualCatalog LoadRoomPlmBombTorizoHandVisuals() =>
        RoomPlmBombTorizoHandVisualFiles.Load(RoomPlmBombTorizoHandVisualDirectory);
    /// <summary>Stock definitions for reachable Draygon cannon appearances under the replaceable content tree.</summary>
    public string RoomPlmDraygonCannonVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomPlmDraygonCannonVisualDirectoryName);
    /// <summary>Editable reachable cannon art; physical blocks remain compiled.</summary>
    public RoomPlmDraygonCannonVisualCatalog LoadRoomPlmDraygonCannonVisuals() =>
        RoomPlmDraygonCannonVisualFiles.Load(RoomPlmDraygonCannonVisualDirectory);
    /// <summary>Stock definitions for Chozo hand and slope-access appearances under the replaceable content tree.</summary>
    public string RoomPlmChozoStatueVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomPlmChozoStatueVisualDirectoryName);
    /// <summary>Editable Chozo hand and slope-access art; physical blocks remain compiled.</summary>
    public RoomPlmChozoStatueVisualCatalog LoadRoomPlmChozoStatueVisuals() =>
        RoomPlmChozoStatueVisualFiles.Load(RoomPlmChozoStatueVisualDirectory);
    /// <summary>Stock definitions for linked-block restoration appearances under the replaceable content tree.</summary>
    public string RoomPlmLinkedRestoreVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomPlmLinkedRestoreVisualDirectoryName);
    /// <summary>Editable linked-block restoration art; collision stays compiled.</summary>
    public InstalledRoomPlmLinkedRestoreVisuals LoadRoomPlmLinkedRestoreVisuals() =>
        RoomPlmLinkedRestoreVisualFiles.Load(RoomPlmLinkedRestoreVisualDirectory);
    /// <summary>Stock definitions for Tourian access-floor appearances under the replaceable content tree.</summary>
    public string RoomPlmTourianAccessVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomPlmTourianAccessVisualDirectoryName);
    /// <summary>Editable Tourian access-floor art; physical rows and timing stay compiled.</summary>
    public RoomPlmTourianAccessVisuals LoadRoomPlmTourianAccessVisuals() =>
        RoomPlmTourianAccessVisualFiles.Load(RoomPlmTourianAccessVisualDirectory);
    /// <summary>Stock definitions for the bomb-revealed Speed Booster tile under the replaceable content tree.</summary>
    public string RoomPlmSpeedBoosterVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomPlmSpeedBoosterVisualDirectoryName);
    /// <summary>Editable bomb-revealed Speed Booster tile; collision stays compiled.</summary>
    public InstalledRoomPlmSpeedBoosterVisuals LoadRoomPlmSpeedBoosterVisuals() =>
        RoomPlmSpeedBoosterVisualFiles.Load(RoomPlmSpeedBoosterVisualDirectory);
    /// <summary>Stock definitions for the Maridia elevatube PLM tile under the replaceable content tree.</summary>
    public string RoomPlmMaridiaElevatubeVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomPlmMaridiaElevatubeVisualDirectoryName);
    /// <summary>Editable Maridia elevatube PLM tile; its physical block stays compiled.</summary>
    public InstalledRoomPlmMaridiaElevatubeVisuals LoadRoomPlmMaridiaElevatubeVisuals() =>
        RoomPlmMaridiaElevatubeVisualFiles.Load(RoomPlmMaridiaElevatubeVisualDirectory);
    /// <summary>Stock definitions for Spore Spawn ceiling tiles under the replaceable content tree.</summary>
    public string RoomPlmSporeSpawnCeilingVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomPlmSporeSpawnCeilingVisualDirectoryName);
    /// <summary>Editable Spore Spawn ceiling tiles; physical draw words stay compiled.</summary>
    public RoomPlmSporeSpawnCeilingVisualCatalog LoadRoomPlmSporeSpawnCeilingVisuals() =>
        RoomPlmSporeSpawnCeilingVisualFiles.Load(RoomPlmSporeSpawnCeilingVisualDirectory);
    /// <summary>Stock definitions for floor and ceiling plant tiles under the replaceable content tree.</summary>
    public string RoomPlmSamusEaterVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomPlmSamusEaterVisualDirectoryName);
    /// <summary>Editable floor/ceiling plant tiles; physical draw geometry stays compiled.</summary>
    public InstalledRoomPlmSamusEaterVisuals LoadRoomPlmSamusEaterVisuals() =>
        RoomPlmSamusEaterVisualFiles.Load(RoomPlmSamusEaterVisualDirectory);
    /// <summary>Stock definitions for Botwoon wall-clear tiles under the replaceable content tree.</summary>
    public string RoomPlmBotwoonWallVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomPlmBotwoonWallVisualDirectoryName);
    /// <summary>Editable Botwoon wall-clear tiles; collision and crumble timing stay compiled.</summary>
    public InstalledRoomPlmBotwoonWallVisuals LoadRoomPlmBotwoonWallVisuals() =>
        RoomPlmBotwoonWallVisualFiles.Load(RoomPlmBotwoonWallVisualDirectory);
    /// <summary>Stock definitions for Kraid ceiling and spike appearances under the replaceable content tree.</summary>
    public string RoomPlmKraidVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomPlmKraidVisualDirectoryName);
    /// <summary>Editable Kraid ceiling/spike art; physical mutations stay compiled.</summary>
    public InstalledRoomPlmKraidVisuals LoadRoomPlmKraidVisuals() =>
        RoomPlmKraidVisualFiles.Load(RoomPlmKraidVisualDirectory);
    /// <summary>Stock definitions for Crocomire bridge and wall appearances under the replaceable content tree.</summary>
    public string RoomPlmCrocomireVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomPlmCrocomireVisualDirectoryName);
    /// <summary>Editable Crocomire bridge/wall art; collision and timing stay compiled.</summary>
    public RoomPlmCrocomireVisualCatalog LoadRoomPlmCrocomireVisuals() =>
        RoomPlmCrocomireVisualFiles.Load(RoomPlmCrocomireVisualDirectory);
    /// <summary>Stock definitions for Mother Brain fake-death terrain appearances under the replaceable content tree.</summary>
    public string RoomPlmMotherBrainFakeDeathVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomPlmMotherBrainFakeDeathVisualDirectoryName);
    /// <summary>Editable fake-death terrain art; physical room mutations remain compiled.</summary>
    public RoomPlmMotherBrainFakeDeathVisualCatalog LoadRoomPlmMotherBrainFakeDeathVisuals() =>
        RoomPlmMotherBrainFakeDeathVisualFiles.Load(
            RoomPlmMotherBrainFakeDeathVisualDirectory);
    /// <summary>Stock definitions for item, orb, and reveal appearances under the replaceable content tree.</summary>
    public string RoomPlmCollectibleVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomPlmCollectibleVisualDirectoryName);
    /// <summary>Editable item/orb/reveal appearances; pickup and collision remain compiled.</summary>
    public InstalledRoomPlmCollectibleVisuals LoadRoomPlmCollectibleVisuals() =>
        RoomPlmCollectibleVisualFiles.Load(RoomPlmCollectibleVisualDirectory);
    /// <summary>Stock definitions for X-ray reveal metatile selections under the replaceable content tree.</summary>
    public string XrayRevealVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.XrayRevealVisualDirectoryName);
    /// <summary>Editable X-ray metatile choices; reveal commands and collision rules remain compiled.</summary>
    public InstalledXrayReveals LoadXrayRevealVisuals() =>
        XrayRevealVisualFiles.Load(XrayRevealVisualDirectory);
    /// <summary>Loads the validated installed projectile presentation.</summary>
    public InstalledProjectilePresentation LoadProjectiles() => ProjectilePresentationFiles.Load(ProjectileDirectory);
    /// <summary>Installed enemy, boss and area artwork, which the player edits in place.</summary>
    public string EnemyTileDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.EnemyTileDirectoryName);
    /// <summary>Loads the validated installed enemy, boss and area artwork.</summary>
    public InstalledEnemyArtwork LoadEnemyTiles() => EnemyTileArtworkFiles.Load(EnemyTileDirectory);

    /// <summary>Loads the installed maps, exploration masks and menu presentation without a cartridge address space.</summary>
    public AreaMapPresentation LoadMaps() => MapPresentationFiles.Load(MapDirectory);

    /// <summary>
    /// Loads every installed presentation catalog the frontend binds. Hosts pass the maps, projectile presentation and
    /// enemy directory they already hold for content identity and save persistence.
    /// </summary>
    public GameArtwork LoadGameArtwork(AreaMapPresentation maps, InstalledProjectilePresentation projectiles, InstalledEnemyArtwork enemies)
    {
        Ensure.NotNull(maps);
        Ensure.NotNull(projectiles);
        Ensure.NotNull(enemies);
        return new GameArtwork
        {
            MapPresentation = maps,
            IntroCinematicArt = LoadIntroCinematicArt(),
            SamusBodyArt = LoadSamusBodyArt(),
            EndingFloorArt = LoadEndingMode7Art(),
            EndingObjectArt = LoadEndingObjectArt(),
            EndingPaletteArt = LoadEndingPalettes(),
            RoomBlockSheets = LoadRoomBlockSheets(),
            RoomVisualLayouts = LoadRoomVisualLayouts(),
            RoomLevelStreams = LoadRoomLevelStreams(),
            RoomPlmShotBlockVisuals = LoadRoomPlmShotBlockVisuals(),
            RoomPlmGrappleBlockVisuals = LoadRoomPlmGrappleBlockVisuals(),
            RoomPlmStationVisuals = LoadRoomPlmStationVisuals(),
            RoomPlmBlueDoorVisuals = LoadRoomPlmBlueDoorVisuals(),
            RoomPlmColoredDoorVisuals = LoadRoomPlmColoredDoorVisuals(),
            RoomPlmGreyDoorVisuals = LoadRoomPlmGreyDoorVisuals(),
            RoomPlmEyeDoorVisuals = LoadRoomPlmEyeDoorVisuals(),
            RoomPlmMotherBrainGlassVisuals = LoadRoomPlmMotherBrainGlassVisuals(),
            RoomPlmNoobTubeVisuals = LoadRoomPlmNoobTubeVisuals(),
            RoomPlmDownwardGateVisuals = LoadRoomPlmDownwardGateVisuals(),
            RoomPlmElevatorPlatformVisuals = LoadRoomPlmElevatorPlatformVisuals(),
            RoomPlmEscapeGateVisuals = LoadRoomPlmEscapeGateVisuals(),
            RoomPlmBombTorizoHandVisuals = LoadRoomPlmBombTorizoHandVisuals(),
            RoomPlmDraygonCannonVisuals = LoadRoomPlmDraygonCannonVisuals(),
            RoomPlmChozoStatueVisuals = LoadRoomPlmChozoStatueVisuals(),
            RoomPlmLinkedRestoreVisuals = LoadRoomPlmLinkedRestoreVisuals(),
            RoomPlmTourianAccessVisuals = LoadRoomPlmTourianAccessVisuals(),
            RoomPlmSpeedBoosterVisuals = LoadRoomPlmSpeedBoosterVisuals(),
            RoomPlmMaridiaElevatubeVisuals = LoadRoomPlmMaridiaElevatubeVisuals(),
            RoomPlmSporeSpawnCeilingVisuals = LoadRoomPlmSporeSpawnCeilingVisuals(),
            RoomPlmSamusEaterVisuals = LoadRoomPlmSamusEaterVisuals(),
            RoomPlmBotwoonWallVisuals = LoadRoomPlmBotwoonWallVisuals(),
            RoomPlmKraidVisuals = LoadRoomPlmKraidVisuals(),
            RoomPlmCrocomireVisuals = LoadRoomPlmCrocomireVisuals(),
            RoomPlmMotherBrainFakeDeathVisuals = LoadRoomPlmMotherBrainFakeDeathVisuals(),
            RoomPlmCollectibleVisuals = LoadRoomPlmCollectibleVisuals(),
            XrayRevealVisuals = LoadXrayRevealVisuals(),
            ProjectileFrameBindings = projectiles.FrameBindings,

            EnemyTileArtwork = enemies.Enemies,
            BossArtwork = enemies.Bosses,
            ChargeFlarePlacement = projectiles.FlarePlacement,
            GrappleFlarePlacement = projectiles.GrappleTiles.FlarePlacement,
            GrappleSwingFrames = projectiles.GrappleTiles.SwingFrames,
            SamusOverreads = LoadSamusOverreads(),
            EnemyOverreads = LoadEnemyOverreads(),
            BossAndIntroOverreads = LoadBossAndIntroOverreads(),
            SpriteSheets = LoadSpriteSheets(),
            EnemySheets = LoadEnemySheets(),
            BossBodySheets = LoadBossBodySheets(),
        };
    }
}
