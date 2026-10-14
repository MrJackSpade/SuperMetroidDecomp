namespace SuperMetroid.Core.Audio;

/// <summary>
/// Marks an approved S-DSP register boundary (#631): the DSP implementation, the typed controls,
/// a native-quirk adapter that writes a computed address, or an exact register-level test.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method | AttributeTargets.Constructor |
    AttributeTargets.Property, Inherited = false)]
internal sealed class DspRegisterBoundaryAttribute : Attribute
{
    /// <summary>Records why this declaration owns raw register layout.</summary>
    public DspRegisterBoundaryAttribute(string reason) => Reason = reason;

    /// <summary>Which boundary this is: implementation, typed controls, native quirk or exact register test.</summary>
    public string Reason { get; }
}
