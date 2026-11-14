namespace SuperMetroid.Core.Samus.Movement;

/// <summary>Which way a hit knocks Samus.</summary>
public enum SamusKnockbackDirection
{
    /// <summary>No knockback is pending or running.</summary>
    None,

    /// <summary>Up and to the left.</summary>
    UpLeft,

    /// <summary>Up and to the right.</summary>
    UpRight,

    /// <summary>Down and to the left: a humanoid hit while holding toward the hit.</summary>
    DownLeft,

    /// <summary>Down and to the right: a humanoid hit while holding toward the hit.</summary>
    DownRight,
}
