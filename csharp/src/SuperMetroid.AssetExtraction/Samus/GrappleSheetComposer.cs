using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.Rendering;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Grapple;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// Composes the grapple sheet (#1392) from the installed stock artwork: the endpoint in each animation frame over the
/// characters that frame uploads, and every rope segment frame in each orientation and rope-direction mirroring over
/// the orientation's characters, coloured as a room load leaves the sprite rows.
/// </summary>
internal static class GrappleSheetComposer
{
    /// <summary>The grapple's endpoint character.</summary>
    private const int EndpointCharacter = GrappleTileDefinitions.FirstCharacter;

    /// <summary>The first and last of the four rope characters an orientation uploads.</summary>
    private const int FirstRopeCharacter = EndpointCharacter + 1, LastRopeCharacter = EndpointCharacter + 4;

    /// <summary>CGRAM's first sprite colour.</summary>
    private const int SpriteColours = 128;

    /// <summary>What the grapple sheet composes.</summary>
    /// <exception cref="InvalidDataException">The endpoint names a rope character, or a rope segment the endpoint's.</exception>
    internal static SpriteSheetSources Sources(SamusSheetInputs inputs)
    {
        Ensure.NotNull(inputs);
        GrappleTileAtlas tiles = inputs.Projectiles.GrappleTiles;
        var frames = new List<SheetFrameSource>();
        var endpoint = new SnesObjAttributeWord(checked((ushort)(tiles.Sprites?.Endpoint ?? GrappleBeamDefinitions.Rendering.EndpointAttributes)));
        if (endpoint.TileNumber is >= FirstRopeCharacter and <= LastRopeCharacter)
            throw new InvalidDataException($"The grapple endpoint names rope character ${endpoint.TileNumber:X3}, which its orientation selects.");
        for (int point = 0; point < GrappleSheetDefinitions.EndpointFrameCount; point++)
        {
            frames.Add(new(GrappleSheetDefinitions.EndpointFrame(point), SpritePartFrames.SmallObject(endpoint, 0),
                tiles.OnStandardObjects(inputs.StandardObjects, point, GrappleRopeOrientation.Horizontal)));
        }
        foreach (GrappleRopeOrientation orientation in Enum.GetValues<GrappleRopeOrientation>())
        {
            SpriteArtwork page = tiles.OnStandardObjects(inputs.StandardObjects, 0, orientation);
            for (int segment = 0; segment < GrappleSpriteDefinitions.SegmentFrameCount; segment++)
            {
                ushort appearance = tiles.Sprites?.Segment(segment)
                    ?? unchecked((ushort)(GrappleBeamDefinitions.Rendering.FirstSegmentAttributes + segment));
                if (new SnesObjAttributeWord(appearance).TileNumber == EndpointCharacter)
                    throw new InvalidDataException($"Grapple rope segment {segment} names the endpoint's character, which its animation selects.");
                foreach (SpriteMirror mirror in GrappleSheetDefinitions.RopeMirrors)
                {
                    // `$94:AFDE` ORs the rope direction's flips into the segment's own attributes.
                    TileFlip flips = ((mirror & SpriteMirror.Horizontal) != 0 ? TileFlip.Horizontal : TileFlip.None) |
                        ((mirror & SpriteMirror.Vertical) != 0 ? TileFlip.Vertical : TileFlip.None);
                    frames.Add(new(GrappleSheetDefinitions.RopeFrame(orientation, segment, mirror),
                        SpritePartFrames.SmallObject(new SnesObjAttributeWord(appearance).Or(SnesObjAttributeWord.Create(0, 0, 0, flips)), 0), page));
                }
            }
        }
        return new SpriteSheetSources(frames, inputs.RoomLoadColours().Colors[SpriteColours..].ToArray(), ObjectSelectionCodec.Sizes(GameplayRenderDefinitions.ObjectSelection));
    }
}
