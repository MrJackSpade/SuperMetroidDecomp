namespace SuperMetroid.Verification.Samus.Abilities;

/// <summary>The bts, area and address of one speed booster plm programs entry.</summary>
/// <param name="Bts">The bts.</param>
/// <param name="Area">The area.</param>
/// <param name="Address">The address.</param>
internal readonly record struct SpeedBoosterPlmProgramsEntry(byte Bts, int Area, int Address);
