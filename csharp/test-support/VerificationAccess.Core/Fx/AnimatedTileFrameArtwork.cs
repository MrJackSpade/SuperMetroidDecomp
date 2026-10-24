using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// The installed characters each animated-tile frame shows (#1406), for checks that model what a room's pictures show over
/// the characters an animation paints.
/// </summary>
internal static class AnimatedTileFrameArtwork
{
    /// <summary>The installed characters of the frame <paramref name="frame"/> names, from <paramref name="atlas"/>.</summary>
    /// <exception cref="InvalidDataException">No animated-tile frame has that identity, or the atlas does not hold it.</exception>
    internal static ReadOnlyMemory<byte> Bytes(BgArtworkIdentity frame, RoomFxAnimatedTileAtlas atlas)
    {
        (ArtworkFrame found, int source, int byteCount) = Find(frame);
        return atlas.TryResolve(source, byteCount, out ReadOnlyMemory<byte> characters) ? characters
            : throw new InvalidDataException($"The installed animated tiles hold no {found}.");
    }

    /// <summary>The cartridge source of <paramref name="frame"/>'s characters, as the cartridge's copy names it.</summary>
    /// <exception cref="InvalidDataException">No animated-tile frame is <paramref name="frame"/>.</exception>
    internal static int SourceOf(ArtworkFrame frame) => Find(frame.Artwork).Source;

    /// <summary>The animated-tile frame <paramref name="frame"/> names, with its characters' cartridge source and size.</summary>
    private static (ArtworkFrame Frame, int Source, int ByteCount) Find(BgArtworkIdentity frame) =>
        Frames().FirstOrDefault(candidate => candidate.Frame.Artwork == frame) is { Frame.Animation.Name: not null } found ? found
            : throw new InvalidDataException($"{frame.Name} is not an animated-tile frame.");

    /// <summary>
    /// Writes the frame <paramref name="animatedObject"/> shows in <paramref name="frames"/>, if any, into <paramref name="vram"/>
    /// where the cartridge copies it, for checks that render a layer from VRAM.
    /// </summary>
    [PpuLayoutBoundary("test fixture: an animated-tile object's native VRAM destination")]
    internal static void ShowIn(SnesVram vram, ArtworkFrameSelections frames, RoomFxAnimatedTileAtlas atlas, AnimatedTileObject animatedObject)
    {
        if (!frames.TryShown(AnimatedTileFrames.Of(animatedObject), out ArtworkFrame shown))
            return;
        RoomFxAnimatedTileObjectDefinition mechanics = RoomFxAnimatedTileMechanicsDefinitions.Require(animatedObject);
        vram.LoadBytes(AnimatedTileVramDestinations.WordOf(mechanics.AnimatedObject) * sizeof(ushort), Bytes(shown.Artwork, atlas).Span);
    }

    /// <summary>Every animated-tile frame, with the cartridge source and size of its installed characters.</summary>
    private static IEnumerable<(ArtworkFrame Frame, int Source, int ByteCount)> Frames()
    {
        foreach (RoomFxAnimatedTileObjectDefinition mechanics in RoomFxAnimatedTileMechanicsDefinitions.All)
        {
            for (int frame = 0; frame < mechanics.Frames.Count; frame++)
                yield return (AnimatedTileFrames.Frame(mechanics.AnimatedObject, frame),
                    RoomFxAnimatedTileArtworkDefinitions.SourceAddress(mechanics, mechanics.Frames[frame]), mechanics.FrameByteCount);
        }
        foreach (WreckedShipTreadmillDirection direction in Enum.GetValues<WreckedShipTreadmillDirection>())
        {
            WreckedShipTreadmillObjectDefinition treadmill = WreckedShipTreadmillMechanicsDefinitions.ForDirection(direction);
            for (int frame = 0; frame < WreckedShipTreadmillMechanicsDefinitions.FrameCount; frame++)
                yield return (AnimatedTileFrames.Frame(treadmill.AnimatedObject, frame), treadmill.FrameSource(frame),
                    WreckedShipTreadmillRomData.FrameByteCount);
        }
        foreach (TourianStatueAnimatedTileProgramDefinition statue in TourianStatueAnimatedTileMechanicsDefinitions.All)
        {
            foreach (TourianStatueTilePicture picture in TourianStatueTileStripDefinitions.Pictures)
                yield return (AnimatedTileFrames.Picture(statue.AnimatedObject, picture),
                    TourianStatueTileStripDefinitions.SourceAddress(statue.AnimatedObject, picture), statue.FrameByteCount);
        }
    }
}
