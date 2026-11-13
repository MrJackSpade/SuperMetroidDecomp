using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Verification.Bosses.Kraid;

/// <summary>
/// The native bank-$A7 and bank-$86 lists the Kraid ROM oracle (#1284) walks: the part and rock list entry points,
/// and their callbacks with the cue each must become. Only verification reads them; the
/// port runs typed programs.
/// </summary>
internal static class KraidNativeListAddresses
{
    /// <summary>Each native sink-callback routine pointer in bank $A7 and the callback it is.</summary>
    internal static readonly Dictionary<ushort, KraidSinkCallback> NativeSinkCallbacks = new()
    {
        [0xc6a6] = KraidSinkCallback.NoOperation,
        [0xc691] = KraidSinkCallback.CrumbleLeftPlatformLeft,
        [0xc6a7] = KraidSinkCallback.CrumbleRightPlatformMiddle,
        [0xc6bd] = KraidSinkCallback.CrumbleRightPlatformLeft,
        [0xc6d3] = KraidSinkCallback.CrumbleLeftPlatformRight,
        [0xc6e9] = KraidSinkCallback.CrumbleLeftPlatformMiddle,
        [0xc6ff] = KraidSinkCallback.CrumbleRightPlatformRight,
    };

    /// <summary>Kraid's instruction bank, $A7.</summary>
    internal const byte PartBankNumber = 0xa7;

    /// <summary>Each typed part program's native entry point in bank $A7, in authored order.</summary>
    internal static readonly PartEntryValue[] PartEntries =
    [
        new PartEntryValue(KraidPartSegment.FootInitial, 0x86e7),
        new PartEntryValue(KraidPartSegment.FootNeutral, 0x86ed),
        new PartEntryValue(KraidPartSegment.FootWalkingForward, 0x86f3),
        new PartEntryValue(KraidPartSegment.FootWalkingForwardFinished, 0x87bb),
        new PartEntryValue(KraidPartSegment.FootLungeForward, 0x87bd),
        new PartEntryValue(KraidPartSegment.FootLungeForwardFinished, 0x8885),
        new PartEntryValue(KraidPartSegment.FootWalkingBackward, 0x8887),
        new PartEntryValue(KraidPartSegment.FootWalkingBackwardLoop, 0x8939),
        new PartEntryValue(KraidPartSegment.ArmNormal, 0x89f3),
        new PartEntryValue(KraidPartSegment.ArmNormalPause, 0x8a37),
        new PartEntryValue(KraidPartSegment.ArmSlow, 0x8a41),
        new PartEntryValue(KraidPartSegment.ArmRisingOrSinking, 0x8aa4),
        new PartEntryValue(KraidPartSegment.ArmDyingOrPreparingToLunge, 0x8af0),
        new PartEntryValue(KraidPartSegment.LintInitial, 0x8afe),
        new PartEntryValue(KraidPartSegment.LintPostGrowth, 0x8b04),
        new PartEntryValue(KraidPartSegment.NailLoop, 0x8b0a),
    ];

    /// <summary>The bank-$A7 part callbacks and the cue each must become; the no-operation callback has none.</summary>
    internal static readonly Dictionary<ushort, KraidPartCue?> PartCallbacks = new()
    {
        [0xb633] = null,
        [0xb636] = KraidPartCue.RaiseBody,
        [0xb63c] = KraidPartCue.LowerBodyAndShake,
        [0xb64e] = KraidPartCue.StepSound,
        [0xb65a] = KraidPartCue.StepLeft,
        [0xb667] = KraidPartCue.StepLeft,
        [0xb674] = KraidPartCue.StepRight,
        [0x8a8f] = KraidPartCue.SlowArmBelowHalfHealth,
    };

    /// <summary>The part program whose native list starts at <paramref name="address"/>, if one does.</summary>
    internal static KraidPartSegment? PartProgramAt(ushort address) =>
        PartEntries.Where(item => item.Entry == address).Select(item => (KraidPartSegment?)item.Program).SingleOrDefault();

    /// <summary>The native entry point of <paramref name="program"/>.</summary>
    internal static ushort PartEntry(KraidPartSegment program) => PartEntries.Single(item => item.Program == program).Entry;

    /// <summary>Whether the native part entries ascend, so the typed declaration order is the authored order.</summary>
    internal static bool PartEntriesAscend => PartEntries.Zip(PartEntries.Skip(1), (first, second) => first.Entry < second.Entry).All(ascends => ascends);

    /// <summary>Each typed rock program's native entry point in bank $86.</summary>
    internal static readonly RockEntry[] RockEntries =
    [
        new RockEntry(KraidRockSegment.SharedPose, 0x9c7d),
        new RockEntry(KraidRockSegment.RisingRightPose, 0x9c83),
        new RockEntry(KraidRockSegment.SpitRockShot, 0x9c89),
    ];
}
