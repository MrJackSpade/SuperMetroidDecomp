namespace SuperMetroid.Core.Game;

/// <summary>
/// One complete fourteen-byte <c>EprojDef</c> record from bank <c>$86</c>.
/// </summary>
/// <param name="InitializationCallback">Definition-specific initializer invoked after the common slot copy.</param>
/// <param name="PreInstruction">Per-frame callback installed before the first frame-list tick.</param>
/// <param name="InitialInstructionList">Initial mixed animation/behavior program.</param>
/// <param name="RadiusWord">Packed X radius in the low byte and Y radius in the high byte.</param>
/// <param name="Properties">Damage and collision/draw property bits.</param>
/// <param name="TouchInstructionList">Optional list installed when the actor touches Samus.</param>
/// <param name="ShotInstructionList">List installed when a destructible actor is shot.</param>
internal readonly record struct EnemyProjectileDefinition(
    ushort InitializationCallback,
    ushort PreInstruction,
    ushort InitialInstructionList,
    ushort RadiusWord,
    ushort Properties,
    ushort TouchInstructionList,
    ushort ShotInstructionList)
{
    internal byte XRadius => unchecked((byte)RadiusWord);
    internal byte YRadius => unchecked((byte)(RadiusWord >> 8));
    internal ushort Damage => unchecked((ushort)(Properties & 0x0fff));
    internal EnemyProjectileDrawPriority DrawPriority =>
        (Properties & 0x1000) != 0
            ? EnemyProjectileDrawPriority.High
            : EnemyProjectileDrawPriority.Low;
    internal bool CanDamageSamus => (Properties & 0x2000) == 0;
    internal bool PersistsOnSamusContact => (Properties & 0x4000) != 0;
    internal bool BlocksSamusProjectiles => (Properties & 0x8000) != 0;
}

