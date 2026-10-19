namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// Which way a bomb's timer-eight overlap launches Samus, chosen from Samus's horizontal position against the bomb's.
/// </summary>
public enum SamusBombJumpDirection
{
    /// <summary>No overlapping bomb published a launch this pass.</summary>
    None,

    /// <summary>Samus is left of the bomb and is launched up and to the left.</summary>
    Left,

    /// <summary>Samus is level with the bomb and is launched straight up.</summary>
    Straight,

    /// <summary>Samus is right of the bomb and is launched up and to the right.</summary>
    Right,
}
