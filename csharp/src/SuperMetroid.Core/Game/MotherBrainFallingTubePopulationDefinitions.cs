namespace SuperMetroid.Core.Game;

/// <summary>
/// The five fixed bank-$A9 enemy placements spawned during Mother Brain's
/// glass-tube collapse. They are separate from room population lists because
/// the cutscene selects one record at a time after the initial room load.
/// </summary>
internal static class MotherBrainFallingTubePopulationDefinitions
{

    /// <summary>Bottom-left tube spawn at $A9:8AE5.</summary>
    internal const ushort BottomLeft = 0x8ae5;

    /// <summary>Bottom-right tube spawn at $A9:8AF5.</summary>
    internal const ushort BottomRight = 0x8af5;

    /// <summary>Bottom-middle-left tube spawn at $A9:8B05.</summary>
    internal const ushort BottomMiddleLeft = 0x8b05;

    /// <summary>Bottom-middle-right tube spawn at $A9:8B15.</summary>
    internal const ushort BottomMiddleRight = 0x8b15;

    /// <summary>Main tube spawn at $A9:8B25.</summary>
    internal const ushort Main = 0x8b25;

    /// <summary>$A9:8AE5-8B34: five records of eight native words each.</summary>
    private const int RecordBytes = 8 * sizeof(ushort), RecordCount = 5;
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
    /// Resolves the selected piece. Native parameter1 is a word offset consumed at
    /// $A9:8B38; pose lists at $8C69-8C85 each contain a duration/map pair and Sleep.
    /// Placement magnitudes and main delay remain independent required inputs.
    /// </summary>
    internal static RoomEnemyPopulationRecord Get(ushort pointer)
    {
        int offset = pointer - BottomLeft;
        if (offset < 0 || offset >= RecordCount * RecordBytes || offset % RecordBytes != 0)
            throw new ArgumentOutOfRangeException(nameof(pointer), pointer,
                "Unknown Mother Brain falling-tube population record.");
        int piece = offset / RecordBytes;
        bool main = pointer == Main;
        bool outer = piece < 2;
        int horizontalOffset = outer ? OuterOffset : InnerOffset;
        ushort x = main ? CenterX : (ushort)(CenterX + ((piece & 1) == 0 ? -horizontalOffset : horizontalOffset));
        int compositionBottom = main ? MainBottom : outer ? OuterBottom : InnerBottom;
        ushort y = (ushort)(BottomBaseline - compositionBottom);
        ushort pose = (ushort)(MotherBrainFallingTubeInstructionDefinitions.FirstList +
            piece * MotherBrainFallingTubeInstructionDefinitions.ListStride);
        EnemyProperties properties = EnemyProperties.SolidToSamus | EnemyProperties.ProcessInstructions;
        if (main) properties |= EnemyProperties.ProcessOffScreen;
        return new(EnemyDefinitionId.MotherBrainTubes, x, y, pose,
            (ushort)properties, (ushort)EnemyExtraProperties.None,
            (ushort)(piece * sizeof(ushort)), main ? MainFallDelay : (ushort)0);
    }
}
