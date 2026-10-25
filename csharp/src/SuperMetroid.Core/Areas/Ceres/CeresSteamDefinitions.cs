namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>Compiled fixed initialization records for the Ceres steam actor.</summary>
public static class CeresSteamDefinitions
{
    /// <summary>The live random bits $A6:EFD9 keeps for a vent's activation delay, before adding one.</summary>
    internal const ushort ActivationDelayMask = 0x001f;


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
    internal static CeresSteamInitialization Initialization(CeresSteamVariant variant) => variant switch
    {
        CeresSteamVariant.Up => new(CeresSteamProgram.Up, CeresSteamFunction.None),
        CeresSteamVariant.Left => new(CeresSteamProgram.Left, CeresSteamFunction.None),
        CeresSteamVariant.Down => new(CeresSteamProgram.Down, CeresSteamFunction.None),
        CeresSteamVariant.Right => new(CeresSteamProgram.Right, CeresSteamFunction.None),
        CeresSteamVariant.RotatingElevatorLeft => new(CeresSteamProgram.Left,
            CeresSteamFunction.ApplyRotatingElevatorOffset),
        CeresSteamVariant.RotatingElevatorRight => new(CeresSteamProgram.Right,
            CeresSteamFunction.ApplyRotatingElevatorOffset),
        _ => throw new InvalidDataException(
            $"Ceres steam variant {variant} exceeds its six-entry tables."),
    };
}
