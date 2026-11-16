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
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Frontend.Map.Artwork;
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
/// <see cref="SuperMetroidGame.BindArtwork"/>.
/// </summary>
public sealed record GameArtwork
{
    /// <summary>No catalog bound: the state of a new game, or of a restored one before its host rebinds.</summary>
    public static GameArtwork Unbound { get; } = new();

    /// <summary>Area maps with the pause, HUD, title, intro and ending text and colours that share their installation.</summary>
    public AreaMapPresentation? MapPresentation { get; init; }

    /// <summary>The opening's and the Ceres scenes' backdrops and actor placements; their pictures are the sprite sheets (#1616).</summary>
    public IntroCinematicArtwork? IntroCinematicArt { get; init; }

    /// <summary>Samus body pictures, suit colours and death colours.</summary>
    public InstalledSamusBodyArtwork? SamusBodyArt { get; init; }

    /// <summary>The frames Samus's body can show in each pose and the picture frame each draws.</summary>
    public SuperMetroid.Core.Samus.Drawing.SamusShownFrames? SamusShownFrames { get; init; }

    /// <summary>The ending scenes' backdrops (#1544).</summary>
    public EndingBackdrops? EndingBackdrops { get; init; }

    /// <summary>The pre-coloured block pictures of each tileset (#1392).</summary>
    public InstalledRoomArtwork? RoomBlockSheets { get; init; }

    /// <summary>Initial room foreground and background layouts.</summary>
    public RoomLevelArtworks? RoomVisualLayouts { get; init; }

    /// <summary>The installed terrain of every room level; not overridable.</summary>
    public InstalledRoomLevelStreams? RoomLevelStreams { get; init; }

    /// <summary>Room-object shot block visuals.</summary>
    public InstalledRoomObjectShotBlockVisuals? RoomObjectShotBlockVisuals { get; init; }

    /// <summary>Room-object grapple block visuals.</summary>
    public InstalledRoomObjectGrappleBlockVisuals? RoomObjectGrappleBlockVisuals { get; init; }

    /// <summary>Room-object station visuals.</summary>
    public InstalledRoomObjectStationVisuals? RoomObjectStationVisuals { get; init; }

    /// <summary>Room-object blue door visuals.</summary>
    public InstalledRoomObjectBlueDoorVisuals? RoomObjectBlueDoorVisuals { get; init; }

    /// <summary>Room-object colored door visuals.</summary>
    public InstalledRoomObjectColoredDoorVisuals? RoomObjectColoredDoorVisuals { get; init; }

    /// <summary>Room-object grey door visuals.</summary>
    public InstalledRoomObjectGreyDoorVisuals? RoomObjectGreyDoorVisuals { get; init; }

    /// <summary>Room-object eye door visuals.</summary>
    public InstalledRoomObjectEyeDoorVisuals? RoomObjectEyeDoorVisuals { get; init; }

    /// <summary>Room-object mother brain glass visuals.</summary>
    public InstalledMotherBrainGlassFrames? RoomObjectMotherBrainGlassVisuals { get; init; }

    /// <summary>Room-object noob tube visuals.</summary>
    public InstalledRoomObjectNoobTubeVisuals? RoomObjectNoobTubeVisuals { get; init; }

    /// <summary>Room-object downward gate visuals.</summary>
    public InstalledRoomObjectDownwardGateVisuals? RoomObjectDownwardGateVisuals { get; init; }

    /// <summary>Room-object elevator platform visuals.</summary>
    public InstalledRoomObjectElevatorPlatformVisuals? RoomObjectElevatorPlatformVisuals { get; init; }

    /// <summary>Room-object escape gate visuals.</summary>
    public InstalledEscapeGateFrames? RoomObjectEscapeGateVisuals { get; init; }

    /// <summary>Room-object bomb torizo hand visuals.</summary>
    public InstalledRoomObjectBombTorizoHandVisuals? RoomObjectBombTorizoHandVisuals { get; init; }

    /// <summary>Room-object draygon cannon visuals.</summary>
    public InstalledDraygonCannonFrames? RoomObjectDraygonCannonVisuals { get; init; }

    /// <summary>Room-object Phantoon door visuals.</summary>
    public InstalledPhantoonDoorFrames? RoomObjectPhantoonDoorVisuals { get; init; }

    /// <summary>Room-object chozo statue visuals.</summary>
    public InstalledRoomObjectChozoStatueVisuals? RoomObjectChozoStatueVisuals { get; init; }

    /// <summary>Room-object linked restore visuals.</summary>
    public InstalledRoomObjectLinkedRestoreVisuals? RoomObjectLinkedRestoreVisuals { get; init; }

    /// <summary>Room-object tourian access visuals.</summary>
    public RoomObjectTourianAccessVisuals? RoomObjectTourianAccessVisuals { get; init; }

    /// <summary>Room-object speed booster visuals.</summary>
    public InstalledRoomObjectSpeedBoosterVisuals? RoomObjectSpeedBoosterVisuals { get; init; }

