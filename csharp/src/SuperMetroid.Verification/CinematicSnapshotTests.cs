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
        VerifyCeresExplosionTimeline();
        VerifyIntroDisplayCapture();
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var flight = new IntroCeresFlightState(bus);
        var flightPhases = new HashSet<IntroCeresFlightPhase>();
        int flightSamples = 0;
        bool coloredRearView = false;
        for (int tick = 0; tick < 4000 && !flight.Finished; tick++)
        {
            bool changed = flightPhases.Add(flight.Phase);
            if (changed || tick % 7 == 0)
            {
                Rgba32[] expected = flight.Render();
                LayeredRenderSnapshot snapshot = flight.CaptureRenderSnapshot();
                coloredRearView |= snapshot.Layers.ToArray().Any(layer => layer is FixedColorAddRenderLayer c
                    && (c.Red != 0 || c.Green != 0 || c.Blue != 0));
                Compare(expected, snapshot, tick);
                flight.Step();
                Compare(expected, snapshot, tick);
                flightSamples++;
            }
            else flight.Step();
        }
        AssertTrue(flight.Finished && coloredRearView, "flight fixture completes and exercises nonzero fixed color");
        AssertTrue(flightPhases.Contains(IntroCeresFlightPhase.SpaceColonyTitle), "flight caption captured");
        Console.WriteLine($"  Ceres flight snapshots: {flightSamples} samples across {flightPhases.Count} phases match, including fixed-color rear view.");
        var destruction = new CeresDestructionCinematicState(bus);
        var phases = new HashSet<CeresDestructionPhase>();
        int samples = 0;
        bool mode1 = false, mode7 = false, explosionWindow = false, explosionCoversTop = false;
        for (int tick = 0; tick < 2000 && !destruction.Finished; tick++)
        {
            bool changed = phases.Add(destruction.Phase);
            if (changed || tick % 13 == 0)
            {
                Rgba32[] expected = destruction.Render();
                LayeredRenderSnapshot snapshot = destruction.CaptureRenderSnapshot();
                mode7 |= snapshot.Layers.ToArray().Any(layer => layer is Mode7RenderLayer);
                mode1 |= snapshot.Layers.ToArray().Any(layer => layer is Bg4BppRenderLayer);
                foreach (var window in snapshot.Layers.ToArray().OfType<ScanlineColorAddRenderLayer>())
                {
                    explosionWindow |= window.Windows.ToArray().Any(row =>
                        row.Left <= row.Right && (row.Red != 0 || row.Green != 0 || row.Blue != 0));
                    ColorAddWindow top = window.Windows[0];
                    explosionCoversTop |= top.Left <= top.Right &&
                        (top.Red != 0 || top.Green != 0 || top.Blue != 0);
                }
                Compare(expected, snapshot, tick);
                destruction.Step();
                Compare(expected, snapshot, tick);
                samples++;
            }
            else destruction.Step();
        }
        AssertTrue(destruction.Finished, "destruction snapshot fixture completes");
        AssertTrue(mode1 && mode7, "destruction captures both PPU modes");
        AssertTrue(explosionWindow, "Ceres destruction publishes the cartridge power-bomb color window");
        AssertTrue(explosionCoversTop, "Ceres explosion covers the top of the cinematic, without a gameplay HUD exclusion");
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

    private static void VerifyIntroDisplayCapture()
    {
        byte[] rom = File.ReadAllBytes(Path.GetFullPath("Super Metroid.smc"));
        var legacy = new IntroCinematicState(new SuperMetroidAddressSpace(rom));
        var captured = new IntroCinematicState(new SuperMetroidAddressSpace(rom));
        var phases = new HashSet<IntroCinematicPhase>();
        int samples = 0;
        for (int tick = 0; tick < 20000 && !legacy.CeresFlightFinished; tick++)
        {
            // Independent owners are essential: legacy draw advances projectile trails.
            // Calling both producers on one scene would run those side effects twice.
            Rgba32[] expected = legacy.Render();
            LayeredRenderSnapshot snapshot = captured.CaptureTranslatedRenderSnapshot();
            bool newPhase = phases.Add(legacy.Phase);
            bool sample = newPhase || tick % 31 == 0;
            if (sample)
            {
                RenderFrameSnapshot packet = RoundTripRenderPacket(new(new(tick + 1, 1, (ushort)tick), snapshot));
                AssertTrue(expected.AsSpan().SequenceEqual(SoftwareFrameSnapshotRenderer.Render(packet)),
                    $"intro captured pixels: {legacy.Phase}, tick {tick}");
                samples++;
            }
            ushort input = tick % 47 == 0 ? (ushort)SnesButton.A : (ushort)0;
            legacy.Step(input);
            captured.Step(input);
            AssertEqual(legacy.Phase, captured.Phase, "intro phase after draw");
            AssertEqual(legacy.MotherBrainHitCount, captured.MotherBrainHitCount, "intro hits after draw");
            AssertEqual(legacy.FlashbackSamusX, captured.FlashbackSamusX, "intro Samus X after draw");
            AssertEqual(legacy.FlashbackSamusY, captured.FlashbackSamusY, "intro Samus Y after draw");
            AssertEqual(legacy.ActiveFlashbackProjectileCount, captured.ActiveFlashbackProjectileCount, "intro projectiles after draw");
            if (sample)
                AssertTrue(expected.AsSpan().SequenceEqual(SoftwareLayeredSnapshotRenderer.Render(snapshot)),
                    "intro packet survives subsequent simulation mutations");
        }
        AssertTrue(legacy.CeresFlightFinished && captured.CeresFlightFinished, "full intro capture reaches completed flight");
        AssertTrue(phases.Contains(IntroCinematicPhase.MotherBrainFlashback)
            && phases.Contains(IntroCinematicPhase.BabyDiscovery)
            && phases.Contains(IntroCinematicPhase.BabyMetroidExamination), "full intro covers each illustration family");
        Console.WriteLine($"  Intro snapshots: {samples} sampled frames across {phases.Count} phases agree with independent legacy display owner.");
    }
}
