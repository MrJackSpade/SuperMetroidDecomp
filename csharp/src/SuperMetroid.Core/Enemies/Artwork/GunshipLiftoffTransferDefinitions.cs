namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Compiled $A2:AC07/$A2:AC11 transfer pairs for the five gunship takeoff uploads.</summary>
internal static class GunshipLiftoffTransferDefinitions
{
    /// <summary>One takeoff upload contains $400 bytes of 4-bpp OBJ characters.</summary>
    internal const ushort ByteCount = 0x0400;
    /// <summary>$94:C800, first gunship takeoff character chunk selected by $A2:AC07.</summary>
    private const int FirstSourceAddress = 0x94c800;
    /// <summary>$7600, first VRAM word selected by the destination list at $A2:AC11.</summary>
    private const int FirstDestinationWord = 0x7600;

    /// <summary>$A2:AC07/$AC11 select five consecutive $400-byte source and VRAM regions.</summary>
    internal static GunshipLiftoffTransferDefinition Frame(int index)
    {
        if ((uint)index >= 5)
            throw new ArgumentOutOfRangeException(nameof(index));
        return new(FirstSourceAddress + index * ByteCount, (ushort)(FirstDestinationWord + index * ByteCount / 2),
            VramAssetId.GunshipLiftoffFirstTiles + index);
    }

    /// <summary>Calculated five-frame transfer sequence without a stored record table.</summary>
    internal static TransferSequence Frames => default;

    /// <summary>Calculated view over the five contiguous character transfers; no records are cached.</summary>
    internal readonly struct TransferSequence : IReadOnlyList<GunshipLiftoffTransferDefinition>
    {
        /// <summary>Five consecutive source and destination pairs in the native takeoff list.</summary>
        public int Count => 5;
        /// <summary>Array-style extent exposed to callers expecting the transfer count.</summary>
        public int Length => Count;
        /// <summary>Calculates the selected transfer pair without retaining a frame table.</summary>
        public GunshipLiftoffTransferDefinition this[int index] => Frame(index);
        /// <summary>Visits the five source and destination pairs in takeoff order.</summary>
        public IEnumerator<GunshipLiftoffTransferDefinition> GetEnumerator()
        {
            for (int index = 0; index < Count; index++)
                yield return Frame(index);
        }
        /// <summary>Exposes the same takeoff order through the non-generic collection interface.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
