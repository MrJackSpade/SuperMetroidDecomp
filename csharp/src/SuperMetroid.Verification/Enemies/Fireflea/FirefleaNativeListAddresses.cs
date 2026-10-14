using SuperMetroid.Core.Enemies.Fireflea;

namespace SuperMetroid.Verification.Enemies.Fireflea;

/// <summary>
/// The native bank-$A3 lists the Fireflea ROM oracle (#641) walks: each program's entry point.
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class FirefleaNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xa3;

    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly (FirefleaProgram Program, ushort Entry)[] Entries =
    [
        (FirefleaProgram.Loop, 0x8c2f),
    ];

}
