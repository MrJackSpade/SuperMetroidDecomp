using SuperMetroid.Core.Enemies.Polyp;

namespace SuperMetroid.Verification.Enemies.Polyp;

/// <summary>
/// The native bank-$86 lists the Polyp lava rock ROM oracle (#641) walks: each program's entry point. Only verification
/// reads them; the port runs typed programs.
/// </summary>
internal static class PolypRockNativeListAddresses
{
    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly (PolypRockProgram Program, ushort Entry)[] Entries =
    [
        (PolypRockProgram.Initial, 0xbbd5),
    ];

    /// <summary>The bank-$86 bytes the lists occupy, $86:BBD5-BBDA, which production must never read.</summary>
    internal static readonly (ushort First, ushort End) Extent = (0xbbd5, 0xbbdb);
}
