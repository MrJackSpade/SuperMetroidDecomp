namespace SuperMetroid.Core.Enemies.Dragon;

/// <summary>
/// Dragon's idle, wing and attack animations (#641): one segment per animation a spawn, branch or wake enters, in authored order.
/// Their steps are <see cref="DragonAnimations"/>.
/// </summary>
internal enum DragonSegment
{
    /// <summary>Its body facing left, held.</summary>
    IdleFacingLeft,
    /// <summary>Its wing beating facing left, raised and lowered, repeating.</summary>
    WingsFacingLeft,
    /// <summary>Its body facing right, held.</summary>
    IdleFacingRight,
    /// <summary>Its wing beating facing right, raised and lowered, repeating.</summary>
    WingsFacingRight,
    /// <summary>
    /// Facing left, drawing its head back, then raising it and thrusting it forward to spit, before it finishes the
    /// attack.
    /// </summary>
    AttackingFacingLeft,
    /// <summary>
    /// Facing right, drawing its head back, then raising it and thrusting it forward to spit, before it finishes the
    /// attack.
    /// </summary>
    AttackingFacingRight,
}
