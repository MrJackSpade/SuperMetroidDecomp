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

/// <summary>
/// Marks an approved CPU A-bus boundary (#629): the address-space implementation, a typed domain
/// memory view, a native-layout codec, or an exact low-level bus test. Inside it, raw
/// <see cref="CpuBusAccessAttribute"/> APIs, address literals and address arithmetic are allowed.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method | AttributeTargets.Constructor |
    AttributeTargets.Property, Inherited = false)]
public sealed class CpuBusBoundaryAttribute : Attribute
{
    /// <summary>Records why this declaration owns raw bus layout.</summary>
    public CpuBusBoundaryAttribute(string reason) => Reason = reason;

    /// <summary>Which boundary this is: implementation, domain view, codec or exact bus test.</summary>
    public string Reason { get; }
}
