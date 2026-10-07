using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Rendering.Direct3D11;
using SuperMetroid.Desktop;

internal static class EndingShootingStarsTests
{
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        var rom = CartridgeImportAddressSpace.LoadRetailRom("Super Metroid.smc");
        for (int index = 0; index < 40; index++)
        {
            var definition = EndingShootingStarDefinitions.Records[index];
            Require(unchecked((ushort)definition.XAcceleration) == Word(0x8be9cf + index * 8), "Native X acceleration");
            Require(unchecked((ushort)definition.YAcceleration) == Word(0x8be9d1 + index * 8), "Native Y acceleration");
            Require(definition.Period == Word(0x8be9d3 + index * 8), "Native animation period");
            Require(definition.Delay == Word(0x8be9d5 + index * 8), "Native start delay");
        }
        for (int frame = 0; frame < 20; frame++)
            Require(EndingShootingStarDefinitions.Attributes[frame] == Word(0x8be9a7 + frame * 2), "Native OBJ attributes");
        var stars = new EndingShootingStars();
        for (int frame = 1; frame <= 68; frame++)
        {
            stars.Step();
            var star = stars.Stars[1];
            if (frame < 68)
            {
                // Closed-form sums: native star 1 accelerates once until frame 38,
                // then twice. This independently checks fixed-point integration.
                int sum = frame * (frame + 1) / 2 + Math.Max(0, frame - 38) * Math.Max(0, frame - 37) / 2;
                Require(((uint)star.X << 16 | star.XSubposition) == (128u << 16) + (uint)(12 * sum * 256), "Star X trajectory");
                Require(((uint)star.Y << 16 | star.YSubposition) == (128u << 16) - (uint)(sum * 256), "Star Y trajectory");
            }
            if (frame == 9)
                Require(stars.Stars[0].X == 128 && stars.Stars[0].XVelocity == 0 && stars.Stars[0].Timer == 31,
                    "Delay expires on underflow, skips motion, and joins the drawing timer on that call");
            if (frame == 32)
            {
                var oam = new OamBuffer(); oam.BeginFrame(); stars.Draw(oam); oam.FinalizeFrame();
                Require(oam.LastFinalizedSpriteCount > 0, "Stars publish visible OAM after the 32-call lead-in");
                Require(oam.LowTable[..4].SequenceEqual(new byte[] {148,121,0xf0,0x09}), "First star's exact native position and attributes");
            }
        }
        Require(stars.Stars[1].X == 128 && stars.Stars[1].Y == 128 && stars.Stars[1].XVelocity == 0 &&
            stars.Stars[1].IndexAndFrame == 1 && stars.Stars[1].Timer == 32, "Offscreen reset restarts animation without its initial delay");
        using (var saved = new MemoryStream())
        {
            DebuggerObjectGraphSerializer.Serialize(saved, stars);
            saved.Position = 0;
            var restored = DebuggerObjectGraphSerializer.Deserialize<EndingShootingStars>(saved);
            stars.Step(); restored.Step();
            for (int index = 0; index < 40; index++)
                Require(stars.Stars[index].Equals(restored.Stars[index]), "Debugger restore preserves star motion and animation");
        }

        const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
        var installed = RepositoryInstallation.Installation;
        var maps = installed.LoadMaps();
        var scene = new EndingCreditsState(installed.OpenRuntimeAddressSpace(), new CartridgeAudioState(), 2, 0);
        scene.BindObjectArtwork(installed.LoadEndingObjectArt());
        scene.BindPaletteArtwork(installed.LoadEndingPalettes());
        scene.BindPaletteFxColors(maps.RoomPaletteFx);
        scene.BindEndingFont(maps.EndingFont); scene.BindEndingText(maps.EndingText);
        typeof(EndingCreditsState).GetMethod("LoadCreditsAndPostCreditsAssets", flags)!.Invoke(scene, null);
        typeof(EndingCreditsState).GetMethod("SetupPostCreditsBlank", flags)!.Invoke(scene, null);
        var seen = new HashSet<EndingCreditsPhase>();
        for (int tick = 0; tick < 400; tick++)
        {
            scene.Step();
            if (scene.Brightness == 0 || !seen.Add(scene.Phase)) continue;
            var snapshot = scene.CaptureRenderSnapshot();
            Require(snapshot.Memory.ModeledSpriteCount > 0, "Post-credits owner emits star OAM");
            var empty = new OamBuffer(); empty.BeginFrame(); empty.FinalizeFrame();
            var emptyMemory = new PpuMemorySnapshot(snapshot.Memory.Vram, snapshot.Memory.Cgram, empty.CreateUploadPayload(), 0);
            var withoutStars = new LayeredRenderSnapshot(emptyMemory, snapshot.Layers.ToArray(), snapshot.ObjectSelection, snapshot.Brightness);
            var pixels = scene.Render();
            var noStars = SoftwareLayeredSnapshotRenderer.Render(withoutStars);
            bool hidden = scene.Phase == EndingCreditsPhase.PostCreditsWaitingSamus;
            Require(pixels.AsSpan().SequenceEqual(noStars) == hidden, "Stars are visible except behind the native text-only TM");
            var packet = new RenderFrameSnapshot(new(tick + 1, 1, (ushort)tick), snapshot);
            PixelComparison.Verify(packet, pixels, renderer.RenderForReadback(packet), "Post-credits stars CPU/GPU rendering");
            PixelComparison.Verify(packet, pixels, renderer.RenderForReadback(packet), "Star rendering is repeatable");
            Require(scene.CaptureRenderSnapshot().Memory.Oam.SequenceEqual(snapshot.Memory.Oam), "Capture does not advance star animation");
            if (hidden) break;
        }
        Require(seen.Contains(EndingCreditsPhase.PostCreditsFadeIn) && seen.Contains(EndingCreditsPhase.PostCreditsShootingStars) &&
            seen.Contains(EndingCreditsPhase.PostCreditsWaitingBackdrop) && seen.Contains(EndingCreditsPhase.PostCreditsWaitingSamus), "Confirmation covers star fade, additive reveal, wave and text masking");
        // A later logo owner must append stars after its own actors, including after
        // the icon's actor program completes and the final text remains on screen.
        var cgram = (SnesCgram)typeof(EndingCreditsState).GetField("cgram", flags)!.GetValue(scene)!;
        var logo = new EndingLogo(installed.OpenRuntimeAddressSpace(), cgram, () => { }, installed.LoadEndingPalettes());
        typeof(EndingCreditsState).GetField("endingLogo", flags)!.SetValue(scene, logo);
        typeof(EndingCreditsState).GetProperty(nameof(scene.Phase))!.SetValue(scene, EndingCreditsPhase.PostCreditsLogo);
        typeof(EndingLogo).GetProperty(nameof(logo.PaletteStep))!.SetValue(logo, 16);
        typeof(EndingLogo).GetProperty(nameof(logo.CrossfadeStarted))!.SetValue(logo, true);
        var final = scene.CaptureRenderSnapshot();
        Require(final.Memory.ModeledSpriteCount > 0 && final.Layers.ToArray().OfType<ObjRenderLayer>().Any(), "Final logo/text retains the star plane after logo actors finish");
        Console.WriteLine($"{device.Kind}: 40 native star definitions, delay/trajectory/animation/reset, visible ending stars, phase masks and late-scene ownership passed.");

        ushort Word(int address) => (ushort)(rom.ReadCartridgeByte(address) | rom.ReadCartridgeByte(address + 1) << 8);
        static void Require(bool value, string message) { if (!value) throw new InvalidDataException(message); }
    }
}
