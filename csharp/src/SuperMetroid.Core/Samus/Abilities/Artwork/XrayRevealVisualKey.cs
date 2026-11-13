using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Samus.Abilities.Artwork;

/// <summary>The collision kind and reaction an installed X-ray reveal visual applies to.</summary>
/// <param name="Type">The block's collision kind.</param>
/// <param name="Reaction">The reaction the kind reads, or null for a kind that reads none.</param>
internal readonly record struct XrayRevealVisualKey(RoomCollisionType Type, Enum? Reaction);
