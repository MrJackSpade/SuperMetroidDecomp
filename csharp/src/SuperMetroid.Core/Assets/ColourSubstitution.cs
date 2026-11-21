using System.Collections.Immutable;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// A colour effect that shows given colours in place of others (#1512): each colour it names shows its substitute, and
/// every other colour shows itself. A draw publishes it for colours that gameplay owns and evolves itself, such as Kraid's
/// eyes as they glow, keyed by the colours the picture's inks show under its shades. The colours it replaces must each
/// appear on the picture only where they are meant to be replaced; the import establishes that for the pictures it shades.
/// </summary>
public sealed record ColourSubstitution : ColourEffect
{
    /// <summary>Keeps the colour shown in place of each colour replaced.</summary>
    /// <param name="shownInPlaceOf">The colour shown in place of each colour replaced, by the colour it replaces.</param>
    public ColourSubstitution(ImmutableDictionary<RgbColor, RgbColor> shownInPlaceOf) => ShownInPlaceOf = Ensure.NotNull(shownInPlaceOf);

    /// <summary>The colour shown in place of each colour replaced, by the colour it replaces.</summary>
    public ImmutableDictionary<RgbColor, RgbColor> ShownInPlaceOf { get; }

    /// <summary>The colour an ink showing <paramref name="shown"/> shows: its substitute, or itself where it is not replaced.</summary>
    /// <param name="shown">The colour the ink shows before the substitution.</param>
    internal RgbColor Substituted(RgbColor shown) => ShownInPlaceOf.TryGetValue(shown, out RgbColor substitute) ? substitute : shown;
}
