namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Compiled physical hitboxes for Mother Brain's body, brain, and three tested neck joints.
/// These definitions remain application-owned when visual spritemaps become editable.
/// </summary>
internal static class MotherBrainContactHitboxDefinitions
{
    /// <summary>The native ordered collision rectangles of each physical component.</summary>
    internal static readonly Dictionary<MotherBrainContactPart, MotherBrainContactHitboxes> Parts = new()
    {
        [MotherBrainContactPart.Body] = new(MotherBrainContactPart.Body),
        [MotherBrainContactPart.Brain] = new(MotherBrainContactPart.Brain),
        [MotherBrainContactPart.Neck] = new(MotherBrainContactPart.Neck),
    };
}
