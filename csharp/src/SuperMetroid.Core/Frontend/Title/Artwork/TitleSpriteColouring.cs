namespace SuperMetroid.Core.Frontend.Title.Artwork;

/// <summary>
/// The colours a title composition's parts draw in (#1512): what the title shows each composition as. Which of the
/// title's colours each one is, the import alone knows.
/// </summary>
public enum TitleSpriteColouring
{
    /// <summary>The opening cards' text: the year, Nintendo, presents and Metroid 3 cards.</summary>
    Text,

    /// <summary>The Super Metroid logo.</summary>
    Logo,

    /// <summary>The Nintendo copyright under the logo.</summary>
    Copyright,
}
