namespace SuperMetroid.Core.Enemies.Dragon;

/// <summary>
/// Dragon's idle, wing and attack animations (#641): one segment per animation a spawn, branch or resumed sleep enters, in authored order.
/// Their steps are <see cref="DragonAnimations"/>.
/// </summary>
internal enum DragonSegment
{
    /// <summary>The idle facing left animation.</summary>
    IdleFacingLeft,
    /// <summary>The wings facing left animation.</summary>
    WingsFacingLeft,
    /// <summary>The idle facing right animation.</summary>
    IdleFacingRight,
    /// <summary>The wings facing right animation.</summary>
    WingsFacingRight,
    /// <summary>The attacking facing left animation.</summary>
    AttackingFacingLeft,
    /// <summary>The attacking facing right animation.</summary>
    AttackingFacingRight,
}