    /// <summary>Room-object maridia elevatube visuals.</summary>
    public InstalledRoomObjectMaridiaElevatubeVisuals? RoomObjectMaridiaElevatubeVisuals { get; init; }

    /// <summary>Room-object spore spawn ceiling visuals.</summary>
    public InstalledRoomObjectSporeSpawnCeilingVisuals? RoomObjectSporeSpawnCeilingVisuals { get; init; }

    /// <summary>Room-object samus eater visuals.</summary>
    public InstalledRoomObjectSamusEaterVisuals? RoomObjectSamusEaterVisuals { get; init; }

    /// <summary>Room-object botwoon wall visuals.</summary>
    public InstalledRoomObjectBotwoonWallVisuals? RoomObjectBotwoonWallVisuals { get; init; }

    /// <summary>Room-object kraid visuals.</summary>
    public InstalledRoomObjectKraidVisuals? RoomObjectKraidVisuals { get; init; }

    /// <summary>Room-object crocomire visuals.</summary>
    public InstalledRoomObjectCrocomireVisuals? RoomObjectCrocomireVisuals { get; init; }

    /// <summary>Room-object mother brain fake death visuals.</summary>
    public InstalledMotherBrainFakeDeathFrames? RoomObjectMotherBrainFakeDeathVisuals { get; init; }

    /// <summary>Room-object collectible visuals.</summary>
    public InstalledRoomObjectCollectibleVisuals? RoomObjectCollectibleVisuals { get; init; }

    /// <summary>Room-object bombed-reveal, escape-passage, escape-shaft-wall and rescue-wall visuals.</summary>
    public InstalledRoomObjectRevealAndWallVisuals? RoomObjectRevealAndWallVisuals { get; init; }

    /// <summary>The blocks room-object setups and block reactions write of their own accord.</summary>
    public InstalledRoomObjectSetupBlocks? RoomObjectSetupBlocks { get; init; }

    /// <summary>X-ray reveal shapes and item and special-room overlays.</summary>
    public InstalledXrayReveals? XrayRevealVisuals { get; init; }

    /// <summary>Timed projectile frame sprite choices.</summary>
    public InstalledProjectileFrameBindings? ProjectileFrameBindings { get; init; }


    /// <summary>Kraid's eye colours, which his eye glow reads back (#1512, #1583).</summary>
    public KraidEyeColours? KraidEyes { get; init; }

    /// <summary>Every boss's pre-assembled body frames (#1466).</summary>
    public BossBodySheets? BossBodySheets { get; init; }

    /// <summary>The compatibility services reproducing what Samus's glitch paths read, decoded from the player's game data.</summary>
    public SamusOverreadServices? SamusOverreads { get; init; }

    /// <summary>
    /// The compatibility services reproducing what Kraid's fingernails, Crocomire's volley and the opening cinematic's
    /// egg shell read past their data, decoded from the player's game data.
    /// </summary>
    public BossAndIntroOverreadServices? BossAndIntroOverreads { get; init; }

    /// <summary>The pre-coloured sprite sheets drawn families select their frames from.</summary>
    public InstalledSpriteSheets? SpriteSheets { get; init; }

    /// <summary>The pre-coloured enemy artwork: each variant of each enemy, and the variants each population draws (#1599).</summary>
    public InstalledEnemySheets? EnemySheets { get; init; }

    /// <summary>Every installed enemy frame's display, drawn with the enemy sheets.</summary>
    public EnemyFrameDisplays? EnemyFrameDisplays { get; init; }

    /// <summary>The compatibility services reproducing what enemies read past their authored data, decoded from the player's game data.</summary>
    public EnemyOverreadServices? EnemyOverreads { get; init; }

    /// <summary>Charge-flare placement.</summary>
    public ChargeFlarePlacements? ChargeFlarePlacement { get; init; }

    /// <summary>The grapple flare's standing and running offsets.</summary>
    public ChargeFlarePlacements? GrappleFlarePlacement { get; init; }

    /// <summary>The frame Samus's swing pose shows at each grapple angle.</summary>
    public InstalledGrappleSwingFrames? GrappleSwingFrames { get; init; }

