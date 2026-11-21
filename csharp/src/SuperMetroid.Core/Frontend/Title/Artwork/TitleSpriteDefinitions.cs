namespace SuperMetroid.Core.Frontend.Title.Artwork;

/// <summary>Names the title sheet's frames.</summary>
public static class TitleSpriteDefinitions
{
    /// <summary>The title sheet's frame for <paramref name="sprite"/>.</summary>
    [InstalledKey]
    public static string FrameName(TitleSprite sprite) => $"Title.{TitleSpriteKeys.Installed.Of(sprite)}";

    /// <summary>
    /// The title sheet's frame for the letter <paramref name="typed"/> types: the last of its card's letters it shows
    /// (#1513, #1979).
    /// </summary>
    [InstalledKey]
    public static string LetterFrameName(TitleCardTyping typed) => $"Title.{TitleCardKeys.Installed.Of(typed.Card)}.Letter{typed.Letters}";
}
