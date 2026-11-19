using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's main tube landed (#2562): it stops the liquid surface's motion, which shows still from the next update.
/// </summary>
public sealed record MotherBrainTubeLanded : EnemyPassEvent;
