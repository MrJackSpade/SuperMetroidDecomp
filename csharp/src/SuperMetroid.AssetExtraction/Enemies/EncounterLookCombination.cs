using System.Collections.Immutable;
using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The look each part of a frame shows, part by part (#1538, #2544): each part's encounter and the look of it shown. Two
/// combinations of the same encounters showing the same looks leave the same characters, so a set's characters are built
/// once per combination.
/// </summary>
internal sealed class EncounterLookCombination : IEquatable<EncounterLookCombination>
{
    /// <summary>Each part's encounter, in the frame's part order.</summary>
    private readonly ImmutableArray<ComposedEncounter> encounters;

    /// <summary>The look of each part's encounter shown, in the same order.</summary>
    private readonly ImmutableArray<ComposedLook> looks;

    /// <summary>The looks <paramref name="combination"/> names of the encounters of <paramref name="parts"/>.</summary>
    /// <exception cref="ArgumentException">There is not one look for each part.</exception>
    internal EncounterLookCombination(IReadOnlyList<SetCharacters.FrameLookPart> parts, ComposedLook[] combination)
    {
        Ensure.NotNull(parts);
        Ensure.NotNull(combination);
        if (parts.Count != combination.Length)
            throw new ArgumentException($"{combination.Length} looks name {parts.Count} parts.", nameof(combination));
        encounters = [.. parts.Select(part => part.Encounter.Encounter)];
        looks = [.. combination];
    }

    /// <inheritdoc/>
    public bool Equals(EncounterLookCombination? other) =>
        other is not null && encounters.SequenceEqual(other.encounters) && looks.SequenceEqual(other.looks);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as EncounterLookCombination);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (ComposedEncounter encounter in encounters)
            hash.Add(encounter);
        foreach (ComposedLook look in looks)
            hash.Add(look);
        return hash.ToHashCode();
    }
}
