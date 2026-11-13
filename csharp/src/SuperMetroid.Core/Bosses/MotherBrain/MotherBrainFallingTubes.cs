using SuperMetroid.Core.Enemies;
using static SuperMetroid.Core.Bosses.MotherBrain.MotherBrainFallingTubePopulationDefinitions;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Places the pieces of Mother Brain's glass tube as they fall.</summary>
internal static class MotherBrainFallingTubes
{
    /// <summary>
    /// Resolves the selected piece's placement. Parameter one is the piece's offset, consumed by the tube's
    /// initializer. The record's animation is unused: the piece runs its typed animation (#641), so the field is
    /// zero. Placement magnitudes and the main delay remain independent required inputs.
    /// </summary>
    internal static RoomEnemyPopulationRecord Get(MotherBrainTubePiece piece)
    {
        int index = Pieces.PositionOf(piece);
        bool main = piece == MotherBrainTubePiece.Main;
        bool outer = piece is MotherBrainTubePiece.BottomLeft or MotherBrainTubePiece.BottomRight;
        int horizontalOffset = outer ? OuterOffset : InnerOffset;
        int x = main ? CenterX : CenterX + ((index & 1) == 0 ? -horizontalOffset : horizontalOffset);
        int compositionBottom = main ? MainBottom : outer ? OuterBottom : InnerBottom;
        Position16<RoomPixels> position = new(x, BottomBaseline - compositionBottom);
        EnemyProcessing processing = main
            ? EnemyProcessing.Animates | EnemyProcessing.OffScreen
            : EnemyProcessing.Animates;
        return new(EnemyDefinitionId.MotherBrainTubes, position, EnemyContact.SolidToSamus, processing, EnemyPresence.None,
            CollidesByFrameHitboxes: false, new MotherBrainTubeSpawnSettings(piece, main ? MainFallDelay : 0));
    }
}
