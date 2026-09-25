using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.AssetExtraction;

/// <summary>App-owned immutable cartridge/audio content, separate from persistent player data.</summary>
public sealed record GameInstallation(string Root)
{
    public string ContentDirectory => Path.Combine(Root, GameInstallationLayout.ContentDirectoryName);
    public string RomPath => Path.Combine(ContentDirectory, GameInstallationLayout.RomFileName);
    public string AudioDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.AudioDirectoryName);
    /// <summary>Persistent editable audio content, outside the replaceable stock installation.</summary>
    public string AudioOverrideDirectory => Path.Combine(Root, "overrides", GameInstallationLayout.AudioDirectoryName);
    /// <summary>Validates stock content, then selects a complete compatible user override when present.</summary>
    public ExtractedAudioAssetCatalog LoadAudio() =>
        ExtractedAudioAssetCatalog.Load(AudioDirectory, AudioOverrideDirectory);
    public string MapDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.MapDirectoryName);
    public string ProjectileDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.ProjectileDirectoryName);
    public string RoomCharacterDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomCharacterDirectoryName);
    public string IntroCinematicDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.IntroCinematicDirectoryName);
    /// <summary>Opening-cinematic PNG edits survive stock content replacement.</summary>
    public string IntroCinematicOverrideDirectory => Path.Combine(Root, "overrides", GameInstallationLayout.IntroCinematicDirectoryName);
    public IntroCinematicArtworkCatalog LoadIntroCinematicArt() =>
        IntroCinematicArtworkFiles.Load(IntroCinematicDirectory, IntroCinematicOverrideDirectory);
    public string EndingMode7Directory => Path.Combine(ContentDirectory, GameInstallationLayout.EndingMode7DirectoryName);
    /// <summary>Ending scene art edits remain outside replaceable stock content.</summary>
    public string EndingMode7OverrideDirectory => Path.Combine(Root, "overrides", GameInstallationLayout.EndingMode7DirectoryName);
    public EndingMode7ArtworkCatalog LoadEndingMode7Art() =>
        EndingMode7ArtworkFiles.Load(EndingMode7Directory, EndingMode7OverrideDirectory);
    public string EndingObjectDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.EndingObjectDirectoryName);
    /// <summary>Ending OBJ edits remain outside replaceable stock content.</summary>
    public string EndingObjectOverrideDirectory => Path.Combine(Root, "overrides", GameInstallationLayout.EndingObjectDirectoryName);
    public EndingObjectArtworkCatalog LoadEndingObjectArt() =>
        EndingObjectArtworkFiles.Load(EndingObjectDirectory, EndingObjectOverrideDirectory);
    public string EndingPaletteDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.EndingPaletteDirectoryName);
    /// <summary>Ending color edits survive replacement of the stock installation.</summary>
    public string EndingPaletteOverrideDirectory => Path.Combine(Root, "overrides", GameInstallationLayout.EndingPaletteDirectoryName);
    public EndingPaletteCatalog LoadEndingPalettes() =>
        EndingPaletteArtworkFiles.Load(EndingPaletteDirectory, EndingPaletteOverrideDirectory);
    /// <summary>Editable room character art stays outside the replaceable stock game directory.</summary>
    public string RoomCharacterOverrideDirectory => Path.Combine(Root, "overrides", GameInstallationLayout.RoomCharacterDirectoryName);
    public RoomCharacterAtlasCatalog LoadRoomCharacters() =>
        RoomCharacterArtworkFiles.Load(RoomCharacterDirectory, RoomCharacterOverrideDirectory);
    public string RoomPaletteDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomPaletteDirectoryName);
    public string RoomPaletteOverrideDirectory => Path.Combine(Root, "overrides", GameInstallationLayout.RoomPaletteDirectoryName);
    public RoomStaticPaletteCatalog LoadRoomPalettes() =>
        RoomStaticPaletteArtworkFiles.Load(RoomPaletteDirectory, RoomPaletteOverrideDirectory);
    public string RoomMetatileDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomMetatileDirectoryName);
    /// <summary>Editable visual block compositions survive stock content replacement.</summary>
    public string RoomMetatileOverrideDirectory => Path.Combine(Root, "overrides", GameInstallationLayout.RoomMetatileDirectoryName);
    public RoomMetatileCatalog LoadRoomMetatiles() =>
        RoomMetatileArtworkFiles.Load(RoomMetatileDirectory, RoomMetatileOverrideDirectory);
    public string RoomBackgroundTilemapDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomBackgroundTilemapDirectoryName);
    public string RoomVisualLayoutDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomVisualLayoutDirectoryName);
    public string RoomVisualLayoutOverrideDirectory => Path.Combine(Root, "overrides", GameInstallationLayout.RoomVisualLayoutDirectoryName);
    /// <summary>Visual room-block references independent of native collision and BTS.</summary>
    public RoomVisualLayoutCatalog LoadRoomVisualLayouts() =>
        RoomVisualLayoutFiles.Load(RoomVisualLayoutDirectory, RoomVisualLayoutOverrideDirectory);
    public string RoomPlmShotBlockVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomPlmShotBlockVisualDirectoryName);
    public string RoomPlmShotBlockVisualOverrideDirectory => Path.Combine(Root, "overrides", GameInstallationLayout.RoomPlmShotBlockVisualDirectoryName);
    /// <summary>Editable shot-block appearances; PLM timing, placement, and collision remain compiled.</summary>
    public RoomPlmShotBlockVisualCatalog LoadRoomPlmShotBlockVisuals() =>
        RoomPlmShotBlockVisualFiles.Load(RoomPlmShotBlockVisualDirectory, RoomPlmShotBlockVisualOverrideDirectory);
    public string RoomPlmGrappleBlockVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomPlmGrappleBlockVisualDirectoryName);
    public string RoomPlmGrappleBlockVisualOverrideDirectory => Path.Combine(Root, "overrides", GameInstallationLayout.RoomPlmGrappleBlockVisualDirectoryName);
    /// <summary>Editable Grapple-block appearances; timing, placement, and collision remain compiled.</summary>
    public RoomPlmGrappleBlockVisualCatalog LoadRoomPlmGrappleBlockVisuals() =>
        RoomPlmGrappleBlockVisualFiles.Load(RoomPlmGrappleBlockVisualDirectory, RoomPlmGrappleBlockVisualOverrideDirectory);
    public string RoomPlmStationVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomPlmStationVisualDirectoryName);
    public string RoomPlmStationVisualOverrideDirectory => Path.Combine(Root, "overrides", GameInstallationLayout.RoomPlmStationVisualDirectoryName);
    /// <summary>Editable station appearances; activation, rewards, and collision remain compiled.</summary>
    public RoomPlmStationVisualCatalog LoadRoomPlmStationVisuals() =>
        RoomPlmStationVisualFiles.Load(RoomPlmStationVisualDirectory, RoomPlmStationVisualOverrideDirectory);
    public string RoomPlmBlueDoorVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomPlmBlueDoorVisualDirectoryName);
    public string RoomPlmBlueDoorVisualOverrideDirectory => Path.Combine(Root, "overrides", GameInstallationLayout.RoomPlmBlueDoorVisualDirectoryName);
    /// <summary>Editable blue-door cap art; opening timing and physical tiles remain compiled.</summary>
    public RoomPlmBlueDoorVisualCatalog LoadRoomPlmBlueDoorVisuals() =>
        RoomPlmBlueDoorVisualFiles.Load(RoomPlmBlueDoorVisualDirectory,
            RoomPlmBlueDoorVisualOverrideDirectory);
    public string RoomPlmColoredDoorVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomPlmColoredDoorVisualDirectoryName);
    public string RoomPlmColoredDoorVisualOverrideDirectory => Path.Combine(Root, "overrides", GameInstallationLayout.RoomPlmColoredDoorVisualDirectoryName);
    /// <summary>Editable colored-door cap art; hit rules and physical tiles remain compiled.</summary>
    public RoomPlmColoredDoorVisualCatalog LoadRoomPlmColoredDoorVisuals() =>
        RoomPlmColoredDoorVisualFiles.Load(RoomPlmColoredDoorVisualDirectory,
            RoomPlmColoredDoorVisualOverrideDirectory);
    public string RoomPlmGreyDoorVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomPlmGreyDoorVisualDirectoryName);
    public string RoomPlmGreyDoorVisualOverrideDirectory => Path.Combine(Root, "overrides", GameInstallationLayout.RoomPlmGreyDoorVisualDirectoryName);
    /// <summary>Editable grey caps and shared clear frames; physical door rules stay compiled.</summary>
    public RoomPlmGreyDoorVisualCatalog LoadRoomPlmGreyDoorVisuals() =>
        RoomPlmGreyDoorVisualFiles.Load(RoomPlmGreyDoorVisualDirectory,
            RoomPlmGreyDoorVisualOverrideDirectory);
    public string RoomPlmEyeDoorVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomPlmEyeDoorVisualDirectoryName);
    public string RoomPlmEyeDoorVisualOverrideDirectory => Path.Combine(Root, "overrides", GameInstallationLayout.RoomPlmEyeDoorVisualDirectoryName);
    /// <summary>Editable eye-door blocks; the three PLM components retain compiled behavior.</summary>
    public RoomPlmEyeDoorVisualCatalog LoadRoomPlmEyeDoorVisuals() =>
        RoomPlmEyeDoorVisualFiles.Load(RoomPlmEyeDoorVisualDirectory,
            RoomPlmEyeDoorVisualOverrideDirectory);
    public string RoomPlmMotherBrainGlassVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomPlmMotherBrainGlassVisualDirectoryName);
    public string RoomPlmMotherBrainGlassVisualOverrideDirectory => Path.Combine(Root, "overrides", GameInstallationLayout.RoomPlmMotherBrainGlassVisualDirectoryName);
    /// <summary>Editable glass appearance; shatter physics and room-object logic stay compiled.</summary>
    public RoomPlmMotherBrainGlassVisualCatalog LoadRoomPlmMotherBrainGlassVisuals() =>
        RoomPlmMotherBrainGlassVisualFiles.Load(RoomPlmMotherBrainGlassVisualDirectory,
            RoomPlmMotherBrainGlassVisualOverrideDirectory);
    public string RoomPlmNoobTubeVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomPlmNoobTubeVisualDirectoryName);
    public string RoomPlmNoobTubeVisualOverrideDirectory => Path.Combine(Root, "overrides", GameInstallationLayout.RoomPlmNoobTubeVisualDirectoryName);
    /// <summary>Editable n00b-tube appearance; break logic and physical blocks stay compiled.</summary>
    public RoomPlmNoobTubeVisualCatalog LoadRoomPlmNoobTubeVisuals() =>
        RoomPlmNoobTubeVisualFiles.Load(RoomPlmNoobTubeVisualDirectory,
            RoomPlmNoobTubeVisualOverrideDirectory);
    public string RoomPlmDownwardGateVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomPlmDownwardGateVisualDirectoryName);
    public string RoomPlmDownwardGateVisualOverrideDirectory => Path.Combine(Root, "overrides", GameInstallationLayout.RoomPlmDownwardGateVisualDirectoryName);
    /// <summary>Editable gate-block art; activation, animation, and collision remain compiled.</summary>
    public RoomPlmDownwardGateVisualCatalog LoadRoomPlmDownwardGateVisuals() =>
        RoomPlmDownwardGateVisualFiles.Load(RoomPlmDownwardGateVisualDirectory,
            RoomPlmDownwardGateVisualOverrideDirectory);
    public string RoomPlmEscapeGateVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomPlmEscapeGateVisualDirectoryName);
    public string RoomPlmEscapeGateVisualOverrideDirectory => Path.Combine(Root, "overrides", GameInstallationLayout.RoomPlmEscapeGateVisualDirectoryName);
    /// <summary>Editable escape-gate appearance; closing and collision stay compiled.</summary>
    public RoomPlmEscapeGateVisualCatalog LoadRoomPlmEscapeGateVisuals() =>
        RoomPlmEscapeGateVisualFiles.Load(RoomPlmEscapeGateVisualDirectory,
            RoomPlmEscapeGateVisualOverrideDirectory);
    public string RoomPlmBombTorizoHandVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomPlmBombTorizoHandVisualDirectoryName);
    public string RoomPlmBombTorizoHandVisualOverrideDirectory => Path.Combine(Root, "overrides", GameInstallationLayout.RoomPlmBombTorizoHandVisualDirectoryName);
    /// <summary>Editable Bomb Torizo hand art; its physical block words stay compiled.</summary>
    public RoomPlmBombTorizoHandVisualCatalog LoadRoomPlmBombTorizoHandVisuals() =>
        RoomPlmBombTorizoHandVisualFiles.Load(RoomPlmBombTorizoHandVisualDirectory,
            RoomPlmBombTorizoHandVisualOverrideDirectory);
    public string RoomPlmDraygonCannonVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomPlmDraygonCannonVisualDirectoryName);
    public string RoomPlmDraygonCannonVisualOverrideDirectory => Path.Combine(Root, "overrides", GameInstallationLayout.RoomPlmDraygonCannonVisualDirectoryName);
    /// <summary>Editable reachable cannon art; physical blocks remain compiled.</summary>
    public RoomPlmDraygonCannonVisualCatalog LoadRoomPlmDraygonCannonVisuals() =>
        RoomPlmDraygonCannonVisualFiles.Load(RoomPlmDraygonCannonVisualDirectory,
            RoomPlmDraygonCannonVisualOverrideDirectory);
    public string RoomPlmChozoStatueVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomPlmChozoStatueVisualDirectoryName);
    public string RoomPlmChozoStatueVisualOverrideDirectory => Path.Combine(Root, "overrides", GameInstallationLayout.RoomPlmChozoStatueVisualDirectoryName);
    /// <summary>Editable Chozo hand and slope-access art; physical blocks remain compiled.</summary>
    public RoomPlmChozoStatueVisualCatalog LoadRoomPlmChozoStatueVisuals() =>
        RoomPlmChozoStatueVisualFiles.Load(RoomPlmChozoStatueVisualDirectory,
            RoomPlmChozoStatueVisualOverrideDirectory);
    public string RoomPlmLinkedRestoreVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomPlmLinkedRestoreVisualDirectoryName);
    public string RoomPlmLinkedRestoreVisualOverrideDirectory => Path.Combine(Root, "overrides", GameInstallationLayout.RoomPlmLinkedRestoreVisualDirectoryName);
    /// <summary>Editable linked-block restoration art; collision stays compiled.</summary>
    public RoomPlmLinkedRestoreVisualCatalog LoadRoomPlmLinkedRestoreVisuals() =>
        RoomPlmLinkedRestoreVisualFiles.Load(RoomPlmLinkedRestoreVisualDirectory,
            RoomPlmLinkedRestoreVisualOverrideDirectory);
    public string RoomPlmTourianAccessVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomPlmTourianAccessVisualDirectoryName);
    public string RoomPlmTourianAccessVisualOverrideDirectory => Path.Combine(Root, "overrides", GameInstallationLayout.RoomPlmTourianAccessVisualDirectoryName);
    /// <summary>Editable Tourian access-floor art; physical rows and timing stay compiled.</summary>
    public RoomPlmTourianAccessVisualCatalog LoadRoomPlmTourianAccessVisuals() =>
        RoomPlmTourianAccessVisualFiles.Load(RoomPlmTourianAccessVisualDirectory,
            RoomPlmTourianAccessVisualOverrideDirectory);
    public string RoomPlmSpeedBoosterVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomPlmSpeedBoosterVisualDirectoryName);
    public string RoomPlmSpeedBoosterVisualOverrideDirectory => Path.Combine(Root, "overrides", GameInstallationLayout.RoomPlmSpeedBoosterVisualDirectoryName);
    /// <summary>Editable bomb-revealed Speed Booster tile; collision stays compiled.</summary>
    public RoomPlmSpeedBoosterVisualCatalog LoadRoomPlmSpeedBoosterVisuals() =>
        RoomPlmSpeedBoosterVisualFiles.Load(RoomPlmSpeedBoosterVisualDirectory,
            RoomPlmSpeedBoosterVisualOverrideDirectory);
    public string RoomPlmMaridiaElevatubeVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomPlmMaridiaElevatubeVisualDirectoryName);
    public string RoomPlmMaridiaElevatubeVisualOverrideDirectory => Path.Combine(Root, "overrides", GameInstallationLayout.RoomPlmMaridiaElevatubeVisualDirectoryName);
    /// <summary>Editable Maridia elevatube PLM tile; its physical block stays compiled.</summary>
    public RoomPlmMaridiaElevatubeVisualCatalog LoadRoomPlmMaridiaElevatubeVisuals() =>
        RoomPlmMaridiaElevatubeVisualFiles.Load(RoomPlmMaridiaElevatubeVisualDirectory,
            RoomPlmMaridiaElevatubeVisualOverrideDirectory);
    public string RoomPlmSporeSpawnCeilingVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomPlmSporeSpawnCeilingVisualDirectoryName);
    public string RoomPlmSporeSpawnCeilingVisualOverrideDirectory => Path.Combine(Root, "overrides", GameInstallationLayout.RoomPlmSporeSpawnCeilingVisualDirectoryName);
    /// <summary>Editable Spore Spawn ceiling tiles; physical draw words stay compiled.</summary>
    public RoomPlmSporeSpawnCeilingVisualCatalog LoadRoomPlmSporeSpawnCeilingVisuals() =>
        RoomPlmSporeSpawnCeilingVisualFiles.Load(RoomPlmSporeSpawnCeilingVisualDirectory,
            RoomPlmSporeSpawnCeilingVisualOverrideDirectory);
    public string RoomPlmBotwoonWallVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomPlmBotwoonWallVisualDirectoryName);
    public string RoomPlmBotwoonWallVisualOverrideDirectory => Path.Combine(Root, "overrides", GameInstallationLayout.RoomPlmBotwoonWallVisualDirectoryName);
    /// <summary>Editable Botwoon wall-clear tiles; collision and crumble timing stay compiled.</summary>
    public RoomPlmBotwoonWallVisualCatalog LoadRoomPlmBotwoonWallVisuals() =>
        RoomPlmBotwoonWallVisualFiles.Load(RoomPlmBotwoonWallVisualDirectory,
            RoomPlmBotwoonWallVisualOverrideDirectory);
    public string RoomPlmKraidVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomPlmKraidVisualDirectoryName);
    public string RoomPlmKraidVisualOverrideDirectory => Path.Combine(Root, "overrides", GameInstallationLayout.RoomPlmKraidVisualDirectoryName);
    /// <summary>Editable Kraid ceiling/spike art; physical mutations stay compiled.</summary>
    public RoomPlmKraidVisualCatalog LoadRoomPlmKraidVisuals() =>
        RoomPlmKraidVisualFiles.Load(RoomPlmKraidVisualDirectory,
            RoomPlmKraidVisualOverrideDirectory);
    public string RoomPlmCrocomireVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomPlmCrocomireVisualDirectoryName);
    public string RoomPlmCrocomireVisualOverrideDirectory => Path.Combine(Root, "overrides", GameInstallationLayout.RoomPlmCrocomireVisualDirectoryName);
    /// <summary>Editable Crocomire bridge/wall art; collision and timing stay compiled.</summary>
    public RoomPlmCrocomireVisualCatalog LoadRoomPlmCrocomireVisuals() =>
        RoomPlmCrocomireVisualFiles.Load(RoomPlmCrocomireVisualDirectory,
            RoomPlmCrocomireVisualOverrideDirectory);
    public string RoomPlmMotherBrainFakeDeathVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomPlmMotherBrainFakeDeathVisualDirectoryName);
    public string RoomPlmMotherBrainFakeDeathVisualOverrideDirectory => Path.Combine(Root, "overrides", GameInstallationLayout.RoomPlmMotherBrainFakeDeathVisualDirectoryName);
    /// <summary>Editable fake-death terrain art; physical room mutations remain compiled.</summary>
    public RoomPlmMotherBrainFakeDeathVisualCatalog LoadRoomPlmMotherBrainFakeDeathVisuals() =>
        RoomPlmMotherBrainFakeDeathVisualFiles.Load(
            RoomPlmMotherBrainFakeDeathVisualDirectory,
            RoomPlmMotherBrainFakeDeathVisualOverrideDirectory);
    public string RoomPlmCollectibleVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomPlmCollectibleVisualDirectoryName);
    public string RoomPlmCollectibleVisualOverrideDirectory => Path.Combine(Root, "overrides", GameInstallationLayout.RoomPlmCollectibleVisualDirectoryName);
    /// <summary>Editable item/orb/reveal appearances; pickup and collision remain compiled.</summary>
    public RoomPlmCollectibleVisualCatalog LoadRoomPlmCollectibleVisuals() =>
        RoomPlmCollectibleVisualFiles.Load(RoomPlmCollectibleVisualDirectory,
            RoomPlmCollectibleVisualOverrideDirectory);
    public string RoomPlmDynamicCollectibleArtDirectory => Path.Combine(ContentDirectory,
        GameInstallationLayout.RoomPlmDynamicCollectibleArtDirectoryName);
    public string RoomPlmDynamicCollectibleArtOverrideDirectory => Path.Combine(Root,
        "overrides", GameInstallationLayout.RoomPlmDynamicCollectibleArtDirectoryName);
    /// <summary>Editable item character PNGs and tile-palette selectors.</summary>
    public RoomPlmDynamicCollectibleArtCatalog LoadRoomPlmDynamicCollectibleArt() =>
        RoomPlmDynamicCollectibleArtFiles.Load(RoomPlmDynamicCollectibleArtDirectory,
            RoomPlmDynamicCollectibleArtOverrideDirectory);
    public string XrayRevealVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.XrayRevealVisualDirectoryName);
    public string XrayRevealVisualOverrideDirectory => Path.Combine(Root, "overrides", GameInstallationLayout.XrayRevealVisualDirectoryName);
    /// <summary>Editable X-ray metatile choices; reveal commands and collision rules remain compiled.</summary>
    public XrayRevealVisualCatalog LoadXrayRevealVisuals() =>
        XrayRevealVisualFiles.Load(XrayRevealVisualDirectory, XrayRevealVisualOverrideDirectory);
    /// <summary>Read-only logical room ID to editable art-file guide for installed content.</summary>
    public string RoomArtIndexPath => Path.Combine(ContentDirectory, RoomArtIndexFiles.FileName);
    /// <summary>Editable BG tilemaps survive replacement of stock game content.</summary>
    public string RoomBackgroundTilemapOverrideDirectory => Path.Combine(Root, "overrides", GameInstallationLayout.RoomBackgroundTilemapDirectoryName);
    public RoomBackgroundTilemapCatalog LoadRoomBackgroundTilemaps() =>
        RoomBackgroundTilemapArtworkFiles.Load(RoomBackgroundTilemapDirectory, RoomBackgroundTilemapOverrideDirectory);
    public RoomSkyTilemapCatalog LoadRoomSkyTilemaps() =>
        RoomSkyTilemapArtworkFiles.Load(RoomBackgroundTilemapDirectory, RoomBackgroundTilemapOverrideDirectory);
    public string ProjectileOverrideDirectory => Path.Combine(Root, "overrides", GameInstallationLayout.ProjectileDirectoryName);
    public InstalledProjectilePresentation LoadProjectiles() => ProjectilePresentationFiles.Load(ProjectileDirectory, ProjectileOverrideDirectory);
    /// <summary>Ordinary enemy tile sheets; replacement PNGs survive stock-content rebuilds.</summary>
    public string EnemyTileDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.EnemyTileDirectoryName);
    public string EnemyTileOverrideDirectory => Path.Combine(Root, "overrides", GameInstallationLayout.EnemyTileDirectoryName);
    public EnemyTileArtworkCatalog LoadEnemyTiles() =>
        EnemyTileArtworkFiles.Load(EnemyTileDirectory, EnemyTileOverrideDirectory);
    /// <summary>Outside the replaceable game directory: reinstall and stock repair preserve these edits.</summary>
    public string MapOverrideDirectory => Path.Combine(Root, "overrides", GameInstallationLayout.MapDirectoryName);

    /// <summary>Loads installed maps and stock exploration masks without a cartridge address space.</summary>
    public AreaMapPresentationCatalog LoadMaps() => AreaMapPresentationCatalog.Load(MapDirectory, MapOverrideDirectory);
}

