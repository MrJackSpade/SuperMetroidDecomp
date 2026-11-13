namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>The main phase a Bomb or Golden Torizo runs (#1413: the values are arbitrary).</summary>
public enum TorizoPhase
{
    /// <summary>Dormant.</summary>
    Idle = 1,

    /// <summary>Torizo behaviour: simple movement.</summary>
    Falling,

    /// <summary>Torizo behaviour: wake when Bomb Torizo Chozo finishes crumbling.</summary>
    WaitForHandTrigger,

    /// <summary>Torizo behaviour: normal movement.</summary>
    Active,

    /// <summary>Golden Torizo behaviour: wake if Samus is below and right of target pos.</summary>
    GoldenWaitForSamus,

    /// <summary>Golden Torizo behaviour: simple movement.</summary>
    GoldenGravity,

    /// <summary>Golden Torizo behaviour: normal movement.</summary>
    GoldenFrameBehavior,
}
