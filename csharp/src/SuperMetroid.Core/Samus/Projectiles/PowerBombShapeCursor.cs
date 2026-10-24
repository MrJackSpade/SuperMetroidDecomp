using SuperMetroid.Core.Samus.Abilities;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The next window shape the Power Bomb consumes: a shape stream and the shape's position within it.</summary>
/// <param name="Stream">The authored stream being walked.</param>
/// <param name="Index">Shapes of the stream already consumed; the next frame consumes the shape at this position.</param>
public readonly record struct PowerBombShapeCursor(PowerBombShapeStream Stream, int Index)
{
    /// <summary>
    /// The shape's position among all 21 authored profiles: the seventeen white shapes first, then the four yellow
    /// ones, as the cartridge lays them out. A method, not a property: a cursor that has consumed its whole stream is a
    /// valid state with no shape, and the record's printed form must not fail on it.
    /// </summary>
    /// <exception cref="InvalidOperationException">The cursor has no shape left in its stream.</exception>
    public int ProfileIndex()
    {
        if (Index < 0 || Index >= CountOf(Stream))
            throw new InvalidOperationException($"Power Bomb {Stream} shape {Index} is outside its stream.");
        return Stream == PowerBombShapeStream.White ? Index : CountOf(PowerBombShapeStream.White) + Index;
    }

    /// <summary>Number of shapes in one authored stream.</summary>
    public static int CountOf(PowerBombShapeStream stream) => stream switch
    {
        PowerBombShapeStream.Yellow => PowerBombExplosionDefinitions.YellowShapeFrames,
        PowerBombShapeStream.White => PowerBombExplosionDefinitions.WhiteShapeFrames,
        _ => throw new InvalidOperationException($"Undefined {nameof(PowerBombShapeStream)} {stream}."),
    };
}
