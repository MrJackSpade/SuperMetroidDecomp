using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Frontend;

/// <summary>Named bit-index lists retain every persistent progression plane without Base64.</summary>
public sealed record SaveProgressionJsonDocument
{
    /// <summary>Canonical names of set global event bits; unknown numeric bits are rejected by the JSON codec.</summary>
    public required string[] Events { get; init; }

    /// <summary>One composable boss-state byte for each native area index.</summary>
    public required BossBits[] BossFlagsByAreaIndex { get; init; }

    /// <summary>Set bit indices in the 64-byte room Chozo-orb persistence plane.</summary>
    public required int[] RoomChozoBits { get; init; }

    /// <summary>Set bit indices in the 64-byte collected-item persistence plane.</summary>
    public required int[] CollectedItemBits { get; init; }

    /// <summary>Set bit indices in the 64-byte opened-door persistence plane.</summary>
    public required int[] OpenedDoorBits { get; init; }

    /// <summary>Set bit indices in the 16-byte save-station and elevator persistence plane.</summary>
    public required int[] UsedSaveStationBits { get; init; }

    /// <summary>Set bit indices in the 12-byte acquired-map-station persistence plane.</summary>
    public required int[] AcquiredMapStationBits { get; init; }

    /// <summary>Explored map cells grouped by the six persisted Zebes areas.</summary>
    public required ExploredAreaJsonDocument[] ExploredAreas { get; init; }
}
