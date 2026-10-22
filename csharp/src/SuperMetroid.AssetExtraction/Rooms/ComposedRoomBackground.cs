using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>A composed library background image (#1392): its sheet and the tilemap words it pictures.</summary>
/// <param name="Sheet">The image's sheet, one frame of both pages.</param>
/// <param name="Words">The tilemap words pictured, page by page.</param>
internal sealed record ComposedRoomBackground(ComposedSpriteSheet Sheet, ushort[] Words);
