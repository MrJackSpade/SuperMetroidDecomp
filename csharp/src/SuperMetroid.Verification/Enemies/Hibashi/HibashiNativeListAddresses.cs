using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Enemies.Hibashi;

namespace SuperMetroid.Verification.Enemies.Hibashi;

/// <summary>
/// The native bank-$A6 lists the Hibashi ROM oracle (#641) walks: each program's entry point and the owner
/// instructions the lists use, with the cue each must become and its operand roles (T target, A alternate, V value).
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class HibashiNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xa6;

    /// <summary>The native table of each rising step's height above the spawn, one word per step.</summary>
    internal const int StepYOffsetTable = 0xa68dbb;

    /// <summary>The native table of each rising step's half-height, one word per step.</summary>
    internal const int StepYRadiusTable = 0xa68de7;

    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<HibashiSegment>[] Entries =
    [
        new NativeProgramEntry<HibashiSegment>(HibashiSegment.Flame, 0x8d1b),
        new NativeProgramEntry<HibashiSegment>(HibashiSegment.Hitbox, 0x8da9),
    ];

    /// <summary>Each signalling owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, HibashiNativeListAddressesInstruction> Instructions = new()
    {
        [0x8daf] = new HibashiNativeListAddressesInstruction(new HibashiCue.Signal(HibashiCueKind.PlayEruptionSound), ""),
        [0x8fd1] = new HibashiNativeListAddressesInstruction(new HibashiCue.Signal(HibashiCueKind.FinishActivity), ""),
    };

    /// <summary>
    /// The native rising routines in step order (#2337): the routine at each place applies the step at that place of
    /// <see cref="StepYOffsetTable"/> and <see cref="StepYRadiusTable"/>, and the first also gives the collision part its
    /// width back. None takes an operand.
    /// </summary>
    internal static readonly ushort[] RisingStepRoutines =
    [
        0x8e13, 0x8e2d, 0x8e41, 0x8e55, 0x8e69, 0x8e7d, 0x8e91, 0x8ea5, 0x8eb9, 0x8ecd, 0x8ee1,
        0x8ef5, 0x8f09, 0x8f1d, 0x8f31, 0x8f45, 0x8f59, 0x8f6d, 0x8f81, 0x8f95, 0x8fa9, 0x8fbd,
    ];
}
