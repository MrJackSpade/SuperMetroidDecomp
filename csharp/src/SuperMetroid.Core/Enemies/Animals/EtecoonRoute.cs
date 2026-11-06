namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>The route an Etecoon takes after its first landing (#1519).</summary>
public enum EtecoonRoute
{
    /// <summary>
    /// Runs left to the teaching start, then hops four times and runs back. The only route that calls out when Samus
    /// wakes it.
    /// </summary>
    RunLeftToTeachingStart,

    /// <summary>Runs right to the teaching start, then hops four times and runs back.</summary>
    RunRightToTeachingStart,

    /// <summary>Starts teaching at once and keeps hopping in groups of four; it also takes the long run.</summary>
    TeachAtOnce,
}
