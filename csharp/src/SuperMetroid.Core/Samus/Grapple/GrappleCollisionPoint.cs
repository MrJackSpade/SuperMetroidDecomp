namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>A point the grapple's probe along the rope reaches, in room pixels, and the room block it lies in.</summary>
internal readonly record struct GrappleCollisionPoint(
    Position16<RoomPixels> Pixel,
    Position16<RoomBlocks> Block);
