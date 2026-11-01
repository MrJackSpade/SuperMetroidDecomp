namespace SuperMetroid.Core.Enemies.Puyo;

/// <summary>The two literal values written to Puyo's direction word.</summary>
public enum PuyoDirection
{
    /// <summary>Direction word zero: positive X motion and right-facing hop poses; also selected when Samus and Puyo have equal X coordinates.</summary>
    Right = 0,
    /// <summary>Direction word one: negative X motion and left-facing hop poses; terrain collision can latch its opposite for the next launch.</summary>
    Left = 1,
}
