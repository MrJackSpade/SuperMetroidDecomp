using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Enemies.SpacePirates;

namespace SuperMetroid.Verification.Enemies.SpacePirates;

/// <summary>
/// The native bank-$B2 lists the Ninja Space Pirate ROM oracle (#641) walks: each program's entry point and the owner
/// instructions the lists use, with the cue each must become and its operand roles (T target, A alternate, V value).
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class NinjaSpacePirateNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xb2;

    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<NinjaSpacePirateSegment>[] Entries =
    [
        new NativeProgramEntry<NinjaSpacePirateSegment>(NinjaSpacePirateSegment.ClawAttackLeft, 0xf15c),
        new NativeProgramEntry<NinjaSpacePirateSegment>(NinjaSpacePirateSegment.SpinJumpLeft, 0xf1c4),
        new NativeProgramEntry<NinjaSpacePirateSegment>(NinjaSpacePirateSegment.SpinJumpLeftLoop, 0xf206),
        new NativeProgramEntry<NinjaSpacePirateSegment>(NinjaSpacePirateSegment.ActiveFacingLeft, 0xf22e),
        new NativeProgramEntry<NinjaSpacePirateSegment>(NinjaSpacePirateSegment.ActiveFacingLeftLoop, 0xf232),
        new NativeProgramEntry<NinjaSpacePirateSegment>(NinjaSpacePirateSegment.FlinchFacingLeft, 0xf270),
        new NativeProgramEntry<NinjaSpacePirateSegment>(NinjaSpacePirateSegment.DivekickLeftJump, 0xf27c),
        new NativeProgramEntry<NinjaSpacePirateSegment>(NinjaSpacePirateSegment.DivekickLeftJumpLoop, 0xf28a),
        new NativeProgramEntry<NinjaSpacePirateSegment>(NinjaSpacePirateSegment.DivekickLeftDive, 0xf2a0),
        new NativeProgramEntry<NinjaSpacePirateSegment>(NinjaSpacePirateSegment.WalkToLeftPost, 0xf2b2),
        new NativeProgramEntry<NinjaSpacePirateSegment>(NinjaSpacePirateSegment.WalkToLeftPostLoop, 0xf2b6),
        new NativeProgramEntry<NinjaSpacePirateSegment>(NinjaSpacePirateSegment.InitialFacingLeft, 0xf2da),
        new NativeProgramEntry<NinjaSpacePirateSegment>(NinjaSpacePirateSegment.InitialFacingLeftLoop, 0xf2de),
        new NativeProgramEntry<NinjaSpacePirateSegment>(NinjaSpacePirateSegment.LandFacingLeft, 0xf2f8),
        new NativeProgramEntry<NinjaSpacePirateSegment>(NinjaSpacePirateSegment.LandFacingLeftLoop, 0xf314),
        new NativeProgramEntry<NinjaSpacePirateSegment>(NinjaSpacePirateSegment.KickFacingLeft, 0xf32e),
        new NativeProgramEntry<NinjaSpacePirateSegment>(NinjaSpacePirateSegment.ClawAttackRight, 0xf34a),
        new NativeProgramEntry<NinjaSpacePirateSegment>(NinjaSpacePirateSegment.SpinJumpRight, 0xf3b2),
        new NativeProgramEntry<NinjaSpacePirateSegment>(NinjaSpacePirateSegment.SpinJumpRightLoop, 0xf3f4),
        new NativeProgramEntry<NinjaSpacePirateSegment>(NinjaSpacePirateSegment.ActiveFacingRight, 0xf420),
        new NativeProgramEntry<NinjaSpacePirateSegment>(NinjaSpacePirateSegment.ActiveFacingRightLoop, 0xf424),
        new NativeProgramEntry<NinjaSpacePirateSegment>(NinjaSpacePirateSegment.FlinchFacingRight, 0xf462),
        new NativeProgramEntry<NinjaSpacePirateSegment>(NinjaSpacePirateSegment.DivekickRightJump, 0xf46e),
        new NativeProgramEntry<NinjaSpacePirateSegment>(NinjaSpacePirateSegment.DivekickRightJumpLoop, 0xf47c),
        new NativeProgramEntry<NinjaSpacePirateSegment>(NinjaSpacePirateSegment.DivekickRightDive, 0xf492),
        new NativeProgramEntry<NinjaSpacePirateSegment>(NinjaSpacePirateSegment.WalkToRightPost, 0xf4a4),
        new NativeProgramEntry<NinjaSpacePirateSegment>(NinjaSpacePirateSegment.WalkToRightPostLoop, 0xf4a8),
        new NativeProgramEntry<NinjaSpacePirateSegment>(NinjaSpacePirateSegment.InitialFacingRight, 0xf4cc),
        new NativeProgramEntry<NinjaSpacePirateSegment>(NinjaSpacePirateSegment.InitialFacingRightLoop, 0xf4d0),
        new NativeProgramEntry<NinjaSpacePirateSegment>(NinjaSpacePirateSegment.LandFacingRight, 0xf4ea),
        new NativeProgramEntry<NinjaSpacePirateSegment>(NinjaSpacePirateSegment.LandFacingRightLoop, 0xf506),
        new NativeProgramEntry<NinjaSpacePirateSegment>(NinjaSpacePirateSegment.KickFacingRight, 0xf51a),
    ];

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, NativeCueShape<NinjaSpacePirateCue>> Instructions = new()
    {
        [0xef83] = NativeCueShape<NinjaSpacePirateCue>.Of<NinjaSpacePirateCue.SetAction>("E"),
        [0xf536] = NativeCueShape<NinjaSpacePirateCue>.Of<NinjaSpacePirateCue.SetShading>("V"),
        [0xf546] = NativeCueShape<NinjaSpacePirateCue>.Of<NinjaSpacePirateCue.PlaySound>("V"),
        [0xf564] = NativeCueShape<NinjaSpacePirateCue>.Of<NinjaSpacePirateCue.ThrowClaw>("VSY"),
        [0xf590] = NativeCueShape<NinjaSpacePirateCue>.Exactly(new NinjaSpacePirateCue.Signal(NinjaSpacePirateCueKind.StartActive)),
        [0xf5d6] = NativeCueShape<NinjaSpacePirateCue>.Exactly(new NinjaSpacePirateCue.Signal(NinjaSpacePirateCueKind.ResetSpeed)),
        [0xf969] = NativeCueShape<NinjaSpacePirateCue>.Exactly(new NinjaSpacePirateCue.Signal(NinjaSpacePirateCueKind.StartLeftDivekick)),
        [0xfa3d] = NativeCueShape<NinjaSpacePirateCue>.Exactly(new NinjaSpacePirateCue.Signal(NinjaSpacePirateCueKind.StartRightDivekick)),
    };
}
