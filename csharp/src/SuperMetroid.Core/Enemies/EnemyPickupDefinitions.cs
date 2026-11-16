using SuperMetroid.Core.Enemies.Deaths;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// What enemy drops are: each pickup's animation, what it restores (#2156), how long it lasts and how near a grapple
/// collects it.
/// </summary>
internal static class EnemyPickupDefinitions
{
    /// <summary>
    /// Each pickup kind's drop animation. <see cref="EnemyPickupKind.NoDrop"/> has no entry; the dormant pickup path
    /// handles it.
    /// </summary>
    internal static readonly Dictionary<EnemyPickupKind, EnemyPickupAnimationDefinition> Animations = new()
    {
        [EnemyPickupKind.SmallEnergy] = new(EnemyDeathProjectileSegment.SmallEnergy),
        [EnemyPickupKind.BigEnergy] = new(EnemyDeathProjectileSegment.BigEnergy),
        [EnemyPickupKind.PowerBomb] = new(EnemyDeathProjectileSegment.PowerBombs),
        [EnemyPickupKind.Missile] = new(EnemyDeathProjectileSegment.Missiles),
        [EnemyPickupKind.SuperMissile] = new(EnemyDeathProjectileSegment.SuperMissiles),
    };

    /// <summary>How long a new pickup stays to be collected, in updates.</summary>
    internal const int Lifetime = 400;

    /// <summary>How long a new pickup ignores the grapple's end, in updates.</summary>
    internal const int GrappleDelay = 16;

    /// <summary>How close on each axis the grapple's end comes, in pixels and exclusive, to collect a pickup.</summary>
    internal const int GrappleReach = 16;

    /// <summary>The energy a small energy pickup restores.</summary>
    internal const int SmallEnergyAmount = 5;

    /// <summary>The energy a big energy pickup restores.</summary>
    internal const int BigEnergyAmount = 20;

    /// <summary>The Power Bombs a Power Bomb pickup restores.</summary>
    internal const int PowerBombAmount = 1;

    /// <summary>The missiles a missile pickup restores.</summary>
    internal const int MissileAmount = 2;

    /// <summary>The Super Missiles a Super Missile pickup restores.</summary>
    internal const int SuperMissileAmount = 1;

    /// <summary>The most missiles a missile pickup sets aside in reserve past Samus's capacity, however large her capacity.</summary>
    internal const int LargestMissileReserve = 99;
}
