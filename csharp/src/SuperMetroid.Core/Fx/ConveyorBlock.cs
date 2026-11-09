namespace SuperMetroid.Core.Fx;

/// <summary>A conveyor block: which way it carries Samus, and when.</summary>
/// <param name="Direction">The way the conveyor carries Samus.</param>
/// <param name="Condition">When the conveyor carries her.</param>
internal readonly record struct ConveyorBlock(HorizontalDirection Direction, ConveyorCarryCondition Condition);
