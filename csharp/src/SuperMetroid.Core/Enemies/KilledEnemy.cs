namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The enemy a death explosion or its drop came from, and whether that enemy respawns from its snapshot when the
/// explosion's respawn cue runs (#1412). A cleared projectile holds the first slot, not respawning.
/// </summary>
public readonly record struct KilledEnemy
{
    /// <summary>Keeps the slot and whether it respawns.</summary>
    private KilledEnemy(int slotIndex, bool respawns)
    {
        SlotIndex = slotIndex;
        Respawns = respawns;
    }

    /// <summary>No killed enemy: a drop spawned directly rather than from a death.</summary>
    public static KilledEnemy None { get; } = new(-1, respawns: false);

    /// <summary>The enemy in <paramref name="slot"/>, which respawns when the explosion ends if <paramref name="respawns"/>.</summary>
    /// <param name="slot">The killed enemy's slot.</param>
    /// <param name="respawns">Whether the explosion's respawn cue rebuilds the enemy from its snapshot.</param>
    public static KilledEnemy Of(RoomEnemySlot slot, bool respawns) => new(Ensure.NotNull(slot).SlotIndex, respawns);

    /// <summary>The killed enemy's slot, or -1 for <see cref="None"/>.</summary>
    public int SlotIndex { get; }

    /// <summary>Whether the explosion's respawn cue rebuilds the enemy from its snapshot.</summary>
    public bool Respawns { get; }
}
