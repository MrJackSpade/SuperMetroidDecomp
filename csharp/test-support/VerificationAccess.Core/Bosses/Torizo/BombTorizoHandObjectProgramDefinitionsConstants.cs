namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Cartridge values and helpers of <see cref="BombTorizoHandObject"/> that only verification reads.</summary>
internal static class BombTorizoHandObjectProgramDefinitionsConstants
{
    /// <summary><c>$84:D33B</c>: the pre-instruction that wakes the hand once Samus has Bombs.</summary>
    internal const ushort WakeIfSamusHasBombsFrameBehavior = 0xd33b;
}
