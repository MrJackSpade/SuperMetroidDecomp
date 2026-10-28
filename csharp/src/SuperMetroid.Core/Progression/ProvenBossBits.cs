namespace SuperMetroid.Core.Progression;

/// <summary>
/// Keeps boss state named: a typed gameplay API accepts only the boss bits whose retail meanings are proven
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
                $"{context} contains unproven boss bits ${unknown.ToSaveByte():X2}.");
        }
    }
}
