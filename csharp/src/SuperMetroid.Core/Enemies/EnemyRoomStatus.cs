using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Room-wide enemy publications read by the runtime: kill count, boss identity, Japanese text, Fireflea
/// darkness, the enemy door-transition flag.
/// </summary>
internal sealed class EnemyRoomStatus
{
    /// <summary>Binds the room's status to the session that holds the language flag.</summary>
    /// <param name="session">The enemy state that outlives each room.</param>
    internal EnemyRoomStatus(EnemySession session) => _session = Ensure.NotNull(session);

    /// <summary>The enemy state that outlives each room.</summary>
    private readonly EnemySession _session;

    /// <summary>
    /// Whether enemies see a door transition in progress. Ridley's shared wait
    /// remains active while destination enemy visuals run during the room fade.
    /// This is distinct from the elevator/Zebetite gate.
    /// </summary>
    public bool EnemyDoorTransitionActive { get; set; }
    /// <summary>
    /// The room's kill count, zero or more: reset at load and incremented by death paths through
    /// <see cref="RecordKill"/>; it counts their publications rather than enumerating remaining or uniquely defeated
    /// slots. Its readers compare it with the room's kill quota, at most 255, on every room-object update from the room's
    /// load, so the count meets the quota long before a count kept to the game's range could pass 65535 and start again;
    /// once met, the grey door has opened and the cleared event is set for good, so no reader sees where a count past
    /// that would stand.
    /// </summary>
    public int EnemiesKilled { get; internal set => field = Ensure.AtLeastZero(value); }
    /// <summary>
    /// The boss of the loaded encounter, or null: reset on room load and published by a boss definition in the
    /// population or by boss initialization. Not an area boss-defeated flag.
    /// </summary>
    public AreaBoss? Boss { get; internal set; }
    /// <summary>
    /// Fireflea darkness progression, 0 through <see cref="MaximumFirefleaDarknessStep"/>: reset on room load and
    /// advanced by one step per Fireflea death until it reaches the last step, where further deaths leave it. No
    /// producer wraps it, so the setter rejects any value outside that range.
    /// </summary>
    public int FirefleaDarknessStep { get; internal set => field = Ensure.BetweenInclusive(0, MaximumFirefleaDarknessStep, value); }

    /// <summary>The last darkness step Fireflea deaths reach: six deaths; a seventh leaves it.</summary>
    public const int MaximumFirefleaDarknessStep = 6;

    /// <summary>Counts one more enemy death in the room's kill count.</summary>
    internal void RecordKill() => EnemiesKilled++;

    /// <summary>Language flag sampled when the Ceres warning-text phase begins and by Mother Brain's alternate escape-text selection.</summary>
    public bool JapaneseText
    {
        get => _session.JapaneseText;
        set => _session.JapaneseText = value;
    }
}
