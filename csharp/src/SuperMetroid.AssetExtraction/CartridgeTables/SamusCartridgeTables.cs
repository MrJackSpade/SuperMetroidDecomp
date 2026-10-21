using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction.CartridgeTables;

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

    /// <summary>$92:90ED: the first Samus spritemap; spritemap pointers below it are invalid.</summary>
    internal const int FirstSpritemap = 0x90ed;

    /// <summary>$92:808D: the spritemap pointer of each pose-half table entry.</summary>
    internal const int SpritemapPointers = 0x92808d;

    /// <summary>$92:9263: each pose's first upper-half table entry.</summary>
    internal const int SpritemapTopBases = 0x929263;

    /// <summary>$92:945D: each pose's first lower-half table entry.</summary>
    internal const int SpritemapBottomBases = 0x92945d;

    /// <summary>$92:CBEE: the first seven-byte body DMA definition record; the rest follow it.</summary>
    internal const int FirstBodyDefinition = 0xcbee;

    /// <summary>Bytes in one body DMA definition record.</summary>
    internal const int BodyDefinitionBytes = 7;

    /// <summary>$92:DB48: the first four-byte body frame selector; the rest follow it.</summary>
    internal const int BodyFrames = 0x92db48;

    /// <summary>The bank-$92 address of the first body frame selector.</summary>
    internal const int FirstBodyFrame = 0xdb48;
}
