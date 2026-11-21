namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>One of Kraid's destinations and the walks he may take from it (#2300).</summary>
/// <param name="X">The destination's X position in room pixels.</param>
/// <param name="Walks">The walk each random sample sends him on from the destination.</param>
internal sealed partial record KraidMovementRow(int X, WeightedRandomChoice<KraidMovementChoice> Walks);
