namespace SuperMetroid.Core.Enemies;

/// <summary>How a wait a room's placement authors as a number of updates runs (#2281).</summary>
internal static partial class EnemyWaitDefinitions
{
    /// <summary>
    /// The update a placement's authored wait runs out on, after the updates it counts: a wait the placement gives as N
    /// updates lasts N + 1, its last update the one on which the waiting enemy acts. A wait held as its true duration
    /// adds this to the placement's count.
    /// </summary>
    internal const int RunOutUpdate = 1;
}
