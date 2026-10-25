using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Chozo;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// Editable RGB5 target images for the two Chozo statues and n00b-tube cracks.
/// Statue variant selection, PLM placement and tube behavior remain compiled.
/// </summary>
public sealed class ChozoAndTubeColorCatalog
{
    /// <summary>Canonical selected RGB5 colors and ordered rows, independent of JSON encoding.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create("ChozoAndTubeColorCatalog-v1", content =>
        {
            Span<Bgr555> tube = stackalloc Bgr555[ChozoAndTubeColorDefinitions.ColorCount];
            for (int color = 0; color < tube.Length; color++) tube[color] = ResolveTubeCracks(color);
            content.AppendColors("tubeCracks", tube);
            content.AppendColors("wreckedShip", Statue(ChozoStatuePalette.WreckedShip));
            content.AppendColors("lowerNorfair", Statue(ChozoStatuePalette.LowerNorfair));
        });

    /// <summary>Eight authored crack inks from which the unedited stock halves and shade ramp are calculated.</summary>
    internal readonly Bgr555[] tubeColorSeeds;
    /// <summary>Supplied crack colors that differ from the stock seed and repeated ramp.</summary>
    internal readonly Dictionary<ColourCell, Bgr555> tubeColorEdits = [];
    /// <summary>Wrecked Ship statue inks that differ from the calculated Chozo paint.</summary>
    internal readonly Dictionary<ColourCell, Bgr555> wreckedShipEdits = [];
    /// <summary>Lower Norfair statue inks that differ from the calculated Chozo paint.</summary>
    internal readonly Dictionary<ColourCell, Bgr555> lowerNorfairEdits = [];

    /// <summary>Preserves authored crack seeds and only deviations from calculated stock tube and statue colors.</summary>
    internal ChozoAndTubeColorCatalog(Bgr555[] tubeCracks, Bgr555[] wreckedShip,
        Bgr555[] lowerNorfair)
    {
        // The eight crack seed colors are authored paint; the remaining stock values
        // are a linear ramp and an exact repeated palette half.
        tubeColorSeeds = tubeCracks[..8];
        for (int color = 8; color < tubeCracks.Length; color++)
            if (tubeCracks[color] != CalculateTubeColor(color))
                tubeColorEdits.Add(new ColourCell(0, color), tubeCracks[color]);
        for (int color = 0; color < ChozoAndTubeColorDefinitions.ColorCount; color++)
        {
            if (wreckedShip[color] != ChozoStatuePaintDefinitions.Color(ChozoStatuePalette.WreckedShip, color))
                wreckedShipEdits.Add(new ColourCell(0, color), wreckedShip[color]);
            if (lowerNorfair[color] != ChozoStatuePaintDefinitions.Color(ChozoStatuePalette.LowerNorfair, color))
                lowerNorfairEdits.Add(new ColourCell(0, color), lowerNorfair[color]);
        }
    }

    /// <summary>Gets packed RGB5 tube-crack ink 0–31 corresponding to $AA:E2DD; the stock second sixteen-color half repeats the first, while supplied edits remain independent.</summary>
    public Bgr555 ResolveTubeCracks(int color)
    {
        if ((uint)color >= ChozoAndTubeColorDefinitions.ColorCount)
            throw new ArgumentOutOfRangeException(nameof(color));
        return tubeColorEdits.TryGetValue(new ColourCell(0, color), out Bgr555 edited) ? edited : CalculateTubeColor(color);
    }

    /// <summary>$AA:E2DD: two equal16-color halves; colors8..15 linearly shade RGB5(31,27,29) to(0,0,1).</summary>
    private Bgr555 CalculateTubeColor(int color)
    {
        int local = color % 16;
        if (local < 8) return tubeColorSeeds[local];
        int step = local - 8;
        int red = 31 - (31 * step + 3) / 7;
        int green = 27 - (27 * step + 3) / 7;
        int blue = 29 - 4 * step;
        return new Bgr555(red, green, blue);
    }

    /// <summary>Resolves a statue ink from an authored edit or the selected variant's calculated paint.</summary>
    internal Bgr555 ResolveStatue(ChozoStatuePalette palette, int color)
    {
        if ((uint)color >= ChozoAndTubeColorDefinitions.ColorCount)
            throw new ArgumentOutOfRangeException(nameof(color));
        Dictionary<ColourCell, Bgr555> edits = palette == ChozoStatuePalette.WreckedShip ? wreckedShipEdits : lowerNorfairEdits;
        return edits.TryGetValue(new ColourCell(0, color), out Bgr555 edited) ? edited : ChozoStatuePaintDefinitions.Color(palette, color);
    }

    /// <summary>Expands a statue variant into its ordered 32-color CGRAM image for identity hashing.</summary>
    private Bgr555[] Statue(ChozoStatuePalette palette) =>
        Enumerable.Range(0, ChozoAndTubeColorDefinitions.ColorCount).Select(color => ResolveStatue(palette, color)).ToArray();

    /// <summary>Installs the tube-crack image at CGRAM 144–175, matching palette-only initializer $AA:E716; tube destruction and the initializer actor's removal remain runtime behavior.</summary>
    public void ApplyTubeCracks(ColourRun destination)
    {
        for (int color = 0; color < ChozoAndTubeColorDefinitions.ColorCount; color++)
            destination.Write(color, ResolveTubeCracks(color));
    }
    /// <summary>Installs the 32 selected Wrecked Ship Chozo inks at CGRAM 144–175, replacing the $AA:E31D image copied by initializer $AA:E75C.</summary>
    public void ApplyWreckedShip(ColourRun destination) => Apply(destination, ChozoStatuePalette.WreckedShip);
    /// <summary>Installs the 32 selected Lower Norfair Chozo inks at CGRAM 144–175, replacing the $AA:E35D image copied by initializer $AA:E784.</summary>
    public void ApplyLowerNorfair(ColourRun destination) => Apply(destination, ChozoStatuePalette.LowerNorfair);

    /// <summary>Writes the selected statue's 32 inks into the shared boss OBJ palette range.</summary>
    private void Apply(ColourRun destination, ChozoStatuePalette palette)
    {
        for (int color = 0; color < ChozoAndTubeColorDefinitions.ColorCount; color++)
            destination.Write(color, ResolveStatue(palette, color));
    }
}
