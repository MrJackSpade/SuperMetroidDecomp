using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Which glass tube piece falls when Mother Brain's room shakes, and how long the main tube waits (#1472).</summary>
/// <param name="Piece">The tube piece.</param>
/// <param name="FallDelay">Updates, 0 through 65535, the piece waits before it falls.</param>
internal sealed record MotherBrainTubeSpawnSettings(MotherBrainTubePiece Piece, int FallDelay) : EnemySpawnSettings
{
    /// <summary>Updates, 0 through 65535, the piece waits before it falls.</summary>
    public int FallDelay { get; init; } = Ensure.BetweenInclusive(0, ushort.MaxValue, FallDelay);
}
