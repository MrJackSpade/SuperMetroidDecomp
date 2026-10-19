using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using System.Collections.Immutable;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// One part a published frame description shows: the artwork it reads and its placed screen geometry. Comparing these
/// compares what the description shows without depending on artwork instances, which each capture may rebuild.
/// </summary>
/// <param name="Artwork">The artwork the part's character indexes.</param>
/// <param name="Part">The part placed on screen.</param>
internal readonly record struct PlacedSpritePart(SpriteArtworkId Artwork, SpriteScreenPart Part)
{
    /// <summary>Every part of <paramref name="draws"/>, frontmost first.</summary>
    internal static PlacedSpritePart[] Of(ImmutableArray<SpriteDraw> draws)
    {
        var parts = new List<PlacedSpritePart>();
        foreach (SpriteDraw draw in draws)
        {
            for (int index = 0; index < draw.Frame.Parts.Length; index++)
                parts.Add(new(draw.Artwork.Id, draw.Place(index)));
        }
        return [.. parts];
    }

    /// <summary>
    /// The parts a frontend screen's finalized OAM shows once captured, converted as <see cref="FrontendOamDraws"/>
    /// converts it. Parts name characters, so the conversion reads no character pixels.
    /// </summary>
    internal static PlacedSpritePart[] OfOam(OamBuffer oam, int objectSelection) =>
        Of(FrontendOamDraws.Draws(oam, new SnesVram(), objectSelection));
}
