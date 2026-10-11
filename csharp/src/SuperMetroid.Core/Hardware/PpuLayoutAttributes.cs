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

/// <summary>
/// Marks an approved PPU layout boundary (#630): the PPU backend, a typed rendering view or layout
/// catalog, a snapshot/DMA codec, or an exact hardware test. Inside it, raw
/// <see cref="PpuLayoutAccessAttribute"/> APIs, addresses and slot arithmetic are allowed.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method | AttributeTargets.Constructor |
    AttributeTargets.Property, Inherited = false)]
public sealed class PpuLayoutBoundaryAttribute : Attribute
{
    /// <summary>Records why this declaration owns raw PPU layout.</summary>
    public PpuLayoutBoundaryAttribute(string reason) => Reason = reason;

    /// <summary>Which boundary this is: backend, rendering view, layout catalog, codec or exact hardware test.</summary>
    public string Reason { get; }
}
