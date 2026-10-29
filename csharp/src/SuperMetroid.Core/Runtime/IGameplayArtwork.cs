using SuperMetroid.Core.Samus.Abilities.Artwork;
using SuperMetroid.Core.Enemies.Elevator.Artwork;
using SuperMetroid.Core.Enemies.Chozo.Artwork;
using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.Core.Areas;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Bosses.Torizo.Artwork;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Bosses.Draygon.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Bosses.SporeSpawn.Artwork;
using SuperMetroid.Core.Areas.Tourian.Artwork;

namespace SuperMetroid.Core.Runtime;

/// <summary>
/// The installed catalogs a gameplay runtime binds (<see cref="SuperMetroidRuntime.BindArtwork"/>). The host's complete
/// catalog set implements it; the runtime never sees the frontend's cinematic and ending catalogs.
/// </summary>
internal interface IGameplayArtwork
{
    /// <summary>Area maps with the pause, HUD, title, intro and ending text and palettes that share their installation.</summary>
    IGameplayPresentationContent? MapPresentation { get; }

    /// <summary>Gameplay base palettes a new runtime starts from.</summary>
    GameplayBasePalette? GameplayBasePalettes { get; }

    /// <summary>Samus body tiles, palettes and death palettes.</summary>
    InstalledSamusBodyArtwork? SamusBodyArt { get; }

    /// <summary>Room static palettes.</summary>
    RoomStaticPalettes? RoomPaletteArt { get; }

    /// <summary>The pre-coloured block pictures of each tileset (#1392).</summary>
    InstalledRoomArtwork? RoomBlockSheets { get; }

    /// <summary>Initial room foreground and background layouts.</summary>
    RoomLevelArtworks? RoomVisualLayouts { get; }

    /// <summary>Stock native room-level allocations; not overridable.</summary>
    InstalledRoomLevelStreams? RoomLevelStreams { get; }

    /// <summary>Room PLM shot block visuals.</summary>
    InstalledRoomPlmShotBlockVisuals? RoomPlmShotBlockVisuals { get; }

    /// <summary>Room PLM grapple block visuals.</summary>
    InstalledRoomPlmGrappleBlockVisuals? RoomPlmGrappleBlockVisuals { get; }

    /// <summary>Room PLM station visuals.</summary>
    InstalledRoomPlmStationVisuals? RoomPlmStationVisuals { get; }

    /// <summary>Room PLM blue door visuals.</summary>
    InstalledRoomPlmBlueDoorVisuals? RoomPlmBlueDoorVisuals { get; }

    /// <summary>Room PLM colored door visuals.</summary>
    InstalledRoomPlmColoredDoorVisuals? RoomPlmColoredDoorVisuals { get; }

    /// <summary>Room PLM grey door visuals.</summary>
    InstalledRoomPlmGreyDoorVisuals? RoomPlmGreyDoorVisuals { get; }

    /// <summary>Room PLM eye door visuals.</summary>
    InstalledRoomPlmEyeDoorVisuals? RoomPlmEyeDoorVisuals { get; }

    /// <summary>Room PLM mother brain glass visuals.</summary>
    RoomPlmMotherBrainGlassVisualCatalog? RoomPlmMotherBrainGlassVisuals { get; }

    /// <summary>Room PLM noob tube visuals.</summary>
    InstalledRoomPlmNoobTubeVisuals? RoomPlmNoobTubeVisuals { get; }

    /// <summary>Room PLM downward gate visuals.</summary>
    InstalledRoomPlmDownwardGateVisuals? RoomPlmDownwardGateVisuals { get; }

    /// <summary>Room PLM elevator platform visuals.</summary>
    RoomPlmElevatorPlatformVisualCatalog? RoomPlmElevatorPlatformVisuals { get; }

    /// <summary>Room PLM escape gate visuals.</summary>
    RoomPlmEscapeGateVisualCatalog? RoomPlmEscapeGateVisuals { get; }

    /// <summary>Room PLM bomb torizo hand visuals.</summary>
    RoomPlmBombTorizoHandVisualCatalog? RoomPlmBombTorizoHandVisuals { get; }

    /// <summary>Room PLM draygon cannon visuals.</summary>
    RoomPlmDraygonCannonVisualCatalog? RoomPlmDraygonCannonVisuals { get; }

