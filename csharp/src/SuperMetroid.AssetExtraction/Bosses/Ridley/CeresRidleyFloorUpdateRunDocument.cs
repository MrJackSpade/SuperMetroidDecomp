namespace SuperMetroid.AssetExtraction.Bosses.Ridley;

/// <summary>One floor update run's descriptor as the getaway queues it (#1467): its tile source, tile count and first floor cell.</summary>
public sealed record CeresRidleyFloorUpdateRunDocument
{
    /// <summary>Where the run takes its tile numbers from, 0 through 65535.</summary>
    public required int Source { get; init; }
    /// <summary>The tile numbers the run copies, 0 through 65535.</summary>
    public required int Count { get; init; }
    /// <summary>The first floor cell the run copies them to, 0 through 65535.</summary>
    public required int Cell { get; init; }
}
