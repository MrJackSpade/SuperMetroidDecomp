namespace SuperMetroid.Core.Hardware;

/// <summary>
/// Marks a raw PPU layout API (#630): a register number or register-cache access, a VRAM word or byte
/// address, a CGRAM slot, the OAM low/high tables, or a backing buffer exposing them. Only declarations
/// inside a <see cref="PpuLayoutBoundaryAttribute"/> may use it; everything else goes through typed,
/// domain-named rendering views.
/// </summary>
[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method |
    AttributeTargets.Property | AttributeTargets.Constructor | AttributeTargets.Field, Inherited = false)]
public sealed class PpuLayoutAccessAttribute : Attribute;
