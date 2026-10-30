namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Builds the Torizo parameter word an egg carries, which only checks need: gameplay only decodes it.</summary>
internal static class TorizoParameterWords
{
    /// <summary>The posture and the low bits beside it as one parameter word, the form an egg carries.</summary>
    internal static ushort ToParameterWord(TorizoPostureFlags posture, int lowBits)
    {
        int word = Ensure.BetweenInclusive(0, TorizoParameterFlagCodec.PostureLowBitsMask, lowBits);
        foreach ((TorizoPostureFlags flag, int bit) in TorizoParameterFlagCodec.PostureBits)
            if ((posture & flag) != 0)
                word |= bit;
        return checked((ushort)word);
    }
}
