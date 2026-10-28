using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>The five gunship takeoff uploads, in takeoff order.</summary>
internal static class GunshipLiftoffTransferDefinitions
{
    /// <summary>One takeoff upload contains $400 bytes of 4-bpp OBJ characters.</summary>
    internal const ushort ByteCount = 0x0400;
    /// <summary>The takeoff's five uploads show five consecutive installed pages.</summary>
    internal static GunshipLiftoffTransferDefinition Frame(int index)
    {
        if ((uint)index >= 5)
            throw new ArgumentOutOfRangeException(nameof(index));
        return new(InstalledArtworkId.GunshipLiftoffFirstTiles + index);
    }

    /// <summary>Calculated five-frame transfer sequence without a stored record table.</summary>
    internal static TransferSequence Frames => default;

    /// <summary>The five takeoff pages, in takeoff order (#1392, #1406).</summary>
    internal static IReadOnlyList<SpriteArtworkSource> Pages { get; } = [.. Frames.Select(frame => new InstalledArtworkCharacters(frame.Asset))];

    /// <summary>Calculated view over the five contiguous character transfers; no records are cached.</summary>
    internal readonly struct TransferSequence : IReadOnlyList<GunshipLiftoffTransferDefinition>
    {
        /// <summary>The five uploads of the takeoff list.</summary>
        public int Count => 5;
        /// <summary>Array-style extent exposed to callers expecting the transfer count.</summary>
        public int Length => Count;
        /// <summary>Calculates the selected upload without retaining a frame table.</summary>
        public GunshipLiftoffTransferDefinition this[int index] => Frame(index);
        /// <summary>Visits the five uploads in takeoff order.</summary>
        public IEnumerator<GunshipLiftoffTransferDefinition> GetEnumerator()
        {
            for (int index = 0; index < Count; index++)
                yield return Frame(index);
        }
        /// <summary>Exposes the same takeoff order through the non-generic collection interface.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
