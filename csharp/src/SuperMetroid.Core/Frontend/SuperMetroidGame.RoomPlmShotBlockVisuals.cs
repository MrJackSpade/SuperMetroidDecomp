using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Frontend;

public sealed partial class SuperMetroidGame
{
    /// <summary>Nonserialized catalog rebound to the runtime for shootable block overlays.</summary>
    [NonSerialized] private RoomPlmShotBlockVisualCatalog? roomPlmShotBlockVisuals;
    /// <summary>Nonserialized catalog rebound to the runtime for Grapple-beam-reactive block overlays.</summary>
    [NonSerialized] private RoomPlmGrappleBlockVisualCatalog? roomPlmGrappleBlockVisuals;
    /// <summary>Nonserialized catalog rebound to the runtime for station PLM art.</summary>
    [NonSerialized] private RoomPlmStationVisualCatalog? roomPlmStationVisuals;
    /// <summary>Nonserialized catalog rebound to the runtime for blue-door cap art.</summary>
    [NonSerialized] private RoomPlmBlueDoorVisualCatalog? roomPlmBlueDoorVisuals;
    /// <summary>Nonserialized catalog rebound to the runtime for colored-door cap art.</summary>
    [NonSerialized] private RoomPlmColoredDoorVisualCatalog? roomPlmColoredDoorVisuals;
    /// <summary>Nonserialized catalog rebound to the runtime for grey doors and shared clear caps.</summary>
    [NonSerialized] private RoomPlmGreyDoorVisualCatalog? roomPlmGreyDoorVisuals;
    /// <summary>Nonserialized catalog rebound to the runtime for mirrored eye-door graphics.</summary>
    [NonSerialized] private RoomPlmEyeDoorVisualCatalog? roomPlmEyeDoorVisuals;
    /// <summary>Nonserialized catalog rebound to the runtime for Mother Brain glass damage stages.</summary>
    [NonSerialized] private RoomPlmMotherBrainGlassVisualCatalog? roomPlmMotherBrainGlassVisuals;
    /// <summary>Nonserialized catalog rebound to the runtime for n00b-tube art.</summary>
    [NonSerialized] private RoomPlmNoobTubeVisualCatalog? roomPlmNoobTubeVisuals;
    /// <summary>Nonserialized catalog rebound to the runtime for downward-gate block graphics.</summary>
    [NonSerialized] private RoomPlmDownwardGateVisualCatalog? roomPlmDownwardGateVisuals;
    /// <summary>Nonserialized catalog rebound to the runtime for elevator-platform frames.</summary>
    [NonSerialized] private RoomPlmElevatorPlatformVisualCatalog? roomPlmElevatorPlatformVisuals;
    /// <summary>Nonserialized catalog rebound to the runtime for escape-gate graphics.</summary>
    [NonSerialized] private RoomPlmEscapeGateVisualCatalog? roomPlmEscapeGateVisuals;
    /// <summary>Nonserialized catalog rebound to the runtime for Bomb Torizo hand graphics.</summary>
    [NonSerialized] private RoomPlmBombTorizoHandVisualCatalog? roomPlmBombTorizoHandVisuals;
    /// <summary>Nonserialized catalog rebound to the runtime for Draygon's cannon graphics.</summary>
    [NonSerialized] private RoomPlmDraygonCannonVisualCatalog? roomPlmDraygonCannonVisuals;
    /// <summary>Nonserialized catalog rebound to the runtime for Chozo statue terrain changes.</summary>
    [NonSerialized] private RoomPlmChozoStatueVisualCatalog? roomPlmChozoStatueVisuals;
    /// <summary>Nonserialized catalog rebound to the runtime for linked bomb/contact restoration effects.</summary>
    [NonSerialized] private RoomPlmLinkedRestoreVisualCatalog? roomPlmLinkedRestoreVisuals;
    /// <summary>Nonserialized catalog rebound to the runtime for Tourian access-floor art.</summary>
    [NonSerialized] private RoomPlmTourianAccessVisualCatalog? roomPlmTourianAccessVisuals;
    /// <summary>Nonserialized catalog rebound to the runtime for Speed Booster block reveals.</summary>
    [NonSerialized] private RoomPlmSpeedBoosterVisualCatalog? roomPlmSpeedBoosterVisuals;
    /// <summary>Nonserialized catalog rebound to the runtime for the Maridia elevatube tile.</summary>
    [NonSerialized] private RoomPlmMaridiaElevatubeVisualCatalog? roomPlmMaridiaElevatubeVisuals;
    /// <summary>Nonserialized catalog rebound to the runtime for Spore Spawn's ceiling PLM.</summary>
    [NonSerialized] private RoomPlmSporeSpawnCeilingVisualCatalog? roomPlmSporeSpawnCeilingVisuals;
    /// <summary>Nonserialized catalog rebound to the runtime for Samus Eater tile changes.</summary>
    [NonSerialized] private RoomPlmSamusEaterVisualCatalog? roomPlmSamusEaterVisuals;
    /// <summary>Nonserialized catalog rebound to the runtime for Botwoon's wall-clearing effect.</summary>
    [NonSerialized] private RoomPlmBotwoonWallVisualCatalog? roomPlmBotwoonWallVisuals;
    /// <summary>Nonserialized catalog rebound to the runtime for Kraid ceiling and spike appearance.</summary>
    [NonSerialized] private RoomPlmKraidVisualCatalog? roomPlmKraidVisuals;
    /// <summary>Nonserialized catalog rebound to the runtime for Crocomire arena appearance.</summary>
    [NonSerialized] private RoomPlmCrocomireVisualCatalog? roomPlmCrocomireVisuals;
    /// <summary>Nonserialized catalog rebound to the runtime for Mother Brain's fake-death room art.</summary>
    [NonSerialized] private RoomPlmMotherBrainFakeDeathVisualCatalog? roomPlmMotherBrainFakeDeathVisuals;
    /// <summary>Nonserialized catalog rebound to the runtime for standard collectible PLM art.</summary>
    [NonSerialized] private RoomPlmCollectibleVisualCatalog? roomPlmCollectibleVisuals;
    /// <summary>Nonserialized catalog rebound to the runtime for permanent-item images and palettes.</summary>
    [NonSerialized] private RoomPlmDynamicCollectibleArtCatalog? roomPlmDynamicCollectibleArt;