    /// <summary>
    /// The catalogs a gameplay runtime is built over, every one installed (#2049): gameplay starts only once the host has
    /// installed its artwork.
    /// </summary>
    /// <exception cref="InvalidOperationException">A catalog gameplay needs is not installed.</exception>
    internal GameplayArtwork Gameplay => new()
    {
        MapPresentation = Installed(MapPresentation),
        SamusBodyArt = Installed(SamusBodyArt),
        SamusShownFrames = Installed(SamusShownFrames),
        RoomBlockSheets = Installed(RoomBlockSheets),
        RoomVisualLayouts = Installed(RoomVisualLayouts),
        RoomLevelStreams = Installed(RoomLevelStreams),
        RoomObjectShotBlockVisuals = Installed(RoomObjectShotBlockVisuals),
        RoomObjectGrappleBlockVisuals = Installed(RoomObjectGrappleBlockVisuals),
        RoomObjectStationVisuals = Installed(RoomObjectStationVisuals),
        RoomObjectBlueDoorVisuals = Installed(RoomObjectBlueDoorVisuals),
        RoomObjectColoredDoorVisuals = Installed(RoomObjectColoredDoorVisuals),
        RoomObjectGreyDoorVisuals = Installed(RoomObjectGreyDoorVisuals),
        RoomObjectEyeDoorVisuals = Installed(RoomObjectEyeDoorVisuals),
        RoomObjectMotherBrainGlassVisuals = Installed(RoomObjectMotherBrainGlassVisuals),
        RoomObjectNoobTubeVisuals = Installed(RoomObjectNoobTubeVisuals),
        RoomObjectDownwardGateVisuals = Installed(RoomObjectDownwardGateVisuals),
        RoomObjectElevatorPlatformVisuals = Installed(RoomObjectElevatorPlatformVisuals),
        RoomObjectEscapeGateVisuals = Installed(RoomObjectEscapeGateVisuals),
        RoomObjectBombTorizoHandVisuals = Installed(RoomObjectBombTorizoHandVisuals),
        RoomObjectDraygonCannonVisuals = Installed(RoomObjectDraygonCannonVisuals),
        RoomObjectPhantoonDoorVisuals = Installed(RoomObjectPhantoonDoorVisuals),
        RoomObjectChozoStatueVisuals = Installed(RoomObjectChozoStatueVisuals),
        RoomObjectLinkedRestoreVisuals = Installed(RoomObjectLinkedRestoreVisuals),
        RoomObjectTourianAccessVisuals = Installed(RoomObjectTourianAccessVisuals),
        RoomObjectSpeedBoosterVisuals = Installed(RoomObjectSpeedBoosterVisuals),
        RoomObjectMaridiaElevatubeVisuals = Installed(RoomObjectMaridiaElevatubeVisuals),
        RoomObjectSporeSpawnCeilingVisuals = Installed(RoomObjectSporeSpawnCeilingVisuals),
        RoomObjectSamusEaterVisuals = Installed(RoomObjectSamusEaterVisuals),
        RoomObjectBotwoonWallVisuals = Installed(RoomObjectBotwoonWallVisuals),
        RoomObjectKraidVisuals = Installed(RoomObjectKraidVisuals),
        RoomObjectCrocomireVisuals = Installed(RoomObjectCrocomireVisuals),
        RoomObjectMotherBrainFakeDeathVisuals = Installed(RoomObjectMotherBrainFakeDeathVisuals),
        RoomObjectCollectibleVisuals = Installed(RoomObjectCollectibleVisuals),
        RoomObjectRevealAndWallVisuals = Installed(RoomObjectRevealAndWallVisuals),
        RoomObjectSetupBlocks = Installed(RoomObjectSetupBlocks),
        XrayRevealVisuals = Installed(XrayRevealVisuals),
        ProjectileFrameBindings = Installed(ProjectileFrameBindings),
        SamusOverreads = Installed(SamusOverreads),
        BossAndIntroOverreads = Installed(BossAndIntroOverreads),
        SpriteSheets = Installed(SpriteSheets),
        EnemySheets = Installed(EnemySheets),
        EnemyFrameDisplays = Installed(EnemyFrameDisplays),
        EnemyOverreads = Installed(EnemyOverreads),
        ChargeFlarePlacement = Installed(ChargeFlarePlacement),
        GrappleFlarePlacement = Installed(GrappleFlarePlacement),
        GrappleSwingFrames = Installed(GrappleSwingFrames),
        KraidEyes = Installed(KraidEyes),
        BossBodySheets = Installed(BossBodySheets),
    };

    /// <summary>The installed Power Bomb glow colours, which the Ceres station's blast and the flashbacks' bombs glow in.</summary>
    /// <exception cref="InvalidOperationException">No map presentation is installed.</exception>
    internal InstalledPowerBombGlowColors InstalledPowerBombGlowColors => Installed(MapPresentation).PowerBombGlowColors;

    /// <summary>The installed Samus artwork the opening cinematic's flashback Samus draws from (#2049).</summary>
    /// <exception cref="InvalidOperationException">A catalog Samus draws from is not installed.</exception>
    internal SamusArtworkBinding FlashbackSamusArtwork() =>
        new(Installed(SamusBodyArt), Installed(SamusShownFrames), Installed(SpriteSheets), Installed(GrappleFlarePlacement), Installed(GrappleSwingFrames),
            Installed(ProjectileFrameBindings));

    /// <summary>The installed catalog <paramref name="catalog"/>, which gameplay requires.</summary>
    /// <exception cref="InvalidOperationException">The catalog is not installed.</exception>
    private static T Installed<T>(T? catalog, [System.Runtime.CompilerServices.CallerArgumentExpression(nameof(catalog))] string name = "") where T : class =>
        catalog ?? throw new InvalidOperationException($"Gameplay requires the installed {name}.");
}
