using SuperMetroid.Core.Lists;

namespace SuperMetroid.Verification.Lists;

/// <summary>The lengths the checks compare.</summary>
internal static class ListChecks
{
    /// <summary>Whether two entries are no shorter than one: the only use of <c>&gt;=</c>.</summary>
    internal static bool TwoReachOne => new ListLength(2) >= new ListLength(1);
}
