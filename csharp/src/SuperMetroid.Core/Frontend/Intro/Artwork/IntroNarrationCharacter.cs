namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>One narration character in typewriter order, at the cell it is typed into.</summary>
/// <param name="Column">Zero-based text column for the character.</param>
/// <param name="Row">Zero-based text row for the character.</param>
/// <param name="Character">The character typed there, one the narration's alphabet supports.</param>
public readonly record struct IntroNarrationCharacter(
    int Column,
    int Row,
    char Character)
{
    /// <summary>Whether the character is a space, which types no letter and makes no typewriter sound.</summary>
    public bool IsSpace => Character == ' ';
}
