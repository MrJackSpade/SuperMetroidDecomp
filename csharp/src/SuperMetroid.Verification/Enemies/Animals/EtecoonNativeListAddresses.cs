using SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies.Animals;

namespace SuperMetroid.Verification.Enemies.Animals;

/// <summary>
/// The native bank-$A7 lists the Etecoon ROM oracle (#641) walks: each program's entry point.
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class EtecoonNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xa7;

    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<EtecoonProgram>[] Entries =
    [
        new NativeProgramEntry<EtecoonProgram>(EtecoonProgram.LookRightAtSamusAndRunLeft, 0xe81e),
        new NativeProgramEntry<EtecoonProgram>(EtecoonProgram.BeginRunningLeft, 0xe824),
        new NativeProgramEntry<EtecoonProgram>(EtecoonProgram.RunningLeft, 0xe828),
        new NativeProgramEntry<EtecoonProgram>(EtecoonProgram.WallJumpLeft, 0xe83c),
        new NativeProgramEntry<EtecoonProgram>(EtecoonProgram.WallJumpLeftLoop, 0xe840),
        new NativeProgramEntry<EtecoonProgram>(EtecoonProgram.HoppingFacingLeft, 0xe854),
        new NativeProgramEntry<EtecoonProgram>(EtecoonProgram.ContinueHoppingFacingLeft, 0xe85a),
        new NativeProgramEntry<EtecoonProgram>(EtecoonProgram.HitCeiling, 0xe862),
        new NativeProgramEntry<EtecoonProgram>(EtecoonProgram.WallJumpLeftEligible, 0xe870),
        new NativeProgramEntry<EtecoonProgram>(EtecoonProgram.LookLeftAtSamusAndRunRight, 0xe876),
        new NativeProgramEntry<EtecoonProgram>(EtecoonProgram.BeginRunningRight, 0xe87c),
        new NativeProgramEntry<EtecoonProgram>(EtecoonProgram.RunningRight, 0xe880),
        new NativeProgramEntry<EtecoonProgram>(EtecoonProgram.WallJumpRight, 0xe894),
        new NativeProgramEntry<EtecoonProgram>(EtecoonProgram.JumpingRight, 0xe898),
        new NativeProgramEntry<EtecoonProgram>(EtecoonProgram.HoppingFacingRight, 0xe8ac),
        new NativeProgramEntry<EtecoonProgram>(EtecoonProgram.ContinueHoppingFacingRight, 0xe8b2),
        new NativeProgramEntry<EtecoonProgram>(EtecoonProgram.WallJumpRightEligible, 0xe8c8),
        new NativeProgramEntry<EtecoonProgram>(EtecoonProgram.Initial, 0xe8ce),
        new NativeProgramEntry<EtecoonProgram>(EtecoonProgram.Flexing, 0xe8d6),
        new NativeProgramEntry<EtecoonProgram>(EtecoonProgram.FlexingLoop, 0xe8da),
    ];

}
