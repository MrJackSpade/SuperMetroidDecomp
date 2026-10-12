namespace SuperMetroid.Core.Audio;

/// <summary>
/// Marks a raw S-DSP register API (#631): a register write or read by address, a register number,
/// a voice stride or a field mask. Only declarations inside a <see cref="DspRegisterBoundaryAttribute"/>
/// may use it; everything else goes through the typed global and per-voice controls.
/// </summary>
[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum |
    AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Constructor | AttributeTargets.Field, Inherited = false)]
internal sealed class DspRegisterAccessAttribute : Attribute;

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