    /// <summary>Binds installed shot-block visuals at startup and after state restoration.</summary>
    public void BindRoomPlmShotBlockVisuals(RoomPlmShotBlockVisualCatalog? catalog)
    {
        roomPlmShotBlockVisuals = catalog;
        if (runtime is not null) runtime.RoomPlmShotBlockVisuals = catalog;
    }

    /// <summary>Binds installed Grapple-block art at startup and after state restoration.</summary>
    public void BindRoomPlmGrappleBlockVisuals(RoomPlmGrappleBlockVisualCatalog? catalog)
    {
        roomPlmGrappleBlockVisuals = catalog;
        if (runtime is not null) runtime.RoomPlmGrappleBlockVisuals = catalog;
    }

    /// <summary>Binds installed station art at startup and after state restoration.</summary>
    public void BindRoomPlmStationVisuals(RoomPlmStationVisualCatalog? catalog)
    {
        roomPlmStationVisuals = catalog;
        if (runtime is not null) runtime.RoomPlmStationVisuals = catalog;
    }

    /// <summary>Binds installed blue-door cap art at startup and after state restoration.</summary>
    public void BindRoomPlmBlueDoorVisuals(RoomPlmBlueDoorVisualCatalog? catalog)
    {
        roomPlmBlueDoorVisuals = catalog;
        if (runtime is not null) runtime.RoomPlmBlueDoorVisuals = catalog;
    }

    /// <summary>Binds installed colored-door cap art at startup and after state restoration.</summary>
    public void BindRoomPlmColoredDoorVisuals(RoomPlmColoredDoorVisualCatalog? catalog)
    {
        roomPlmColoredDoorVisuals = catalog;
        if (runtime is not null) runtime.RoomPlmColoredDoorVisuals = catalog;
    }

