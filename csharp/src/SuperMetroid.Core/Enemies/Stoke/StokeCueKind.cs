namespace SuperMetroid.Core.Enemies.Stoke;

/// <summary>What a cue of Stoke's programs does: one per kind of step its lists use.</summary>
internal enum StokeCueKind
{
    /// <summary>Makes main AI walk Stoke left.</summary>
    SetMovingLeft,
    /// <summary>Makes main AI walk Stoke right.</summary>
    SetMovingRight,
    /// <summary>Spits a fireball in the direction its operand selects.</summary>
    SpawnFireball,
}
