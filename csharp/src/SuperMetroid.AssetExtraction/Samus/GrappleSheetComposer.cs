using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// Composes the grapple sheet (#1392) from the installed stock artwork: the endpoint in each animation frame over the
/// characters that frame uploads, and every rope segment frame in each orientation and rope-direction mirroring over
/// the orientation's characters, coloured as a room load leaves the sprite rows. Each picture is centred on its frame's
/// origin, since the beam places its endpoint and every rope picture by its centre (#1588).
/// </summary>
internal static partial class GrappleSheetComposer
{
    /// <summary>Every rope-direction mirroring a segment is drawn with.</summary>
    internal static IReadOnlyList<SpriteMirror> RopeMirrors { get; } =
        [SpriteMirror.None, SpriteMirror.Horizontal, SpriteMirror.Vertical, SpriteMirror.Horizontal | SpriteMirror.Vertical];

    /// <summary>The rope's orientations in the sheet's order, each listed once (#2567).</summary>
    private static readonly IReadOnlyList<GrappleRopeOrientation> RopeOrientations =
        Ensure.EveryMemberOnce(GrappleSheetDefinitions.RopeOrientations);

    /// <summary>Every frame the grapple sheet holds.</summary>
    internal static IEnumerable<string> Frames =>
        GrappleSheetDefinitions.Endpoint.Select(GrappleSheets.EndpointFrame)
            .Concat(RopeOrientations.SelectMany(orientation =>
                GrappleSheetDefinitions.RopePattern.SelectMany(picture =>
                    RopeMirrors.Select(mirror => GrappleSheets.RopeFrame(orientation, picture, mirror)))));

    /// <summary>The grapple's endpoint character.</summary>
    private const int EndpointCharacter = GrappleTileDefinitions.FirstCharacter;

    /// <summary>The first and last of the four rope characters an orientation uploads.</summary>
    private const int FirstRopeCharacter = EndpointCharacter + 1, LastRopeCharacter = EndpointCharacter + 4;


    /// <summary>What the grapple sheet composes.</summary>
    /// <exception cref="InvalidDataException">The endpoint names a rope character, or a rope segment the endpoint's.</exception>
    internal static SpriteSheetSources Sources(SamusSheetInputs inputs)
    {
        Ensure.NotNull(inputs);
        GrappleTileAtlas tiles = inputs.Projectiles.GrappleTiles;
        var frames = new List<SheetFrameSource>();
        InstalledGrappleSprites sprites = inputs.Projectiles.Compositions.Grapple;
        SpritePart endpoint = sprites.Endpoint;
        if (endpoint.Character is >= FirstRopeCharacter and <= LastRopeCharacter)
            throw new InvalidDataException($"The grapple endpoint names rope character ${endpoint.Character:X3}, which its orientation selects.");
        // Each endpoint picture shows its own characters; each rope picture its own timed appearance.
        foreach (GrappleEndpointFrame point in GrappleSheetDefinitions.Endpoint)
        {
            frames.Add(new(GrappleSheets.EndpointFrame(point), SpritePartFrames.CentredSmallObject(endpoint, 0),
                tiles.OnStandardObjects(inputs.StandardObjects, point, GrappleRopeOrientation.Horizontal)));
        }
        foreach (GrappleRopeOrientation orientation in RopeOrientations)
        {
            SpriteArtwork page = tiles.OnStandardObjects(inputs.StandardObjects, GrappleSheetDefinitions.Endpoint[0], orientation);
            foreach (GrappleRopePicture picture in GrappleSheetDefinitions.RopePattern)
            {
                SpritePart appearance = sprites.Segment(picture);
                if (appearance.Character == EndpointCharacter)
                    throw new InvalidDataException($"Grapple rope picture {picture} names the endpoint's character, which its animation selects.");
                foreach (SpriteMirror mirror in RopeMirrors)
                {
                    // `$94:AFDE` ORs the rope direction's flips into the segment's own attributes.
                    PictureFlip flips = (mirror.HasFlag(SpriteMirror.Horizontal) ? PictureFlip.Horizontal : PictureFlip.None) |
                        (mirror.HasFlag(SpriteMirror.Vertical) ? PictureFlip.Vertical : PictureFlip.None);
                    frames.Add(new(GrappleSheets.RopeFrame(orientation, picture, mirror),
                        SpritePartFrames.CentredSmallObject(appearance with { Flip = appearance.Flip | flips }, 0), page));
                }
            }
        }
        return new SpriteSheetSources(frames, inputs.RoomLoadSpriteColours(), ObjectSelectionCodec.GameplaySizes, Shades: inputs.Shades);
    }
}
