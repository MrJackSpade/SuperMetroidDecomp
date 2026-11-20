using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Enemies.Rios;

namespace SuperMetroid.Verification.Enemies.Rios;

/// <summary>
/// The native bank-$A2 lists the Holtz ROM oracle (#641) walks: each program's entry point and the owner
/// instructions the lists use, with the cue each must become and its operand roles (T target, A alternate, V value).
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class HoltzNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xa2;

    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<HoltzSegment>[] Entries =
    [
        new NativeProgramEntry<HoltzSegment>(HoltzSegment.Idle, 0xc61a),
        new NativeProgramEntry<HoltzSegment>(HoltzSegment.IdleWingbeat, 0xc61c),
        new NativeProgramEntry<HoltzSegment>(HoltzSegment.PrepareToSwoop, 0xc630),
        new NativeProgramEntry<HoltzSegment>(HoltzSegment.Descending, 0xc65a),
        new NativeProgramEntry<HoltzSegment>(HoltzSegment.AscentOpening, 0xc662),
        new NativeProgramEntry<HoltzSegment>(HoltzSegment.Ascending, 0xc674),
        new NativeProgramEntry<HoltzSegment>(HoltzSegment.Cooldown, 0xc686),
        new NativeProgramEntry<HoltzSegment>(HoltzSegment.Flames, 0xc6b0),
    ];

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, HoltzNativeListAddressesInstruction> Instructions = new()
    {
        [0xc6d2] = new HoltzNativeListAddressesInstruction(HoltzCueKind.AnimationFinished, ""),
        [0xc6dd] = new HoltzNativeListAddressesInstruction(HoltzCueKind.HideFlames, ""),
        [0xc6e8] = new HoltzNativeListAddressesInstruction(HoltzCueKind.ShowFlames, ""),
    };
}
