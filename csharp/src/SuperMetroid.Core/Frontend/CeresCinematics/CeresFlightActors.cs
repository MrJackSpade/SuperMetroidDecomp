using static SuperMetroid.Core.Frontend.CeresCinematics.CeresFlightActorDefinitions;

namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>Selects each rear-view actor of Samus's flight toward Ceres (<see cref="CeresFlightActorDefinitions"/>) by role.</summary>
internal static class CeresFlightActors
{
    /// <summary>Selects the definition of the rear-view actor that shows <paramref name="role"/>.</summary>
    /// <remarks>#1165 independently verifies each spawn identity, definition, start and motion.
    /// Each actor type chooses its own: large debris, station, small debris, vortex and rear stars select their own
    /// motion and placement. Each actor's start is authored, not a sample of a numerical curve.
    /// The animations have separate existing authored-data proofs shared across both Ceres scenes.</remarks>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="role"/> is not a rear-view role.</exception>
    public static CeresFlightActorDefinition RearViewActor(CeresRearViewActorRole role) => role switch
    {
        CeresRearViewActorRole.LargeAsteroids => LargeAsteroids,
        CeresRearViewActorRole.StationUnderAttack => StationUnderAttack,
        CeresRearViewActorRole.SmallAsteroids => SmallAsteroids,
        CeresRearViewActorRole.Vortex => MovingVortex,
        CeresRearViewActorRole.RearStars => RearStars,
        _ => throw new ArgumentOutOfRangeException(nameof(role)),
    };
}
