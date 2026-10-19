namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Marks a type that holds one room's state (#1329). Each room load builds it as part of the room scope, and only
/// room-lifetime types and room-scope boundaries keep it, so no owner that outlives the room caches it (SME6409,
/// SME6410).
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
internal sealed class RoomLifetimeAttribute : Attribute;
