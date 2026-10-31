namespace SuperMetroid.Core.Progression;

/// <summary>
/// Keeps boss state named: a typed gameplay API accepts only the boss flags whose retail meanings are proven
/// (<see cref="BossBitMasks.AllKnown"/>).
/// </summary>
public static class ProvenBossBits
{
    /// <summary>Rejects unnamed flags passed through a typed gameplay API.</summary>
    public static void Validate(BossBits bits, string context)
    {
        BossBits unknown = bits & ~BossBitMasks.AllKnown;
        if (unknown != BossBits.None)
        {
            throw new ArgumentOutOfRangeException(
                nameof(bits),
                bits,
                $"{context} contains boss flags the game does not name ({unknown}).");
        }
    }
}
