using SuperMetroid.Core.Frontend.Title;
using SuperMetroid.Core.Frontend.Title.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Title.Artwork;

/// <summary>
/// The colouring the title draws each composition in (#1513), from Core's definitions: each card's text in its card's
/// colouring, the logo in the logo's and the copyright in the copyright's. The title sheet pictures each composition in
/// that colouring's sprite set.
/// </summary>
internal static partial class TitleSpriteColourings
{
    /// <summary>The title's four cards.</summary>
    private static IReadOnlyList<TitleTextSequenceDefinition> Cards => TitleSequenceDefinitions.TextSequences.All;

    /// <summary>The colouring the title draws <paramref name="sprite"/> in.</summary>
    /// <exception cref="InvalidDataException">The title never draws the composition.</exception>
    internal static TitleSpriteColouring Of(TitleSprite sprite) => sprite switch
    {
        TitleSprite.SuperMetroidLogo => TitleSequenceDefinitions.Sprites.LogoColouring,
        TitleSprite.NintendoCopyright => TitleSequenceDefinitions.Sprites.CopyrightColouring,
        _ => throw new InvalidDataException($"The title never draws {sprite}."),
    };

    /// <summary>The colouring the title draws <paramref name="typed"/>, a card's text typed so far, in: its card's.</summary>
    /// <exception cref="InvalidDataException">No title card shows the typing.</exception>
    internal static TitleSpriteColouring Of(TitleCardTyping typed) =>
        Cards.Where(card => card.Card.Card == typed.Card).Select(card => (TitleSpriteColouring?)card.Colouring).SingleOrDefault()
            ?? throw new InvalidDataException($"No title card types {typed}.");
}
