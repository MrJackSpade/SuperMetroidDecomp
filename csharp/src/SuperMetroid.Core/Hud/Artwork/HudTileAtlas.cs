using SuperMetroid.Core.Assets;

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
    /// <param name="set">The layer-3 characters the HUD's are currently shown from.</param>
    /// <exception cref="InvalidDataException">The HUD characters' binding does not begin with them.</exception>
    public static void ShowCharactersAgain(BgArtworkBindings backgrounds, BgCharacterSet set)
    {
        Ensure.NotNull(backgrounds);
        int characters = set.CharactersIn(HudTileAtlasFormat.CharacterByteCount);
        BgArtworkShown shown = backgrounds.Showing(set, 0, characters);
        if (shown.WordOffset != 0)
            throw new InvalidDataException($"{shown.Artwork.Name} shows the HUD characters from word {shown.WordOffset} of its artwork, not its first.");
        backgrounds.Bind(shown.Artwork, set, 0, characters);
    }
}
