using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Harness.GraphDigestVerification;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.RepositoryInstallation;
namespace SuperMetroid.Verification.Frontend.Ending;

/// <summary>Checks retained render packets across the escape, credits, and timed reward branches.</summary>
internal static class EndingSnapshotTestsVerification
{
    /// <summary>Compares sampled packet pixels with the live ending while checking native palettes, OAM, phase timing, and later packet stability.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for ending palettes, scroll programs and Mode 7 artwork compared with installed ending renders")]
    internal static void VerifyEndingRenderSnapshots()
    {
        byte[] rom = File.ReadAllBytes(Path.GetFullPath("Super Metroid.smc"));
        var rewardArt = RepositoryInstallation.Installation.LoadEndingObjectArt().RewardSprites;
        int samples = 0;
        foreach (ushort hours in new ushort[] { 2, 3, 10 })
        {
            var bus = new SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace(rom);
            var otherBus = new SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace(rom);
            var audio = new CartridgeAudioState(); var otherAudio = new CartridgeAudioState();
            var legacy = CreateRetailEndingFixture(audio, hours, 59);
            var captured = CreateRetailEndingFixture(otherAudio, hours, 59);
            var credits = CreditsPresentation.Load(new MemoryStream(
                SuperMetroid.AssetExtraction.Frontend.Ending.CreditsPresentationExtractor.Extract(bus)));
            legacy.BindStaffCredits(credits);
            captured.BindStaffCredits(credits);
            var phases = new HashSet<EndingCreditsPhase>();
            var phaseEntryFrames = new Dictionary<EndingCreditsPhase, int>();
            var gunshipPalettes = new HashSet<string>();
            Bgr555[]? explosionBackgroundSource = null;
            int finaleDisplayFrames = 0;
            int burstDisplayFrames = 0;
            EndingRewardJump? referenceJump = null;
            RenderFrameSnapshot? previousPacket = null;
            Rgba32[]? previousPixels = null;
            for (int tick = 0; tick < 60000; tick++)
            {
                legacy.Step(); captured.Step();
                if (previousPacket is not null)
                    AssertTrue(previousPixels.SequenceEqual(RenderForComparison(previousPacket)),
                        "ending packet survives subsequent palette/tile/sprite updates");
                bool firstPhaseFrame = phases.Add(legacy.Phase);
                if (firstPhaseFrame) phaseEntryFrames.Add(legacy.Phase, tick);
                if (legacy.Phase is EndingCreditsPhase.FadeInZebesExplosion or EndingCreditsPhase.ZebesExplosionPaletteCrossfade)
                {
                    int elapsed = tick - phaseEntryFrames[EndingCreditsPhase.FadeInZebesExplosion];
                    var explosion = legacy.Presentation.CaptureRenderSnapshot();
                    if (firstPhaseFrame && legacy.Phase == EndingCreditsPhase.FadeInZebesExplosion)
                        explosionBackgroundSource = explosion.Memory.Cgram.Slice(112, 16).ToArray();
                    for (int i = 0; i < 16; i++)
                    {
                        int remaining = 32 - elapsed / 2;
                        Bgr555 expectedColor = (explosionBackgroundSource ?? throw new InvalidOperationException("Verification requires explosionBackgroundSource."))[i].Map((_, channel) => channel * remaining / 32);
                        AssertEqual(expectedColor, explosion.Memory.Cgram[112 + i], "native explosion BG palette fades out on the same cadence");
                    }

                    foreach (int start in new[] { 208, 240 })
                    for (int i = start; i < start + 16; i++)
                    {
                        ushort source = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), 0x8cebe9 + i * 2);
                        int step = elapsed / 2;
                        ushort expectedColor = (ushort)(((source & 31) * step / 32)
                            | (((source >> 5 & 31) * step / 32) << 5)
                            | (((source >> 10 & 31) * step / 32) << 10));
                        AssertEqual(expectedColor, explosion.Memory.Cgram[i], "native explosion OBJ palette fades in over both fade-in and crossfade phases");
                    }
                    AssertTrue(explosion.LayersOf<ObjRenderLayer>().Single().AddToScreen,
                        "explosion transition adds OBJ subscreen to BG1 and backdrop");
                }
                if (firstPhaseFrame && legacy.Phase == EndingCreditsPhase.ZebesExplosionTileUpload)
                    AssertEqual(64, tick - phaseEntryFrames[EndingCreditsPhase.FadeInZebesExplosion],
                        "native shared explosion countdown includes screen fade-in and expires after 64 calls");
                if (firstPhaseFrame && legacy.Phase == EndingCreditsPhase.PostCreditsWaitingSamus)
                    AssertEqual(212, tick - phaseEntryFrames[EndingCreditsPhase.PostCreditsShootingStars],
                        "native post-credits backdrop has 32 fade frames followed by 180 waiting frames before producer text");
                if (legacy.Phase == EndingCreditsPhase.PostCreditsShootingStars)
                {
                    int elapsed = tick - phaseEntryFrames[EndingCreditsPhase.PostCreditsShootingStars];
                    var colors = legacy.Presentation.CaptureRenderSnapshot().Memory.Cgram;
                    for (int i = 32; i < 48; i++)
                    {
                        ushort source = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), 0x8ce7e9 + i * 2);
                        int expectedColor = ((source & 31) * elapsed / 32)
                            | (((source >> 5 & 31) * elapsed / 32) << 5)
                            | (((source >> 10 & 31) * elapsed / 32) << 10);
                        AssertEqual((ushort)expectedColor, colors[i], "native 8.8 waiting-backdrop palette fade");
                    }
                }
                bool sample = firstPhaseFrame || tick % 97 == 0;
                if (legacy.Phase == EndingCreditsPhase.ZebesExplosionAnimation &&
                    legacy.Presentation.CaptureRenderSnapshot().LayersOf<BgSubscreenAddRenderLayer>().Any(layer => layer.MainObjects))
                {
                    var burst = legacy.Presentation.CaptureRenderSnapshot();
                    var sub = burst.LayersOf<BgSubscreenAddRenderLayer>().Single();
                    AssertEqual(0x7c00, sub.TilemapWord, "F2B7 selects burst BG2 map");
                    AssertEqual(0x7000, (sub.MainCoverage ?? throw new InvalidOperationException("Verification requires sub.MainCoverage.")).TilemapWord, "F2B7 keeps BG1 map and excludes backdrop color math");

                    burstDisplayFrames++;
                }
                if (legacy.Phase == EndingCreditsPhase.ZebesExplosionAnimation
                    && legacy.Presentation.CaptureRenderSnapshot().LayersOf<BgSubscreenAddRenderLayer>().Any(layer => layer.IncludeObjects))
                {
                    var finale = legacy.Presentation.CaptureRenderSnapshot();
                    var backgrounds = finale.LayersOf<Bg4BppRenderLayer>();
                    AssertTrue(backgrounds.Select(bg => bg.TilemapWord).SequenceEqual(new[] { 0x7800, 0x7400, 0x7800, 0x7400 }),
                        "native explosion finale selects both background maps in main-screen priority order");
                    AssertTrue(backgrounds.All(bg => bg.CharacterWord == 0x4000), "explosion finale uses Mode1 character base, not stale Mode7 decoding");
                    AssertTrue(!finale.Layers.Any(layer => layer is ObjRenderLayer or ObjPriorityRenderLayer),
                        "F2FA moves OBJ exclusively to the subscreen");
                    if (hours == 2 && finaleDisplayFrames == 10)
                    {

                    }
                    finaleDisplayFrames++;
                }
                if (legacy.Phase == EndingCreditsPhase.PostCreditsGesture)
                {
                    AssertEqual(308, phaseEntryFrames[EndingCreditsPhase.PostCreditsGesture]
                        - phaseEntryFrames[EndingCreditsPhase.PostCreditsReward],
                        "gesture starts after two 64-frame reveals and the 180-frame copyright hold");
                    var gestureFrame = legacy.Presentation.CaptureRenderSnapshot();
                    AssertTrue(gestureFrame.Layers.All(layer => layer is ObjRenderLayer),
                        "E342 gesture renders OBJ without the stale waiting or copyright backgrounds");
                }
                if (firstPhaseFrame && legacy.Phase == EndingCreditsPhase.PostCreditsJump)
                    AssertEqual(hours < 3 ? 244 : 329,
                        tick - phaseEntryFrames[EndingCreditsPhase.PostCreditsGesture],
                        "live reward owner consumes the complete native gesture before handing off");
                if (legacy.Phase == EndingCreditsPhase.PostCreditsJump)
                {
                    if (firstPhaseFrame)
                        referenceJump = new EndingRewardJump(legacy.EndingReward, _ => { });
                    else
                        (referenceJump ?? throw new InvalidOperationException("Verification requires referenceJump.")).Step();
                    var jumpFrame = legacy.Presentation.CaptureRenderSnapshot();
                    var expectedOam = ScreenSpritesFixtures.Frame(jumpFrame.ObjectSelection, objects => referenceJump.Draw(objects, rewardArt));
                    AssertEqual(referenceJump.FallingSheetSelected
                            ? EndingCreditsRomData.Rendering.FallingRewardObjectSelection
                            : EndingCreditsRomData.Rendering.RewardObjectSelection,
                        jumpFrame.ObjectSelection,
                        "live reward switches OBJ sheet on the same native flight frame");
                    // The live ending also appends the restored shooting stars after the reference jump actors.
                    PlacedSpritePart[] jumpParts = PlacedSpritePart.Of(expectedOam.Draws);
                    PlacedSpritePart[] liveParts = [.. jumpFrame.Memory.SpriteParts().Take(jumpParts.Length)];
                    int[] mismatches = [.. Enumerable.Range(0, jumpParts.Length).Where(index => index >= liveParts.Length || liveParts[index] != jumpParts[index])];
                    AssertTrue(mismatches.Length == 0,
                        "live reward preserves native actor positions, sizes, tile indices and palettes on every jump frame" +
                        string.Concat(mismatches.Select(index => $" [part {index}: live {(index < liveParts.Length ? liveParts[index].Part.ToString() : "missing")}, native {jumpParts[index].Part}]")));
                    AssertTrue(jumpFrame.Layers.All(layer => layer is ObjRenderLayer),
                        "reward flight and landing keep BG disabled during icon graphics replacement");
                }
                if (firstPhaseFrame && legacy.Phase == EndingCreditsPhase.PostCreditsShot)
                {
                    AssertEqual(hours < 3 ? 279 : 200,
                        tick - phaseEntryFrames[EndingCreditsPhase.PostCreditsJump],
                        "live reward jump consumes native flight and landing durations before shooting");
                    byte[] icon = RomDataReader.Decompress(bus, EndingCreditsRomData.Assets.PostCreditsMode7Characters,
                        EndingCreditsRomData.Rendering.DecompressionLimit);
                    AssertTrue(icon.AsSpan(0, 0x8000).SequenceEqual(legacy.Presentation.CaptureRenderSnapshot().Memory.Vram[..0x8000]),
                        "live landing uploads all sixteen chunks of the native interleaved icon graphics");
                }
                if (firstPhaseFrame && legacy.Phase == EndingCreditsPhase.PostCreditsWhiteFlash)
                    AssertEqual(216, tick - phaseEntryFrames[EndingCreditsPhase.PostCreditsShot],
                        "live post-shot owner includes rotation and the complete native hold");
                if (legacy.Phase == EndingCreditsPhase.PostCreditsWhiteFlash)
                {
                    int elapsed = tick - phaseEntryFrames[EndingCreditsPhase.PostCreditsWhiteFlash];
                    var flash = legacy.Presentation.CaptureRenderSnapshot();
                    var add = flash.LayersOf<FixedColorAddRenderLayer>().Single();
                    AssertEqual((byte)Math.Max(0, 31 - elapsed), add.Red, "native post-shot white fade component");
                    var textPlane = flash.LayersOf<Bg4BppRenderLayer>().Single();
                    AssertEqual(0x4c00, textPlane.TilemapWord, "final text uses native upper screen");
                    AssertEqual(64, textPlane.MapSize.Y, "final text map is two screens tall");
                    if (firstPhaseFrame)
                        AssertTrue(legacy.Presentation.Render().All(pixel => pixel == new Rgba32(255, 255, 255)), "post-shot flash begins fully white");
                }
                if (firstPhaseFrame && legacy.Phase == EndingCreditsPhase.PostCreditsLogo)
                    AssertEqual(32, tick - phaseEntryFrames[EndingCreditsPhase.PostCreditsWhiteFlash], "native white flash lasts 32 calls");
                if (firstPhaseFrame && legacy.Phase == EndingCreditsPhase.ItemPercentage)
                    AssertEqual(187, tick - phaseEntryFrames[EndingCreditsPhase.PostCreditsLogo], "live logo completes its native approach and palette handoff");
                if (legacy.Phase == EndingCreditsPhase.PostCreditsShot)
                {
                    int elapsed = tick - phaseEntryFrames[EndingCreditsPhase.PostCreditsShot];
                    var shotFrame = legacy.Presentation.CaptureRenderSnapshot();
                    var mode = shotFrame.LayersOf<Mode7RenderLayer>().Single();
                    AssertTrue(mode.SubtractObjSubscreen, "live shot enables native BG1 minus OBJ blending");
                    AssertEqual((short)104, mode.Registers.Center.X, "post-shot matrix horizontal origin");
                    AssertEqual((short)112, mode.Registers.Center.Y, "post-shot matrix vertical origin");
                    AssertEqual((short)-4, mode.Registers.Scroll.X, "post-shot horizontal scroll");
                    AssertEqual((short)-8, mode.Registers.Scroll.Y, "post-shot vertical scroll");
                    if (hours == 2 && elapsed == 16)
                    {

                    }
                }
                if (hours < 10 && phaseEntryFrames.TryGetValue(EndingCreditsPhase.PostCreditsReward, out int rewardStart)
                    && legacy.Phase is EndingCreditsPhase.PostCreditsReward or EndingCreditsPhase.PostCreditsCopyright)
                {
                    int elapsed = tick - rewardStart;
                    int activeElapsed = elapsed <= 64 ? elapsed : elapsed < 244 ? 64 : elapsed - 180;
                    int fadeStep = activeElapsed / 4;
                    var colors = legacy.Presentation.CaptureRenderSnapshot().Memory.Cgram;
                    foreach (int start in hours < 3 ? new[] { 32, 208 } : new[] { 32, 208, 224 })
                    {
                        int factor = start == 32 ? 32 - fadeStep : fadeStep;
                        for (int i = start; i < start + 16; i++)
                        {
                            ushort source = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), 0x8ce7e9 + i * 2);
                            ushort expectedColor = (ushort)(((source & 31) * factor / 32)
                                | (((source >> 5 & 31) * factor / 32) << 5)
                                | (((source >> 10 & 31) * factor / 32) << 10));
                            AssertEqual(expectedColor, colors[i], "native reward dissolve pauses and resumes around copyright");
                        }
                    }
                }
                if (phaseEntryFrames.TryGetValue(EndingCreditsPhase.PostCreditsCopyright, out int copyrightStart)
                    && tick - copyrightStart <= 180)
                    AssertEqual(tick - copyrightStart < 180 ? EndingCreditsPhase.PostCreditsCopyright : EndingCreditsPhase.PostCreditsReward,
                        legacy.Phase, "native copyright holds for exactly 180 frames before resuming reward reveal");
                if (phaseEntryFrames.TryGetValue(EndingCreditsPhase.PostCreditsReward, out int revealStart)
                    && tick - revealStart == 64)
                {
                    var copyright = legacy.Presentation.CaptureRenderSnapshot();
                    AssertTrue(!copyright.Layers.Any(layer => layer is ObjRenderLayer),
                        "native E293 hides reward actors during copyright panel");
                    AssertEqual(0x4800, copyright.LayersOf<Bg4BppRenderLayer>().Single().TilemapWord,
                        "native E293 selects copyright BG1 after first 64 reveal frames");
                }
                if (legacy.Phase is EndingCreditsPhase.WaitForPlanetEscapeMusic or EndingCreditsPhase.WaitForPlanetEscapeMusicQueue)
                {
                    var flash = legacy.Presentation.CaptureRenderSnapshot();
                    AssertEqual(0, flash.Layers.Length, "native F32B disables both screens for explosion whiteout");
                    AssertEqual(0x7fff, flash.Memory.Cgram[0], "native explosion whiteout backdrop");
                    if (firstPhaseFrame)
                        AssertTrue(legacy.Presentation.Render().All(pixel => pixel == new Rgba32(255, 255, 255)),
                            "explosion whiteout is white across the actual rendered viewport");
                }
                if (legacy.Phase == EndingCreditsPhase.PostCreditsReward)
                {
                    var backgrounds = legacy.Presentation.CaptureRenderSnapshot().LayersOf<Bg4BppRenderLayer>();
                    AssertEqual(hours >= 10 ? 0 : 1, backgrounds.Length, "native reward reveal background enable mask");
                    if (hours < 10)
                    {
                        AssertEqual(0x4c00, backgrounds[0].TilemapWord, "reward reveal selects waiting BG2, not producer BG1");
                        AssertEqual(0x5000, backgrounds[0].CharacterWord, "reward reveal uses waiting BG2 characters");
                    }
                }
                if (legacy.Phase is EndingCreditsPhase.PlanetEscapeFast or EndingCreditsPhase.PlanetEscapeSlow or EndingCreditsPhase.PlanetEscapeAccelerating)
                {
                    gunshipPalettes.Add(string.Join(',', legacy.Presentation.CaptureRenderSnapshot().Memory.Cgram.Slice(80, 16).ToArray()));
                    int elapsed = tick - phaseEntryFrames[EndingCreditsPhase.PlanetEscapeFast];
                    int expectedWhite = elapsed <= 192 ? 31 : Math.Max(0, 30 - (elapsed - 193) / 8);
                    var flight = legacy.Presentation.CaptureRenderSnapshot();
                    var fixedColors = flight.LayersOf<FixedColorAddRenderLayer>();
                    AssertEqual(expectedWhite > 0 ? 1 : 0, fixedColors.Length, "native flyaway disables fixed color after final decrement");
                    if (expectedWhite > 0)
                    {
                        AssertEqual((byte)expectedWhite, fixedColors[0].Red, "native flyaway holds 192 calls then fades every eight");
                        AssertTrue(flight.LayersOf<ObjPriorityRenderLayer>().All(layer => layer.FixedColor == fixedColors[0]), "OBJ and backdrop share native fixed white");
                        AssertTrue(flight.Layers[0] is FixedColorAddRenderLayer && flight.Layers[1] is ObjPriorityRenderLayer { Priority: 0 }
                            && flight.Layers[2] is Mode7RenderLayer,
                            "fixed backdrop is composed before unaffected Mode7 BG1");
                    }

                }
                if (legacy.Phase == EndingCreditsPhase.OperationSuccessfulText && tick % 97 == 0)
                {
                    var memory = legacy.Presentation.CaptureRenderSnapshot().Memory;
                    AssertTrue(memory.SpriteParts().All(part => part.Part.Palette <= 2),
                        "explosion actors delete before operation text, leaving only text OBJ palettes");
                }
                if (legacy.Phase >= EndingCreditsPhase.ItemPercentage)
                {
                    // E58A clears cinematic actors; the independent star field survives.
                    var stars = legacy.ShootingStars;
                    var starOnlyOam = ScreenSpritesFixtures.Blank();
                    starOnlyOam.BeginFrame();
                    stars?.Draw(starOnlyOam);
                    starOnlyOam.FinalizeFrame();
                    var actual = legacy.Presentation.CaptureRenderSnapshot();
                    AssertTrue(PlacedSpritePart.Of(starOnlyOam.Draws).SequenceEqual(actual.Memory.SpriteParts()),
                        "native E58A leaves only the independent shooting stars before final percentage text");
                }
                if (legacy.Phase == EndingCreditsPhase.PlanetEscapeFast && sample)
                {
                    var memory = legacy.Presentation.CaptureRenderSnapshot().Memory;
                    byte[] characters = RomDataReader.Decompress(bus, 0x95a82f, 0x8000);
                    byte[] map = RomDataReader.Decompress(bus, 0x96fe69, 0x8000);
                    for (int word = 0; word < 0x4000; word++)
                    {
                        AssertEqual(characters[word], memory.Vram[word * 2 + 1], "native flyaway character upload");
                        AssertEqual(word < 0x300 ? map[word] : (byte)0x8c, memory.Vram[word * 2],
                            "native flyaway map upload and padded background");
                    }
                    AssertEqual(0, memory.Cgram[0], "flyaway clears explosion backdrop");
                    if (firstPhaseFrame)
                        AssertEqual(0x7fff, memory.Cgram[81], "native gunship emergence palette starts white");
                }
                if (legacy.Phase == EndingCreditsPhase.PostCreditsBlank)
                {
                    var memory = legacy.Presentation.CaptureRenderSnapshot().Memory;
                    for (int color = 4; color < 256; color++)
                        AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), 0x8ce7e9 + color * 2),
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
                    expected = legacy.Presentation.Render();
                    if (legacy.Brightness == 15)
                    {

                    }
                    packet = new(new(++samples, 1, (ushort)tick), captured.Presentation.CaptureRenderSnapshot());
                    AssertTrue(expected.SequenceEqual(RenderForComparison(packet)),
                        $"ending reward {hours}, phase {legacy.Phase}, tick {tick} pixel parity");
                }
                audio.AdvanceFrame(default); otherAudio.AdvanceFrame(default);
                AssertEqual(legacy.Phase, captured.Phase, "ending capture preserves phase");
                AssertEqual(legacy.CinematicFrame, captured.CinematicFrame, "ending capture preserves timer");
                AssertEqual(legacy.CreditsVerticalScroll, captured.CreditsVerticalScroll, "ending capture preserves half-pixel scroll result");
                if (packet is not null)
                    AssertTrue(expected.SequenceEqual(RenderForComparison(packet)), "ending packet repeat rendering is observational");
                previousPacket = packet;
                previousPixels = expected;
                if (legacy.Phase == EndingCreditsPhase.SeeYouNextMission) break;
            }
            AssertEqual(EndingCreditsPhase.SeeYouNextMission, legacy.Phase, "ending capture fixture completes");
            AssertTrue(gunshipPalettes.Count >= 10, "gunship emergence executes changing native palette records");
            AssertTrue(finaleDisplayFrames > 10, "ending executes the actual callback-driven Mode1 explosion finale");
            AssertTrue(burstDisplayFrames > 10, "ending executes the callback-driven explosion burst before its finale");
            AssertTrue(phases.Contains(EndingCreditsPhase.Credits) && phases.Contains(EndingCreditsPhase.PostCreditsReward)
                && phases.Contains(EndingCreditsPhase.ZebesExplosionAnimation), "ending covers all composition families");
            Rgba32[] final = legacy.Presentation.Render();
            LayeredRenderSnapshot retained = captured.Presentation.CaptureRenderSnapshot();
            for (int i = 0; i < 600; i++) captured.Step();

            AssertTrue(final.SequenceEqual(RenderForComparison(retained)), "ending final packet survives simulation advance");
        }
        Console.WriteLine($"  Ending snapshots: {samples} sampled frames cover escape, credits and all three reward branches.");
    }
}
