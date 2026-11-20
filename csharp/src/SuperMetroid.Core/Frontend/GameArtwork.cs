using SuperMetroid.Core.Frontend.Artwork;
using SuperMetroid.Core.Samus.Abilities.Artwork;
using SuperMetroid.Core.Enemies.Chozo.Artwork;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Bosses.Torizo.Artwork;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Bosses.Draygon.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Bosses.Phantoon.Artwork;
using SuperMetroid.Core.Bosses.SporeSpawn.Artwork;
using SuperMetroid.Core.Areas.Tourian.Artwork;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Every installed presentation catalog the frontend binds: maps, room and room-object art, cinematic, ending, Samus,
/// enemy and projectile artwork. Hosts load one set from an installation and bind it with
/// <see cref="SuperMetroidGame.BindArtwork"/>. Every catalog is installed (#2224): a screen's art is never absent, so a
/// rebinding replaces each catalog with another installed one and never with nothing.
/// </summary>
public sealed record GameArtwork
{
    /// <summary>The installed content of the screens, scenes and HUD (#2203): the area maps, the menus, the title, the opening and ending text and the scenes' colours.</summary>
    public required InstalledScreenContent ScreenContent { get; init; }

    /// <summary>The opening's and the Ceres scenes' backdrops and actor placements; their pictures are the sprite sheets (#1616).</summary>
    public required IntroCinematicArtwork IntroCinematicArt { get; init; }

    /// <summary>Samus body pictures, suit colours and death colours.</summary>
    public required InstalledSamusBodyArtwork SamusBodyArt { get; init; }

    /// <summary>The frames Samus's body can show in each pose and the picture frame each draws.</summary>
    public required SuperMetroid.Core.Samus.Drawing.SamusShownFrames SamusShownFrames { get; init; }

    /// <summary>The ending scenes' backdrops (#1544).</summary>
    public required EndingBackdrops EndingBackdrops { get; init; }

    /// <summary>The pre-coloured block pictures of each tileset (#1392).</summary>
    public required InstalledRoomArtwork RoomBlockSheets { get; init; }

    /// <summary>Initial room foreground and background layouts.</summary>
    public required RoomLevelArtworks RoomVisualLayouts { get; init; }

    /// <summary>The installed terrain of every room level; not overridable.</summary>
    public required InstalledRoomLevelStreams RoomLevelStreams { get; init; }

    /// <summary>Room-object shot block visuals.</summary>
    public required InstalledRoomObjectShotBlockVisuals RoomObjectShotBlockVisuals { get; init; }

    /// <summary>Room-object grapple block visuals.</summary>
    public required InstalledRoomObjectGrappleBlockVisuals RoomObjectGrappleBlockVisuals { get; init; }

    /// <summary>Room-object station visuals.</summary>
    public required InstalledRoomObjectStationVisuals RoomObjectStationVisuals { get; init; }

    /// <summary>Room-object blue door visuals.</summary>
    public required InstalledRoomObjectBlueDoorVisuals RoomObjectBlueDoorVisuals { get; init; }

    /// <summary>Room-object colored door visuals.</summary>
    public required InstalledRoomObjectColoredDoorVisuals RoomObjectColoredDoorVisuals { get; init; }

    /// <summary>Room-object grey door visuals.</summary>
    public required InstalledRoomObjectGreyDoorVisuals RoomObjectGreyDoorVisuals { get; init; }

    /// <summary>Room-object eye door visuals.</summary>
    public required InstalledRoomObjectEyeDoorVisuals RoomObjectEyeDoorVisuals { get; init; }

    /// <summary>Room-object mother brain glass visuals.</summary>
    public required InstalledMotherBrainGlassFrames RoomObjectMotherBrainGlassVisuals { get; init; }

    /// <summary>Room-object noob tube visuals.</summary>
    public required InstalledRoomObjectNoobTubeVisuals RoomObjectNoobTubeVisuals { get; init; }

    /// <summary>Room-object downward gate visuals.</summary>
    public required InstalledRoomObjectDownwardGateVisuals RoomObjectDownwardGateVisuals { get; init; }

    /// <summary>Room-object elevator platform visuals.</summary>
    public required InstalledRoomObjectElevatorPlatformVisuals RoomObjectElevatorPlatformVisuals { get; init; }

    /// <summary>Room-object escape gate visuals.</summary>
    public required InstalledEscapeGateFrames RoomObjectEscapeGateVisuals { get; init; }

    /// <summary>Room-object bomb torizo hand visuals.</summary>
    public required InstalledRoomObjectBombTorizoHandVisuals RoomObjectBombTorizoHandVisuals { get; init; }

    /// <summary>Room-object draygon cannon visuals.</summary>
    public required InstalledDraygonCannonFrames RoomObjectDraygonCannonVisuals { get; init; }

