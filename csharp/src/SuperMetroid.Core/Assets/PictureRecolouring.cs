using System.Collections.Immutable;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// How a shade shows a picture (#1633): the colours it shows in place of the picture's own, each named by the
/// <see cref="ColourKey"/> of the pixels it recolours. Pixels whose key it does not name keep the colour they show without
/// it. A shade recolours by the colour a pixel holds, never by where the colour came from.
/// </summary>
public sealed record PictureRecolouring
{
    /// <summary>Creates the recolouring that shows each key of <paramref name="substitutions"/> in its colour.</summary>
    /// <exception cref="ArgumentException">Two substitutions name the same key.</exception>
    public PictureRecolouring(ImmutableArray<KeyedSubstitution> substitutions)
    {
        Substitutions = substitutions.IsDefault ? [] : substitutions;
        var keys = new HashSet<ColourKey>();
        foreach (KeyedSubstitution substitution in Substitutions)
        {
            if (!keys.Add(substitution.Replaced))
                throw new ArgumentException($"A shade shows the pixels of {substitution.Replaced} in two colours.", nameof(substitutions));
        }
    }

    /// <summary>The colours shown in place of the picture's own, one for each key the shade recolours.</summary>
    public ImmutableArray<KeyedSubstitution> Substitutions { get; }

    /// <summary>
    /// Whether a picture drawn in this shade blends with what lies behind it, whatever its own pixels say (#1512): a shade
    /// that shows a picture in the colours of a blending draw.
    /// </summary>
    public bool Blends { get; init; }

    /// <summary>Value equality over the substitutions in order and the blending.</summary>
    public bool Equals(PictureRecolouring? other) =>
        other is not null && Blends == other.Blends && Substitutions.SequenceEqual(other.Substitutions);

    /// <summary>A hash over the substitutions in order and the blending.</summary>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Blends);
        foreach (KeyedSubstitution substitution in Substitutions)
            hash.Add(substitution);
        return hash.ToHashCode();
    }
}
