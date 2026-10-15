namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Outcome of Morph Ball charge input before ordinary bomb placement.</summary>
internal enum BombSpreadAdmission
{
    /// <summary>No spread charge was consumed by this pass.</summary>
    NotApplicable,
    /// <summary>Held Down continued the spread charge timeout.</summary>
    Charging,
    /// <summary>The completed charge installed all five spread bombs.</summary>
    Spawned,
    /// <summary>Releasing Shoot canceled an existing beam flare.</summary>
    ChargeCancelled,
}
