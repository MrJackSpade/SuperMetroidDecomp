using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>One X-ray reveal visual: the block collision kind and behaviour it applies to and the blocks it shows.</summary>
/// <param name="Type">The block's collision kind.</param>
/// <param name="Behavior">The block's behaviour.</param>
/// <param name="Visual">The blocks the reveal shows.</param>
public readonly record struct XrayRevealVisualDefinition(RoomCollisionType Type, RoomBlockBehavior Behavior, XrayRevealVisualBlocks Visual);
