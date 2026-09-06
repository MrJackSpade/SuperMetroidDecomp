using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;

internal static partial class Program
{
    private static void VerifyPauseRenderSnapshots()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var samus = new SamusState
        {
            CollectedItems = (ushort)(SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.Bombs),
            EquippedItems = (ushort)(SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.Bombs),
            MaxReserveEnergy = 100, ReserveEnergy = 47,
            XPosition = 512, YPosition = 768,
        };
        var system = new Bank80SystemState();
        system.SetAreaMapAcquired((byte)AreaId.Maridia);
        system.MarkExploredMapTile((byte)AreaId.Maridia, 30, 5);
        // Seed retained HUD words rather than testing only the cleared menu background.
        var gameplayVram = new SnesVram();
        ushort[] hud = new ushort[128];
        for (int i = 0; i < hud.Length; i++) hud[i] = (ushort)(0x2000 | (i % 64));
        gameplayVram.ExecuteWordTransfer(hud, SnesPpuLayout.GameplayHudTilemapWord, 1);
        var pause = new PauseMenuState(bus, samus, system, AreaId.Maridia, 28, 1,
            gameplayVram: gameplayVram);
        var pages = new HashSet<int>();
        var levels = new HashSet<byte>();
        for (int frame = 0; frame < 128; frame++)
        {
            Rgba32[] expected = pause.Render();
            LayeredRenderSnapshot packet = pause.CaptureRenderSnapshot();
            Match(expected, SoftwareLayeredSnapshotRenderer.Render(packet), frame);
            Match(expected, SoftwareLayeredSnapshotRenderer.Render(packet), frame);
            LayeredRenderSnapshot repeated = pause.CaptureRenderSnapshot();
            AssertTrue(packet.Memory.Oam.SequenceEqual(repeated.Memory.Oam), "repeat pause capture OAM");
            pages.Add(pause.ScreenMode);
            levels.Add(packet.Brightness);
            ushort input = frame == 8 ? (ushort)SnesButton.R
                : frame == 80 ? (ushort)SnesButton.L : (ushort)0;
            pause.Step(input, input);
            Match(expected, SoftwareLayeredSnapshotRenderer.Render(packet), frame);
        }
        AssertEqual(2, pages.Count, "snapshot covers map and equipment");
        AssertEqual(16, levels.Count, "snapshot covers every page-fade brightness");

        // A caller may reuse the command array immediately after publication.
        var captured = pause.CaptureRenderSnapshot();
        RenderLayer[] layers = captured.Layers.ToArray();
        var owned = new LayeredRenderSnapshot(captured.Memory, layers,
            captured.ObjectSelection, captured.Brightness);
        layers[0] = new ObjPriorityRenderLayer(3);
        AssertEqual(captured.Layers[0], owned.Layers[0], "snapshot owns priority sequence");
        AssertThrows<ArgumentException>(() => new LayeredRenderSnapshot(captured.Memory,
            new RenderLayer[] { new ObjPriorityRenderLayer(4) }, 0, 15), "reject invalid OBJ priority");
        Console.WriteLine("  Pause snapshots: 128 retail map/equipment/fade frames match exactly, including retained HUD and packet lifetime.");

        static void Match(Rgba32[] expected, Rgba32[] actual, int frame)
        {
            AssertEqual(expected.Length, actual.Length, "pause snapshot geometry");
            for (int pixel = 0; pixel < expected.Length; pixel++)
                if (expected[pixel] != actual[pixel])
                    throw new InvalidOperationException($"Pause snapshot frame {frame} pixel ({pixel % FrontendFrame.Width},{pixel / FrontendFrame.Width}): expected {expected[pixel]}, got {actual[pixel]}.");
        }
    }
}
