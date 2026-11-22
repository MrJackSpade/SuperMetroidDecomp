using SuperMetroid.Core;

namespace SuperMetroid.Verification;

/// <summary>The tail steps the checks read.</summary>
internal static class TailStepsChecks
{
    /// <summary>Whether the stab step has a name.</summary>
    internal static bool StabNamed => TailSteps.StabName.Length > 0;
}
