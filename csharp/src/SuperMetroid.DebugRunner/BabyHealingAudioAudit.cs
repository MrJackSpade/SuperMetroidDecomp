using System.Reflection;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

/// <summary>Isolates the real Baby main-AI owner and cartridge healing-SFX boundary for #616.</summary>
internal static class BabyHealingAudioAudit
{
    public static int Run(string rom)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        var room = CartridgeRoomHeader.Load(bus, 0xdd58);
        var assets = CartridgeRoomAssets.Load(bus, room);
        const BindingFlags hidden = BindingFlags.Instance | BindingFlags.NonPublic;
        int cases = 0;
        foreach (ushort health in new ushort[] { 79, 80, 81, 98 })
        for (ushort clock = 0; clock < 16; clock++)
        foreach (bool suppressed in new[] { false, true })
        {
            var vram = new SnesVram();
            var cgram = new SnesCgram();
            assets.LoadGraphics(vram, cgram);
            var random = new Bank80SystemState(0x1234);
            var samus = new SamusState { Health = health, MaxHealth = 99, XPosition = 128,
                YPosition = 160, Pose = SamusPoseIds.FacingRightNormalPose };
            var enemies = new RoomEnemySystem();
            enemies.Load(bus, room.State.EnemyPopulationPointer, room.State.EnemyTilesetPointer,
                vram, cgram, random.NextRandom, random.SetRandomNumber,
                readRandomNumber: () => random.RandomNumber, level: assets.LevelData,
                samus: samus, isAreaBossDefeated: () => false, hasEvent: _ => false);
            var state = enemies.MotherBrain!;
            state.RainbowBeamSequence = new MotherBrainRainbowBeamAttackSequence();
            typeof(MotherBrainRainbowBeamAttackSequence).GetProperty(nameof(state.RainbowBeamSequence.Phase))!
                .SetValue(state.RainbowBeamSequence, MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidTransitionToGrey);
            typeof(RoomEnemySystem).GetMethod("SpawnMotherBrainBabyMetroid", hidden)!
                .Invoke(enemies, [state]);
            var baby = state.BabyMetroid!;
            typeof(BabyMetroidCutsceneState).GetProperty(nameof(baby.Phase))!
                .SetValue(baby, BabyMetroidCutscenePhase.HealSamusToFullHealth);
            typeof(RoomEnemySystem).GetField("_randomEnemyCounter", hidden)!.SetValue(enemies, clock);
            var explosion = new SamusPowerBombExplosionState();
            if (suppressed)
            {
                explosion.Arm();
                explosion.Spawn(samus.XPosition, samus.YPosition);
            }
            typeof(RoomEnemySystem).GetField("_audioPowerBomb", hidden)!.SetValue(enemies, explosion);
            typeof(RoomEnemySystem).GetMethod("RunMotherBrainBabyMetroidMain", hidden)!
                .Invoke(enemies, [state.BabyMetroidSlot, samus, (ushort)0, (ushort)0]);
            var ticks = enemies.SoundRequests.Where(r => r.SoundEffect.Library == SoundEffectLibrary.Library3 &&
                r.SoundEffect.Value == 0x2d).ToArray();
            int expected = health + 1 >= 81 && (clock & 7) == 0 ? 1 : 0;
            if (samus.Health != health + 1 || ticks.Length != expected ||
                ticks.Any(r => r.MaximumQueued != 3 || r.SoundSuppressed != suppressed))
                throw new InvalidDataException($"Healing tick health={health}->{samus.Health}, clock={clock}: expected {expected}, got {ticks.Length}.");
            cases++;
        }
        Console.WriteLine($"Baby healing audio: {cases} health/clock/suppression cases passed, including final energy point.");
        return 0;
    }
}
