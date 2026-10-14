namespace SuperMetroid.Core.Assets;

/// <summary>Native tile character indexes of the title punctuation; letters are the contiguous range at <see cref="GameplayMessageTitleDefinitions.FirstLetterTile"/>.</summary>
public enum GameplayMessageTitleGlyph : ushort
{
    /// <summary>Native tile character index for a space.</summary>
    Space = 0x04e,
    /// <summary>Native tile character index for a hyphen.</summary>
    Hyphen = 0x0cf,
    /// <summary>Native tile character index for a period.</summary>
    Period = 0x0fa,
    /// <summary>Native tile character index for a question mark.</summary>
    QuestionMark = 0x0fe,
}
