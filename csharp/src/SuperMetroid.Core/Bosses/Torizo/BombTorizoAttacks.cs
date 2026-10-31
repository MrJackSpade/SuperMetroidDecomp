using static SuperMetroid.Core.Bosses.Torizo.BombTorizoAttackDefinitions;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Places Bomb Torizo's swipes and low-health explosions from their cue's placement and facing.</summary>
internal static class BombTorizoAttacks
{
    /// <summary>The left-facing swipe placement a swipe cue selects.</summary>
    /// <exception cref="InvalidDataException"><paramref name="placement"/> is not one of the swipe placements.</exception>
    internal static Offset16<RoomPixels> Swipe(int placement) =>
        (uint)placement < (uint)SwipePlacements.Length
            ? SwipePlacements[placement]
            : throw new InvalidDataException(
                $"Bomb Torizo swipe placement {placement} is outside its {SwipePlacements.Length} placements.");

    /// <summary>The placement of a low-health explosion at the gut or the face, for the facing Torizo has.</summary>
    /// <param name="site">Where the explosion bursts.</param>
    /// <param name="facingRight">Whether Torizo faces right.</param>
    /// <exception cref="InvalidDataException"><paramref name="site"/> is neither site.</exception>
    internal static Offset16<RoomPixels> LowHealthExplosion(BombTorizoExplosionSite site, bool facingRight) => site switch
    {
        BombTorizoExplosionSite.Gut => ExplosionPlacements[facingRight ? 0 : 1],
        BombTorizoExplosionSite.Face => ExplosionPlacements[facingRight ? 2 : 3],
        _ => throw new InvalidDataException($"Bomb Torizo low-health explosion site {site} is neither the gut nor the face."),
    };
}
