using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Map.Artwork;

namespace SuperMetroid.Core.Hud.Artwork;

/// <summary>Standard 2-bpp HUD/minimap artwork, followed by the cleared half its setup shows over BG2's words.</summary>
public sealed class HudTileAtlas
{
    /// <summary>Contiguous HUD characters followed by the zero-filled half that clears BG2's words.</summary>
    internal readonly byte[] bytes;
    /// <summary>Retains the HUD characters and their cleared half.</summary>
    internal HudTileAtlas(byte[] bytes) => this.bytes = bytes;
    /// <summary>The HUD characters and their cleared half, which restoration slices divide.</summary>
    internal ReadOnlyMemory<byte> Bytes => bytes;

    /// <summary>One native 0x400-byte death-restoration slice of the shared BG3 characters.</summary>
    public ReadOnlyMemory<byte> KraidRestoreQuarter(int index)
    {
        if ((uint)index >= HudTileAtlasFormat.RestoreQuarterCount)
            throw new ArgumentOutOfRangeException(nameof(index));
        return bytes.AsMemory(
            index * HudTileAtlasFormat.RestoreQuarterBytes,
            HudTileAtlasFormat.RestoreQuarterBytes);
    }

    /// <summary>
    /// Shows rebound artwork without the cleared half, under the identity the characters are already shown as, so the
    /// HUD's pictures still name what they show (#1392).
    /// </summary>
    /// <param name="backgrounds">The background artwork bindings, which show the HUD characters.</param>
    /// <param name="characterBase">The BG3 character base the HUD's characters are currently shown from.</param>
    /// <exception cref="InvalidDataException">The HUD characters' binding does not begin with them.</exception>
    public void ShowCharactersAgain(BgArtworkBindings backgrounds, int characterBase)
    {
        Ensure.NotNull(backgrounds);
        ReadOnlySpan<byte> characters = bytes.AsSpan(0, HudTileAtlasFormat.CharacterByteCount);
        BgArtworkShown shown = backgrounds.Showing(characterBase, characters.Length / sizeof(ushort));
        if (shown.WordOffset != 0)
            throw new InvalidDataException($"{shown.Artwork.Name} shows the HUD characters from word {shown.WordOffset} of its artwork, not its first.");
        backgrounds.Bind(shown.Artwork, characterBase, characters);
    }

    /// <summary>Compiles a 256-by-64 indexed PNG into 256 row-major 8-by-8 2-bpp HUD/minimap characters and appends the native zero-filled clearing half.</summary>
    /// <param name="png">Readable PNG stream at its current position; it remains open and caller-owned. Pixel indices must be 0..3; PNG RGB palette colors do not replace runtime CGRAM.</param>
    /// <returns>Independently owned artwork: $1000 bytes of planar characters followed by $1000 zero bytes.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="png"/> is null.</exception>
    /// <exception cref="InvalidDataException">PNG is malformed, has unsupported features or incorrect dimensions, or uses a pixel index outside the 2-bpp range.</exception>
    public static HudTileAtlas Load(Stream png)
    {
        var image = IndexedPng.Read(png, new(MapTileAtlasFormat.Width, MapTileAtlasFormat.Height));
        byte[] planar = SnesPlanarTileEncoder.Encode(image.Pixels, image.Size, 2);
        var bytes = new byte[HudTileAtlasFormat.TransferByteCount];
        planar.CopyTo(bytes, 0);
        return new(bytes);
    }
}
