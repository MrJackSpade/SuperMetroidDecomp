namespace SuperMetroid.Core.Hardware;

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
