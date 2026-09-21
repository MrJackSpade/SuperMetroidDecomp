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
        RoomEnemyProjectileKind.YappingMawBody => new(
            0xEC62, 0xEC94, YappingMawBodyProjectileInstructionProgramDefinitions.FacingDown,
            0x0202, 0x2005, 0x0000, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
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
        RoomEnemyProjectileKind.CrocomireProjectile => new(
            0x9023, 0x906B, CrocomireProjectileInstructionProgramDefinitions.MouthProjectile,
            0x0808, 0x8014, 0x0000,
            CrocomireProjectileInstructionProgramDefinitions.MouthProjectileShot),
        RoomEnemyProjectileKind.CrocomireBridgeCrumbling => new(
            0x9286, 0x92BA, CrocomireProjectileInstructionProgramDefinitions.BridgeFragment,
            0x0404, 0x8000, 0x0000,
            CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.CrocomireSpikeWallPieces => new(
            0x90CF, 0x9115, CrocomireProjectileInstructionProgramDefinitions.SpikeWallPiece,
            0x0000, 0x0000, 0x0000,
            CommonEnemyProjectileInstructionProgramDefinitions.Delete),
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
        RoomEnemyProjectileKind.PhantoonDestroyableFlame => new(
            0x9824, 0x9981,
            PhantoonProjectileInstructionProgramDefinitions.DestroyableIdle,
            0x1008, 0x8028, 0x0000,
            PhantoonProjectileInstructionProgramDefinitions.DestroyableShot),
        RoomEnemyProjectileKind.PhantoonStartingFlame => new(
            0x993A, 0x9B29,
            PhantoonProjectileInstructionProgramDefinitions.StartingFlame,
            0x1008, 0x4028, 0x0000,
            PhantoonProjectileInstructionProgramDefinitions.DestroyableIdle),
        RoomEnemyProjectileKind.DraygonGoop => new(
            0x8D04, 0x8E0F, DraygonProjectileInstructionProgramDefinitions.Goop,
            0x0808, 0xD000, DraygonProjectileInstructionProgramDefinitions.GoopTouch,
            DraygonProjectileInstructionProgramDefinitions.GoopShot),
        RoomEnemyProjectileKind.DraygonWallTurret => new(
            0x8D40, 0x8DFF, DraygonProjectileInstructionProgramDefinitions.WallTurretBloom,
            0x0808, 0x1080, 0x0000, 0xE138),
        RoomEnemyProjectileKind.MotherBrainRoomTurret => new(
            0xBE4F, 0xBFDF, MotherBrainTurretInstructionProgramDefinitions.TurretDown,
            0x0000, 0x6000, 0x0000, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.MotherBrainRoomTurretBullet => new(
            0xBF59, 0xC0E0, MotherBrainTurretInstructionProgramDefinitions.BulletSelector,
            0x0303, 0x4014,
            MotherBrainTurretInstructionProgramDefinitions.BulletTouchOrShot,
            MotherBrainTurretInstructionProgramDefinitions.BulletTouchOrShot),
        RoomEnemyProjectileKind.MotherBrainGlassShard => new(
            0xCDC5, 0xCE9B, MotherBrainGlassInstructionProgramDefinitions.ShardGroup0,
            0x0000, 0x3000, 0x0000, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.MotherBrainGlassSparkle => new(
            0xCE6D, 0x84FB, MotherBrainGlassInstructionProgramDefinitions.Sparkle,
            0x0000, 0x3000, 0x0000, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.MotherBrainOnionRing => new(
            0xC2F3, 0xC335,
            EnemyProjectileInstructionMechanicsDefinitions.MotherBrainBlueRingInitial,
            0x0606, 0x3050,
            EnemyProjectileInstructionMechanicsDefinitions.MotherBrainBlueRingTouch,
            0x84FC),
        RoomEnemyProjectileKind.MotherBrainBomb => new(
            0xC482, 0xC4C8,
            EnemyProjectileInstructionMechanicsDefinitions.MotherBrainBombInitial,
            0x0606, 0x40A0, 0x0000,
            CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.MotherBrainHandBeamCharging => new(
            0xC605, 0xC76D, MotherBrainHandBeamInstructionProgramDefinitions.Initial,
            0x0606, 0x1190, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.MotherBrainHandBeamFired => new(
            0xC684, 0xC76D, MotherBrainHandBeamInstructionProgramDefinitions.Initial,
            0x0606, 0x1190, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.MotherBrainRainbowBeamCharging => new(
            0xC80A,
            0xC814,
            EnemyProjectileInstructionMechanicsDefinitions.MotherBrainRainbowBeamChargingInitial,
            0x0000,
            0x7000,
            0x0000,
            0x84FC),
        RoomEnemyProjectileKind.MotherBrainPurpleBreathBig => new(
            0xCA6A, 0xCAA3,
            EnemyProjectileInstructionMechanicsDefinitions.MotherBrainPurpleBreathInitial,
            0x0000, 0x3000, 0x0000,
            CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.MotherBrainDrool => new(
            0xC843, 0xC84D,
            EnemyProjectileInstructionMechanicsDefinitions.MotherBrainDroolInitial,
            0x0000, 0x7000, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.MotherBrainDyingDrool => new(
            0xC843, 0xC84D,
            EnemyProjectileInstructionMechanicsDefinitions.MotherBrainDroolInitial,
            0x0000, 0x7000, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.MotherBrainRainbowBeamExplosion => new(
            0xC92F, 0xC94C,
            EnemyProjectileInstructionMechanicsDefinitions.MotherBrainRainbowExplosionInitial,
            0x0101, 0x7000, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.MotherBrainTopRightTube => new(
            0xCBC9, 0xCBE7, MotherBrainTopTubeInstructionProgramDefinitions.TopRight,
            0x1008, 0x5000, 0x0000, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.MotherBrainTopLeftTube => new(
            0xCBC9, 0xCBE7, MotherBrainTopTubeInstructionProgramDefinitions.TopLeft,
            0x1008, 0x5000, 0x0000, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.MotherBrainTopMiddleLeftTube => new(
            0xCBC9, 0xCBE7, MotherBrainTopTubeInstructionProgramDefinitions.TopMiddleLeft,
            0x1808, 0x5000, 0x0000, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.MotherBrainTopMiddleRightTube => new(
            0xCBC9, 0xCBE7, MotherBrainTopTubeInstructionProgramDefinitions.TopMiddleRight,
            0x1808, 0x5000, 0x0000, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.CeresRidleyFireball => new(
            0x93CA, 0x940E, CeresRidleyProjectileInstructionProgramDefinitions.Fireball,
            0x0606, 0x5003, 0x0000,
            CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnCenter => new(
            0x947F, 0x950C,
            CeresRidleyProjectileInstructionProgramDefinitions.HorizontalCenter,
            0x0606, 0x5003, 0x0000,
            CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.CeresRidleyVerticalAfterburnCenter => new(
            0x947F, 0x950C,
            CeresRidleyProjectileInstructionProgramDefinitions.VerticalCenter,
            0x0606, 0x5003, 0x0000,
            CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnRight => new(
            0x94C8, 0x950D,
            CeresRidleyProjectileInstructionProgramDefinitions.DirectionalAfterburn,
            0x0606, 0x5003, 0x0000,
            CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnLeft => new(
            0x94DC, 0x950D,
            CeresRidleyProjectileInstructionProgramDefinitions.DirectionalAfterburn,
            0x0606, 0x5003, 0x0000,
            CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.CeresRidleyVerticalAfterburnUp => new(
            0x94A0, 0x9522,
            CeresRidleyProjectileInstructionProgramDefinitions.DirectionalAfterburn,
            0x0606, 0x5003, 0x0000,
            CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.CeresRidleyVerticalAfterburnDown => new(
            0x94B4, 0x9522,
            CeresRidleyProjectileInstructionProgramDefinitions.DirectionalAfterburn,
            0x0606, 0x5003, 0x0000,
            CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.CeresFallingDebrisLight => new(
            0x96DC, 0x9701, CeresFallingDebrisInstructionProgramDefinitions.Light,
            0x0808, 0x4000, 0x0000,
            CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.CeresFallingDebrisDark => new(
            0x96DC, 0x9701, CeresFallingDebrisInstructionProgramDefinitions.Dark,
            0x0808, 0x4000, 0x0000,
            CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.GunshipLiftoffDustCloud => new(
            0xA2A1, 0xA327, GunshipDustInstructionProgramDefinitions.Index0,
            0x0808, 0x3000, 0x0000,
            GunshipDustInstructionProgramDefinitions.Index0),
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
        RoomEnemyProjectileKind.NuclearWaffleBody => new(
            0xBB92, 0xBBC6, NuclearWaffleProjectileInstructionProgramDefinitions.Initial,
            0x0808, 0xC040, 0x0000, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.FakeKraidSpit => new(
            0x9DEC, 0x9E1E, FakeKraidProjectileInstructionProgramDefinitions.Spit,
            0x0404, 0x0014, 0x0000, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.FakeKraidSpikeLeft => new(
            0x9E46, 0x9E83, FakeKraidProjectileInstructionProgramDefinitions.SpikeLeft,
            0x0204, 0x0006, 0x0000, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.FakeKraidSpikeRight => new(
            0x9E4B, 0x9E83, FakeKraidProjectileInstructionProgramDefinitions.SpikeRight,
            0x0204, 0x0006, 0x0000, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.KiHunterAcidSpitLeft => new(
            0xCF90, 0xCFF7, KiHunterAcidSpitInstructionProgramDefinitions.Left,
            0x0802, 0x0014, 0x0000, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.KiHunterAcidSpitRight => new(
            0xCFA6, 0xCFF7, KiHunterAcidSpitInstructionProgramDefinitions.Right,
            0x0802, 0x0014, 0x0000, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.PirateMotherBrainLaser => new(
            0xA009, 0xA05C,
            SpacePirateProjectileInstructionProgramDefinitions.LaserLeft,
            0x0410, 0x100A, 0x0000,
            CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.PirateClaw => new(
            0xA098, 0xA05B, 0x0000, 0x0808, 0x1014, 0x0000,
            CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.PolypRock => new(
            0xBBDB, 0xBC0F, PolypRockInstructionProgramDefinitions.Initial,
            0x0202, 0x0010, 0x0000, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.CacatacSpike => new(0xD992, 0xD9DB, 0xD92E, 0x0202, 0x0005, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.StokeProjectile => new(
            0xDB18, 0xDB5B, StokeProjectileInstructionProgramDefinitions.Initial,
            0x0202, 0x0005, 0x0000, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
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
        RoomEnemyProjectileKind.BombTorizoLowHealthDrool => new(
            0xA5D3, 0x84FB, BombTorizoDroolInstructionProgramDefinitions.NoDelay,
            0x0201, 0x3000, 0x0000, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.BombTorizoInitialDrool => new(
            0xA65D, 0x84FB, BombTorizoDroolInstructionProgramDefinitions.NoDelay,
            0x0201, 0x2000, 0x0000, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.BombTorizoExplosiveSwipe => new(
            0xA6F6, 0xA919, TorizoExplosiveSwipeInstructionProgramDefinitions.Initial,
            0x1010, 0x500A, 0x0000, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.BombTorizoLowHealthExplosion => new(
            0xA81B, 0x84FB, TorizoExplosionInstructionProgramDefinitions.LowHealthInitial,
            0x1004, 0x3000, 0x0000, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.BombTorizoDeathExplosion => new(
            0xA871, 0x84FB, TorizoExplosionInstructionProgramDefinitions.DeathInitial,
            0x1004, 0x3000, 0x0000, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.BombTorizoStatueBreaking => new(
            BombTorizoStatueInstructionProgramDefinitions.InitializationAi,
            EnemyProjectileCodePointers.RTS_8684FB,
            BombTorizoStatueInstructionProgramDefinitions.Program(8),
            0x0808, 0x3000, 0x0000,
            CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.BombTorizoChozoOrb => new(
            0xABEB, 0xACAD, TorizoChozoOrbInstructionProgramDefinitions.MovingLeft,
            0x0707, 0x9008, 0x0000, TorizoChozoOrbInstructionProgramDefinitions.Shot),
        RoomEnemyProjectileKind.BombTorizoSonicBoom => new(
            TorizoSonicBoomInstructionProgramDefinitions.InitializationAi,
            EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_TorizoSonicBoom,
            TorizoSonicBoomInstructionProgramDefinitions.FiredLeft,
            0x1403, 0x100A, 0x0000,
            CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.GoldenTorizoChozoOrb => new(
            0xAC7C, 0xACFA, TorizoChozoOrbInstructionProgramDefinitions.MovingLeft,
            0x0707, 0xB050, 0x0000, TorizoChozoOrbInstructionProgramDefinitions.Shot),
        RoomEnemyProjectileKind.GoldenTorizoSonicBoom => new(
            TorizoSonicBoomInstructionProgramDefinitions.InitializationAi,
            EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_TorizoSonicBoom,
            TorizoSonicBoomInstructionProgramDefinitions.FiredLeft,
            0x1403, 0x1078, 0x0000,
            CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.BombTorizoRightFootDust => new(
            0xAF50, 0x84FB, TorizoLandingDustInstructionProgramDefinitions.RightFoot,
            0x0000, 0x3000, 0x0000, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.BombTorizoLeftFootDust => new(
            0xAFCD, 0x84FB, TorizoLandingDustInstructionProgramDefinitions.LeftFoot,
            0x0000, 0x3000, 0x0000, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.GoldenTorizoEgg => new(
            GoldenTorizoEggInstructionProgramDefinitions.InitializationAi,
            EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_GoldenTorizoEgg_Bouncing,
            GoldenTorizoEggInstructionProgramDefinitions.BouncingLeft,
            0x0707, 0x6064, 0x0000,
            TorizoChozoOrbInstructionProgramDefinitions.WallImpact),
        RoomEnemyProjectileKind.GoldenTorizoSuperMissile => new(0xB1CE, 0xB20D, 0xB293, 0x0404, 0xA0C8, 0x0000, 0xB2EF),
        RoomEnemyProjectileKind.GoldenTorizoEyeBeam => new(0xB328, 0xB38A, 0xB410, 0x0303, 0x700A, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.TourianStatueSplash => new(0xB87A, 0xB977, TourianStatueProjectileInstructionProgramDefinitions.Splash, 0x0000, 0x3000, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.TourianStatueEyeGlow => new(0xB88E, 0x84FB, TourianStatueProjectileInstructionProgramDefinitions.EyeGlow, 0x0000, 0x3000, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.TourianStatueParticle => new(0xB8B5, 0xB982, TourianStatueProjectileInstructionProgramDefinitions.Particle, 0x0000, 0x3000, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.TourianStatueTail => new(0xB8E8, 0x84FB, TourianStatueProjectileInstructionProgramDefinitions.Tail, 0x0000, 0x3000, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.TourianStatueSoul => new(0xB8F8, 0xB9FD, TourianStatueProjectileInstructionProgramDefinitions.Soul, 0x0000, 0x3000, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.TourianStatueRidley => new(0xB951, 0xBA42, TourianStatueProjectileInstructionProgramDefinitions.Ridley, 0x0000, 0x2000, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.TourianStatuePhantoon => new(0xB964, 0xBA42, TourianStatueProjectileInstructionProgramDefinitions.Phantoon, 0x0000, 0x2000, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.TourianStatueBaseDecoration => new(0xB93E, 0xBA42, TourianStatueProjectileInstructionProgramDefinitions.BaseDecoration, 0x0000, 0x2000, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.WreckedShipChozoSpikeFootstep => new(0xAEFC, 0x84FB, ChozoTourianDustInstructionProgramDefinitions.Footsteps, 0x0000, 0x3000, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.WreckedShipChozoSpikeFootstepAlternate => new(0xAEFC, 0x84FB, ChozoTourianDustInstructionProgramDefinitions.SpikeClearingExplosions, 0x0000, 0x3000, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.TourianStatueDescentDust => new(0xAF43, 0x84FB, ChozoTourianDustInstructionProgramDefinitions.TourianDescentDust, 0x0000, 0x3000, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.ShaktoolAttackFrontCircle => new(0xBDA2, 0xBE03, ShaktoolProjectileInstructionProgramDefinitions.Front, 0x0404, 0x000A, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.ShaktoolAttackMiddleCircle => new(0xBD9C, 0x84FB, ShaktoolProjectileInstructionProgramDefinitions.Middle, 0x0404, 0x2000, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.ShaktoolAttackBackCircle => new(0xBD9C, 0x84FB, ShaktoolProjectileInstructionProgramDefinitions.Back, 0x0404, 0x2000, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.SporeSpawnStalk => new(0xDCA3, 0xDD44, SporeSpawnProjectileInstructionProgramDefinitions.Stalk, 0x0808, 0x2000, 0x0000, SporeSpawnProjectileInstructionProgramDefinitions.Stalk),
        RoomEnemyProjectileKind.SporeSpawnSpore => new(0xDC8D, 0xDCEE, SporeSpawnProjectileInstructionProgramDefinitions.Spore, 0x0202, 0x8004, 0x0000, SporeSpawnProjectileInstructionProgramDefinitions.SporeShot),
        RoomEnemyProjectileKind.SporeSpawnSpawner => new(0xDCD4, 0xDD46, SporeSpawnProjectileInstructionProgramDefinitions.SpawnerClosed, 0x0202, 0x2000, 0x0000, SporeSpawnProjectileInstructionProgramDefinitions.SpawnerClosed),
        RoomEnemyProjectileKind.SaveStationElectricity => new(
            0xE6AD,
            0xE6D1,
            SaveStationElectricityInstructionProgramDefinitions.Initial,
            0x0000,
            0x3000,
            0x0000,
            CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.DownwardGateMoving => new(
            0xE5D0,
            DownwardGateEnemyProjectileRomData.InertPreInstruction,
            DownwardGateProjectileInstructionProgramDefinitions.Moving,
            0x0000,
            0x2000,
            0x0000,
            CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.DownwardGateClosed => new(
            0xE5D5,
            DownwardGateEnemyProjectileRomData.InertPreInstruction,
            DownwardGateProjectileInstructionProgramDefinitions.Closed,
            0x0000,
            0x2000,
            0x0000,
            CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.NoobTubeCrack => new(
            0xD6A5, 0x84FB, NoobTubeProjectileInstructionProgramDefinitions.Crack,
            0x0000, 0x3000, 0x0000, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.NoobTubeShard => new(
            0xD6C9, 0xD7FD,
            NoobTubeProjectileInstructionProgramDefinitions.ShardInstructionLists[0],
            0x0000, 0x3000, 0x0000, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.NoobTubeReleasedAirBubble => new(
            0xD774, 0x84FB, NoobTubeProjectileInstructionProgramDefinitions.ReleasedAirBubble,
            0x0000, 0x3000, 0x0000, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.MotherBrainDeathExplosion => new(0xC8F5, 0xC914, 0x0000, 0x0000, 0x2000, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.MotherBrainEscapeDoorFragment => new(
            0xC961, 0xC9D2,
            EnemyProjectileInstructionMechanicsDefinitions.MotherBrainEscapeDoorFragmentInitial,
            0x0000, 0x3000, 0x0000,
            CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.MotherBrainEscapeSubtitle => new(
            0xCAF6, 0xCAFA,
            EnemyProjectileInstructionMechanicsDefinitions.MotherBrainSubtitleInitial,
            0x0000, 0x1000, 0x0000,
            CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        _ => throw new InvalidDataException(
            $"Enemy projectile definition $86:{(ushort)kind:X4} is not compiled."),
    };
}
