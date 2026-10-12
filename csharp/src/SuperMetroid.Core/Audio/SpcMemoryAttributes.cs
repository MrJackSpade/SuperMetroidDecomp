namespace SuperMetroid.Core.Audio;

/// <summary>
/// Marks a raw SPC700 memory API (#632): raw APU RAM indexing or spans, a raw RAM address,
/// a raw port number or sequence-pointer arithmetic. Only declarations inside a <see cref="SpcMemoryBoundaryAttribute"/>
/// may use it; everything else goes through the typed audio-driver memory views.
/// </summary>
[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum |
    AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Constructor | AttributeTargets.Field, Inherited = false)]
internal sealed class SpcMemoryAccessAttribute : Attribute;

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
