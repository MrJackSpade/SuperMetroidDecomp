namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>The left, top and bottom edges of Kraid's mouth collision scratch, relative to his body position.</summary>
/// <param name="Left">The left edge, in room pixels.</param>
/// <param name="Top">The top edge, in room pixels.</param>
/// <param name="Bottom">The bottom edge, in room pixels.</param>
public readonly record struct KraidMouthCollisionEdges(short Left, short Top, short Bottom);
