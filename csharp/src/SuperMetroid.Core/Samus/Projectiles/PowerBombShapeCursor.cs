namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The next window shape the Power Bomb consumes: a shape stream and the shape's position within it.</summary>
/// <param name="Stream">The authored stream being walked.</param>
/// <param name="Index">Shapes of the stream already consumed; the next frame consumes the shape at this position.</param>
public readonly record struct PowerBombShapeCursor(PowerBombShapeStream Stream, int Index);
