using System.Collections.Immutable;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>One of Kraid's destinations and the walks he may take from it (#2300).</summary>
/// <param name="X">The destination's X position in room pixels.</param>
/// <param name="Moves">The walk each random choice sends him on, the last serving every choice past it.</param>
internal sealed record KraidMovementRow(int X, ImmutableArray<KraidMovementChoice> Moves);
