namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// The reactions of one collision kind that each stand for one meaning (#1722): the four blue-door caps, the seven station
/// accesses, the eight downward-gate triggers, the eight crumble blocks.
/// </summary>
/// <typeparam name="TReaction">The kind's reaction.</typeparam>
/// <typeparam name="TMeaning">What each reaction stands for.</typeparam>
public sealed class BlockReactionMeanings<TReaction, TMeaning>
    where TReaction : struct, Enum
    where TMeaning : struct, Enum
{
    /// <summary>What each reaction stands for.</summary>
    private readonly Dictionary<TReaction, TMeaning> meanings;

    /// <summary>The reaction standing for each meaning.</summary>
    private readonly Dictionary<TMeaning, TReaction> reactions;

    /// <summary>Pairs each reaction with the one meaning it stands for.</summary>
    /// <param name="meanings">Each reaction and its meaning; each reaction and each meaning once.</param>
    /// <exception cref="ArgumentException">The pairs are empty or repeat a reaction or a meaning.</exception>
    public BlockReactionMeanings(IReadOnlyDictionary<TReaction, TMeaning> meanings)
    {
        Ensure.NotNull(meanings);
        if (meanings.Count == 0 || meanings.Values.Distinct().Count() != meanings.Count)
            throw new ArgumentException("Reaction meanings pair each of at least one meaning with one reaction.", nameof(meanings));
        this.meanings = new Dictionary<TReaction, TMeaning>(meanings);
        reactions = meanings.ToDictionary(pair => pair.Value, pair => pair.Key);
    }

    /// <summary>The reaction standing for <paramref name="meaning"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="meaning"/> is not among the meanings.</exception>
    public TReaction ReactionFor(TMeaning meaning) =>
        reactions.TryGetValue(meaning, out TReaction reaction)
            ? reaction
            : throw new ArgumentOutOfRangeException(nameof(meaning), meaning, $"Undefined {typeof(TMeaning).Name}.");

    /// <summary>What <paramref name="reaction"/> stands for, if it is among the reactions.</summary>
    public bool TryGetMeaning(TReaction reaction, out TMeaning meaning) => meanings.TryGetValue(reaction, out meaning);
}
