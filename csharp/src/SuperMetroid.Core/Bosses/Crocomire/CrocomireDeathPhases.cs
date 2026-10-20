namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Domain-owned transitions over <see cref="CrocomireDeathPhase"/>.</summary>
public static class CrocomireDeathPhases
{
    /// <summary>
    /// The dispatcher entry after <paramref name="phase"/>, as the native death handlers
    /// advance variable A by one table word. <see cref="CrocomireDeathPhase.RiverSkeletonDetour"/> is the last
    /// table entry and has no successor.
    /// </summary>
    public static CrocomireDeathPhase Next(this CrocomireDeathPhase phase) => EnumOrder.Next(phase);
}
