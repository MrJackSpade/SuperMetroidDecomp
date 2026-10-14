namespace SuperMetroid.Core.Hardware;

/// <summary>One PPU layout catalog cell found by reflection: its declaring catalog, member and descriptor.</summary>
internal sealed record PpuLayoutCatalogEntry(Type Owner, string Member, PpuLayoutCell Cell)
{
    /// <summary>The <c>Owner.Member</c> reference callers write.</summary>
    internal string Reference => $"{Owner.Name}.{Member}";
}
