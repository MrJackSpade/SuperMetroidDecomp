namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>
/// Marks a raw SPC700 memory API (#632): raw APU RAM indexing or spans, a raw RAM address,
/// a raw port number or sequence-pointer arithmetic. Only declarations inside a <see cref="SpcMemoryBoundaryAttribute"/>
/// may use it; everything else goes through the typed audio-driver memory views.
/// </summary>
[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum |
    AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Constructor | AttributeTargets.Field, Inherited = false)]
internal sealed class SpcMemoryAccessAttribute : Attribute;
