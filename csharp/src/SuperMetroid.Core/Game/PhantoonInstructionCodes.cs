namespace SuperMetroid.Core.Game;

/// <summary>Named bank-$A7 code pointers consumed by translated Phantoon dispatchers.</summary>
internal enum PhantoonInstruction : ushort
{
    /// <summary><c>PlayPhantoonMaterializationSFX</c> at $A7:CEED.</summary>
    PlayPhantoonMaterializationSFX = 0xceed,

    /// <summary><c>SetupEyeOpenPhantoonState</c> at $A7:D03F.</summary>
    SetupEyeOpenPhantoonState = 0xd03f,

    /// <summary><c>PickNewPhantoonPattern</c> at $A7:D076.</summary>
    PickNewPhantoonPattern = 0xd076,

    /// <summary><c>SpawnCasualFlame</c> at $A7:CF5E.</summary>
    SpawnCasualFlame = 0xcf5e,
}

/// <summary>
/// The animation instructions Phantoon's parts execute beyond the shared common set. The
/// bank's common call-function command is translated only for Phantoon, whose callbacks are
/// the <see cref="PhantoonInstruction"/> routines.
/// </summary>
internal enum PhantoonPartInstruction : ushort
{
    /// <summary><c>Instruction_CommonA7_CallFunctionInY</c> at $A7:808A.</summary>
    CallFunctionInY = 0x808a,
}
