using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>One fixed native takeoff upload; only its character pixels are editable.</summary>
internal readonly record struct GunshipLiftoffTransferDefinition(
    int SourceAddress, ushort DestinationWord, VramAssetId Asset);

/// <summary>Compiled $A2:AC07/$A2:AC11 transfer pairs for the five gunship takeoff uploads.</summary>
internal static class GunshipLiftoffTransferDefinitions
{
    internal const ushort ByteCount = 0x0400;

    /// <summary>First $94:C800 character chunk, uploaded to VRAM word $7600.</summary>
    internal static readonly GunshipLiftoffTransferDefinition First =
        new(0x94c800, 0x7600, VramAssetId.GunshipLiftoffFirstTiles);
    /// <summary>Second $94:CC00 character chunk, uploaded to VRAM word $7800.</summary>
    internal static readonly GunshipLiftoffTransferDefinition Second =
        new(0x94cc00, 0x7800, VramAssetId.GunshipLiftoffSecondTiles);
    /// <summary>Third $94:D000 character chunk, uploaded to VRAM word $7A00.</summary>
    internal static readonly GunshipLiftoffTransferDefinition Third =
        new(0x94d000, 0x7a00, VramAssetId.GunshipLiftoffThirdTiles);
    /// <summary>Fourth $94:D400 character chunk, uploaded to VRAM word $7C00.</summary>
    internal static readonly GunshipLiftoffTransferDefinition Fourth =
        new(0x94d400, 0x7c00, VramAssetId.GunshipLiftoffFourthTiles);
    /// <summary>Fifth $94:D800 character chunk, uploaded to VRAM word $7E00.</summary>
    internal static readonly GunshipLiftoffTransferDefinition Fifth =
        new(0x94d800, 0x7e00, VramAssetId.GunshipLiftoffFifthTiles);

    private static readonly GunshipLiftoffTransferDefinition[] Entries =
        [First, Second, Third, Fourth, Fifth];

    internal static ReadOnlySpan<GunshipLiftoffTransferDefinition> Frames => Entries;
}

/// <summary>Five installed, palette-indexed takeoff frames resolved at accepted NMI.</summary>
public sealed class GunshipLiftoffArtworkCatalog
{
    private readonly RoomCharacterAtlas[] frames;

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
