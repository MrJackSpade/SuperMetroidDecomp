using SuperMetroid.Core.Enemies.KiHunters;

namespace SuperMetroid.Verification.Enemies.KiHunters;

/// <summary>
/// The native bank-$86 lists the Ki-Hunter acid spit ROM oracle (#641) walks: each program's entry point. Only verification
/// reads them; the port runs typed programs.
/// </summary>
internal static class KiHunterAcidSpitNativeListAddresses
{
    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly (KiHunterAcidSpitProgram Program, ushort Entry)[] Entries =
    [
        (KiHunterAcidSpitProgram.Left, 0xcf34),
        (KiHunterAcidSpitProgram.HitFloor, 0xcf56),
        (KiHunterAcidSpitProgram.Right, 0xcf6e),
    ];

    /// <summary>The bank-$86 bytes the lists occupy, $86:CF34-CF8F, which production must never read.</summary>
    internal static readonly (ushort First, ushort End) Extent = (0xcf34, 0xcf90);
}
