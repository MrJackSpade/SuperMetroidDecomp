using SuperMetroid.Core.Game;
using SuperMetroid.Core.Frontend;

namespace SuperMetroid.Desktop;

/// <summary>
/// Exact historical field names whose meaning was generalized under a new name. A renamed
/// field remains part of the current layout, so count-based migrations are unaffected.
/// </summary>
internal static class DebuggerFieldRenameDefinitions
{
    // An explicit inventory, not permission to remap arbitrary unknown fields.
    /// <summary>Approved serialized-field name changes keyed by their exact declaring type and old name.</summary>
    private static readonly Dictionary<(Type DeclaringType, string Name), string> Renames = new()
    {
        // The post-Ceres countdown became the NMI-wait count of every $82:8000 load.
        // Its legacy -1 "not started" value reads as no pending wait.
        [(typeof(SuperMetroidGame), "postCeresLoadFramesRemaining")] = "gameLoadingWaitsRemaining",
        // Native word $0FF2 serves the death explosions and the escape-door dust alike.
        [(typeof(MotherBrainRainbowBeamAttackSequence), "<DeathExplosionIndex>k__BackingField")] =
            "<DeathAndEscapeExplosionIndex>k__BackingField",
    };

    /// <summary>Returns the current name of a renamed serialized field.</summary>
    /// <param name="declaringType">Type that owned the serialized field in the saved state.</param>
    /// <param name="serializedName">Historical field name from the save.</param>
    /// <param name="currentName">Receives its current name when an explicit migration exists.</param>
    /// <returns><see langword="true"/> when the exact historical field is recognized.</returns>
    internal static bool TryGetCurrentName(Type declaringType, string serializedName, out string currentName) =>
        Renames.TryGetValue((declaringType, serializedName), out currentName!);
}
