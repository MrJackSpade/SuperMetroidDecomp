namespace SuperMetroid.Core.Frontend.Title.Artwork;

/// <summary>Names the title sheet's frames.</summary>
public static class TitleSpriteDefinitions
{
    /// <summary>The title sheet's frame for <paramref name="sprite"/>.</summary>
    public static string FrameName(TitleSprite sprite) => $"Title.{sprite}";
}
