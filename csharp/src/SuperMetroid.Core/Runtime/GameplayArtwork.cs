using SuperMetroid.Core.Samus.Abilities.Artwork;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.Core.Runtime;

/// <summary>
/// The installed catalogs a gameplay runtime is built over and binds (#2049): every one is required, since the host
/// puts its artwork in place before gameplay builds anything that draws (<see cref="Frontend.GameArtwork.Gameplay"/>).
/// The runtime never
/// sees the frontend's cinematic and ending catalogs.
/// </summary>
internal sealed partial record GameplayArtwork
{
    /// <summary>The installed content of the screens, scenes and HUD (#2203): the area maps, the menus, the title, the opening and ending text and the scenes' colours.</summary>
    public required IGameplayPresentationContent ScreenContent { get; init; }

    /// <summary>Samus body pictures, colours and death colours.</summary>
    public required InstalledSamusBodyArtwork SamusBodyArt { get; init; }

    /// <summary>The frames Samus's body can show in each pose and the picture frame each draws.</summary>
    public required SuperMetroid.Core.Samus.Drawing.SamusShownFrames SamusShownFrames { get; init; }

    /// <summary>The pre-coloured block pictures of each tileset (#1392).</summary>
    public required InstalledRoomArtwork RoomBlockSheets { get; init; }

    /// <summary>Initial room foreground and background layouts.</summary>
    public required RoomLevelArtworks RoomVisualLayouts { get; init; }

    /// <summary>The installed terrain of every room level; not overridable.</summary>
    public required InstalledRoomLevelStreams RoomLevelStreams { get; init; }

    /// <summary>The installed pictures of every room-object family's blocks, which the room's room objects draw (#2290).</summary>
    public required SuperMetroid.Core.World.RoomObjectArtwork RoomObjects { get; init; }

    /// <summary>X-ray reveal shapes and item and special-room overlays.</summary>
    public required InstalledXrayReveals XrayRevealVisuals { get; init; }

    /// <summary>The composition every frame of Samus's shots' animations draws.</summary>
    public required InstalledProjectileFrameBindings ProjectileFrameBindings { get; init; }

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

    /// <summary>Where the beam's charge flare is drawn from Samus in each pose.</summary>
    public required BeamFlarePlacements ChargeFlarePlacement { get; init; }

    /// <summary>The grapple flare's standing and running offsets.</summary>
    public required ChargeFlarePlacements GrappleFlarePlacement { get; init; }

    /// <summary>The frame Samus's swing pose shows at each grapple angle.</summary>
    public required InstalledGrappleSwingFrames GrappleSwingFrames { get; init; }

    /// <summary>Kraid's eye colours, which his eye glow reads back (#1512, #1583).</summary>
    public required KraidEyeColours KraidEyes { get; init; }

    /// <summary>Every boss's pre-assembled body frames (#1466).</summary>
    public required BossBodySheets BossBodySheets { get; init; }
}
