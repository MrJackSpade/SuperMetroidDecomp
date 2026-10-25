using System.Collections;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Map;

/// <summary>Bank $81 SRAM packing calculated from immutable stock map occupancy.</summary>
internal static class ExploredMapPackingDefinitions
{

    /// <summary>
    /// SaveMap/LoadMap omit Ceres. Each retained byte describes eight adjacent tiles;
    /// native page order places the left 32-column page before the right page.
    /// Installed IsDiscoverable rules always describe stock topology, even with art overrides.
    /// </summary>
    public static ExploredMapPackingDefinition Area(int area, IAreaMapSource maps)
    {
        if ((uint)area >= ExploredMapStorage.PackedAreaCount)
            throw new ArgumentOutOfRangeException(nameof(area));
        Ensure.NotNull(maps);
        int offset = 0;
        for (int previous = 0; previous < area; previous++)
            offset += new OccupiedByteIndexes(maps.Get((AreaId)previous)).Count;
        return new(checked((ushort)offset), new(maps.Get((AreaId)area)));
    }

    /// <summary>
    /// A calculated view, not a stored index list. Bank B5's map words and bank $81's
    /// sparse lists agree exactly: a byte is saved iff one of its eight cells is nonblank.
    /// </summary>
    internal readonly struct OccupiedByteIndexes(IAreaMapView stockRules) : IReadOnlyList<byte>
    {
        /// <summary>Counts native map bytes with at least one discoverable stock tile.</summary>
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

        /// <summary>Finds the occupied byte at the given position in the native page-first packing order.</summary>
        public byte this[int index]
        {
            get
            {
                Ensure.AtLeastZero(index);
                for (int candidate = 0; candidate < Bank80SystemState.ExploredMapBytesPerArea; candidate++)
                    if (Contains(candidate) && index-- == 0) return checked((byte)candidate);
                throw new ArgumentOutOfRangeException(nameof(index));
            }
        }

        /// <summary>Checks the eight horizontal map cells one stored exploration byte represents, including the page offset.</summary>
        private bool Contains(int storedByte)
        {
            int pageBytes = AreaMapLayout.HeightInTiles * AreaMapLayout.PageWidthInTiles / 8;
            int local = storedByte % pageBytes;
            int x = storedByte / pageBytes * AreaMapLayout.PageWidthInTiles + local % 4 * 8;
            int y = local / 4;
            for (int bit = 0; bit < 8; bit++)
                if (stockRules.IsDiscoverable(new Position16<MapTiles>(unchecked((ushort)(x + bit)), unchecked((ushort)y)))) return true;
            return false;
        }

        /// <summary>Visits occupied bytes in the same page-first order used by bank $81's sparse save lists.</summary>
        public IEnumerator<byte> GetEnumerator()
        {
            for (int candidate = 0; candidate < Bank80SystemState.ExploredMapBytesPerArea; candidate++)
                if (Contains(candidate)) yield return checked((byte)candidate);
        }
        /// <summary>Exposes the native sparse byte order through the non-generic collection interface.</summary>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
