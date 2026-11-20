namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The pacing of Mother Brain's death explosions.</summary>
internal enum MotherBrainDeathExplosionStyle
{
    /// <summary>A pair of explosions at the smoky interval (<see cref="MotherBrainDeathExplosionDefinitions.SmokyIntervalUpdates"/>).</summary>
    Smoky,

    /// <summary>A full group at the mixed interval (<see cref="MotherBrainDeathExplosionDefinitions.MixedIntervalUpdates"/>).</summary>
    Mixed,
}
