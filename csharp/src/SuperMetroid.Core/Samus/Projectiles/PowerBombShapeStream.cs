namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The two authored sequences of window shapes the Power Bomb walks one shape per frame.</summary>
public enum PowerBombShapeStream
{
    /// <summary>The yellow shapes walked at the end of the pre-explosion flash.</summary>
    Yellow,

    /// <summary>The white shapes walked while the damaging explosion finishes expanding.</summary>
    White,
}
