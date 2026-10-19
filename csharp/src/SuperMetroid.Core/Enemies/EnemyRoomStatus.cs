namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Room-wide enemy publications read by the runtime: kill count, boss identity, Japanese text, Fireflea
/// darkness, the enemy door-transition flag.
/// </summary>
internal sealed class EnemyRoomStatus
{
    /// <summary>
    /// WRAM $0797, door_transition_flag_enemies. Ridley's shared A35B wait
    /// remains active while destination enemy visuals run during the room fade.
    /// This is distinct from the elevator/Zebetite gate at $0795.
    /// </summary>
    public bool EnemyDoorTransitionActive { get; set; }
    /// <summary>
    /// The room's kill count, 0 through 65535: reset at load and incremented by death paths through
    /// <see cref="RecordKill"/>; it counts their publications rather than enumerating remaining or uniquely defeated
    /// slots. The count returns to zero after 65535, as the cartridge's does.
    /// </summary>
    public int EnemiesKilled { get; internal set => field = Ensure.BetweenInclusive(0, KillCountPeriod - 1, value); }

    /// <summary>How many kills the count records before it returns to zero.</summary>
    private const int KillCountPeriod = 65536;
    /// <summary>
    /// Native BossID ($179C) for the loaded encounter, 0 through 65535: reset to zero on room load and published by
    /// nonzero definition IDs or boss initialization; only compared for equality. Not an area boss-defeated flag.
    /// </summary>
    public int BossId { get; internal set; }
    /// <summary>
    /// Fireflea darkness progression, 0 through <see cref="MaximumFirefleaDarknessLevel"/>: reset on room load and
    /// advanced by two per Fireflea death, where the advance rejects the step to 14. No producer wraps it, so the
    /// setter rejects any value outside that range.
    /// </summary>
    public int FirefleaDarknessLevel { get; internal set => field = Ensure.BetweenInclusive(0, MaximumFirefleaDarknessLevel, value); }

    /// <summary>The darkest level a Fireflea death can reach; the next step to 14 is rejected.</summary>
    public const int MaximumFirefleaDarknessLevel = 12;

    /// <summary>Counts one more enemy death in the room's kill count, which wraps from 65535 to zero.</summary>
    internal void RecordKill() => EnemiesKilled = (EnemiesKilled + 1) % KillCountPeriod;

    /// <summary>Language flag sampled by $A6:C0D9 when the warning-text phase begins and by Mother Brain's alternate escape-text selection.</summary>
    public bool JapaneseText { get; set; }
}
