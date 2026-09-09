using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyEndingRenderSnapshots()
    {
        byte[] rom = File.ReadAllBytes(Path.GetFullPath("Super Metroid.smc"));
        int samples = 0;
        foreach (ushort hours in new ushort[] { 2, 3, 10 })
        {
            var bus = new SuperMetroidAddressSpace(rom);
            var otherBus = new SuperMetroidAddressSpace(rom);
            var audio = new CartridgeAudioState(); var otherAudio = new CartridgeAudioState();
            var legacy = new EndingCreditsState(bus, audio, hours, 59);
            var captured = new EndingCreditsState(otherBus, otherAudio, hours, 59);
            var phases = new HashSet<EndingCreditsPhase>();
            var phaseEntryFrames = new Dictionary<EndingCreditsPhase, int>();
            var gunshipPalettes = new HashSet<string>();
            RenderFrameSnapshot? previousPacket = null;
            Rgba32[]? previousPixels = null;
            for (int tick = 0; tick < 60000; tick++)
            {
                legacy.Step(); captured.Step();
                if (previousPacket is not null)
                    AssertTrue(previousPixels.AsSpan().SequenceEqual(SoftwareFrameSnapshotRenderer.Render(previousPacket)),
                        "ending packet survives subsequent palette/tile/sprite updates");
                bool firstPhaseFrame = phases.Add(legacy.Phase);
                if (firstPhaseFrame) phaseEntryFrames.Add(legacy.Phase, tick);
                if (firstPhaseFrame && legacy.Phase == EndingCreditsPhase.PostCreditsWaitingSamus)
                    AssertEqual(212, tick - phaseEntryFrames[EndingCreditsPhase.PostCreditsShootingStars],
                        "native post-credits backdrop has 32 fade frames followed by 180 waiting frames before producer text");
                if (legacy.Phase == EndingCreditsPhase.PostCreditsShootingStars)
                {
                    int elapsed = tick - phaseEntryFrames[EndingCreditsPhase.PostCreditsShootingStars];
                    var colors = legacy.CaptureRenderSnapshot().Memory.Cgram;
                    for (int i = 32; i < 48; i++)
                    {
                        ushort source = RomDataReader.ReadWordFixedBank(bus, 0x8ce7e9 + i * 2);
                        int expectedColor = ((source & 31) * elapsed / 32)
                            | (((source >> 5 & 31) * elapsed / 32) << 5)
                            | (((source >> 10 & 31) * elapsed / 32) << 10);
                        AssertEqual((ushort)expectedColor, colors[i], "native 8.8 waiting-backdrop palette fade");
                    }
                }
                bool sample = firstPhaseFrame || tick % 97 == 0;
                if (phaseEntryFrames.TryGetValue(EndingCreditsPhase.PostCreditsCopyright, out int copyrightStart)
                    && tick - copyrightStart <= 180)
                    AssertEqual(tick - copyrightStart < 180 ? EndingCreditsPhase.PostCreditsCopyright : EndingCreditsPhase.PostCreditsReward,
                        legacy.Phase, "native copyright holds for exactly 180 frames before resuming reward reveal");
                if (phaseEntryFrames.TryGetValue(EndingCreditsPhase.PostCreditsReward, out int revealStart)
                    && tick - revealStart == 64)
                {
                    var copyright = legacy.CaptureRenderSnapshot();
                    AssertTrue(!copyright.Layers.ToArray().Any(layer => layer is ObjRenderLayer),
                        "native E293 hides reward actors during copyright panel");
                    AssertEqual((ushort)0x4800, copyright.Layers.ToArray().OfType<Bg4BppRenderLayer>().Single().TilemapWord,
                        "native E293 selects copyright BG1 after first 64 reveal frames");
                }
                if (legacy.Phase is EndingCreditsPhase.WaitForPlanetEscapeMusic or EndingCreditsPhase.WaitForPlanetEscapeMusicQueue)
                {
                    var flash = legacy.CaptureRenderSnapshot();
                    AssertEqual(0, flash.Layers.Length, "native F32B disables both screens for explosion whiteout");
                    AssertEqual((ushort)0x7fff, flash.Memory.Cgram[0], "native explosion whiteout backdrop");
                    if (firstPhaseFrame)
                        AssertTrue(legacy.Render().All(pixel => pixel == new Rgba32(255, 255, 255)),
                            "explosion whiteout is white across the actual rendered viewport");
                }
                if (legacy.Phase == EndingCreditsPhase.PostCreditsReward)
                {
                    var backgrounds = legacy.CaptureRenderSnapshot().Layers.ToArray().OfType<Bg4BppRenderLayer>().ToArray();
                    AssertEqual(hours >= 10 ? 0 : 1, backgrounds.Length, "native reward reveal background enable mask");
                    if (hours < 10)
                    {
                        AssertEqual((ushort)0x4c00, backgrounds[0].TilemapWord, "reward reveal selects waiting BG2, not producer BG1");
                        AssertEqual((ushort)0x5000, backgrounds[0].CharacterWord, "reward reveal uses waiting BG2 characters");
                    }
                }
                if (legacy.Phase is EndingCreditsPhase.PlanetEscapeFast or EndingCreditsPhase.PlanetEscapeSlow or EndingCreditsPhase.PlanetEscapeAccelerating)
                    gunshipPalettes.Add(string.Join(',', legacy.CaptureRenderSnapshot().Memory.Cgram.Slice(80, 16).ToArray()));
                if (legacy.Phase == EndingCreditsPhase.OperationSuccessfulText && tick % 97 == 0)
                {
                    var memory = legacy.CaptureRenderSnapshot().Memory;
                    for (int sprite = 0; sprite < memory.ModeledSpriteCount; sprite++)
                        AssertTrue(((memory.Oam[sprite * 4 + 3] >> 1) & 7) <= 2,
                            "explosion actors delete before operation text, leaving only text OBJ palettes");
                }
                if (legacy.Phase >= EndingCreditsPhase.ItemPercentage)
                    AssertEqual(0, legacy.CaptureRenderSnapshot().Memory.ModeledSpriteCount,
                        "native E58A clears cinematic sprites before final percentage text");
                if (legacy.Phase == EndingCreditsPhase.PlanetEscapeFast && sample)
                {
                    var memory = legacy.CaptureRenderSnapshot().Memory;
                    byte[] characters = RomDataReader.Decompress(bus, 0x95a82f, 0x8000);
                    byte[] map = RomDataReader.Decompress(bus, 0x96fe69, 0x8000);
                    for (int word = 0; word < 0x4000; word++)
                    {
                        AssertEqual(characters[word], memory.Vram[word * 2 + 1], "native flyaway character upload");
                        AssertEqual(word < 0x300 ? map[word] : (byte)0x8c, memory.Vram[word * 2],
                            "native flyaway map upload and padded background");
                    }
                    AssertEqual((ushort)0, memory.Cgram[0], "flyaway clears explosion backdrop");
                    if (firstPhaseFrame)
                        AssertEqual((ushort)0x7fff, memory.Cgram[81], "native gunship emergence palette starts white");
                }
                if (legacy.Phase == EndingCreditsPhase.PostCreditsBlank)
                {
                    var memory = legacy.CaptureRenderSnapshot().Memory;
                    for (int color = 4; color < 256; color++)
                        AssertEqual(RomDataReader.ReadWordFixedBank(bus, 0x8ce7e9 + color * 2),
                            memory.Cgram[color], "native end-credits Intro4 palette");
                    byte[] reward = RomDataReader.Decompress(bus, hours < 3 ? 0x97b957 : 0x979803, 0x8000);
                    AssertTrue(reward.AsSpan(0, 0x4000).SequenceEqual(memory.Vram[..0x4000]),
                        "reward branch uses native contiguous OBJ characters");
                }
                RenderFrameSnapshot? packet = null;
                Rgba32[]? expected = null;
                if (sample)
                {
                    // IntroDiscoverySprite.Draw only writes OAM; it does not advance
                    // this ending's sprite state. Independent owners still guard against
                    // an accidental producer-side mutation introduced by extraction.
                    expected = legacy.Render();
                    if (legacy.Brightness == 15)
                    {
                        Directory.CreateDirectory("csharp/test-temp/ending-504");
                        PngWriter.WriteRgba($"csharp/test-temp/ending-504/{hours}-{legacy.Phase}.png", 256, 224, expected);
                    }
                    packet = RoundTripRenderPacket(new(new(++samples, 1, (ushort)tick), captured.CaptureRenderSnapshot()));
                    AssertTrue(expected.AsSpan().SequenceEqual(SoftwareFrameSnapshotRenderer.Render(packet)),
                        $"ending reward {hours}, phase {legacy.Phase}, tick {tick} pixel parity");
                }
                audio.AdvanceFrame(bus, default); otherAudio.AdvanceFrame(otherBus, default);
                AssertEqual(legacy.Phase, captured.Phase, "ending capture preserves phase");
                AssertEqual(legacy.CinematicFrame, captured.CinematicFrame, "ending capture preserves timer");
                AssertEqual(legacy.CreditsVerticalScroll, captured.CreditsVerticalScroll, "ending capture preserves half-pixel scroll result");
                if (packet is not null)
                    AssertTrue(expected.AsSpan().SequenceEqual(SoftwareFrameSnapshotRenderer.Render(packet)), "ending packet repeat rendering is observational");
                previousPacket = packet;
                previousPixels = expected;
                if (legacy.Phase == EndingCreditsPhase.SeeYouNextMission) break;
            }
            AssertEqual(EndingCreditsPhase.SeeYouNextMission, legacy.Phase, "ending capture fixture completes");
            AssertTrue(gunshipPalettes.Count >= 10, "gunship emergence executes changing native palette records");
            AssertTrue(phases.Contains(EndingCreditsPhase.Credits) && phases.Contains(EndingCreditsPhase.PostCreditsReward)
                && phases.Contains(EndingCreditsPhase.ZebesExplosionAnimation), "ending covers all composition families");
            Rgba32[] final = legacy.Render();
            LayeredRenderSnapshot retained = captured.CaptureRenderSnapshot();
            for (int i = 0; i < 120; i++) captured.Step();
            PngWriter.WriteRgba($"csharp/test-temp/ending-504/{hours}-Final.png", 256, 224, captured.Render());
            AssertTrue(final.AsSpan().SequenceEqual(SoftwareLayeredSnapshotRenderer.Render(retained)), "ending final packet survives simulation advance");
        }
        Console.WriteLine($"  Ending snapshots: {samples} sampled frames cover escape, credits and all three reward branches.");
    }
}
