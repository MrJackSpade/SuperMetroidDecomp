namespace SuperMetroid.Core.Hardware;

/// <summary>
/// Artwork shown over the sprite characters (#1392, #1406): the source and the characters it shows over, from its first.
/// A room's load, its setup and its encounters show their artwork this way, and a pre-coloured frame drawn over those
/// characters has a picture for each placement that can be there.
/// </summary>
/// <param name="Source">The artwork shown.</param>
/// <param name="FirstCharacter">The sprite character it shows from.</param>
/// <param name="CharacterCount">The characters it shows.</param>
public sealed record SpriteCharacterPlacement(ObjCharacterSource Source, int FirstCharacter, int CharacterCount)
{
    /// <summary>The artwork shown.</summary>
    public ObjCharacterSource Source { get; } = Ensure.NotNull(Source);

    /// <summary>The sprite character it shows from.</summary>
    public int FirstCharacter { get; } = Ensure.BetweenInclusive(0, SpriteCharacters.CharacterCount - 1, FirstCharacter);

    /// <summary>The characters it shows.</summary>
    public int CharacterCount { get; } = Ensure.BetweenInclusive(1, SpriteCharacters.CharacterCount - FirstCharacter, CharacterCount);

    /// <summary>Whether the placement shares a character with <paramref name="characterCount"/> characters from <paramref name="firstCharacter"/>.</summary>
    public bool Overlaps(int firstCharacter, int characterCount) =>
        FirstCharacter < firstCharacter + characterCount && firstCharacter < FirstCharacter + CharacterCount;

    /// <summary>Whether the placement shows over every one of <paramref name="characterCount"/> characters from <paramref name="firstCharacter"/>.</summary>
    public bool Covers(int firstCharacter, int characterCount) =>
        FirstCharacter <= firstCharacter && firstCharacter + characterCount <= FirstCharacter + CharacterCount;
}
