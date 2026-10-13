using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>One fixed native takeoff upload; only its character pixels are editable.</summary>
internal readonly record struct GunshipLiftoffTransferDefinition(
    int SourceAddress, ushort DestinationWord, VramAssetId Asset);

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
            throw new IndexOutOfRangeException();
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

    /// <summary>Owned five-frame character atlases indexed by the fixed takeoff transfer order.</summary>
    private readonly RoomCharacterAtlas[] frames;

    /// <summary>Validates five complete native-size atlases and copies the caller's frame array.</summary>
    internal GunshipLiftoffArtworkCatalog(RoomCharacterAtlas[] frames)
    {
        Ensure.NotNull(frames);
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
