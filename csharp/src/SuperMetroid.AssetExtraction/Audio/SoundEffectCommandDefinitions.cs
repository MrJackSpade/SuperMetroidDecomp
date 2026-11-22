using SuperMetroid.Core;
using System.Collections.Frozen;
using SuperMetroid.Core.Audio;

namespace SuperMetroid.AssetExtraction.Audio;

/// <summary>
/// The request number each sound and each stop has in its library of the installed sound program: the import pipeline's
/// private knowledge of the cartridge's sound numbering, read only by <see cref="SoundEffectCommands"/>. A stop's program
/// plays one note at zero volume (#2580). A library's other requests name nothing the game asks for (#1686), among them
/// the 27 whose sound is identical to another request's, which the game asks for under that one name (#2580): the import
/// keeps their programs in its catalog but exports no clip of them. Eleven unnamed requests are silent as the stops are, and stay
/// unnamed because nothing asks for them: library one's 0x2b to 0x2d and 0x3a, library two's 0x41 and 0x44, and library
/// three's 0x12, 0x18, 0x1a, 0x20 and 0x2f.
/// </summary>
[EncodingOwner("request numbers of the sound driver's three sound libraries")]
internal static partial class SoundEffectCommandDefinitions
{
    /// <summary>Each library-one sound's request number.</summary>
    internal static readonly FrozenDictionary<SamusSound, byte> Library1 = new Dictionary<SamusSound, byte>
    {
        [SamusSound.PowerBombExplosion] = 0x01,
        [SamusSound.Missile] = 0x03,
        [SamusSound.SuperMissile] = 0x04,
        [SamusSound.GrappleStart] = 0x05,
        [SamusSound.Grappling] = 0x06,
        [SamusSound.GrappleEnd] = 0x07,
        [SamusSound.ChargingBeam] = 0x08,
        [SamusSound.XrayStart] = 0x09,
        [SamusSound.UnchargedPowerBeam] = 0x0b,
        [SamusSound.UnchargedIceBeam] = 0x0c,
        [SamusSound.UnchargedWaveBeam] = 0x0d,
        [SamusSound.UnchargedSpazer] = 0x0f,
        [SamusSound.UnchargedSpazerIce] = 0x10,
        [SamusSound.UnchargedPlasma] = 0x13,
        [SamusSound.ChargedPowerBeam] = 0x17,
        [SamusSound.ChargedIceBeam] = 0x18,
        [SamusSound.ChargedWaveBeam] = 0x19,
        [SamusSound.ChargedSpazer] = 0x1b,
        [SamusSound.ChargedPlasma] = 0x1f,
        [SamusSound.IceSpecialAttack] = 0x23,
        [SamusSound.IceSpecialAttackEnd] = 0x24,
        [SamusSound.SpazerSpecialAttack] = 0x25,
        [SamusSound.SpazerSpecialAttackEnd] = 0x26,
        [SamusSound.PlasmaSpecialAttack] = 0x27,
        [SamusSound.WaveSpecialAttack] = 0x28,
        [SamusSound.FileSelected] = 0x2a,
        [SamusSound.Saving] = 0x2e,
        [SamusSound.UnderwaterSpaceJump] = 0x2f,
        [SamusSound.ResumedSpinJump] = 0x30,
        [SamusSound.SpinJump] = 0x31,
        [SamusSound.ScrewAttack] = 0x33,
        [SamusSound.SamusDamaged] = 0x35,
        [SamusSound.MapScroll] = 0x36,
        [SamusSound.MenuCursor] = 0x37,
        [SamusSound.MenuConfirm] = 0x38,
        [SamusSound.HudWeaponSelect] = 0x39,
        [SamusSound.MapExpand] = 0x3b,
        [SamusSound.MapReturn] = 0x3c,
        [SamusSound.DudShot] = 0x3d,
        [SamusSound.SpaceJump] = 0x3e,
        [SamusSound.MotherBrainRainbowBeam] = 0x40,
        [SamusSound.ResumeChargingBeam] = 0x41,
    }.ToFrozenDictionary();

    /// <summary>Each library-one stop's request number.</summary>
    internal static readonly FrozenDictionary<SamusSoundStop, byte> Library1Stops = new Dictionary<SamusSoundStop, byte>
    {
        [SamusSoundStop.Any] = 0x02,
        [SamusSoundStop.Xray] = 0x0a,
        [SamusSoundStop.WaveSpecialAttack] = 0x29,
        [SamusSoundStop.SpinJump] = 0x32,
        [SamusSoundStop.ScrewAttack] = 0x34,
    }.ToFrozenDictionary();

