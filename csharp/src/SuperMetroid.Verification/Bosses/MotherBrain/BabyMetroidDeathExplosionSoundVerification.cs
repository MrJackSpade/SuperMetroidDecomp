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
using SuperMetroid.Core.Audio;

/// <summary>Checks Baby Metroid death sounds and Mother Brain escape effects use their native sound libraries.</summary>
internal static class BabyMetroidDeathExplosionSoundVerification
{
    /// <summary>
    /// The Baby Metroid's death explosions ($A9:CDB1) queue library-three $13 through
    /// QueueSound_Lib3_Max3 ($A9:CDF4). The port queued it in library two, so its library-two
    /// ring filled while native's library three drained; in the 13% movie that backlog
    /// lengthened the escape door's sound wait.
    /// </summary>
    internal static void VerifyBabyMetroidDeathExplosionSound()
    {
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.MotherBrain);
        MotherBrainEnemyState state = (runtime.Enemies.MotherBrainEncounter.MotherBrain ?? throw new InvalidOperationException("Verification requires runtime.Enemies.MotherBrainEncounter.MotherBrain."));
        var step = new BabyMetroidCutsceneStepResult(
            BabyMetroidCutscenePhase.TakeFinalBlow, BabyMetroidCutscenePhase.TakeFinalBlow,
            BodyStumbleRequested: false, LatchSoundQueued: false, ReleaseDustClouds: [],
            AmbientCrySoundQueued: false,
            DeathExplosion: new DustExplosionRequest(new(0x0080, 0x0080), MiscDustAnimation.SmallExplosion, MotherBrainSoundDefinitions.Explosion),
            BabyFadeShade: null, AttackPage: null, RecoveryLightsFrame: null);

        runtime.Enemies.BabyMetroidCutsceneBehavior.ApplyBabyMetroidFrameEffects((state), step);
        AssertEqual((SoundEffectId?)MotherBrainSoundDefinitions.Explosion, state.LastSoundEffectLibrary3, "the explosion queues library-three $13");
        AssertEqual(null, state.LastSoundEffect, "and nothing in library two");

        // Mother Brain's corpse-rot dust ($A9:B252) and escape-door explosion ($A9:B346) both
        // queue through QueueSound_Lib2_Max3.
        var death = default(MotherBrainRainbowBeamAttackStepResult) with
        {
            DeathExplosions = [], EscapeSequencePages = [],
            EscapePaletteFxRequests = [], EscapeDoorParticleSpawns = [],
            CorpseDustRequests = [new MotherBrainCorpseDustRequest(new Position16<RoomPixels>(0x0080, 0x0080), MiscDustAnimation.CorpseDustCloud, SoundEffectQueued: true, SoundEffect: Library2Sound.LiquidDamage)],
            EscapeDoorExplosion = new DustExplosionRequest(new(0x0080, 0x0080), MiscDustAnimation.SmallExplosion, Library2Sound.SmallExplosion),
        };
        state.LastSoundEffectLibrary3 = null;
        runtime.Enemies.MotherBrainBehavior.RainbowAndDeath.ApplyLiveMotherBrainDeath(state, new MotherBrainRainbowBeamAttackSequence(), death);
        AssertEqual(null, state.LastSoundEffectLibrary3, "neither sound goes to library three");
        AssertSequenceEqual(
            new[]
            {
                new SoundRequest(Library2Sound.LiquidDamage, 3),
                new SoundRequest(Library2Sound.SmallExplosion, 3),
            },
            runtime.Enemies.Audio.SoundRequests, "the rot dust and door explosion queue in library two with Max3");
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
