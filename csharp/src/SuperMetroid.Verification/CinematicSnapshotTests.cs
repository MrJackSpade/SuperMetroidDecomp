using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;

internal static partial class Program
{
    private static void VerifyCinematicRenderSnapshots()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var destruction = new CeresDestructionCinematicState(bus);
        var phases = new HashSet<CeresDestructionPhase>();
        int samples = 0;
        bool mode1 = false, mode7 = false;
        for (int tick = 0; tick < 2000 && !destruction.Finished; tick++)
        {
            bool changed = phases.Add(destruction.Phase);
            if (changed || tick % 13 == 0)
            {
                Rgba32[] expected = destruction.Render();
                LayeredRenderSnapshot snapshot = destruction.CaptureRenderSnapshot();
                mode7 |= snapshot.Layers.ToArray().Any(layer => layer is Mode7RenderLayer);
                mode1 |= snapshot.Layers.ToArray().Any(layer => layer is Bg4BppRenderLayer);
                Compare(expected, snapshot, tick);
                destruction.Step();
                Compare(expected, snapshot, tick);
                samples++;
            }
            else destruction.Step();
        }
        AssertTrue(destruction.Finished, "destruction snapshot fixture completes");
        AssertTrue(mode1 && mode7, "destruction captures both PPU modes");
        int menuSamples = 0;
        foreach (bool chooseNo in new[] { false, true })
        {
            var audio = new CartridgeAudioState();
            var menu = new GameOverMenuState(bus, audio);
            bool toggled = false, released = false;
            for (int tick = 0; tick < 300 && !menu.ContinueRequested && !menu.TitleRequested; tick++)
            {
                Rgba32[] expected = menu.Render();
                LayeredRenderSnapshot snapshot = menu.CaptureRenderSnapshot();
                Compare(expected, snapshot, tick);
                ushort input = 0;
                if (menu.Phase == GameOverMenuPhase.Main)
                {
                    if (chooseNo && !toggled) { input = (ushort)SnesButton.Down; toggled = true; }
                    else if (!released) released = true;
                    else input = (ushort)SnesButton.A;
                }
                menu.Step(input);
                audio.AdvanceFrame(bus, default);
                Compare(expected, snapshot, tick);
                menuSamples++;
            }
            AssertEqual(chooseNo, menu.TitleRequested, "game-over no outcome");
            AssertEqual(!chooseNo, menu.ContinueRequested, "game-over yes outcome");
        }
        Console.WriteLine($"  Cinematic snapshots: {samples} Ceres/Zebes frames ({phases.Count} phases) and {menuSamples} game-over frames match, including packet round trips.");

        static void Compare(Rgba32[] expected, LayeredRenderSnapshot snapshot, int tick)
        {
            RenderFrameSnapshot roundTrip = RoundTripRenderPacket(new(new(tick + 1, 1, (ushort)tick), snapshot));
            Rgba32[] pixels = SoftwareFrameSnapshotRenderer.Render(roundTrip);
            AssertEqual(expected.Length, pixels.Length, "cinematic snapshot dimensions");
            for (int i = 0; i < pixels.Length; i++)
                if (pixels[i] != expected[i])
                    throw new InvalidOperationException($"Cinematic snapshot tick {tick}, pixel ({i % FrontendFrame.Width},{i / FrontendFrame.Width}): {expected[i]} != {pixels[i]}.");
        }
    }
}
