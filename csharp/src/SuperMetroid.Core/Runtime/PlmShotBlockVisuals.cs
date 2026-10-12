using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Runtime;

/// <summary>
/// Visual state of the room's shot-reactive PLM blocks.
/// </summary>
internal sealed class PlmShotBlockVisuals
{
    private readonly RoomPlmSystem _plms;

    internal PlmShotBlockVisuals(RoomPlmSystem plms)
    {
        _plms = plms;
    }
    /// <summary>Visual-only shot-block resources; reattached after debugger-state restoration.</summary>
    public RoomPlmShotBlockVisualCatalog? RoomPlmShotBlockVisuals
    {
        get => _plms.Drawing.ShotBlockVisuals;
        set => _plms.Drawing.ShotBlockVisuals = value;
    }

    /// <summary>Visual-only Grapple-block resources; reattached after debugger-state restoration.</summary>
    public RoomPlmGrappleBlockVisualCatalog? RoomPlmGrappleBlockVisuals
    {
        get => _plms.Drawing.GrappleBlockVisuals;
        set => _plms.Drawing.GrappleBlockVisuals = value;
    }

    /// <summary>Visual-only station resources; reattached after debugger-state restoration.</summary>
    public RoomPlmStationVisualCatalog? RoomPlmStationVisuals
    {
        get => _plms.Drawing.StationVisuals;
        set => _plms.Drawing.StationVisuals = value;
    }

    /// <summary>Visual-only blue-door cap resources; reattached after debugger-state restoration.</summary>
    public RoomPlmBlueDoorVisualCatalog? RoomPlmBlueDoorVisuals
    {
        get => _plms.Drawing.BlueDoorVisuals;
        set => _plms.Drawing.BlueDoorVisuals = value;
    }

    /// <summary>Visual-only colored-door resources; reattached after debugger-state restoration.</summary>
    public RoomPlmColoredDoorVisualCatalog? RoomPlmColoredDoorVisuals
    {
        get => _plms.Drawing.ColoredDoorVisuals;
        set => _plms.Drawing.ColoredDoorVisuals = value;
    }

    /// <summary>Visual-only grey-door and shared clear-cap resources; reattached after state restoration.</summary>
    public RoomPlmGreyDoorVisualCatalog? RoomPlmGreyDoorVisuals
    {
        get => _plms.Drawing.GreyDoorVisuals;
        set => _plms.Drawing.GreyDoorVisuals = value;
    }

    /// <summary>Visual-only eye-door resources; reattached after state restoration.</summary>
    public RoomPlmEyeDoorVisualCatalog? RoomPlmEyeDoorVisuals
    {
        get => _plms.Drawing.EyeDoorVisuals;
        set => _plms.Drawing.EyeDoorVisuals = value;
    }

    /// <summary>Visual-only Mother Brain glass resources; reattached after state restoration.</summary>
    public RoomPlmMotherBrainGlassVisualCatalog? RoomPlmMotherBrainGlassVisuals
    {
        get => _plms.Drawing.MotherBrainGlassVisuals;
        set => _plms.Drawing.MotherBrainGlassVisuals = value;
    }

    /// <summary>Visual-only n00b-tube resources; reattached after state restoration.</summary>
    public RoomPlmNoobTubeVisualCatalog? RoomPlmNoobTubeVisuals
    {
        get => _plms.Drawing.NoobTubeVisuals;
        set => _plms.Drawing.NoobTubeVisuals = value;
    }

    /// <summary>Visual-only gate-block resources; reattached after debugger-state restoration.</summary>
    public RoomPlmDownwardGateVisualCatalog? RoomPlmDownwardGateVisuals
    {
        get => _plms.Drawing.DownwardGateVisuals;
        set => _plms.Drawing.DownwardGateVisuals = value;
    }

    /// <summary>Visual-only elevator-platform frames; reattached after state restoration.</summary>
    public RoomPlmElevatorPlatformVisualCatalog? RoomPlmElevatorPlatformVisuals
    {
        get => _plms.Drawing.ElevatorPlatformVisuals;
        set => _plms.Drawing.ElevatorPlatformVisuals = value;
    }

    /// <summary>Visual-only escape-gate resources; reattached after debugger-state restoration.</summary>
    public RoomPlmEscapeGateVisualCatalog? RoomPlmEscapeGateVisuals
    {
        get => _plms.Drawing.EscapeGateVisuals;
        set => _plms.Drawing.EscapeGateVisuals = value;
    }

    /// <summary>Visual-only Bomb Torizo hand resource; reattached after state restoration.</summary>
    public RoomPlmBombTorizoHandVisualCatalog? RoomPlmBombTorizoHandVisuals
    {
        get => _plms.Drawing.BombTorizoHandVisuals;
        set => _plms.Drawing.BombTorizoHandVisuals = value;
    }

    /// <summary>Visual-only Draygon cannon resource; reattached after state restoration.</summary>
    public RoomPlmDraygonCannonVisualCatalog? RoomPlmDraygonCannonVisuals
    {
        get => _plms.Drawing.DraygonCannonVisuals;
        set => _plms.Drawing.DraygonCannonVisuals = value;
    }

