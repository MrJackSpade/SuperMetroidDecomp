namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Marks a type that builds or holds room scopes, or a scene that owns room-lifetime state of its own, and so may
/// construct and keep room-lifetime types (SME6409, SME6410). The reason names why the type is a boundary.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
internal sealed class RoomScopeBoundaryAttribute : Attribute
{
    /// <summary>Marks a room-scope boundary; SME6409 and SME6410 read the reason from the attribute argument.</summary>
    /// <param name="reason">Why the type builds or holds room-lifetime state.</param>
    public RoomScopeBoundaryAttribute(string reason) => ArgumentException.ThrowIfNullOrWhiteSpace(reason);
}
