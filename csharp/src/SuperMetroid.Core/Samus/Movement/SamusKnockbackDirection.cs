namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// Which way a hit knocks Samus. The values follow the cartridge's knockback table for the native snapshot import; its
/// unused straight-up entry is not a member, so a knockback that reaches it is rejected.
/// </summary>
public enum SamusKnockbackDirection
{
    /// <summary>No knockback is pending or running.</summary>
    None = 0,

    /// <summary>Up and to the left.</summary>
    UpLeft = 1,

    /// <summary>Up and to the right.</summary>
    UpRight = 2,

    /// <summary>Down and to the left: a humanoid hit while holding toward the hit.</summary>
    DownLeft = 4,

    /// <summary>Down and to the right: a humanoid hit while holding toward the hit.</summary>
    DownRight = 5,
}
