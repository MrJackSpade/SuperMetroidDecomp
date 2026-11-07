namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>What a family step left for the rest of the slot's pass.</summary>
internal enum RoomObjectStepOutcome
{
    /// <summary>The family handled the slot: its pre-instruction, timer and instruction list do not run this pass.</summary>
    Consumed,

    /// <summary>The slot continues through the shared pre-instruction, timer and instruction interpreter.</summary>
    Shared,
}
