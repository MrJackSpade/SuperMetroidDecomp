namespace SuperMetroid.Core.Enemies.Dragon;

/// <summary>A Dragon body's cosmetic wing (#2554): its place and the state its body made for it.</summary>
/// <param name="Place">The wing's place, which the room lists right after its body.</param>
/// <param name="State">The wing's state, which the body made as it was placed and the wing took as it was placed.</param>
internal readonly record struct DragonWing(RoomEnemyPlace Place, DragonWingState State);
