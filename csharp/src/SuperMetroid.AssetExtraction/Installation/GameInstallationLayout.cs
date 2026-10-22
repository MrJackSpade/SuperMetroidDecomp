namespace SuperMetroid.AssetExtraction.Installation;

/// <summary>Shared on-disk layout used by the desktop host, Android host, and installation CLI.</summary>
public static class GameInstallationLayout
{
    /// <summary>Replaceable stock-content child directory, atomically published and repaired by the installer.</summary>
    public const string ContentDirectoryName = "game";
    /// <summary>Private validated, unheadered ROM filename retained for asset extraction and repair.</summary>
    public const string RomFileName = "SuperMetroid.smc";
    /// <summary>Content child directory for stock audio and its compatible override counterpart.</summary>
    public const string AudioDirectoryName = "audio";
    /// <summary>Content child directory for area-map artwork and stock exploration masks.</summary>
    public const string MapDirectoryName = "maps";
    /// <summary>Content child directory for initial CGRAM and room-entry sprite colors.</summary>
    public const string GameplayBasePaletteDirectoryName = "gameplay-palettes";
    /// <summary>Content child directory for the indexed common gameplay OBJ sheet.</summary>
    public const string StandardObjectDirectoryName = "standard-objects";
    /// <summary>Content child directory for projectile artwork and presentation selectors.</summary>
    public const string ProjectileDirectoryName = "projectiles";
    /// <summary>Content child directory for ordinary-enemy tile artwork.</summary>
    public const string EnemyTileDirectoryName = "enemy-tiles";
    /// <summary>Content child directory for room character-tile atlases.</summary>
    public const string RoomCharacterDirectoryName = "room-characters";
    /// <summary>Content child directory for the cartridge bytes Samus's glitch paths read.</summary>
    public const string SamusOverreadDirectoryName = "samus-overreads";
    /// <summary>Pre-coloured sprite sheets and their frame manifests.</summary>
    public const string SpriteSheetDirectoryName = "sprite-sheets";
    /// <summary>Pre-coloured enemy sheets and the manifest naming each graphics set's sheet for each enemy.</summary>
    public const string EnemySheetDirectoryName = SuperMetroid.Core.Enemies.Artwork.EnemySheetDefinitions.DirectoryName;
    /// <summary>Content child directory for Samus body PNGs and visual selectors.</summary>
    public const string SamusBodyDirectoryName = "samus-body";
    /// <summary>Content child directory for opening-cinematic artwork.</summary>
    public const string IntroCinematicDirectoryName = "intro-cinematic";
    /// <summary>Content child directory for ending Mode 7 scene artwork.</summary>
    public const string EndingMode7DirectoryName = "ending-mode7";
    /// <summary>Content child directory for ending OBJ artwork.</summary>
    public const string EndingObjectDirectoryName = "ending-objects";
    /// <summary>Content child directory for ending-scene color palettes.</summary>
    public const string EndingPaletteDirectoryName = "ending-palettes";
    /// <summary>Content child directory for stock RGB5 room palettes.</summary>
    public const string RoomPaletteDirectoryName = "room-palettes";
    /// <summary>Content child directory for visual block compositions independent of collision.</summary>
    public const string RoomMetatileDirectoryName = "room-blocks";
    /// <summary>Pre-coloured room block sheets, one per tileset (#1392).</summary>
    public const string RoomBlockSheetDirectoryName = "room-block-sheets";
    /// <summary>Content child directory shared by background tilemaps and sky tilemaps.</summary>
    public const string RoomBackgroundTilemapDirectoryName = "room-backgrounds";
    /// <summary>Content child directory for visual room-block references independent of native collision and BTS.</summary>
    public const string RoomVisualLayoutDirectoryName = "room-layouts";
    /// <summary>Content child directory for stock-only native room-level allocations (collision, BTS, and streaming baseline).</summary>
    public const string RoomLevelStreamDirectoryName = "room-level-streams";
    /// <summary>Content child directory for shootable block appearances.</summary>
    public const string RoomPlmShotBlockVisualDirectoryName = "room-plm-shot-blocks";
    /// <summary>Content child directory for Grapple-reactive block appearances.</summary>
    public const string RoomPlmGrappleBlockVisualDirectoryName = "room-plm-grapple-blocks";
    /// <summary>Content child directory for station appearances; activation and rewards remain compiled.</summary>
    public const string RoomPlmStationVisualDirectoryName = "room-plm-stations";
    /// <summary>Content child directory for blue-door cap artwork.</summary>
    public const string RoomPlmBlueDoorVisualDirectoryName = "room-plm-blue-doors";
    /// <summary>Content child directory for colored-door cap artwork.</summary>
    public const string RoomPlmColoredDoorVisualDirectoryName = "room-plm-colored-doors";
    /// <summary>Content child directory for grey-door caps and shared clear frames.</summary>
    public const string RoomPlmGreyDoorVisualDirectoryName = "room-plm-grey-doors";
    /// <summary>Content child directory for the three eye-door components' artwork.</summary>
    public const string RoomPlmEyeDoorVisualDirectoryName = "room-plm-eye-doors";
    /// <summary>Content child directory for Mother Brain glass appearances.</summary>
    public const string RoomPlmMotherBrainGlassVisualDirectoryName = "room-plm-mother-brain-glass";
    /// <summary>Content child directory for n00b-tube appearances.</summary>
    public const string RoomPlmNoobTubeVisualDirectoryName = "room-plm-noob-tube";
    /// <summary>Content child directory for downward gate-block artwork.</summary>
    public const string RoomPlmDownwardGateVisualDirectoryName = "room-plm-downward-gates";
    /// <summary>Content child directory for elevator-platform frames.</summary>
    public const string RoomPlmElevatorPlatformVisualDirectoryName = "room-plm-elevator-platforms";
    /// <summary>Content child directory for closing escape-gate appearances.</summary>
    public const string RoomPlmEscapeGateVisualDirectoryName = "room-plm-escape-gate";
    /// <summary>Content child directory for Bomb Torizo hand artwork.</summary>
    public const string RoomPlmBombTorizoHandVisualDirectoryName = "room-plm-bomb-torizo-hand";
    /// <summary>Content child directory for reachable Draygon cannon artwork.</summary>
    public const string RoomPlmDraygonCannonVisualDirectoryName = "room-plm-draygon-cannons";
    /// <summary>Content child directory for Chozo hand and slope-access artwork.</summary>
    public const string RoomPlmChozoStatueVisualDirectoryName = "room-plm-chozo-statues";
    /// <summary>Content child directory for linked-block restoration appearances.</summary>
    public const string RoomPlmLinkedRestoreVisualDirectoryName = "room-plm-linked-restores";
    /// <summary>Content child directory for Tourian access-floor artwork.</summary>
    public const string RoomPlmTourianAccessVisualDirectoryName = "room-plm-tourian-access";
    /// <summary>Content child directory for the bomb-revealed Speed Booster tile.</summary>
    public const string RoomPlmSpeedBoosterVisualDirectoryName = "room-plm-speed-booster";
    /// <summary>Content child directory for the Maridia elevatube PLM tile.</summary>
    public const string RoomPlmMaridiaElevatubeVisualDirectoryName = "room-plm-maridia-elevatube";
    /// <summary>Content child directory for Spore Spawn ceiling tiles.</summary>
    public const string RoomPlmSporeSpawnCeilingVisualDirectoryName = "room-plm-spore-spawn-ceiling";
    /// <summary>Content child directory for floor and ceiling plant tiles.</summary>
    public const string RoomPlmSamusEaterVisualDirectoryName = "room-plm-samus-eater";
    /// <summary>Content child directory for Botwoon wall-clear tiles.</summary>
    public const string RoomPlmBotwoonWallVisualDirectoryName = "room-plm-botwoon-wall";
    /// <summary>Content child directory for Kraid ceiling and spike artwork.</summary>
    public const string RoomPlmKraidVisualDirectoryName = "room-plm-kraid";
    /// <summary>Content child directory for Crocomire bridge and wall artwork.</summary>
    public const string RoomPlmCrocomireVisualDirectoryName = "room-plm-crocomire";
    /// <summary>Content child directory for Mother Brain fake-death terrain artwork.</summary>
    public const string RoomPlmMotherBrainFakeDeathVisualDirectoryName = "room-plm-mother-brain-fake-death";
    /// <summary>Content child directory for item, orb, and reveal appearances.</summary>
    public const string RoomPlmCollectibleVisualDirectoryName = "room-plm-collectibles";
    /// <summary>Content child directory for collectible character PNGs and tile-palette selectors.</summary>
    public const string RoomPlmDynamicCollectibleArtDirectoryName = "room-plm-collectible-tiles";
    /// <summary>Content child directory for X-ray reveal metatile selections.</summary>
    public const string XrayRevealVisualDirectoryName = "xray-reveals";
    /// <summary>Receipt filename within stock content, recording the installation format and source cartridge digest.</summary>
    public const string ReceiptFileName = "installation.json";
    /// <summary>Current installation-receipt version; incompatible stock content requires validation and repair before opening.</summary>
    public const int FormatVersion = 127;
    /// <summary>Backup of the previously published stock game tree during an atomic install.</summary>
    internal const string PreviousDirectoryName = ".game.previous";
    /// <summary>Prefix for a stock tree being prepared before publication.</summary>
    internal const string StagingPrefix = ".game.install-";
    /// <summary>Coordinates installers sharing the same installation root.</summary>
    internal const string LockFileName = ".game-install.lock";
}
