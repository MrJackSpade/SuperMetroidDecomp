using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using System.Collections.Immutable;
using SuperMetroid.AssetExtraction.Artwork;

using SuperMetroid.AssetExtraction.Video;
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
            // A whole pre-coloured frame stands for the parts of the composition it was composed from.
            CharacterSpriteDraw character = SuperMetroid.TestSupport.RepositoryInstallation.Verification.SheetFrameSources.CharacterDraw((ImageSpriteDraw)draw);
            for (int index = 0; index < character.Frame.Parts.Length; index++)
                parts.Add(new(character.Artwork.Id, character.Place(index)));
        }
        return [.. parts];
    }

    /// <summary>
    /// The parts a screen's finalized OAM shows as sprite entries publish them (#1358): one part per record, sized by the
    /// screen's OBJ size selection, over <paramref name="artwork"/>.
    /// </summary>
    internal static PlacedSpritePart[] OfOamObjects(OamBuffer oam, int objectSelection, SpriteArtworkId artwork)
    {
        ScreenObjectSizes sizes = ObjectSelectionCodec.Sizes(objectSelection);
        var parts = new PlacedSpritePart[oam.LastFinalizedSpriteCount];
        for (int index = 0; index < parts.Length; index++)
        {
            OamEntry entry = oam.GetEntry(index);
            int x = entry.Position.X >= 256 ? entry.Position.X - 512 : entry.Position.X;
            int y = entry.Position.Y >= ScreenGeometry.ScreenHeightPixels ? entry.Position.Y - 256 : entry.Position.Y;
            parts[index] = new(artwork, new SpriteScreenPart(new Offset16<ScreenPixels>((short)x, (short)y),
                entry.IsLarge ? sizes.Large : sizes.Small, entry.TileNumber, entry.FlipX, entry.FlipY, entry.Palette, entry.Priority));
        }
        return parts;
    }

    /// <summary>
    /// The parts a frontend screen's finalized OAM shows once captured, converted as <see cref="ObjCharacterImage"/>
    /// converts it. Parts name characters, so the conversion reads no character pixels.
    /// </summary>
    internal static PlacedSpritePart[] OfOam(OamBuffer oam, int objectSelection)
    {
        if (ObjCharacterImageConstants.Composition(oam, new SnesVram(), objectSelection) is not { } composition)
            return [];
        var parts = new PlacedSpritePart[composition.Frame.Parts.Length];
        for (int index = 0; index < parts.Length; index++)
            parts[index] = new(composition.Artwork.Id, composition.Place(index));
        return parts;
    }
}
