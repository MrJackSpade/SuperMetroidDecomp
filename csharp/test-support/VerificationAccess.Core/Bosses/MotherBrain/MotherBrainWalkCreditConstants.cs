namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Cartridge values and helpers of <see cref="MotherBrainWalkCredit"/> that only verification reads.</summary>
internal static class MotherBrainWalkCreditConstants
{
    /// <summary>Preserves a snapshot's credit word exactly; every word is a valid credit.</summary>
    public static MotherBrainWalkCredit FromNativeWord(ushort word) => new(word);
}
