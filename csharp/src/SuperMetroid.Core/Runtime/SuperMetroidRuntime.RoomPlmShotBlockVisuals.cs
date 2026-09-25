using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Runtime;

public sealed partial class SuperMetroidRuntime
{
    /// <summary>Visual-only shot-block resources; reattached after debugger-state restoration.</summary>
    public RoomPlmShotBlockVisualCatalog? RoomPlmShotBlockVisuals
    {
        get => Plms.ShotBlockVisuals;
        set => Plms.ShotBlockVisuals = value;
    }

    /// <summary>Visual-only Grapple-block resources; reattached after debugger-state restoration.</summary>
    public RoomPlmGrappleBlockVisualCatalog? RoomPlmGrappleBlockVisuals
    {
        get => Plms.GrappleBlockVisuals;
        set => Plms.GrappleBlockVisuals = value;
    }

    /// <summary>Visual-only station resources; reattached after debugger-state restoration.</summary>
    public RoomPlmStationVisualCatalog? RoomPlmStationVisuals
    {
        get => Plms.StationVisuals;
        set => Plms.StationVisuals = value;
    }

    /// <summary>Visual-only blue-door cap resources; reattached after debugger-state restoration.</summary>
    public RoomPlmBlueDoorVisualCatalog? RoomPlmBlueDoorVisuals
    {
        get => Plms.BlueDoorVisuals;
        set => Plms.BlueDoorVisuals = value;
    }

    /// <summary>Visual-only colored-door resources; reattached after debugger-state restoration.</summary>
    public RoomPlmColoredDoorVisualCatalog? RoomPlmColoredDoorVisuals
    {
        get => Plms.ColoredDoorVisuals;
        set => Plms.ColoredDoorVisuals = value;
    }

    /// <summary>Visual-only grey-door and shared clear-cap resources; reattached after state restoration.</summary>
    public RoomPlmGreyDoorVisualCatalog? RoomPlmGreyDoorVisuals
    {
        get => Plms.GreyDoorVisuals;
        set => Plms.GreyDoorVisuals = value;
    }

    /// <summary>Visual-only eye-door resources; reattached after state restoration.</summary>
    public RoomPlmEyeDoorVisualCatalog? RoomPlmEyeDoorVisuals
    {
        get => Plms.EyeDoorVisuals;
        set => Plms.EyeDoorVisuals = value;
    }

    /// <summary>Visual-only Mother Brain glass resources; reattached after state restoration.</summary>
    public RoomPlmMotherBrainGlassVisualCatalog? RoomPlmMotherBrainGlassVisuals
    {
        get => Plms.MotherBrainGlassVisuals;
        set => Plms.MotherBrainGlassVisuals = value;
    }

    /// <summary>Visual-only n00b-tube resources; reattached after state restoration.</summary>
    public RoomPlmNoobTubeVisualCatalog? RoomPlmNoobTubeVisuals
    {
        get => Plms.NoobTubeVisuals;
        set => Plms.NoobTubeVisuals = value;
    }

    /// <summary>Visual-only gate-block resources; reattached after debugger-state restoration.</summary>
    public RoomPlmDownwardGateVisualCatalog? RoomPlmDownwardGateVisuals
    {
        get => Plms.DownwardGateVisuals;
        set => Plms.DownwardGateVisuals = value;
    }

    /// <summary>Visual-only escape-gate resources; reattached after debugger-state restoration.</summary>
    public RoomPlmEscapeGateVisualCatalog? RoomPlmEscapeGateVisuals
    {
        get => Plms.EscapeGateVisuals;
        set => Plms.EscapeGateVisuals = value;
    }

    /// <summary>Visual-only Bomb Torizo hand resource; reattached after state restoration.</summary>
    public RoomPlmBombTorizoHandVisualCatalog? RoomPlmBombTorizoHandVisuals
    {
        get => Plms.BombTorizoHandVisuals;
        set => Plms.BombTorizoHandVisuals = value;
    }

    /// <summary>Visual-only Draygon cannon resource; reattached after state restoration.</summary>
    public RoomPlmDraygonCannonVisualCatalog? RoomPlmDraygonCannonVisuals
    {
        get => Plms.DraygonCannonVisuals;
        set => Plms.DraygonCannonVisuals = value;
    }

    /// <summary>Visual-only Chozo statue terrain resources; reattached after state restoration.</summary>
    public RoomPlmChozoStatueVisualCatalog? RoomPlmChozoStatueVisuals
    {
        get => Plms.ChozoStatueVisuals;
        set => Plms.ChozoStatueVisuals = value;
    }

    /// <summary>Visual-only linked restoration resource; reattached after state restoration.</summary>
    public RoomPlmLinkedRestoreVisualCatalog? RoomPlmLinkedRestoreVisuals
    {
        get => Plms.LinkedRestoreVisuals;
        set => Plms.LinkedRestoreVisuals = value;
    }

    /// <summary>Visual-only Tourian access-floor resource; reattached after state restoration.</summary>
    public RoomPlmTourianAccessVisualCatalog? RoomPlmTourianAccessVisuals
    {
        get => Plms.TourianAccessVisuals;
        set => Plms.TourianAccessVisuals = value;
    }

    public RoomPlmSpeedBoosterVisualCatalog? RoomPlmSpeedBoosterVisuals
    {
        get => Plms.SpeedBoosterVisuals;
        set => Plms.SpeedBoosterVisuals = value;
    }

    public RoomPlmMaridiaElevatubeVisualCatalog? RoomPlmMaridiaElevatubeVisuals
    {
        get => Plms.MaridiaElevatubeVisuals;
        set => Plms.MaridiaElevatubeVisuals = value;
    }

    public RoomPlmSporeSpawnCeilingVisualCatalog? RoomPlmSporeSpawnCeilingVisuals
    {
        get => Plms.SporeSpawnCeilingVisuals;
        set => Plms.SporeSpawnCeilingVisuals = value;
    }

    public RoomPlmBotwoonWallVisualCatalog? RoomPlmBotwoonWallVisuals
    {
        get => Plms.BotwoonWallVisuals;
        set => Plms.BotwoonWallVisuals = value;
    }

    public RoomPlmKraidVisualCatalog? RoomPlmKraidVisuals
    {
        get => Plms.KraidVisuals;
        set => Plms.KraidVisuals = value;
    }

    public RoomPlmCrocomireVisualCatalog? RoomPlmCrocomireVisuals
    {
        get => Plms.CrocomireVisuals;
        set => Plms.CrocomireVisuals = value;
    }

    /// <summary>Visual-only collectible resources; reattached after debugger-state restoration.</summary>
    public RoomPlmCollectibleVisualCatalog? RoomPlmCollectibleVisuals
    {
        get => Plms.CollectibleVisuals;
        set => Plms.CollectibleVisuals = value;
    }

    /// <summary>Installed permanent-item tile/palette art; restored independently of PLM state.</summary>
    public RoomPlmDynamicCollectibleArtCatalog? RoomPlmDynamicCollectibleArt
    {
        get => Plms.DynamicCollectibleArt;
        set => Plms.DynamicCollectibleArt = value;
    }
}
