namespace SuperMetroid.Core.Frontend.Map;

/// <summary>
/// An accepted eight-update scroll step under way on a map screen (#1975): the way it moves the map, and its age, counted
/// once each update it runs, which moves the map at its pulse and finishes the step on its eighth update.
/// </summary>
public sealed partial class MapScrollStep
{
    /// <summary>A step in <paramref name="direction"/> with all its updates left.</summary>
    internal MapScrollStep(MapScrollDirection direction) => Direction = direction;

    /// <summary>The way the step moves the map.</summary>
    public MapScrollDirection Direction { get; }

    /// <summary>The step's updates, counted once each update it runs.</summary>
    internal UpdateCountdown Updates { get; } = UpdateCountdown.Lasting(MapScrollDefinitions.StepTicks);
}
