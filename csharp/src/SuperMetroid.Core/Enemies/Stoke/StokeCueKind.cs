namespace SuperMetroid.Core.Enemies.Stoke;

/// <summary>What a cue of Stoke's programs does: one per bank-$A2 instruction its lists use.</summary>
internal enum StokeCueKind
{
    /// <summary>Makes main AI walk Stoke left (<c>SetMovingLeft</c>, $A2:8990).</summary>
    SetMovingLeft,
    /// <summary>Makes main AI walk Stoke right (<c>SetMovingRight</c>, $A2:899D).</summary>
    SetMovingRight,
    /// <summary>Spits a fireball in the direction its operand selects (<c>SpawnFireball</c>, $A2:897E).</summary>
    SpawnFireball,
}
