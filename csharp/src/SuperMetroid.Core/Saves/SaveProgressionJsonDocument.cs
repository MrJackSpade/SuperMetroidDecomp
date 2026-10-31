using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Map;

namespace SuperMetroid.Core.Saves;

/// <summary>A save slot's persistent progress, each set listed by its own identities (schema three, #1475).</summary>
public sealed record SaveProgressionJsonDocument
{
    /// <summary>The story events that have happened: a retail event's name, or <c>0xNN</c> for an event the game does not name.</summary>
    public required string[] Events { get; init; }

    /// <summary>Each area's defeated bosses, one entry per area in area order.</summary>
    public required SaveAreaBossesJsonDocument[] DefeatedBosses { get; init; }

    /// <summary>The Chozo orbs broken open, by room-data number, ascending.</summary>
    public required int[] BrokenChozoOrbs { get; init; }

    /// <summary>The item locations collected, by room-data number, ascending.</summary>
    public required int[] CollectedItems { get; init; }

    /// <summary>The coloured doors opened for good, by room-data number, ascending.</summary>
    public required int[] OpenedDoors { get; init; }

    /// <summary>Each area's used save stations and elevators, one entry per area in area order.</summary>
    public required SaveAreaStationsJsonDocument[] UsedStations { get; init; }

    /// <summary>The areas whose map has been acquired, in area order.</summary>
    public required AreaId[] AcquiredMaps { get; init; }

    /// <summary>Explored map cells, one entry per area in area order.</summary>
    public required ExploredAreaJsonDocument[] ExploredAreas { get; init; }
}
