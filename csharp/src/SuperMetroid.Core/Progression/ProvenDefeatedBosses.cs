namespace SuperMetroid.Core.Progression;

/// <summary>
/// Keeps defeated bosses named: a typed gameplay API accepts only the kinds of boss whose retail meanings are proven
/// (<see cref="DefeatedBossKinds.AllKnown"/>).
/// </summary>
public static class ProvenDefeatedBosses
{
    /// <summary>Rejects unnamed bosses passed through a typed gameplay API.</summary>
    public static void Validate(DefeatedBosses bosses, string context)
    {
        DefeatedBosses unknown = bosses & ~DefeatedBossKinds.AllKnown;
        if (unknown != DefeatedBosses.None)
        {
            throw new ArgumentOutOfRangeException(
                nameof(bosses),
                bosses,
                $"{context} names bosses the game does not name ({unknown}).");
        }
    }
}
