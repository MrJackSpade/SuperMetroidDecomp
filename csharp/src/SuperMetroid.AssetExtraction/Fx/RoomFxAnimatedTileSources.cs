using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.AssetExtraction.Fx;

/// <summary>
/// Maps the room-FX atlas's picture identities to where the cartridge stores their characters (#1415): the only place
/// the import knows the bank-$87 artwork addresses the runtime atlas used to be keyed by.
/// </summary>
[CpuBusBoundary("codec: room-FX atlas picture identities → bank-$87 artwork sources")]
internal static class RoomFxAnimatedTileSources
{
    /// <summary>Where the cartridge stores <paramref name="segment"/>'s characters.</summary>
    internal static SnesAddress Of(RoomFxAtlasSegment segment) => segment switch
    {
        RoomFxAtlasSegment.Image image => Of(image.Picture),
        RoomFxAtlasSegment.TourianStatueStrip => RoomFxAnimatedTileSourceDefinitions.TourianStatueStrip,
        _ => throw new InvalidDataException($"Undefined room-FX atlas segment {segment}."),
    };

    /// <summary>Where the cartridge stores <paramref name="image"/>'s characters: its object's first picture, then one frame transfer further per picture.</summary>
    /// <exception cref="InvalidDataException">The object has no pictures in the atlas.</exception>
    internal static SnesAddress Of(AnimatedTileImage image)
    {
        (SnesAddress first, int byteCount) = image.Object switch
        {
            AnimatedTileObject.HorizontalSpikes => Simple(RoomFxAnimatedTileSourceDefinitions.HorizontalSpikes),
            AnimatedTileObject.MaridiaSandCeiling => Simple(RoomFxAnimatedTileSourceDefinitions.MaridiaSandCeiling),
            AnimatedTileObject.MaridiaSandFalling => Simple(RoomFxAnimatedTileSourceDefinitions.MaridiaSandFalling),
            AnimatedTileObject.Lava => Simple(RoomFxAnimatedTileSourceDefinitions.Lava),
            AnimatedTileObject.Acid => Simple(RoomFxAnimatedTileSourceDefinitions.Acid),
            AnimatedTileObject.Rain => Simple(RoomFxAnimatedTileSourceDefinitions.Rain),
            AnimatedTileObject.Spores => Simple(RoomFxAnimatedTileSourceDefinitions.Spores),
            AnimatedTileObject.WreckedShipTreadmillRightwards => new PictureRun(
                RoomFxAnimatedTileSourceDefinitions.WreckedShipTreadmill, WreckedShipTreadmillMechanicsDefinitions.FrameByteCount),
            AnimatedTileObject.WreckedShipTreadmillLeftwards or AnimatedTileObject.None or AnimatedTileObject.TourianStatuePhantoon or AnimatedTileObject.TourianStatueRidley
                or AnimatedTileObject.TourianStatueKraid or AnimatedTileObject.TourianStatueDraygon or AnimatedTileObject.Empty
                or AnimatedTileObject.VerticalSpikes or AnimatedTileObject.CrateriaLake or AnimatedTileObject.UnusedCrateriaLava
                or AnimatedTileObject.BrinstarPlant or AnimatedTileObject.WreckedShipScreen
                => throw new InvalidDataException($"{image.Object} has no pictures in the room-FX atlas."),
            _ => throw new InvalidDataException($"Undefined room-FX object {image.Object}."),
        };
        return first.AddWithinBank(image.Index * byteCount);

        PictureRun Simple(SnesAddress start) =>
            new(start, RoomFxAnimatedTileMechanicsDefinitions.Require(image.Object).FrameByteCount);
    }

    /// <summary>An object's first picture and the bytes between consecutive pictures.</summary>
    private readonly record struct PictureRun(SnesAddress First, int ByteCount);
}
