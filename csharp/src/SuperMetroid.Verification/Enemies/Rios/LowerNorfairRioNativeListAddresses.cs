using SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies.Rios;

namespace SuperMetroid.Verification.Enemies.Rios;

/// <summary>
/// The native bank-$A2 lists the Lower Norfair Rio ROM oracle (#641) walks: each program's entry point and the owner
/// instructions the lists use, with the cue each must become and its operand roles (T target, A alternate, V value).
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class LowerNorfairRioNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xa2;

    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<LowerNorfairRioProgram>[] Entries =
    [
        new NativeProgramEntry<LowerNorfairRioProgram>(LowerNorfairRioProgram.Idle, 0xc61a),
        new NativeProgramEntry<LowerNorfairRioProgram>(LowerNorfairRioProgram.IdleLoop, 0xc61c),
        new NativeProgramEntry<LowerNorfairRioProgram>(LowerNorfairRioProgram.PrepareToSwoop, 0xc630),
        new NativeProgramEntry<LowerNorfairRioProgram>(LowerNorfairRioProgram.Descending, 0xc65a),
        new NativeProgramEntry<LowerNorfairRioProgram>(LowerNorfairRioProgram.AscendingPart1, 0xc662),
        new NativeProgramEntry<LowerNorfairRioProgram>(LowerNorfairRioProgram.AscendingPart2, 0xc674),
        new NativeProgramEntry<LowerNorfairRioProgram>(LowerNorfairRioProgram.AscendingPart2Loop, 0xc676),
        new NativeProgramEntry<LowerNorfairRioProgram>(LowerNorfairRioProgram.Cooldown, 0xc686),
        new NativeProgramEntry<LowerNorfairRioProgram>(LowerNorfairRioProgram.Flames, 0xc6b0),
    ];

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, LowerNorfairRioNativeListAddressesInstruction> Instructions = new()
    {
        [0xc6d2] = new LowerNorfairRioNativeListAddressesInstruction(LowerNorfairRioCueKind.AnimationFinished, ""),
        [0xc6dd] = new LowerNorfairRioNativeListAddressesInstruction(LowerNorfairRioCueKind.HideFlames, ""),
        [0xc6e8] = new LowerNorfairRioNativeListAddressesInstruction(LowerNorfairRioCueKind.ShowFlames, ""),
    };
}
