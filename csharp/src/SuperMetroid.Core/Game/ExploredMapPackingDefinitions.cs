using System.Collections;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Game;

/// <summary>Bank $81 SRAM packing calculated from immutable stock map occupancy.</summary>
internal static class ExploredMapPackingDefinitions
{
    /// <summary>$81:8146, SRAMMapData_crateria, native sparse map-byte order.</summary>
    private const ushort CrateriaSource = 0x8146;
    /// <summary>$81:8196, SRAMMapData_brinstar, native sparse map-byte order.</summary>
    private const ushort BrinstarSource = 0x8196;
    /// <summary>$81:81E6, SRAMMapData_norfair, native sparse map-byte order.</summary>
    private const ushort NorfairSource = 0x81e6;
    /// <summary>$81:8236, SRAMMapData_wreckedShip, native sparse map-byte order.</summary>
    private const ushort WreckedShipSource = 0x8236;
    /// <summary>$81:8256, SRAMMapData_maridia, native sparse map-byte order.</summary>
    private const ushort MaridiaSource = 0x8256;
    /// <summary>$81:82A6, SRAMMapData_tourian, native sparse map-byte order.</summary>
    private const ushort TourianSource = 0x82a6;

    /// <summary>
    /// SaveMap/LoadMap omit Ceres. Each retained byte describes eight adjacent tiles;
    /// native page order places the left 32-column page before the right page.
    /// Installed IsDiscoverable rules always describe stock topology, even with art overrides.
    /// </summary>
    public static ExploredMapPackingDefinition Area(int area, AreaMapPresentationCatalog maps)
    {
        if ((uint)area >= SaveRamLayout.PackedMapAreaCount)
            throw new ArgumentOutOfRangeException(nameof(area));
        ArgumentNullException.ThrowIfNull(maps);
        int offset = 0;
        for (int previous = 0; previous < area; previous++)
            offset += new OccupiedByteIndexes(maps.Get((AreaId)previous)).Count;
        ushort source = (AreaId)area switch
        {
            AreaId.Crateria => CrateriaSource,
            AreaId.Brinstar => BrinstarSource,
            AreaId.Norfair => NorfairSource,
            AreaId.WreckedShip => WreckedShipSource,
            AreaId.Maridia => MaridiaSource,
            AreaId.Tourian => TourianSource,
            _ => throw new ArgumentOutOfRangeException(nameof(area)),
        };
        return new(checked((ushort)offset), new(maps.Get((AreaId)area)));
    }

    /// <summary>
    /// A calculated view, not a stored index list. Bank B5's map words and bank $81's
    /// sparse lists agree exactly: a byte is saved iff one of its eight cells is nonblank.
    /// </summary>
    /// <param name="stockRules">Stock area-map view used to identify discoverable cells.</param>
    internal readonly struct OccupiedByteIndexes(IAreaMapView stockRules) : IReadOnlyList<byte>
    {
        /// <summary>Gets the number of native map bytes containing at least one discoverable tile.</summary>
        public int Count
        {
            get
            {
                int count = 0;
                for (int candidate = 0; candidate < Bank80SystemState.ExploredMapBytesPerArea; candidate++)
                    if (Contains(candidate)) count++;
                return count;
            }
        }

        /// <summary>
        /// Gets the native map-byte index at the requested ordinal among occupied bytes.
        /// The ordinal must be between zero and <see cref="Count"/> minus one.
        /// </summary>
        public byte this[int index]
        {
            get
            {
                ArgumentOutOfRangeException.ThrowIfNegative(index);
                for (int candidate = 0; candidate < Bank80SystemState.ExploredMapBytesPerArea; candidate++)
                    if (Contains(candidate) && index-- == 0) return checked((byte)candidate);
                throw new ArgumentOutOfRangeException(nameof(index));
            }
        }

        /// <summary>
        /// Determines whether a native map byte is retained by the sparse save format
        /// by checking its eight tile cells against the stock discoverability rules.
        /// </summary>
        private bool Contains(int nativeByte)
        {
            int pageBytes = AreaMapLayout.HeightInTiles * AreaMapLayout.PageWidthInTiles / 8;
            int local = nativeByte % pageBytes;
            int x = nativeByte / pageBytes * AreaMapLayout.PageWidthInTiles + local % 4 * 8;
            int y = local / 4;
            for (int bit = 0; bit < 8; bit++)
                if (stockRules.IsDiscoverable(x + bit, y)) return true;
            return false;
        }

        /// <summary>Enumerates occupied native map-byte indexes in ascending native order.</summary>
        public IEnumerator<byte> GetEnumerator()
        {
            for (int candidate = 0; candidate < Bank80SystemState.ExploredMapBytesPerArea; candidate++)
                if (Contains(candidate)) yield return checked((byte)candidate);
        }
        /// <summary>Returns a non-generic enumerator over this sequence.</summary>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}

/// <summary>One area's calculated SRAM destination and occupied native map-byte view.</summary>
/// <param name="DestinationOffset">The byte offset where this area's retained map bytes begin in the packed SRAM map data.</param>
/// <param name="AreaByteIndexes">The native map-byte indexes retained for this area, in native map order.</param>
internal readonly record struct ExploredMapPackingDefinition(
    ushort DestinationOffset,
    ExploredMapPackingDefinitions.OccupiedByteIndexes AreaByteIndexes);