/// <summary>Shared on-disk layout used by the desktop host, Android host, and installation CLI.</summary>
public static class GameInstallationLayout
{
    public const string ContentDirectoryName = "game";
    public const string RomFileName = "SuperMetroid.smc";
    public const string AudioDirectoryName = "audio";
    public const string MapDirectoryName = "maps";
    public const string ProjectileDirectoryName = "projectiles";
    public const string EnemyTileDirectoryName = "enemy-tiles";
    public const string RoomCharacterDirectoryName = "room-characters";
    public const string IntroCinematicDirectoryName = "intro-cinematic";
    public const string EndingMode7DirectoryName = "ending-mode7";
    public const string EndingObjectDirectoryName = "ending-objects";
    public const string EndingPaletteDirectoryName = "ending-palettes";
    public const string RoomPaletteDirectoryName = "room-palettes";
    public const string RoomMetatileDirectoryName = "room-blocks";
    public const string RoomBackgroundTilemapDirectoryName = "room-backgrounds";
    public const string RoomVisualLayoutDirectoryName = "room-layouts";
    public const string RoomPlmShotBlockVisualDirectoryName = "room-plm-shot-blocks";
    public const string RoomPlmGrappleBlockVisualDirectoryName = "room-plm-grapple-blocks";
    public const string RoomPlmStationVisualDirectoryName = "room-plm-stations";
    public const string RoomPlmBlueDoorVisualDirectoryName = "room-plm-blue-doors";
    public const string RoomPlmColoredDoorVisualDirectoryName = "room-plm-colored-doors";
    public const string RoomPlmGreyDoorVisualDirectoryName = "room-plm-grey-doors";
    public const string RoomPlmEyeDoorVisualDirectoryName = "room-plm-eye-doors";
    public const string RoomPlmMotherBrainGlassVisualDirectoryName = "room-plm-mother-brain-glass";
    public const string RoomPlmNoobTubeVisualDirectoryName = "room-plm-noob-tube";
    public const string RoomPlmDownwardGateVisualDirectoryName = "room-plm-downward-gates";
    public const string RoomPlmEscapeGateVisualDirectoryName = "room-plm-escape-gate";
    public const string RoomPlmBombTorizoHandVisualDirectoryName = "room-plm-bomb-torizo-hand";
    public const string RoomPlmDraygonCannonVisualDirectoryName = "room-plm-draygon-cannons";
    public const string RoomPlmChozoStatueVisualDirectoryName = "room-plm-chozo-statues";
    public const string RoomPlmLinkedRestoreVisualDirectoryName = "room-plm-linked-restores";
    public const string RoomPlmTourianAccessVisualDirectoryName = "room-plm-tourian-access";
    public const string RoomPlmSpeedBoosterVisualDirectoryName = "room-plm-speed-booster";
    public const string RoomPlmMaridiaElevatubeVisualDirectoryName = "room-plm-maridia-elevatube";
    public const string RoomPlmSporeSpawnCeilingVisualDirectoryName = "room-plm-spore-spawn-ceiling";
    public const string RoomPlmBotwoonWallVisualDirectoryName = "room-plm-botwoon-wall";
    public const string RoomPlmKraidVisualDirectoryName = "room-plm-kraid";
    public const string RoomPlmCrocomireVisualDirectoryName = "room-plm-crocomire";
    public const string RoomPlmMotherBrainFakeDeathVisualDirectoryName = "room-plm-mother-brain-fake-death";
    public const string RoomPlmCollectibleVisualDirectoryName = "room-plm-collectibles";
    public const string RoomPlmDynamicCollectibleArtDirectoryName = "room-plm-collectible-tiles";
    public const string XrayRevealVisualDirectoryName = "xray-reveals";
    public const string ReceiptFileName = "installation.json";
    public const int FormatVersion = 62;
    internal const string PreviousDirectoryName = ".game.previous";
    internal const string StagingPrefix = ".game.install-";
    internal const string LockFileName = ".game-install.lock";
}
