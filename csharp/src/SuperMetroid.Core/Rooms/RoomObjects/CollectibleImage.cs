namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>One image of a collectible's short animations (<see cref="CollectibleSequenceAnimations"/>): the draw it shows at the item's block.</summary>
/// <param name="Draw">The draw.</param>
internal readonly record struct CollectibleImage(RoomObjectDraw Draw);
