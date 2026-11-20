namespace SuperMetroid.Core.Fx;

/// <summary>
/// Maridia quicksand's reactions (<see cref="QuicksandDefinitions"/>): which special-air reactions place a sand reaction
/// object, and the surface physics Gravity equipment selects.
/// </summary>
public static class QuicksandReactions
{
    /// <summary>
    /// The surface's sinking while Samus moves and while she stands still, and the upward speed limit, by suit: the
    /// Gravity Suit selects one of two cases (<see cref="QuicksandDefinitions"/>): she sinks as fast while moving in either
    /// suit, a little slower while standing with the Gravity Suit, and rises faster with it.
    /// </summary>
    public static QuicksandSurfacePhysics SurfacePhysics(bool gravitySuit) =>
        gravitySuit ? QuicksandDefinitions.WithGravitySuit : QuicksandDefinitions.WithoutGravitySuit;

    /// <summary>Whether <paramref name="reaction"/> is one of the inside sand reactions, each of which places a sand reaction object.</summary>
    internal static bool PlacesObject(SpecialAirInsideReaction reaction) => Ensure.NotNull(reaction).PlacesSandReaction;

    /// <summary>Whether <paramref name="reaction"/> is one of the collision sand reactions, each of which places a sand reaction object.</summary>
    internal static bool PlacesObject(SpecialAirCollisionReaction reaction) => Ensure.NotNull(reaction).PlacesSandReaction;
}
