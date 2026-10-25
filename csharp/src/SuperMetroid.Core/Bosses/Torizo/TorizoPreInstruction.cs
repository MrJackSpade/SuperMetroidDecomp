namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>The movement step a Torizo runs before its instructions; each member names the bank-$AA routine it replaced (#1413: the values are arbitrary).</summary>
public enum TorizoPreInstruction
{
    /// <summary>$AA:C95E, the shared no-op movement step.</summary>
    Idle = 1,

    /// <summary><c>Function_Torizo_Movement_Walking</c> at $AA:C752.</summary>
    AirTransition,

    /// <summary><c>Function_Torizo_Movement_Attacking</c> at $AA:C828.</summary>
    Gravity,

    /// <summary><c>Function_Torizo_Movement_Jumping_Falling</c> at $AA:C82C.</summary>
    Jump,

    /// <summary><c>Function_GoldenTorizo_Movement_Walking</c> at $AA:D5F1.</summary>
    GoldenAirTransition,

    /// <summary><c>Function_GoldenTorizo_Movement_Attacking</c> at $AA:D5ED.</summary>
    GoldenGravity,
}
