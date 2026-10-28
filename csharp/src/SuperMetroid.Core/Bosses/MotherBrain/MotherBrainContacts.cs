namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Finds the collision rectangles of Mother Brain's body, brain and neck.</summary>
internal static class MotherBrainContacts
{
    /// <summary>Returns the native ordered collision rectangles for one physical component.</summary>
    internal static MotherBrainContactHitboxes Get(MotherBrainContactPart part) =>
        MotherBrainContactHitboxDefinitions.Parts.TryGetValue(part, out MotherBrainContactHitboxes hitboxes)
            ? hitboxes
            : throw new ArgumentOutOfRangeException(nameof(part));
}
