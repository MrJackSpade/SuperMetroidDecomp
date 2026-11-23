using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Which glass tube piece falls when Mother Brain's room shakes (#1472, #1519); the main tube's wait before it falls is its
/// own (<see cref="MotherBrainFallingTubeDefinition.FallWaitUpdates"/>).
/// </summary>
/// <param name="Piece">The tube piece.</param>
internal sealed partial record MotherBrainTubeSpawnSettings(MotherBrainTubePiece Piece) : EnemySpawnSettings;
