using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// Phantoon's fight changed its door (#1678, #2562): the fight's door as the starting flames close in, or the restored
/// door once the Wrecked Ship's power is back.
/// </summary>
/// <param name="Change">The door's change.</param>
public sealed record PhantoonDoorChanged(PhantoonDoorChange Change) : EnemyPassEvent;
