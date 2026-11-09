using SuperMetroid.Core.Progression;

namespace SuperMetroid.Verification.Harness;

/// <summary>
/// Event bits a focused enemy fixture installs (#633). An operation the fixture did not install fails
/// at the dependency, as a host without the service would.
/// </summary>
internal sealed class FixtureEventFlags : IGlobalEventFlags
{
    /// <summary>Answers event queries.</summary>
    public Func<StoryEvent, bool>? HasEvent { get; init; }

    /// <summary>Receives event sets.</summary>
    public Action<StoryEvent>? SetEvent { get; init; }

    /// <summary>Delegates an event query or fails at the missing fixture dependency.</summary>
    public bool Has(StoryEvent eventNumber) => (HasEvent ?? throw Missing("read"))(eventNumber);

    /// <summary>Delegates an event set or fails at the missing fixture dependency.</summary>
    public void Set(StoryEvent eventNumber) => (SetEvent ?? throw Missing("set"))(eventNumber);

    /// <summary>Reports which event operation the fixture did not install.</summary>
    private static InvalidOperationException Missing(string operation) =>
        new($"The enemy fixture did not install an event {operation}.");
}
