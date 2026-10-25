using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Fx;
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

    /// <summary>The cartridge address of the picture <paramref name="image"/> names, or null when there is none.</summary>
    internal static int? Of(AnimatedTileImage? image) => image is { } shown ? Of(shown) : null;

    /// <summary>The cartridge address the import reads the picture of the rightward treadmill's frame <paramref name="frame"/> from.</summary>
    internal static int Treadmill(int frame) =>
        Of(WreckedShipTreadmillMechanicsDefinitions.ForDirection(WreckedShipTreadmillDirection.Rightwards).Image(frame));

    /// <summary>The cartridge address <paramref name="picture"/> of <paramref name="statue"/> starts at in the shared statue strip.</summary>
    internal static int TourianStatue(AnimatedTileObject statue, TourianStatueTilePicture picture) =>
        RoomFxAnimatedTileSourceDefinitions.TourianStatueStrip.AddWithinBank(
            TourianStatueTileStripDefinitions.FirstTile(statue, picture) * TourianStatueTileStripDefinitions.BytesPerTile).Value;

    /// <summary>The cartridge address the shared Tourian statue strip starts at.</summary>
    internal static int TourianStatueStripStart => RoomFxAnimatedTileSourceDefinitions.TourianStatueStrip.Value;

    /// <summary>The cartridge address after the shared Tourian statue strip.</summary>
    internal static int TourianStatueStripEnd => TourianStatueStripStart + TourianStatueTileStripDefinitions.ByteCount;

    /// <summary>Resolves installed characters by the cartridge address a native frame operand names.</summary>
    extension(RoomFxAnimatedTileAtlas atlas)
    {
        /// <summary>
        /// The installed characters of the picture the import reads from <paramref name="sourceAddress"/>: a ROM oracle's
        /// frame operand, translated to its picture identity through the import's own mapping (#1415).
        /// </summary>
        internal bool TryResolve(int sourceAddress, int byteCount, out ReadOnlyMemory<byte> data)
        {
            foreach (RoomFxAtlasSegment segment in RoomFxAnimatedTileAtlasFormat.Segments)
                if (segment is RoomFxAtlasSegment.Image image && Of(image.Picture) == sourceAddress)
                    return atlas.TryResolve(image.Picture, byteCount, out data);
            foreach (AnimatedTileObject statue in (AnimatedTileObject[])[AnimatedTileObject.TourianStatuePhantoon,
                AnimatedTileObject.TourianStatueRidley, AnimatedTileObject.TourianStatueKraid, AnimatedTileObject.TourianStatueDraygon])
            foreach (TourianStatueTilePicture picture in TourianStatueTileStripDefinitions.Pictures)
            {
                if (TourianStatue(statue, picture) != sourceAddress)
                    continue;
                data = atlas.TourianStatuePicture(statue, picture, byteCount);
                return true;
            }
            data = default;
            return false;
        }
    }

    /// <summary>The cartridge address of the picture shown by the frame whose timed control word sits at bank-$87 <paramref name="instructionPointer"/>.</summary>
    /// <exception cref="InvalidDataException">No frame of <paramref name="definition"/> sits there.</exception>
    internal static int OfCursor(RoomFxAnimatedTileObjectDefinition definition, ushort instructionPointer)
    {
        foreach (RoomFxAnimatedTileFrameDefinition frame in definition.Frames)
            if (frame.InstructionPointer == instructionPointer)
                return Of(definition.Image(frame));
        throw new InvalidDataException(
            $"Room-FX object {definition.AnimatedObject} has no artwork frame at $87:{instructionPointer:X4}.");
    }
}
