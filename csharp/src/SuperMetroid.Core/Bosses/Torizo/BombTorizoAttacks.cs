using static SuperMetroid.Core.Bosses.Torizo.BombTorizoAttackDefinitions;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Places Bomb Torizo's low-health explosions from their cue's site and Torizo's facing.</summary>
internal static class BombTorizoAttacks
{
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
