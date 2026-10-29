using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Harness.GraphDigestVerification;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;
namespace SuperMetroid.Verification.Frontend.Ending;

/// <summary>Checks retained render packets across the escape, credits, and timed reward branches.</summary>
internal static class EndingSnapshotTestsVerification
{
    /// <summary>Compares sampled packet pixels with the live ending while checking native palettes, OAM, phase timing, and later packet stability.</summary>
    internal static void VerifyEndingRenderSnapshots()
    {
        byte[] rom = File.ReadAllBytes(Path.GetFullPath("Super Metroid.smc"));
        int samples = 0;
        foreach (ushort hours in new ushort[] { 2, 3, 10 })
        {
            var bus = new SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace(rom);
            var otherBus = new SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace(rom);
            var audio = new AudioQueues(); var otherAudio = new AudioQueues();
            var legacy = CreateRetailEndingFixture(audio, hours, 59);
            var captured = CreateRetailEndingFixture(otherAudio, hours, 59);
            var credits = CreditsPresentation.Load(new MemoryStream(
                SuperMetroid.AssetExtraction.Frontend.Ending.CreditsPresentationExtractor.Extract(bus)));
            legacy.InstalledArtwork.BindStaffCredits(credits);
            captured.InstalledArtwork.BindStaffCredits(credits);
            var phases = new HashSet<EndingCreditsPhase>();
            var phaseEntryFrames = new Dictionary<EndingCreditsPhase, int>();
            var gunshipPalettes = new HashSet<string>();
            RgbColor[]? explosionBackgroundSource = null;
            int finaleDisplayFrames = 0;
            int burstDisplayFrames = 0;
            EndingRewardJump? referenceJump = null;
            DisplayPacket? previousPacket = null;
            RgbaPixel[]? previousPixels = null;
            for (int tick = 0; tick < 60000; tick++)
            {
                legacy.Step(); captured.Step();
                if (previousPacket is not null)
                    AssertTrue(previousPixels.SequenceEqual(RenderForComparison(previousPacket)),
                        "ending packet survives subsequent palette/tile/sprite updates");
                bool firstPhaseFrame = phases.Add(legacy.Stage.Phase);
                if (firstPhaseFrame) phaseEntryFrames.Add(legacy.Stage.Phase, tick);
                if (legacy.Stage.Phase is EndingCreditsPhase.FadeInZebesExplosion or EndingCreditsPhase.ZebesExplosionPaletteCrossfade)
                {
                    int elapsed = tick - phaseEntryFrames[EndingCreditsPhase.FadeInZebesExplosion];
                    var explosion = EndingCreditsPresentation.CaptureRenderSnapshot(legacy);
                    if (firstPhaseFrame && legacy.Stage.Phase == EndingCreditsPhase.FadeInZebesExplosion)
                        explosionBackgroundSource = explosion.Paint.Colors.Slice(112, 16).ToArray();
                    for (int i = 0; i < 16; i++)
                    {
                        int remaining = 32 - elapsed / 2;
                        RgbColor expectedColor = (explosionBackgroundSource ?? throw new InvalidOperationException("Verification requires explosionBackgroundSource."))[i].Map((_, channel) => channel * remaining / 32);
                        AssertEqual(expectedColor, explosion.Paint.Colors[112 + i], "native explosion BG palette fades out on the same cadence");
                    }

                    foreach (int start in new[] { 208, 240 })
                    for (int i = start; i < start + 16; i++)
                    {
                        ushort source = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), 0x8cebe9 + i * 2);
                        int step = elapsed / 2;
                        ushort expectedColor = (ushort)(((source & 31) * step / 32)
                            | (((source >> 5 & 31) * step / 32) << 5)
                            | (((source >> 10 & 31) * step / 32) << 10));
                        AssertEqual(expectedColor, explosion.Paint.Colors[i], "native explosion OBJ palette fades in over both fade-in and crossfade phases");
                    }
                    AssertTrue(explosion.LayersOf<SpritesLayer>().Single().AddToScreen,
                        "explosion transition adds OBJ subscreen to BG1 and backdrop");
                }
                if (firstPhaseFrame && legacy.Stage.Phase == EndingCreditsPhase.ZebesExplosionTileUpload)
                    AssertEqual(64, tick - phaseEntryFrames[EndingCreditsPhase.FadeInZebesExplosion],
                        "native shared explosion countdown includes screen fade-in and expires after 64 calls");
                if (firstPhaseFrame && legacy.Stage.Phase == EndingCreditsPhase.PostCreditsWaitingSamus)
                    AssertEqual(212, tick - phaseEntryFrames[EndingCreditsPhase.PostCreditsShootingStars],
                        "native post-credits backdrop has 32 fade frames followed by 180 waiting frames before producer text");
                if (legacy.Stage.Phase == EndingCreditsPhase.PostCreditsShootingStars)
                {
                    int elapsed = tick - phaseEntryFrames[EndingCreditsPhase.PostCreditsShootingStars];
                    var colors = EndingCreditsPresentation.CaptureRenderSnapshot(legacy).Paint.Colors;
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
                if (legacy.Stage.Phase == EndingCreditsPhase.ZebesExplosionAnimation &&
                    EndingCreditsPresentation.CaptureRenderSnapshot(legacy).LayersOf<AddedScreenTilesLayer>().Any(layer => layer.MainObjects))
                {
                    var burst = EndingCreditsPresentation.CaptureRenderSnapshot(legacy);
                    var sub = burst.LayersOf<AddedScreenTilesLayer>().Single();
                    var shownLegacy = legacy.ShownVram();
                    AssertTrue(ShowsTilemap(shownLegacy, sub.Cells, 0x7c00), "F2B7 selects burst BG2 map");
                    var shownLegacy2 = legacy.ShownVram();
                    AssertTrue(ShowsTilemap(shownLegacy2, (sub.MainCoverage ?? throw new InvalidOperationException("Verification requires sub.MainCoverage.")).Cells, 0x7000),
                        "F2B7 keeps BG1 map and excludes backdrop color math");

                    burstDisplayFrames++;
                }
                if (legacy.Stage.Phase == EndingCreditsPhase.ZebesExplosionAnimation
                    && EndingCreditsPresentation.CaptureRenderSnapshot(legacy).LayersOf<AddedScreenTilesLayer>().Any(layer => layer.IncludeObjects))
                {
                    var finale = EndingCreditsPresentation.CaptureRenderSnapshot(legacy);
                    var backgrounds = finale.LayersOf<ScreenTilesLayer>();
                    var shownLegacy3 = legacy.ShownVram();
                    AssertTrue(backgrounds.Zip(new[] { 0x7800, 0x7400, 0x7800, 0x7400 }).All(shown => ShowsTilemap(shownLegacy3, shown.First.Cells, shown.Second)),
                        "native explosion finale selects both background maps in main-screen priority order");
                    AssertTrue(backgrounds.All(bg => ShowsCharacters(bg.Cells, legacy.LayerTiles(EndingTileLayer.Explosion, TileInkDepth.SixteenColours))),
                        "explosion finale uses Mode1 character base, not stale Mode7 decoding");
                    AssertTrue(!finale.Layers.Any(layer => layer is SpritesLayer or SpriteDepthLayer),
                        "F2FA moves OBJ exclusively to the subscreen");
                    if (hours == 2 && finaleDisplayFrames == 10)
                    {

                    }
                    finaleDisplayFrames++;
                }
                if (legacy.Stage.Phase == EndingCreditsPhase.PostCreditsGesture)
                {
                    AssertEqual(308, phaseEntryFrames[EndingCreditsPhase.PostCreditsGesture]
                        - phaseEntryFrames[EndingCreditsPhase.PostCreditsReward],
                        "gesture starts after two 64-frame reveals and the 180-frame copyright hold");
                    var gestureFrame = EndingCreditsPresentation.CaptureRenderSnapshot(legacy);
                    AssertTrue(gestureFrame.Layers.All(layer => layer is SpritesLayer),
                        "E342 gesture renders OBJ without the stale waiting or copyright backgrounds");
                }
                if (firstPhaseFrame && legacy.Stage.Phase == EndingCreditsPhase.PostCreditsJump)
                    AssertEqual(hours < 3 ? 244 : 329,
                        tick - phaseEntryFrames[EndingCreditsPhase.PostCreditsGesture],
                        "live reward owner consumes the complete native gesture before handing off");
                if (legacy.Stage.Phase == EndingCreditsPhase.PostCreditsJump)
                {
                    if (firstPhaseFrame)
                        referenceJump = new EndingRewardJump(legacy.Stage.EndingReward, _ => { });
                    else
                        (referenceJump ?? throw new InvalidOperationException("Verification requires referenceJump.")).Step();
                    var jumpFrame = EndingCreditsPresentation.CaptureRenderSnapshot(legacy);
                    // The reference jump draws from the sheet the live ending draws the page it selects from.
                    var rewardArt = EndingSheetFixtures.Drawer(legacy, referenceJump.FallingSheetSelected
                        ? EndingObjectSheet.RewardFalling : EndingObjectSheet.Reward, CinematicSpriteFamily.EndingReward);
                    var expectedOam = ScreenSpritesFixtures.Frame(objects => referenceJump.Draw(objects, rewardArt));
                    // The live ending also appends the restored shooting stars after the reference jump actors.
                    PlacedSpritePart[] jumpParts = PlacedSpritePart.Of(expectedOam.Draws);
                    PlacedSpritePart[] liveParts = [.. jumpFrame.Paint.SpriteParts().Take(jumpParts.Length)];
                    int[] mismatches = [.. Enumerable.Range(0, jumpParts.Length).Where(index => index >= liveParts.Length || liveParts[index] != jumpParts[index])];
                    AssertTrue(mismatches.Length == 0,
                        "live reward preserves native actor positions, sizes, tile indices and palettes on every jump frame" +
                        string.Concat(mismatches.Select(index => $" [part {index}: live {(index < liveParts.Length ? liveParts[index].Part.ToString() : "missing")}, native {jumpParts[index].Part}]")));
                    AssertTrue(jumpFrame.Layers.All(layer => layer is SpritesLayer),
                        "reward flight and landing keep BG disabled during icon graphics replacement");
                }
                if (firstPhaseFrame && legacy.Stage.Phase == EndingCreditsPhase.PostCreditsShot)
                {
                    AssertEqual(hours < 3 ? 279 : 200,
                        tick - phaseEntryFrames[EndingCreditsPhase.PostCreditsJump],
                        "live reward jump consumes native flight and landing durations before shooting");
                    byte[] icon = RomDataReader.Decompress(bus, EndingSourceDefinitions.Artwork.PostCreditsFloorCharacters,
                        EndingSourceDefinitions.Artwork.DecompressionLimit);
                    var shownLegacy4 = legacy.ShownVram();
                    AssertTrue(icon.AsSpan(0, 0x8000).SequenceEqual(shownLegacy4[..0x8000]),
                        "live landing uploads all sixteen chunks of the native interleaved icon graphics");
                }
                if (firstPhaseFrame && legacy.Stage.Phase == EndingCreditsPhase.PostCreditsWhiteFlash)
                    AssertEqual(216, tick - phaseEntryFrames[EndingCreditsPhase.PostCreditsShot],
                        "live post-shot owner includes rotation and the complete native hold");
                if (legacy.Stage.Phase == EndingCreditsPhase.PostCreditsWhiteFlash)
                {
                    int elapsed = tick - phaseEntryFrames[EndingCreditsPhase.PostCreditsWhiteFlash];
                    var flash = EndingCreditsPresentation.CaptureRenderSnapshot(legacy);
                    var add = flash.LayersOf<ScreenTintLayer>().Single();
                    AssertEqual((byte)Math.Max(0, 31 - elapsed), add.Red, "native post-shot white fade component");
                    var textPlane = flash.LayersOf<ScreenTilesLayer>().Single();
                    var shownLegacy5 = legacy.ShownVram();
                    // The lower screen is off screen at zero scroll until its own upload replaces it (#1404), so only the
                    // upper screen is the native one here.
                    AssertTrue(ShowsUpperScreen(shownLegacy5, textPlane.Cells, 0x4c00),
                        "final text uses native upper screen" + TilemapDifference(shownLegacy5, textPlane.Cells, 0x4c00));
                    AssertEqual(64, textPlane.MapSize.Y, "final text map is two screens tall");
                    if (firstPhaseFrame)
                        AssertTrue(legacy.Render().All(pixel => pixel == new RgbaPixel(255, 255, 255)), "post-shot flash begins fully white");
                }
                if (firstPhaseFrame && legacy.Stage.Phase == EndingCreditsPhase.PostCreditsLogo)
                    AssertEqual(32, tick - phaseEntryFrames[EndingCreditsPhase.PostCreditsWhiteFlash], "native white flash lasts 32 calls");
                if (firstPhaseFrame && legacy.Stage.Phase == EndingCreditsPhase.ItemPercentage)
                    AssertEqual(187, tick - phaseEntryFrames[EndingCreditsPhase.PostCreditsLogo], "live logo completes its native approach and palette handoff");
                if (legacy.Stage.Phase == EndingCreditsPhase.PostCreditsShot)
                {
                    int elapsed = tick - phaseEntryFrames[EndingCreditsPhase.PostCreditsShot];
                    var shotFrame = EndingCreditsPresentation.CaptureRenderSnapshot(legacy);
                    var mode = shotFrame.LayersOf<TurningFloorLayer>().Single();
                    AssertTrue(mode.SubtractsSprites, "live shot enables native BG1 minus OBJ blending");
                    AssertEqual((short)104, mode.Transform.Center.X, "post-shot matrix horizontal origin");
                    AssertEqual((short)112, mode.Transform.Center.Y, "post-shot matrix vertical origin");
                    AssertEqual((short)-4, mode.Transform.Scroll.X, "post-shot horizontal scroll");
                    AssertEqual((short)-8, mode.Transform.Scroll.Y, "post-shot vertical scroll");
                    if (hours == 2 && elapsed == 16)
                    {

                    }
                }
                if (hours < 10 && phaseEntryFrames.TryGetValue(EndingCreditsPhase.PostCreditsReward, out int rewardStart)
                    && legacy.Stage.Phase is EndingCreditsPhase.PostCreditsReward or EndingCreditsPhase.PostCreditsCopyright)
                {
                    int elapsed = tick - rewardStart;
                    int activeElapsed = elapsed <= 64 ? elapsed : elapsed < 244 ? 64 : elapsed - 180;
                    int fadeStep = activeElapsed / 4;
                    var colors = EndingCreditsPresentation.CaptureRenderSnapshot(legacy).Paint.Colors;
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
                        legacy.Stage.Phase, "native copyright holds for exactly 180 frames before resuming reward reveal");
                if (phaseEntryFrames.TryGetValue(EndingCreditsPhase.PostCreditsReward, out int revealStart)
                    && tick - revealStart == 64)
                {
                    var copyright = EndingCreditsPresentation.CaptureRenderSnapshot(legacy);
                    AssertTrue(!copyright.Layers.Any(layer => layer is SpritesLayer),
                        "native E293 hides reward actors during copyright panel");
                    var shownLegacy6 = legacy.ShownVram();
                    AssertTrue(ShowsTilemap(shownLegacy6, copyright.LayersOf<ScreenTilesLayer>().Single().Cells, 0x4800),
                        "native E293 selects copyright BG1 after first 64 reveal frames");
                }
                if (legacy.Stage.Phase is EndingCreditsPhase.WaitForPlanetEscapeMusic or EndingCreditsPhase.WaitForPlanetEscapeMusicQueue)
                {
                    var flash = EndingCreditsPresentation.CaptureRenderSnapshot(legacy);
                    AssertEqual(0, flash.Layers.Length, "native F32B disables both screens for explosion whiteout");
                    AssertEqual(0x7fff, flash.Paint.Colors[0], "native explosion whiteout backdrop");
                    if (firstPhaseFrame)
                        AssertTrue(legacy.Render().All(pixel => pixel == new RgbaPixel(255, 255, 255)),
                            "explosion whiteout is white across the actual rendered viewport");
                }
                if (legacy.Stage.Phase == EndingCreditsPhase.PostCreditsReward)
                {
                    var reveal = EndingCreditsPresentation.CaptureRenderSnapshot(legacy);
                    var backgrounds = reveal.LayersOf<ScreenTilesLayer>();
                    AssertEqual(hours >= 10 ? 0 : 1, backgrounds.Length, "native reward reveal background enable mask");
                    if (hours < 10)
                    {
                        var shownLegacy7 = legacy.ShownVram();
                        AssertTrue(ShowsTilemap(shownLegacy7, backgrounds[0].Cells, 0x4c00), "reward reveal selects waiting BG2, not producer BG1");
                        AssertTrue(ShowsCharacters(backgrounds[0].Cells, legacy.LayerTiles(EndingTileLayer.Waiting, TileInkDepth.SixteenColours)),
                            "reward reveal uses waiting BG2 characters");
                    }
                }
                if (legacy.Stage.Phase is EndingCreditsPhase.PlanetEscapeFast or EndingCreditsPhase.PlanetEscapeSlow or EndingCreditsPhase.PlanetEscapeAccelerating)
                {
                    gunshipPalettes.Add(string.Join(',', EndingCreditsPresentation.CaptureRenderSnapshot(legacy).Paint.Colors.Slice(80, 16).ToArray()));
                    int elapsed = tick - phaseEntryFrames[EndingCreditsPhase.PlanetEscapeFast];
                    int expectedWhite = elapsed <= 192 ? 31 : Math.Max(0, 30 - (elapsed - 193) / 8);
                    var flight = EndingCreditsPresentation.CaptureRenderSnapshot(legacy);
                    var fixedColors = flight.LayersOf<ScreenTintLayer>();
                    AssertEqual(expectedWhite > 0 ? 1 : 0, fixedColors.Length, "native flyaway disables fixed color after final decrement");
                    if (expectedWhite > 0)
                    {
                        AssertEqual((byte)expectedWhite, fixedColors[0].Red, "native flyaway holds 192 calls then fades every eight");
                        AssertTrue(flight.LayersOf<SpriteDepthLayer>().All(layer => layer.FixedColor == fixedColors[0]), "OBJ and backdrop share native fixed white");
                        AssertTrue(flight.Layers[0] is ScreenTintLayer && flight.Layers[1] is SpriteDepthLayer { Depth: 0 }
                            && flight.Layers[2] is TurningFloorLayer,
                            "fixed backdrop is composed before unaffected Mode7 BG1");
                    }

                }
                if (legacy.Stage.Phase == EndingCreditsPhase.OperationSuccessfulText && tick % 97 == 0)
                {
                    var memory = EndingCreditsPresentation.CaptureRenderSnapshot(legacy).Paint;
                    AssertTrue(memory.SpriteParts().All(part => part.Part.Palette <= 2),
                        "explosion actors delete before operation text, leaving only text OBJ palettes");
                }
                if (legacy.Stage.Phase >= EndingCreditsPhase.ItemPercentage)
                {
                    // E58A clears cinematic actors; the independent star field survives.
                    var stars = legacy.CreditsRoll.ShootingStars;
                    var starOnlyOam = ScreenSpritesFixtures.Blank();
                    starOnlyOam.BeginFrame();
                    // The logo has assembled by the percentage text, so the stars draw from its sheet.
                    stars?.Draw(starOnlyOam, InstalledFixtures.SpriteSheets.Get(SpriteSheetId.EndingLogoSprites));
                    starOnlyOam.FinalizeFrame();
                    var actual = EndingCreditsPresentation.CaptureRenderSnapshot(legacy);
                    AssertTrue(PlacedSpritePart.Of(starOnlyOam.Draws).SequenceEqual(actual.Paint.SpriteParts()),
                        "native E58A leaves only the independent shooting stars before final percentage text");
                }
                if (legacy.Stage.Phase == EndingCreditsPhase.PlanetEscapeFast && sample)
                {
                    var memory = EndingCreditsPresentation.CaptureRenderSnapshot(legacy).Paint;
                    byte[] vram = legacy.ShownVram();
                    byte[] characters = RomDataReader.Decompress(bus, 0x95a82f, 0x8000);
                    byte[] map = RomDataReader.Decompress(bus, 0x96fe69, 0x8000);
                    for (int word = 0; word < 0x4000; word++)
                    {
                        AssertEqual(characters[word], vram[word * 2 + 1], "native flyaway character upload");
                        AssertEqual(word < 0x300 ? map[word] : (byte)0x8c, vram[word * 2],
                            "native flyaway map upload and padded background");
                    }
                    AssertEqual(0, memory.Colors[0], "flyaway clears explosion backdrop");
                    if (firstPhaseFrame)
                        AssertEqual(0x7fff, memory.Colors[81], "native gunship emergence palette starts white");
                }
                if (legacy.Stage.Phase == EndingCreditsPhase.PostCreditsBlank)
                {
                    var memory = EndingCreditsPresentation.CaptureRenderSnapshot(legacy).Paint;
                    for (int color = 4; color < 256; color++)
                        AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), 0x8ce7e9 + color * 2),
                            memory.Colors[color], "native end-credits Intro4 palette");
                    byte[] reward = RomDataReader.Decompress(bus, hours < 3 ? 0x97b957 : 0x979803, 0x8000);
                    var shownLegacy8 = legacy.ShownVram();
                    AssertTrue(reward.AsSpan(0, 0x4000).SequenceEqual(shownLegacy8.AsSpan(0, 0x4000)),
                        "reward branch uses native contiguous OBJ characters");
                }
                DisplayPacket? packet = null;
                RgbaPixel[]? expected = null;
                if (sample)
                {
                    // IntroDiscoverySprite.Draw only writes OAM; it does not advance
                    // this ending's sprite state. Independent owners still guard against
                    // an accidental producer-side mutation introduced by extraction.
                    expected = legacy.Render();
                    if (legacy.Stage.Brightness == 15)
                    {

                    }
                    packet = new(new(++samples, 1, (ushort)tick), EndingCreditsPresentation.CaptureRenderSnapshot(captured));
                    AssertTrue(expected.SequenceEqual(RenderForComparison(packet)),
                        $"ending reward {hours}, phase {legacy.Stage.Phase}, tick {tick} pixel parity");
                }
                audio.AdvanceFrame(default); otherAudio.AdvanceFrame(default);
                AssertEqual(legacy.Stage.Phase, captured.Stage.Phase, "ending capture preserves phase");
                AssertEqual(legacy.Flyaway.CinematicFrame, captured.Flyaway.CinematicFrame, "ending capture preserves timer");
                AssertEqual(legacy.CreditsVerticalScroll, captured.CreditsVerticalScroll, "ending capture preserves half-pixel scroll result");
                if (packet is not null)
                    AssertTrue(expected.SequenceEqual(RenderForComparison(packet)), "ending packet repeat rendering is observational");
                previousPacket = packet;
                previousPixels = expected;
                if (legacy.Stage.Phase == EndingCreditsPhase.SeeYouNextMission) break;
            }
            AssertEqual(EndingCreditsPhase.SeeYouNextMission, legacy.Stage.Phase, "ending capture fixture completes");
            AssertTrue(gunshipPalettes.Count >= 10, "gunship emergence executes changing native palette records");
            AssertTrue(finaleDisplayFrames > 10, "ending executes the actual callback-driven Mode1 explosion finale");
            AssertTrue(burstDisplayFrames > 10, "ending executes the callback-driven explosion burst before its finale");
            AssertTrue(phases.Contains(EndingCreditsPhase.Credits) && phases.Contains(EndingCreditsPhase.PostCreditsReward)
                && phases.Contains(EndingCreditsPhase.ZebesExplosionAnimation), "ending covers all composition families");
            RgbaPixel[] final = legacy.Render();
            LayeredScene retained = EndingCreditsPresentation.CaptureRenderSnapshot(captured);
            for (int i = 0; i < 600; i++) captured.Step();

            AssertTrue(final.SequenceEqual(RenderForComparison(retained)), "ending final packet survives simulation advance");
        }
        Console.WriteLine($"  Ending snapshots: {samples} sampled frames cover escape, credits and all three reward branches.");
    }

    /// <summary>Whether <paramref name="cells"/> are the cells of the tilemap at <paramref name="tilemapWord"/> in the scene's memory image <paramref name="vram"/>.</summary>
    private static bool ShowsTilemap(byte[] vram, ScreenTileMap cells, int tilemapWord) =>
        ScreenTileMapConstants.Capture(vram, tilemapWord, cells.Size, cells.Tiles).Equals(cells);

    /// <summary>Whether the upper 32-row screen of <paramref name="cells"/> shows the tilemap at <paramref name="tilemapWord"/>.</summary>
    private static bool ShowsUpperScreen(byte[] vram, ScreenTileMap cells, int tilemapWord)
    {
        var screen = new Extent16<TilemapCells>(cells.Size.X, 32);
        ScreenTileMap native = ScreenTileMapConstants.Capture(vram, tilemapWord, screen, cells.Tiles);
        return native.Cells.AsSpan().SequenceEqual(cells.Cells.AsSpan(0, native.Cells.Length));
    }

    /// <summary>Where <paramref name="cells"/> first differ from the tilemap at <paramref name="tilemapWord"/>, or nothing when they match.</summary>
    private static string TilemapDifference(byte[] vram, ScreenTileMap cells, int tilemapWord)
    {
        ScreenTileMap native = ScreenTileMapConstants.Capture(vram, tilemapWord, cells.Size, cells.Tiles);
        int differing = Enumerable.Range(0, cells.Cells.Length).Where(cell => native.Cells[cell] != cells.Cells[cell]).Count();
        int first = Enumerable.Range(0, cells.Cells.Length).FirstOrDefault(cell => native.Cells[cell] != cells.Cells[cell], -1);
        return first < 0 ? "" : $" ({differing} cells differ; first at row {first / cells.Size.X}, column {first % cells.Size.X}: native {native.Cells[first]}, shown {cells.Cells[first]})";
    }

    /// <summary>Whether <paramref name="cells"/> show the tiles of <paramref name="characters"/>.</summary>
    private static bool ShowsCharacters(ScreenTileMap cells, ScreenLayerPictures characters) =>
        cells.Tiles.Depth == characters.Depth && cells.Tiles.Tiles.SequenceEqual(characters.Tiles);
}
