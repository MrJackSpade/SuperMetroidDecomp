using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The hit areas of Mother Brain's body, brain, and three tested neck joints (#2211): each part's areas around it, in
/// the order her contact tests them, stopping at the first hit.
/// </summary>
internal static class MotherBrainContactHitboxDefinitions
{
    /// <summary>
    /// The ordered collision rectangles of each physical component: the body's lower body, then its narrower upper body;
    /// the brain's upper and lower regions, which meet between local Y zero and one; and the neck joint's one region.
    /// </summary>
    internal static readonly IdentityTable<MotherBrainContactPart, IReadOnlyList<EnemyHitboxRectangle>> Parts = new(new Dictionary<MotherBrainContactPart, IReadOnlyList<EnemyHitboxRectangle>>
    {
        [MotherBrainContactPart.Body] = [new(-32, -24, 42, 56), new(-24, -42, 28, -25)],
        [MotherBrainContactPart.Brain] = [new(-24, -22, 22, 0), new(-22, 1, 16, 20)],
        [MotherBrainContactPart.Neck] = [new(-8, -8, 8, 8)],
    });
}
