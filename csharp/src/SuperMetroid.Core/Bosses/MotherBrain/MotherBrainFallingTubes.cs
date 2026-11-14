using SuperMetroid.Core.Enemies;
using static SuperMetroid.Core.Bosses.MotherBrain.MotherBrainFallingTubePopulationDefinitions;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Places the pieces of Mother Brain's glass tube as they fall.</summary>
internal static class MotherBrainFallingTubes
{
    /// <summary>
    /// Resolves the selected piece's placement. The piece's offset is consumed by the tube's
    /// initializer. The entry's animation is unused: the piece runs its typed animation (#641), so the field is
    /// zero. Placement magnitudes and the main delay remain independent required inputs.
    /// </summary>
    internal static RoomEnemyPlacement Get(MotherBrainTubePiece piece)
    {
        bool main = piece == MotherBrainTubePiece.Main;
        Position16<RoomPixels> position = Positions[piece];
        EnemyProcessing processing = main
            ? EnemyProcessing.Animates | EnemyProcessing.OffScreen
            : EnemyProcessing.Animates;
        return new(EnemyDefinitionId.MotherBrainTubes, position, EnemyContact.SolidToSamus, processing, EnemyPresence.None,
            CollidesByFrameHitboxes: false, new MotherBrainTubeSpawnSettings(piece, main ? MainFallDelay : 0));
    }
}
