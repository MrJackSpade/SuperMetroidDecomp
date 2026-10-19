namespace SuperMetroid.Core.Enemies.Evir;

/// <summary>Data of an Evir body's initialization.</summary>
public static class EvirInitDefinitions
{
    /// <summary>
    /// The first bobbing half-cycle of a body, by speed record. The cartridge's initialization looks the body's own
    /// direction-change period up with the speed-table offset in place of the body's own position, so the value it
    /// halves belongs to another enemy, fixed by the speed record alone. Every Evir in the game uses speed record 8
    /// or 12: record 8 lands on the second enemy's second population parameter, and record 12 on the third enemy's
    /// palette. The read happens while the room's population is loading, so an enemy later in the list than the body
    /// still holds the empty record the room load left.
    /// </summary>
    public static IReadOnlyList<EvirFirstHalfCycleSource> FirstHalfCycleSources { get; } =
    [
        new(SpeedRecord: 8, SlotIndex: 1, EvirHalfCycleSourceField.PopulationParameter2),
        new(SpeedRecord: 12, SlotIndex: 2, EvirHalfCycleSourceField.Palette),
    ];
}
