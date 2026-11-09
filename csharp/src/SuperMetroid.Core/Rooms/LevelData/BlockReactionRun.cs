namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// Consecutive common reactions of one collision kind, each standing for one meaning in order: the four blue-door caps,
/// the seven station accesses, the eight downward-gate triggers.
/// </summary>
/// <typeparam name="T">What each reaction of the run stands for.</typeparam>
public sealed class BlockReactionRun<T>
    where T : struct, Enum
{
    /// <summary>The run's first reaction.</summary>
    private readonly BlockReaction first;

    /// <summary>What each reaction stands for, in order.</summary>
    private readonly T[] meanings;

    /// <summary>A run of common reactions starting at <paramref name="firstEntry"/>.</summary>
    /// <param name="firstEntry">The common entry of the run's first reaction.</param>
    /// <param name="meanings">What each reaction stands for, in order; each meaning once.</param>
    /// <exception cref="ArgumentException">The run is empty, repeats a meaning, or runs past the last entry.</exception>
    public BlockReactionRun(int firstEntry, params T[] meanings)
    {
        Ensure.NotNull(meanings);
        if (meanings.Length == 0 || meanings.Distinct().Count() != meanings.Length)
            throw new ArgumentException("A reaction run names each of at least one meaning once.", nameof(meanings));
        first = BlockReaction.Common(firstEntry);
        _ = BlockReaction.Common(firstEntry + meanings.Length - 1);
        this.meanings = [.. meanings];
    }

    /// <summary>The reaction standing for <paramref name="meaning"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="meaning"/> is not in the run.</exception>
    public BlockReaction ReactionFor(T meaning)
    {
        int index = Array.IndexOf(meanings, meaning);
        return index >= 0
            ? BlockReaction.Common(first.Entry + index)
            : throw new ArgumentOutOfRangeException(nameof(meaning), meaning, $"Undefined {typeof(T).Name}.");
    }

    /// <summary>What <paramref name="reaction"/> stands for, if it is in the run.</summary>
    public bool TryGetMeaning(BlockReaction reaction, out T meaning)
    {
        int index = reaction.Entry - first.Entry;
        if (reaction.Scope == BlockReactionScope.Common && index >= 0 && index < meanings.Length)
        {
            meaning = meanings[index];
            return true;
        }

        meaning = default;
        return false;
    }
}
