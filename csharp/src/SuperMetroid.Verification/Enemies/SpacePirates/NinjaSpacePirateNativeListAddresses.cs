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
    internal static readonly (NinjaSpacePirateProgram Program, ushort Entry)[] Entries =
    [
        (NinjaSpacePirateProgram.ClawAttackLeft, 0xf15c),
        (NinjaSpacePirateProgram.SpinJumpLeft, 0xf1c4),
        (NinjaSpacePirateProgram.SpinJumpLeftLoop, 0xf206),
        (NinjaSpacePirateProgram.ActiveFacingLeft, 0xf22e),
        (NinjaSpacePirateProgram.ActiveFacingLeftLoop, 0xf232),
        (NinjaSpacePirateProgram.FlinchFacingLeft, 0xf270),
        (NinjaSpacePirateProgram.DivekickLeftJump, 0xf27c),
        (NinjaSpacePirateProgram.DivekickLeftJumpLoop, 0xf28a),
        (NinjaSpacePirateProgram.DivekickLeftDive, 0xf2a0),
        (NinjaSpacePirateProgram.WalkToLeftPost, 0xf2b2),
        (NinjaSpacePirateProgram.WalkToLeftPostLoop, 0xf2b6),
        (NinjaSpacePirateProgram.InitialFacingLeft, 0xf2da),
        (NinjaSpacePirateProgram.InitialFacingLeftLoop, 0xf2de),
        (NinjaSpacePirateProgram.LandFacingLeft, 0xf2f8),
        (NinjaSpacePirateProgram.LandFacingLeftLoop, 0xf314),
        (NinjaSpacePirateProgram.KickFacingLeft, 0xf32e),
        (NinjaSpacePirateProgram.ClawAttackRight, 0xf34a),
        (NinjaSpacePirateProgram.SpinJumpRight, 0xf3b2),
        (NinjaSpacePirateProgram.SpinJumpRightLoop, 0xf3f4),
        (NinjaSpacePirateProgram.ActiveFacingRight, 0xf420),
        (NinjaSpacePirateProgram.ActiveFacingRightLoop, 0xf424),
        (NinjaSpacePirateProgram.FlinchFacingRight, 0xf462),
        (NinjaSpacePirateProgram.DivekickRightJump, 0xf46e),
        (NinjaSpacePirateProgram.DivekickRightJumpLoop, 0xf47c),
        (NinjaSpacePirateProgram.DivekickRightDive, 0xf492),
        (NinjaSpacePirateProgram.WalkToRightPost, 0xf4a4),
        (NinjaSpacePirateProgram.WalkToRightPostLoop, 0xf4a8),
        (NinjaSpacePirateProgram.InitialFacingRight, 0xf4cc),
        (NinjaSpacePirateProgram.InitialFacingRightLoop, 0xf4d0),
        (NinjaSpacePirateProgram.LandFacingRight, 0xf4ea),
        (NinjaSpacePirateProgram.LandFacingRightLoop, 0xf506),
        (NinjaSpacePirateProgram.KickFacingRight, 0xf51a),
    ];

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, NativeCueShape<NinjaSpacePirateCue>> Instructions = new()
    {
        [0xef83] = NativeCueShape<NinjaSpacePirateCue>.Of<NinjaSpacePirateCue.SetFunction>("E"),
        [0xf536] = NativeCueShape<NinjaSpacePirateCue>.Of<NinjaSpacePirateCue.SetPalette>("V"),
        [0xf546] = NativeCueShape<NinjaSpacePirateCue>.Of<NinjaSpacePirateCue.PlaySound>("V"),
        [0xf564] = NativeCueShape<NinjaSpacePirateCue>.Of<NinjaSpacePirateCue.ThrowClaw>("VSY"),
        [0xf590] = NativeCueShape<NinjaSpacePirateCue>.Exactly(new NinjaSpacePirateCue.Command(NinjaSpacePirateCueKind.StartActive)),
        [0xf5d6] = NativeCueShape<NinjaSpacePirateCue>.Exactly(new NinjaSpacePirateCue.Command(NinjaSpacePirateCueKind.ResetSpeed)),
        [0xf969] = NativeCueShape<NinjaSpacePirateCue>.Exactly(new NinjaSpacePirateCue.Command(NinjaSpacePirateCueKind.StartLeftDivekick)),
        [0xfa3d] = NativeCueShape<NinjaSpacePirateCue>.Exactly(new NinjaSpacePirateCue.Command(NinjaSpacePirateCueKind.StartRightDivekick)),
    };
}
