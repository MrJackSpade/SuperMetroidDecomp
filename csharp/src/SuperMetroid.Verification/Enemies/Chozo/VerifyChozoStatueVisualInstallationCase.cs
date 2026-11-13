using SuperMetroid.Core.Enemies.Chozo;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Verification.Enemies.Chozo;

/// <summary>One Chozo slope-access object drawn with the installed art: the object and the terrain block it must keep.</summary>
/// <param name="Header">The statue room object spawned.</param>
/// <param name="Physical">The terrain block the draw must write at its origin.</param>
internal readonly record struct VerifyChozoStatueVisualInstallationCase(ChozoStatueRoomObject Header, RoomTerrainBlock Physical);
