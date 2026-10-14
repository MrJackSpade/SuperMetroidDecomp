namespace SuperMetroid.Core.Game;

/// <summary>Bank-$AA movement pre-instructions a Torizo installs in <c>toriz_var_F</c>.</summary>
public enum TorizoPreInstruction : ushort
{
    /// <summary>$AA:C95E, the shared no-op movement step.</summary>
    Idle = 0xc95e,

    /// <summary><c>Function_Torizo_Movement_Walking</c> at $AA:C752.</summary>
    AirTransition = 0xc752,

    /// <summary><c>Function_Torizo_Movement_Attacking</c> at $AA:C828.</summary>
    Gravity = 0xc828,

    /// <summary><c>Function_Torizo_Movement_Jumping_Falling</c> at $AA:C82C.</summary>
    Jump = 0xc82c,

    /// <summary><c>Function_GoldenTorizo_Movement_Walking</c> at $AA:D5F1.</summary>
    GoldenAirTransition = 0xd5f1,

    /// <summary><c>Function_GoldenTorizo_Movement_Attacking</c> at $AA:D5ED.</summary>
    GoldenGravity = 0xd5ed,
}
