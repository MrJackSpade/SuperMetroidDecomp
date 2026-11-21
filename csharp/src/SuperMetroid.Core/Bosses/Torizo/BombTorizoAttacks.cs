using static SuperMetroid.Core.Bosses.Torizo.BombTorizoAttackDefinitions;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Places Bomb Torizo's low-health explosions from their cue's site and Torizo's facing.</summary>
internal static partial class BombTorizoAttacks
{
    /// <summary>The placement of a low-health explosion at the gut or the face, for the facing Torizo has.</summary>
    /// <param name="site">Where the explosion bursts.</param>
    /// <param name="facing">The way Torizo faces.</param>
    /// <exception cref="InvalidDataException"><paramref name="site"/> is neither site.</exception>
    internal static Offset16<RoomPixels> LowHealthExplosion(BombTorizoExplosionSite site, HorizontalDirection facing)
    {
        Offset16<RoomPixels> placement = site switch
        {
            BombTorizoExplosionSite.Gut => GutExplosion,
            BombTorizoExplosionSite.Face => FaceExplosion,
            _ => throw new InvalidDataException($"Bomb Torizo low-health explosion site {site} is neither the gut nor the face."),
        };
        return facing == HorizontalDirection.Right ? placement : placement.MirroredX();
    }
}
