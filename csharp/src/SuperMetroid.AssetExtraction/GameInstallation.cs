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
    public const string RoomPaletteDirectoryName = "room-palettes";
    public const string RoomMetatileDirectoryName = "room-blocks";
    public const string RoomBackgroundTilemapDirectoryName = "room-backgrounds";
    public const string RoomVisualLayoutDirectoryName = "room-layouts";
    public const string RoomPlmShotBlockVisualDirectoryName = "room-plm-shot-blocks";
    public const string RoomPlmGrappleBlockVisualDirectoryName = "room-plm-grapple-blocks";
    public const string RoomPlmStationVisualDirectoryName = "room-plm-stations";
    public const string XrayRevealVisualDirectoryName = "xray-reveals";
    public const string ReceiptFileName = "installation.json";
    public const int FormatVersion = 38;
    internal const string PreviousDirectoryName = ".game.previous";
    internal const string StagingPrefix = ".game.install-";
    internal const string LockFileName = ".game-install.lock";
}
