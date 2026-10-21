using SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies.Sparks;

namespace SuperMetroid.Verification.Enemies.Sparks;

/// <summary>
/// The native bank-$A8 lists the Spark ROM oracle (#641) walks: each program's entry point and the owner
/// instructions the lists use, with the cue each must become and its operand roles (T target, A alternate, V value).
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class SparkNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xa8;

    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<SparkProgram>[] Entries =
    [
        new NativeProgramEntry<SparkProgram>(SparkProgram.FlickerOn, 0xe5a7),
        new NativeProgramEntry<SparkProgram>(SparkProgram.Active, 0xe5d1),
        new NativeProgramEntry<SparkProgram>(SparkProgram.FlickerOut, 0xe5e5),
        new NativeProgramEntry<SparkProgram>(SparkProgram.Emitter, 0xe609),
    ];

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, SparkNativeListAddressesInstruction> Instructions = new()
    {
        [0xe61d] = new SparkNativeListAddressesInstruction(SparkCueKind.BecomeIntangible, ""),
        [0xe62a] = new SparkNativeListAddressesInstruction(SparkCueKind.BecomeTangible, ""),
    };
}
