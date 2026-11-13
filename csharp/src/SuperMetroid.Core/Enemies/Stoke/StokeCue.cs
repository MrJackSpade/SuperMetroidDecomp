namespace SuperMetroid.Core.Enemies.Stoke;

/// <summary>One cue of Stoke's animations: what it does, with the values it acts on.</summary>
/// <param name="Kind">What the cue does.</param>
/// <param name="FireballDirection">The direction a <see cref="StokeCueKind.SpawnFireball"/> cue spits its fireball.</param>
internal readonly record struct StokeCue(StokeCueKind Kind, StokeDirection FireballDirection = StokeDirection.Left);
