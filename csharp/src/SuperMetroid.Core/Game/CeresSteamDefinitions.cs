namespace SuperMetroid.Core.Game;

/// <summary>
/// Mutually exclusive population variants accepted by the Ceres steam initializer.
/// The numeric values are the exact parameter-one indexes used by <c>$A6:EFB1</c>.
/// </summary>
public enum CeresSteamVariant : ushort
{
    Up = 0,
    Left = 1,
    Down = 2,
    Right = 3,
    RotatingElevatorLeft = 4,
    RotatingElevatorRight = 5,
}

/// <summary>
/// Mutually exclusive bank-$A6 main-function identities stored in the steam actor's
/// variable A by <c>$A6:EFB1</c> and dispatched by <c>$A6:F00D</c>.
/// </summary>
public enum CeresSteamFunction : ushort
{
    /// <summary>The shared return at <c>$A6:EFF4</c>; no graphical offset is required.</summary>
    None = 0xeff4,

    /// <summary>Applies the rotating-elevator graphical transform at <c>$A6:F019</c>.</summary>
    ApplyRotatingElevatorOffset = 0xf019,
}

/// <summary>Compiled fixed initialization records for the Ceres steam actor.</summary>
public static class CeresSteamDefinitions
{
    /// <summary>The Ceres steam enemy header at <c>$A0:E1FF</c>.</summary>
    public const ushort EnemyDefinition = 0xe1ff;

    /// <summary>
    /// The parallel instruction/function tables at <c>$A6:EFF5-$A6:F00C</c>, indexed
    /// by the six authored <see cref="CeresSteamVariant"/> values.
    /// The six instruction pointers at <c>$A6:EFF5-F000</c> exactly match
    /// <c>$F04D + $34 * direction</c> in the pinned NTSC J/U v1.0 ROM.
    /// Variants 0..3 use directions 0..3 (up, left, down, right); rotating
    /// variants 4 and 5 reuse directions 1 and 3. Native <c>$A6:EFE3-EFE8</c>
    /// indexes by <c>2 * parameter1</c>; production accepts only 0..5 and
    /// rejects 6. The constant stride describes four distinct instruction
    /// programs, while the last two entries are deliberate aliases.
    /// The separate six-word function table at <c>$A6:F001-F00C</c> follows
    /// <c>variant &lt; 4 ? $EFF4 : $F019</c> exactly in the pinned ROM: ordinary
    /// directions use the return function, while both rotating-room variants
    /// apply the graphical transform. The same bounded index selects both
    /// fields; the adjacent tables have independent value rules.
    /// </summary>
    private static readonly CeresSteamInitialization[] Initializations =
    [
        new(CeresSteamInstructionProgramDefinitions.Up, CeresSteamFunction.None),
        new(CeresSteamInstructionProgramDefinitions.Left, CeresSteamFunction.None),
        new(CeresSteamInstructionProgramDefinitions.Down, CeresSteamFunction.None),
        new(CeresSteamInstructionProgramDefinitions.Right, CeresSteamFunction.None),
        new(CeresSteamInstructionProgramDefinitions.Left,
            CeresSteamFunction.ApplyRotatingElevatorOffset),
        new(CeresSteamInstructionProgramDefinitions.Right,
            CeresSteamFunction.ApplyRotatingElevatorOffset),
    ];

    /// <summary>Returns the cartridge-authored initialization for one steam variant.</summary>
    internal static CeresSteamInitialization Initialization(CeresSteamVariant variant)
    {
        int index = (int)variant;
        if ((uint)index >= Initializations.Length)
        {
            throw new InvalidDataException(
                $"Ceres steam variant ${index:X4} exceeds its six-entry tables.");
        }

        return Initializations[index];
    }
}

/// <summary>One compiled Ceres steam instruction/function selection.</summary>
internal readonly record struct CeresSteamInitialization(
    ushort InstructionList,
    CeresSteamFunction Function);
