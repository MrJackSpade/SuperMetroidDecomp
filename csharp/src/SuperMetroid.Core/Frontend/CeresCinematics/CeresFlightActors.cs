using static SuperMetroid.Core.Frontend.CeresCinematics.CeresFlightActorDefinitions;

namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>
/// Selects the five rear-view actors of Samus's flight toward Ceres (<see cref="CeresFlightActorDefinitions"/>) in spawn
/// order, with their installed layout ids.
/// </summary>
internal static class CeresFlightActors
{
    /// <summary>The installed layout id of each of the five rear-view actors, in spawn order.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The index is outside the five actors.</exception>
    public static string RearViewPlacementId(int index) => index switch
    {
        0 => "large-asteroid",
        1 => "station-under-attack",
        2 => "small-asteroid",
        3 => "vortex",
        4 => "rear-stars",
        _ => throw new ArgumentOutOfRangeException(nameof(index)),
    };

    /// <summary>Selects the five named actors spawned for the flight.</summary>
    /// <remarks>#1165 independently verifies each spawn identity, definition triple,
    /// initializer branch and motion. This is actor-type dispatch: large debris,
    /// station, small debris, vortex and rear stars select their own behavior and
    /// placement. The selected initializer parameters are not samples of a numerical curve.
    /// Named cases replace the earlier blanket retention claims; no output table or cache
    /// is retained. Inputs outside 0..4 still throw. The animations have separate
    /// existing authored-data proofs shared across both Ceres scenes.</remarks>
    public static CeresFlightActorDefinition RearViewActor(int index) => index switch
    {
        0 => LargeAsteroids,
        1 => StationUnderAttack,
        2 => SmallAsteroids,
        3 => MovingVortex,
        4 => RearStars,
        _ => throw new ArgumentOutOfRangeException(nameof(index)),
    };
}
