namespace SuperMetroid.Core.Spatial;

/// <summary>
/// The part of a pixel a mover that moves its position by whole pixels carries from one update to the next (#2555): from
/// none up to, but not including, one pixel. Each update's travel adds to it; the whole pixels the sum crosses move the
/// position, and the part of a pixel past them is carried on, so a slow mover's position moves only on the updates its
/// carried travel crosses a pixel. The carry is kept apart from the position's own fraction, which such a mover leaves as
/// it was.
/// </summary>
public readonly record struct PixelFractionCarry
{
    /// <summary>Keeps a part of a pixel already checked to lie within one pixel.</summary>
    internal PixelFractionCarry(SignedEightEight part) => Part = part;

    /// <summary>The part of a pixel carried: none up to, but not including, one pixel.</summary>
    internal SignedEightEight Part { get; }

    /// <summary>No travel carried.</summary>
    public static PixelFractionCarry None => default;

    /// <summary>
    /// The whole pixels the carried part and <paramref name="travel"/> cross together: the pixels the position moves this
    /// update, toward decreasing coordinates when the travel takes the sum below none.
    /// </summary>
    public int WholePixelsCrossedBy(SignedEightEight travel) => (Part + travel).WholePixels;

    /// <summary>The part of a pixel carried on after <paramref name="travel"/>: the sum's part past the pixels it crosses.</summary>
    public PixelFractionCarry After(SignedEightEight travel) => new((Part + travel).WithoutWholePixels());
}
