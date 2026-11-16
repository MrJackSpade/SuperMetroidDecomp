namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Builds an authored Torizo parameter-1 word from its posture, which only checks need.</summary>
internal static class TorizoParameterWords
{
    /// <summary>The posture and the low bits beside it as one parameter word, the form the cartridge authors (#2237).</summary>
    internal static ushort ToParameterWord(TorizoPosture posture, int lowBits)
    {
        int word = Ensure.BetweenInclusive(0, TorizoParameterFlagWords.PostureLowBitsMask, lowBits);
        if (posture.LeadsWithFarFoot)
            word |= TorizoParameterFlagWords.FarFootLeadsBit;
        if (posture.IsTurning)
            word |= TorizoParameterFlagWords.TurningBit;
        if (posture.Facing == HorizontalDirection.Right)
            word |= TorizoParameterFlagWords.FacingRightBit;
        return checked((ushort)word);
    }
}