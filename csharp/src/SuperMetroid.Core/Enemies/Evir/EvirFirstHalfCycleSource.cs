namespace SuperMetroid.Core.Enemies.Evir;

/// <summary>
/// Where a body bobbing at one speed takes its first bobbing half-cycle from: a value of the enemy at a fixed position
/// in the room's population, whatever that enemy is when the body initializes.
/// </summary>
/// <param name="Speed">The body's bobbing speed.</param>
/// <param name="SlotIndex">The zero-based position, in the room's enemy population, of the enemy whose value is read.</param>
/// <param name="Field">Which of that enemy's values is read.</param>
public readonly record struct EvirFirstHalfCycleSource(SignedSixteenSixteen Speed, int SlotIndex, EvirHalfCycleSourceField Field);
