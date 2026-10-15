using SuperMetroid.Core.Enemies.Ripper;

namespace SuperMetroid.Verification.Enemies.Ripper;

/// <summary>
/// The native bank-$A2 lists the Ripper ROM oracle (#1290) walks: each program's entry point.
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class RipperNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xa2;

    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly (RipperProgram Program, ushort Entry)[] Entries =
    [
        (RipperProgram.GRipperMovingLeft, 0xe19b),
        (RipperProgram.GRipperMovingRight, 0xe1af),
        (RipperProgram.Ripper2MovingRight, 0xe2e0),
        (RipperProgram.Ripper2MovingLeft, 0xe2f4),
        (RipperProgram.RipperMovingRight, 0xe477),
        (RipperProgram.RipperMovingLeft, 0xe48b),
    ];

}
