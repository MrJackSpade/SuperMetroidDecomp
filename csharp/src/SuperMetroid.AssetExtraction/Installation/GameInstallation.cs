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
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Frontend.Intro.Artwork;
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
using SuperMetroid.Core.Bosses.Phantoon.Artwork;
using SuperMetroid.Core.Bosses.SporeSpawn.Artwork;
using SuperMetroid.Core.Areas.Tourian.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

namespace SuperMetroid.AssetExtraction.Installation;

/// <summary>App-owned immutable cartridge/audio content, separate from persistent player data.</summary>
/// <param name="Root">Installation root containing the replaceable <c>game</c> content and persistent player data; constructing this record does not create or validate it.</param>
/// <remarks>
/// Directory properties compute paths without accessing the filesystem. Loaders validate the
/// installed formats. Missing files, unreadable paths, and malformed installed content
/// propagate the corresponding I/O or invalid-data exception to the caller.
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
    /// <summary>Stock decoded results of Samus's glitch reads, which are not presentation the player edits.</summary>
    public string SamusOverreadDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.SamusOverreadDirectoryName);
    /// <summary>
    /// Loads the validated compatibility services reproducing Samus's glitch reads, with the installed straddled terrain
    /// her Wave shots' scan past the room's ceiling finds.
    /// </summary>
    public SamusOverreadServices LoadSamusOverreads() => SamusOverreadFiles.Load(SamusOverreadDirectory, LoadStraddledTerrain());
    /// <summary>Stock decoded terrain a Wave shot's scan finds straddling two blocks, which is not presentation the player edits.</summary>
    public string StraddledTerrainDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.StraddledTerrainDirectoryName);
    /// <summary>Loads the validated terrain a Wave shot's scan past the room's ceiling finds straddling two blocks.</summary>
    public SuperMetroid.Core.Compatibility.StraddledTerrain<SuperMetroid.Core.Rooms.LevelData.RoomTerrainBlock> LoadStraddledTerrain() => StraddledTerrainFiles.Load(StraddledTerrainDirectory);
    /// <summary>Stock decoded results of what enemies read past their authored data, which are not presentation the player edits.</summary>
    public string EnemyOverreadDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.EnemyOverreadDirectoryName);
    /// <summary>Loads the validated compatibility services reproducing what enemies read past their authored data.</summary>
    public EnemyOverreadServices LoadEnemyOverreads() => EnemyOverreadFiles.Load(EnemyOverreadDirectory);
    /// <summary>Stock decoded results of Kraid's, Crocomire's and the intro egg shell's reads past their data, which are not presentation the player edits.</summary>
    public string BossAndIntroOverreadDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.BossAndIntroOverreadDirectoryName);
    /// <summary>Loads the validated compatibility services reproducing Kraid's, Crocomire's and the intro egg shell's reads past their data.</summary>
    public SuperMetroid.Core.Compatibility.BossAndIntroOverreadServices LoadBossAndIntroOverreads() => BossAndIntroOverreadFiles.Load(BossAndIntroOverreadDirectory);
    /// <summary>Installed sprite sheet manifests, which name where the picture store holds each frame's picture.</summary>
    public string SpriteSheetDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.SpriteSheetDirectoryName);
    /// <summary>Loads every installed sprite sheet.</summary>
    public InstalledSpriteSheets LoadSpriteSheets() => SpriteSheetFiles.Load(SpriteSheetDirectory);
    /// <summary>Installed pre-coloured enemy sheets and the manifest naming each graphics set's sheet for each enemy.</summary>
    public string EnemySheetDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.EnemySheetDirectoryName);
    /// <summary>Loads every installed enemy sheet.</summary>
    public InstalledEnemySheets LoadEnemySheets() => EnemySheetFiles.Load(EnemySheetDirectory);
    /// <summary>Loads the installed artwork only the import reads, which the installed content's identity covers (#1616).</summary>
    /// <param name="samusBody">The Samus body the host loaded once and bound the game's body catalog from.</param>
    public InstalledArtworkSources LoadArtworkSources(SamusBodySources samusBody) => new()
    {
        SamusBody = Ensure.NotNull(samusBody),
        Intro = LoadIntroCinematicArt(),
    };
    /// <summary>Installed projectile artwork and presentation selectors, which the player edits in place.</summary>
    public string ProjectileDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.ProjectileDirectoryName);
    /// <summary>Installed Samus body PNGs and visual selectors, which the player edits in place.</summary>
    public string SamusBodyDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.SamusBodyDirectoryName);
    /// <summary>Loads the validated installed Samus body: the tables, compositions and linked artwork it is composed from.</summary>
    public SamusBodySources LoadSamusBody() => SamusBodyArtworkFiles.Load(SamusBodyDirectory);
    /// <summary>Installed opening-cinematic artwork, which the player edits in place.</summary>
    public string IntroCinematicDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.IntroCinematicDirectoryName);
    /// <summary>Loads the installed opening-cinematic artwork, which only the import reads.</summary>
    public IntroCinematicSourceArtwork LoadIntroCinematicArt() =>
        IntroCinematicArtworkFiles.Load(IntroCinematicDirectory);
    /// <summary>Loads what the game binds of the installed opening cinematic: its backdrops and the Ceres scenes' placements (#1616).</summary>
    public IntroCinematicArtwork LoadIntroCinematic() =>
        IntroCinematicArtworkFiles.LoadCinematic(IntroCinematicDirectory);
    /// <summary>
    /// The ending object component's directory, which holds only its provenance manifest: its maps and compositions are
    /// import data (#1638) and the installation holds the pictures composed from them.
    /// </summary>
    public string EndingObjectDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.EndingObjectDirectoryName);
    /// <summary>Installed ending scene backdrops, which the player edits in place (#1544).</summary>
    public string EndingBackdropDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.EndingBackdropDirectoryName);
    /// <summary>Loads the installed ending scene backdrops.</summary>
    public EndingBackdrops LoadEndingBackdrops() =>
        EndingBackdropFiles.Load(EndingBackdropDirectory);
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
    public string RoomObjectShotBlockVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomObjectShotBlockVisualDirectoryName);
    /// <summary>Editable shot-block appearances; PLM timing, placement, and collision remain compiled.</summary>
    public InstalledRoomObjectShotBlockVisuals LoadRoomObjectShotBlockVisuals() =>
        RoomObjectShotBlockVisualFiles.Load(RoomObjectShotBlockVisualDirectory);
    /// <summary>Stock definitions for Grapple-reactive block appearances under the replaceable content tree.</summary>
    public string RoomObjectGrappleBlockVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomObjectGrappleBlockVisualDirectoryName);
    /// <summary>Editable Grapple-block appearances; timing, placement, and collision remain compiled.</summary>
    public InstalledRoomObjectGrappleBlockVisuals LoadRoomObjectGrappleBlockVisuals() =>
        RoomObjectGrappleBlockVisualFiles.Load(RoomObjectGrappleBlockVisualDirectory);
    /// <summary>Stock definitions for save, refill, and map station appearances under the replaceable content tree.</summary>
    public string RoomObjectStationVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomObjectStationVisualDirectoryName);
    /// <summary>Editable station appearances; activation, rewards, and collision remain compiled.</summary>
    public InstalledRoomObjectStationVisuals LoadRoomObjectStationVisuals() =>
        RoomObjectStationVisualFiles.Load(RoomObjectStationVisualDirectory);
    /// <summary>Stock definitions for blue-door cap frames under the replaceable content tree.</summary>
    public string RoomObjectBlueDoorVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomObjectBlueDoorVisualDirectoryName);
    /// <summary>Editable blue-door cap art; opening timing and physical tiles remain compiled.</summary>
    public InstalledRoomObjectBlueDoorVisuals LoadRoomObjectBlueDoorVisuals() =>
        RoomObjectBlueDoorVisualFiles.Load(RoomObjectBlueDoorVisualDirectory);
    /// <summary>Stock definitions for colored-door cap frames under the replaceable content tree.</summary>
    public string RoomObjectColoredDoorVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomObjectColoredDoorVisualDirectoryName);
    /// <summary>Editable colored-door cap art; hit rules and physical tiles remain compiled.</summary>
    public InstalledRoomObjectColoredDoorVisuals LoadRoomObjectColoredDoorVisuals() =>
        RoomObjectColoredDoorVisualFiles.Load(RoomObjectColoredDoorVisualDirectory);
    /// <summary>Stock definitions for grey-door caps and shared clear frames under the replaceable content tree.</summary>
    public string RoomObjectGreyDoorVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomObjectGreyDoorVisualDirectoryName);
    /// <summary>Editable grey caps and shared clear frames; physical door rules stay compiled.</summary>
    public InstalledRoomObjectGreyDoorVisuals LoadRoomObjectGreyDoorVisuals() =>
        RoomObjectGreyDoorVisualFiles.Load(RoomObjectGreyDoorVisualDirectory);
    /// <summary>Stock definitions for the three eye-door components' appearances under the replaceable content tree.</summary>
    public string RoomObjectEyeDoorVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomObjectEyeDoorVisualDirectoryName);
    /// <summary>Editable eye-door blocks; the three PLM components retain compiled behavior.</summary>
    public InstalledRoomObjectEyeDoorVisuals LoadRoomObjectEyeDoorVisuals() =>
        RoomObjectEyeDoorVisualFiles.Load(RoomObjectEyeDoorVisualDirectory);
    /// <summary>Stock definitions for Mother Brain glass appearances under the replaceable content tree.</summary>
    public string RoomObjectMotherBrainGlassVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomObjectMotherBrainGlassVisualDirectoryName);
    /// <summary>Loads the installed pictures of Mother Brain's glass frames as the player left them.</summary>
    public InstalledMotherBrainGlassFrames LoadRoomObjectMotherBrainGlassVisuals() =>
        RoomObjectMotherBrainGlassVisualFiles.Load(RoomObjectMotherBrainGlassVisualDirectory);
    /// <summary>Stock definitions for intact and broken n00b-tube appearances under the replaceable content tree.</summary>
    public string RoomObjectNoobTubeVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomObjectNoobTubeVisualDirectoryName);
    /// <summary>Editable n00b-tube appearance; break logic and physical blocks stay compiled.</summary>
    public InstalledRoomObjectNoobTubeVisuals LoadRoomObjectNoobTubeVisuals() =>
        RoomObjectNoobTubeVisualFiles.Load(RoomObjectNoobTubeVisualDirectory);
    /// <summary>Stock definitions for downward gate-block appearances under the replaceable content tree.</summary>
    public string RoomObjectDownwardGateVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomObjectDownwardGateVisualDirectoryName);
    /// <summary>Editable gate-block art; activation, animation, and collision remain compiled.</summary>
    public InstalledRoomObjectDownwardGateVisuals LoadRoomObjectDownwardGateVisuals() =>
        RoomObjectDownwardGateVisualFiles.Load(RoomObjectDownwardGateVisualDirectory);
    /// <summary>Stock definitions for elevator-platform frames under the replaceable content tree.</summary>
    public string RoomObjectElevatorPlatformVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomObjectElevatorPlatformVisualDirectoryName);
    /// <summary>Editable elevator frames; physical blocks and the instruction loop stay compiled.</summary>
    public InstalledRoomObjectElevatorPlatformVisuals LoadRoomObjectElevatorPlatformVisuals() =>
        RoomObjectElevatorPlatformVisualFiles.Load(RoomObjectElevatorPlatformVisualDirectory);
    /// <summary>Stock definitions for closing escape-gate appearances under the replaceable content tree.</summary>
    public string RoomObjectEscapeGateVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomObjectEscapeGateVisualDirectoryName);
    /// <summary>Loads the installed pictures of Mother Brain's escape-gate frames as the player left them.</summary>
    public InstalledEscapeGateFrames LoadRoomObjectEscapeGateVisuals() =>
        RoomObjectEscapeGateVisualFiles.Load(RoomObjectEscapeGateVisualDirectory);
    /// <summary>Stock definitions for Bomb Torizo hand appearances under the replaceable content tree.</summary>
    public string RoomObjectBombTorizoHandVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomObjectBombTorizoHandVisualDirectoryName);
    /// <summary>Editable Bomb Torizo hand art; its physical block words stay compiled.</summary>
    public InstalledRoomObjectBombTorizoHandVisuals LoadRoomObjectBombTorizoHandVisuals() =>
        RoomObjectBombTorizoHandVisualFiles.Load(RoomObjectBombTorizoHandVisualDirectory);
    /// <summary>Stock definitions for reachable Draygon cannon appearances under the replaceable content tree.</summary>
    public string RoomObjectDraygonCannonVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomObjectDraygonCannonVisualDirectoryName);
    /// <summary>Loads the installed pictures of Draygon's cannon frames as the player left them.</summary>
    public InstalledDraygonCannonFrames LoadRoomObjectDraygonCannonVisuals() =>
        RoomObjectDraygonCannonVisualFiles.Load(RoomObjectDraygonCannonVisualDirectory);
    /// <summary>Stock definitions for Phantoon's fight and restored door appearances under the replaceable content tree.</summary>
    public string RoomObjectPhantoonDoorVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomObjectPhantoonDoorVisualDirectoryName);
    /// <summary>Loads the installed pictures of Phantoon's door frames as the player left them.</summary>
    public InstalledPhantoonDoorFrames LoadRoomObjectPhantoonDoorVisuals() =>
        RoomObjectPhantoonDoorVisualFiles.Load(RoomObjectPhantoonDoorVisualDirectory);
    /// <summary>Stock definitions for Chozo hand and slope-access appearances under the replaceable content tree.</summary>
    public string RoomObjectChozoStatueVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomObjectChozoStatueVisualDirectoryName);
    /// <summary>Editable Chozo hand and slope-access art; physical blocks remain compiled.</summary>
    public InstalledRoomObjectChozoStatueVisuals LoadRoomObjectChozoStatueVisuals() =>
        RoomObjectChozoStatueVisualFiles.Load(RoomObjectChozoStatueVisualDirectory);
    /// <summary>Stock definitions for linked-block restoration appearances under the replaceable content tree.</summary>
    public string RoomObjectLinkedRestoreVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomObjectLinkedRestoreVisualDirectoryName);
    /// <summary>Editable linked-block restoration art; collision stays compiled.</summary>
    public InstalledRoomObjectLinkedRestoreVisuals LoadRoomObjectLinkedRestoreVisuals() =>
        RoomObjectLinkedRestoreVisualFiles.Load(RoomObjectLinkedRestoreVisualDirectory);
    /// <summary>Stock definitions for Tourian access-floor appearances under the replaceable content tree.</summary>
    public string RoomObjectTourianAccessVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomObjectTourianAccessVisualDirectoryName);
    /// <summary>Editable Tourian access-floor art; physical rows and timing stay compiled.</summary>
    public RoomObjectTourianAccessVisuals LoadRoomObjectTourianAccessVisuals() =>
        RoomObjectTourianAccessVisualFiles.Load(RoomObjectTourianAccessVisualDirectory);
    /// <summary>Stock definitions for the bomb-revealed Speed Booster tile under the replaceable content tree.</summary>
    public string RoomObjectSpeedBoosterVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomObjectSpeedBoosterVisualDirectoryName);
    /// <summary>Editable bomb-revealed Speed Booster tile; collision stays compiled.</summary>
    public InstalledRoomObjectSpeedBoosterVisuals LoadRoomObjectSpeedBoosterVisuals() =>
        RoomObjectSpeedBoosterVisualFiles.Load(RoomObjectSpeedBoosterVisualDirectory);
    /// <summary>Stock definitions for the Maridia elevatube PLM tile under the replaceable content tree.</summary>
    public string RoomObjectMaridiaElevatubeVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomObjectMaridiaElevatubeVisualDirectoryName);
    /// <summary>Editable Maridia elevatube PLM tile; its physical block stays compiled.</summary>
    public InstalledRoomObjectMaridiaElevatubeVisuals LoadRoomObjectMaridiaElevatubeVisuals() =>
        RoomObjectMaridiaElevatubeVisualFiles.Load(RoomObjectMaridiaElevatubeVisualDirectory);
    /// <summary>Stock definitions for Spore Spawn ceiling tiles under the replaceable content tree.</summary>
    public string RoomObjectSporeSpawnCeilingVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomObjectSporeSpawnCeilingVisualDirectoryName);
    /// <summary>Editable Spore Spawn ceiling tiles; physical draw words stay compiled.</summary>
    public InstalledRoomObjectSporeSpawnCeilingVisuals LoadRoomObjectSporeSpawnCeilingVisuals() =>
        RoomObjectSporeSpawnCeilingVisualFiles.Load(RoomObjectSporeSpawnCeilingVisualDirectory);
    /// <summary>Stock definitions for floor and ceiling plant tiles under the replaceable content tree.</summary>
    public string RoomObjectSamusEaterVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomObjectSamusEaterVisualDirectoryName);
    /// <summary>Editable floor/ceiling plant tiles; physical draw geometry stays compiled.</summary>
    public InstalledRoomObjectSamusEaterVisuals LoadRoomObjectSamusEaterVisuals() =>
        RoomObjectSamusEaterVisualFiles.Load(RoomObjectSamusEaterVisualDirectory);
    /// <summary>Stock definitions for Botwoon wall-clear tiles under the replaceable content tree.</summary>
    public string RoomObjectBotwoonWallVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomObjectBotwoonWallVisualDirectoryName);
    /// <summary>Editable Botwoon wall-clear tiles; collision and crumble timing stay compiled.</summary>
    public InstalledRoomObjectBotwoonWallVisuals LoadRoomObjectBotwoonWallVisuals() =>
        RoomObjectBotwoonWallVisualFiles.Load(RoomObjectBotwoonWallVisualDirectory);
    /// <summary>Stock definitions for Kraid ceiling and spike appearances under the replaceable content tree.</summary>
    public string RoomObjectKraidVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomObjectKraidVisualDirectoryName);
    /// <summary>Editable Kraid ceiling/spike art; physical mutations stay compiled.</summary>
    public InstalledRoomObjectKraidVisuals LoadRoomObjectKraidVisuals() =>
        RoomObjectKraidVisualFiles.Load(RoomObjectKraidVisualDirectory);
    /// <summary>Stock definitions for Crocomire bridge and wall appearances under the replaceable content tree.</summary>
    public string RoomObjectCrocomireVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomObjectCrocomireVisualDirectoryName);
    /// <summary>Editable Crocomire bridge/wall art; collision and timing stay compiled.</summary>
    public InstalledRoomObjectCrocomireVisuals LoadRoomObjectCrocomireVisuals() =>
        RoomObjectCrocomireVisualFiles.Load(RoomObjectCrocomireVisualDirectory);
    /// <summary>Stock definitions for Mother Brain fake-death terrain appearances under the replaceable content tree.</summary>
    public string RoomObjectMotherBrainFakeDeathVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomObjectMotherBrainFakeDeathVisualDirectoryName);
    /// <summary>Loads the installed pictures of Mother Brain's fake-death room frames as the player left them.</summary>
    public InstalledMotherBrainFakeDeathFrames LoadRoomObjectMotherBrainFakeDeathVisuals() =>
        RoomObjectMotherBrainFakeDeathVisualFiles.Load(
            RoomObjectMotherBrainFakeDeathVisualDirectory);
    /// <summary>Stock definitions for item, orb, and reveal appearances under the replaceable content tree.</summary>
    public string RoomObjectCollectibleVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomObjectCollectibleVisualDirectoryName);
    /// <summary>Editable item/orb/reveal appearances; pickup and collision remain compiled.</summary>
    public InstalledRoomObjectCollectibleVisuals LoadRoomObjectCollectibleVisuals() =>
        RoomObjectCollectibleVisualFiles.Load(RoomObjectCollectibleVisualDirectory);
    /// <summary>The installed pictures of the bombed reveals, the escape passage, the escape shaft wall and the rescue wall.</summary>
    public string RoomObjectRevealAndWallVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomObjectRevealAndWallVisualDirectoryName);
    /// <summary>Loads the bombed reveals', escape passage's, escape shaft wall's and rescue wall's pictures as installed; their terrain stays the game's.</summary>
    public InstalledRoomObjectRevealAndWallVisuals LoadRoomObjectRevealAndWallVisuals() =>
        RoomObjectRevealAndWallVisualFiles.Load(RoomObjectRevealAndWallVisualDirectory);
    /// <summary>The installed pictures of the blocks room-object setups and block reactions write of their own accord.</summary>
    public string RoomObjectSetupBlockDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomObjectSetupBlockDirectoryName);
    /// <summary>Loads the setup blocks' pictures as installed; how each collides stays the game's.</summary>
    public InstalledRoomObjectSetupBlocks LoadRoomObjectSetupBlocks() =>
        RoomObjectSetupBlockFiles.Load(RoomObjectSetupBlockDirectory);
    /// <summary>Stock definitions for X-ray reveal metatile selections under the replaceable content tree.</summary>
    public string XrayRevealVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.XrayRevealVisualDirectoryName);
    /// <summary>Editable X-ray metatile choices; reveal commands and collision rules remain compiled.</summary>
    public InstalledXrayReveals LoadXrayRevealVisuals() =>
        XrayRevealVisualFiles.Load(XrayRevealVisualDirectory);
    /// <summary>Loads the validated installed projectile presentation.</summary>
    public InstalledProjectilePresentation LoadProjectiles() => ProjectilePresentationFiles.Load(ProjectileDirectory);
    /// <summary>Installed enemy, boss and area documents, which the player edits in place.</summary>
    public string EnemyTileDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.EnemyTileDirectoryName);
    /// <summary>Loads what the game binds of the installed enemy documents: every frame's display and Kraid's colours (#1636).</summary>
    public InstalledEnemyArtwork LoadEnemyTiles() => EnemyTileArtworkFiles.Load(EnemyTileDirectory);

    /// <summary>
    /// Loads the installed maps, exploration masks and menu presentation without a cartridge address space: the catalog the
    /// game binds, after the whole installed presentation has been read and validated (#1616).
    /// </summary>
    public AreaMapPresentation LoadMaps() => MapPresentationFiles.Load(MapDirectory).Game;

    /// <summary>
    /// Loads every installed presentation catalog the frontend binds. Hosts pass the maps, projectile presentation, enemy
    /// directory and Samus body they already hold for content identity and save persistence; of the body, the frontend
    /// binds what the game reads.
    /// </summary>
    public GameArtwork LoadGameArtwork(AreaMapPresentation maps, InstalledProjectilePresentation projectiles, InstalledEnemyArtwork enemies,
        SamusBodySources samusBody)
    {
        Ensure.NotNull(maps);
        Ensure.NotNull(projectiles);
        Ensure.NotNull(enemies);
        Ensure.NotNull(samusBody);
        return new GameArtwork
        {
            MapPresentation = maps,
            IntroCinematicArt = LoadIntroCinematic(),
            SamusBodyArt = samusBody.Gameplay,

            EndingBackdrops = LoadEndingBackdrops(),
            RoomBlockSheets = LoadRoomBlockSheets(),
            RoomVisualLayouts = LoadRoomVisualLayouts(),
            RoomLevelStreams = LoadRoomLevelStreams(),
            RoomObjectShotBlockVisuals = LoadRoomObjectShotBlockVisuals(),
            RoomObjectGrappleBlockVisuals = LoadRoomObjectGrappleBlockVisuals(),
            RoomObjectStationVisuals = LoadRoomObjectStationVisuals(),
            RoomObjectBlueDoorVisuals = LoadRoomObjectBlueDoorVisuals(),
            RoomObjectColoredDoorVisuals = LoadRoomObjectColoredDoorVisuals(),
            RoomObjectGreyDoorVisuals = LoadRoomObjectGreyDoorVisuals(),
            RoomObjectEyeDoorVisuals = LoadRoomObjectEyeDoorVisuals(),
            RoomObjectMotherBrainGlassVisuals = LoadRoomObjectMotherBrainGlassVisuals(),
            RoomObjectNoobTubeVisuals = LoadRoomObjectNoobTubeVisuals(),
            RoomObjectDownwardGateVisuals = LoadRoomObjectDownwardGateVisuals(),
            RoomObjectElevatorPlatformVisuals = LoadRoomObjectElevatorPlatformVisuals(),
            RoomObjectEscapeGateVisuals = LoadRoomObjectEscapeGateVisuals(),
            RoomObjectBombTorizoHandVisuals = LoadRoomObjectBombTorizoHandVisuals(),
            RoomObjectDraygonCannonVisuals = LoadRoomObjectDraygonCannonVisuals(),
            RoomObjectPhantoonDoorVisuals = LoadRoomObjectPhantoonDoorVisuals(),
            RoomObjectChozoStatueVisuals = LoadRoomObjectChozoStatueVisuals(),
            RoomObjectLinkedRestoreVisuals = LoadRoomObjectLinkedRestoreVisuals(),
            RoomObjectTourianAccessVisuals = LoadRoomObjectTourianAccessVisuals(),
            RoomObjectSpeedBoosterVisuals = LoadRoomObjectSpeedBoosterVisuals(),
            RoomObjectMaridiaElevatubeVisuals = LoadRoomObjectMaridiaElevatubeVisuals(),
            RoomObjectSporeSpawnCeilingVisuals = LoadRoomObjectSporeSpawnCeilingVisuals(),
            RoomObjectSamusEaterVisuals = LoadRoomObjectSamusEaterVisuals(),
            RoomObjectBotwoonWallVisuals = LoadRoomObjectBotwoonWallVisuals(),
            RoomObjectKraidVisuals = LoadRoomObjectKraidVisuals(),
            RoomObjectCrocomireVisuals = LoadRoomObjectCrocomireVisuals(),
            RoomObjectMotherBrainFakeDeathVisuals = LoadRoomObjectMotherBrainFakeDeathVisuals(),
            RoomObjectCollectibleVisuals = LoadRoomObjectCollectibleVisuals(),
            RoomObjectRevealAndWallVisuals = LoadRoomObjectRevealAndWallVisuals(),
            RoomObjectSetupBlocks = LoadRoomObjectSetupBlocks(),
            XrayRevealVisuals = LoadXrayRevealVisuals(),
            ProjectileFrameBindings = projectiles.FrameBindings,

            KraidEyes = enemies.KraidEyes,
            ChargeFlarePlacement = projectiles.FlarePlacement,
            GrappleFlarePlacement = projectiles.Grapple.FlarePlacement,
            GrappleSwingFrames = projectiles.Grapple.SwingFrames,
            SamusOverreads = LoadSamusOverreads(),
            EnemyOverreads = LoadEnemyOverreads(),
            BossAndIntroOverreads = LoadBossAndIntroOverreads(),
            SpriteSheets = LoadSpriteSheets(),
            EnemySheets = LoadEnemySheets(),
            EnemyFrameDisplays = enemies.FrameDisplays,
            BossBodySheets = LoadBossBodySheets(),
        };
    }
}
