using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>What one of Botwoon's body links and his tail cap show in one facing (#2557).</summary>
/// <param name="Ripples">The link's ripples, in the order it cycles them.</param>
/// <param name="Tail">The tail cap's frame.</param>
internal sealed partial record BotwoonBodyLook(IReadOnlyList<EnemyProjectileFrameId> Ripples, EnemyProjectileFrameId Tail);
