using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// A room plane shown through a scroll register: the level pixel at a scrolled position is that position less the
/// layer's offset (the BG offset words $091D-$0923), so camera motion, rumble and per-line scroll all move it.
/// </summary>
/// <param name="Blocks">The room plane's blocks shown (#1403).</param>
/// <param name="ScrollOffset">The scroll register less the layer position.</param>
/// <param name="Pictures">
/// The pre-coloured picture each of the plane's blocks shows (#1392); null until the accepted NMI that latched the plane
/// resolves them.
/// </param>
public sealed record RoomBackgroundLayer(RoomBlockCells Blocks, Offset16<BgPlanePixels> ScrollOffset, RoomBlockPictureTable? Pictures)
    : IBgSource
{
    /// <summary>What the plane shows at a scrolled position: its block's picture pixel.</summary>
    /// <exception cref="InvalidOperationException">The plane's pictures are not yet resolved.</exception>
    public bool TrySample(Position16<BgPlanePixels> scrolled, out BgSample sample) =>
        (Pictures ?? throw new InvalidOperationException("A room plane is shown once the accepted NMI that latched it resolves its pictures."))
            .TrySample(Blocks, scrolled - ScrollOffset, out sample);
}
