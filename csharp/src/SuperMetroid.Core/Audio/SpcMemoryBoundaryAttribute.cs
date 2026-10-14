namespace SuperMetroid.Core.Audio;

/// <summary>
/// Marks an approved SPC700 memory boundary (#632): the APU RAM implementation, a typed driver-memory view,
/// a definition codec, or an exact APU memory test.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method | AttributeTargets.Constructor |
    AttributeTargets.Property, Inherited = false)]
internal sealed class SpcMemoryBoundaryAttribute : Attribute
{
    /// <summary>Records why this declaration owns raw APU memory layout.</summary>
    public SpcMemoryBoundaryAttribute(string reason) => Reason = reason;

    /// <summary>Which boundary this is: implementation, driver-memory view, codec or exact APU memory test.</summary>
    public string Reason { get; }
}
