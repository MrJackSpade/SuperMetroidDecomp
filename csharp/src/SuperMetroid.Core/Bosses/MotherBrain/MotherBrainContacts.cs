using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Finds the collision rectangles of Mother Brain's body, brain and neck.</summary>
internal static class MotherBrainContacts
{
    /// <summary>Returns the ordered collision rectangles for one physical component.</summary>
    internal static IReadOnlyList<EnemyHitboxRectangle> Get(MotherBrainContactPart part) =>
        MotherBrainContactHitboxDefinitions.Parts[part];
}
