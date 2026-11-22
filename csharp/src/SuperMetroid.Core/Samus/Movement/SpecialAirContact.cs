using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>A special-air block Samus's movement reached, with the reaction it carries (#2554).</summary>
/// <param name="Block">The special-air block the collision scan reached.</param>
/// <param name="Reaction">The special-air reaction the block carries.</param>
internal readonly partial record struct SpecialAirContact(RoomCollisionBlock Block, SpecialAirReaction Reaction);
