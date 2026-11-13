namespace SuperMetroid.AssetExtraction.Installation;

/// <summary>Shared on-disk layout used by the desktop host, Android host, and installation CLI.</summary>
public static class GameInstallationLayout
{
    /// <summary>Replaceable stock-content child directory, atomically published and repaired by the installer.</summary>
    public const string ContentDirectoryName = "game";
    /// <summary>Private validated, unheadered ROM filename retained for asset extraction and repair.</summary>
    public const string RomFileName = "SuperMetroid.smc";
    /// <summary>Content child directory for the installed audio.</summary>
    public const string AudioDirectoryName = "audio";
    /// <summary>Content child directory for area-map artwork and stock exploration masks.</summary>
    public const string MapDirectoryName = "maps";
    /// <summary>Component directory whose import data holds the starting gameplay colours and room-entry sprite colours, which only the import reads (#1637); nothing of it is installed.</summary>
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
    /// <summary>Content child directory for what enemies read past their authored data, decoded from the cartridge (#1500).</summary>
    public const string EnemyOverreadDirectoryName = "enemy-overreads";
    /// <summary>Content child directory for what Kraid's nails, Crocomire's volley and the intro egg shell read past their data.</summary>
    public const string BossAndIntroOverreadDirectoryName = "boss-intro-overreads";
    /// <summary>Content child directory for the terrain a Wave shot's scan finds straddling two blocks, decoded from the level-word layout (#1612).</summary>
    public const string StraddledTerrainDirectoryName = "straddled-terrain";
    /// <summary>Pre-coloured sprite sheets and their frame manifests.</summary>
    public const string SpriteSheetDirectoryName = "sprite-sheets";
    /// <summary>Pre-coloured enemy sheets and the manifest naming each graphics set's sheet for each enemy.</summary>
    public const string EnemySheetDirectoryName = EnemySheetFiles.DirectoryName;
    /// <summary>Content child directory for Samus body PNGs and visual selectors.</summary>
    public const string SamusBodyDirectoryName = "samus-body";
    /// <summary>Content child directory for opening-cinematic artwork.</summary>
    public const string IntroCinematicDirectoryName = "intro-cinematic";
    /// <summary>Content child directory for ending Mode 7 scene artwork.</summary>
    public const string EndingMode7DirectoryName = "ending-mode7";
    /// <summary>Content child directory for ending OBJ artwork.</summary>
    public const string EndingObjectDirectoryName = "ending-objects";
    /// <summary>Content child directory for the ending scenes' backdrops (#1544).</summary>
    public const string EndingBackdropDirectoryName = "ending-backdrops";
    /// <summary>Component directory whose import data holds the rooms' base colours, which only the import reads (#1637); nothing of it is installed.</summary>
    public const string RoomPaletteDirectoryName = "room-palettes";
    /// <summary>Content child directory for visual block compositions independent of collision.</summary>
    public const string RoomMetatileDirectoryName = "room-blocks";
    /// <summary>Pre-coloured room block sheets, one per tileset (#1392).</summary>
    public const string RoomBlockSheetDirectoryName = "room-block-sheets";
    /// <summary>Pre-assembled boss body sheets, one per boss (#1466).</summary>
    public const string BossBodySheetDirectoryName = "boss-body-sheets";
    /// <summary>
    /// Content child directory for the recolour-region maps the picture store's pictures are shown with (#1633, #1644), apart
    /// from the pictures: <c>pictures.regions</c> holds each map once, by its tag, which the frames showing a picture whose
    /// pixels of one colour recolour differently name.
    /// </summary>
    public const string RecolourRegionDirectoryName = "recolour-regions";
    /// <summary>
    /// Content child directory for the blend maps the picture store's pictures are shown with (#1633, #1644), apart from the
    /// pictures: <c>pictures.blendmap</c> holds each map once, by its tag, which the frames showing a picture that blends in
    /// some of its pixels name.
    /// </summary>
    public const string BlendMapDirectoryName = "blend-maps";
    /// <summary>Content child directory shared by background tilemaps and sky tilemaps.</summary>
    public const string RoomBackgroundTilemapDirectoryName = "room-backgrounds";
    /// <summary>Content child directory for visual room-block references independent of native collision and BTS.</summary>
    public const string RoomVisualLayoutDirectoryName = "room-layouts";
    /// <summary>Content child directory for the stock-only typed terrain of every room level (terrain blocks, behaviours, background appearances and the terrain beyond each room).</summary>
    public const string RoomLevelStreamDirectoryName = "room-level-streams";
    /// <summary>Content child directory for shootable block appearances.</summary>
    public const string RoomObjectShotBlockVisualDirectoryName = "room-plm-shot-blocks";
    /// <summary>Content child directory for Grapple-reactive block appearances.</summary>
    public const string RoomObjectGrappleBlockVisualDirectoryName = "room-plm-grapple-blocks";
    /// <summary>Content child directory for station appearances; activation and rewards remain compiled.</summary>
    public const string RoomObjectStationVisualDirectoryName = "room-plm-stations";
    /// <summary>Content child directory for blue-door cap artwork.</summary>
    public const string RoomObjectBlueDoorVisualDirectoryName = "room-plm-blue-doors";
    /// <summary>Content child directory for colored-door cap artwork.</summary>
    public const string RoomObjectColoredDoorVisualDirectoryName = "room-plm-colored-doors";
    /// <summary>Content child directory for grey-door caps and shared clear frames.</summary>
    public const string RoomObjectGreyDoorVisualDirectoryName = "room-plm-grey-doors";
    /// <summary>Content child directory for the three eye-door components' artwork.</summary>
    public const string RoomObjectEyeDoorVisualDirectoryName = "room-plm-eye-doors";
    /// <summary>Content child directory for Mother Brain glass appearances.</summary>
    public const string RoomObjectMotherBrainGlassVisualDirectoryName = "room-plm-mother-brain-glass";
    /// <summary>Content child directory for n00b-tube appearances.</summary>
    public const string RoomObjectNoobTubeVisualDirectoryName = "room-plm-noob-tube";
    /// <summary>Content child directory for downward gate-block artwork.</summary>
    public const string RoomObjectDownwardGateVisualDirectoryName = "room-plm-downward-gates";
    /// <summary>Content child directory for elevator-platform frames.</summary>
    public const string RoomObjectElevatorPlatformVisualDirectoryName = "room-plm-elevator-platforms";
    /// <summary>Content child directory for closing escape-gate appearances.</summary>
    public const string RoomObjectEscapeGateVisualDirectoryName = "room-plm-escape-gate";
    /// <summary>Content child directory for Bomb Torizo hand artwork.</summary>
    public const string RoomObjectBombTorizoHandVisualDirectoryName = "room-plm-bomb-torizo-hand";
    /// <summary>Content child directory for reachable Draygon cannon artwork.</summary>
    public const string RoomObjectDraygonCannonVisualDirectoryName = "room-plm-draygon-cannons";
    /// <summary>Content child directory for Phantoon's fight and restored door artwork.</summary>
    public const string RoomObjectPhantoonDoorVisualDirectoryName = "room-plm-phantoon-door";
    /// <summary>Content child directory for Chozo hand and slope-access artwork.</summary>
    public const string RoomObjectChozoStatueVisualDirectoryName = "room-plm-chozo-statues";
    /// <summary>Content child directory for linked-block restoration appearances.</summary>
    public const string RoomObjectLinkedRestoreVisualDirectoryName = "room-plm-linked-restores";
    /// <summary>Content child directory for Tourian access-floor artwork.</summary>
    public const string RoomObjectTourianAccessVisualDirectoryName = "room-plm-tourian-access";
    /// <summary>Content child directory for the bomb-revealed Speed Booster tile.</summary>
    public const string RoomObjectSpeedBoosterVisualDirectoryName = "room-plm-speed-booster";
    /// <summary>Content child directory for the Maridia elevatube PLM tile.</summary>
    public const string RoomObjectMaridiaElevatubeVisualDirectoryName = "room-plm-maridia-elevatube";
    /// <summary>Content child directory for Spore Spawn ceiling tiles.</summary>
    public const string RoomObjectSporeSpawnCeilingVisualDirectoryName = "room-plm-spore-spawn-ceiling";
    /// <summary>Content child directory for floor and ceiling plant tiles.</summary>
    public const string RoomObjectSamusEaterVisualDirectoryName = "room-plm-samus-eater";
    /// <summary>Content child directory for Botwoon wall-clear tiles.</summary>
    public const string RoomObjectBotwoonWallVisualDirectoryName = "room-plm-botwoon-wall";
    /// <summary>Content child directory for Kraid ceiling and spike artwork.</summary>
    public const string RoomObjectKraidVisualDirectoryName = "room-plm-kraid";
    /// <summary>Content child directory for Crocomire bridge and wall artwork.</summary>
    public const string RoomObjectCrocomireVisualDirectoryName = "room-plm-crocomire";
    /// <summary>Content child directory for Mother Brain fake-death terrain artwork.</summary>
    public const string RoomObjectMotherBrainFakeDeathVisualDirectoryName = "room-plm-mother-brain-fake-death";
    /// <summary>Content child directory for item, orb, and reveal appearances.</summary>
    public const string RoomObjectCollectibleVisualDirectoryName = "room-plm-collectibles";
    /// <summary>Content child directory for the bombed reveals', escape passage's, escape shaft wall's and rescue wall's pictures (#1557).</summary>
    public const string RoomObjectRevealAndWallVisualDirectoryName = "room-plm-reveals-and-walls";
    /// <summary>Content child directory for the pictures of the blocks room-object setups and block reactions write of their own accord (#1580).</summary>
    public const string RoomObjectSetupBlockDirectoryName = "room-plm-setup-blocks";
    /// <summary>Content child directory for X-ray reveal metatile selections.</summary>
    public const string XrayRevealVisualDirectoryName = "xray-reveals";
    /// <summary>
    /// Staging child directory holding the import's character and tile pages (#1636), one child per component that
    /// decodes them (<see cref="ImportDataDirectory.Of"/>): the composition components compose the installed pictures from
    /// them, and the installer deletes the directory before it publishes, so an installation never holds it.
    /// </summary>
    public const string ImportDataDirectoryName = "import-data";
    /// <summary>Receipt filename within stock content, recording the installation format and source cartridge digest.</summary>
    public const string ReceiptFileName = "installation.json";
    /// <summary>Current installation-receipt version; content whose receipt names another is extracted again before opening.</summary>
    public const int FormatVersion = 298;

    /// <summary>Backup of the previously published stock game tree during an atomic install.</summary>
    internal const string PreviousDirectoryName = ".game.previous";
    /// <summary>Prefix for a stock tree being prepared before publication.</summary>
    internal const string StagingPrefix = ".game.install-";
    /// <summary>Coordinates installers sharing the same installation root.</summary>
    internal const string LockFileName = ".game-install.lock";
}
