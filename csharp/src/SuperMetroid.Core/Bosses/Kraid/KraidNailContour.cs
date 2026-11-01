namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Pushes Kraid's fingernails along his contour: the ledge a nail is level with selects its left offset.</summary>
internal static class KraidNailContour
{
    /// <summary>Returns a ledge's left offset as a wrapped room coordinate.</summary>
    internal static int LeftAt(int ledge) =>
        CoordinateAxis.Wrap(KraidNailContourDefinitions.Lefts[CheckLedge(ledge)]);

    /// <summary>Returns the comparison boundary paired with a ledge's left offset, as a wrapped room coordinate.</summary>
    internal static int TopAt(int ledge) =>
        CoordinateAxis.Wrap(KraidNailContourDefinitions.Tops[CheckLedge(ledge)]);

    /// <summary>
    /// The left offset of the ledge a nail <paramref name="relativeY"/> pixels below Kraid's body is level with: the first
    /// ledge whose bound lies above the nail. Null for a nail above every ledge, which <see cref="KraidNailPastLedges"/>
    /// pushes back instead.
    /// </summary>
    /// <param name="relativeY">The nail's Y minus the body's Y, as a wrapped room coordinate.</param>
    public static int? LedgeLeftOffset(int relativeY)
    {
        for (int ledge = 0; ledge < KraidNailContourDefinitions.LedgeCount; ledge++)
            if (CoordinateAxis.Difference(TopAt(ledge), relativeY) < 0)
                return LeftAt(ledge);
        return null;
    }

    /// <summary>Rejects a ledge outside the contour's four.</summary>
    private static int CheckLedge(int ledge) =>
        (uint)ledge < KraidNailContourDefinitions.LedgeCount ? ledge : throw new ArgumentOutOfRangeException(nameof(ledge));
}
