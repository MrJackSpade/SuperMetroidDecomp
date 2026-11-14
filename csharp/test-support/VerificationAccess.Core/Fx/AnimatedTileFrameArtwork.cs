using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Fx.Artwork;

using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.AssetExtraction.Areas.Tourian;
namespace SuperMetroid.Core.Fx;

/// <summary>
/// The installed characters each animated-tile frame shows (#1406), for checks that model what a room's pictures show over
/// the characters an animation paints.
/// </summary>
internal static class AnimatedTileFrameArtwork
{
    /// <summary>The installed characters of the frame <paramref name="frame"/> names, from <paramref name="atlas"/>.</summary>
    /// <exception cref="InvalidDataException">No animated-tile frame has that identity, or the atlas does not hold it.</exception>
    internal static ReadOnlyMemory<byte> Bytes(BgArtworkIdentity frame, RoomFxBlockAnimationAtlas atlas) => Find(frame).Resolve(atlas);

    /// <summary>The cartridge source of <paramref name="frame"/>'s characters, as the cartridge's copy names it.</summary>
    /// <exception cref="InvalidDataException">No animated-tile frame is <paramref name="frame"/>.</exception>
    internal static int SourceOf(ArtworkFrame frame) => Find(BgArtworkIdentity.Frame(frame)).Source;

    /// <summary>The animated-tile frame <paramref name="frame"/> names, with its characters' cartridge source and how the atlas holds them.</summary>
    private static FramePicture Find(BgArtworkIdentity frame) =>
        ByIdentity.Value.TryGetValue(frame, out FramePicture? picture) ? picture
            : throw new InvalidDataException($"{frame.Name} is not an animated-tile frame.");

    /// <summary>
    /// Every animated-tile frame by the identity it is bound as, built once since checks resolve a frame for each character
    /// they read: the first frame of each identity, in the order the frames list them.
    /// </summary>
    private static readonly Lazy<Dictionary<BgArtworkIdentity, FramePicture>> ByIdentity = new(() =>
    {
        var frames = new Dictionary<BgArtworkIdentity, FramePicture>();
        foreach (FramePicture picture in Frames())
            frames.TryAdd(BgArtworkIdentity.Frame(picture.Frame), picture);
        return frames;
    });

    /// <summary>One animated-tile frame: its characters' cartridge source and how the installed atlas holds them.</summary>
    private sealed record FramePicture(ArtworkFrame Frame, int Source, Func<RoomFxBlockAnimationAtlas, ReadOnlyMemory<byte>> Resolve);

    /// <summary>The installed characters of <paramref name="image"/>, which the atlas must hold.</summary>
    private static Func<RoomFxBlockAnimationAtlas, ReadOnlyMemory<byte>> Picture(AnimatedTileImage image, int byteCount) =>
        atlas => atlas.TryResolve(image, byteCount, out ReadOnlyMemory<byte> characters) ? characters
            : throw new InvalidDataException($"The installed animated tiles hold no {image}.");

    /// <summary>
    /// Writes the frame <paramref name="animatedObject"/> shows in <paramref name="frames"/>, if any, into <paramref name="vram"/>
    /// where the cartridge copies it, for checks that render a layer from VRAM.
    /// </summary>
    internal static void ShowIn(SnesVram vram, ArtworkFrameSelections frames, RoomFxBlockAnimationAtlas atlas, BlockAnimation animatedObject)
    {
        if (!frames.TryShown(BlockAnimationFrames.Of(animatedObject), out ArtworkFrame shown))
            return;
        RoomFxBlockAnimationLoop mechanics = RoomFxBlockAnimationLoops.Require(animatedObject);
        vram.LoadBytes(AnimatedTileVramDestinations.WordOf(mechanics.AnimatedObject) * sizeof(ushort), Bytes(BgArtworkIdentity.Frame(shown), atlas).Span);
    }

    /// <summary>Every animated-tile frame, with the cartridge source and size of its installed characters.</summary>
    private static IEnumerable<FramePicture> Frames()
    {
        foreach (RoomFxBlockAnimationLoop mechanics in RoomFxBlockAnimationLoops.All)
        {
            for (int frame = 0; frame < mechanics.Frames.Count; frame++)
            {
                AnimatedTileImage image = mechanics.Image(frame);
                yield return new(mechanics.Frames[frame].Frame,
                    AnimatedTileImageSources.Of(image), Picture(image, mechanics.PictureByteCount));
            }
        }
        foreach (WreckedShipTreadmillDirection direction in Enum.GetValues<WreckedShipTreadmillDirection>())
        {
            WreckedShipTreadmillLoop treadmill = WreckedShipTreadmills.ForDirection(direction);
            for (int frame = 0; frame < WreckedShipTreadmillMechanicsDefinitions.FrameCount; frame++)
                yield return new(BlockAnimationFrames.Frame(treadmill.AnimatedObject, frame), AnimatedTileImageSources.Of(treadmill.Image(frame)),
                    Picture(treadmill.Image(frame), RoomFxBlockAnimationLoops.TreadmillFrameByteCount));
        }
        foreach (TourianStatueAnimationDefinition statue in TourianStatueAnimatedTilePrograms.All)
        {
            foreach (TourianStatueBlockPicture picture in RoomFxBlockAnimationPictures.TourianStatuePictures)
                yield return new(statue.Picture(picture),
                    AnimatedTileImageSources.TourianStatue(statue.Statue, picture),
                    atlas => atlas.TourianStatuePicture(statue.Statue, picture));
        }
    }
}
