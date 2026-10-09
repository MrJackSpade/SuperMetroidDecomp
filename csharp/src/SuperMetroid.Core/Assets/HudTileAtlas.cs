using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Standard 2-bpp HUD/minimap artwork and the native clearing half of its queued transfer.</summary>
public sealed class HudTileAtlas : IInstalledArtworkTransferSource
{
    /// <summary>Owned native-sized byte transfer containing planar HUD characters followed by the tilemap-clearing region.</summary>
    private readonly byte[] transfer;

    /// <summary>Stores the compiled transfer used for initial uploads and character-only refreshes.</summary>
    /// <param name="transfer">Owned bytes for the standard HUD upload, including its zero-filled clearing half.</param>
    private HudTileAtlas(byte[] transfer) => this.transfer = transfer;

    /// <summary>Read-only view of the complete native transfer supplied to queued VRAM writes.</summary>
    internal ReadOnlyMemory<byte> Transfer => transfer;

    /// <summary>Resolves the complete native gameplay upload, including its clearing half, from current PNG content.</summary>
    public bool TryResolve(int sourceAddress, int byteCount, out ReadOnlyMemory<byte> data)
    {
        if (sourceAddress == HudTileAtlasFormat.SourceAddress && byteCount == HudTileAtlasFormat.TransferByteCount)
        {
            data = transfer;
            return true;
        }
        data = default;
        return false;
    }

    /// <summary>One native 0x400-byte death-restoration slice of the shared BG3 characters.</summary>
    public ReadOnlyMemory<byte> KraidRestoreQuarter(int index)
    {
        if ((uint)index >= Game.KraidBackgroundRomData.StandardBg3TransferCount)
            throw new ArgumentOutOfRangeException(nameof(index));
        return transfer.AsMemory(
            index * Game.KraidBackgroundRomData.StandardBg3TransferBytes,
            Game.KraidBackgroundRomData.StandardBg3TransferBytes);
    }
    /// <summary>Writes all $2000 bytes: standard BG3 characters followed by the zero-filled tilemap-clearing half.</summary>
    /// <param name="vram">Destination VRAM whose existing bytes are overwritten.</param>
    /// <param name="destinationByteAddress">Physical VRAM byte offset, normally twice <see cref="HudTileAtlasFormat.DestinationWord"/>; not a native word address.</param>
    public void LoadTo(SnesVram vram, int destinationByteAddress) => vram.LoadBytes(destinationByteAddress, transfer);

    /// <summary>Refreshes artwork without replaying the initial transfer's room-tilemap clearing half.</summary>
    public void LoadCharactersTo(SnesVram vram, int destinationByteAddress) =>
        vram.LoadBytes(destinationByteAddress, transfer.AsSpan(0, HudTileAtlasFormat.CharacterByteCount));

    /// <summary>Compiles a 256-by-64 indexed PNG into 256 row-major 8-by-8 2-bpp HUD/minimap characters and appends the native zero-filled clearing half.</summary>
    /// <param name="png">Readable PNG stream at its current position; it remains open and caller-owned. Pixel indices must be 0..3; PNG RGB palette colors do not replace runtime CGRAM.</param>
    /// <returns>An independently owned $2000-byte transfer with $1000 bytes of planar characters followed by $1000 zero bytes.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="png"/> is null.</exception>
    /// <exception cref="InvalidDataException">PNG is malformed, has unsupported features or incorrect dimensions, or uses a pixel index outside the 2-bpp range.</exception>
    public static HudTileAtlas Load(Stream png)
    {
        var image = IndexedPng.Read(png, MapTileAtlasFormat.Width, MapTileAtlasFormat.Height);
        byte[] planar = SnesPlanarTileEncoder.Encode(image.Pixels, image.Width, image.Height, 2);
        var transfer = new byte[HudTileAtlasFormat.TransferByteCount];
        planar.CopyTo(transfer, 0);
        return new(transfer);
    }
}

/// <summary>Installed HUD atlas identity and standard transfer geometry.</summary>
public static class HudTileAtlasFormat
{
    /// <summary>Installation-relative indexed PNG filename for standard HUD/minimap characters, separate from the 4-bpp map atlas.</summary>
    public const string FileName = "hud-tiles.png";
    /// <summary>$1000 bytes: 256 8-by-8 2-bpp characters, sixteen planar bytes each.</summary>
    public const int CharacterByteCount = MapTileAtlasFormat.Width * MapTileAtlasFormat.Height / 4;
    /// <summary>$2000-byte native upload: the character data plus the equal-sized all-zero BG2 tilemap-clearing region.</summary>
    public const ushort TransferByteCount = CharacterByteCount * 2;
    /// <summary>VRAM word $4000, the standard BG3 character base; multiply by two for direct byte-addressed loads.</summary>
    public const ushort DestinationWord = SnesPpuLayout.GameplayHudCharacterBaseWord;
    /// <summary>$9A:B200, Tiles_Standard_BG3; the installed transfer supplies these characters and the following Clear BG2 tilemap region rather than rereading cartridge bytes.</summary>
    public const int SourceAddress = MapTileAtlasRomData.HudCharacterData;
}
