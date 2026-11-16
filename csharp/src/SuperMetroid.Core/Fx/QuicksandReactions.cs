namespace SuperMetroid.Core.Fx;

/// <summary>
/// Maridia quicksand's reactions (<see cref="QuicksandDefinitions"/>): which special-air reactions place a sand reaction
/// object, and the surface physics Gravity equipment selects.
/// </summary>
public static class QuicksandReactions
{
    /// <summary>
    /// The surface's sinking while Samus moves and while she stands still, and the upward speed limit, by suit: the
    /// Gravity Suit selects one of two cases. She sinks 2 pixels per frame while moving in either suit, 1.125 pixels
    /// (1 with the Gravity Suit) while standing, and rises at most 2.5 pixels per frame (3.5 with the Gravity Suit). Each
    /// is held in 1/65536 pixels, the precision her movement adds them at.
    /// </summary>
    public static QuicksandSurfacePhysics SurfacePhysics(bool gravitySuit) =>
        gravitySuit ? QuicksandDefinitions.WithGravitySuit : QuicksandDefinitions.WithoutGravitySuit;

    /// <summary>Whether <paramref name="reaction"/> is one of the four inside sand reactions, each of which places a sand reaction object.</summary>
    internal static bool PlacesObject(SpecialAirInsideReaction reaction) => QuicksandDefinitions.ObjectPlacingInsideReactions.Contains(reaction);

    /// <summary>Whether <paramref name="reaction"/> is one of the four collision sand reactions, each of which places a sand reaction object.</summary>
    internal static bool PlacesObject(SpecialAirCollisionReaction reaction) => QuicksandDefinitions.ObjectPlacingCollisionReactions.Contains(reaction);
}
