namespace SuperMetroid.Core.Enemies.Cacatac;

/// <summary>What a cue of Cacatac's programs does: one per bank-$A2 instruction its lists use.</summary>
internal enum CacatacCueKind
{
    /// <summary>Plays the spike-release sound (<c>PlaySpikesSFX</c>, $A2:9F2A).</summary>
    PlaySpikeSound,
    /// <summary>Restores main AI's patrol from the walking direction (<c>SetFunctionMovingLeftRight</c>, $A2:A095).</summary>
    RestorePatrol,
    /// <summary>Spits a spike in the direction its operand selects (<c>SpawnSpikeProjectileWithParameterInY</c>, $A2:A0A7).</summary>
    SpawnSpike,
}
