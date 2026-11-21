using System.Collections.Immutable;
using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The look each part of a frame shows, part by part (#1538, #2544): each look names the encounter it is a look of. Two
/// combinations showing the same looks of the same encounters leave the same characters, so a set's characters are built
/// once per combination.
/// </summary>
internal sealed class EncounterLookCombination : IEquatable<EncounterLookCombination>
{
    /// <summary>The look each part shows, in the frame's part order, each naming its encounter.</summary>
    private readonly ImmutableArray<ComposedLook> looks;

    /// <summary>The combination of <paramref name="looks"/>, one for each part in the frame's part order.</summary>
    internal EncounterLookCombination(IEnumerable<ComposedLook> looks) => this.looks = [.. Ensure.NotNull(looks)];

    /// <inheritdoc/>
    public bool Equals(EncounterLookCombination? other) => other is not null && looks.SequenceEqual(other.looks);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as EncounterLookCombination);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (ComposedLook look in looks)
            hash.Add(look);
        return hash.ToHashCode();
    }
}
