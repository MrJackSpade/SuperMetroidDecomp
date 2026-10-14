namespace SuperMetroid.Core.Enemies.MaridiaSnails;

/// <summary>What a cue of Oum's programs does: one per bank-$A2 instruction its lists use.</summary>
internal enum MaridiaLargeSnailCueKind
{
    /// <summary>Opens the window in which an attack may rotate the shell (<c>AllowAttackRotation</c>, $A2:CCBE).</summary>
    AllowAttackRotation,
    /// <summary>Closes the attack-rotation window (<c>DisallowAttackRotation</c>, $A2:CCC9).</summary>
    DisallowAttackRotation,
    /// <summary>Marks the attack animation finished for main AI (<c>SetAnimationFinished</c>, $A2:CCB3).</summary>
    FinishAttack,
    /// <summary>Plays the splashed-out-of-water sound (<c>PlaySplashedOutOfWaterSound</c>, $A2:CB6B).</summary>
    PlaySplash,
}
