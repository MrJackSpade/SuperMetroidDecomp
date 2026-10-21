
namespace SuperMetroid.Core.Progression;

/// <summary>An entry of a per-area table.</summary>
/// <param name="Area">The area.</param>
/// <param name="Index">The index.</param>
internal readonly record struct AreaTableEntry(AreaId Area, int Index);
