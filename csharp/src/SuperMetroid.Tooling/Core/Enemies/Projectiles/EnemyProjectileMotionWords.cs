using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// The bank-$86 routine addresses that stand for each enemy-projectile pre-instruction, for tools and verification that
/// read or emit native words: each stands for the motion of its projectile's behaviour that replaced it (#1843), resolved
/// through the behaviour bound to the projectile (#2571), or for none, holding still. Every empty routine, the pre-Phantoon room projectile's (whose background pin the room's
/// background-scroll rule performs, not the projectile), and the word zero of a freed slot, stand for holding still. A
/// routine of a kind that moves one way stands for <see cref="SingleMotion.Moving"/>.
/// </summary>
internal static class EnemyProjectileMotionWords
{
    /// <summary>The routine that starts the Tourian statues' base decoration following the reveal.</summary>
    internal const ushort TourianStatueRevealFollowing = 0xba37;

    /// <summary>
    /// The routine that flies a Space Pirate's or Mother Brain's laser each way (#2278): both stand for
    /// <see cref="SingleMotion.Moving"/>, and the laser's state keeps the way it flies.
    /// </summary>
    internal static readonly IReadOnlyDictionary<HorizontalDirection, ushort> PirateLaserFlights = new Dictionary<HorizontalDirection, ushort>
    {
        [HorizontalDirection.Left] = 0xa05c,
        [HorizontalDirection.Right] = 0xa07a,
    };

    /// <summary>
    /// The routine that flies a Ninja Space Pirate's claw thrown each way (#2278): both stand for
    /// <see cref="SingleMotion.Moving"/>, and the claw's state keeps the way it was thrown.
    /// </summary>
    internal static readonly IReadOnlyDictionary<HorizontalDirection, ushort> PirateClawThrows = new Dictionary<HorizontalDirection, ushort>
    {
        [HorizontalDirection.Left] = 0xa0d1,
        [HorizontalDirection.Right] = 0xa124,
    };
}
