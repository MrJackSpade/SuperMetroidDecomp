namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Builds an authored Torizo parameter-1 word from its posture latches, which only checks need.</summary>
internal static class TorizoParameterWords
{
    /// <summary>The posture and the low bits beside it as one parameter word, the form the cartridge authors.</summary>
    internal static ushort ToParameterWord(TorizoPostureFlags posture, int lowBits)
    {
        int word = Ensure.BetweenInclusive(0, TorizoParameterFlagWords.PostureLowBitsMask, lowBits);
        foreach ((TorizoPostureFlags flag, int bit) in TorizoParameterFlagWords.PostureBits)
            if ((posture & flag) != 0)
                word |= bit;
        return checked((ushort)word);
    }
}
