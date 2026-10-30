using SuperMetroid.Core.Enemies;
using static SuperMetroid.Core.Bosses.MotherBrain.MotherBrainFallingTubePopulationDefinitions;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Places the pieces of Mother Brain's glass tube as they fall.</summary>
internal static class MotherBrainFallingTubes
{
    /// <summary>
    /// Resolves the selected piece's placement. Native parameter one is the piece's word offset, consumed at
    /// $A9:8B38. The record's instruction list is unused: the piece runs its typed program (#641), so the field is
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
        Position16<RoomPixels> position = new(checked((ushort)x), checked((ushort)(BottomBaseline - compositionBottom)));
        EnemyProperties properties = EnemyProperties.SolidToSamus | EnemyProperties.ProcessInstructions;
        if (main) properties |= EnemyProperties.ProcessOffScreen;
        return new(EnemyDefinitionId.MotherBrainTubes, position, EnemySpawnSettings.None, new EnemyPropertyWord(properties), new EnemyExtraPropertyWord(EnemyExtraProperties.None),
            (ushort)(index * sizeof(ushort)), main ? MainFallDelay : (ushort)0);
    }
}
