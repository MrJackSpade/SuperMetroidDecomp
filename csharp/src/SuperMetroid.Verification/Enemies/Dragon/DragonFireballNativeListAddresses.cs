using SuperMetroid.Core.Enemies.Dragon;

namespace SuperMetroid.Verification.Enemies.Dragon;

/// <summary>
/// The native bank-$86 lists the Dragon fireball ROM oracle (#641) walks: each program's entry point. Only verification
/// reads them; the port runs typed programs.
/// </summary>
internal static class DragonFireballNativeListAddresses
{
    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly (DragonFireballProgram Program, ushort Entry)[] Entries =
    [
        (DragonFireballProgram.RisingLeft, 0xb4bf),
        (DragonFireballProgram.RisingRight, 0xb4cb),
        (DragonFireballProgram.FallingLeft, 0xb4d7),
        (DragonFireballProgram.FallingRight, 0xb4e3),
    ];

    /// <summary>The bank-$86 bytes the lists occupy, $86:B4BF-B4EE, which production must never read.</summary>
    internal static readonly (ushort First, ushort End) Extent = (0xb4bf, 0xb4ef);
}
