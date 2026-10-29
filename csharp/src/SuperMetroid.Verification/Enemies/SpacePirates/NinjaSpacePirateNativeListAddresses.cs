using SuperMetroid.Verification.Bosses.MotherBrain;
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
    internal static readonly NativeProgramEntry<NinjaSpacePirateProgram>[] Entries =
    [
        new NativeProgramEntry<NinjaSpacePirateProgram>(NinjaSpacePirateProgram.ClawAttackLeft, 0xf15c),
        new NativeProgramEntry<NinjaSpacePirateProgram>(NinjaSpacePirateProgram.SpinJumpLeft, 0xf1c4),
        new NativeProgramEntry<NinjaSpacePirateProgram>(NinjaSpacePirateProgram.SpinJumpLeftLoop, 0xf206),
        new NativeProgramEntry<NinjaSpacePirateProgram>(NinjaSpacePirateProgram.ActiveFacingLeft, 0xf22e),
        new NativeProgramEntry<NinjaSpacePirateProgram>(NinjaSpacePirateProgram.ActiveFacingLeftLoop, 0xf232),
        new NativeProgramEntry<NinjaSpacePirateProgram>(NinjaSpacePirateProgram.FlinchFacingLeft, 0xf270),
        new NativeProgramEntry<NinjaSpacePirateProgram>(NinjaSpacePirateProgram.DivekickLeftJump, 0xf27c),
        new NativeProgramEntry<NinjaSpacePirateProgram>(NinjaSpacePirateProgram.DivekickLeftJumpLoop, 0xf28a),
        new NativeProgramEntry<NinjaSpacePirateProgram>(NinjaSpacePirateProgram.DivekickLeftDive, 0xf2a0),
        new NativeProgramEntry<NinjaSpacePirateProgram>(NinjaSpacePirateProgram.WalkToLeftPost, 0xf2b2),
        new NativeProgramEntry<NinjaSpacePirateProgram>(NinjaSpacePirateProgram.WalkToLeftPostLoop, 0xf2b6),
        new NativeProgramEntry<NinjaSpacePirateProgram>(NinjaSpacePirateProgram.InitialFacingLeft, 0xf2da),
        new NativeProgramEntry<NinjaSpacePirateProgram>(NinjaSpacePirateProgram.InitialFacingLeftLoop, 0xf2de),
        new NativeProgramEntry<NinjaSpacePirateProgram>(NinjaSpacePirateProgram.LandFacingLeft, 0xf2f8),
        new NativeProgramEntry<NinjaSpacePirateProgram>(NinjaSpacePirateProgram.LandFacingLeftLoop, 0xf314),
        new NativeProgramEntry<NinjaSpacePirateProgram>(NinjaSpacePirateProgram.KickFacingLeft, 0xf32e),
        new NativeProgramEntry<NinjaSpacePirateProgram>(NinjaSpacePirateProgram.ClawAttackRight, 0xf34a),
        new NativeProgramEntry<NinjaSpacePirateProgram>(NinjaSpacePirateProgram.SpinJumpRight, 0xf3b2),
        new NativeProgramEntry<NinjaSpacePirateProgram>(NinjaSpacePirateProgram.SpinJumpRightLoop, 0xf3f4),
        new NativeProgramEntry<NinjaSpacePirateProgram>(NinjaSpacePirateProgram.ActiveFacingRight, 0xf420),
        new NativeProgramEntry<NinjaSpacePirateProgram>(NinjaSpacePirateProgram.ActiveFacingRightLoop, 0xf424),
        new NativeProgramEntry<NinjaSpacePirateProgram>(NinjaSpacePirateProgram.FlinchFacingRight, 0xf462),
        new NativeProgramEntry<NinjaSpacePirateProgram>(NinjaSpacePirateProgram.DivekickRightJump, 0xf46e),
        new NativeProgramEntry<NinjaSpacePirateProgram>(NinjaSpacePirateProgram.DivekickRightJumpLoop, 0xf47c),
        new NativeProgramEntry<NinjaSpacePirateProgram>(NinjaSpacePirateProgram.DivekickRightDive, 0xf492),
        new NativeProgramEntry<NinjaSpacePirateProgram>(NinjaSpacePirateProgram.WalkToRightPost, 0xf4a4),
        new NativeProgramEntry<NinjaSpacePirateProgram>(NinjaSpacePirateProgram.WalkToRightPostLoop, 0xf4a8),
        new NativeProgramEntry<NinjaSpacePirateProgram>(NinjaSpacePirateProgram.InitialFacingRight, 0xf4cc),
        new NativeProgramEntry<NinjaSpacePirateProgram>(NinjaSpacePirateProgram.InitialFacingRightLoop, 0xf4d0),
        new NativeProgramEntry<NinjaSpacePirateProgram>(NinjaSpacePirateProgram.LandFacingRight, 0xf4ea),
        new NativeProgramEntry<NinjaSpacePirateProgram>(NinjaSpacePirateProgram.LandFacingRightLoop, 0xf506),
        new NativeProgramEntry<NinjaSpacePirateProgram>(NinjaSpacePirateProgram.KickFacingRight, 0xf51a),
    ];

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, NativeCueShape<NinjaSpacePirateCue>> Instructions = new()
    {
        [0xef83] = NativeCueShape<NinjaSpacePirateCue>.Of<NinjaSpacePirateCue.SetFunction>("E"),
        [0xf536] = NativeCueShape<NinjaSpacePirateCue>.Of<NinjaSpacePirateCue.SetColouring>("V"),
        [0xf546] = NativeCueShape<NinjaSpacePirateCue>.Of<NinjaSpacePirateCue.PlaySound>("V"),
        [0xf564] = NativeCueShape<NinjaSpacePirateCue>.Of<NinjaSpacePirateCue.ThrowClaw>("VSY"),
        [0xf590] = NativeCueShape<NinjaSpacePirateCue>.Exactly(new NinjaSpacePirateCue.Command(NinjaSpacePirateCueKind.StartActive)),
        [0xf5d6] = NativeCueShape<NinjaSpacePirateCue>.Exactly(new NinjaSpacePirateCue.Command(NinjaSpacePirateCueKind.ResetSpeed)),
        [0xf969] = NativeCueShape<NinjaSpacePirateCue>.Exactly(new NinjaSpacePirateCue.Command(NinjaSpacePirateCueKind.StartLeftDivekick)),
        [0xfa3d] = NativeCueShape<NinjaSpacePirateCue>.Exactly(new NinjaSpacePirateCue.Command(NinjaSpacePirateCueKind.StartRightDivekick)),
    };
}
