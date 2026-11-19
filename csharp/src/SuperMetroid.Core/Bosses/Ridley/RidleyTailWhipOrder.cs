namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// The whip Ridley's fight orders of his tail (#1940, #2562): the swoop's lowest point, his wall sweep, a missed lunge and
/// his release of Samus order one. His tail's neutral control carries it out on the first update all its links are active,
/// aiming it a whole turn further along its swing than an idle random whip, and the order waits until then, through any
/// updates his tail spends on another action.
/// </summary>
public sealed class RidleyTailWhipOrder
{
    /// <summary>Whether an ordered whip waits for his tail's neutral control.</summary>
    public bool IsWaiting { get; private set; }

    /// <summary>The fight orders a whip of his tail.</summary>
    internal void Give() => IsWaiting = true;

    /// <summary>His tail's neutral control, all its links active, takes the order: whether a whip was ordered.</summary>
    internal bool Take()
    {
        bool waiting = IsWaiting;
        IsWaiting = false;
        return waiting;
    }
}
