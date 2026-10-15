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
    internal static readonly (EtecoonProgram Program, ushort Entry)[] Entries =
    [
        (EtecoonProgram.LookRightAtSamusAndRunLeft, 0xe81e),
        (EtecoonProgram.BeginRunningLeft, 0xe824),
        (EtecoonProgram.RunningLeft, 0xe828),
        (EtecoonProgram.WallJumpLeft, 0xe83c),
        (EtecoonProgram.WallJumpLeftLoop, 0xe840),
        (EtecoonProgram.HoppingFacingLeft, 0xe854),
        (EtecoonProgram.ContinueHoppingFacingLeft, 0xe85a),
        (EtecoonProgram.HitCeiling, 0xe862),
        (EtecoonProgram.WallJumpLeftEligible, 0xe870),
        (EtecoonProgram.LookLeftAtSamusAndRunRight, 0xe876),
        (EtecoonProgram.BeginRunningRight, 0xe87c),
        (EtecoonProgram.RunningRight, 0xe880),
        (EtecoonProgram.WallJumpRight, 0xe894),
        (EtecoonProgram.JumpingRight, 0xe898),
        (EtecoonProgram.HoppingFacingRight, 0xe8ac),
        (EtecoonProgram.ContinueHoppingFacingRight, 0xe8b2),
        (EtecoonProgram.WallJumpRightEligible, 0xe8c8),
        (EtecoonProgram.Initial, 0xe8ce),
        (EtecoonProgram.Flexing, 0xe8d6),
        (EtecoonProgram.FlexingLoop, 0xe8da),
    ];

}
