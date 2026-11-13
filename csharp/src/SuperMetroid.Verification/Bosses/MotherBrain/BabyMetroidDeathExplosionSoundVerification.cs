using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Samus.Projectiles;
namespace SuperMetroid.Verification.Bosses.MotherBrain;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies;

/// <summary>Checks Baby Metroid death sounds and Mother Brain escape effects use their native sound libraries.</summary>
internal static class BabyMetroidDeathExplosionSoundVerification
{
    /// <summary>
    /// The Baby Metroid's death explosions ($A9:CDB1) request library-three $13 at low priority
    /// ($A9:CDF4). The port queued it in library two, so its library-two
    /// ring filled while native's library three drained; in the 13% movie that backlog
    /// lengthened the escape door's sound wait.
    /// </summary>
    internal static void VerifyBabyMetroidDeathExplosionSound()
    {
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.MotherBrain);
        MotherBrainEnemyState state = (runtime.Enemies.MotherBrainEncounter.MotherBrain ?? throw new InvalidOperationException("Verification requires runtime.Enemies.MotherBrainEncounter.MotherBrain."));
        var step = new BabyMetroidCutsceneStepResult(
            BabyMetroidCutscenePhase.TakeFinalBlow, BabyMetroidCutscenePhase.TakeFinalBlow,
            BodyStumbleRequested: false, LatchSoundQueued: false, ReleaseDustClouds: [],
            AmbientCrySoundQueued: false,
            DeathExplosion: new DustExplosionRequest(new(0x0080, 0x0080), MiscDustAnimation.SmallExplosion, MotherBrainSoundDefinitions.Explosion),
            BabyFadeShade: null, RecoveryLightsFrame: null);

        runtime.Enemies.Audio.BeginFrame();
        runtime.Enemies.BabyMetroidCutsceneBehavior.ApplyBabyMetroidFrameEffects(step);
        AssertSequenceEqual(
            new[] { new SoundRequest(MotherBrainSoundDefinitions.Explosion, SoundRequestPriority.Low) },
            runtime.Enemies.Audio.SoundRequests, "the explosion queues library-three $13 at low priority, and nothing else");

        // Mother Brain's corpse-rot dust ($A9:B252) and escape-door explosion ($A9:B346) both
        // request library-two sounds at low priority.
        var death = default(MotherBrainRainbowBeamAttackStepResult) with
        {
            DeathExplosions = [],
            EscapePaletteFxRequests = [], EscapeDoorParticleSpawns = [],
            CorpseDustRequests = [new MotherBrainCorpseDustRequest(new Position16<RoomPixels>(0x0080, 0x0080), MiscDustAnimation.CorpseDustCloud, SoundEffectQueued: true, SoundEffect: EnemySound.LiquidDamage)],
            EscapeDoorExplosion = new DustExplosionRequest(new(0x0080, 0x0080), MiscDustAnimation.SmallExplosion, EnemySound.SmallExplosion),
        };
        runtime.Enemies.Audio.BeginFrame();
        runtime.Enemies.MotherBrainBehavior.RainbowAndDeath.ApplyLiveMotherBrainDeath(state, new MotherBrainRainbowBeamAttackSequence(), death);
        AssertSequenceEqual(
            new[]
            {
                new SoundRequest(EnemySound.LiquidDamage, SoundRequestPriority.Low),
                new SoundRequest(EnemySound.SmallExplosion, SoundRequestPriority.Low),
            },
            runtime.Enemies.Audio.SoundRequests, "the rot dust and door explosion request library-two sounds at low priority");
        Console.WriteLine("  Escape explosion sounds: Baby Metroid's in library three, Mother Brain's dust and door in library two.");
    }

    /// <summary>Runs the focused Baby Metroid death-explosion sound verification command.</summary>
    [VerificationCommand("--baby-metroid-death-explosion-sound")]
    internal static int BabyMetroidDeathExplosionSoundCommand()
    {
        Suite(nameof(VerifyBabyMetroidDeathExplosionSound), () => VerifyBabyMetroidDeathExplosionSound());
        return 0;
    }
}
