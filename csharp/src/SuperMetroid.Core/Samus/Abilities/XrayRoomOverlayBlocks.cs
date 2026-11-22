using SuperMetroid.Core.Rooms.Xray;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>An x-ray room overlay and its blocks.</summary>
/// <param name="Overlay">The overlay.</param>
/// <param name="Blocks">The blocks.</param>
public readonly partial record struct XrayRoomOverlayBlocks(XrayRoomOverlayDefinition Overlay, IReadOnlyList<XrayRoomOverlayVisual> Blocks);
