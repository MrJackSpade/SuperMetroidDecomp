namespace SuperMetroid.Core.Enemies.Projectiles.Artwork;

/// <summary>
/// The projectile frames that are the room's own pictures (#2260): a projectile showing one draws it from the room's shared
/// sheet in its standard colours, whoever spawned it and whatever shading its spawner handed it. They are the bursts a
/// destroyed projectile shows in place of itself: a shot Kago bug's and Kraid's shot spat rock's spark and flare, the
/// onion ring's impact fire, and a wall turret's bullet's smoke. The import installs each on the room's shared sheet.
/// </summary>
internal static class EnemyProjectileRoomPictureDefinitions
{
    /// <summary>The frames drawn from the room's shared sheet in their standard colours.</summary>
    internal static readonly HashSet<EnemyProjectileFrameId> RoomPictures =
    [
        EnemyProjectileFrameId.KagoBugSpark,
        EnemyProjectileFrameId.KagoBugFlareBall,
        EnemyProjectileFrameId.KagoBugFlareRingCored,
        EnemyProjectileFrameId.KagoBugFlareRing,
        EnemyProjectileFrameId.KagoBugFlareRingFading,
        EnemyProjectileFrameId.KraidRockSpark,
        EnemyProjectileFrameId.KraidRockFlareBall,
        EnemyProjectileFrameId.KraidRockFlareRingCored,
        EnemyProjectileFrameId.KraidRockFlareRing,
        EnemyProjectileFrameId.KraidRockFlareRingFading,
        EnemyProjectileFrameId.MotherBrainBlueRingFireballSmall,
        EnemyProjectileFrameId.MotherBrainBlueRingFireballLarge,
        EnemyProjectileFrameId.MotherBrainBlueRingFlamingRing,
        EnemyProjectileFrameId.MotherBrainBlueRingFireballHuge,
        EnemyProjectileFrameId.MotherBrainBlueRingWideRing,
        EnemyProjectileFrameId.MotherBrainBlueRingWideRingFading,
        EnemyProjectileFrameId.MotherBrainTurretBulletFireCloud,
        EnemyProjectileFrameId.MotherBrainTurretBulletFireCloudSpreading,
        EnemyProjectileFrameId.MotherBrainTurretBulletFireCloudBreaking,
        EnemyProjectileFrameId.MotherBrainTurretBulletFireCloudEmbers,
        EnemyProjectileFrameId.MotherBrainTurretBulletSpeck,
    ];
}
