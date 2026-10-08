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

    /// <summary>Visual-only elevator-platform frames; reattached after state restoration.</summary>
    public RoomPlmElevatorPlatformVisualCatalog? RoomPlmElevatorPlatformVisuals
    {
        get => Plms.ElevatorPlatformVisuals;
        set => Plms.ElevatorPlatformVisuals = value;
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

    /// <summary>Host-installed bomb-revealed Speed Booster block appearance forwarded to the PLM system; nonserialized and rebound after restoration, while type-B collision and reveal timing remain compiled.</summary>
    public RoomPlmSpeedBoosterVisualCatalog? RoomPlmSpeedBoosterVisuals
    {
        get => Plms.SpeedBoosterVisuals;
        set => Plms.SpeedBoosterVisuals = value;
    }

    /// <summary>Host-installed Maridia elevatube block appearance forwarded to the PLM system; rebound after restoration independently of physical blocks, hold duration, sound, and deletion mechanics.</summary>
    public RoomPlmMaridiaElevatubeVisualCatalog? RoomPlmMaridiaElevatubeVisuals
    {
        get => Plms.MaridiaElevatubeVisuals;
        set => Plms.MaridiaElevatubeVisuals = value;
    }

    /// <summary>Nonserialized Spore Spawn ceiling artwork for three crumble frames and final clear, forwarded to the PLM system and rebound after restoration; physical words and 2-by-2 draw geometry remain compiled.</summary>
    public RoomPlmSporeSpawnCeilingVisualCatalog? RoomPlmSporeSpawnCeilingVisuals
    {
        get => Plms.SporeSpawnCeilingVisuals;
        set => Plms.SporeSpawnCeilingVisuals = value;
    }

    /// <summary>Nonserialized artwork for four floor and four ceiling Samus Eater plant poses, rebound through the PLM system after restoration without replacing their collision nibbles or three-run geometry.</summary>
    public RoomPlmSamusEaterVisualCatalog? RoomPlmSamusEaterVisuals
    {
        get => Plms.SamusEaterVisuals;
        set => Plms.SamusEaterVisuals = value;
    }

    /// <summary>Nonserialized nine-block defeated-Botwoon wall-clear appearance forwarded to the PLM system and rebound after restoration; crumble frames use the shared shot-block catalog and mechanics remain compiled.</summary>
    public RoomPlmBotwoonWallVisualCatalog? RoomPlmBotwoonWallVisuals
    {
        get => Plms.BotwoonWallVisuals;
        set => Plms.BotwoonWallVisuals = value;
    }

    /// <summary>Host-installed Kraid ceiling and floor-spike appearances forwarded to the PLM system; rebound after restoration independently of native block words, animation timing, and movement callbacks.</summary>
    public RoomPlmKraidVisualCatalog? RoomPlmKraidVisuals
    {
        get => Plms.KraidVisuals;
        set => Plms.KraidVisuals = value;
    }

    /// <summary>Nonserialized Crocomire bridge and invisible-wall appearances forwarded to the PLM system and rebound after restoration; physical level words, scripted timing, and draw geometry remain compiled.</summary>
    public RoomPlmCrocomireVisualCatalog? RoomPlmCrocomireVisuals
    {
        get => Plms.CrocomireVisuals;
        set => Plms.CrocomireVisuals = value;
    }

    /// <summary>Nonserialized Mother Brain fake-death wall, background, door, and tube appearances forwarded to the PLM system and rebound after restoration; collision words, draw geometry, and PLM timing remain unchanged.</summary>
    public RoomPlmMotherBrainFakeDeathVisualCatalog? RoomPlmMotherBrainFakeDeathVisuals
    {
        get => Plms.MotherBrainFakeDeathVisuals;
        set => Plms.MotherBrainFakeDeathVisuals = value;
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
