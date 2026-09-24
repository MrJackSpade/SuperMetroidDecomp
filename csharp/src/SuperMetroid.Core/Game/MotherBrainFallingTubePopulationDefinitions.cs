namespace SuperMetroid.Core.Game;

/// <summary>
/// The five fixed bank-$A9 enemy placements spawned during Mother Brain's
/// glass-tube collapse. They are separate from room population lists because
/// the cutscene selects one record at a time after the initial room load.
/// </summary>
internal static class MotherBrainFallingTubePopulationDefinitions
{
    /// <summary>Bank $A9 containing the native tube-collapse placement records.</summary>
    internal const int NativeBank = 0xa90000;

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

    /// <summary>The ordered native record identities for exhaustive parity tests.</summary>
    internal static ReadOnlySpan<ushort> Pointers =>
        [BottomLeft, BottomRight, BottomMiddleLeft, BottomMiddleRight, Main];

    /// <summary>Resolves a complete 16-byte native population record.</summary>
    internal static RoomEnemyPopulationRecord Get(ushort pointer) => pointer switch
    {
        BottomLeft => new(EnemyDefinitionPointers.MotherBrainFallingTube, 0x0060, 0x00b3, 0x8c69, 0xa000, 0x0000, 0x0000, 0x0000),
        BottomRight => new(EnemyDefinitionPointers.MotherBrainFallingTube, 0x00a0, 0x00b3, 0x8c6f, 0xa000, 0x0000, 0x0002, 0x0000),
        BottomMiddleLeft => new(EnemyDefinitionPointers.MotherBrainFallingTube, 0x0068, 0x00bb, 0x8c75, 0xa000, 0x0000, 0x0004, 0x0000),
        BottomMiddleRight => new(EnemyDefinitionPointers.MotherBrainFallingTube, 0x0098, 0x00bb, 0x8c7b, 0xa000, 0x0000, 0x0006, 0x0000),
        Main => new(EnemyDefinitionPointers.MotherBrainFallingTube, 0x0080, 0x00a7, 0x8c81, 0xa800, 0x0000, 0x0008, 0x0020),
        _ => throw new ArgumentOutOfRangeException(nameof(pointer), pointer,
            "Unknown Mother Brain falling-tube population record."),
    };
}
