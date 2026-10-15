namespace SuperMetroid.Core;

/// <summary>
/// Marks code whose subject is reflection itself: which members a type declares and what their
/// declared types are, or how a serializer keyed by member names reads and writes them. The owned-reflection
/// analyzer (SME6370, #622) accepts reflection only inside such a declaration, and requires the reason.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property, Inherited = false)]
public sealed class ReflectionAuditAttribute : Attribute
{
    /// <summary>Marks a reflection-owning declaration; SME6370 reads the reason from the attribute argument.</summary>
    /// <param name="reason">Why metadata, rather than behavior, is what this declaration verifies.</param>
    public ReflectionAuditAttribute(string reason) => ArgumentException.ThrowIfNullOrWhiteSpace(reason);
}
