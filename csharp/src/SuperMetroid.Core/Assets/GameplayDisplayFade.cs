using System.Collections.Immutable;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// A fade of the whole gameplay display (#1512): a door's fade or the death fade, one colour effect the backend applies
/// after every picture's and every shaded sprite draw's own shading, except where the game keeps colours lit.
/// </summary>
/// <param name="Effect">The fade's colour effect.</param>
/// <param name="Spares">What the fade leaves in its own colours.</param>
/// <param name="LitOnSamus">
/// Samus's shades the fade keeps lit (#2093): their colours stay lit on her draws over the fade, in order, as a door's
/// fade in keeps her beams and visor lit; empty for none.
/// </param>
public sealed partial record GameplayDisplayFade(ColourEffect Effect, GameplayFadeSpares Spares, ImmutableArray<PictureShade> LitOnSamus = default)
{
    /// <summary>Samus's shades the fade keeps lit on her draws, in order.</summary>
    public ImmutableArray<PictureShade> LitOnSamus { get; } = LitOnSamus.IsDefault ? [] : LitOnSamus;

    /// <summary>Whether both fades show the same effect, spare the same pictures and keep the same shades of Samus's lit.</summary>
    public bool Equals(GameplayDisplayFade? other) =>
        other is not null && Effect == other.Effect && Spares == other.Spares && LitOnSamus.SequenceEqual(other.LitOnSamus);

    /// <summary>Hashes what <see cref="Equals(GameplayDisplayFade)"/> compares.</summary>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Effect);
        hash.Add(Spares);
        foreach (PictureShade shade in LitOnSamus)
            hash.Add(shade);
        return hash.ToHashCode();
    }

    /// <summary>The fade's colour effect.</summary>
    public ColourEffect Effect { get; } = Ensure.NotNull(Effect);

    /// <summary>
    /// Whether the background's pictures keep their own colours, because the import shades the steps of a fade that keeps
    /// some of their colours lit, as a door's does (#3036).
    /// </summary>
    public bool SparesBackgrounds => Spares == GameplayFadeSpares.Backgrounds;

    /// <summary>Whether Samus's draws keep their own colours, as the death fade keeps her lit.</summary>
    public bool SparesSamus => Spares == GameplayFadeSpares.Samus;
}
