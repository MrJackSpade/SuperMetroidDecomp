namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Room-wide enemy publications read by the runtime: kill count, boss identity, Japanese text, Fireflea
/// darkness, the enemy door-transition flag and the per-frame Ceres-escape and Draygon-release events.
/// </summary>
internal sealed class EnemyRoomStatus
{
    /// <summary>
    /// WRAM $0797, door_transition_flag_enemies. Ridley's shared A35B wait
    /// remains active while destination enemy visuals run during the room fade.
    /// This is distinct from the elevator/Zebetite gate at $0795.
    /// </summary>
    public bool EnemyDoorTransitionActive { get; set; }
    /// <summary>Wrapping native room kill counter, reset at load and incremented by death paths; counts their publications rather than enumerating remaining or uniquely defeated slots.</summary>
    public ushort EnemiesKilled { get; internal set; }
    /// <summary>Native BossID ($179C) for the loaded encounter, reset to zero on room load and published by nonzero definition IDs or boss initialization; not an area boss-defeated flag.</summary>
    public ushort BossId { get; internal set; }
    /// <summary>Native $177E darkness progression, reset on room load and advanced by two per Fireflea death; the cartridge permits values through 12 and rejects the next step to 14.</summary>
    public ushort FirefleaDarknessLevel { get; internal set; }

    /// <summary>
    /// One-frame publication of $A6:C117. The runtime consumes this after EnemyMain to
    /// start the global escape timer and set Ceres's boss bit in their native owners.
    /// </summary>
    public bool CeresEscapeStartedThisFrame { get; internal set; }

    /// <summary>
    /// One-frame publication of a Draygon-owned <c>ReleaseSamusFromDraygon</c> ($90:E2DE)
    /// during EnemyMain. The release writes $FFFF to all three prospective-pose slots, which
    /// the runtime owns and clears after EnemyMain.
    /// </summary>
    public bool SamusReleasedByDraygonThisFrame { get; internal set; }

    /// <summary>Language flag sampled by $A6:C0D9 when the warning-text phase begins and by Mother Brain's alternate escape-text selection.</summary>
    public bool JapaneseText { get; set; }

    /// <summary>Clears the one-frame events at the start of an enemy pass.</summary>
    internal void BeginFrame()
    {
        CeresEscapeStartedThisFrame = false;
        SamusReleasedByDraygonThisFrame = false;
    }
}
