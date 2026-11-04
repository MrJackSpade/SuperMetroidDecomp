using System.Collections.Immutable;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// A fade of the whole gameplay display (#1512): a door's fade or the death fade, one colour effect the backend applies
/// after every picture's and every shaded sprite draw's own shading, except where the game keeps colours lit.
/// </summary>
/// <param name="Effect">The fade's colour effect.</param>
/// <param name="Spares">What the fade leaves in its own colours.</param>
/// <param name="OverSamus">
/// Samus's shades whose colours were written over the faded display, in the order written, which the fade leaves as
/// they are; empty when none were.
/// </param>
public sealed record GameplayDisplayFade(ColourEffect Effect, GameplayFadeSpares Spares, ImmutableArray<PictureShade> OverSamus = default)
{
    /// <summary>Samus's shades written over the faded display, in the order written.</summary>
    public ImmutableArray<PictureShade> OverSamus { get; } = OverSamus.IsDefault ? [] : OverSamus;

    /// <summary>Whether both fades show the same effect, spare the same pictures and leave the same shades of Samus's.</summary>
    public bool Equals(GameplayDisplayFade? other) =>
        other is not null && Effect == other.Effect && Spares == other.Spares && OverSamus.SequenceEqual(other.OverSamus);

    /// <summary>Hashes what <see cref="Equals(GameplayDisplayFade)"/> compares.</summary>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Effect);
        hash.Add(Spares);
        foreach (PictureShade shade in OverSamus)
            hash.Add(shade);
        return hash.ToHashCode();
    }

    /// <summary>The fade's colour effect.</summary>
    public ColourEffect Effect { get; } = Ensure.NotNull(Effect);

    /// <summary>
    /// Whether the HUD's pictures keep their own colours, because the import shades the steps of a fade that keeps some of
    /// them lit, as a door's does.
    /// </summary>
    public bool SparesHud => Spares == GameplayFadeSpares.Hud;

    /// <summary>Whether Samus's draws keep their own colours, as the death fade keeps her lit.</summary>
    public bool SparesSamus => Spares == GameplayFadeSpares.Samus;
}
