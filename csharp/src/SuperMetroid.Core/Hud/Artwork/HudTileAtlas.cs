using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Map.Artwork;

namespace SuperMetroid.Core.Hud.Artwork;

/// <summary>Standard 2-bpp HUD/minimap artwork and the native clearing half of its queued transfer.</summary>
public sealed class HudTileAtlas : IInstalledArtworkTransferSource
{
    /// <summary>Contiguous HUD characters followed by the zero-filled native BG2 clearing region.</summary>
    private readonly byte[] transfer;
    /// <summary>Retains an independently encoded native-size HUD upload for installed transfer lookup.</summary>
    internal HudTileAtlas(byte[] transfer) => this.transfer = transfer;
    /// <summary>Provides the entire $2000-byte upload to callers that divide it into restoration slices.</summary>
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
        if ((uint)index >= HudTileAtlasFormat.RestoreQuarterCount)
            throw new ArgumentOutOfRangeException(nameof(index));
        return transfer.AsMemory(
            index * HudTileAtlasFormat.RestoreQuarterBytes,
            HudTileAtlasFormat.RestoreQuarterBytes);
    }
    /// <summary>Writes all $2000 bytes at the standard BG3 character base: the characters followed by the zero-filled BG2 tilemap-clearing half.</summary>
    /// <param name="vram">Destination VRAM whose existing bytes are overwritten.</param>
    public void LoadTo(SnesVram vram) => new GameplayVram(vram).HudCharacterUpload.Load(transfer);

    /// <summary>Refreshes artwork without replaying the initial transfer's room-tilemap clearing half.</summary>
    /// <param name="destination">The BG3 character region at the current character base.</param>
    public void LoadCharactersTo(VramRegion destination) =>
        destination.Load(transfer.AsSpan(0, HudTileAtlasFormat.CharacterByteCount));

    /// <summary>Compiles a 256-by-64 indexed PNG into 256 row-major 8-by-8 2-bpp HUD/minimap characters and appends the native zero-filled clearing half.</summary>
    /// <param name="png">Readable PNG stream at its current position; it remains open and caller-owned. Pixel indices must be 0..3; PNG RGB palette colors do not replace runtime CGRAM.</param>
    /// <returns>An independently owned $2000-byte transfer with $1000 bytes of planar characters followed by $1000 zero bytes.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="png"/> is null.</exception>
    /// <exception cref="InvalidDataException">PNG is malformed, has unsupported features or incorrect dimensions, or uses a pixel index outside the 2-bpp range.</exception>
    public static HudTileAtlas Load(Stream png)
    {
        var image = IndexedPng.Read(png, new(MapTileAtlasFormat.Width, MapTileAtlasFormat.Height));
        byte[] planar = SnesPlanarTileEncoder.Encode(image.Pixels, image.Size, 2);
        var transfer = new byte[HudTileAtlasFormat.TransferByteCount];
        planar.CopyTo(transfer, 0);
        return new(transfer);
    }
}
