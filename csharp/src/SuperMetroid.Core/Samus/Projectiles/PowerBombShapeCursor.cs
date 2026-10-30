using SuperMetroid.Core.Samus.Abilities;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The next oval the Power Bomb's blast takes: an authored stream of ovals and the oval's position within it.</summary>
/// <param name="Stream">The authored stream being walked.</param>
/// <param name="Index">Ovals of the stream already consumed; the next frame consumes the oval at this position.</param>
public readonly record struct PowerBombShapeCursor(PowerBombShapeStream Stream, int Index)
{
    /// <summary>
    /// The horizontal radius, in pixels, of the oval at the cursor (#1468). A method, not a property: a cursor that has
    /// consumed its whole stream is a valid state with no oval, and the record's printed form must not fail on it.
    /// </summary>
    /// <exception cref="InvalidOperationException">The cursor has no oval left in its stream.</exception>
    public int HorizontalRadius()
    {
        IReadOnlyList<int> radii = RadiiOf(Stream);
        if (Index < 0 || Index >= radii.Count)
            throw new InvalidOperationException($"Power Bomb {Stream} shape {Index} is outside its stream.");
        return radii[Index];
    }

    /// <summary>Number of ovals in one authored stream.</summary>
    public static int CountOf(PowerBombShapeStream stream) => RadiiOf(stream).Count;

    /// <summary>The horizontal radius of each oval of one authored stream.</summary>
    private static IReadOnlyList<int> RadiiOf(PowerBombShapeStream stream) => stream switch
    {
        PowerBombShapeStream.Yellow => PowerBombExplosionDefinitions.YellowShapeRadii,
        PowerBombShapeStream.White => PowerBombExplosionDefinitions.WhiteShapeRadii,
        _ => throw new InvalidOperationException($"Undefined {nameof(PowerBombShapeStream)} {stream}."),
    };
}
