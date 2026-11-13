namespace SuperMetroid.Core.Enemies.Puyo;

/// <summary>The two values Puyo's direction holds.</summary>
public enum PuyoDirection
{
    /// <summary>Direction zero: positive X motion and right-facing hop poses; also selected when Samus and Puyo have equal X coordinates.</summary>
    Right = 0,
    /// <summary>Direction one: negative X motion and left-facing hop poses; terrain collision can latch its opposite for the next launch.</summary>
    Left = 1,
}
