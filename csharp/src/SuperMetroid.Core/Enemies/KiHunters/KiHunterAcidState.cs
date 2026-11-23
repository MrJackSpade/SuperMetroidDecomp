using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>What Ki-Hunter acid keeps (#2338): the way its Ki-Hunter spat it, which its launch shifts it.</summary>
/// <param name="spat">The way the acid was spat.</param>
internal sealed partial class KiHunterAcidState(HorizontalDirection spat) : EnemyProjectileState
{
    /// <summary>The way the acid was spat, which its Ki-Hunter faced: its launch shifts it that way.</summary>
    internal HorizontalDirection Spat { get; } = spat;
}