    /// <summary>Binds installed grey-door and shared clear-cap art after state restoration.</summary>
    public void BindRoomPlmGreyDoorVisuals(RoomPlmGreyDoorVisualCatalog? catalog)
    {
        roomPlmGreyDoorVisuals = catalog;
        if (runtime is not null) runtime.RoomPlmGreyDoorVisuals = catalog;
    }

    /// <summary>Binds installed mirrored eye-door art after state restoration.</summary>
    public void BindRoomPlmEyeDoorVisuals(RoomPlmEyeDoorVisualCatalog? catalog)
    {
        roomPlmEyeDoorVisuals = catalog;
        if (runtime is not null) runtime.RoomPlmEyeDoorVisuals = catalog;
    }

    /// <summary>Binds installed Mother Brain glass art after state restoration.</summary>
    public void BindRoomPlmMotherBrainGlassVisuals(
        RoomPlmMotherBrainGlassVisualCatalog? catalog)
    {
        roomPlmMotherBrainGlassVisuals = catalog;
        if (runtime is not null) runtime.RoomPlmMotherBrainGlassVisuals = catalog;
    }

    /// <summary>Binds installed n00b-tube art after state restoration.</summary>
    public void BindRoomPlmNoobTubeVisuals(RoomPlmNoobTubeVisualCatalog? catalog)
    {
        roomPlmNoobTubeVisuals = catalog;
        if (runtime is not null) runtime.RoomPlmNoobTubeVisuals = catalog;
    }

    /// <summary>Binds installed downward-gate block art at startup and after state restoration.</summary>
    public void BindRoomPlmDownwardGateVisuals(RoomPlmDownwardGateVisualCatalog? catalog)
    {
        roomPlmDownwardGateVisuals = catalog;
        if (runtime is not null) runtime.RoomPlmDownwardGateVisuals = catalog;
    }

    /// <summary>Binds installed elevator-platform frames at startup and after state restoration.</summary>
    public void BindRoomPlmElevatorPlatformVisuals(RoomPlmElevatorPlatformVisualCatalog? catalog)
    {
        roomPlmElevatorPlatformVisuals = catalog;
        if (runtime is not null) runtime.RoomPlmElevatorPlatformVisuals = catalog;
    }

    /// <summary>Binds installed escape-gate art at startup and after state restoration.</summary>
    public void BindRoomPlmEscapeGateVisuals(RoomPlmEscapeGateVisualCatalog? catalog)
    {
        roomPlmEscapeGateVisuals = catalog;
        if (runtime is not null) runtime.RoomPlmEscapeGateVisuals = catalog;
    }

    /// <summary>Binds installed Bomb Torizo hand art at startup and after state restoration.</summary>
    public void BindRoomPlmBombTorizoHandVisuals(RoomPlmBombTorizoHandVisualCatalog? catalog)
    {
        roomPlmBombTorizoHandVisuals = catalog;
        if (runtime is not null) runtime.RoomPlmBombTorizoHandVisuals = catalog;
    }

    /// <summary>Binds installed Draygon cannon art at startup and after state restoration.</summary>
    public void BindRoomPlmDraygonCannonVisuals(RoomPlmDraygonCannonVisualCatalog? catalog)
    {
        roomPlmDraygonCannonVisuals = catalog;
        if (runtime is not null) runtime.RoomPlmDraygonCannonVisuals = catalog;
    }

    /// <summary>Binds installed Chozo statue terrain art after state restoration.</summary>
    public void BindRoomPlmChozoStatueVisuals(RoomPlmChozoStatueVisualCatalog? catalog)
    {
        roomPlmChozoStatueVisuals = catalog;
        if (runtime is not null) runtime.RoomPlmChozoStatueVisuals = catalog;
    }

    /// <summary>Binds linked bomb/contact restore art at startup and after state restoration.</summary>
    public void BindRoomPlmLinkedRestoreVisuals(RoomPlmLinkedRestoreVisualCatalog? catalog)
    {
        roomPlmLinkedRestoreVisuals = catalog;
        if (runtime is not null) runtime.RoomPlmLinkedRestoreVisuals = catalog;
    }

