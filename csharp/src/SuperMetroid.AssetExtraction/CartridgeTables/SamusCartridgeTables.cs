using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction;

/// <summary>Cartridge regions whose bank-local pointers Samus and projectile codecs resolve.</summary>
[CpuBusBoundary("catalog: Samus cartridge tables")]
internal static class SamusCartridgeTables
{
    /// <summary>
    /// $92:8000-$92:FFFF: Samus animation data — seven-byte body DMA definitions and Samus
    /// spritemaps addressed by bank-local pointers.
    /// </summary>
    internal static readonly CpuBusCell AnimationBank = CpuBusCell.CartridgeBytes(nameof(AnimationBank), 0x928000, 0x8000,
        "body DMA definitions (7-byte records) and spritemaps (u16 count + 5-byte parts)");

    /// <summary>
    /// $93:8000-$93:FFFF: projectile definitions and spritemaps. Projectile spritemap identifiers are
    /// bank-local pointers to a u16 part count followed by five-byte OAM parts.
    /// </summary>
    internal static readonly CpuBusCell ProjectileBank = CpuBusCell.CartridgeBytes(nameof(ProjectileBank), 0x938000, 0x8000,
        "projectile spritemaps (u16 count + 5-byte parts)");
}
