namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Pushes Kraid's fingernails along his contour: the ledge a nail is level with selects its left offset.</summary>
internal static class KraidNailContour
{
    /// <summary>
    /// The left offset of the ledge a nail <paramref name="relativeY"/> pixels below Kraid's body is level with: the first
    /// ledge whose bound lies above the nail. Null for a nail above every ledge, which <see cref="KraidNailPastLedges"/>
    /// pushes back instead.
    /// </summary>
    /// <param name="relativeY">The nail's Y minus the body's Y.</param>
    public static int? LedgeLeftOffset(int relativeY)
    {
        foreach (KraidNailLedge ledge in KraidNailContourDefinitions.Ledges)
            if (ledge.Top < relativeY)
                return ledge.Left;
        return null;
    }
}
