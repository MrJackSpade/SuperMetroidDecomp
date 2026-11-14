namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The game sound effect ids ordinary enemies queue, by enemy and event. Each value is the id within the library its queuing call names.
/// </summary>
internal static class EnemySoundEffectDefinitions
{
    /// <summary>Enemy-library sound queued when Alcoon emerges near Samus.</summary>
    internal static readonly SoundEffectId AlcoonEmergeSound = EnemySound.LowThud;

    /// <summary>Enemy-library sound queued by each fireball cue.</summary>
    internal static readonly SoundEffectId AlcoonFireSound = EnemySound.AlcoonSpit;

    /// <summary>Enemy-library cue replacing the impact cue when the boulder breaks into dust.</summary>
    internal static readonly SoundEffectId BoulderBreakSound = EnemySound.BoulderExplosion;

    /// <summary>Enemy-library cue published when a falling boulder touches ground.</summary>
    internal static readonly SoundEffectId BoulderImpactSound = EnemySound.BoulderBounce;

    /// <summary>Enemy-library sound request emitted by Boyon's start-bounce cue.</summary>
    internal static readonly SoundEffectId BoyonBounceSound = EnemySound.SplashOut;

    /// <summary>Enemy-library request emitted by the spike-release animation cue.</summary>
    internal static readonly SoundEffectId CacatacSpikeSound = EnemySound.CacatacSpikes;

    /// <summary>Enemy-library sound queued when Samus wakes the tutorial Dachora.</summary>
    internal static readonly SoundEffectId DachoraActivationSound = EnemySound.DachoraCry;

    /// <summary>Enemy-library sound queued on Dachora's ceiling contact.</summary>
    internal static readonly SoundEffectId DachoraCeilingImpactSound = EnemySound.DachoraShinesparkEnd;

    /// <summary>Enemy-library sound queued while Dachora stores its shinespark.</summary>
    internal static readonly SoundEffectId DachoraChargeSound = EnemySound.DachoraStoredShinespark;

    /// <summary>Enemy-library sound queued when Dachora launches upward.</summary>
    internal static readonly SoundEffectId DachoraLaunchSound = EnemySound.DachoraShinespark;

    /// <summary>Enemy-library sound accompanying Dachora's Speed Booster run.</summary>
    internal static readonly SoundEffectId DachoraSpeedBoosterSound = EnemySound.DachoraSpeedBooster;

    /// <summary>
    /// The enemy-library silence queued at the scripted left wall turn. It is a real request (#1842): it interrupts the
    /// looping <see cref="DachoraSpeedBoosterSound"/> of the run that hit the wall.
    /// </summary>
    internal static readonly SoundEffectId DachoraSpeedBoosterSoundStop = EnemySound.Silence;

    /// <summary>Enemy-library cue published by a Dragon fireball volley.</summary>
    internal static readonly SoundEffectId DragonFireballSound = EnemySound.FireSpit;

    /// <summary>The room-library request that stops the looping <see cref="ElevatorDepartureSound"/> when arrival reaches the resting height.</summary>
    internal static readonly SoundEffectId ElevatorSoundStop = RoomSound.SpeedBoosterOrElevatorStop;

    /// <summary>The Samus-library sound that accompanies departure.</summary>
    internal static readonly SoundEffectId ElevatorDepartureSpinEndSound = SamusSound.SpinJumpEnd;

    /// <summary>The room-library elevator sound that accompanies departure.</summary>
    internal static readonly SoundEffectId ElevatorDepartureSound = RoomSound.Elevator;

    /// <summary>Enemy-library sound queued at an audible teaching hop.</summary>
    internal static readonly SoundEffectId EtecoonJumpSound = EnemySound.EtecoonCry;

    /// <summary>Enemy-library sound queued when an Etecoon wakes near Samus.</summary>
    internal static readonly SoundEffectId EtecoonWakeSound = EnemySound.EtecoonTheme;

    /// <summary>Enemy-library sound queued when an airborne Etecoon meets a wall.</summary>
    internal static readonly SoundEffectId EtecoonWallSound = EnemySound.EtecoonWallJump;

    /// <summary>Enemy-library sound emitted by the spit cue.</summary>
    internal static readonly SoundEffectId EvirSpitSound = EnemySound.LowThud;

    /// <summary>Enemy-library cue when the eye finishes opening and requests its beam.</summary>
    internal static readonly SoundEffectId EyeActivationSound = EnemySound.MorphBallEyeRay;

    /// <summary>
    /// The enemy-library silence queued when Samus leaves the active eye's range. It is a real request (#1842): it
    /// interrupts the looping <see cref="EyeActivationSound"/> of the eye's beam.
    /// </summary>
    internal static readonly SoundEffectId EyeRaySoundStop = EnemySound.Silence;

    /// <summary>Enemy-library cue emitted when a live bug's sound countdown reaches zero.</summary>
    internal static readonly SoundEffectId KagoBugSoundEffect = EnemySound.KagoBug;

