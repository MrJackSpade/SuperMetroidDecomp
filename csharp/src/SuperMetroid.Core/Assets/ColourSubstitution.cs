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
    /// <summary>Keeps each colour replaced and the colour shown in its place.</summary>
    /// <param name="replaced">The colours replaced, each once.</param>
    /// <param name="shown">The colour shown in place of each, in the same order.</param>
    /// <exception cref="ArgumentException">The lists differ in length, or a colour is replaced twice.</exception>
    public ColourSubstitution(ImmutableArray<RgbColor> replaced, ImmutableArray<RgbColor> shown)
    {
        if (replaced.IsDefault || shown.IsDefault || replaced.Length != shown.Length)
            throw new ArgumentException("A colour substitution shows one colour in place of each colour it replaces.", nameof(shown));
        if (replaced.Distinct().Count() != replaced.Length)
            throw new ArgumentException("A colour substitution replaces each colour once; two of its colours are the same.", nameof(replaced));
        Replaced = replaced;
        Shown = shown;
    }

    /// <summary>The colours replaced.</summary>
    public ImmutableArray<RgbColor> Replaced { get; }

    /// <summary>The colour shown in place of each replaced colour.</summary>
    public ImmutableArray<RgbColor> Shown { get; }

    /// <inheritdoc />
    public override RgbColor Apply(RgbColor shown)
    {
        int index = Replaced.IndexOf(shown);
        return index < 0 ? shown : Shown[index];
    }
}
