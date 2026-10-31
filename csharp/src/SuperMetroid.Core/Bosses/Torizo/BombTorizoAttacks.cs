using static SuperMetroid.Core.Bosses.Torizo.BombTorizoAttackDefinitions;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Places Bomb Torizo's swipes and low-health explosions from their cue's placement and facing.</summary>
internal static class BombTorizoAttacks
{
    /// <summary>The low-health explosion site of the gut.</summary>
    internal const int GutSite = 0;

    /// <summary>The low-health explosion site of the face.</summary>
    internal const int FaceSite = 1;

    /// <summary>The left-facing swipe placement a swipe cue selects.</summary>
    /// <exception cref="InvalidDataException"><paramref name="placement"/> is not one of the swipe placements.</exception>
    internal static Offset16<RoomPixels> Swipe(int placement) =>
        (uint)placement < (uint)SwipePlacements.Length
            ? SwipePlacements[placement]
            : throw new InvalidDataException(
                $"Bomb Torizo swipe placement {placement} is outside its {SwipePlacements.Length} placements.");

    /// <summary>The placement of a low-health explosion at the gut or the face, for the facing Torizo has.</summary>
    /// <param name="site"><see cref="GutSite"/> or <see cref="FaceSite"/>.</param>
    /// <param name="facingRight">Whether Torizo faces right.</param>
    /// <exception cref="InvalidDataException"><paramref name="site"/> is neither site.</exception>
    internal static Offset16<RoomPixels> LowHealthExplosion(int site, bool facingRight)
    {
        if (site is not (GutSite or FaceSite))
            throw new InvalidDataException($"Bomb Torizo low-health explosion site {site} is neither the gut nor the face.");
        return ExplosionPlacements[site * 2 + (facingRight ? 0 : 1)];
    }
}
