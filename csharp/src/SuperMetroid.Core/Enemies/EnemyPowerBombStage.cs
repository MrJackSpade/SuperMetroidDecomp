using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Enemies;

/// <summary>A species reaction run by the power-bomb pass for an enemy whose ellipse overlap it accepted.</summary>
internal delegate void EnemyPowerBombStage(RoomEnemySlot enemy, SamusState? samus);
