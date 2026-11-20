using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Verification's native view of <see cref="RoomEnemyProjectileKind"/> (#1452): the bank-$86 definition word naming each
/// member, which a native snapshot's <c>eproj_id</c> holds. Core's values are arbitrary; a native snapshot, a ROM oracle or
/// a synthetic cartridge fixture translates through this table. Zero names the empty slot.
/// </summary>
internal static class EnemyProjectileNativeWords
{
    /// <summary>
    /// The enemy-projectile kind each bank-$86 definition word names. Several identical definitions that differ only in the
    /// way their projectile flies or the look its animation shows name one kind (#2278), its state or velocity holding that
    /// way.
    /// </summary>
    internal static readonly NativeWordIdentities<RoomEnemyProjectileKind> Kinds = new(new Dictionary<ushort, RoomEnemyProjectileKind>
    {
        [0xa3b0] = RoomEnemyProjectileKind.PrePhantoonRoomHeldPlace,
        [0xec95] = RoomEnemyProjectileKind.YappingMawBody,
        [0x8bc2] = RoomEnemyProjectileKind.SkreeMetareeParticle,
        [0x8bd0] = RoomEnemyProjectileKind.SkreeMetareeParticle,
        [0x8bde] = RoomEnemyProjectileKind.SkreeMetareeParticle,
        [0x8bec] = RoomEnemyProjectileKind.SkreeMetareeParticle,
        [0x8bfa] = RoomEnemyProjectileKind.SkreeMetareeParticle,
        [0x8c08] = RoomEnemyProjectileKind.SkreeMetareeParticle,
        [0x8c16] = RoomEnemyProjectileKind.SkreeMetareeParticle,
        [0x8c24] = RoomEnemyProjectileKind.SkreeMetareeParticle,
        [0x8f8f] = RoomEnemyProjectileKind.CrocomireProjectile,
        [0x8f9d] = RoomEnemyProjectileKind.CrocomireBridgeCrumbling,
        [0x90c1] = RoomEnemyProjectileKind.CrocomireSpikeWallPieces,
        [0x9c45] = RoomEnemyProjectileKind.KraidSpitRock,
        [0x9c53] = RoomEnemyProjectileKind.KraidCeilingRock,
        [0x9c61] = RoomEnemyProjectileKind.KraidRisingRock,
        [0x9c6f] = RoomEnemyProjectileKind.KraidRisingRock,
        [0x9c29] = RoomEnemyProjectileKind.PhantoonDestroyableFlame,
        [0x9c37] = RoomEnemyProjectileKind.PhantoonStartingFlame,
        [0x8e50] = RoomEnemyProjectileKind.DraygonGoop,
        [0x8e5e] = RoomEnemyProjectileKind.DraygonWallTurret,
        [0xc17e] = RoomEnemyProjectileKind.MotherBrainRoomTurret,
        [0xc18c] = RoomEnemyProjectileKind.MotherBrainRoomTurretBullet,
        [0xcefc] = RoomEnemyProjectileKind.MotherBrainGlassShard,
        [0xcf0a] = RoomEnemyProjectileKind.MotherBrainGlassSparkle,
        [0xcb4b] = RoomEnemyProjectileKind.MotherBrainOnionRing,
        [0xb4b1] = RoomEnemyProjectileKind.OldTourianEscapeShaftFakeWallExplosion,
        [0xcb59] = RoomEnemyProjectileKind.MotherBrainBomb,
        [0xcb67] = RoomEnemyProjectileKind.MotherBrainHandBeamCharging,
        [0xcb75] = RoomEnemyProjectileKind.MotherBrainHandBeamFired,
        [0xcb83] = RoomEnemyProjectileKind.MotherBrainRainbowBeamCharging,
        [0xcb2f] = RoomEnemyProjectileKind.MotherBrainPurpleBreathBig,
        [0xcb3d] = RoomEnemyProjectileKind.MotherBrainPurpleBreathSmall,
        [0xcb91] = RoomEnemyProjectileKind.MotherBrainDrool,
        [0xcb9f] = RoomEnemyProjectileKind.MotherBrainDyingDrool,
        [0xcbad] = RoomEnemyProjectileKind.MotherBrainRainbowBeamExplosion,
        [0xcb13] = RoomEnemyProjectileKind.MotherBrainDeathExplosion,
        [0xcb21] = RoomEnemyProjectileKind.MotherBrainEscapeDoorFragment,
        [0xcbbb] = RoomEnemyProjectileKind.MotherBrainEscapeSubtitle,
        [0xcc5b] = RoomEnemyProjectileKind.MotherBrainTopRightTube,
        [0xcc69] = RoomEnemyProjectileKind.MotherBrainTopLeftTube,
        [0xcc77] = RoomEnemyProjectileKind.MotherBrainTopMiddleLeftTube,
        [0xcc85] = RoomEnemyProjectileKind.MotherBrainTopMiddleRightTube,
        [0x9642] = RoomEnemyProjectileKind.RidleyFireball,
        [0x9650] = RoomEnemyProjectileKind.RidleyAfterburnCenter,
        [0x965e] = RoomEnemyProjectileKind.RidleyAfterburnCenter,
        [0x966c] = RoomEnemyProjectileKind.RidleyHorizontalAfterburn,
        [0x967a] = RoomEnemyProjectileKind.RidleyHorizontalAfterburn,
        [0x9688] = RoomEnemyProjectileKind.RidleyVerticalAfterburn,
        [0x9696] = RoomEnemyProjectileKind.RidleyVerticalAfterburn,
        [0x9734] = RoomEnemyProjectileKind.CeresFallingDebris,
        [0x9742] = RoomEnemyProjectileKind.CeresFallingDebris,
        [0xa379] = RoomEnemyProjectileKind.GunshipLiftoffDustCloud,
        [0x9e90] = RoomEnemyProjectileKind.AlcoonFireball,
        [0xd298] = RoomEnemyProjectileKind.PowampSpike,
        [0xd2a6] = RoomEnemyProjectileKind.WorkRobotLaserDiagonal,
        [0xd2b4] = RoomEnemyProjectileKind.WorkRobotLaserHorizontal,
        [0xd2c2] = RoomEnemyProjectileKind.WorkRobotLaserDiagonal,
        [0xd2d0] = RoomEnemyProjectileKind.WorkRobotLaserDiagonal,
        [0xd2de] = RoomEnemyProjectileKind.WorkRobotLaserDiagonal,
        [0xf498] = RoomEnemyProjectileKind.FallingSpark,
        [0xbbc7] = RoomEnemyProjectileKind.NuclearWaffleBody,
        [0x9db0] = RoomEnemyProjectileKind.FakeKraidSpit,
        [0x9dbe] = RoomEnemyProjectileKind.FakeKraidSpike,
        [0x9dcc] = RoomEnemyProjectileKind.FakeKraidSpike,
        [0xcf18] = RoomEnemyProjectileKind.KiHunterAcidSpit,
        [0xcf26] = RoomEnemyProjectileKind.KiHunterAcidSpit,
        [0xa17b] = RoomEnemyProjectileKind.PirateMotherBrainLaser,
        [0xa189] = RoomEnemyProjectileKind.PirateClaw,
        [0xbd5a] = RoomEnemyProjectileKind.PolypRock,
        [0xdafe] = RoomEnemyProjectileKind.CacatacSpike,
        [0xdfbc] = RoomEnemyProjectileKind.NamiheFireball,
        [0xdfca] = RoomEnemyProjectileKind.FuneFireball,
        [0xe0e0] = RoomEnemyProjectileKind.LavaThrownByMagdollite,
        [0xb5cb] = RoomEnemyProjectileKind.DragonFireball,
        [0xb743] = RoomEnemyProjectileKind.EyeDoorProjectile,
        [0xb751] = RoomEnemyProjectileKind.EyeDoorSweat,
        [0xe517] = RoomEnemyProjectileKind.EyeDoorSmoke,
        [0xe509] = RoomEnemyProjectileKind.DustOrExplosion,
        [0xf337] = RoomEnemyProjectileKind.EnemyDeathPickup,
        [0xf345] = RoomEnemyProjectileKind.EnemyDeathExplosion,
        [0xd02e] = RoomEnemyProjectileKind.KagoBug,
        [0xeba0] = RoomEnemyProjectileKind.BotwoonBody,
        [0xec48] = RoomEnemyProjectileKind.BotwoonSpit,
        [0xa95b] = RoomEnemyProjectileKind.BombTorizoLowHealthDrool,
        [0xa969] = RoomEnemyProjectileKind.TorizoInitialDrool,
        [0xa985] = RoomEnemyProjectileKind.BombTorizoExplosiveSwipe,
        [0xa9a1] = RoomEnemyProjectileKind.BombTorizoLowHealthExplosion,
        [0xa9af] = RoomEnemyProjectileKind.TorizoDeathExplosion,
        [0xa993] = RoomEnemyProjectileKind.BombTorizoStatueBreaking,
        [0xad5e] = RoomEnemyProjectileKind.BombTorizoChozoOrb,
        [0xaea8] = RoomEnemyProjectileKind.BombTorizoSonicBoom,
        [0xad7a] = RoomEnemyProjectileKind.GoldenTorizoChozoOrb,
        [0xaeb6] = RoomEnemyProjectileKind.GoldenTorizoSonicBoom,
        [0xafe5] = RoomEnemyProjectileKind.TorizoLandingDust,
        [0xaff3] = RoomEnemyProjectileKind.TorizoLandingDust,
        [0xb1c0] = RoomEnemyProjectileKind.GoldenTorizoEgg,
        [0xb31a] = RoomEnemyProjectileKind.GoldenTorizoSuperMissile,
        [0xb428] = RoomEnemyProjectileKind.GoldenTorizoEyeBeam,
        [0xba5c] = RoomEnemyProjectileKind.TourianStatueSplash,
        [0xba6a] = RoomEnemyProjectileKind.TourianStatueEyeGlow,
        [0xba78] = RoomEnemyProjectileKind.TourianStatueParticle,
        [0xba86] = RoomEnemyProjectileKind.TourianStatueTail,
        [0xba94] = RoomEnemyProjectileKind.TourianStatueSoul,
        [0xbaa2] = RoomEnemyProjectileKind.TourianStatueRidley,
        [0xbab0] = RoomEnemyProjectileKind.TourianStatuePhantoon,
        [0xbabe] = RoomEnemyProjectileKind.TourianStatueBaseDecoration,
        [0xaf68] = RoomEnemyProjectileKind.WreckedShipChozoSpikeFootstep,
        [0xaf84] = RoomEnemyProjectileKind.TourianStatueDescentDust,
        [0xde6c] = RoomEnemyProjectileKind.SporeSpawnStalk,
        [0xde7a] = RoomEnemyProjectileKind.SporeSpawnSpore,
        [0xde88] = RoomEnemyProjectileKind.SporeSpawnSpawner,
        [0xe6d2] = RoomEnemyProjectileKind.SaveStationElectricity,
        [0xe64b] = RoomEnemyProjectileKind.DownwardGateMoving,
        [0xe659] = RoomEnemyProjectileKind.DownwardGateClosed,
        [0xd904] = RoomEnemyProjectileKind.NoobTubeCrack,
        [0xd912] = RoomEnemyProjectileKind.NoobTubeShard,
        [0xd920] = RoomEnemyProjectileKind.NoobTubeReleasedAirBubble,
    });

    /// <summary>
    /// The definition word a projectile of <paramref name="kind"/> is compared by beside a native slot holding
    /// <paramref name="native"/>: the kind's one word, or, for a kind several identical definitions name, the native word
    /// itself when it is one of them, so a slot compares equal when it holds any definition of the kind (#2278).
    /// </summary>
    internal static ushort IdentityWord(RoomEnemyProjectileKind kind, int native)
    {
        ushort[] words = Kinds.WordsByIdentity[kind];
        return words.Contains(checked((ushort)native)) ? checked((ushort)native) : words[0];
    }
}
