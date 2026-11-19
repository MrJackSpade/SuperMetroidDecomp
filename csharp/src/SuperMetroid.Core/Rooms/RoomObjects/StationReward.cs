namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// What a station's activation gives Samus: a refill, which the station turns her away for when already full and
/// releases her right after, or the area's map, which leaves her locked for leaving the map screen to release.
/// </summary>
internal abstract record StationReward
{
    /// <summary>Only the nested cases derive.</summary>
    private StationReward()
    {
    }

    /// <summary>The map station's reward: its area's map.</summary>
    internal static StationReward AreaMap { get; } = new AreaMapReward();

    /// <summary>Whether the reward releases Samus right after its activation rather than after the map screen.</summary>
    internal bool ReleasesAtActivation => this is Refills;

    /// <summary>A refill station's reward.</summary>
    /// <param name="Refill">What the station refills.</param>
    internal sealed record Refills(StationRefill Refill) : StationReward;

    /// <summary>The area's map.</summary>
    private sealed record AreaMapReward : StationReward;
}
