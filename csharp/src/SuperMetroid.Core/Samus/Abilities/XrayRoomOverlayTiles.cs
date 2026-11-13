using SuperMetroid.Core.Rooms.Xray;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>An x-ray room overlay and its blocks.</summary>
/// <param name="Overlay">The overlay.</param>
/// <param name="Tiles">The blocks.</param>
public readonly record struct XrayRoomOverlayTiles(XrayRoomOverlayDefinition Overlay, IReadOnlyList<XrayRoomOverlayVisual> Tiles);
