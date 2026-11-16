using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Compiled physical hitboxes for Mother Brain's body, brain, and three tested neck joints (#2211): each part's signed
/// local rectangles in the order her contact tests them, stopping at the first hit. These definitions remain
/// application-owned when visual frames become editable.
/// </summary>
internal static class MotherBrainContactHitboxDefinitions
{
    /// <summary>
    /// The ordered collision rectangles of each physical component: the body's lower body, then its narrower upper body;
    /// the brain's upper and lower regions, which meet between local Y zero and one; and the neck joint's one region.
    /// </summary>
    internal static readonly Dictionary<MotherBrainContactPart, IReadOnlyList<EnemyHitboxRectangle>> Parts = new()
    {
        [MotherBrainContactPart.Body] = [new(-32, -24, 42, 56), new(-24, -42, 28, -25)],
        [MotherBrainContactPart.Brain] = [new(-24, -22, 22, 0), new(-22, 1, 16, 20)],
        [MotherBrainContactPart.Neck] = [new(-8, -8, 8, 8)],
    };
}
