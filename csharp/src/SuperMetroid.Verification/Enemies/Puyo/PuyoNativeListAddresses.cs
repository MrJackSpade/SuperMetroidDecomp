using SuperMetroid.Core.Enemies.Puyo;

namespace SuperMetroid.Verification.Enemies.Puyo;

/// <summary>
/// The native bank-$A2 lists the Puyo ROM oracle (#641) walks: each program's entry point.
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class PuyoNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xa2;

    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly (PuyoProgram Program, ushort Entry)[] Entries =
    [
        (PuyoProgram.GroundedFast, 0x99ad),
        (PuyoProgram.GroundedMedium, 0x99c1),
        (PuyoProgram.GroundedSlow, 0x99d5),
        (PuyoProgram.RightFrame0LeftFrame4, 0x99e9),
        (PuyoProgram.RightFrame1LeftFrame3, 0x99ef),
        (PuyoProgram.Frame2, 0x99f5),
        (PuyoProgram.RightFrame3LeftFrame1, 0x99fb),
        (PuyoProgram.RightFrame4LeftFrame0, 0x9a01),
    ];

}
