namespace SuperMetroid.Core.Enemies.Stoke;

/// <summary>One cue of Stoke's programs, with the operand its native instruction carries.</summary>
/// <param name="Kind">What the cue does.</param>
/// <param name="FireballDirection">The direction a <see cref="StokeCueKind.SpawnFireball"/> cue spits its fireball.</param>
internal readonly record struct StokeCue(StokeCueKind Kind, StokeDirection FireballDirection = StokeDirection.Left);
