using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Fx;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.AssetExtraction.Fx;

/// <summary>
/// Maps the room-FX atlas's picture identities to where the cartridge stores their characters (#1415): the only place
/// the import knows the bank-$87 artwork addresses the runtime atlas used to be keyed by.
/// </summary>
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
            BlockAnimation.HorizontalSpikes => Simple(RoomFxAnimatedTileSourceDefinitions.HorizontalSpikes),
            BlockAnimation.MaridiaSandCeiling => Simple(RoomFxAnimatedTileSourceDefinitions.MaridiaSandCeiling),
            BlockAnimation.MaridiaSandFalling => Simple(RoomFxAnimatedTileSourceDefinitions.MaridiaSandFalling),
            BlockAnimation.Lava => Simple(RoomFxAnimatedTileSourceDefinitions.Lava),
            BlockAnimation.Acid => Simple(RoomFxAnimatedTileSourceDefinitions.Acid),
            BlockAnimation.Rain => Simple(RoomFxAnimatedTileSourceDefinitions.Rain),
            BlockAnimation.Spores => Simple(RoomFxAnimatedTileSourceDefinitions.Spores),
            BlockAnimation.WreckedShipTreadmillRightwards => new PictureRun(
                RoomFxAnimatedTileSourceDefinitions.WreckedShipTreadmill, RoomFxAnimatedTileLoops.TreadmillFrameByteCount),
            BlockAnimation.WreckedShipTreadmillLeftwards or BlockAnimation.None or BlockAnimation.TourianStatuePhantoon or BlockAnimation.TourianStatueRidley
                or BlockAnimation.TourianStatueKraid or BlockAnimation.TourianStatueDraygon
                or BlockAnimation.VerticalSpikes or BlockAnimation.CrateriaLake
                or BlockAnimation.BrinstarPlant or BlockAnimation.WreckedShipScreen
                => throw new InvalidDataException($"{image.Object} has no pictures in the room-FX atlas."),
            _ => throw new InvalidDataException($"Undefined room-FX object {image.Object}."),
        };
        return first.AddWithinBank(image.Index * byteCount);

        PictureRun Simple(SnesAddress start) =>
            new(start, RoomFxAnimatedTileLoops.Require(image.Object).PictureByteCount);
    }

    /// <summary>An object's first picture and the bytes between consecutive pictures.</summary>
    private readonly record struct PictureRun(SnesAddress First, int ByteCount);
}
