namespace SuperMetroid.Core.Enemies.MaridiaSnails;

/// <summary>What a cue of Oum's animations does: open or close the shell's attack-rotation window, finish the attack, or play the splash sound.</summary>
internal enum MaridiaLargeSnailCueKind
{
    /// <summary>Opens the window in which an attack may rotate the shell.</summary>
    AllowAttackRotation,
    /// <summary>Closes the attack-rotation window.</summary>
    DisallowAttackRotation,
    /// <summary>Marks the attack animation finished for main AI.</summary>
    FinishAttack,
    /// <summary>Plays the splashed-out-of-water sound.</summary>
    PlaySplash,
}
