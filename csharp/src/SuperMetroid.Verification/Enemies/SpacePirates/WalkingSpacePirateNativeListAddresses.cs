using SuperMetroid.Core.Enemies.SpacePirates;

namespace SuperMetroid.Verification.Enemies.SpacePirates;

/// <summary>
/// The native bank-$B2 lists the Walking Space Pirate ROM oracle (#641) walks: each program's entry point and the owner
/// instructions the lists use, with the cue each must become and its operand roles (T target, A alternate, V value).
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class WalkingSpacePirateNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xb2;

    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly (WalkingSpacePirateProgram Program, ushort Entry)[] Entries =
    [
        (WalkingSpacePirateProgram.FlinchFacingLeft, 0xfb4c),
        (WalkingSpacePirateProgram.FlinchFacingRight, 0xfb58),
        (WalkingSpacePirateProgram.WalkingLeft, 0xfb64),
        (WalkingSpacePirateProgram.WalkingLeftLoop, 0xfb68),
        (WalkingSpacePirateProgram.FireLasersLeft, 0xfb8c),
        (WalkingSpacePirateProgram.LookingFacingLeft, 0xfbc6),
        (WalkingSpacePirateProgram.WalkingRight, 0xfbe6),
        (WalkingSpacePirateProgram.WalkingRightLoop, 0xfbea),
        (WalkingSpacePirateProgram.FireLasersRight, 0xfc0e),
        (WalkingSpacePirateProgram.LookingFacingRight, 0xfc48),
    ];

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, NativeCueShape<WalkingSpacePirateCue>> Instructions = new()
    {
        [0xfc68] = NativeCueShape<WalkingSpacePirateCue>.Of<WalkingSpacePirateCue.FireLaserLeft>("V"),
        [0xfc90] = NativeCueShape<WalkingSpacePirateCue>.Of<WalkingSpacePirateCue.FireLaserRight>("V"),
        [0xfcb8] = NativeCueShape<WalkingSpacePirateCue>.Of<WalkingSpacePirateCue.SetFunction>("E"),
        [0xfcc8] = NativeCueShape<WalkingSpacePirateCue>.Exactly(new WalkingSpacePirateCue.Command(WalkingSpacePirateCueKind.ChooseMovement)),
    };
}
