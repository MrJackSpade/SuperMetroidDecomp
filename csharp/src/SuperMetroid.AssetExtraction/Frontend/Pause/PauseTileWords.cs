using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Pause;

/// <summary>Decodes the cartridge's pause-screen tilemap words into the editable atlas cells the pause documents hold (#1466).</summary>
internal static class PauseTileWords
{
    /// <summary>Decodes a packed pause tile into its selected map or interface atlas cell and display attributes.</summary>
    /// <param name="raw">The packed pause tile, 0 through 65535.</param>
    /// <param name="name">The artwork's name, for errors.</param>
    /// <exception cref="InvalidDataException">The word selects a character the pause screen does not load.</exception>
    internal static PauseBackdropCell FromWord(int raw, string name)
    {
        var word = new MapTileWord(checked((ushort)raw));
        int character = word.CharacterIndex;
        if (character >= PauseBackdropDefinitions.AtlasTileCount * 2)
            throw new InvalidDataException($"Pause artwork {name} references unloaded character {character}.");
        return new()
        {
            Atlas = character < PauseBackdropDefinitions.AtlasTileCount ? PauseAtlas.Map : PauseAtlas.Interface,
            TileColumn = character % PauseBackdropDefinitions.AtlasColumns,
            TileRow = character % PauseBackdropDefinitions.AtlasTileCount / PauseBackdropDefinitions.AtlasColumns,
            Palette = word.PaletteIndex,
            Priority = word.HasPriority,
            FlipX = (word.Mirroring & TileFlip.Horizontal) != TileFlip.None,
            FlipY = (word.Mirroring & TileFlip.Vertical) != TileFlip.None,
        };
    }
}
