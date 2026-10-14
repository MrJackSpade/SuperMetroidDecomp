namespace SuperMetroid.Core.Spatial;

/// <summary>
/// Marks a spatial value type, or a narrow storage/hardware/serialization adapter, that may declare
/// paired X/Y, width/height or horizontal/vertical scalar components (#628).
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method | AttributeTargets.Constructor,
    Inherited = false)]
public sealed class SpatialComponentsAttribute : Attribute
{
    /// <summary>Records why the components are stored or passed separately.</summary>
    public SpatialComponentsAttribute(string reason) => Reason = reason;

    /// <summary>Why this declaration is a component boundary rather than a spatial data clump.</summary>
    public string Reason { get; }
}
