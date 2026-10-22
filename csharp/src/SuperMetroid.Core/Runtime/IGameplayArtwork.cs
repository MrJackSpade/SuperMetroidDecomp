using SuperMetroid.Core.Samus.Abilities.Artwork;
using SuperMetroid.Core.Enemies.Elevator.Artwork;
using SuperMetroid.Core.Enemies.Chozo.Artwork;
using SuperMetroid.Core.Bosses.Artwork;
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
    GameplayBasePaletteCatalog? GameplayBasePalettes { get; }

    /// <summary>Common gameplay sprite characters (standard objects).</summary>
    RoomCharacterAtlas? StandardObjectArt { get; }

    /// <summary>Samus body tiles, palettes and death palettes.</summary>
    SamusBodyArtworkCatalog? SamusBodyArt { get; }

    /// <summary>Room tileset characters.</summary>
    RoomCharacterAtlasCatalog? RoomCharacterArt { get; }

    /// <summary>Room static palettes.</summary>
    RoomStaticPaletteCatalog? RoomPaletteArt { get; }

    /// <summary>Room metatiles.</summary>
    RoomMetatileCatalog? RoomMetatileArt { get; }

    /// <summary>Initial room foreground and background layouts.</summary>
    RoomVisualLayoutCatalog? RoomVisualLayouts { get; }

    /// <summary>Stock native room-level allocations; not overridable.</summary>
    RoomLevelStreamCatalog? RoomLevelStreams { get; }

    /// <summary>Room PLM shot block visuals.</summary>
    RoomPlmShotBlockVisualCatalog? RoomPlmShotBlockVisuals { get; }

    /// <summary>Room PLM grapple block visuals.</summary>
    RoomPlmGrappleBlockVisualCatalog? RoomPlmGrappleBlockVisuals { get; }

    /// <summary>Room PLM station visuals.</summary>
    RoomPlmStationVisualCatalog? RoomPlmStationVisuals { get; }

    /// <summary>Room PLM blue door visuals.</summary>
    RoomPlmBlueDoorVisualCatalog? RoomPlmBlueDoorVisuals { get; }

    /// <summary>Room PLM colored door visuals.</summary>
    RoomPlmColoredDoorVisualCatalog? RoomPlmColoredDoorVisuals { get; }

    /// <summary>Room PLM grey door visuals.</summary>
    RoomPlmGreyDoorVisualCatalog? RoomPlmGreyDoorVisuals { get; }

    /// <summary>Room PLM eye door visuals.</summary>
    RoomPlmEyeDoorVisualCatalog? RoomPlmEyeDoorVisuals { get; }

    /// <summary>Room PLM mother brain glass visuals.</summary>
    RoomPlmMotherBrainGlassVisualCatalog? RoomPlmMotherBrainGlassVisuals { get; }

    /// <summary>Room PLM noob tube visuals.</summary>
    RoomPlmNoobTubeVisualCatalog? RoomPlmNoobTubeVisuals { get; }

    /// <summary>Room PLM downward gate visuals.</summary>
    RoomPlmDownwardGateVisualCatalog? RoomPlmDownwardGateVisuals { get; }

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
    RoomPlmLinkedRestoreVisualCatalog? RoomPlmLinkedRestoreVisuals { get; }

    /// <summary>Room PLM tourian access visuals.</summary>
    RoomPlmTourianAccessVisualCatalog? RoomPlmTourianAccessVisuals { get; }

    /// <summary>Room PLM speed booster visuals.</summary>
    RoomPlmSpeedBoosterVisualCatalog? RoomPlmSpeedBoosterVisuals { get; }

    /// <summary>Room PLM maridia elevatube visuals.</summary>
    RoomPlmMaridiaElevatubeVisualCatalog? RoomPlmMaridiaElevatubeVisuals { get; }

    /// <summary>Room PLM spore spawn ceiling visuals.</summary>
    RoomPlmSporeSpawnCeilingVisualCatalog? RoomPlmSporeSpawnCeilingVisuals { get; }

    /// <summary>Room PLM samus eater visuals.</summary>
    RoomPlmSamusEaterVisualCatalog? RoomPlmSamusEaterVisuals { get; }

    /// <summary>Room PLM botwoon wall visuals.</summary>
    RoomPlmBotwoonWallVisualCatalog? RoomPlmBotwoonWallVisuals { get; }

    /// <summary>Room PLM kraid visuals.</summary>
    RoomPlmKraidVisualCatalog? RoomPlmKraidVisuals { get; }

    /// <summary>Room PLM crocomire visuals.</summary>
    RoomPlmCrocomireVisualCatalog? RoomPlmCrocomireVisuals { get; }

    /// <summary>Room PLM mother brain fake death visuals.</summary>
    RoomPlmMotherBrainFakeDeathVisualCatalog? RoomPlmMotherBrainFakeDeathVisuals { get; }

    /// <summary>Room PLM collectible visuals.</summary>
    RoomPlmCollectibleVisualCatalog? RoomPlmCollectibleVisuals { get; }

    /// <summary>Room PLM dynamic collectible artwork.</summary>
    RoomPlmDynamicCollectibleArtCatalog? RoomPlmDynamicCollectibleArt { get; }

    /// <summary>X-ray reveal operands and item and special-room overlays.</summary>
    XrayRevealVisualCatalog? XrayRevealVisuals { get; }

    /// <summary>Room background tilemaps.</summary>
    RoomBackgroundTilemapCatalog? RoomBackgroundTilemapArt { get; }

    /// <summary>Room sky tilemaps.</summary>
    RoomSkyTilemapCatalog? RoomSkyTilemapArt { get; }

    /// <summary>Samus projectile sprite compositions.</summary>
    ProjectileSpriteCatalog? ProjectileCompositions { get; }

    /// <summary>Timed projectile frame sprite choices.</summary>
    ProjectileFrameBindingCatalog? ProjectileFrameBindings { get; }

    /// <summary>Projectile trail artwork.</summary>
    ProjectileTrailCatalog? TrailArtwork { get; }

    /// <summary>The cartridge bytes Samus's glitch paths read, extracted from the player's cartridge.</summary>
    SamusOverreadCatalog? SamusOverreads { get; }

    /// <summary>The pre-coloured sprite sheets drawn families select their frames from.</summary>
    InstalledSpriteSheets? SpriteSheets { get; }

    /// <summary>The pre-coloured enemy sheets, by the room graphics set that loads each enemy.</summary>
    InstalledEnemySheets? EnemySheets { get; }

    /// <summary>Charge-flare placement.</summary>
    ChargeFlarePlacementCatalog? ChargeFlarePlacement { get; }

    /// <summary>Charge-flare sprite compositions.</summary>
    ChargeFlareSpriteCatalog? ChargeFlareCompositions { get; }

    /// <summary>Grapple beam tiles.</summary>
    GrappleTileAtlas? GrappleArtwork { get; }

    /// <summary>Beam tiles.</summary>
    BeamTileCatalog? BeamArtwork { get; }

    /// <summary>Enemy sprite sheets, palettes and enemy-projectile spritemaps.</summary>
    EnemyTileArtworkCatalog? EnemyTileArtwork { get; }

    /// <summary>The bosses' melts, skeletons, BG2 frames, special pages and colors.</summary>
    BossArtworkCatalog? BossArtwork { get; }

    /// <summary>The Ceres door and escape artwork and the Tourian statues' colors.</summary>
    AreaArtworkCatalog? AreaArtwork { get; }
}
