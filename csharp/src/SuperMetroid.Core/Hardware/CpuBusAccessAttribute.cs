namespace SuperMetroid.Core.Hardware;

/// <summary>
/// Marks a raw CPU A-bus API (#629): an address-space read or write, a raw WRAM/SRAM/ROM buffer, or a
/// cell/range primitive constructed from a bus address. Only declarations inside a
/// <see cref="CpuBusBoundaryAttribute"/> may use it; everything else goes through a typed,
/// domain-named memory view.
/// </summary>
[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method |
    AttributeTargets.Property | AttributeTargets.Constructor, Inherited = false)]
public sealed class CpuBusAccessAttribute : Attribute;
