namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>Room-pixel point and its wrapped room block produced by the grapple's radial helper.</summary>
internal readonly record struct GrappleCollisionPoint(
    Position16<RoomPixels> Pixel,
    Position16<RoomBlocks> Block);
