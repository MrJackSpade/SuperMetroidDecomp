namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>A family's step for one of its slots at the start of the slot's pass.</summary>
/// <param name="slot">The slot, whose family state belongs to the handler's family.</param>
/// <param name="context">The handler pass.</param>
/// <returns>Whether the family consumed the slot's pass or left it to the shared path.</returns>
internal delegate RoomObjectStepOutcome RoomObjectFamilyStepHandler(RoomObjectSlot slot, RoomObjectStepContext context);
