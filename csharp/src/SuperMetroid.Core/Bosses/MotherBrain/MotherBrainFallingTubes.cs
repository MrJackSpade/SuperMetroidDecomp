using SuperMetroid.Core.Enemies;
using static SuperMetroid.Core.Bosses.MotherBrain.MotherBrainFallingTubePopulationDefinitions;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Places the pieces of Mother Brain's glass tube as they fall.</summary>
internal static class MotherBrainFallingTubes
{
    /// <summary>
    /// Resolves the selected piece's placement: where the tube's setup places it. The piece runs its own animation (#641).
    /// </summary>
    internal static RoomEnemyPlacement Get(MotherBrainTubePiece piece)
    {
        bool main = piece == MotherBrainTubePiece.Main;
        Position16<RoomPixels> position = Positions[piece];
        EnemyProcessing processing = main
            ? EnemyProcessing.Animates | EnemyProcessing.OffScreen
            : EnemyProcessing.Animates;
        return new(EnemyDefinitionId.MotherBrainTubes, EnemyPlacing.At(position), EnemyContact.SolidToSamus, processing, EnemyPresence.None,
            CollidesByFrameHitboxes: false, new MotherBrainTubeSpawnSettings(piece));
    }
}
