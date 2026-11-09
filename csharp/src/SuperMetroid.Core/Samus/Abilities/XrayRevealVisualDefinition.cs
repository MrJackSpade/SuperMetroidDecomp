using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>One X-ray reveal visual: the block collision kind and reaction it applies to and the blocks it shows.</summary>
/// <param name="Type">The block's collision kind, one that reads its block's reaction.</param>
/// <param name="Reaction">The block's reaction.</param>
/// <param name="Visual">The blocks the reveal shows.</param>
public readonly record struct XrayRevealVisualDefinition(RoomCollisionType Type, BlockReaction Reaction, XrayRevealVisualBlocks Visual);
