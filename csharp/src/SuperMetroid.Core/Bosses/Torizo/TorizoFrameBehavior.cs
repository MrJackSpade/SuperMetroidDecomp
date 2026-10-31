namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>The movement step a Torizo runs before its instructions; each member names the bank-$AA routine it replaced (#1413: the values are arbitrary).</summary>
public enum TorizoFrameBehavior
{
    /// <summary>The shared no-op movement step.</summary>
    Idle = 1,

    /// <summary><c>Function_Torizo_Movement_Walking</c>.</summary>
    AirTransition,

    /// <summary><c>Function_Torizo_Movement_Attacking</c>.</summary>
    Gravity,

    /// <summary><c>Function_Torizo_Movement_Jumping_Falling</c>.</summary>
    Jump,

    /// <summary><c>Function_GoldenTorizo_Movement_Walking</c>.</summary>
    GoldenAirTransition,

    /// <summary><c>Function_GoldenTorizo_Movement_Attacking</c>.</summary>
    GoldenGravity,
}
