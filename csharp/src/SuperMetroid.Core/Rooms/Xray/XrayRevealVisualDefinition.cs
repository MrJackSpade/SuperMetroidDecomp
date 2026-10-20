using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Abilities;

namespace SuperMetroid.Core.Rooms.Xray;

/// <summary>One X-ray reveal visual: the block collision type and BTS it applies to and the words it shows.</summary>
/// <param name="Type">The block's collision type.</param>
/// <param name="Bts">The block's BTS.</param>
/// <param name="Visual">The words the reveal shows.</param>
public readonly record struct XrayRevealVisualDefinition(RoomCollisionType Type, byte Bts, XrayRevealVisualWords Visual);
