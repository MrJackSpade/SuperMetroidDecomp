using SuperMetroid.Core.Frontend;

namespace SuperMetroid.Desktop;

/// <summary>
/// Exact historical field names whose meaning was generalized under a new name. A renamed
/// field remains part of the current layout, so count-based migrations are unaffected.
/// </summary>
internal static class DebuggerFieldRenameDefinitions
{
    // An explicit inventory, not permission to remap arbitrary unknown fields.
    private static readonly Dictionary<(Type DeclaringType, string Name), string> Renames = new()
    {
        // The post-Ceres countdown became the NMI-wait count of every $82:8000 load.
        // Its legacy -1 "not started" value reads as no pending wait.
        [(typeof(SuperMetroidGame), "postCeresLoadFramesRemaining")] = "gameLoadingWaitsRemaining",
    };

    /// <summary>Returns the current name of a renamed serialized field.</summary>
    internal static bool TryGetCurrentName(Type declaringType, string serializedName, out string currentName) =>
        Renames.TryGetValue((declaringType, serializedName), out currentName!);
}
