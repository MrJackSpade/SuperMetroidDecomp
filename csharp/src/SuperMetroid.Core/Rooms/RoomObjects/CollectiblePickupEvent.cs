namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>One defined permanent pickup completed by the current room-object pass.</summary>
/// <param name="Kind">The item whose inventory effect and collected-item persistence have been applied.</param>
/// <param name="Message">The gameplay-message identity requested for the synchronous pickup notification.</param>
public readonly record struct CollectiblePickupEvent(
    InWorldCollectibleKind Kind,
    GameplayMessageId Message);
