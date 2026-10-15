namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The cartridge sound effect ids ordinary enemies queue, by enemy and event. Each value is the id within the library its queuing call names.
/// </summary>
internal static class EnemySoundEffectDefinitions
{
    /// <summary>Library-two sound queued when Alcoon emerges near Samus.</summary>
    internal const ushort AlcoonEmergeSound = 0x005e;

    /// <summary>Library-two sound queued by each fireball instruction.</summary>
    internal const ushort AlcoonFireSound = 0x003f;

    /// <summary>Library-two cue replacing the impact cue when the boulder breaks into dust.</summary>
    internal const ushort BoulderBreakSound = 0x0043;

    /// <summary>Library-two cue published when a falling boulder touches ground.</summary>
    internal const ushort BoulderImpactSound = 0x0042;

    /// <summary>Library-two sound request emitted by Boyon's start-bounce instruction.</summary>
    internal const ushort BoyonBounceSound = 0x000e;

    /// <summary>Library-two request emitted by the spike-release animation command.</summary>
    internal const ushort CacatacSpikeSound = 0x0034;

    /// <summary>Library-two sound queued when Samus wakes the tutorial Dachora.</summary>
    internal const ushort DachoraActivationSound = 0x001d;

    /// <summary>Library-two sound queued on Dachora's ceiling contact.</summary>
    internal const ushort DachoraCeilingImpactSound = 0x003c;

    /// <summary>Library-two sound queued while Dachora stores its shinespark.</summary>
    internal const ushort DachoraChargeSound = 0x003d;

    /// <summary>Library-two sound queued when Dachora launches upward.</summary>
    internal const ushort DachoraLaunchSound = 0x003b;

    /// <summary>Library-two sound accompanying Dachora's Speed Booster run.</summary>
    internal const ushort DachoraSpeedBoosterSound = 0x0039;

    /// <summary>Library-two sound queued at the scripted left wall turn.</summary>
    internal const ushort DachoraWallImpactSound = 0x0071;

    /// <summary>Library-two cue published by a Dragon fireball volley.</summary>
    internal const ushort DragonFireballSound = 0x0061;

    /// <summary>Library-three selector $25 plays when arrival reaches the resting height.</summary>
    internal const ushort ElevatorArrivalSoundLibrary3 = 0x0025;

    /// <summary>Library-one selector $32 accompanies departure.</summary>
    internal const ushort ElevatorDepartureSoundLibrary1 = 0x0032;

    /// <summary>Library-three selector $0B accompanies departure.</summary>
    internal const ushort ElevatorDepartureSoundLibrary3 = 0x000b;

    /// <summary>Library-two sound queued at an audible teaching hop.</summary>
    internal const ushort EtecoonJumpSound = 0x0033;

    /// <summary>Library-two sound queued when an Etecoon wakes near Samus.</summary>
    internal const ushort EtecoonWakeSound = 0x0035;

    /// <summary>Library-two sound queued when an airborne Etecoon meets a wall.</summary>
    internal const ushort EtecoonWallSound = 0x0032;

    /// <summary>Library-two sound emitted by the spit instruction.</summary>
    internal const ushort EvirSpitSound = 0x005e;

    /// <summary>Library-two cue when the eye finishes opening and requests its beam.</summary>
    internal const ushort EyeActivationSound = 0x0017;

    /// <summary>Library-two cue when Samus leaves the active eye's range.</summary>
    internal const ushort EyeDeactivationSound = 0x0071;

    /// <summary>Library-two cue emitted when a live bug's sound countdown reaches zero.</summary>
    internal const ushort KagoBugSoundEffect = 0x006c;

    /// <summary>Library-two landing cue published when the falling top reaches its target.</summary>
    internal const ushort KzanLandingSoundEffect = 0x001b;

    /// <summary>Library-two selector $64 is published when the Rio starts its return.</summary>
    internal const ushort LowerNorfairRioReturnSound = 0x0064;

    /// <summary>Library-two sound requested by the spinning animation instruction.</summary>
    internal const ushort MamaTurtleSpinSound = 0x003a;

    /// <summary>Library-two sound requested on Mama's hover collision with a wall.</summary>
    internal const ushort MamaTurtleWallSound = 0x001b;

    /// <summary>Library-two sound from the private tail of Oum shot handling.</summary>
    internal const ushort MaridiaLargeSnailShotSound = 0x0057;

    /// <summary>Library-two splash sound emitted at the water collision.</summary>
    internal const ushort MaridiaLargeSnailSplashSound = 0x000e;

    /// <summary>Library-two sound selected by Metroid's private animation bytecode.</summary>
    internal const ushort MetroidAnimationSoundEffect = 0x0050;

    /// <summary>Library-three ice sound queued when an unfrozen Metroid takes an Ice shot.</summary>
    internal const ushort MetroidIceSoundEffect = 0x000a;

    /// <summary>Library-two sound queued when an unfrozen Metroid recoils.</summary>
    internal const ushort MetroidRecoilSoundEffect = 0x005a;

    /// <summary>Library-two sound emitted when a parent begins its jump.</summary>
    internal const ushort NorfairLavaJumpingEnemyJumpSound = 0x000d;

    /// <summary>Library-two sound queued when a takeoff becomes a dive.</summary>
    internal const ushort NorfairRioDiveSound = 0x0065;

    /// <summary>Library-two sound requested when the shared Pirate/Mother Brain laser launches.</summary>
    internal const ushort PirateMotherBrainLaserSound = 0x0067;

    /// <summary>Library-two sound requested when an on-screen Rio begins its dive.</summary>
    internal const ushort RioDiveSound = 0x0065;

    /// <summary>Library-two sound queued when an on-screen shutter begins moving.</summary>
    internal const ushort ShutterActivationSound = 0x000e;

    /// <summary>Library-two cue requested by the wall-jump opcode.</summary>
    internal const ushort WallPirateJumpSound = 0x0066;

    /// <summary>Library-two cue requested after a successful laser allocation.</summary>
    internal const ushort WallPirateLaserSound = 0x0067;

    /// <summary>Library-two cue requested by the footstep opcode.</summary>
    internal const ushort WorkRobotFootstepSound = 0x0068;

    /// <summary>Library-two cue requested after a robot laser projectile is allocated.</summary>
    internal const ushort WorkRobotLaserSound = 0x0067;

    /// <summary>Library-two attack sound emitted only while the Maw is on screen.</summary>
    internal const ushort YappingMawAttackSound = 0x002f;

    /// <summary>Library-two sound queued by the Chozo statue's grab instruction.</summary>
    internal const ushort ChozoGrabsSamusSound = 0x001c;

    /// <summary>Library-two sound queued by the Chozo statue's footstep instruction.</summary>
    internal const ushort ChozoFootstepsSound = 0x004b;

    /// <summary>Library-two small-explosion sound queued by an enemy death explosion's instruction.</summary>
    internal const ushort DeathSmallExplosionSound = 0x0024;

    /// <summary>Library-two sound queued by an enemy death explosion when contact killed the enemy.</summary>
    internal const ushort DeathContactKilledSound = 0x000b;
}
