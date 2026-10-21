using SuperMetroid.Verification.Bosses.MotherBrain;
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
    internal static readonly NativeProgramEntry<WalkingSpacePirateProgram>[] Entries =
    [
        new NativeProgramEntry<WalkingSpacePirateProgram>(WalkingSpacePirateProgram.FlinchFacingLeft, 0xfb4c),
        new NativeProgramEntry<WalkingSpacePirateProgram>(WalkingSpacePirateProgram.FlinchFacingRight, 0xfb58),
        new NativeProgramEntry<WalkingSpacePirateProgram>(WalkingSpacePirateProgram.WalkingLeft, 0xfb64),
        new NativeProgramEntry<WalkingSpacePirateProgram>(WalkingSpacePirateProgram.WalkingLeftLoop, 0xfb68),
        new NativeProgramEntry<WalkingSpacePirateProgram>(WalkingSpacePirateProgram.FireLasersLeft, 0xfb8c),
        new NativeProgramEntry<WalkingSpacePirateProgram>(WalkingSpacePirateProgram.LookingFacingLeft, 0xfbc6),
        new NativeProgramEntry<WalkingSpacePirateProgram>(WalkingSpacePirateProgram.WalkingRight, 0xfbe6),
        new NativeProgramEntry<WalkingSpacePirateProgram>(WalkingSpacePirateProgram.WalkingRightLoop, 0xfbea),
        new NativeProgramEntry<WalkingSpacePirateProgram>(WalkingSpacePirateProgram.FireLasersRight, 0xfc0e),
        new NativeProgramEntry<WalkingSpacePirateProgram>(WalkingSpacePirateProgram.LookingFacingRight, 0xfc48),
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
