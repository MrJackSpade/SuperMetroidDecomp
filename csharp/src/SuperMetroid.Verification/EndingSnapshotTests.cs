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
            RenderFrameSnapshot? previousPacket = null;
            Rgba32[]? previousPixels = null;
            for (int tick = 0; tick < 60000; tick++)
            {
                legacy.Step(); captured.Step();
                if (previousPacket is not null)
                    AssertTrue(previousPixels.AsSpan().SequenceEqual(SoftwareFrameSnapshotRenderer.Render(previousPacket)),
                        "ending packet survives subsequent palette/tile/sprite updates");
                bool sample = phases.Add(legacy.Phase) || tick % 97 == 0;
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
