using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Which glass tube piece falls when Mother Brain's room shakes, and how long the main tube waits (#1472, #1519).</summary>
/// <param name="Piece">The tube piece.</param>
/// <param name="FallDelay">Updates the piece waits before it falls.</param>
internal sealed record MotherBrainTubeSpawnSettings(MotherBrainTubePiece Piece, int FallDelay) : EnemySpawnSettings;
