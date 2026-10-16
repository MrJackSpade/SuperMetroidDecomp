namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>A family's step for one of its slots at the start of the slot's pass.</summary>
/// <param name="slot">The slot, whose family state belongs to the handler's family.</param>
/// <param name="context">The handler pass.</param>
/// <returns>Whether the family consumed the slot's pass or left it to the shared path.</returns>
internal delegate PlmStepOutcome PlmFamilyStepHandler(PlmSlot slot, PlmStepContext context);
