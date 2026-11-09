using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Samus.Abilities.Artwork;

/// <summary>The collision kind and reaction an installed X-ray reveal visual applies to.</summary>
/// <param name="Type">The block's collision kind.</param>
/// <param name="Reaction">The block's reaction.</param>
internal readonly record struct XrayRevealVisualKey(RoomCollisionType Type, BlockReaction Reaction);