    /// <summary>Binds Tourian access-floor art at startup and after state restoration.</summary>
    public void BindRoomPlmTourianAccessVisuals(RoomPlmTourianAccessVisualCatalog? catalog)
    {
        roomPlmTourianAccessVisuals = catalog;
        if (runtime is not null) runtime.RoomPlmTourianAccessVisuals = catalog;
    }

    /// <summary>Binds Speed Booster bomb-reveal art after startup or state restoration.</summary>
    public void BindRoomPlmSpeedBoosterVisuals(RoomPlmSpeedBoosterVisualCatalog? catalog)
    {
        roomPlmSpeedBoosterVisuals = catalog;
        if (runtime is not null) runtime.RoomPlmSpeedBoosterVisuals = catalog;
    }

    /// <summary>Binds the installed Maridia elevatube tile after startup or state restoration.</summary>
    public void BindRoomPlmMaridiaElevatubeVisuals(
        RoomPlmMaridiaElevatubeVisualCatalog? catalog)
    {
        roomPlmMaridiaElevatubeVisuals = catalog;
        if (runtime is not null) runtime.RoomPlmMaridiaElevatubeVisuals = catalog;
    }

    /// <summary>Binds Spore Spawn ceiling art after startup or state restoration.</summary>
    public void BindRoomPlmSporeSpawnCeilingVisuals(
        RoomPlmSporeSpawnCeilingVisualCatalog? catalog)
    {
        roomPlmSporeSpawnCeilingVisuals = catalog;
        if (runtime is not null) runtime.RoomPlmSporeSpawnCeilingVisuals = catalog;
    }

    /// <summary>Binds Samus Eater tile art after startup or state restoration.</summary>
    public void BindRoomPlmSamusEaterVisuals(RoomPlmSamusEaterVisualCatalog? catalog)
    {
        roomPlmSamusEaterVisuals = catalog;
        if (runtime is not null) runtime.RoomPlmSamusEaterVisuals = catalog;
    }

    /// <summary>Binds Botwoon wall-clear art after startup or state restoration.</summary>
    public void BindRoomPlmBotwoonWallVisuals(
        RoomPlmBotwoonWallVisualCatalog? catalog)
    {
        roomPlmBotwoonWallVisuals = catalog;
        if (runtime is not null) runtime.RoomPlmBotwoonWallVisuals = catalog;
    }

    /// <summary>Binds Kraid ceiling/spike appearance after startup or state restoration.</summary>
    public void BindRoomPlmKraidVisuals(RoomPlmKraidVisualCatalog? catalog)
    {
        roomPlmKraidVisuals = catalog;
        if (runtime is not null) runtime.RoomPlmKraidVisuals = catalog;
    }

    /// <summary>Binds Crocomire arena appearance after startup or state restoration.</summary>
    public void BindRoomPlmCrocomireVisuals(RoomPlmCrocomireVisualCatalog? catalog)
    {
        roomPlmCrocomireVisuals = catalog;
        if (runtime is not null) runtime.RoomPlmCrocomireVisuals = catalog;
    }

    /// <summary>Binds Mother Brain fake-death room art after startup or state restoration.</summary>
    public void BindRoomPlmMotherBrainFakeDeathVisuals(
        RoomPlmMotherBrainFakeDeathVisualCatalog? catalog)
    {
        roomPlmMotherBrainFakeDeathVisuals = catalog;
        if (runtime is not null) runtime.RoomPlmMotherBrainFakeDeathVisuals = catalog;
    }

    /// <summary>Binds installed collectible art at startup and after state restoration.</summary>
    public void BindRoomPlmCollectibleVisuals(RoomPlmCollectibleVisualCatalog? catalog)
    {
        roomPlmCollectibleVisuals = catalog;
        if (runtime is not null) runtime.RoomPlmCollectibleVisuals = catalog;
    }

    /// <summary>Binds installed permanent-item PNG and palette art after startup or state restore.</summary>
    public void BindRoomPlmDynamicCollectibleArt(
        RoomPlmDynamicCollectibleArtCatalog? catalog)
    {
        roomPlmDynamicCollectibleArt = catalog;
        if (runtime is not null) runtime.RoomPlmDynamicCollectibleArt = catalog;
    }
}
