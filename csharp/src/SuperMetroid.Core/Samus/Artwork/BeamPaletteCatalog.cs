using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Immutable ordinary beam colors; selection and charge/Hyper animation remain engine-owned.</summary>
public sealed class BeamPaletteCatalog
{
    /// <summary>Only selected RGB5 words that differ from calculated stock beam paint.</summary>
    internal readonly Dictionary<ColourCell, Bgr555> palettes = new();
    /// <summary>Stores independently edited colors while deriving unchanged words from stock paint.</summary>
    internal BeamPaletteCatalog(Bgr555[][] rows)
    {
        for (int index = 0; index < rows.Length; index++)
        for (int color = 0; color < BeamPaletteDefinitions.ColorCount; color++)
            if (rows[index][color] != BeamPaintDefinitions.Color(SamusBeamCombinations.FromTableIndex(index), color))
                palettes.Add(new ColourCell(index, color), rows[index][color]);
    }

    /// <summary>Gets an edited beam color when supplied, otherwise its calculated stock value.</summary>
    private Bgr555 Color(SamusBeamCombination selection, int color) =>
        palettes.TryGetValue(new ColourCell(selection.TableIndex, color), out Bgr555 supplied)
            ? supplied : BeamPaintDefinitions.Color(selection, color);

    /// <summary>Copies all sixteen colors of an ordinary beam selection to CGRAM entries 224..239 (OBJ palette 6), matching native <c>Load_Beam_Palette_withStackPrepped</c> at <c>$90:ACCD</c>; does not run charge or Hyper Beam color animation.</summary>
    /// <param name="beam">The beam colours to change.</param>
    /// <param name="selection">Retail beam combination $0..$B.</param>
    public void LoadTo(ColourRun beam, SamusBeamCombination selection)
    {
        if (!selection.IsRetail) throw new ArgumentOutOfRangeException(nameof(selection), selection, "Beam palettes cover retail combinations only.");
        for (int i = 0; i < BeamPaletteDefinitions.ColorCount; i++)
            beam.Write(i, Color(selection, i));
    }
}
