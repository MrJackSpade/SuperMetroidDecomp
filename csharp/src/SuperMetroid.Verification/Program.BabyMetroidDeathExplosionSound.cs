using System.Reflection;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    /// <summary>
    /// The Baby Metroid's death explosions ($A9:CDB1) queue library-three $13 through
    /// QueueSound_Lib3_Max3 ($A9:CDF4). The port queued it in library two, so its library-two
    /// ring filled while native's library three drained; in the 13% movie that backlog
    /// lengthened the escape door's sound wait.
    /// </summary>
    private static void VerifyBabyMetroidDeathExplosionSound()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(FixtureRoomHeaders.MotherBrain);
        MotherBrainEnemyState state = runtime.Enemies.MotherBrain!;
        var step = new BabyMetroidCutsceneStepResult(
            BabyMetroidCutscenePhase.TakeFinalBlow, BabyMetroidCutscenePhase.TakeFinalBlow,
            BodyStumbleRequested: false, LatchSoundQueued: false, ReleaseDustClouds: [],
            AmbientCrySoundQueued: false,
            DeathExplosion: new BabyMetroidDeathExplosionRequest(0x0080, 0x0080, 3, 0x0013),
            BabyPaletteTransfer: null, AttackTileTransfer: null, BackgroundPaletteTransfer: null);

        typeof(RoomEnemySystem).GetMethod("ApplyBabyMetroidFrameEffects", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(runtime.Enemies, [state, step]);
        AssertEqual((ushort?)0x0013, state.LastSoundEffectLibrary3, "the explosion queues library-three $13");
        AssertEqual((ushort?)null, state.LastSoundEffect, "and nothing in library two");

        // Mother Brain's corpse-rot dust ($A9:B252) and escape-door explosion ($A9:B346) both
        // queue through QueueSound_Lib2_Max3.
        var death = default(MotherBrainRainbowBeamAttackStepResult) with
        {
            DeathExplosions = [], CorpseRottingVramTransfers = [], EscapeSequenceTileTransfers = [],
            EscapePaletteFxRequests = [], EscapeDoorParticleSpawns = [],
            CorpseDustRequests = [new MotherBrainCorpseDustRequest(0x0080, 0x0080, 0x000a, SoundEffectQueued: true, SoundEffect: 0x0010)],
            EscapeDoorExplosion = new MotherBrainEscapeDoorExplosionRequest(0x0080, 0x0080, 0x0003, 0x0024),
        };
        state.LastSoundEffectLibrary3 = null;
        typeof(RoomEnemySystem).GetMethod("ApplyLiveMotherBrainDeath", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(runtime.Enemies, [state, new MotherBrainRainbowBeamAttackSequence(), death]);
        AssertEqual((ushort?)null, state.LastSoundEffectLibrary3, "neither sound goes to library three");
        AssertSequenceEqual(
            new[]
            {
                new EnemySoundRequest(SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, 0x10), 3),
                new EnemySoundRequest(SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, 0x24), 3),
            },
            runtime.Enemies.SoundRequests, "the rot dust and door explosion queue in library two with Max3");
        Console.WriteLine("  Escape explosion sounds: Baby Metroid's in library three, Mother Brain's dust and door in library two.");
    }
}