    /// <summary>Room PLM chozo statue visuals.</summary>
    RoomPlmChozoStatueVisualCatalog? RoomPlmChozoStatueVisuals { get; }

    /// <summary>Room PLM linked restore visuals.</summary>
    InstalledRoomPlmLinkedRestoreVisuals? RoomPlmLinkedRestoreVisuals { get; }

    /// <summary>Room PLM tourian access visuals.</summary>
    RoomPlmTourianAccessVisuals? RoomPlmTourianAccessVisuals { get; }

    /// <summary>Room PLM speed booster visuals.</summary>
    InstalledRoomPlmSpeedBoosterVisuals? RoomPlmSpeedBoosterVisuals { get; }

    /// <summary>Room PLM maridia elevatube visuals.</summary>
    InstalledRoomPlmMaridiaElevatubeVisuals? RoomPlmMaridiaElevatubeVisuals { get; }

    /// <summary>Room PLM spore spawn ceiling visuals.</summary>
    RoomPlmSporeSpawnCeilingVisualCatalog? RoomPlmSporeSpawnCeilingVisuals { get; }

    /// <summary>Room PLM samus eater visuals.</summary>
    InstalledRoomPlmSamusEaterVisuals? RoomPlmSamusEaterVisuals { get; }

    /// <summary>Room PLM botwoon wall visuals.</summary>
    InstalledRoomPlmBotwoonWallVisuals? RoomPlmBotwoonWallVisuals { get; }

    /// <summary>Room PLM kraid visuals.</summary>
    InstalledRoomPlmKraidVisuals? RoomPlmKraidVisuals { get; }

    /// <summary>Room PLM crocomire visuals.</summary>
    RoomPlmCrocomireVisualCatalog? RoomPlmCrocomireVisuals { get; }

    /// <summary>Room PLM mother brain fake death visuals.</summary>
    RoomPlmMotherBrainFakeDeathVisualCatalog? RoomPlmMotherBrainFakeDeathVisuals { get; }

    /// <summary>Room PLM collectible visuals.</summary>
    InstalledRoomPlmCollectibleVisuals? RoomPlmCollectibleVisuals { get; }

    /// <summary>X-ray reveal operands and item and special-room overlays.</summary>
    InstalledXrayReveals? XrayRevealVisuals { get; }


    /// <summary>Timed projectile frame sprite choices.</summary>
    InstalledProjectileFrameBindings? ProjectileFrameBindings { get; }

    /// <summary>The cartridge bytes Samus's glitch paths read, extracted from the player's cartridge.</summary>
    InstalledSamusOverreads? SamusOverreads { get; }

    /// <summary>The pre-coloured sprite sheets drawn families select their frames from.</summary>
    InstalledSpriteSheets? SpriteSheets { get; }

    /// <summary>The pre-coloured enemy sheets, by the room graphics set that loads each enemy.</summary>
    InstalledEnemySheets? EnemySheets { get; }

    /// <summary>Charge-flare placement.</summary>
    ChargeFlarePlacements? ChargeFlarePlacement { get; }

    /// <summary>The grapple flare's standing and running offsets.</summary>
    ChargeFlarePlacements? GrappleFlarePlacement { get; }

    /// <summary>The frame Samus's swing pose shows at each grapple angle.</summary>
    InstalledGrappleSwingFrames? GrappleSwingFrames { get; }

    /// <summary>The beam palettes, by beam combination.</summary>
    InstalledBeamPalettes? BeamPalettes { get; }

    /// <summary>The Hyper Beam palette effect's colour cycle.</summary>
    HyperBeamFxColours? HyperBeamFxColors { get; }

    /// <summary>Enemy palettes, extended frames and colour catalogs.</summary>
    EnemyTileArtworkCatalog? EnemyTileArtwork { get; }

    /// <summary>The bosses' melts, BG2 frames, corpse characters and colors.</summary>
    BossArtworkCatalog? BossArtwork { get; }

    /// <summary>Every boss's pre-assembled body frames (#1466).</summary>
    BossBodySheets? BossBodySheets { get; }

    /// <summary>The Ceres door and escape overlay and the Tourian statues' colors.</summary>
    AreaArtworkBundle? AreaArtwork { get; }
}
