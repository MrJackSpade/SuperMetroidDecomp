namespace SuperMetroid.Core.Enemies.Cacatac;

/// <summary>One cue of Cacatac's animations: what it does, with the values it acts on.</summary>
/// <param name="Kind">What the cue does.</param>
/// <param name="Direction">The launch direction a <see cref="CacatacCueKind.SpawnSpike"/> cue spits its spike in.</param>
internal readonly record struct CacatacCue(CacatacCueKind Kind, CacatacSpikeDirection Direction = default);
