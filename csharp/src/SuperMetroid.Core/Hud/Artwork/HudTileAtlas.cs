namespace SuperMetroid.Core.Hud.Artwork;

/// <summary>Standard 2-bpp HUD/minimap artwork, followed by the cleared half its setup shows over BG2's words.</summary>
public sealed class HudTileAtlas
{
    /// <summary>Contiguous HUD characters followed by the zero-filled half that clears BG2's words.</summary>
    internal readonly byte[] bytes;
    /// <summary>Retains the HUD characters and their cleared half.</summary>
    internal HudTileAtlas(byte[] bytes) => this.bytes = bytes;
    /// <summary>The HUD characters and their cleared half.</summary>
    internal ReadOnlyMemory<byte> Bytes => bytes;
}
