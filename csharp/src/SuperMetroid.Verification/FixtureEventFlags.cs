using SuperMetroid.Core.Game;

/// <summary>
/// Event bits a focused enemy fixture installs (#633). An operation the fixture did not install fails
/// at the dependency, as a host without the service would.
/// </summary>
internal sealed class FixtureEventFlags : IGlobalEventFlags
{
    /// <summary>Answers event queries.</summary>
    public Func<EventNumber, bool>? HasEvent { get; init; }

    /// <summary>Receives event sets.</summary>
    public Action<EventNumber>? SetEvent { get; init; }

    /// <summary>Receives event clears.</summary>
    public Action<EventNumber>? ClearEvent { get; init; }

    /// <summary>Delegates an event query or fails at the missing fixture dependency.</summary>
    public bool Has(EventNumber eventNumber) => (HasEvent ?? throw Missing("read"))(eventNumber);

    /// <summary>Delegates an event set or fails at the missing fixture dependency.</summary>
    public void Set(EventNumber eventNumber) => (SetEvent ?? throw Missing("set"))(eventNumber);

    /// <summary>Delegates an event clear or fails at the missing fixture dependency.</summary>
    public void Clear(EventNumber eventNumber) => (ClearEvent ?? throw Missing("clear"))(eventNumber);

    /// <summary>Reports which event operation the fixture did not install.</summary>
    private static InvalidOperationException Missing(string operation) =>
        new($"The enemy fixture did not install an event {operation}.");
}