    /// <summary>Each library-two sound's request number.</summary>
    internal static readonly FrozenDictionary<EnemySound, byte> Library2 = new Dictionary<EnemySound, byte>
    {
        [EnemySound.SmallEnergyPickup] = 0x01,
        [EnemySound.BigEnergyPickup] = 0x02,
        [EnemySound.AmmoPickup] = 0x03,
        [EnemySound.ContactBlockBreak] = 0x06,
        [EnemySound.WeaponExplosion] = 0x07,
        [EnemySound.EnemyKilled] = 0x09,
        [EnemySound.BlockCrumble] = 0x0a,
        [EnemySound.ContactKill] = 0x0b,
        [EnemySound.BeamImpact] = 0x0c,
        [EnemySound.SplashIn] = 0x0d,
        [EnemySound.SplashOut] = 0x0e,
        [EnemySound.LowBubbles] = 0x0f,
        [EnemySound.LiquidDamage] = 0x10,
        [EnemySound.HighBubbles] = 0x11,
        [EnemySound.LavaBubblingBurst] = 0x12,
        [EnemySound.LavaBubblingFalling] = 0x13,
        [EnemySound.LavaBubblingRising] = 0x14,
        [EnemySound.FakeKraidCry] = 0x16,
        [EnemySound.MorphBallEyeRay] = 0x17,
        [EnemySound.Beacon] = 0x18,
        [EnemySound.TourianStatueParticle] = 0x19,
        [EnemySound.GlassTubeShatter] = 0x1a,
        [EnemySound.HeavyClank] = 0x1b,
        [EnemySound.Rattle] = 0x1c,
        [EnemySound.DachoraCry] = 0x1d,
        [EnemySound.HighSpit] = 0x1f,
        [EnemySound.ShotFly] = 0x20,
        [EnemySound.Screech] = 0x21,
        [EnemySound.RisingSqueaks] = 0x22,
        [EnemySound.LowSqueaks] = 0x23,
        [EnemySound.SmallExplosion] = 0x24,
        [EnemySound.BigExplosion] = 0x25,
        [EnemySound.BombTorizoSwipe] = 0x26,
        [EnemySound.ShotTorizo] = 0x27,
        [EnemySound.HeavyRumble] = 0x29,
        [EnemySound.RollingRumble] = 0x2b,
        [EnemySound.ShotSporeSpawn] = 0x2c,
        [EnemySound.Roar] = 0x2d,
        [EnemySound.KraidDyingCry] = 0x2e,
        [EnemySound.YappingMaw] = 0x2f,
        [EnemySound.SteppedSqueal] = 0x30,
        [EnemySound.PlantChewing] = 0x31,
        [EnemySound.EtecoonWallJump] = 0x32,
        [EnemySound.EtecoonCry] = 0x33,
        [EnemySound.LowThump] = 0x34,
        [EnemySound.EtecoonTheme] = 0x35,
        [EnemySound.Squeaks] = 0x36,
        [EnemySound.StationEngaged] = 0x37,
        [EnemySound.StationDisengaged] = 0x38,
        [EnemySound.MamaTurtleSpinning] = 0x3a,
        [EnemySound.Shinespark] = 0x3b,
        [EnemySound.LowBuzz] = 0x3e,
        [EnemySound.Whoosh] = 0x3f,
        [EnemySound.BoulderBounce] = 0x42,
        [EnemySound.BoulderExplosion] = 0x43,
        [EnemySound.Lavaquake] = 0x46,
        [EnemySound.ShotWaver] = 0x47,
        [EnemySound.TorizoSonicBoom] = 0x48,
        [EnemySound.Hiss] = 0x49,
        [EnemySound.ShotEvir] = 0x4a,
        [EnemySound.ChozoFootsteps] = 0x4b,
        [EnemySound.AcidSpit] = 0x4c,
        [EnemySound.GunshipHover] = 0x4d,
        [EnemySound.CeresRidleyGetaway] = 0x4e,
        [EnemySound.MetroidDrain] = 0x50,
        [EnemySound.ShotCoven] = 0x51,
        [EnemySound.BabyMetroidRemorse] = 0x52,
        [EnemySound.ShotBeetom] = 0x55,
        [EnemySound.SuitAcquired] = 0x56,
        [EnemySound.DudShot] = 0x57,
        [EnemySound.RisingSqueal] = 0x58,
        [EnemySound.RidleyRoar] = 0x59,
        [EnemySound.ShotMetroid] = 0x5a,
        [EnemySound.Burrow] = 0x5c,
        [EnemySound.SidehopperJump] = 0x5d,
        [EnemySound.LowThud] = 0x5e,
        [EnemySound.LowWarble] = 0x5f,
        [EnemySound.FireSpit] = 0x61,
        [EnemySound.MotherBrainHandBeam] = 0x63,
        [EnemySound.HoltzCry] = 0x64,
        [EnemySound.RioCry] = 0x65,
        [EnemySound.Yell] = 0x66,
        [EnemySound.Laser] = 0x67,
        [EnemySound.WorkRobot] = 0x68,
        [EnemySound.ShotShaktool] = 0x69,
        [EnemySound.KagoBug] = 0x6c,
        [EnemySound.CeresTileFall] = 0x6d,
        [EnemySound.ShotMotherBrain] = 0x6e,
        [EnemySound.MotherBrainLowCry] = 0x6f,
        [EnemySound.YardBounce] = 0x70,
        [EnemySound.BabyMetroidCry] = 0x72,
        [EnemySound.Shriek] = 0x73,
        [EnemySound.CrocomireCry] = 0x74,
        [EnemySound.CrocomireSkeletonCollapse] = 0x75,
        [EnemySound.CrocomireMeltingCry] = 0x77,
        [EnemySound.BabyMetroidDrain] = 0x78,
        [EnemySound.PhantoonAppearsMiddle] = 0x79,
        [EnemySound.PhantoonAppearsLow] = 0x7a,
        [EnemySound.PhantoonAppearsHigh] = 0x7b,
        [EnemySound.BotwoonSpit] = 0x7c,
        [EnemySound.BabyMetroidGuilt] = 0x7d,
        [EnemySound.LongWail] = 0x7e,
        [EnemySound.MotherBrainRainbowCharge] = 0x7f,
    }.ToFrozenDictionary();

