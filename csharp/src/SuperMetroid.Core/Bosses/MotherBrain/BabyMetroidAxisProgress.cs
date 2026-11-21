namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Where the Baby Metroid's approach along one axis stands after an update (#2557).</summary>
internal enum BabyMetroidAxisProgress
{
    /// <summary>The Baby is still approaching the target along the axis.</summary>
    Approaching,

    /// <summary>The Baby has arrived at the target along the axis.</summary>
    Arrived,
}
