namespace SuperMetroid.Core.Enemies;

/// <summary>Converts an enemy projectile's collision option to the native <c>$F380</c> word at the snapshot boundary.</summary>
internal static class EnemyProjectileCollisionOptionCodec
{
    /// <summary>The native word an option is stored as.</summary>
    internal static int ToNative(EnemyProjectileCollisionOption option) => option switch
    {
        EnemyProjectileCollisionOption.RunShotList => 0,
        EnemyProjectileCollisionOption.IndestructibleDud => 1,
        EnemyProjectileCollisionOption.NoCollision => 2,
        _ => throw new InvalidOperationException($"Undefined {nameof(EnemyProjectileCollisionOption)} {option}."),
    };
}
