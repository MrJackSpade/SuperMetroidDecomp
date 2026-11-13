
namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>Compiled fixed initialization records for the Ceres steam actor.</summary>
public static class CeresSteamDefinitions
{
    /// <summary>The random selection the initializer draws for a vent's activation delay, before adding one: 32 delays.</summary>
    internal static readonly RandomSampleField ActivationDelay = new(0, 5);


    /// <summary>
    /// The parallel animation/function tables, indexed
    /// by the six authored <see cref="CeresSteamVariant"/> values.
    /// The six animations are one per direction.
    /// Variants 0..3 use directions 0..3 (up, left, down, right); rotating
    /// variants 4 and 5 reuse directions 1 and 3. The initializer
    /// indexes by <c>2 * parameter1</c>; production accepts only 0..5 and
    /// rejects 6. The constant stride describes four distinct animation
    /// programs, while the last two entries are deliberate aliases.
    /// The separate six-entry function table follows
    /// the rule <c>variant &lt; 4</c> exactly: ordinary
    /// directions use the return function, while both rotating-room variants
    /// apply the graphical transform. The same bounded index selects both
    /// fields; the adjacent tables have independent value rules.
    /// </summary>
    internal static readonly IReadOnlyDictionary<CeresSteamVariant, CeresSteamInitialization> Initializations =
        new Dictionary<CeresSteamVariant, CeresSteamInitialization>
        {
            [CeresSteamVariant.Up] = new(CeresSteamProgram.Up, CeresSteamFunction.None),
            [CeresSteamVariant.Left] = new(CeresSteamProgram.Left, CeresSteamFunction.None),
            [CeresSteamVariant.Down] = new(CeresSteamProgram.Down, CeresSteamFunction.None),
            [CeresSteamVariant.Right] = new(CeresSteamProgram.Right, CeresSteamFunction.None),
            [CeresSteamVariant.RotatingElevatorLeft] = new(CeresSteamProgram.Left,
                CeresSteamFunction.ApplyRotatingElevatorOffset),
            [CeresSteamVariant.RotatingElevatorRight] = new(CeresSteamProgram.Right,
                CeresSteamFunction.ApplyRotatingElevatorOffset),
        };
}
