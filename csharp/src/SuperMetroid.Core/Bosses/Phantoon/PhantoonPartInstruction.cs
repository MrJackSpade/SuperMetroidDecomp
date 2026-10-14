namespace SuperMetroid.Core.Bosses.Phantoon;

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
