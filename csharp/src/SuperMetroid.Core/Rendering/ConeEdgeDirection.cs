namespace SuperMetroid.Core.Rendering;

/// <summary>
/// The way one edge of a <see cref="WindowCone"/> leaves its apex (#2009, #2555): how far it runs right across the screen
/// and down it for one step along the edge, in whole numbers from the game's fine sine, so every device tests a pixel
/// against the edge with the same exact arithmetic.
/// </summary>
/// <param name="Across">How far the edge runs right; negative to the left.</param>
/// <param name="Down">How far the edge runs down; negative upward.</param>
public readonly record struct ConeEdgeDirection(int Across, int Down)
{
    /// <summary>The length of one step along an edge in the direction's own numbers: the peak of the game's fine sine.</summary>
    private const int StepLength = 32767;

    /// <summary>
    /// How far outside an edge a pixel may lie and stay inside the cone, as the cross product of the edge's direction and
    /// the pixel's offset from the apex: half a pixel in the direction's own numbers, so two equal edges make a ray one
    /// pixel wide.
    /// </summary>
    public const int HalfPixelAllowance = (StepLength + 1) / 2;

    /// <summary>
    /// The way an edge at <paramref name="edge"/>, clockwise from straight up, leaves the apex: the game's fine sine of
    /// the angle across and its fine cosine up, the game's own steps rather than any device's sine.
    /// </summary>
    public static ConeEdgeDirection Toward(TrigonometryAngle edge) =>
        new(Trigonometry.FineSignedSine(edge), -Trigonometry.FineSignedSine(edge + TrigonometryAngle.QuarterTurn));
}
