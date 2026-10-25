using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The five fixed bank-$A9 enemy placements spawned during Mother Brain's
/// glass-tube collapse. They are separate from room population lists because
/// the cutscene selects one record at a time after the initial room load.
/// </summary>
internal static class MotherBrainFallingTubePopulationDefinitions
{
    /// <summary>The five pieces in the order their spawn parameters and frames count them.</summary>
    internal static readonly IdentitySequence<MotherBrainTubePiece> Pieces = new(
        MotherBrainTubePiece.BottomLeft, MotherBrainTubePiece.BottomRight, MotherBrainTubePiece.BottomMiddleLeft,
        MotherBrainTubePiece.BottomMiddleRight, MotherBrainTubePiece.Main);

    /// <summary>$A9:8B27: selected center X for the main tube; remains required.</summary>
    private const ushort CenterX = 128;
    /// <summary>$A9:8AE7/8AF7: chosen outside-piece horizontal distance from the center; remains required.</summary>
    private const int OuterOffset = 32;
    /// <summary>$A9:8B07/8B17: chosen inside-piece horizontal distance from the center; remains required.</summary>
    private const int InnerOffset = 24;
    /// <summary>$A9:8AE9/8AF9/8B09/8B19/8B29: the five compositions share exclusive bottom Y215.
    /// This chosen placement baseline remains required, independent of installed artwork edits.</summary>
    private const int BottomBaseline = 215;
    /// <summary>$A9:ADA1/ADD5: outer tube compositions reach y28+8=36 below their origin.
    /// Native geometry input remains required; not inferred from an editable presentation instance.</summary>
    private const int OuterBottom = 36;
    /// <summary>$A9:AE09/AE33: inner tube compositions reach y20+8=28 below their origin; required geometry.</summary>
    private const int InnerBottom = 28;
    /// <summary>$A9:AE5D: main tube composition reaches y40+8=48 below its origin; required geometry.</summary>
    private const int MainBottom = 48;
    /// <summary>$A9:8B33: main-tube delay decremented at $8BCB before falling; remains required.</summary>
    private const ushort MainFallDelay = 32;

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
        return new(EnemyDefinitionId.MotherBrainTubes, position, 0, new EnemyPropertyWord(properties), new EnemyExtraPropertyWord(EnemyExtraProperties.None),
            (ushort)(index * sizeof(ushort)), main ? MainFallDelay : (ushort)0);
    }
}
