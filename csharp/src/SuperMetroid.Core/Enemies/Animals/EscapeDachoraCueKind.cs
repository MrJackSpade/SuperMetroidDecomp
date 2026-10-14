namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>What a cue of the escaping Dachora's programs does: one per bank-$B3 instruction its lists use.</summary>
internal enum EscapeDachoraCueKind
{
    /// <summary>Continues at the target once the rising acid is above Y $00CE (<c>GotoIfAcidYLessThanCE</c>, $B3:EAA8).</summary>
    GoToIfAcidHigh,
    /// <summary>Continues at the target once the critters have escaped (<c>GotoIfCrittersEscaped</c>, $B3:EAB8).</summary>
    GoToIfCrittersEscaped,
    /// <summary>Moves the Dachora six pixels left (<c>MoveLeft</c>, $B3:EAC9).</summary>
    MoveLeft,
    /// <summary>Moves the Dachora six pixels right (<c>MoveRight</c>, $B3:EAD7).</summary>
    MoveRight,
}
