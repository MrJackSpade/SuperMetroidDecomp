using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>One fixed native takeoff upload; only its character pixels are editable.</summary>
/// <param name="SourceAddress">SNES byte address of the transfer's native source region.</param>
/// <param name="DestinationWord">Starting VRAM word address used by the native upload list.</param>
/// <param name="Asset">Typed artwork identity associated with this transfer region.</param>
internal readonly record struct GunshipLiftoffTransferDefinition(
    int SourceAddress, ushort DestinationWord, VramAssetId Asset);

/// <summary>Compiled $A2:AC07/$A2:AC11 transfer pairs for the five gunship takeoff uploads.</summary>
internal static class GunshipLiftoffTransferDefinitions
{
    /// <summary>Size in bytes of each native gunship takeoff character transfer.</summary>
    internal const ushort ByteCount = 0x0400;
    /// <summary>$94:C800, first gunship takeoff character chunk selected by $A2:AC07.</summary>
    private const int FirstSourceAddress = 0x94c800;
    /// <summary>$7600, first VRAM word selected by the destination list at $A2:AC11.</summary>
    private const int FirstDestinationWord = 0x7600;

    /// <summary>$A2:AC07/$AC11 select five consecutive $400-byte source and VRAM regions.</summary>
    internal static GunshipLiftoffTransferDefinition Frame(int index)
    {
        if ((uint)index >= 5)
            throw new IndexOutOfRangeException();
        return new(FirstSourceAddress + index * ByteCount, (ushort)(FirstDestinationWord + index * ByteCount / 2),
            VramAssetId.GunshipLiftoffFirstTiles + index);
    }

    /// <summary>Indexed view of the five native source and VRAM destination pairs.</summary>
    internal static TransferSequence Frames => default;

    /// <summary>Calculated view over the five contiguous character transfers; no records are cached.</summary>
    internal readonly struct TransferSequence : IReadOnlyList<GunshipLiftoffTransferDefinition>
    {
        /// <summary>Number of transfer pairs in the takeoff upload sequence.</summary>
        public int Count => 5;

        /// <summary>Number of transfer pairs, matching <see cref="Count"/>.</summary>
        public int Length => Count;

        /// <summary>Gets the native transfer pair at the requested sequence position.</summary>
        /// <param name="index">Zero-based position from zero through four.</param>
        /// <exception cref="IndexOutOfRangeException">The index is outside the five native transfer pairs.</exception>
        public GunshipLiftoffTransferDefinition this[int index] => Frame(index);

        /// <summary>Enumerates the five transfer pairs in native upload order.</summary>
        /// <returns>An enumerator that calculates each transfer as it is requested.</returns>
        public IEnumerator<GunshipLiftoffTransferDefinition> GetEnumerator()
        {
            for (int index = 0; index < Count; index++)
                yield return Frame(index);
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}

/// <summary>Five installed, palette-indexed takeoff frames resolved at accepted NMI.</summary>
public sealed class GunshipLiftoffArtworkCatalog
{
    /// <summary>Canonical selected presentation data; no derived field is added to debugger states.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create("enemy-gunship-liftoff-v1", content =>
        {
            content.Append("frames", frames.Length);
            foreach (RoomCharacterAtlas frame in frames)
                content.Append("tiles", frame.Transfer.Span);
        });

    /// <summary>Selected artwork frames, copied at construction and resolved by transfer index.</summary>
    private readonly RoomCharacterAtlas[] frames;

    /// <summary>Creates a catalog from the five complete character atlases used by the takeoff uploads.</summary>
    /// <param name="frames">Artwork frames in the same order as the native transfer sequence.</param>
    /// <exception cref="ArgumentNullException">The frame array is <see langword="null"/>.</exception>
    /// <exception cref="InvalidDataException">The array does not contain five frames of the required transfer size.</exception>
    internal GunshipLiftoffArtworkCatalog(RoomCharacterAtlas[] frames)
    {
        ArgumentNullException.ThrowIfNull(frames);
        if (frames.Length != GunshipLiftoffTransferDefinitions.Frames.Length ||
            frames.Any(frame => frame is null ||
                frame.Transfer.Length != GunshipLiftoffTransferDefinitions.ByteCount))
            throw new InvalidDataException("Gunship takeoff requires five complete character frames.");
        this.frames = frames.ToArray();
    }

    /// <summary>Returns the current editable characters for a typed pending VRAM transfer.</summary>
    public ReadOnlyMemory<byte> Resolve(VramAssetId asset)
    {
        int index = (int)asset - (int)VramAssetId.GunshipLiftoffFirstTiles;
        if ((uint)index >= (uint)frames.Length ||
            GunshipLiftoffTransferDefinitions.Frames[index].Asset != asset)
            throw new ArgumentOutOfRangeException(nameof(asset), asset,
                "Not a gunship takeoff transfer.");
        return frames[index].Transfer;
    }

    /// <summary>Rebinds an older pending cartridge-source upload after debugger restore.</summary>
    public bool TryResolve(int sourceAddress, int byteCount, out ReadOnlyMemory<byte> data)
    {
        for (int index = 0; index < frames.Length; index++)
        {
            if (GunshipLiftoffTransferDefinitions.Frames[index].SourceAddress != sourceAddress)
                continue;
            if (byteCount != GunshipLiftoffTransferDefinitions.ByteCount)
                throw new InvalidDataException(
                    $"Gunship takeoff upload ${sourceAddress:X6} requires " +
                    $"${GunshipLiftoffTransferDefinitions.ByteCount:X} bytes, not ${byteCount:X}.");
            data = frames[index].Transfer;
            return true;
        }
        data = default;
        return false;
    }
}
