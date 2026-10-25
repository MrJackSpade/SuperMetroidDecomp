using SuperMetroid.Core.Frontend;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Diagnostics.SaveStates;
using SuperMetroid.Rendering.Direct3D11.Device;
using SuperMetroid.Rendering.Direct3D11.Frames;
namespace SuperMetroid.RenderVerification.Frontend;
using SuperMetroid.TestSupport.FrameCapture;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.Core.Assets;
using SuperMetroid.TestSupport.FrameRendering;

/// <summary>Checks post-credits shooting-star motion, OAM, phase masks, and rendering on this device against the WARP reference device.</summary>
internal static class EndingShootingStarsTests
{
    /// <summary>Compares native star records and trajectory before exercising their actual ending scene owner.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables")]
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        for (int index = 0; index < 40; index++)
        {
            var definition = EndingShootingStarDefinitions.Records[index];
            Require(definition.Acceleration.X.RawValue == Word(0x8be9cf + index * 8), "Native X acceleration");
            Require(definition.Acceleration.Y.RawValue == Word(0x8be9d1 + index * 8), "Native Y acceleration");
            Require(definition.Period == Word(0x8be9d3 + index * 8), "Native animation period");
            Require(definition.Delay == Word(0x8be9d5 + index * 8), "Native start delay");
        }
        for (int frame = 0; frame < EndingShootingStarDefinitions.FrameCount; frame++)
            Require(EndingShootingStarDefinitions.Attribute(frame).Raw == Word(0x8be9a7 + frame * 2), "Native OBJ attributes");
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
                Require((new FixedCoordinate(star.Position.X.Whole, star.Position.X.Fraction).RawValue) == (128u << 16) + (uint)(12 * sum * 256), "Star X trajectory");
                Require((new FixedCoordinate(star.Position.Y.Whole, star.Position.Y.Fraction).RawValue) == (128u << 16) - (uint)(sum * 256), "Star Y trajectory");
            }
            if (frame == 9)
                Require(stars.Stars[0].Position.X.Whole == 128 && stars.Stars[0].Velocity.X.RawValue == 0 && stars.Stars[0].Timer.RawValue == 31,
                    "Delay expires on underflow, skips motion, and joins the drawing timer on that call");
            if (frame == 32)
            {
                var oam = ScreenSpritesFixtures.Blank(); oam.BeginFrame(); stars.Draw(oam, InstalledFixtures.SpriteSheets.Get(SpriteSheetId.EndingShootingSprites)); oam.FinishFrame();
                Require(oam.LastFinalizedSpriteCount > 0, "Stars publish visible OAM after the 32-call lead-in");
                Require(oam.LowTable[..4].SequenceEqual(new byte[] {148,121,0xf0,0x09}), "First star's exact native position and attributes");
            }
        }
        Require(stars.Stars[1].Position.Whole == new Position16<ScreenPixels>(128, 128) && stars.Stars[1].Velocity.X.RawValue == 0 &&
            !stars.Stars[1].Delayed && stars.Stars[1].AnimationFrame == 0 && stars.Stars[1].Timer.RawValue == 32, "Offscreen reset restarts animation without its initial delay");
        using (var saved = new MemoryStream())
        {
            DebuggerObjectGraphSerializer.Serialize(saved, stars);
            saved.Position = 0;
            var restored = DebuggerObjectGraphSerializer.Deserialize<EndingShootingStars>(saved);
            stars.Step(); restored.Step();
            for (int index = 0; index < 40; index++)
                Require(stars.Stars[index].Equals(restored.Stars[index]), "Debugger restore preserves star motion and animation");
        }

        var installed = RepositoryInstallation.Installation;
        var maps = installed.LoadMaps();
        var scene = new EndingCreditsState(new AudioQueues(), new EndingCreditsSetup { GameTimeHours = 2, GameTimeMinutes = 0 });
        scene.InstalledArtwork.BindFixtureObjectArtwork(installed.LoadEndingObjectArt());
        scene.InstalledArtwork.BindPaletteArtwork(installed.LoadEndingPalettes());
        scene.InstalledArtwork.BindPaletteFxColors(maps.RoomPaletteFx);
        scene.Stage.BindEndingFont(maps.EndingFont); scene.InstalledArtwork.BindEndingText(maps.EndingText);
        scene.InstalledArtwork.LoadCreditsAndPostCreditsAssets();
        scene.CreditsRoll.SetupPostCreditsBlank();
        var seen = new HashSet<EndingCreditsPhase>();
        for (int tick = 0; tick < 400; tick++)
        {
            scene.Step();
            if (scene.Brightness == 0 || !seen.Add(scene.Stage.Phase)) continue;
            var snapshot = EndingCreditsPresentation.CaptureRenderSnapshot(scene);
            byte[] snapshotVram = scene.ShownVram();
            Require(snapshot.Paint.SpriteDraws.Length > 0, "Post-credits owner emits star OAM");
            var emptyMemory = new FramePaint(snapshot.Paint.Colors, []);
            var withoutStars = new LayeredScene(emptyMemory, snapshot.Layers.ToArray(), snapshot.Brightness);
            var pixels = scene.Render();
            var noStars = DisplayRendering.Render(withoutStars);
            bool hidden = scene.Stage.Phase == EndingCreditsPhase.PostCreditsWaitingSamus;
            Require(pixels.SequenceEqual(noStars) == hidden, "Stars are visible except behind the native text-only TM");
            var packet = new DisplayPacket(new(tick + 1, 1, (ushort)tick), snapshot);
            PixelComparison.Verify(packet, pixels, renderer.RenderForReadback(packet), "Post-credits stars on this device match the WARP reference device");
            PixelComparison.Verify(packet, pixels, renderer.RenderForReadback(packet), "Star rendering is repeatable");
            Require(EndingCreditsPresentation.CaptureRenderSnapshot(scene).Paint.SpriteParts().SequenceEqual(snapshot.Paint.SpriteParts()), "Capture does not advance star animation");
            if (hidden) break;
        }
        Require(seen.Contains(EndingCreditsPhase.PostCreditsFadeIn) && seen.Contains(EndingCreditsPhase.PostCreditsShootingStars) &&
            seen.Contains(EndingCreditsPhase.PostCreditsWaitingBackdrop) && seen.Contains(EndingCreditsPhase.PostCreditsWaitingSamus), "Confirmation covers star fade, additive reveal, wave and text masking");
        // A later logo owner must append stars after its own actors, including after
        // the icon's actor program completes and the final text remains on screen.
        var cgram = scene.screen.Palette;
        var logo = new EndingLogo(cgram, () => { }, installed.LoadEndingPalettes());
        scene.InstalledArtwork.CurrentEndingLogo = logo;
        scene.Stage.Phase = EndingCreditsPhase.PostCreditsLogo;
        logo.PaletteStep = 16;
        logo.CrossfadeStarted = true;
        var final = EndingCreditsPresentation.CaptureRenderSnapshot(scene);
        Require(final.Paint.SpriteDraws.Length > 0 && final.LayersOf<SpriteRenderLayer>().Any(), "Final logo/text retains the star plane after logo actors finish");
        Console.WriteLine($"{device.Kind}: 40 native star definitions, delay/trajectory/animation/reset, visible ending stars, phase masks and late-scene ownership passed.");

        ushort Word(int address) => rom.ReadCartridgeWord(address);
        static void Require(bool value, string message) { if (!value) throw new InvalidDataException(message); }
    }
}
