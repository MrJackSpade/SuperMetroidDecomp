namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Pushes Kraid's fingernails along his contour: the ledge a nail is level with selects its left offset.</summary>
internal static class KraidNailContour
{
    /// <summary>Returns a ledge's left offset from Kraid's body.</summary>
    internal static int LeftAt(int ledge) =>
        KraidNailContourDefinitions.Lefts[CheckLedge(ledge)];

    /// <summary>Returns the height below Kraid's body above which a nail is level with a ledge.</summary>
    internal static int TopAt(int ledge) =>
        KraidNailContourDefinitions.Tops[CheckLedge(ledge)];

    /// <summary>
    /// The left offset of the ledge a nail <paramref name="relativeY"/> pixels below Kraid's body is level with: the first
    /// ledge whose bound lies above the nail. Null for a nail above every ledge, which <see cref="KraidNailPastLedges"/>
    /// pushes back instead.
    /// </summary>
    /// <param name="relativeY">The nail's Y minus the body's Y.</param>
    public static int? LedgeLeftOffset(int relativeY)
    {
        for (int ledge = 0; ledge < KraidNailContourDefinitions.LedgeCount; ledge++)
            if (TopAt(ledge) < relativeY)
                return LeftAt(ledge);
        return null;
    }

    /// <summary>Rejects a ledge outside the contour's four.</summary>
    private static int CheckLedge(int ledge) =>
        ledge is >= 0 and < KraidNailContourDefinitions.LedgeCount ? ledge : throw new ArgumentOutOfRangeException(nameof(ledge));
}
