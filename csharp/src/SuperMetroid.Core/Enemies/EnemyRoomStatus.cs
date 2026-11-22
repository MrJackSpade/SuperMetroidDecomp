using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Room-wide enemy publications read by the runtime: kill count, boss identity, Japanese text and Fireflea
/// darkness.
/// </summary>
internal sealed partial class EnemyRoomStatus
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
    /// The darkness the Firefleas killed in the room bring (#2367, #2432): none as the room loads, one step deeper for each
    /// Fireflea death <see cref="RecordFirefleaDeath"/> records.
    /// </summary>
    public FirefleaDarkness FirefleaDarkness { get; internal set; }

    /// <summary>Counts one more enemy death in the room's kill count.</summary>
    internal void RecordKill() => EnemiesKilled++;

    /// <summary>Deepens the room's Fireflea darkness by one more Fireflea death (<see cref="FirefleaDarkness.Deeper"/>).</summary>
    internal void RecordFirefleaDeath() => FirefleaDarkness = FirefleaDarkness.Deeper;

    /// <summary>Text language sampled when the Ceres warning-text phase begins and by Mother Brain's alternate escape-text selection.</summary>
    public TextLanguage Language
    {
        get => _session.Language;
        set => _session.Language = value;
    }
}
