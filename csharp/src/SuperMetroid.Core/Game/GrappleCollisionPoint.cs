namespace SuperMetroid.Core.Game;

/// <summary>Room-pixel point and its byte-masked room block produced by bank-$94's radial helper.</summary>
internal readonly record struct GrappleCollisionPoint(
    Position16<RoomPixels> Pixel,
    Position16<RoomBlocks> Block);
