namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>Where the cartridge keeps the arm cannon's twelve cover tiles (#1411).</summary>
[SuperMetroid.Core.Hardware.CpuBusBoundary("catalog: arm cannon cover tile sources")]
internal static class ArmCannonTileSources
{
    /// <summary>$9A:9A00, Tiles_NonClosed_ArmCannon_Vertical_0, first of twelve cover tile sources.</summary>
    private const int FirstTileSource = 0x9a00;
    /// <summary>The twelve native cover sources from $9A:9A00 through $9A:B000 occupy successive $200-byte character blocks.</summary>
    private const int TileSourceStride = 512;
    /// <summary>The bank-$9A DMA source of each installed cover tile, as a queued VRAM DMA names it.</summary>
    internal static TileSourceSequence CoverTileDmaSources => new();

    /// <summary>Calculated source identities in the original exported tile order; no tile pixels are inferred.</summary>
    internal readonly record struct TileSourceSequence : IReadOnlyList<ushort>
    {
        /// <summary>Twelve source identities: three cover frames for each vertical, horizontal, downward-diagonal, and upward-diagonal artwork orientation.</summary>
        public int Count => 12;
        /// <summary>Gets bank-$9A source identity $9A00 plus index times $200 for zero-based index 0–11; each identity resolves to an installed 32-byte character.</summary>
        public ushort this[int index] => (uint)index < Count
            ? (ushort)(FirstTileSource + index * TileSourceStride)
            : throw new ArgumentOutOfRangeException(nameof(index));
        /// <summary>Finds the exact bank-relative native tile identity, returning its exported character index 0–11 or -1 if the position is absent or unaligned.</summary>
        public int IndexOf(ushort source)
        {
            int index = (source - FirstTileSource) / TileSourceStride;
            return (uint)index < Count && this[index] == source ? index : -1;
        }
        /// <summary>Enumerates source identities in exported PNG character order, with the three frames of each native artwork orientation kept together.</summary>
        public IEnumerator<ushort> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        /// <summary>Exposes the same native source order through the non-generic collection interface.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
