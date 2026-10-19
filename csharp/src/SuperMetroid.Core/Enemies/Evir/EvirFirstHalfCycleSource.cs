namespace SuperMetroid.Core.Enemies.Evir;

/// <summary>
/// Where a body with one speed record takes its first bobbing half-cycle from: a value of the enemy at a fixed position
/// in the room's population, whatever that enemy is when the body initializes.
/// </summary>
/// <param name="SpeedRecord">The body's linear-speed record, from the low eight bits of its second population parameter.</param>
/// <param name="SlotIndex">The zero-based position, in the room's enemy population, of the enemy whose value is read.</param>
/// <param name="Field">Which of that enemy's values is read.</param>
public readonly record struct EvirFirstHalfCycleSource(int SpeedRecord, int SlotIndex, EvirHalfCycleSourceField Field);
