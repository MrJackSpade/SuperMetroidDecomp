using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Verification.Enemies.Chozo;

/// <summary>One Chozo slope PLM drawn with the installed art: its header and the terrain block it must keep.</summary>
/// <param name="Header">The PLM header spawned.</param>
/// <param name="Physical">The terrain block the draw must write at its origin.</param>
internal readonly record struct VerifyChozoStatueVisualInstallationCase(PlmHeaderId Header, RoomTerrainBlock Physical);
