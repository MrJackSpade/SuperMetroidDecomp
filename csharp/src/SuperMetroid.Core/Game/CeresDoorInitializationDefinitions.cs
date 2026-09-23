namespace SuperMetroid.Core.Game;

/// <summary>The seven mutually exclusive meanings of Ceres-door population parameter one.</summary>
internal enum CeresDoorVariant : ushort
{
    NormalFacingRight = 0,
    NormalFacingLeft = 1,
    RotatingElevatorPreExplosionOverlay = 2,
    RidleyRoomFacingRight = 3,
    RotatingElevatorInvisibleWall = 4,
    RidleyEscapeMode7LeftWall = 5,
    RidleyEscapeMode7RightWall = 6,
}

/// <summary>Initial main-function and instruction-list selection for one Ceres-door actor.</summary>
internal readonly record struct CeresDoorInitializationDefinition(
    ushort MainFunction,
    ushort InstructionList);

/// <summary>
/// Compiled behavior selectors used by <c>InitAI_CeresDoor</c> at <c>$A6:F6C5</c>.
/// </summary>
internal static class CeresDoorInitializationDefinitions
{
    /// <summary>
    /// <c>Function_CeresDoor_RotatingElevatorRoom_Default</c> at <c>$A6:F7BD</c>.
    /// </summary>
    internal const ushort RotatingElevatorRoomDefaultFunction = 0xf7bd;

    /// <summary><c>InstList_CeresDoor_Normal_FacingRight</c> at <c>$A6:F56C</c>.</summary>
    private const ushort NormalFacingRightInstructionList =
        CeresDoorInstructionProgramDefinitions.NormalFacingRight;

    /// <summary><c>InstList_CeresDoor_Normal_FacingLeft_0</c> at <c>$A6:F5BE</c>.</summary>
    private const ushort NormalFacingLeftInstructionList =
        CeresDoorInstructionProgramDefinitions.NormalFacingLeft;

    /// <summary>
    /// <c>InstList_CeresDoor_RotatingElevRoom_PreExploDoorOverlay_0</c> at
    /// <c>$A6:F610</c>.
    /// </summary>
    private const ushort RotatingElevatorPreExplosionOverlayInstructionList =
        CeresDoorInstructionProgramDefinitions.RotatingElevatorPreExplosionOverlay;

    /// <summary>
    /// <c>InstList_CeresDoor_RidleysRoom_FacingRight_0</c> at <c>$A6:F53A</c>.
    /// </summary>
    private const ushort RidleyRoomFacingRightInstructionList =
        CeresDoorInstructionProgramDefinitions.RidleyRoomFacingRight;

    /// <summary>
    /// <c>InstList_CeresDoor_RotatingElevatorRoom_InvisibleWall_0</c> at
    /// <c>$A6:F61A</c>.
    /// </summary>
    private const ushort RotatingElevatorInvisibleWallInstructionList =
        CeresDoorInstructionProgramDefinitions.RotatingElevatorInvisibleWall;

    /// <summary>
    /// <c>InstList_CeresDoor_RidleyEscapeMode7LeftWall_0</c> at <c>$A6:F62A</c>.
    /// </summary>
    private const ushort RidleyEscapeMode7LeftWallInstructionList =
        CeresDoorInstructionProgramDefinitions.RidleyEscapeMode7LeftWall;

    /// <summary>
    /// <c>InstList_CeresDoor_RidleyEscapeMode7RightWall_0</c> at <c>$A6:F634</c>.
    /// </summary>
    private const ushort RidleyEscapeMode7RightWallInstructionList =
        CeresDoorInstructionProgramDefinitions.RidleyEscapeMode7RightWall;

    /// <summary>
    /// <c>InitAI_CeresDoor.functionPointers</c> at <c>$A6:F72B-$A6:F738</c> paired
    /// with <c>InstListPointers_CeresDoor</c> at <c>$A6:F52C-$A6:F539</c>.
    /// For full-word population variant i = 0..6, the instruction-list
    /// pointer at $F52C + 2*i is respectively $F56C, $F5BE, $F610,
    /// $F53A, $F61A, $F62A, or $F634. These authored entries select the
    /// normal right/left, pre-explosion, Ridley-room, invisible-wall, and
    /// two mode-7 wall programs; variant seven is outside the table.
    /// </summary>
    private static readonly CeresDoorInitializationDefinition[] Definitions =
    [
        new(CeresEnemyCodePointers.Function_CeresDoor_HandleEarthquakeDuringEscape,
            NormalFacingRightInstructionList),
        new(CeresEnemyCodePointers.Function_CeresDoor_HandleEarthquakeDuringEscape,
            NormalFacingLeftInstructionList),
        new(RotatingElevatorRoomDefaultFunction,
            RotatingElevatorPreExplosionOverlayInstructionList),
        new(CeresEnemyCodePointers.Function_CeresDoor_HandleEarthquakeDuringEscapeInRidleysRoom,
            RidleyRoomFacingRightInstructionList),
        new(CeresEnemyCodePointers.Function_CeresDoor_HandleEarthquakeDuringEscape,
            RotatingElevatorInvisibleWallInstructionList),
        new(CeresEnemyCodePointers.Function_CeresDoor_RidleyEscapeMode7Wall,
            RidleyEscapeMode7LeftWallInstructionList),
        new(CeresEnemyCodePointers.Function_CeresDoor_RidleyEscapeMode7Wall,
            RidleyEscapeMode7RightWallInstructionList),
    ];

    /// <summary>Returns the paired native selectors for one authored population variant.</summary>
    internal static CeresDoorInitializationDefinition For(ushort variant)
    {
        if (variant >= Definitions.Length)
        {
            throw new ArgumentOutOfRangeException(
                nameof(variant), variant,
                "Ceres door initialization variant must be zero through six.");
        }

        return Definitions[variant];
    }
}
