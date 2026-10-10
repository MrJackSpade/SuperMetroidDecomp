using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Game;

public sealed partial class RoomEnemySystem
{
    /// <summary>Separates immutable RGB5 artwork from Kraid's native palette timing.</summary>
    private ushort ReadKraidColor(KraidPaletteSource source, int index)
    {
        if ((uint)index >= KraidPaletteRomData.ColorCount(source))
            throw new ArgumentOutOfRangeException(nameof(index));
        KraidColorCatalog colors = TileArtwork?.KraidColors ?? throw new InvalidDataException(
            "Installed enemy artwork has no Kraid RGB5 palette catalog.");
        return colors.Resolve(source, index);
    }

    /// <summary>Copies one 16-color RGB5 band from Kraid's installed palette catalog into CGRAM.</summary>
    /// <param name="source">The named Kraid palette containing the band to load.</param>
    /// <param name="cgramDestination">The first CGRAM color slot that receives the band.</param>
    private void LoadKraidColorBand(KraidPaletteSource source, int cgramDestination)
    {
        for (int color = 0; color < KraidPaletteRomData.BandColors; color++)
            _cgram!.SetColor(cgramDestination + color, ReadKraidColor(source, color));
    }
}
