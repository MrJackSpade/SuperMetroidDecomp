using SuperMetroid.Core.Enemies.Alcoon;

namespace SuperMetroid.Verification.Enemies.Alcoon;

/// <summary>
/// The native bank-$86 lists the Alcoon fireball ROM oracle (#641) walks: each program's entry point. Only verification
/// reads them; the port runs typed programs.
/// </summary>
internal static class AlcoonFireballNativeListAddresses
{
    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly (AlcoonFireballProgram Program, ushort Entry)[] Entries =
    [
        (AlcoonFireballProgram.Initial, 0x9e9e),
    ];

    /// <summary>The bank-$86 bytes the lists occupy, $86:9E9E-9EB1, which production must never read.</summary>
    internal static readonly (ushort First, ushort End) Extent = (0x9e9e, 0x9eb2);
}
