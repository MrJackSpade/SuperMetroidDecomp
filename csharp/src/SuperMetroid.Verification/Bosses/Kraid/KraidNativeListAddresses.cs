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
    internal static readonly (KraidPartProgram Program, ushort Entry)[] PartEntries =
    [
        (KraidPartProgram.FootInitial, 0x86e7),
        (KraidPartProgram.FootNeutral, 0x86ed),
        (KraidPartProgram.FootWalkingForward, 0x86f3),
        (KraidPartProgram.FootWalkingForwardFinished, 0x87bb),
        (KraidPartProgram.FootLungeForward, 0x87bd),
        (KraidPartProgram.FootLungeForwardFinished, 0x8885),
        (KraidPartProgram.FootWalkingBackward, 0x8887),
        (KraidPartProgram.FootWalkingBackwardLoop, 0x8939),
        (KraidPartProgram.ArmNormal, 0x89f3),
        (KraidPartProgram.ArmNormalPause, 0x8a37),
        (KraidPartProgram.ArmSlow, 0x8a41),
        (KraidPartProgram.ArmRisingOrSinking, 0x8aa4),
        (KraidPartProgram.ArmDyingOrPreparingToLunge, 0x8af0),
        (KraidPartProgram.LintInitial, 0x8afe),
        (KraidPartProgram.LintPostGrowth, 0x8b04),
        (KraidPartProgram.NailLoop, 0x8b0a),
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
    internal static KraidPartProgram? PartProgramAt(ushort address) =>
        PartEntries.Where(item => item.Entry == address).Select(item => (KraidPartProgram?)item.Program).SingleOrDefault();

    /// <summary>The native entry point of <paramref name="program"/>.</summary>
    internal static ushort PartEntry(KraidPartProgram program) => PartEntries.Single(item => item.Program == program).Entry;

    /// <summary>Whether the native part entries ascend, so the typed declaration order is the authored order.</summary>
    internal static bool PartEntriesAscend => PartEntries.Zip(PartEntries.Skip(1)).All(pair => pair.First.Entry < pair.Second.Entry);

    /// <summary>Each typed rock program's native entry point in bank $86.</summary>
    internal static readonly (KraidRockProgram Program, ushort Entry)[] RockEntries =
    [
        (KraidRockProgram.SharedPose, 0x9c7d),
        (KraidRockProgram.RisingRightPose, 0x9c83),
        (KraidRockProgram.SpitRockShot, 0x9c89),
    ];
}
