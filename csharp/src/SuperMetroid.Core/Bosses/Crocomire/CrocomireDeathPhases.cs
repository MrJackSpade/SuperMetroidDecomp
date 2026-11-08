namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Domain-owned transitions over <see cref="CrocomireDeathPhase"/>.</summary>
public static class CrocomireDeathPhases
{
    /// <summary>
    /// The phase after <paramref name="phase"/> in the death sequence's declared order, which a finishing phase hands
    /// over to. <see cref="CrocomireDeathPhase.RiverSkeletonDetour"/> is the last phase and has no successor.
    /// </summary>
    public static CrocomireDeathPhase Next(this CrocomireDeathPhase phase) => EnumOrder.Next(phase);
}