    /// <summary>Room-object Phantoon door visuals.</summary>
    public required InstalledPhantoonDoorFrames RoomObjectPhantoonDoorVisuals { get; init; }

    /// <summary>Room-object chozo statue visuals.</summary>
    public required InstalledRoomObjectChozoStatueVisuals RoomObjectChozoStatueVisuals { get; init; }

    /// <summary>Room-object linked restore visuals.</summary>
    public required InstalledRoomObjectLinkedRestoreVisuals RoomObjectLinkedRestoreVisuals { get; init; }

    /// <summary>Room-object tourian access visuals.</summary>
    public required RoomObjectTourianAccessVisuals RoomObjectTourianAccessVisuals { get; init; }

    /// <summary>Room-object speed booster visuals.</summary>
    public required InstalledRoomObjectSpeedBoosterVisuals RoomObjectSpeedBoosterVisuals { get; init; }

    /// <summary>Room-object maridia elevatube visuals.</summary>
    public required InstalledRoomObjectMaridiaElevatubeVisuals RoomObjectMaridiaElevatubeVisuals { get; init; }

    /// <summary>Room-object spore spawn ceiling visuals.</summary>
    public required InstalledRoomObjectSporeSpawnCeilingVisuals RoomObjectSporeSpawnCeilingVisuals { get; init; }

    /// <summary>Room-object samus eater visuals.</summary>
    public required InstalledRoomObjectSamusEaterVisuals RoomObjectSamusEaterVisuals { get; init; }

    /// <summary>Room-object botwoon wall visuals.</summary>
    public required InstalledRoomObjectBotwoonWallVisuals RoomObjectBotwoonWallVisuals { get; init; }

    /// <summary>Room-object kraid visuals.</summary>
    public required InstalledRoomObjectKraidVisuals RoomObjectKraidVisuals { get; init; }

    /// <summary>Room-object crocomire visuals.</summary>
    public required InstalledRoomObjectCrocomireVisuals RoomObjectCrocomireVisuals { get; init; }

    /// <summary>Room-object mother brain fake death visuals.</summary>
    public required InstalledMotherBrainFakeDeathFrames RoomObjectMotherBrainFakeDeathVisuals { get; init; }

    /// <summary>Room-object collectible visuals.</summary>
    public required InstalledRoomObjectCollectibleVisuals RoomObjectCollectibleVisuals { get; init; }

    /// <summary>Room-object bombed-reveal, escape-passage, escape-shaft-wall and rescue-wall visuals.</summary>
    public required InstalledRoomObjectRevealAndWallVisuals RoomObjectRevealAndWallVisuals { get; init; }

    /// <summary>The blocks room-object setups and block reactions write of their own accord.</summary>
    public required InstalledRoomObjectSetupBlocks RoomObjectSetupBlocks { get; init; }

    /// <summary>X-ray reveal shapes and item and special-room overlays.</summary>
    public required InstalledXrayReveals XrayRevealVisuals { get; init; }

    /// <summary>Timed projectile frame sprite choices.</summary>
    public required InstalledProjectileFrameBindings ProjectileFrameBindings { get; init; }

    /// <summary>Kraid's eye colours, which his eye glow reads back (#1512, #1583).</summary>
    public required KraidEyeColours KraidEyes { get; init; }

    /// <summary>Every boss's pre-assembled body frames (#1466).</summary>
    public required BossBodySheets BossBodySheets { get; init; }

    /// <summary>The compatibility services reproducing what Samus's glitch paths read, decoded from the player's game data.</summary>
    public required SamusOverreadServices SamusOverreads { get; init; }

    /// <summary>
    /// The compatibility services reproducing what Kraid's fingernails, Crocomire's volley and the opening cinematic's
    /// egg shell read past their data, decoded from the player's game data.
    /// </summary>
    public required BossAndIntroOverreadServices BossAndIntroOverreads { get; init; }

    /// <summary>
    /// The compatibility service reproducing what each explicit scroll layout's load writes past the room's screens,
    /// decoded from the player's game data (#2356).
    /// </summary>
    public required ScrollCellsPastRoomScreens<RoomScrollState> ScrollCellsPastRoomScreens { get; init; }

    /// <summary>The pre-coloured sprite sheets drawn families select their frames from.</summary>
    public required InstalledSpriteSheets SpriteSheets { get; init; }

    /// <summary>The pre-coloured enemy artwork: each variant of each enemy, and the variants each population draws (#1599).</summary>
    public required InstalledEnemySheets EnemySheets { get; init; }

    /// <summary>Every installed enemy frame's display, drawn with the enemy sheets.</summary>
    public required EnemyFrameDisplays EnemyFrameDisplays { get; init; }

    /// <summary>The compatibility services reproducing what enemies read past their authored data, decoded from the player's game data.</summary>
    public required EnemyOverreadServices EnemyOverreads { get; init; }

    /// <summary>Charge-flare placement.</summary>
    public required BeamFlarePlacements ChargeFlarePlacement { private get; init; }

