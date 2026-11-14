namespace SuperMetroid.Core.Enemies.Puyo;

/// <summary>The way a Puyo hops.</summary>
public enum PuyoDirection
{
    /// <summary>Hops right with right-facing hop poses; also chosen when Samus and the Puyo share an X coordinate.</summary>
    Right,
    /// <summary>Hops left with left-facing hop poses; terrain collision can latch the opposite way for the next launch.</summary>
    Left,
}
