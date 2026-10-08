using System.Collections;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Game;

/// <summary>Bank $81 SRAM packing calculated from immutable stock map occupancy.</summary>
internal static class ExploredMapPackingDefinitions
{
    /// <summary>$81:8131, six live area byte counts followed by unused Ceres.</summary>
    public const int NativeByteCountTable = 0x818131;
    /// <summary>$81:8138, native packed SRAM destination offsets.</summary>
    public const int NativeDestinationOffsetTable = 0x818138;
    /// <summary>$81:82D6, pointers to native sparse byte-index lists.</summary>
    public const int NativeSourcePointerTable = 0x8182d6;
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
    internal readonly struct OccupiedByteIndexes(IAreaMapView stockRules) : IReadOnlyList<byte>
    {
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

        public IEnumerator<byte> GetEnumerator()
        {
            for (int candidate = 0; candidate < Bank80SystemState.ExploredMapBytesPerArea; candidate++)
                if (Contains(candidate)) yield return checked((byte)candidate);
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}

/// <summary>One area's calculated SRAM destination and occupied native map-byte view.</summary>
internal readonly record struct ExploredMapPackingDefinition(
    ushort DestinationOffset,
    ExploredMapPackingDefinitions.OccupiedByteIndexes AreaByteIndexes);
