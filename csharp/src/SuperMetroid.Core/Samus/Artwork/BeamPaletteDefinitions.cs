namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Presentation geometry for the sixteen colors the beam palette load writes.</summary>
/// <remarks>
/// Each of the twelve beam combinations selects one of five beam materials (Power, Ice, Wave, Plasma, Spazer), and the
/// load writes that material's sixteen colours. The selected material paint is owned by BeamPaintDefinitions; its exact
/// channel composition calculates every stock colour and preserves independent replacements. How the stock colours are
/// read from the cartridge is the import pipeline's (BeamPaletteFile). Investigation: #625 / #901.
/// </remarks>
public static class BeamPaletteDefinitions
{
    /// <summary>Sixteen colors copied by the beam loader, covering all entries of OBJ palette 6, including color zero.</summary>
    public const int ColorCount = 16;
}
