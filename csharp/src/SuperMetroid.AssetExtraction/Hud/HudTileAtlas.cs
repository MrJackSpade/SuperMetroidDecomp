namespace SuperMetroid.AssetExtraction.Hud;

/// <summary>
/// Standard 2-bpp HUD/minimap artwork, followed by the cleared half its setup shows over BG2's words (#1466): the
/// characters the import pictures the HUD's images, the pause HUD plane and the BG3 sheets from. The game never reads
/// them.
/// </summary>
public sealed class HudTileAtlas
{
    /// <summary>Contiguous HUD characters followed by the zero-filled half that clears BG2's words.</summary>
    private readonly byte[] bytes;
    /// <summary>Retains the HUD characters and their cleared half.</summary>
    internal HudTileAtlas(byte[] bytes) => this.bytes = bytes;
    /// <summary>The HUD characters and their cleared half.</summary>
    internal ReadOnlyMemory<byte> Bytes => bytes;
}
