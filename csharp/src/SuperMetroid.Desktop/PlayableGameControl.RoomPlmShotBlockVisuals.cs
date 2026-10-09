namespace SuperMetroid.Desktop;

public sealed partial class PlayableGameControl
{
    /// <summary>Visual definitions for PLMs that break in response to weapon shots.</summary>
    private SuperMetroid.Core.Rooms.RoomPlmShotBlockVisualCatalog? roomPlmShotBlockVisuals;
    /// <summary>Visual definitions for grapple-reactive blocks, including their attached and detached presentation.</summary>
    private SuperMetroid.Core.Rooms.RoomPlmGrappleBlockVisualCatalog? roomPlmGrappleBlockVisuals;
    /// <summary>Presentation data for save and recharge stations placed as room PLMs.</summary>
    private SuperMetroid.Core.Rooms.RoomPlmStationVisualCatalog? roomPlmStationVisuals;
    /// <summary>Presentation data for standard blue room doors.</summary>
    private SuperMetroid.Core.Rooms.RoomPlmBlueDoorVisualCatalog? roomPlmBlueDoorVisuals;
    /// <summary>Presentation data for doors whose color and opening state select their PLM graphics.</summary>
    private SuperMetroid.Core.Rooms.RoomPlmColoredDoorVisualCatalog? roomPlmColoredDoorVisuals;
    /// <summary>Presentation data for grey doors that remain sealed until their room condition is met.</summary>
    private SuperMetroid.Core.Rooms.RoomPlmGreyDoorVisualCatalog? roomPlmGreyDoorVisuals;
    /// <summary>Presentation data for the eye-door sequence, including its animated opening states.</summary>
    private SuperMetroid.Core.Rooms.RoomPlmEyeDoorVisualCatalog? roomPlmEyeDoorVisuals;
    /// <summary>Graphics and state presentation for Mother Brain's glass tube PLM.</summary>
    private SuperMetroid.Core.Rooms.RoomPlmMotherBrainGlassVisualCatalog? roomPlmMotherBrainGlassVisuals;
    /// <summary>Presentation data for the destructible Maridia tube and its broken state.</summary>
    private SuperMetroid.Core.Rooms.RoomPlmNoobTubeVisualCatalog? roomPlmNoobTubeVisuals;
    /// <summary>Visual definitions for gates that open downward as the room mechanism advances.</summary>
    private SuperMetroid.Core.Rooms.RoomPlmDownwardGateVisualCatalog? roomPlmDownwardGateVisuals;
    /// <summary>Presentation data for room elevator platforms and their movement frames.</summary>
    private SuperMetroid.Core.Rooms.RoomPlmElevatorPlatformVisualCatalog? roomPlmElevatorPlatformVisuals;
    /// <summary>Visual definitions for the timed escape sequence gates.</summary>
    private SuperMetroid.Core.Rooms.RoomPlmEscapeGateVisualCatalog? roomPlmEscapeGateVisuals;
    /// <summary>Presentation data for Bomb Torizo's hand PLM and its animation states.</summary>
    private SuperMetroid.Core.Rooms.RoomPlmBombTorizoHandVisualCatalog? roomPlmBombTorizoHandVisuals;
    /// <summary>Presentation data for Draygon's cannon PLM and its room-dependent states.</summary>
    private SuperMetroid.Core.Rooms.RoomPlmDraygonCannonVisualCatalog? roomPlmDraygonCannonVisuals;
    /// <summary>Visual definitions for Chozo statues, including the item-reveal presentation.</summary>
    private SuperMetroid.Core.Rooms.RoomPlmChozoStatueVisualCatalog? roomPlmChozoStatueVisuals;
    /// <summary>Presentation data for linked PLMs that restore or update together.</summary>
    private SuperMetroid.Core.Rooms.RoomPlmLinkedRestoreVisualCatalog? roomPlmLinkedRestoreVisuals;
    /// <summary>Visual definitions for PLMs that control access into Tourian.</summary>
    private SuperMetroid.Core.Rooms.RoomPlmTourianAccessVisualCatalog? roomPlmTourianAccessVisuals;
    /// <summary>Presentation data for Speed Booster blocks and their breakable states.</summary>
    private SuperMetroid.Core.Rooms.RoomPlmSpeedBoosterVisualCatalog? roomPlmSpeedBoosterVisuals;
    /// <summary>Visual definitions for the Maridia elevatube and its changing tube state.</summary>
    private SuperMetroid.Core.Rooms.RoomPlmMaridiaElevatubeVisualCatalog? roomPlmMaridiaElevatubeVisuals;
    /// <summary>Presentation data for Spore Spawn's ceiling PLM and its animated states.</summary>
    private SuperMetroid.Core.Rooms.RoomPlmSporeSpawnCeilingVisualCatalog? roomPlmSporeSpawnCeilingVisuals;
    /// <summary>Visual definitions for the Samus Eater PLM and its capture or release presentation.</summary>
    private SuperMetroid.Core.Rooms.RoomPlmSamusEaterVisualCatalog? roomPlmSamusEaterVisuals;
    /// <summary>Presentation data for Botwoon's wall PLM and its changing wall state.</summary>
    private SuperMetroid.Core.Rooms.RoomPlmBotwoonWallVisualCatalog? roomPlmBotwoonWallVisuals;
    /// <summary>Visual definitions for Kraid's room PLMs and related encounter presentation.</summary>
    private SuperMetroid.Core.Rooms.RoomPlmKraidVisualCatalog? roomPlmKraidVisuals;
    /// <summary>Visual definitions for Crocomire's room PLMs and related encounter presentation.</summary>
    private SuperMetroid.Core.Rooms.RoomPlmCrocomireVisualCatalog? roomPlmCrocomireVisuals;
    /// <summary>Presentation data for Mother Brain's fake-death sequence PLMs.</summary>
    private SuperMetroid.Core.Rooms.RoomPlmMotherBrainFakeDeathVisualCatalog? roomPlmMotherBrainFakeDeathVisuals;
    /// <summary>Visual definitions for room item collectibles and their collected state.</summary>
    private SuperMetroid.Core.Rooms.RoomPlmCollectibleVisualCatalog? roomPlmCollectibleVisuals;
    /// <summary>Graphics data for collectible PLMs whose art is supplied dynamically at runtime.</summary>
    private SuperMetroid.Core.Rooms.RoomPlmDynamicCollectibleArtCatalog? roomPlmDynamicCollectibleArt;
}
