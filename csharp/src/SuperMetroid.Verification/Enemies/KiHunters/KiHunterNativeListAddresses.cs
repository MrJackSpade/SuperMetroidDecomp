using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Enemies.KiHunters;

namespace SuperMetroid.Verification.Enemies.KiHunters;

/// <summary>
/// The native bank-$A8 lists the Ki-Hunter ROM oracle (#641) walks: each program's entry point and the owner
/// instructions the lists use, with the cue each must become and its operand roles (T target, A alternate, V value).
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class KiHunterNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xa8;

    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<KiHunterSegment>[] Entries =
    [
        new NativeProgramEntry<KiHunterSegment>(KiHunterSegment.FlyingLeft, 0xe9fa),
        new NativeProgramEntry<KiHunterSegment>(KiHunterSegment.SwoopLeft, 0xea08),
        new NativeProgramEntry<KiHunterSegment>(KiHunterSegment.FlyingRight, 0xea24),
        new NativeProgramEntry<KiHunterSegment>(KiHunterSegment.SwoopRight, 0xea32),
        new NativeProgramEntry<KiHunterSegment>(KiHunterSegment.WingsLeft, 0xea4e),
        new NativeProgramEntry<KiHunterSegment>(KiHunterSegment.WingsRight, 0xea5e),
        new NativeProgramEntry<KiHunterSegment>(KiHunterSegment.DetachedWings, 0xea7e),
        new NativeProgramEntry<KiHunterSegment>(KiHunterSegment.JumpLeft, 0xea8a),
        new NativeProgramEntry<KiHunterSegment>(KiHunterSegment.JumpRight, 0xeaa6),
        new NativeProgramEntry<KiHunterSegment>(KiHunterSegment.LandLeft, 0xeac2),
        new NativeProgramEntry<KiHunterSegment>(KiHunterSegment.LandRight, 0xeada),
        new NativeProgramEntry<KiHunterSegment>(KiHunterSegment.SpitLeft, 0xeaf2),
        new NativeProgramEntry<KiHunterSegment>(KiHunterSegment.SpitRight, 0xeb10),
    ];

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, KiHunterNativeListAddressesInstruction> Instructions = new()
    {
        [0xf526] = new KiHunterNativeListAddressesInstruction(KiHunterCueKind.ReturnToFlying, ""),
        [0xf5e4] = new KiHunterNativeListAddressesInstruction(KiHunterCueKind.StartHop, ""),
        [0xf67f] = new KiHunterNativeListAddressesInstruction(KiHunterCueKind.StartWinglessThinking, ""),
        [0xf6d2] = new KiHunterNativeListAddressesInstruction(KiHunterCueKind.SpitLeft, ""),
        [0xf6d8] = new KiHunterNativeListAddressesInstruction(KiHunterCueKind.SpitRight, ""),
    };
}
