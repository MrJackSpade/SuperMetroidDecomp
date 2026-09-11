using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Standard 2-bpp HUD/minimap artwork and the native clearing half of its queued transfer.</summary>
public sealed class HudTileAtlas
{
    private readonly byte[] transfer;
    private HudTileAtlas(byte[] transfer) => this.transfer = transfer;
    internal ReadOnlyMemory<byte> Transfer => transfer;
    public void LoadTo(SnesVram vram, int destinationByteAddress) => vram.LoadBytes(destinationByteAddress, transfer);

    /// <summary>Refreshes artwork without replaying the initial transfer's room-tilemap clearing half.</summary>
    public void LoadCharactersTo(SnesVram vram, int destinationByteAddress) =>
        vram.LoadBytes(destinationByteAddress, transfer.AsSpan(0, HudTileAtlasFormat.CharacterByteCount));

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
    public const string FileName = "hud-tiles.png";
    public const int CharacterByteCount = MapTileAtlasFormat.Width * MapTileAtlasFormat.Height / 4;
    public const ushort TransferByteCount = CharacterByteCount * 2;
    public const ushort DestinationWord = SnesPpuLayout.GameplayHudCharacterBaseWord;
    public const int SourceAddress = MapTileAtlasRomData.HudCharacterData;
}