/// <summary>
/// Compiled bank-$86 enemy-projectile definitions used by every translated projectile
/// family. Enum values retain each native <c>EprojDef</c> address for debugger parity.
/// </summary>
internal static class EnemyProjectileDefinitionCatalog
{
    /// <summary>Returns the complete native definition for one translated projectile identity.</summary>
    internal static EnemyProjectileDefinition Get(RoomEnemyProjectileKind kind) => kind switch
    {
        RoomEnemyProjectileKind.YappingMawBody => new(0xEC62, 0xEC94, 0xEC56, 0x0202, 0x2005, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.SkreeParticleDownRight => new(
            0x8ACD, 0x8B5D, SkreeMetareeParticleInstructionProgramDefinitions.Skree,
            0x0202, 0x0004, 0x0000, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.SkreeParticleUpRight => new(
            0x8AF1, 0x8B5D, SkreeMetareeParticleInstructionProgramDefinitions.Skree,
            0x0202, 0x0004, 0x0000, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.SkreeParticleDownLeft => new(
            0x8B15, 0x8B5D, SkreeMetareeParticleInstructionProgramDefinitions.Skree,
            0x0202, 0x0004, 0x0000, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.SkreeParticleUpLeft => new(
            0x8B39, 0x8B5D, SkreeMetareeParticleInstructionProgramDefinitions.Skree,
            0x0202, 0x0004, 0x0000, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.MetareeParticleDownRight => new(
            0x8ACD, 0x8B5D, SkreeMetareeParticleInstructionProgramDefinitions.Metaree,
            0x0202, 0x0004, 0x0000, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.MetareeParticleUpRight => new(
            0x8AF1, 0x8B5D, SkreeMetareeParticleInstructionProgramDefinitions.Metaree,
            0x0202, 0x0004, 0x0000, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.MetareeParticleDownLeft => new(
            0x8B15, 0x8B5D, SkreeMetareeParticleInstructionProgramDefinitions.Metaree,
            0x0202, 0x0004, 0x0000, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.MetareeParticleUpLeft => new(
            0x8B39, 0x8B5D, SkreeMetareeParticleInstructionProgramDefinitions.Metaree,
            0x0202, 0x0004, 0x0000, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.CrocomireProjectile => new(0x9023, 0x906B, 0x8FCF, 0x0808, 0x8014, 0x0000, 0x9007),
        RoomEnemyProjectileKind.CrocomireBridgeCrumbling => new(0x9286, 0x92BA, 0x8FEB, 0x0404, 0x8000, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.CrocomireSpikeWallPieces => new(0x90CF, 0x9115, 0x8FF3, 0x0000, 0x0000, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.KraidSpitRock => new(
            0x9CA3, 0x9D56,
            KraidRockProjectileInstructionProgramDefinitions.SharedRockAndKagoBug,
            0x0404, 0x8002, 0x0000,
            KraidRockProjectileInstructionProgramDefinitions.SpitRockShot),
        RoomEnemyProjectileKind.KraidCeilingRock => new(
            0x9CD8, 0x9D89,
            KraidRockProjectileInstructionProgramDefinitions.SharedRockAndKagoBug,
            0x0404, 0xA000, 0x0000,
            CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.KraidRisingRockLeft => new(
            0x9D0C, 0x9D56,
            KraidRockProjectileInstructionProgramDefinitions.SharedRockAndKagoBug,
            0x0404, 0xA000, 0x0000,
            CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.KraidRisingRockRight => new(
            0x9D0C, 0x9D56,
            KraidRockProjectileInstructionProgramDefinitions.RisingRockRight,
            0x0404, 0xA000, 0x0000,
            CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.PhantoonDestroyableFlame => new(0x9824, 0x9981, 0x975C, 0x1008, 0x8028, 0x0000, 0x97FA),
        RoomEnemyProjectileKind.PhantoonStartingFlame => new(0x993A, 0x9B29, 0x97E8, 0x1008, 0x4028, 0x0000, 0x975C),
        RoomEnemyProjectileKind.DraygonGoop => new(0x8D04, 0x8E0F, 0x8C3A, 0x0808, 0xD000, 0x8C38, 0x8C58),
        RoomEnemyProjectileKind.DraygonWallTurret => new(0x8D40, 0x8DFF, 0x8CA4, 0x0808, 0x1080, 0x0000, 0xE138),
        RoomEnemyProjectileKind.MotherBrainRoomTurret => new(0xBE4F, 0xBFDF, 0xC10D, 0x0000, 0x6000, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.MotherBrainRoomTurretBullet => new(0xBF59, 0xC0E0, 0xC131, 0x0303, 0x4014, 0xC19A, 0xC19A),
        RoomEnemyProjectileKind.MotherBrainGlassShard => new(0xCDC5, 0xCE9B, 0xCC93, 0x0000, 0x3000, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.MotherBrainGlassSparkle => new(0xCE6D, 0x84FB, 0xCDB3, 0x0000, 0x3000, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.MotherBrainOnionRing => new(0xC2F3, 0xC335, 0xC432, 0x0606, 0x3050, 0xC464, 0x84FC),
        RoomEnemyProjectileKind.MotherBrainBomb => new(0xC482, 0xC4C8, 0xC76E, 0x0606, 0x40A0, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.MotherBrainHandBeamCharging => new(0xC605, 0xC76D, 0xC796, 0x0606, 0x1190, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.MotherBrainHandBeamFired => new(0xC684, 0xC76D, 0xC796, 0x0606, 0x1190, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.MotherBrainRainbowBeamCharging => new(0xC80A, 0xC814, 0xC829, 0x0000, 0x7000, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.MotherBrainPurpleBreathBig => new(0xCA6A, 0xCAA3, 0xCAA4, 0x0000, 0x3000, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.MotherBrainDrool => new(0xC843, 0xC84D, 0xC8B0, 0x0000, 0x7000, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.MotherBrainDyingDrool => new(0xC843, 0xC84D, 0xC8B0, 0x0000, 0x7000, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.MotherBrainRainbowBeamExplosion => new(0xC92F, 0xC94C, 0xE152, 0x0101, 0x7000, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.MotherBrainTopRightTube => new(0xCBC9, 0xCBE7, 0xCC43, 0x1008, 0x5000, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.MotherBrainTopLeftTube => new(0xCBC9, 0xCBE7, 0xCC49, 0x1008, 0x5000, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.MotherBrainTopMiddleLeftTube => new(0xCBC9, 0xCBE7, 0xCC4F, 0x1808, 0x5000, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.MotherBrainTopMiddleRightTube => new(0xCBC9, 0xCBE7, 0xCC55, 0x1808, 0x5000, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.CeresRidleyFireball => new(0x93CA, 0x940E, 0x9552, 0x0606, 0x5003, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnCenter => new(0x947F, 0x950C, 0x95A0, 0x0606, 0x5003, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.CeresRidleyVerticalAfterburnCenter => new(0x947F, 0x950C, 0x95D3, 0x0606, 0x5003, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnRight => new(0x94C8, 0x950D, 0x9606, 0x0606, 0x5003, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnLeft => new(0x94DC, 0x950D, 0x9606, 0x0606, 0x5003, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.CeresRidleyVerticalAfterburnUp => new(0x94A0, 0x9522, 0x9606, 0x0606, 0x5003, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.CeresRidleyVerticalAfterburnDown => new(0x94B4, 0x9522, 0x9606, 0x0606, 0x5003, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.CeresFallingDebrisLight => new(0x96DC, 0x9701, 0x9750, 0x0808, 0x4000, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.CeresFallingDebrisDark => new(0x96DC, 0x9701, 0x9756, 0x0808, 0x4000, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.GunshipLiftoffDustCloud => new(0xA2A1, 0xA327, 0xA197, 0x0808, 0x3000, 0x0000, 0xA197),
        RoomEnemyProjectileKind.AlcoonFireball => new(
            0x9EB2, 0x9EFF, AlcoonFireballInstructionProgramDefinitions.Initial,
            0x0404, 0x0014, 0x0000, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.PowampSpike => new(
            0xD23A, 0xD263, PowampSpikeInstructionProgramDefinitions.Initial,
            0x0404, 0x0014, 0x0000, PowampSpikeInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.WorkRobotLaserUpLeft => new(
            0xD341, 0xD3BF, WorkRobotLaserInstructionProgramDefinitions.Initial,
            0x0C0C, 0x0004, 0x0000, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.WorkRobotLaserHorizontal => new(
            0xD32E, 0xD3BF, WorkRobotLaserInstructionProgramDefinitions.Initial,
            0x020F, 0x0014, 0x0000, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.WorkRobotLaserDownLeft => new(
            0xD30C, 0xD3BF, WorkRobotLaserInstructionProgramDefinitions.Initial,
            0x0C0C, 0x0004, 0x0000, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.WorkRobotLaserUpRight => new(
            0xD341, 0xD3BF, WorkRobotLaserInstructionProgramDefinitions.Initial,
            0x0C0C, 0x0004, 0x0000, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.WorkRobotLaserDownRight => new(
            0xD30C, 0xD3BF, WorkRobotLaserInstructionProgramDefinitions.Initial,
            0x0C0C, 0x0004, 0x0000, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.FallingSpark => new(
            0xF391,
            0xF3F0,
            FallingSparkInstructionProgramDefinitions.Falling,
            0x0404,
            0x0005,
            0x0000,
            0x84FC),
        RoomEnemyProjectileKind.NuclearWaffleBody => new(0xBB92, 0xBBC6, 0xBB5E, 0x0808, 0xC040, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.FakeKraidSpit => new(
            0x9DEC, 0x9E1E, FakeKraidProjectileInstructionProgramDefinitions.Spit,
            0x0404, 0x0014, 0x0000, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.FakeKraidSpikeLeft => new(
            0x9E46, 0x9E83, FakeKraidProjectileInstructionProgramDefinitions.SpikeLeft,
            0x0204, 0x0006, 0x0000, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.FakeKraidSpikeRight => new(
            0x9E4B, 0x9E83, FakeKraidProjectileInstructionProgramDefinitions.SpikeRight,
            0x0204, 0x0006, 0x0000, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.KiHunterAcidSpitLeft => new(0xCF90, 0xCFF7, 0xCF34, 0x0802, 0x0014, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.KiHunterAcidSpitRight => new(0xCFA6, 0xCFF7, 0xCF6E, 0x0802, 0x0014, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.PirateMotherBrainLaser => new(0xA009, 0xA05C, 0x9F41, 0x0410, 0x100A, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.PirateClaw => new(0xA098, 0xA05B, 0x0000, 0x0808, 0x1014, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.PolypRock => new(0xBBDB, 0xBC0F, 0xBBD5, 0x0202, 0x0010, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.CacatacSpike => new(0xD992, 0xD9DB, 0xD92E, 0x0202, 0x0005, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.StokeProjectile => new(0xDB18, 0xDB5B, 0xDB0C, 0x0202, 0x0005, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.NamiheFireball => new(
            0xDED6,
            0xDF39,
            FuneNamiheFireballInstructionProgramDefinitions.Left,
            0x0804,
            0x00C8,
            0x0000,
            0x84FC),
        RoomEnemyProjectileKind.FuneFireball => new(
            0xDED6,
            0xDF39,
            FuneNamiheFireballInstructionProgramDefinitions.Left,
            0x0804,
            0x003C,
            0x0000,
            0x84FC),
        RoomEnemyProjectileKind.LavaThrownByMagdollite => new(
            0xE000,
            0xE049,
            MagdolliteLavaInstructionProgramDefinitions.Left,
            0x0202,
            0x8028,
            0x0000,
            MagdolliteLavaInstructionProgramDefinitions.Shot),
        RoomEnemyProjectileKind.DragonFireball => new(
            0xB4EF,
            0xB535,
            DragonFireballInstructionProgramDefinitions.RisingLeft,
            0x0202,
            0x000A,
            0x0000,
            CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.EyeDoorProjectile => new(
            0xB62D,
            0x84FB,
            EyeDoorProjectileInstructionProgramDefinitions.Initial,
            0x0808,
            0x8004,
            0x0000,
            EyeDoorProjectileInstructionProgramDefinitions.Shot),
        RoomEnemyProjectileKind.EyeDoorSweat => new(
            0xB683,
            0xB714,
            EyeDoorSweatInstructionProgramDefinitions.Initial,
            0x0000,
            0x0004,
            0x0000,
            CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.EyeDoorSmoke => new(0xE4A6, 0xE508, 0xE138, 0x0000, 0x0000, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.MiscDustExplosion => new(0xE468, 0xE4FE, 0xE0EE, 0x0000, 0x1000, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.EnemyDeathPickup => new(0xEF29, 0xEFE0, 0xED8D, 0x1010, 0x3000, 0x84FC, 0x84FC),
        RoomEnemyProjectileKind.EnemyDeathExplosion => new(0xEF89, 0xEFDF, 0xED8D, 0x1010, 0x7000, 0x84FC, 0x84FC),
        RoomEnemyProjectileKind.KagoBug => new(
            0xD088, 0xD0EB,
            KraidRockProjectileInstructionProgramDefinitions.SharedRockAndKagoBug,
            0x0404, 0x0014, 0x0000, 0xD064),
        RoomEnemyProjectileKind.BotwoonBody => new(0xEA31, 0xEA80, 0xE8F3, 0x0202, 0xE080, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.BotwoonSpit => new(0xEBC6, 0xEC05, 0xEBAE, 0x0202, 0x1060, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.BombTorizoLowHealthDrool => new(0xA5D3, 0x84FB, 0xA472, 0x0201, 0x3000, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.BombTorizoInitialDrool => new(0xA65D, 0x84FB, 0xA472, 0x0201, 0x2000, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.BombTorizoExplosiveSwipe => new(0xA6F6, 0xA919, 0xA4AA, 0x1010, 0x500A, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.BombTorizoLowHealthExplosion => new(0xA81B, 0x84FB, 0xA3CB, 0x1004, 0x3000, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.BombTorizoDeathExplosion => new(0xA871, 0x84FB, 0xA3FA, 0x1004, 0x3000, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.BombTorizoStatueBreaking => new(0xA764, 0x84FB, 0xA54B, 0x0808, 0x3000, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.BombTorizoChozoOrb => new(0xABEB, 0xACAD, 0xAB15, 0x0707, 0x9008, 0x0000, 0xAB68),
        RoomEnemyProjectileKind.BombTorizoSonicBoom => new(0xAE15, 0xAE6C, 0xADBF, 0x1403, 0x100A, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.GoldenTorizoChozoOrb => new(0xAC7C, 0xACFA, 0xAB15, 0x0707, 0xB050, 0x0000, 0xAB68),
        RoomEnemyProjectileKind.GoldenTorizoSonicBoom => new(0xAE15, 0xAE6C, 0xADBF, 0x1403, 0x1078, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.BombTorizoRightFootDust => new(0xAF50, 0x84FB, 0xAF9D, 0x0000, 0x3000, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.BombTorizoLeftFootDust => new(0xAFCD, 0x84FB, 0xAFB5, 0x0000, 0x3000, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.GoldenTorizoEgg => new(0xB001, 0xB043, 0xB104, 0x0707, 0x6064, 0x0000, 0xAB25),
        RoomEnemyProjectileKind.GoldenTorizoSuperMissile => new(0xB1CE, 0xB20D, 0xB293, 0x0404, 0xA0C8, 0x0000, 0xB2EF),
        RoomEnemyProjectileKind.GoldenTorizoEyeBeam => new(0xB328, 0xB38A, 0xB410, 0x0303, 0x700A, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.TourianStatueSplash => new(0xB87A, 0xB977, 0xB7A1, 0x0000, 0x3000, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.TourianStatueEyeGlow => new(0xB88E, 0x84FB, 0xB7B3, 0x0000, 0x3000, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.TourianStatueParticle => new(0xB8B5, 0xB982, 0xB802, 0x0000, 0x3000, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.TourianStatueTail => new(0xB8E8, 0x84FB, 0xB823, 0x0000, 0x3000, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.TourianStatueSoul => new(0xB8F8, 0xB9FD, 0xB84E, 0x0000, 0x3000, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.TourianStatueRidley => new(0xB951, 0xBA42, 0xB86A, 0x0000, 0x2000, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.TourianStatuePhantoon => new(0xB964, 0xBA42, 0xB872, 0x0000, 0x2000, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.TourianStatueBaseDecoration => new(0xB93E, 0xBA42, 0xB85A, 0x0000, 0x2000, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.WreckedShipChozoSpikeFootstep => new(0xAEFC, 0x84FB, 0xAEC4, 0x0000, 0x3000, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.WreckedShipChozoSpikeFootstepAlternate => new(0xAEFC, 0x84FB, 0xAEDC, 0x0000, 0x3000, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.TourianStatueDescentDust => new(0xAF43, 0x84FB, 0xAF14, 0x0000, 0x3000, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.ShaktoolAttackFrontCircle => new(0xBDA2, 0xBE03, 0xBD68, 0x0404, 0x000A, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.ShaktoolAttackMiddleCircle => new(0xBD9C, 0x84FB, 0xBD78, 0x0404, 0x2000, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.ShaktoolAttackBackCircle => new(0xBD9C, 0x84FB, 0xBD8C, 0x0404, 0x2000, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.SporeSpawnStalk => new(0xDCA3, 0xDD44, 0xDC2E, 0x0808, 0x2000, 0x0000, 0xDC2E),
        RoomEnemyProjectileKind.SporeSpawnSpore => new(0xDC8D, 0xDCEE, 0xDC1E, 0x0202, 0x8004, 0x0000, 0xDC34),
        RoomEnemyProjectileKind.SporeSpawnSpawner => new(0xDCD4, 0xDD46, 0xDC00, 0x0202, 0x2000, 0x0000, 0xDC00),
        RoomEnemyProjectileKind.SaveStationElectricity => new(0xE6AD, 0xE6D1, 0xE683, 0x0000, 0x3000, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.DownwardGateMoving => new(0xE5D0, 0xE604, 0xE53C, 0x0000, 0x2000, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.DownwardGateClosed => new(0xE5D5, 0xE604, 0xE55E, 0x0000, 0x2000, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.NoobTubeCrack => new(0xD6A5, 0x84FB, 0xD3D7, 0x0000, 0x3000, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.NoobTubeShard => new(0xD6C9, 0xD7FD, 0xD47D, 0x0000, 0x3000, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.NoobTubeReleasedAirBubble => new(0xD774, 0x84FB, 0xD652, 0x0000, 0x3000, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.MotherBrainDeathExplosion => new(0xC8F5, 0xC914, 0x0000, 0x0000, 0x2000, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.MotherBrainEscapeDoorFragment => new(0xC961, 0xC9D2, 0xCA22, 0x0000, 0x3000, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.MotherBrainEscapeSubtitle => new(0xCAF6, 0xCAFA, 0xCB0D, 0x0000, 0x1000, 0x0000, 0x84FC),
        _ => throw new InvalidDataException(
            $"Enemy projectile definition $86:{(ushort)kind:X4} is not compiled."),
    };
}
