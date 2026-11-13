namespace SuperMetroid.Core.Enemies.Sparks;

/// <summary>The three animation and behaviour pairs a Wrecked Ship Spark's placement can start it with.</summary>
internal static class SparkMovementDefinitions
{
    /// <summary>An active spark that stays active.</summary>
    internal static readonly SparkMovementDefinition AlwaysActive = new(SparkSegment.Active, SparkAction.AlwaysActive);

    /// <summary>An active spark that goes out and comes back on its function timer.</summary>
    internal static readonly SparkMovementDefinition Intermittent = new(SparkSegment.Active, SparkAction.IntermittentActive);

    /// <summary>An emitter that drops falling sparks.</summary>
    internal static readonly SparkMovementDefinition Emitter = new(SparkSegment.Emitter, SparkAction.EmitFallingSparks);

}