    /// <summary>Visual-only Chozo statue terrain resources; reattached after state restoration.</summary>
    public RoomPlmChozoStatueVisualCatalog? RoomPlmChozoStatueVisuals
    {
        get => _plms.Drawing.ChozoStatueVisuals;
        set => _plms.Drawing.ChozoStatueVisuals = value;
    }

    /// <summary>Visual-only linked restoration resource; reattached after state restoration.</summary>
    public RoomPlmLinkedRestoreVisualCatalog? RoomPlmLinkedRestoreVisuals
    {
        get => _plms.Drawing.LinkedRestoreVisuals;
        set => _plms.Drawing.LinkedRestoreVisuals = value;
    }

    /// <summary>Visual-only Tourian access-floor resource; reattached after state restoration.</summary>
    public RoomPlmTourianAccessVisualCatalog? RoomPlmTourianAccessVisuals
    {
        get => _plms.Drawing.TourianAccessVisuals;
        set => _plms.Drawing.TourianAccessVisuals = value;
    }

    /// <summary>Host-installed bomb-revealed Speed Booster block appearance forwarded to the PLM system; nonserialized and rebound after restoration, while type-B collision and reveal timing remain compiled.</summary>
    public RoomPlmSpeedBoosterVisualCatalog? RoomPlmSpeedBoosterVisuals
    {
        get => _plms.Drawing.SpeedBoosterVisuals;
        set => _plms.Drawing.SpeedBoosterVisuals = value;
    }

    /// <summary>Host-installed Maridia elevatube block appearance forwarded to the PLM system; rebound after restoration independently of physical blocks, hold duration, sound, and deletion mechanics.</summary>
    public RoomPlmMaridiaElevatubeVisualCatalog? RoomPlmMaridiaElevatubeVisuals
    {
        get => _plms.Drawing.MaridiaElevatubeVisuals;
        set => _plms.Drawing.MaridiaElevatubeVisuals = value;
    }

    /// <summary>Nonserialized Spore Spawn ceiling artwork for three crumble frames and final clear, forwarded to the PLM system and rebound after restoration; physical words and 2-by-2 draw geometry remain compiled.</summary>
    public RoomPlmSporeSpawnCeilingVisualCatalog? RoomPlmSporeSpawnCeilingVisuals
    {
        get => _plms.Drawing.SporeSpawnCeilingVisuals;
        set => _plms.Drawing.SporeSpawnCeilingVisuals = value;
    }

    /// <summary>Nonserialized artwork for four floor and four ceiling Samus Eater plant poses, rebound through the PLM system after restoration without replacing their collision nibbles or three-run geometry.</summary>
    public RoomPlmSamusEaterVisualCatalog? RoomPlmSamusEaterVisuals
    {
        get => _plms.Drawing.SamusEaterVisuals;
        set => _plms.Drawing.SamusEaterVisuals = value;
    }

    /// <summary>Nonserialized nine-block defeated-Botwoon wall-clear appearance forwarded to the PLM system and rebound after restoration; crumble frames use the shared shot-block catalog and mechanics remain compiled.</summary>
    public RoomPlmBotwoonWallVisualCatalog? RoomPlmBotwoonWallVisuals
    {
        get => _plms.Drawing.BotwoonWallVisuals;
        set => _plms.Drawing.BotwoonWallVisuals = value;
    }

    /// <summary>Host-installed Kraid ceiling and floor-spike appearances forwarded to the PLM system; rebound after restoration independently of native block words, animation timing, and movement callbacks.</summary>
    public RoomPlmKraidVisualCatalog? RoomPlmKraidVisuals
    {
        get => _plms.Drawing.KraidVisuals;
        set => _plms.Drawing.KraidVisuals = value;
    }

    /// <summary>Nonserialized Crocomire bridge and invisible-wall appearances forwarded to the PLM system and rebound after restoration; physical level words, scripted timing, and draw geometry remain compiled.</summary>
    public RoomPlmCrocomireVisualCatalog? RoomPlmCrocomireVisuals
    {
        get => _plms.Drawing.CrocomireVisuals;
        set => _plms.Drawing.CrocomireVisuals = value;
    }

    /// <summary>Nonserialized Mother Brain fake-death wall, background, door, and tube appearances forwarded to the PLM system and rebound after restoration; collision words, draw geometry, and PLM timing remain unchanged.</summary>
    public RoomPlmMotherBrainFakeDeathVisualCatalog? RoomPlmMotherBrainFakeDeathVisuals
    {
        get => _plms.Drawing.MotherBrainFakeDeathVisuals;
        set => _plms.Drawing.MotherBrainFakeDeathVisuals = value;
    }

    /// <summary>Visual-only collectible resources; reattached after debugger-state restoration.</summary>
    public RoomPlmCollectibleVisualCatalog? RoomPlmCollectibleVisuals
    {
        get => _plms.Drawing.CollectibleVisuals;
        set => _plms.Drawing.CollectibleVisuals = value;
    }

    /// <summary>Installed permanent-item tile/palette art; restored independently of PLM state.</summary>
    public RoomPlmDynamicCollectibleArtCatalog? RoomPlmDynamicCollectibleArt
    {
        get => _plms.Drawing.DynamicCollectibleArt;
        set => _plms.Drawing.DynamicCollectibleArt = value;
    }
}
