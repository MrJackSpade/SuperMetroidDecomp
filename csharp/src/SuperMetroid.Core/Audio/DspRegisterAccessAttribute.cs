namespace SuperMetroid.Core.Audio;

/// <summary>
/// Marks a raw S-DSP register API (#631): a register write or read by address, a register number,
/// a voice stride or a field mask. Only declarations inside a <see cref="DspRegisterBoundaryAttribute"/>
/// may use it; everything else goes through the typed global and per-voice controls.
/// </summary>
[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum |
    AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Constructor | AttributeTargets.Field, Inherited = false)]
internal sealed class DspRegisterAccessAttribute : Attribute;
