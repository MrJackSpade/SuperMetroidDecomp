
namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// Five stock beam material palettes. The beam palette load copies
/// the selected sixteen colors to OBJ palette6; charge/impact sprites and frozen
/// enemy components also use this palette. The reviewed exact material channel
/// choices and shade memberships are selected paint composition; generating them
/// without those inputs invents different hues. Shared ramps/channels calculate.
/// Copied clear/black target metadata is explicit, not inferred from visibility.
/// No pixels, cadence, freeze logic or selection precedence are exempted.
/// </summary>
internal static class BeamPaintDefinitions
{
    /// <summary>Highest channel value in a five-bit RGB component.</summary>
    internal const int Rgb5Maximum = (1 << 5) - 1;
    /// <summary>Every beam selection's first colour and its slot0 aliases copy this blue target despite sprite transparency.</summary>
    internal const int ClearTargetBlue = 14;
}
