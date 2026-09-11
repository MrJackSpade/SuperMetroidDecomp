using System.Reflection;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Audio;

internal static partial class Program
{
    private static void VerifyReserveNativeTrace(string path)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        int checkedFrames = 0;
        var sprites = File.ReadLines(path + ".oam.csv").Select(line => line.Split(',').Select(int.Parse).ToArray())
            .ToDictionary(row => (Supply: row[0], Frame: row[1]));
        foreach (var group in File.ReadLines(path).Skip(1).Select(line => line.Split(',').Select(int.Parse).ToArray())
            .GroupBy(row => (Manual: row[0] != 0, Supply: row[1])))
        {
            var samus = new SamusState { Health = (ushort)(group.Key.Manual ? 20 : 0), MaxHealth = 99,
                ReserveEnergy = (ushort)group.Key.Supply, MaxReserveEnergy = 200,
                ReserveTankMode = (ushort)(group.Key.Manual ? 2 : 1), CollectedBeams = (ushort)SamusBeamFlags.Charge };
            var hud = new HudState();
            hud.Initialize(bus, HudSnapshot.CeresDebug with { Health = samus.Health,
                ReserveHealth = samus.ReserveEnergy, ReserveMode = samus.ReserveTankMode });
            var auto = new SamusReserveAutoRecoveryState();
            var audio = new CartridgeAudioState();
            var pause = new PauseMenuState(bus, samus, new Bank80SystemState(), AreaId.Crateria, 0, 0, audio);
            if (group.Key.Manual)
            {
                pause.Step((ushort)SnesButton.R, (ushort)SnesButton.R);
                for (int i = 0; i < 32; i++) pause.Step(0, 0);
                pause.Step(0, (ushort)SnesButton.Up);
                pause.Step(0, (ushort)SnesButton.Down);
            }
            else auto.Begin(samus);
            foreach (int[] row in group)
            {
                int frame = row[2];
                audio.Reset();
                bool autoSound = false;
                if (group.Key.Manual) pause.Step(0, frame == 0 ? (ushort)SnesButton.A : (ushort)0, nmiFrameCounter8: (byte)frame);
                else autoSound = auto.StepAfterNmi(samus, (ushort)frame).RefillSoundRequested;
                hud.UpdateGameplayCounters(bus, samus, timeIsFrozen: true);
                string context = $"native manual={group.Key.Manual} supply={group.Key.Supply} frame={frame}";
                AssertEqual(row[3], samus.Health, context + " health");
                AssertEqual(row[4], samus.ReserveEnergy, context + " supply");
                var positions = (byte[])typeof(CartridgeAudioState).GetField("_soundWritePositions", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(audio)!;
                var queues = (byte[,])typeof(CartridgeAudioState).GetField("_soundQueues", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(audio)!;
                bool manualSound = Enumerable.Range(0, positions[2]).Any(i => queues[2, i] == 0x2d);
                AssertEqual(row[14], (group.Key.Manual ? manualSound : autoSound) ? 0x2d : 0, context + " refill sound request");
                if (group.Key.Manual)
                    AssertEqual(row[5], (ushort)typeof(PauseMenuState).GetField("reserveTransferSoundDelay",
                        BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(pause)!, context + " transfer timer");
                AssertEqual(row[6], hud.Tiles[70], context + " HUD tens");
                AssertEqual(row[7], hud.Tiles[71], context + " HUD ones");
                if (!group.Key.Manual)
                {
                    int[] indices = [8, 9, 40, 41, 72, 73];
                    for (int i = 0; i < indices.Length; i++)
                        AssertEqual(row[8 + i], hud.Tiles[indices[i]], context + " AUTO cell " + i);
                }
                else
                {
                    int[] native = sprites[(group.Key.Supply, frame)];
                    var capture = pause.CaptureRenderSnapshot();
                    // Preserve the separately animated selector. Its transfer-item
                    // art overlaps the strip rectangle; only replace the tank suffix
                    // with independently executed cartridge OAM for this comparison.
                    int firstTank = capture.Memory.ModeledSpriteCount - native[2];
                    AssertTrue(firstTank >= 0, "production includes native tank sprite count");
                    byte[] expectedOam = capture.Memory.Oam.ToArray();
                    for (int sprite = 0; sprite < native[2]; sprite++)
                    {
                        for (int b = 0; b < 4; b++) expectedOam[(firstTank + sprite) * 4 + b] = (byte)native[3 + sprite * 4 + b];
                        int shift = ((firstTank + sprite) % 4) * 2;
                        int high = 512 + (firstTank + sprite) / 4;
                        int bits = (native[3 + 512 + sprite / 4] >> (sprite % 4 * 2)) & 3;
                        expectedOam[high] = (byte)((expectedOam[high] & ~(3 << shift)) | bits << shift);
                    }
                    var memory = new PpuMemorySnapshot(capture.Memory.Vram, capture.Memory.Cgram,
                        expectedOam, capture.Memory.ModeledSpriteCount);
                    var expected = SoftwareLayeredSnapshotRenderer.Render(new(memory, capture.Layers,
                        capture.ObjectSelection, capture.Brightness));
                    var actual = pause.Render();
                    for (int y = 88; y < 120; y++) for (int x = 16; x < 72; x++)
                        AssertEqual(expected[y * 256 + x], actual[y * 256 + x], context + $" tank pixel {x},{y}");
                }
                checkedFrames++;
            }
            AssertEqual(0, samus.ReserveEnergy, "native trace includes exhaustion");
        }
        AssertEqual(8, File.ReadLines(path).Skip(1).Select(line => string.Join(',', line.Split(',').Take(2))).Distinct().Count(), "all eight native refill cases present");
        AssertEqual(402, checkedFrames, "complete native frame coverage");
        AssertEqual(181, sprites.Count, "complete native manual sprite coverage");
        Console.WriteLine($"Original cartridge reserve trace: {checkedFrames} frames of transfer/timer/HUD/refill-sound requests and {sprites.Count} rendered tank frames compared.");
    }
}
