using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Hardware;

/// <summary>One catalog cell found by reflection: its declaring catalog, member and descriptor.</summary>
internal sealed record CpuBusCatalogEntry(Type Owner, string Member, CpuBusCell Cell)
{
    /// <summary>The <c>Owner.Member</c> reference callers write.</summary>
    internal string Reference => $"{Owner.Name}.{Member}";
}
