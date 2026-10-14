namespace SuperMetroid.Core.Enemies.Dragon;

/// <summary>What a cue of Dragon's programs does: one per bank-$A2 instruction its lists use.</summary>
internal enum DragonCueKind
{
    /// <summary>Marks the body's attack animation complete for main AI (<c>AttackFinishedCallback</c>, $A2:E5FB).</summary>
    FinishAttack,
}
