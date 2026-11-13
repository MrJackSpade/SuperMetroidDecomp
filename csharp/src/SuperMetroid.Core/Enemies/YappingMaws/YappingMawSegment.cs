namespace SuperMetroid.Core.Enemies.YappingMaws;

/// <summary>
/// The Yapping Maw's eight attack bites and six cooldown animations (#641): one segment per animation a spawn, branch or resumed sleep enters, in authored order.
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
    /// <summary>The loop.</summary>
    CooldownUpLoop,
    /// <summary>The cooldown up left animation.</summary>
    CooldownUpLeft,
    /// <summary>The loop.</summary>
    CooldownUpLeftLoop,
    /// <summary>The cooldown down right animation.</summary>
    CooldownDownRight,
    /// <summary>The cooldown down animation.</summary>
    CooldownDown,
    /// <summary>The loop.</summary>
    CooldownDownLoop,
    /// <summary>The cooldown down left animation.</summary>
    CooldownDownLeft,
    /// <summary>The loop.</summary>
    CooldownDownLeftLoop,
}
