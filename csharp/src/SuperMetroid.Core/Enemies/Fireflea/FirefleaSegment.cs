namespace SuperMetroid.Core.Enemies.Fireflea;

/// <summary>
/// Fireflea's 52-frame glow loop as a typed animation (#641): one segment per animation a spawn, branch or wake enters, in authored order.
/// Their steps are <see cref="FirefleaAnimations"/>.
/// </summary>
internal enum FirefleaSegment
{
    /// <summary>
    /// Beating its wings up, level and down while its glow dims from brightest to dimmest and brightens back,
    /// repeating.
    /// </summary>
    Loop,
}
