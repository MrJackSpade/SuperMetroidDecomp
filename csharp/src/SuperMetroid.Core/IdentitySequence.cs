using System.Collections.Frozen;

namespace SuperMetroid.Core;

/// <summary>
/// An authored order of identities: each identity's position in a sequence the game data defines, such as a Yapping
/// Maw's aims clockwise from straight up or the turret directions around the compass in Mother Brain's room. Positions
/// come from the sequence, never from the identities' values.
/// </summary>
/// <typeparam name="T">The identity.</typeparam>
internal sealed class IdentitySequence<T>
    where T : struct, Enum
{
    /// <summary>The identities in sequence order.</summary>
    private readonly T[] _members;

    /// <summary>Each identity's position.</summary>
    private readonly FrozenDictionary<T, int> _positions;

    /// <summary>Keeps <paramref name="members"/> in order; each identity appears once.</summary>
    internal IdentitySequence(params T[] members)
    {
        _members = [.. members];
        _positions = Enumerable.Range(0, _members.Length).ToFrozenDictionary(position => _members[position]);
    }

    /// <summary>The identities in sequence order.</summary>
    internal IReadOnlyList<T> Members => _members;

    /// <summary>The identity at <paramref name="position"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The position is outside the sequence.</exception>
    internal T At(int position) => (uint)position < (uint)_members.Length
        ? _members[position]
        : throw new ArgumentOutOfRangeException(nameof(position), position, $"No {typeof(T).Name} is at position {position}.");


    /// <summary>The position of <paramref name="member"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The identity is not in the sequence.</exception>
    internal int PositionOf(T member) => _positions.TryGetValue(member, out int position)
        ? position
        : throw new ArgumentOutOfRangeException(nameof(member), member, $"{member} is not in the {typeof(T).Name} sequence.");
}
