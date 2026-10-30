using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Verification.Enemies.Chozo;

/// <summary>One Chozo slope PLM drawn with edited art: its header, the terrain block it must keep and the appearance shown.</summary>
/// <param name="Header">The PLM header spawned.</param>
/// <param name="Physical">The terrain block the draw must write at its origin.</param>
/// <param name="Visual">The edited appearance the room must display there.</param>
internal readonly record struct VerifyChozoStatueVisualInstallationCase(PlmHeaderId Header, RoomTerrainBlock Physical, RoomBlockAppearance Visual);
