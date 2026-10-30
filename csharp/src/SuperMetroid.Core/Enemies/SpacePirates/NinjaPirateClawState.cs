using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>What a ninja pirate's thrown claw keeps: its speed, and whether it is still flying out or coming back.</summary>
/// <param name="speed">The claw's launch speed, in 1/256 pixel per update.</param>
internal sealed class NinjaPirateClawState(int speed) : EnemyProjectileState
{
    /// <summary>
    /// The claw's sideways speed in 1/256 pixel per update, 0 through 65535: it slows while the claw flies out and speeds
    /// up once it comes back; only its whole pixels move the claw.
    /// </summary>
    internal int Speed
    {
        get;
        set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    } = speed;

    /// <summary>Whether the claw is still flying out; it turns back once its outward speed has run out.</summary>
    internal bool Outbound { get; set; } = true;
}
