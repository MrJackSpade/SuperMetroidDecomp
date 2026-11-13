using SuperMetroid.AssetExtraction.Areas.Tourian;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx.Artwork;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// Verification's cartridge view of the room-FX atlas's picture identities (#1415): the bank-$87 artwork address the
/// import reads each picture from, so a ROM oracle comparing a native frame operand checks the import's mapping.
/// </summary>
internal static class AnimatedTileImageSources
{
    /// <summary>The cartridge address the import reads <paramref name="image"/>'s characters from.</summary>
    internal static int Of(AnimatedTileImage image) => RoomFxAnimatedTileSources.Of(image).Value;

    /// <summary>
    /// The cartridge address of the picture the frame <paramref name="animatedObject"/> shows in <paramref name="frames"/>
    /// names, or null when it shows none: the selected frame folded through the import's frame-to-picture mapping
    /// (#1527), for a ROM oracle comparing a native frame operand.
    /// </summary>
    internal static int? OfShown(ArtworkFrameSelections frames, BlockAnimation animatedObject) =>
        frames.TryShown(BlockAnimationFrames.Of(animatedObject), out ArtworkFrame shown) ? AnimatedTileFrameArtwork.SourceOf(shown) : null;

    /// <summary>The cartridge address the import reads the picture of the rightward treadmill's frame <paramref name="frame"/> from.</summary>
    internal static int Treadmill(int frame) =>
        Of(WreckedShipTreadmills.ForDirection(WreckedShipTreadmillDirection.Rightwards).Image(frame));

    /// <summary>The cartridge address <paramref name="picture"/> of <paramref name="statue"/> starts at in the shared statue strip.</summary>
    internal static int TourianStatue(TourianGreyStatues statue, TourianStatueBlockPicture picture) =>
        RoomFxAnimatedTileSourceDefinitions.TourianStatueStrip.AddWithinBank(
            RoomFxAnimatedTilePictures.TourianStatueFirstTile(statue, picture) * TourianStatueTileStripDefinitions.CharacterByteCount).Value;

    /// <summary>The cartridge address the shared Tourian statue strip starts at.</summary>
    internal static int TourianStatueStripStart => RoomFxAnimatedTileSourceDefinitions.TourianStatueStrip.Value;

    /// <summary>The cartridge address after the shared Tourian statue strip.</summary>
    internal static int TourianStatueStripEnd => TourianStatueStripStart + RoomFxAnimatedTileLoops.TourianStatueStripByteCount;

    /// <summary>Resolves installed characters by the cartridge address a native frame operand names.</summary>
    extension(RoomFxAnimatedTileAtlas atlas)
    {
        /// <summary>
        /// The installed characters of the picture the import reads from <paramref name="sourceAddress"/>: a ROM oracle's
        /// frame operand, translated to its picture identity through the import's own mapping (#1415).
        /// </summary>
        /// <exception cref="InvalidDataException"><paramref name="byteCount"/> is not the picture's size.</exception>
        internal bool TryResolve(int sourceAddress, int byteCount, out ReadOnlyMemory<byte> data)
        {
            foreach (RoomFxAtlasSegment segment in RoomFxAnimatedTileAtlasFile.Segments)
                if (segment is RoomFxAtlasSegment.Image image && Of(image.Picture) == sourceAddress)
                    return atlas.TryResolve(image.Picture, byteCount, out data);
            foreach (TourianStatueAnimationDefinition statue in TourianStatueAnimatedTilePrograms.All)
            foreach (TourianStatueBlockPicture picture in RoomFxAnimatedTilePictures.TourianStatuePictures)
            {
                if (TourianStatue(statue.Statue, picture) != sourceAddress)
                    continue;
                int pictureByteCount = RoomFxAnimatedTilePictures.TourianStatuePictureByteCount(statue.Statue);
                if (byteCount != pictureByteCount)
                    throw new InvalidDataException(
                        $"Tourian statue {statue.Statue} {picture} requires {pictureByteCount} bytes, not {byteCount}.");
                data = atlas.TourianStatuePicture(statue.Statue, picture);
                return true;
            }
            data = default;
            return false;
        }
    }

    /// <summary>The cartridge address of the picture shown by the frame whose timed control word sits at bank-$87 <paramref name="instructionPointer"/>.</summary>
    /// <exception cref="InvalidDataException">No frame of <paramref name="definition"/> sits there.</exception>
    internal static int OfCursor(RoomFxBlockAnimationLoop definition, ushort instructionPointer) =>
        TryOfCursor(definition, instructionPointer, out int source)
            ? source
            : throw new InvalidDataException(
                $"Room-FX object {definition.AnimatedObject} has no artwork frame at $87:{instructionPointer:X4}.");

    /// <summary>
    /// Whether a frame of <paramref name="definition"/> has its timed control word at bank-$87
    /// <paramref name="instructionPointer"/>, and the cartridge address of its picture when one does: the classification
    /// <see cref="OfCursor"/> rejects through, so a sweep of the whole cursor domain classifies each cursor without an
    /// exception per non-frame cursor (#1532).
    /// </summary>
    internal static bool TryOfCursor(RoomFxBlockAnimationLoop definition, ushort instructionPointer, out int source)
    {
        for (int frame = 0; frame < definition.Frames.Count; frame++)
        {
            if (definition.FrameInstructionPointer(frame) == instructionPointer)
            {
                source = Of(definition.Image(frame));
                return true;
            }
        }
        source = 0;
        return false;
    }
}
