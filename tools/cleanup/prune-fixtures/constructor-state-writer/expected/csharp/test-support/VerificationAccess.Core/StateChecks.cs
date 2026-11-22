using SuperMetroid.Core;

namespace SuperMetroid.Verification;

/// <summary>The failures and notes the checks make.</summary>
internal static class StateChecks
{
    /// <summary>Whether an unsupported state keeps its identity.</summary>
    internal static bool KeepsIdentity => new UnsupportedState("door", "no door").Identity == "door";

    /// <summary>Whether a prefixed note joins its parts.</summary>
    internal static bool JoinsPrefix => new Note("a", "b").Text == "ab";
}
