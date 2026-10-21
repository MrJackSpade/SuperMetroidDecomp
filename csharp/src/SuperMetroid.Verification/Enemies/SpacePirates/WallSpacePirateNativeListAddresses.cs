using SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies.SpacePirates;

namespace SuperMetroid.Verification.Enemies.SpacePirates;

/// <summary>
/// The native bank-$B2 lists the Wall Space Pirate ROM oracle (#641) walks: each program's entry point and the owner
/// instructions the lists use, with the cue each must become and its operand roles (T target, A alternate, V value).
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class WallSpacePirateNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xb2;

    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<WallSpacePirateProgram>[] Entries =
    [
        new NativeProgramEntry<WallSpacePirateProgram>(WallSpacePirateProgram.FireAndJumpLeft, 0xecc0),
        new NativeProgramEntry<WallSpacePirateProgram>(WallSpacePirateProgram.LandedOnLeftWall, 0xece4),
        new NativeProgramEntry<WallSpacePirateProgram>(WallSpacePirateProgram.MovingUpLeftWall, 0xecec),
        new NativeProgramEntry<WallSpacePirateProgram>(WallSpacePirateProgram.MovingUpLeftWallLoop, 0xecf4),
        new NativeProgramEntry<WallSpacePirateProgram>(WallSpacePirateProgram.MovingDownLeftWall, 0xed36),
        new NativeProgramEntry<WallSpacePirateProgram>(WallSpacePirateProgram.MovingDownLeftWallLoop, 0xed3e),
        new NativeProgramEntry<WallSpacePirateProgram>(WallSpacePirateProgram.FireAndJumpRight, 0xed80),
        new NativeProgramEntry<WallSpacePirateProgram>(WallSpacePirateProgram.LandedOnRightWall, 0xeda4),
        new NativeProgramEntry<WallSpacePirateProgram>(WallSpacePirateProgram.MovingDownRightWall, 0xedac),
        new NativeProgramEntry<WallSpacePirateProgram>(WallSpacePirateProgram.MovingDownRightWallLoop, 0xedb4),
        new NativeProgramEntry<WallSpacePirateProgram>(WallSpacePirateProgram.MovingUpRightWall, 0xedf6),
        new NativeProgramEntry<WallSpacePirateProgram>(WallSpacePirateProgram.MovingUpRightWallLoop, 0xedfe),
    ];

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, NativeCueShape<WallSpacePirateCue>> Instructions = new()
    {
        [0xee40] = NativeCueShape<WallSpacePirateCue>.Of<WallSpacePirateCue.ClimbLeftWall>("V"),
        [0xee72] = NativeCueShape<WallSpacePirateCue>.Of<WallSpacePirateCue.ClimbRightWall>("V"),
        [0xeea4] = NativeCueShape<WallSpacePirateCue>.Exactly(new WallSpacePirateCue.Command(WallSpacePirateCueKind.ChooseLeftWallDirection)),
        [0xeebc] = NativeCueShape<WallSpacePirateCue>.Exactly(new WallSpacePirateCue.Command(WallSpacePirateCueKind.ChooseRightWallDirection)),
        [0xeed4] = NativeCueShape<WallSpacePirateCue>.Exactly(new WallSpacePirateCue.Command(WallSpacePirateCueKind.PrepareJumpRight)),
        [0xeefd] = NativeCueShape<WallSpacePirateCue>.Exactly(new WallSpacePirateCue.Command(WallSpacePirateCueKind.PrepareJumpLeft)),
        [0xef2a] = NativeCueShape<WallSpacePirateCue>.Exactly(new WallSpacePirateCue.Command(WallSpacePirateCueKind.FireLaserLeft)),
        [0xef5d] = NativeCueShape<WallSpacePirateCue>.Exactly(new WallSpacePirateCue.Command(WallSpacePirateCueKind.FireLaserRight)),
        [0xef83] = NativeCueShape<WallSpacePirateCue>.Of<WallSpacePirateCue.SetFunction>("E"),
        [0xef93] = NativeCueShape<WallSpacePirateCue>.Exactly(new WallSpacePirateCue.Command(WallSpacePirateCueKind.PlayAttackSound)),
    };
}
