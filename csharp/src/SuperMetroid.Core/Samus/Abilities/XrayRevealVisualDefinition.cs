using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>One X-ray reveal visual: the block collision kind and reaction it applies to and the blocks it shows.</summary>
/// <param name="Type">The block's collision kind.</param>
/// <param name="Reaction">The reaction the kind reads (<see cref="BlockReactionFamilies.Of"/>), or null for a kind that reads none.</param>
/// <param name="Visual">The blocks the reveal shows.</param>
public readonly record struct XrayRevealVisualDefinition(RoomCollisionType Type, Enum? Reaction, XrayRevealVisualBlocks Visual);
