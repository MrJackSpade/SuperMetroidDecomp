namespace SuperMetroid.Core;

/// <summary>
/// Justifies one architecture-audit finding on the type that carries it: <c>back-reference</c>, <c>context-bag</c>,
/// <c>service-locator</c>, <c>large</c> or <c>wide-dependencies</c>; or, on a member, <c>test-only-member</c> (a member
/// wider than its production use that only checks reach outside its type, #2570). The reason says why this one site
/// is intrinsic (a framework callback that must reach its host, a type that exists only to carry services) or a
/// false positive of the heuristic (a record of installed-artwork data counted as services). The audit requires a
/// review per current finding and fails on a review whose finding is gone. A reason that would be equally true of
/// many other sites is not a justification; the shape is fixed instead.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Field | AttributeTargets.Property |
    AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Event, AllowMultiple = true, Inherited = false)]
public sealed class ArchitectureReviewAttribute : Attribute
{
    /// <summary>Records which finding this type justifies and why that finding does not apply to it.</summary>
    public ArchitectureReviewAttribute(string finding, string reason)
    {
        Finding = finding;
        Reason = reason;
    }

    /// <summary>The finding kind being justified: <c>back-reference</c>, <c>context-bag</c>, <c>service-locator</c>, <c>large</c>, <c>wide-dependencies</c> or <c>test-only-member</c>.</summary>
    public string Finding { get; }

    /// <summary>Why this site is intrinsic or a false positive; specific to the type, not to the rule.</summary>
    public string Reason { get; }
}
