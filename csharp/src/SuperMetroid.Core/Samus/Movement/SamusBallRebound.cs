namespace SuperMetroid.Core.Samus.Movement;

/// <summary>The rebound a landing ball has reached (#2560).</summary>
public enum SamusBallRebound
{
    /// <summary>The first rebound, at the ball-bounce speed.</summary>
    First,

    /// <summary>The second rebound, at the second ball-bounce speed; the next landing brings the ball to rest.</summary>
    Second,
}
