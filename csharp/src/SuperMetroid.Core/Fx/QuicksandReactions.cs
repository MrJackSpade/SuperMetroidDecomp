using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// Maridia quicksand's reactions (<see cref="QuicksandDefinitions"/>): the setup each sand header selects, and the
/// surface physics Gravity equipment selects.
/// </summary>
public static class QuicksandReactions
{
    /// <summary>
    /// `$84:B48B-$84:B495`, suit-indexed moving displacement, stationary
    /// displacement, and upward-speed-limit values used by the surface reaction.
    /// Gravity equipment selects one of two named scalar cases. Moving displacement is
    /// 2 pixels per frame for either suit, stationary displacement is 1.125/1, and the
    /// NTSC upward cap is 2.5/3.5 pixels per frame without/with Gravity. The cartridge
    /// stores them as unsigned 8.8 words; they are held here as the 16.16 values its
    /// consumers widen them to.
    /// </summary>
    public static QuicksandSurfacePhysics SurfacePhysics(bool gravitySuit) =>
        gravitySuit ? QuicksandDefinitions.WithGravitySuit : QuicksandDefinitions.WithoutGravitySuit;

    /// <summary>
    /// The definition of one of the eight supported sand headers, which select the shared delete list; any other header,
    /// including the unused surface clones between the named headers, returns false.
    /// </summary>
    internal static bool TryGetReaction(PlmHeaderId header, out QuicksandReactionDefinition definition)
    {
        if (!QuicksandDefinitions.Reactions.TryGetValue(header, out SpecialAirReactionSetup setup))
        {
            definition = default;
            return false;
        }
        definition = new(setup, PlmProgramStart.Delete);
        return true;
    }

    /// <summary>Resolves one supported sand header or rejects an invalid allocator request.</summary>
    internal static QuicksandReactionDefinition ResolveReaction(PlmHeaderId header) =>
        TryGetReaction(header, out QuicksandReactionDefinition definition)
            ? definition
            : throw new ArgumentOutOfRangeException(
                nameof(header),
                header,
                "Quicksand reaction header must be one of the eight bank-$84 sand entries.");
}
