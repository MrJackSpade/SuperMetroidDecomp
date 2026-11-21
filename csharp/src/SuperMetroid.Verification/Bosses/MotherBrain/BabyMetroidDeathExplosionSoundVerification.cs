using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Hud;
namespace SuperMetroid.Verification.Bosses.MotherBrain;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms;

/// <summary>Checks the Baby Metroid's death explosions and Mother Brain's escape effects request their sounds at low priority.</summary>
internal static class BabyMetroidDeathExplosionSoundVerification
{
    /// <summary>
    /// The Baby Metroid's death explosions ($A9:CDB1) request Mother Brain's explosion at low priority ($A9:CDF4). The
    /// cartridge requests library three's $13 there; #2580 merged it into the enemies' identical small explosion, so the
    /// request waits in library two's queue as the rot dust and the door's do.
    /// </summary>
    internal static void VerifyBabyMetroidDeathExplosionSound()
    {
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.NewGame);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.MotherBrain);
        MotherBrainEnemyState state = (runtime.Enemies.BossEncounters.MotherBrain ?? throw new InvalidOperationException("Verification requires runtime.Enemies.BossEncounters.MotherBrain."));
        var step = new BabyMetroidCutsceneStepResult(
            new BabyMetroidCutscenePhase.Flying.TakeFinalBlow(), new BabyMetroidCutscenePhase.Flying.TakeFinalBlow(), BabyDeleted: false,
            LatchSoundQueued: false, ReleaseDustClouds: [],
            AmbientCrySoundQueued: false,
            DeathExplosion: new DustExplosionRequest(new(0x0080, 0x0080), DustExplosionAnimation.SmallExplosion, MotherBrainSoundDefinitions.Explosion),
            BabyFadeImage: null, RecoveryLightsImage: null, StartedAnimation: null, Visibility: BabyMetroidVisibilityRequest.Unchanged);

        runtime.Enemies.Audio.BeginFrame();
        runtime.Enemies.BabyMetroidCutsceneBehavior.ApplyBabyMetroidFrameEffects(state.Colours, step);
        // A runtime's requests go on into its session's queues, which admit them in request order (#2602).
        AssertSequenceEqual(
            new[] { MotherBrainSoundDefinitions.Explosion.SoundEffect },
            runtime.Enemies.Audio.Requests.SoundsQueued, "the explosion queues the enemies' small explosion, and nothing else");

        // Mother Brain's corpse-rot dust ($A9:B252) and escape-door explosion ($A9:B346) both
        // request library-two sounds at low priority.
        var death = default(MotherBrainRainbowBeamAttackStepResult) with
        {
            Effects = [], DeathExplosions = [],
            EscapeColourEffectRequests = [], EscapeDoorParticleSpawns = [],
            CorpseDustRequests = [new MotherBrainCorpseDustRequest(new Position16<RoomPixels>(0x0080, 0x0080), DustExplosionAnimation.CorpseDustCloud, Sound: EnemySound.LiquidDamage)],
            EscapeDoorExplosion = new DustExplosionRequest(new(0x0080, 0x0080), DustExplosionAnimation.SmallExplosion, MotherBrainSoundDefinitions.Explosion),
        };
        runtime.Enemies.Audio.BeginFrame();
        runtime.Enemies.MotherBrainBehavior.RainbowAndDeath.ApplyLiveMotherBrainDeath(state, death);
        AssertSequenceEqual(
            new SoundEffectId[] { EnemySound.LiquidDamage, EnemySound.SmallExplosion },
            runtime.Enemies.Audio.Requests.SoundsQueued, "the rot dust and door explosion queue library-two sounds");
        Console.WriteLine("  Escape explosion sounds: the Baby Metroid's, Mother Brain's dust and the door's in library two.");
    }

    /// <summary>Runs the focused Baby Metroid death-explosion sound verification command.</summary>
    [VerificationCommand("--baby-metroid-death-explosion-sound")]
    internal static int BabyMetroidDeathExplosionSoundCommand()
    {
        Suite(nameof(VerifyBabyMetroidDeathExplosionSound), () => VerifyBabyMetroidDeathExplosionSound());
        return 0;
    }
}
