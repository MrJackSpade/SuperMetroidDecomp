using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

/// <summary>Matched room-data/position/RNG stimulus for Mini-Kraid attack cadence (#518).</summary>
internal static class FakeKraidCadenceAudit
{
    public static int Run(string rom, string directory, string? nativeCsv = null)
    {
        Directory.CreateDirectory(directory);
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        var room = CartridgeRoomHeader.Load(bus, FakeKraidCadenceDefinitions.RoomPointer);
        var assets = CartridgeRoomAssets.Load(bus, room);
        using (var data = new BinaryWriter(File.Create(Path.Combine(directory, "level.bin"))))
        {
            data.Write((ushort)assets.LevelData.WidthInBlocks);
            data.Write((ushort)assets.LevelData.HeightInBlocks);
            foreach (ushort word in assets.LevelData.ForegroundEntries.Span) data.Write(word);
            data.Write(assets.LevelData.BehaviorBytes.Span);
        }
        using var trace = new StreamWriter(Path.Combine(directory, "managed.csv"));
        trace.WriteLine("side,frame,random,x,walk,facing,walktimer,spittimer,clock0,clock1,clock2,selector,instruction,map,timer,spikes,spit");
        for (int side = 0; side < 2; side++)
        {
            ushort random = 9;
            var samus = new SamusState { XPosition = (ushort)(side == 0 ? 0x500 : 0x560),
                YPosition = 0, Health = 999, MaxHealth = 999, InvincibilityTimer = ushort.MaxValue,
                Pose = SamusPoseIds.FacingRightNormalPose };
            samus.RefreshCollisionRadii(bus);
            samus.InitializeAnimation(bus);
            var enemies = new RoomEnemySystem();
            enemies.Load(bus, room.State.EnemyPopulationPointer, room.State.EnemyTilesetPointer,
                new SnesVram(), new SnesCgram(), () => random, readRandomNumber: () => random,
                level: assets.LevelData, samus: samus);
            var actor = enemies.Slots.Single(slot => slot.EnemyDefinitionPointer == FakeKraidCadenceDefinitions.EnemyDefinition);
            foreach (var other in enemies.Slots)
                if (other.SlotIndex != actor.SlotIndex) other.Properties = other.Properties.With(EnemyProperties.Deleted);
            var state = enemies.FakeKraidStates[actor.SlotIndex]!;
            var spikeFrames = new List<int>();
            var spitFrames = new List<int>();
            for (int frame = 0; frame < 2400; frame++)
            {
                // Both implementations receive identical changing random words. This
                // isolates cadence from unrelated global RNG consumers, not a player replay.
                random = unchecked((ushort)(9 + frame * 37));
                int previousSpikes = state.SpawnedSpikeCount, previousSpit = state.SpawnedSpitCount;
                enemies.StepFrame(0x500, 0, false, samus, level: assets.LevelData);
                int spikes = state.SpawnedSpikeCount - previousSpikes, spit = state.SpawnedSpitCount - previousSpit;
                if (spikes != 0) spikeFrames.Add(frame);
                if (spit != 0) spitFrames.Add(frame);
                trace.WriteLine($"{side},{frame},{random},{actor.XPosition},{state.WalkDelta},{state.FacingDelta}," +
                    $"{state.WalkStepTimer},{state.SpitDecisionTimer},{string.Join(',', state.SpikeTimers)}," +
                    $"{state.SpikeTimerByteOffset},{actor.CurrentInstruction:X4},{actor.SpritemapPointer:X4}," +
                    $"{actor.InstructionTimer},{spikes},{spit}");
                // Keep the actual projectile lifetime/allocation path. Samus is outside
                // their trajectories; projectile collision against her is excluded in both.
                enemies.StepEnemyProjectileInstructions(assets.LevelData, null, 0x500, 0);
            }
            if (spikeFrames.Count < 10 || spitFrames.Count < 4)
                throw new InvalidDataException("Mini-Kraid cadence fixture did not span enough attacks.");
            Console.WriteLine($"Mini-Kraid side {side}: {spikeFrames.Count} spikes, {spitFrames.Count} spit pairs; " +
                $"spike intervals {string.Join(',', spikeFrames.Zip(spikeFrames.Skip(1), (a,b) => b-a).Distinct().Order())}; " +
                $"spit frames {string.Join(',', spitFrames)}.");
        }
        // Complete the capture before opening it for the native comparison on Windows.
        trace.Dispose();
        if (nativeCsv is not null)
        {
            byte[] nativeBytes = File.ReadAllBytes(nativeCsv);
            if (Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(nativeBytes)) !=
                FakeKraidCadenceDefinitions.NativeTraceSha256)
                throw new InvalidDataException("Unrecognized original-CPU Mini-Kraid cadence trace.");
            string[] expected = File.ReadAllLines(nativeCsv);
            string[] actual = File.ReadAllLines(Path.Combine(directory, "managed.csv"));
            if (expected.Length != actual.Length)
                throw new InvalidDataException("Mini-Kraid cadence trace length differs from original CPU.");
            for (int row = 0; row < expected.Length; row++)
                if (expected[row] != actual[row])
                    throw new InvalidDataException($"Mini-Kraid cadence differs at row {row}: " +
                        $"expected '{expected[row]}', actual '{actual[row]}'.");
            Console.WriteLine($"Original CPU matches every cadence/state row across {actual.Length - 1} frames.");
        }
        return 0;
    }
}