    /// <summary>Each library-two stop's request number.</summary>
    internal static readonly FrozenDictionary<EnemySoundStop, byte> Library2Stops = new Dictionary<EnemySoundStop, byte>
    {
        [EnemySoundStop.Elevatube] = 0x15,
        [EnemySoundStop.Any] = 0x71,
    }.ToFrozenDictionary();

    /// <summary>Each library-three sound's request number.</summary>
    internal static readonly FrozenDictionary<RoomSound, byte> Library3 = new Dictionary<RoomSound, byte>
    {
        [RoomSound.LowHealthBeep] = 0x02,
        [RoomSound.SpeedBooster] = 0x03,
        [RoomSound.HardLanding] = 0x04,
        [RoomSound.Landing] = 0x05,
        [RoomSound.Footsteps] = 0x06,
        [RoomSound.DoorOpened] = 0x07,
        [RoomSound.DoorClosed] = 0x08,
        [RoomSound.MissileDoorHit] = 0x09,
        [RoomSound.EnemyFrozen] = 0x0a,
        [RoomSound.Elevator] = 0x0b,
        [RoomSound.StoredShinespark] = 0x0c,
        [RoomSound.Typewriter] = 0x0d,
        [RoomSound.GateMoving] = 0x0e,
        [RoomSound.Shinespark] = 0x0f,
        [RoomSound.ShinesparkEnd] = 0x10,
        [RoomSound.ShinesparkEndShort] = 0x11,
        [RoomSound.GunshipPadOpening] = 0x14,
        [RoomSound.GunshipPadClosing] = 0x15,
        [RoomSound.MotherBrainFootstep] = 0x16,
        [RoomSound.MotherBrainBlueRings] = 0x17,
        [RoomSound.BabyMetroidDeath] = 0x19,
        [RoomSound.DraygonDyingCry] = 0x1b,
        [RoomSound.CrocomireSpit] = 0x1c,
        [RoomSound.PhantoonFlame] = 0x1d,
        [RoomSound.KraidLint] = 0x1f,
        [RoomSound.RidleyTailWhip] = 0x21,
        [RoomSound.CrocomireAcidDamage] = 0x22,
        [RoomSound.BabyMetroidHighCry] = 0x23,
        [RoomSound.CeresBabyMetroidCry] = 0x24,
        [RoomSound.BabyMetroidHitchedCry] = 0x26,
        [RoomSound.BabyMetroidLowCry] = 0x27,
        [RoomSound.PhantoonMaterialise] = 0x28,
        [RoomSound.PhantoonSuperMissiled] = 0x29,
        [RoomSound.PauseBeep] = 0x2a,
        [RoomSound.ResumedSpeedBooster] = 0x2b,
        [RoomSound.CeresDoorOpening] = 0x2c,
        [RoomSound.IncrementalEnergy] = 0x2d,
        [RoomSound.MotherBrainGlassShatter] = 0x2e,
    }.ToFrozenDictionary();

    /// <summary>Each library-three stop's request number.</summary>
    internal static readonly FrozenDictionary<RoomSoundStop, byte> Library3Stops = new Dictionary<RoomSoundStop, byte>
    {
        [RoomSoundStop.Any] = 0x01,
        [RoomSoundStop.SpeedBoosterOrElevator] = 0x25,
    }.ToFrozenDictionary();
}
