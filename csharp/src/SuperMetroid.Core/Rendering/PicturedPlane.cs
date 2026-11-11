using System.Collections.Immutable;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// A frontend background drawn from placed pictures (#1467, #1555): installed pictures, each at its plane pixel, in drawing
/// order, a later picture covering an earlier one where it is opaque. Each picture is its part drawn behind and its part
/// drawn in front (#1634), the in-front part over the behind part. The plane has no edges and does not repeat: a picture
/// shows only at its own place, and where no picture lies the plane shows nothing.
/// </summary>
public sealed class PicturedPlane : IEquatable<PicturedPlane>
{
    /// <summary>Keeps the plane's pictures in drawing order.</summary>
    /// <param name="pictures">The placed pictures, earliest first.</param>
    /// <exception cref="ArgumentException">The placements are missing, or one names no picture or a picture that shows nothing.</exception>
    public PicturedPlane(ImmutableArray<PlacedPlanePicture> pictures)
    {
        if (pictures.IsDefault)
            throw new ArgumentException("A pictured plane needs its placements.", nameof(pictures));
        foreach (PlacedPlanePicture placed in pictures)
        {
            if (Ensure.NotNull(placed.Picture).Size is null)
                throw new ArgumentException($"The picture placed at {placed.Corner} shows nothing; a plane places only pictures that show something.", nameof(pictures));
        }
        Pictures = pictures;
    }

    /// <summary>
    /// The plane placing those of <paramref name="pictures"/> that show something, in drawing order (#1634): a picture both
    /// of whose parts the import found show nothing draws nothing, so it is not placed, as a frame showing nothing is not
    /// drawn (#1513).
    /// </summary>
    public static PicturedPlane Showing(IEnumerable<PlacedPlanePicture> pictures) =>
        new([.. Ensure.NotNull(pictures).Where(placed => Ensure.NotNull(placed.Picture).Size is not null)]);

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
