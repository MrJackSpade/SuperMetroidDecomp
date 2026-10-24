namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The cartridge sound effect ids ordinary enemies queue, by enemy and event. Each value is the id within the library its queuing call names.
/// </summary>
internal static class EnemySoundEffectDefinitions
{
    /// <summary>Library-two sound queued when Alcoon emerges near Samus.</summary>
    internal static readonly SoundEffectId AlcoonEmergeSound = Library2Sound.SidehopperLanding;

    /// <summary>Library-two sound queued by each fireball instruction.</summary>
    internal static readonly SoundEffectId AlcoonFireSound = Library2Sound.AlcoonSpit;

    /// <summary>Library-two cue replacing the impact cue when the boulder breaks into dust.</summary>
    internal static readonly SoundEffectId BoulderBreakSound = Library2Sound.BoulderExplosion;

    /// <summary>Library-two cue published when a falling boulder touches ground.</summary>
    internal static readonly SoundEffectId BoulderImpactSound = Library2Sound.BoulderBounce;

    /// <summary>Library-two sound request emitted by Boyon's start-bounce instruction.</summary>
    internal static readonly SoundEffectId BoyonBounceSound = Library2Sound.SplashOut;

    /// <summary>Library-two request emitted by the spike-release animation command.</summary>
    internal static readonly SoundEffectId CacatacSpikeSound = Library2Sound.CacatacSpikes;

    /// <summary>Library-two sound queued when Samus wakes the tutorial Dachora.</summary>
    internal static readonly SoundEffectId DachoraActivationSound = Library2Sound.DachoraCry;

    /// <summary>Library-two sound queued on Dachora's ceiling contact.</summary>
    internal static readonly SoundEffectId DachoraCeilingImpactSound = Library2Sound.DachoraShinesparkEnd;

    /// <summary>Library-two sound queued while Dachora stores its shinespark.</summary>
    internal static readonly SoundEffectId DachoraChargeSound = Library2Sound.DachoraStoredShinespark;

    /// <summary>Library-two sound queued when Dachora launches upward.</summary>
    internal static readonly SoundEffectId DachoraLaunchSound = Library2Sound.DachoraShinespark;

    /// <summary>Library-two sound accompanying Dachora's Speed Booster run.</summary>
    internal static readonly SoundEffectId DachoraSpeedBoosterSound = Library2Sound.DachoraSpeedBooster;

    /// <summary>Library-two sound queued at the scripted left wall turn.</summary>
    internal static readonly SoundEffectId DachoraWallImpactSound = Library2Sound.Silence;

    /// <summary>Library-two cue published by a Dragon fireball volley.</summary>
    internal static readonly SoundEffectId DragonFireballSound = Library2Sound.FireSpit;

    /// <summary>Library-three selector $25 plays when arrival reaches the resting height.</summary>
    internal static readonly SoundEffectId ElevatorArrivalSoundLibrary3 = Library3Sound.SpeedBoosterEnd;

    /// <summary>Library-one selector $32 accompanies departure.</summary>
    internal static readonly SoundEffectId ElevatorDepartureSoundLibrary1 = Library1Sound.SpinJumpEnd;

    /// <summary>Library-three selector $0B accompanies departure.</summary>
    internal static readonly SoundEffectId ElevatorDepartureSoundLibrary3 = Library3Sound.Elevator;

    /// <summary>Library-two sound queued at an audible teaching hop.</summary>
    internal static readonly SoundEffectId EtecoonJumpSound = Library2Sound.EtecoonCry;

    /// <summary>Library-two sound queued when an Etecoon wakes near Samus.</summary>
    internal static readonly SoundEffectId EtecoonWakeSound = Library2Sound.EtecoonTheme;

    /// <summary>Library-two sound queued when an airborne Etecoon meets a wall.</summary>
    internal static readonly SoundEffectId EtecoonWallSound = Library2Sound.EtecoonWallJump;

    /// <summary>Library-two sound emitted by the spit instruction.</summary>
    internal static readonly SoundEffectId EvirSpitSound = Library2Sound.SidehopperLanding;

    /// <summary>Library-two cue when the eye finishes opening and requests its beam.</summary>
    internal static readonly SoundEffectId EyeActivationSound = Library2Sound.MorphBallEyeRay;

    /// <summary>Library-two cue when Samus leaves the active eye's range.</summary>
    internal static readonly SoundEffectId EyeDeactivationSound = Library2Sound.Silence;

    /// <summary>Library-two cue emitted when a live bug's sound countdown reaches zero.</summary>
    internal static readonly SoundEffectId KagoBugSoundEffect = Library2Sound.KagoBug;

    /// <summary>Library-two landing cue published when the falling top reaches its target.</summary>
    internal static readonly SoundEffectId KzanLandingSoundEffect = Library2Sound.SpikePlatformStop;

