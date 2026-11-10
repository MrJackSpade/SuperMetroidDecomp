using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.AssetExtraction.Samus.Artwork;

/// <summary>Immutable ordinary beam colors, as installed (#1596); selection and charge/Hyper animation remain engine-owned.</summary>
public sealed class InstalledBeamPalettes
{
    /// <summary>The installed sixteen colours of every retail beam combination, by its table index.</summary>
    private readonly RgbColor[][] palettes;

    /// <summary>Copies the installed colours of every retail beam combination.</summary>
    /// <param name="palettes">Sixteen colours for each retail beam combination, by its table index.</param>
    /// <exception cref="ArgumentException">A combination is missing or does not hold sixteen colours.</exception>
    internal InstalledBeamPalettes(RgbColor[][] palettes)
    {
        Ensure.NotNull(palettes);
        if (palettes.Length != BeamTileAtlasDefinitions.SelectionCount || palettes.Any(colours => colours is null || colours.Length != BeamPaletteDefinitions.ColorCount))
            throw new ArgumentException($"Beam palettes need {BeamPaletteDefinitions.ColorCount} colours for each of {BeamTileAtlasDefinitions.SelectionCount} combinations.", nameof(palettes));
        this.palettes = [.. palettes.Select(colours => (RgbColor[])[.. colours])];
    }

    /// <summary>
    /// Colour <paramref name="color"/> of retail beam combination <paramref name="selection"/>, which the import also
    /// pre-colours frozen enemies in (#1512).
    /// </summary>
    public RgbColor ColourOf(SamusBeamCombination selection, int color)
    {
        if (!selection.IsRetail) throw new ArgumentOutOfRangeException(nameof(selection), selection, "Beam palettes cover retail combinations only.");
        return palettes[selection.TableIndex][Ensure.BetweenInclusive(0, BeamPaletteDefinitions.ColorCount - 1, color)];
    }

    /// <summary>The sixteen colours a retail beam combination's beams show (#1512); the import pre-colours Samus's sheets in them.</summary>
    /// <param name="selection">A retail beam combination.</param>
    /// <exception cref="ArgumentOutOfRangeException">The combination is not a retail one.</exception>
    public RgbColor[] Colours(SamusBeamCombination selection)
    {
        if (!selection.IsRetail) throw new ArgumentOutOfRangeException(nameof(selection), selection, "Beam palettes cover retail combinations only.");
        return [.. palettes[selection.TableIndex]];
    }
}
