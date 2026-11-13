using SuperMetroid.TestSupport.MovieReplay;
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
    internal static readonly NativeProgramEntry<EtecoonSegment>[] Entries =
    [
        new NativeProgramEntry<EtecoonSegment>(EtecoonSegment.LookRightAtSamusAndRunLeft, 0xe81e),
        new NativeProgramEntry<EtecoonSegment>(EtecoonSegment.BeginRunningLeft, 0xe824),
        new NativeProgramEntry<EtecoonSegment>(EtecoonSegment.RunningLeft, 0xe828),
        new NativeProgramEntry<EtecoonSegment>(EtecoonSegment.WallJumpLeft, 0xe83c),
        new NativeProgramEntry<EtecoonSegment>(EtecoonSegment.WallJumpLeftLoop, 0xe840),
        new NativeProgramEntry<EtecoonSegment>(EtecoonSegment.HoppingFacingLeft, 0xe854),
        new NativeProgramEntry<EtecoonSegment>(EtecoonSegment.ContinueHoppingFacingLeft, 0xe85a),
        new NativeProgramEntry<EtecoonSegment>(EtecoonSegment.HitCeiling, 0xe862),
        new NativeProgramEntry<EtecoonSegment>(EtecoonSegment.WallJumpLeftEligible, 0xe870),
        new NativeProgramEntry<EtecoonSegment>(EtecoonSegment.LookLeftAtSamusAndRunRight, 0xe876),
        new NativeProgramEntry<EtecoonSegment>(EtecoonSegment.BeginRunningRight, 0xe87c),
        new NativeProgramEntry<EtecoonSegment>(EtecoonSegment.RunningRight, 0xe880),
        new NativeProgramEntry<EtecoonSegment>(EtecoonSegment.WallJumpRight, 0xe894),
        new NativeProgramEntry<EtecoonSegment>(EtecoonSegment.JumpingRight, 0xe898),
        new NativeProgramEntry<EtecoonSegment>(EtecoonSegment.HoppingFacingRight, 0xe8ac),
        new NativeProgramEntry<EtecoonSegment>(EtecoonSegment.ContinueHoppingFacingRight, 0xe8b2),
        new NativeProgramEntry<EtecoonSegment>(EtecoonSegment.WallJumpRightEligible, 0xe8c8),
        new NativeProgramEntry<EtecoonSegment>(EtecoonSegment.Initial, 0xe8ce),
        new NativeProgramEntry<EtecoonSegment>(EtecoonSegment.Flexing, 0xe8d6),
        new NativeProgramEntry<EtecoonSegment>(EtecoonSegment.FlexingLoop, 0xe8da),
    ];

}
