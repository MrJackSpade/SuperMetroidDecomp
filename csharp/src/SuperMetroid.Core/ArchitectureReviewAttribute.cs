namespace SuperMetroid.Core;

/// <summary>
/// Justifies one architecture-audit finding on the type that carries it: <c>back-reference</c>, <c>context-bag</c>,
/// <c>service-locator</c>, <c>large</c> or <c>wide-dependencies</c> (see <c>docs/architecture-audit.md</c>). The reason says why this one site
/// is intrinsic (a framework callback that must reach its host, a test fixture that exists to carry services) or a
/// false positive of the heuristic (a record of installed-artwork data counted as services). The audit requires a
/// review per current finding and fails on a review whose finding is gone. A reason that would be equally true of
/// many other sites is not a justification; the shape is fixed instead.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
public sealed class ArchitectureReviewAttribute : Attribute
{
    /// <summary>Records which finding this type justifies and why that finding does not apply to it.</summary>
    public ArchitectureReviewAttribute(string finding, string reason)
    {
        Finding = finding;
        Reason = reason;
    }

    /// <summary>The finding kind being justified: <c>back-reference</c>, <c>context-bag</c>, <c>service-locator</c>, <c>large</c> or <c>wide-dependencies</c>.</summary>
    public string Finding { get; }

    /// <summary>Why this site is intrinsic or a false positive; specific to the type, not to the rule.</summary>
    public string Reason { get; }
}
