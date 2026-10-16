namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>What a family step left for the rest of the slot's pass.</summary>
internal enum PlmStepOutcome
{
    /// <summary>The family handled the slot: its pre-instruction, timer and instruction list do not run this pass.</summary>
    Consumed,

    /// <summary>The slot continues through the shared pre-instruction, timer and instruction interpreter.</summary>
    Shared,
}
