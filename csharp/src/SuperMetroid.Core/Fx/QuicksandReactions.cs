namespace SuperMetroid.Core.Fx;

/// <summary>
/// Maridia quicksand's reactions (<see cref="QuicksandDefinitions"/>): which special-air reactions place a sand reaction
/// object, and the surface physics Gravity equipment selects.
/// </summary>
public static class QuicksandReactions
{
    /// <summary>
    /// The suit-indexed moving displacement, stationary
    /// displacement, and upward-speed-limit values used by the surface reaction.
    /// Gravity equipment selects one of two named scalar cases. Moving displacement is
    /// 2 pixels per frame for either suit, stationary displacement is 1.125/1, and the
    /// upward cap is 2.5/3.5 pixels per frame without/with Gravity. The game
    /// stores them as unsigned 1/256-pixel values; they are held here as the 1/65536-pixel values its
    /// consumers widen them to.
    /// </summary>
    public static QuicksandSurfacePhysics SurfacePhysics(bool gravitySuit) =>
        gravitySuit ? QuicksandDefinitions.WithGravitySuit : QuicksandDefinitions.WithoutGravitySuit;

    /// <summary>Whether <paramref name="reaction"/> is one of the four inside sand reactions, each of which places a sand reaction object.</summary>
    internal static bool PlacesObject(SpecialAirInsideReaction reaction) => QuicksandDefinitions.ObjectPlacingInsideReactions.Contains(reaction);

    /// <summary>Whether <paramref name="reaction"/> is one of the four collision sand reactions, each of which places a sand reaction object.</summary>
    internal static bool PlacesObject(SpecialAirCollisionReaction reaction) => QuicksandDefinitions.ObjectPlacingCollisionReactions.Contains(reaction);
}