    /// <summary>Enemy-library landing cue published when the falling top reaches its target.</summary>
    internal static readonly SoundEffectId KzanLandingSoundEffect = EnemySound.SpikePlatformStop;

    /// <summary>The enemy-library sound published when the Rio starts its return.</summary>
    internal static readonly SoundEffectId LowerNorfairRioReturnSound = EnemySound.HoltzCry;

    /// <summary>Enemy-library sound requested by the spinning animation cue.</summary>
    internal static readonly SoundEffectId MamaTurtleSpinSound = EnemySound.TatoriSpinning;

    /// <summary>Enemy-library sound requested on Mama's hover collision with a wall.</summary>
    internal static readonly SoundEffectId MamaTurtleWallSound = EnemySound.SpikePlatformStop;

    /// <summary>Enemy-library sound from the private tail of Oum shot handling.</summary>
    internal static readonly SoundEffectId MaridiaLargeSnailShotSound = EnemySound.DudShotOnDoor;

    /// <summary>Enemy-library splash sound emitted at the water collision.</summary>
    internal static readonly SoundEffectId MaridiaLargeSnailSplashSound = EnemySound.SplashOut;

    /// <summary>Enemy-library sound selected by Metroid's private animation.</summary>
    internal static readonly SoundEffectId MetroidAnimationSoundEffect = EnemySound.MetroidDrain;

    /// <summary>Enemy-library sound queued when an unfrozen Metroid recoils.</summary>
    internal static readonly SoundEffectId MetroidRecoilSoundEffect = EnemySound.ShotMetroid;

    /// <summary>Enemy-library sound emitted when a parent begins its jump.</summary>
    internal static readonly SoundEffectId NorfairLavaJumpingEnemyJumpSound = EnemySound.SplashIn;

    /// <summary>Enemy-library sound queued when a takeoff becomes a dive.</summary>
    internal static readonly SoundEffectId NorfairRioDiveSound = EnemySound.RioCry;

    /// <summary>Enemy-library sound requested when the shared Pirate/Mother Brain laser launches.</summary>
    internal static readonly SoundEffectId PirateMotherBrainLaserSound = EnemySound.Laser;

    /// <summary>Enemy-library sound requested when an on-screen Rio begins its dive.</summary>
    internal static readonly SoundEffectId RioDiveSound = EnemySound.RioCry;

    /// <summary>Enemy-library sound queued when an on-screen shutter begins moving.</summary>
    internal static readonly SoundEffectId ShutterActivationSound = EnemySound.SplashOut;

    /// <summary>Enemy-library cue requested by the wall-jump animation step.</summary>
    internal static readonly SoundEffectId WallPirateJumpSound = EnemySound.ShotSpacePirate;

    /// <summary>Enemy-library cue requested after a successful laser allocation.</summary>
    internal static readonly SoundEffectId WallPirateLaserSound = EnemySound.Laser;

    /// <summary>Enemy-library cue requested by the footstep animation step.</summary>
    internal static readonly SoundEffectId WorkRobotFootstepSound = EnemySound.WorkRobot;

    /// <summary>Enemy-library cue requested after a robot laser projectile is allocated.</summary>
    internal static readonly SoundEffectId WorkRobotLaserSound = EnemySound.Laser;

    /// <summary>Enemy-library attack sound emitted only while the Maw is on screen.</summary>
    internal static readonly SoundEffectId YappingMawAttackSound = EnemySound.YappingMaw;

    /// <summary>Enemy-library sound queued by the Chozo statue's grab cue.</summary>
    internal static readonly SoundEffectId ChozoGrabsSamusSound = EnemySound.ChozoGrab;

    /// <summary>Enemy-library sound queued by the Chozo statue's footstep cue.</summary>
    internal static readonly SoundEffectId ChozoFootstepsSound = EnemySound.ChozoFootsteps;

    /// <summary>Enemy-library small-explosion sound queued by an enemy death explosion's cue.</summary>
    internal static readonly SoundEffectId DeathSmallExplosionSound = EnemySound.SmallExplosion;

    /// <summary>Enemy-library sound queued by an enemy death explosion when contact killed the enemy.</summary>
    internal static readonly SoundEffectId DeathContactKilledSound = EnemySound.ContactKill;

    /// <summary>Enemy-library sound queued as a Metaree or Skree releases its dive.</summary>
    internal static readonly SoundEffectId DiveSound = EnemySound.SkreeDive;

    /// <summary>Enemy-library sound queued as a Metaree or Skree hits the floor and burrows.</summary>
    internal static readonly SoundEffectId BurrowSound = EnemySound.SkreeBurrow;

    /// <summary>Enemy-library sound queued when an airborne Yard bounces off a wall or the floor.</summary>
    internal static readonly SoundEffectId YardBounceSound = EnemySound.YardBounce;

    /// <summary>Enemy-library sound queued when a walking Space Pirate reflects a projectile.</summary>
    internal static readonly SoundEffectId WalkingPirateReflectSound = EnemySound.ShotSpacePirate;
}
