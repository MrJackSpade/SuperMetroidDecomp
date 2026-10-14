namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled physical hitboxes for Mother Brain's body, brain, and three tested neck joints.
/// These definitions remain application-owned when visual spritemaps become editable.
/// </summary>
internal static class MotherBrainContactHitboxDefinitions
{

    /// <summary>Returns the native ordered collision rectangles for one physical component.</summary>
    internal static MotherBrainContactHitboxes Get(MotherBrainContactPart part) =>
        part switch
        {
            MotherBrainContactPart.Body or MotherBrainContactPart.Brain or MotherBrainContactPart.Neck => new(part),
            _ => throw new ArgumentOutOfRangeException(nameof(part)),
        };
}
