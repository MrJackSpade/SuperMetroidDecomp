namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>One step a cue of Crocomire's walk takes (#2163): how far it moves him, when it may, and whether his foot raises dust first.</summary>
internal sealed record CrocomireStep
{
    /// <summary>The whole pixels the step moves him; negative moves him left, toward the spike wall.</summary>
    public required int HorizontalPixels { get; init; }

    /// <summary>When the step moves him.</summary>
    public required CrocomireStepCondition Condition { get; init; }

    /// <summary>Whether his foot raises random dust before the step moves him.</summary>
    public required bool RaisesFootDust { get; init; }
}
