using SuperMetroid.Core.Frontend.Title;
using SuperMetroid.Core.Frontend.Title.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Title.Artwork;

/// <summary>
/// The colouring the title draws each composition in (#1513), from Core's definitions: each card's reveals in its card's
/// colouring, the logo in the logo's and the copyright in the copyright's. The title sheet pictures each composition in
/// that colouring's sprite set.
/// </summary>
internal static class TitleSpriteColourings
{
    /// <summary>The title's four cards.</summary>
    private static readonly TitleTextSequenceDefinition[] Cards =
    [
        TitleSequenceDefinitions.TextSequences.Year, TitleSequenceDefinitions.TextSequences.Nintendo,
        TitleSequenceDefinitions.TextSequences.Presents, TitleSequenceDefinitions.TextSequences.MetroidThree,
    ];

    /// <summary>The colouring each composition is drawn in.</summary>
    private static readonly Dictionary<TitleSprite, TitleSpriteColouring> Colourings = Drawn();

    /// <summary>The colouring the title draws <paramref name="sprite"/> in.</summary>
    /// <exception cref="InvalidDataException">The title never draws the composition.</exception>
    internal static TitleSpriteColouring Of(TitleSprite sprite) => Colourings.TryGetValue(sprite, out TitleSpriteColouring colouring)
        ? colouring
        : throw new InvalidDataException($"The title never draws {sprite}.");

    /// <summary>Every composition the title draws, with the one colouring it draws it in.</summary>
    /// <exception cref="InvalidDataException">The title draws a composition in two colourings.</exception>
    private static Dictionary<TitleSprite, TitleSpriteColouring> Drawn()
    {
        var colourings = new Dictionary<TitleSprite, TitleSpriteColouring>();
        foreach (TitleTextSequenceDefinition card in Cards)
        {
            foreach (TitleCardStep step in TitleCards.Steps(card.Phase))
            {
                if (step.Sprite is { } sprite)
                    Add(colourings, sprite, card.Colouring);
            }
        }
        Add(colourings, TitleSprite.SuperMetroidLogo, TitleSequenceDefinitions.Sprites.LogoColouring);
        Add(colourings, TitleSprite.NintendoCopyright, TitleSequenceDefinitions.Sprites.CopyrightColouring);
        return colourings;
    }

    /// <summary>Records <paramref name="sprite"/> as drawn in <paramref name="colouring"/>.</summary>
    /// <exception cref="InvalidDataException">It is already drawn in another colouring.</exception>
    private static void Add(Dictionary<TitleSprite, TitleSpriteColouring> colourings, TitleSprite sprite, TitleSpriteColouring colouring)
    {
        if (colourings.TryGetValue(sprite, out TitleSpriteColouring drawn) && drawn != colouring)
            throw new InvalidDataException($"The title draws {sprite} in both {drawn} and {colouring}.");
        colourings[sprite] = colouring;
    }
}
