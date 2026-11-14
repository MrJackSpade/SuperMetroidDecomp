
namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>Compiled fixed initialization values for the Ceres steam actor.</summary>
public static class CeresSteamDefinitions
{
    /// <summary>The random selection the initializer draws for a vent's activation delay, before adding one: 32 delays.</summary>
    internal static readonly RandomSampleField ActivationDelay = new(0, 5);


    /// <summary>
    /// Each vent's animation and action, keyed
    /// by the six authored <see cref="CeresSteamVariant"/> values.
    /// The six animations are one per direction.
    /// Variants 0..3 use directions 0..3 (up, left, down, right); rotating
    /// variants 4 and 5 reuse directions 1 and 3. The initializer
    /// indexes them by variant; production accepts only the six variants and
    /// rejects a seventh. The constant stride describes four distinct
    /// animations, while the last two entries are deliberate aliases.
    /// The actions follow
    /// the rule <c>variant &lt; 4</c> exactly: ordinary
    /// directions take no action, while both rotating-room variants
    /// apply the graphical transform. The same bounded index selects both
    /// fields; the adjacent tables have independent value rules.
    /// </summary>
    internal static readonly IReadOnlyDictionary<CeresSteamVariant, CeresSteamInitialization> Initializations =
        new Dictionary<CeresSteamVariant, CeresSteamInitialization>
        {
            [CeresSteamVariant.Up] = new(CeresSteamSegment.UpDormant, null),
            [CeresSteamVariant.Left] = new(CeresSteamSegment.LeftDormant, null),
            [CeresSteamVariant.Down] = new(CeresSteamSegment.DownDormant, null),
            [CeresSteamVariant.Right] = new(CeresSteamSegment.RightDormant, null),
            [CeresSteamVariant.RotatingElevatorLeft] = new(CeresSteamSegment.LeftDormant,
                CeresSteamAction.ApplyRotatingElevatorOffset),
            [CeresSteamVariant.RotatingElevatorRight] = new(CeresSteamSegment.RightDormant,
                CeresSteamAction.ApplyRotatingElevatorOffset),
        };
}
