using System.Collections.Immutable;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// A frontend background drawn from placed pictures (#1467, #1555): installed pictures, each at its plane pixel, in drawing
/// order, a later picture covering an earlier one where it is opaque. The plane has no edges and does not repeat: a
/// picture shows only at its own place, and where no picture lies the plane shows nothing.
/// </summary>
public sealed class PicturedPlane : IEquatable<PicturedPlane>
{
    /// <summary>Keeps the plane's pictures in drawing order.</summary>
    /// <param name="pictures">The placed pictures, earliest first.</param>
    /// <exception cref="ArgumentException">The placements are missing, or one names no picture.</exception>
    public PicturedPlane(ImmutableArray<PlacedPlanePicture> pictures)
    {
        if (pictures.IsDefault)
            throw new ArgumentException("A pictured plane needs its placements.", nameof(pictures));
        foreach (PlacedPlanePicture placed in pictures)
            Ensure.NotNull(placed.Picture);
        Pictures = pictures;
    }

    /// <summary>The placed pictures, earliest first.</summary>
    public ImmutableArray<PlacedPlanePicture> Pictures { get; }

    /// <inheritdoc />
    public bool Equals(PicturedPlane? other) =>
        other is not null && Pictures.AsSpan().SequenceEqual(other.Pictures.AsSpan());

    /// <inheritdoc />
    public override bool Equals(object? obj) => Equals(obj as PicturedPlane);

    /// <inheritdoc />
    public override int GetHashCode() => Pictures.Length;
}
