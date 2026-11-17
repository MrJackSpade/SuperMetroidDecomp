using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Room-wide enemy publications read by the runtime: kill count, boss identity, Japanese text and Fireflea
/// darkness.
/// </summary>
internal sealed class EnemyRoomStatus
{
    /// <summary>Binds the room's status to the session that holds the language flag.</summary>
    /// <param name="session">The enemy state that outlives each room.</param>
    internal EnemyRoomStatus(EnemySession session) => _session = Ensure.NotNull(session);

    /// <summary>The enemy state that outlives each room.</summary>
    private readonly EnemySession _session;

    /// <summary>
    /// The room's kill count, zero or more: reset as the room loads and raised by each kill <see cref="RecordKill"/>
    /// records, so it counts kills rather than the enemies left or defeated. Its readers compare it with the room's kill
    /// quota on every room-object update; once it meets the quota the grey door has opened and the cleared event is set for
    /// good.
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
