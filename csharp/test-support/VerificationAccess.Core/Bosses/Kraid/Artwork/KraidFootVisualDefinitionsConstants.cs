using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Kraid.Artwork;

/// <summary>Cartridge values and helpers of <see cref="KraidFootVisualDefinitions"/> that only verification reads.</summary>
internal static class KraidFootVisualDefinitionsConstants
{
    /// <summary>$A7:8CE3, ExtendedSpritemap_KraidFoot_0; each root has two components.</summary>
    internal const ushort MovingStart = 0x8ce3;
    /// <summary>$A7:A565, ExtendedSpritemap_KraidFoot_Initial, sorted after moving roots.</summary>
    internal const ushort InitialFrame = 0xa565;
    /// <summary>Selected moving roots use eighteen-byte records (two count bytes
    /// plus two eight-byte components). Native ordinal $21 is never selected by
    /// the compiled programs; skip it and append the separate initial root.</summary>
    internal static NativeEnemySpritemapDefinition Frame(int index)
    {
        if ((uint)index >= KraidFootVisualDefinitions.FrameCount) throw new ArgumentOutOfRangeException(nameof(index));
        ushort pointer = index == KraidFootVisualDefinitions.FrameCount - 1 ? InitialFrame
            : (ushort)(MovingStart + 18 * (index < 0x21 ? index : index + 1));
        return new(KraidFootVisualToolingDefinitions.Bank, pointer, $"kraid_foot_oam_{pointer:X4}");
    }
}
