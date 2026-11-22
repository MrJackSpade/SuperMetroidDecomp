using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// What a projectile of an afterburn chain keeps (#2245, the #1470 mechanism): Ridley's fireball, the centre its impact
/// leaves (or Mother Brain's bomb's explosion leaves) and the directional afterburns that centre's pair starts. It counts
/// the afterburns the chain still spawns after this one; each directional afterburn spawns its successor flying its own
/// way at its own speed and hands it one fewer, and one with none left spawns none.
/// </summary>
/// <param name="remaining">The afterburns the chain spawns after this one, zero or more.</param>
internal sealed partial class RidleyAfterburnChainState(int remaining) : EnemyProjectileState
{
    /// <summary>The afterburns the chain spawns after this one, zero or more.</summary>
    internal int Remaining
    {
        get;
        set => field = Ensure.AtLeastZero(value);
    } = Ensure.AtLeastZero(remaining);
}