    /// <summary>The grapple flare's standing and running offsets.</summary>
    public required ChargeFlarePlacements GrappleFlarePlacement { private get; init; }

    /// <summary>The frame Samus's swing pose shows at each grapple angle.</summary>
    public required InstalledGrappleSwingFrames GrappleSwingFrames { private get; init; }

    /// <summary>
    /// The catalogs a gameplay runtime is built over (#2049).
    /// </summary>
    internal GameplayArtwork Gameplay => new()
    {
        ScreenContent = ScreenContent,
        SamusBodyArt = SamusBodyArt,
        SamusShownFrames = SamusShownFrames,
        RoomBlockSheets = RoomBlockSheets,
        RoomVisualLayouts = RoomVisualLayouts,
        RoomLevelStreams = RoomLevelStreams,
        RoomObjects = new SuperMetroid.Core.World.RoomObjectArtwork
        {
            RoomObjectShotBlockVisuals = RoomObjectShotBlockVisuals,
            RoomObjectGrappleBlockVisuals = RoomObjectGrappleBlockVisuals,
            RoomObjectStationVisuals = RoomObjectStationVisuals,
            RoomObjectBlueDoorVisuals = RoomObjectBlueDoorVisuals,
            RoomObjectColoredDoorVisuals = RoomObjectColoredDoorVisuals,
            RoomObjectGreyDoorVisuals = RoomObjectGreyDoorVisuals,
            RoomObjectEyeDoorVisuals = RoomObjectEyeDoorVisuals,
            RoomObjectMotherBrainGlassVisuals = RoomObjectMotherBrainGlassVisuals,
            RoomObjectNoobTubeVisuals = RoomObjectNoobTubeVisuals,
            RoomObjectDownwardGateVisuals = RoomObjectDownwardGateVisuals,
            RoomObjectElevatorPlatformVisuals = RoomObjectElevatorPlatformVisuals,
            RoomObjectEscapeGateVisuals = RoomObjectEscapeGateVisuals,
            RoomObjectBombTorizoHandVisuals = RoomObjectBombTorizoHandVisuals,
            RoomObjectDraygonCannonVisuals = RoomObjectDraygonCannonVisuals,
            RoomObjectPhantoonDoorVisuals = RoomObjectPhantoonDoorVisuals,
            RoomObjectChozoStatueVisuals = RoomObjectChozoStatueVisuals,
            RoomObjectLinkedRestoreVisuals = RoomObjectLinkedRestoreVisuals,
            RoomObjectTourianAccessVisuals = RoomObjectTourianAccessVisuals,
            RoomObjectSpeedBoosterVisuals = RoomObjectSpeedBoosterVisuals,
            RoomObjectMaridiaElevatubeVisuals = RoomObjectMaridiaElevatubeVisuals,
            RoomObjectSporeSpawnCeilingVisuals = RoomObjectSporeSpawnCeilingVisuals,
            RoomObjectSamusEaterVisuals = RoomObjectSamusEaterVisuals,
            RoomObjectBotwoonWallVisuals = RoomObjectBotwoonWallVisuals,
            RoomObjectKraidVisuals = RoomObjectKraidVisuals,
            RoomObjectCrocomireVisuals = RoomObjectCrocomireVisuals,
            RoomObjectMotherBrainFakeDeathVisuals = RoomObjectMotherBrainFakeDeathVisuals,
            RoomObjectCollectibleVisuals = RoomObjectCollectibleVisuals,
            RoomObjectRevealAndWallVisuals = RoomObjectRevealAndWallVisuals,
            RoomObjectSetupBlocks = RoomObjectSetupBlocks,
        },
        XrayRevealVisuals = XrayRevealVisuals,
        ProjectileFrameBindings = ProjectileFrameBindings,
        SamusOverreads = SamusOverreads,
        BossAndIntroOverreads = BossAndIntroOverreads,
        ScrollCellsPastRoomScreens = ScrollCellsPastRoomScreens,
        SpriteSheets = SpriteSheets,
        EnemySheets = EnemySheets,
        EnemyFrameDisplays = EnemyFrameDisplays,
        EnemyOverreads = EnemyOverreads,
        ChargeFlarePlacement = ChargeFlarePlacement,
        GrappleFlarePlacement = GrappleFlarePlacement,
        GrappleSwingFrames = GrappleSwingFrames,
        KraidEyes = KraidEyes,
        BossBodySheets = BossBodySheets,
    };

    /// <summary>The installed Samus artwork the opening cinematic's flashback Samus draws from (#2049).</summary>
    internal SamusArtworkBinding FlashbackSamusArtwork() =>
        new(SamusBodyArt, SamusShownFrames, SpriteSheets, GrappleFlarePlacement, GrappleSwingFrames,
            ProjectileFrameBindings, ScreenContent.PowerBombGlowColors);
}
