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
    internal static readonly (WallSpacePirateProgram Program, ushort Entry)[] Entries =
    [
        (WallSpacePirateProgram.FireAndJumpLeft, 0xecc0),
        (WallSpacePirateProgram.LandedOnLeftWall, 0xece4),
        (WallSpacePirateProgram.MovingUpLeftWall, 0xecec),
        (WallSpacePirateProgram.MovingUpLeftWallLoop, 0xecf4),
        (WallSpacePirateProgram.MovingDownLeftWall, 0xed36),
        (WallSpacePirateProgram.MovingDownLeftWallLoop, 0xed3e),
        (WallSpacePirateProgram.FireAndJumpRight, 0xed80),
        (WallSpacePirateProgram.LandedOnRightWall, 0xeda4),
        (WallSpacePirateProgram.MovingDownRightWall, 0xedac),
        (WallSpacePirateProgram.MovingDownRightWallLoop, 0xedb4),
        (WallSpacePirateProgram.MovingUpRightWall, 0xedf6),
        (WallSpacePirateProgram.MovingUpRightWallLoop, 0xedfe),
    ];

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, (WallSpacePirateCueKind Kind, string Operands)> Instructions = new()
    {
        [0xee40] = (WallSpacePirateCueKind.ClimbLeftWall, "V"),
        [0xee72] = (WallSpacePirateCueKind.ClimbRightWall, "V"),
        [0xeea4] = (WallSpacePirateCueKind.ChooseLeftWallDirection, ""),
        [0xeebc] = (WallSpacePirateCueKind.ChooseRightWallDirection, ""),
        [0xeed4] = (WallSpacePirateCueKind.PrepareJumpRight, ""),
        [0xeefd] = (WallSpacePirateCueKind.PrepareJumpLeft, ""),
        [0xef2a] = (WallSpacePirateCueKind.FireLaserLeft, ""),
        [0xef5d] = (WallSpacePirateCueKind.FireLaserRight, ""),
        [0xef83] = (WallSpacePirateCueKind.SetFunction, "E"),
        [0xef93] = (WallSpacePirateCueKind.PlayAttackSound, ""),
    };
}
