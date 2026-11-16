using SuperMetroid.AssetExtraction.Bosses.Kraid.Artwork;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Kraid.Artwork;

/// <summary>Cartridge values and helpers of <see cref="KraidFootVisualDefinitions"/> that only verification reads.</summary>
internal static class KraidFootVisualDefinitionsConstants
{
    /// <summary>
    /// The roots each installed foot picture is composed from (#2210), in the document's order: where the foot's walk first
    /// shows the planted, lifting, raised, swinging and reaching picture, the roots $00, $05, $0A, $0B and $12 of the
    /// eighteen-byte run from $A7:8CE3, each of two components.
    /// </summary>
    private static readonly ushort[] Composed = [0x8ce3, 0x8d3d, 0x8d97, 0x8da9, 0x8e27];

    /// <summary>The root installed foot picture <paramref name="index"/> is composed from.</summary>
    internal static NativeEnemySpritemapDefinition Frame(int index)
    {
        if ((uint)index >= KraidFootVisualDefinitions.FrameCount) throw new ArgumentOutOfRangeException(nameof(index));
        ushort pointer = Composed[index];
        return new(KraidFootVisualToolingDefinitions.Bank, pointer, $"kraid_foot_oam_{pointer:X4}");
    }
}
