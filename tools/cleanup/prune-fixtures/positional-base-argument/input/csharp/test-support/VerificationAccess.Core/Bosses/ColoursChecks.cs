using SuperMetroid.Core.Bosses;

namespace SuperMetroid.Verification.Bosses;

/// <summary>The colours the checks read.</summary>
internal static class ColoursChecks
{
    /// <summary>Whether the parts show no shade before any band.</summary>
    internal static bool StartsUnshaded => new Colours().PartsShade is null;
}
