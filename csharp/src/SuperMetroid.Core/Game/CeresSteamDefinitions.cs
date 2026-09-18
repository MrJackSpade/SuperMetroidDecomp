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
    /// </summary>
    private static readonly CeresSteamInitialization[] Initializations =
    [
        new(0xf04d, CeresSteamFunction.None),
        new(0xf081, CeresSteamFunction.None),
        new(0xf0b5, CeresSteamFunction.None),
        new(0xf0e9, CeresSteamFunction.None),
        new(0xf081, CeresSteamFunction.ApplyRotatingElevatorOffset),
        new(0xf0e9, CeresSteamFunction.ApplyRotatingElevatorOffset),
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
