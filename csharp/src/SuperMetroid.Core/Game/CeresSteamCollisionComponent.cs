namespace SuperMetroid.Core.Game;

/// <summary>Immutable physical component of a Ceres steam extended frame.</summary>
internal readonly record struct CeresSteamCollisionComponent(Offset16<RoomPixels> Offset, ushort HitboxPointer);
