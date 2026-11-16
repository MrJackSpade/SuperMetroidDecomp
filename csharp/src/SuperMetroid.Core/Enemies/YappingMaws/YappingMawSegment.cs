namespace SuperMetroid.Core.Enemies.YappingMaws;

/// <summary>
/// The Yapping Maw's eight attack bites and six cooldown animations (#641): one segment per animation a spawn, branch or wake enters, in authored order.
/// Their steps are <see cref="YappingMawAnimations"/>.
/// </summary>
internal enum YappingMawSegment
{
    /// <summary>The attacking up animation.</summary>
    AttackingUp,
    /// <summary>The attacking up right animation.</summary>
    AttackingUpRight,
    /// <summary>The attacking right animation.</summary>
    AttackingRight,
    /// <summary>The attacking down right animation.</summary>
    AttackingDownRight,
    /// <summary>The attacking down animation.</summary>
    AttackingDown,
    /// <summary>The attacking down left animation.</summary>
    AttackingDownLeft,
    /// <summary>The attacking left animation.</summary>
    AttackingLeft,
    /// <summary>The attacking up left animation.</summary>
    AttackingUpLeft,
    /// <summary>The cooldown up right animation.</summary>
    CooldownUpRight,
    /// <summary>The cooldown up animation.</summary>
    CooldownUp,
    /// <summary>The jaws chewing up on the held Samus, which every cooldown aimed up loops (#2276).</summary>
    ChewingUp,
    /// <summary>The cooldown up left animation.</summary>
    CooldownUpLeft,
    /// <summary>The cooldown down right animation.</summary>
    CooldownDownRight,
    /// <summary>The cooldown down animation.</summary>
    CooldownDown,
    /// <summary>The jaws chewing down on the held Samus, which every cooldown aimed down loops (#2276).</summary>
    ChewingDown,
    /// <summary>The cooldown down left animation.</summary>
    CooldownDownLeft,
}