    /// <summary>Library-two selector $64 is published when the Rio starts its return.</summary>
    internal static readonly SoundEffectId LowerNorfairRioReturnSound = Library2Sound.HoltzCry;

    /// <summary>Library-two sound requested by the spinning animation instruction.</summary>
    internal static readonly SoundEffectId MamaTurtleSpinSound = Library2Sound.TatoriSpinning;

    /// <summary>Library-two sound requested on Mama's hover collision with a wall.</summary>
    internal static readonly SoundEffectId MamaTurtleWallSound = Library2Sound.SpikePlatformStop;

    /// <summary>Library-two sound from the private tail of Oum shot handling.</summary>
    internal static readonly SoundEffectId MaridiaLargeSnailShotSound = Library2Sound.DudShotOnDoor;

    /// <summary>Library-two splash sound emitted at the water collision.</summary>
    internal static readonly SoundEffectId MaridiaLargeSnailSplashSound = Library2Sound.SplashOut;

    /// <summary>Library-two sound selected by Metroid's private animation bytecode.</summary>
    internal static readonly SoundEffectId MetroidAnimationSoundEffect = Library2Sound.MetroidDrain;

    /// <summary>Library-two sound queued when an unfrozen Metroid recoils.</summary>
    internal static readonly SoundEffectId MetroidRecoilSoundEffect = Library2Sound.ShotMetroid;

    /// <summary>Library-two sound emitted when a parent begins its jump.</summary>
    internal static readonly SoundEffectId NorfairLavaJumpingEnemyJumpSound = Library2Sound.SplashIn;

    /// <summary>Library-two sound queued when a takeoff becomes a dive.</summary>
    internal static readonly SoundEffectId NorfairRioDiveSound = Library2Sound.RioCry;

    /// <summary>Library-two sound requested when the shared Pirate/Mother Brain laser launches.</summary>
    internal static readonly SoundEffectId PirateMotherBrainLaserSound = Library2Sound.Laser;

    /// <summary>Library-two sound requested when an on-screen Rio begins its dive.</summary>
    internal static readonly SoundEffectId RioDiveSound = Library2Sound.RioCry;

    /// <summary>Library-two sound queued when an on-screen shutter begins moving.</summary>
    internal static readonly SoundEffectId ShutterActivationSound = Library2Sound.SplashOut;

    /// <summary>Library-two cue requested by the wall-jump opcode.</summary>
    internal static readonly SoundEffectId WallPirateJumpSound = Library2Sound.ShotSpacePirate;

    /// <summary>Library-two cue requested after a successful laser allocation.</summary>
    internal static readonly SoundEffectId WallPirateLaserSound = Library2Sound.Laser;

    /// <summary>Library-two cue requested by the footstep opcode.</summary>
    internal static readonly SoundEffectId WorkRobotFootstepSound = Library2Sound.WorkRobot;

    /// <summary>Library-two cue requested after a robot laser projectile is allocated.</summary>
    internal static readonly SoundEffectId WorkRobotLaserSound = Library2Sound.Laser;

    /// <summary>Library-two attack sound emitted only while the Maw is on screen.</summary>
    internal static readonly SoundEffectId YappingMawAttackSound = Library2Sound.YappingMaw;

    /// <summary>Library-two sound queued by the Chozo statue's grab instruction.</summary>
    internal static readonly SoundEffectId ChozoGrabsSamusSound = Library2Sound.ChozoGrab;

    /// <summary>Library-two sound queued by the Chozo statue's footstep instruction.</summary>
    internal static readonly SoundEffectId ChozoFootstepsSound = Library2Sound.ChozoFootsteps;

    /// <summary>Library-two small-explosion sound queued by an enemy death explosion's instruction.</summary>
    internal static readonly SoundEffectId DeathSmallExplosionSound = Library2Sound.SmallExplosion;

    /// <summary>Library-two sound queued by an enemy death explosion when contact killed the enemy.</summary>
    internal static readonly SoundEffectId DeathContactKilledSound = Library2Sound.ContactKill;

    /// <summary>Library-two sound queued as a Metaree or Skree releases its dive.</summary>
    internal static readonly SoundEffectId DiveSound = Library2Sound.SkreeDive;

    /// <summary>Library-two sound queued as a Metaree or Skree hits the floor and burrows.</summary>
    internal static readonly SoundEffectId BurrowSound = Library2Sound.SkreeBurrow;

    /// <summary>Library-two sound queued when an airborne Yard bounces off a wall or the floor.</summary>
    internal static readonly SoundEffectId YardBounceSound = Library2Sound.YardBounce;

    /// <summary>Library-two sound queued when a walking Space Pirate reflects a projectile.</summary>
    internal static readonly SoundEffectId WalkingPirateReflectSound = Library2Sound.ShotSpacePirate;
}
