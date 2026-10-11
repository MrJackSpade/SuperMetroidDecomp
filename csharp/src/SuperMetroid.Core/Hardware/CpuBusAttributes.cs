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

/// <summary>
/// Declares that a domain-view property returns exactly the raw value of one catalog cell — the
/// same width, signedness and bytes as a direct read, with no validation or conversion (#629). The
/// SME6290 code fix rewrites a raw read of that cell to this member only when exactly one member
/// declares it.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = false)]
public sealed class CpuBusViewOfAttribute : Attribute
{
    /// <summary>Names the catalog cell the property reads.</summary>
    public CpuBusViewOfAttribute(Type catalog, string cell)
    {
        Catalog = catalog;
        Cell = cell;
    }

    /// <summary>The catalog type declaring the cell.</summary>
    public Type Catalog { get; }

    /// <summary>The cell field's name.</summary>
    public string Cell { get; }
}
