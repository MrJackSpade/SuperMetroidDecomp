using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.AssetExtraction.Samus.Artwork;

/// <summary>Immutable ordinary beam colors, as installed (#1596); selection and charge/Hyper animation remain engine-owned.</summary>
public sealed partial class InstalledBeamPalettes
{
    /// <summary>Each equippable combination's installed colours.</summary>
    private readonly IReadOnlyDictionary<SamusBeamCombination, RgbColor[]> palettes;

    /// <summary>Keeps each equippable combination's colours, as installed.</summary>
    /// <exception cref="ArgumentException">A combination Samus can equip has no colours, or not sixteen.</exception>
    internal InstalledBeamPalettes(IReadOnlyDictionary<SamusBeamCombination, RgbColor[]> palettes)
    {
        Ensure.NotNull(palettes);
        if (palettes.Count != BeamTileAtlasDefinitions.Equippable.Count || BeamTileAtlasDefinitions.Equippable.Any(selection =>
                !palettes.TryGetValue(selection, out RgbColor[]? colours) || colours is null || colours.Length != BeamPaletteDefinitions.ColorCount))
            throw new ArgumentException($"Beam palettes need {BeamPaletteDefinitions.ColorCount} colours for each of {BeamTileAtlasDefinitions.SelectionCount} combinations.", nameof(palettes));
        this.palettes = palettes.ToDictionary(entry => entry.Key, entry => (RgbColor[])[.. entry.Value]);
    }

    /// <summary>One colour of <paramref name="selection"/>'s palette.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The combination cannot be equipped, or the colour lies past the sixteen.</exception>
    public RgbColor ColourOf(SamusBeamCombination selection, int color)
    {
        if (!selection.CanBeEquipped) throw new ArgumentOutOfRangeException(nameof(selection), selection, "Beam palettes cover retail combinations only.");
        return Ensure.Listed(palettes, selection)[Ensure.BetweenInclusive(0, BeamPaletteDefinitions.ColorCount - 1, color)];
    }

    /// <summary>All sixteen colours of <paramref name="selection"/>'s palette.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The combination cannot be equipped.</exception>
    public RgbColor[] Colours(SamusBeamCombination selection)
    {
        if (!selection.CanBeEquipped) throw new ArgumentOutOfRangeException(nameof(selection), selection, "Beam palettes cover retail combinations only.");
        return [.. Ensure.Listed(palettes, selection)];
    }
}
