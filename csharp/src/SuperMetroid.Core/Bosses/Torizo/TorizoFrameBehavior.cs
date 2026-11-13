namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>The movement step a Torizo runs before its animation (#1413: the values are arbitrary).</summary>
public enum TorizoFrameBehavior
{
    /// <summary>The shared no-op movement step.</summary>
    Idle = 1,

    /// <summary>Torizo behaviour: movement, walking.</summary>
    AirTransition,

    /// <summary>Torizo behaviour: movement, attacking.</summary>
    Gravity,

    /// <summary>Torizo behaviour: movement, jumping, falling.</summary>
    Jump,

    /// <summary>Golden Torizo behaviour: movement, walking.</summary>
    GoldenAirTransition,

    /// <summary>Golden Torizo behaviour: movement, attacking.</summary>
    GoldenGravity,
}
