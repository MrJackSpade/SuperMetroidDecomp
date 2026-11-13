using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.FrameRendering;
using static SuperMetroid.Verification.Harness.GraphDigestVerification;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Ending;
using System.Collections.Immutable;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Map;
namespace SuperMetroid.Verification.Frontend.Ending;

/// <summary>Checks retained render packets across the escape, credits, and timed reward branches.</summary>
internal static class EndingSnapshotTestsVerification
{
    /// <summary>Compares sampled packet pixels with the live ending while checking native palettes, OAM, phase timing, and later packet stability.</summary>
    internal static void VerifyEndingRenderSnapshots()
    {
        byte[] rom = File.ReadAllBytes(Path.GetFullPath("Super Metroid.smc"));
        int samples = 0;
        // The shades the import pre-colours the ending's sheets in, which the screen names (#1512), read once for every
        // reward branch.
        EndingPaletteSet installedPalettes = InstalledFixtures.EndingColours;
        IReadOnlyList<SheetShade> installedShades = EndingScreenShades.Screen(installedPalettes, InstalledFixtures.ImportMaps.RoomPaletteFx);
        foreach (ushort hours in new ushort[] { 2, 3, 10 })
        {
            var bus = new SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace(rom);
            var otherBus = new SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace(rom);
            var audio = AudioQueues.Standalone(); var otherAudio = AudioQueues.Standalone();
            var legacy = CreateRetailEndingFixture(audio, hours, 59);
            var captured = CreateRetailEndingFixture(otherAudio, hours, 59);
            var phases = new HashSet<EndingCreditsPhase>();
            var phaseEntryFrames = new Dictionary<EndingCreditsPhase, int>();
            var gunshipPalettes = new HashSet<string>();
            RgbColor[]? explosionBackgroundSource = null;
            int finaleDisplayFrames = 0;
            int burstDisplayFrames = 0;
            EndingRewardJump? referenceJump = null;
            // A sampled packet's whole description, every value it reaches through owned or shared state, is digested when
            // captured and again after the next update (#1532).
            DisplayPacket? previousPacket = null;
            byte[] previousHeld = [];
            for (int tick = 0; tick < 60000; tick++)
            {
                legacy.Step(); captured.Step();
                // The live ending's frame this update, captured once for every check of it: a capture does not advance the
                // ending, which the capture-only control checks below (#1532).
                LayeredScene? liveScene = null;
                LayeredScene Live() => liveScene ??= EndingCreditsPresentation.CaptureRenderSnapshot(legacy);
                if (previousPacket is not null)
                    AssertTrue(previousHeld.AsSpan().SequenceEqual(GraphDigest(previousPacket)),
                        "ending packet survives subsequent palette/tile/sprite updates");
                bool firstPhaseFrame = phases.Add(legacy.Stage.Phase);
                if (firstPhaseFrame) phaseEntryFrames.Add(legacy.Stage.Phase, tick);
                if (legacy.Stage.Phase is EndingCreditsPhase.FadeInZebesExplosion or EndingCreditsPhase.ZebesExplosionPaletteCrossfade)
                {
                    int elapsed = tick - phaseEntryFrames[EndingCreditsPhase.FadeInZebesExplosion];
                    var explosion = Live();
                    // The screen names each group's fade level on the cartridge's cadence (#1512): the far background
                    // keeps the escape's colours until its first step, and the planet's sprite groups start at level zero.
                    ImmutableArray<PictureShade> shown = legacy.screen.Colours.Shading.Shades;
                    int step = elapsed / 2;
                    AssertTrue(step == 0
                            ? !shown.Any(shade => shade.Effect == EndingShades.Faded(EndingFade.ExplosionBackground, 0).Effect)
                            : shown.Contains(EndingShades.Faded(EndingFade.ExplosionBackground, 32 - step)),
                        "native explosion BG palette fades out on the same cadence");
                    AssertTrue(shown.Contains(EndingShades.Faded(EndingFade.ExplosionLava, step)) && shown.Contains(EndingShades.Faded(EndingFade.ExplodingPlanet, step)),
                        "native explosion OBJ palette fades in over both fade-in and crossfade phases");
                    if (explosionBackgroundSource is null)
                    {
                        // The import's levels scale the escape's far background and the cartridge's explosion sprite colours.
                        explosionBackgroundSource = [.. installedPalettes[EndingPaletteId.Escape].Colours.Skip(112).Take(16)];
                        for (int level = 0; level <= 32; level++)
                        {
                            SheetShade background = installedShades.Single(shade => shade.Shade == EndingShades.Faded(EndingFade.ExplosionBackground, level));
                            for (int i = 0; i < 16; i++)
                            {
                                int remaining = level;
                                AssertEqual(explosionBackgroundSource[i].FadeLevels.Map((_, channel) => channel * remaining / 32).ToColour(), background.Colours[i],
                                    "the import's explosion BG fade level scales the escape's far background");
                            }
                            foreach ((EndingFade fade, int start) in new[] { (EndingFade.ExplosionLava, 208), (EndingFade.ExplodingPlanet, 240) })
                            {
                                SheetShade sprite = installedShades.Single(shade => shade.Shade == EndingShades.Faded(fade, level));
                                AssertTrue(sprite.Inks.SequenceEqual(Enumerable.Range(start, 16)), "an explosion OBJ fade level covers its colour group");
                                for (int i = start; i < start + 16; i++)
                                {
                                    ushort source = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), 0x8cebe9 + i * 2);
                                    ushort expectedColor = (ushort)(((source & 31) * level / 32)
                                        | (((source >> 5 & 31) * level / 32) << 5)
                                        | (((source >> 10 & 31) * level / 32) << 10));
                                    AssertEqual(expectedColor, sprite.Colours[i - start].ToWord(), "the import's explosion OBJ fade level scales the cartridge's colours");
                                }
                            }
                        }
                    }
                    AssertTrue(explosion.LayersOf<SpritesLayer>().Single().Blend == SpriteBlend.Glows,
                        "explosion transition's sprites glow over the planet and the sky");
                }
                if (firstPhaseFrame && legacy.Stage.Phase == EndingCreditsPhase.ZebesExplosionHold)
                    AssertEqual(64, tick - phaseEntryFrames[EndingCreditsPhase.FadeInZebesExplosion],
                        "native shared explosion countdown includes screen fade-in and expires after 64 calls");
                if (firstPhaseFrame && legacy.Stage.Phase == EndingCreditsPhase.PostCreditsWaitingSamus)
                    AssertEqual(212, tick - phaseEntryFrames[EndingCreditsPhase.PostCreditsShootingStars],
                        "native post-credits backdrop has 32 fade frames followed by 180 waiting frames before producer text");
                if (legacy.Stage.Phase == EndingCreditsPhase.PostCreditsShootingStars)
                {
                    int elapsed = tick - phaseEntryFrames[EndingCreditsPhase.PostCreditsShootingStars];
                    SheetShade waiting = installedShades.Single(shade => shade.Shade == EndingShades.Faded(EndingFade.WaitingBackdrop, elapsed));
                    AssertTrue(legacy.screen.Colours.Shading.Shades.Contains(waiting.Shade), "the screen names the waiting backdrop's fade level");
                    RgbColor[] colors = [.. Enumerable.Repeat(RgbColor.Black, 32), .. waiting.Colours];
                    for (int i = 32; i < 48; i++)
                    {
                        ushort source = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), 0x8ce7e9 + i * 2);
                        int expectedColor = ((source & 31) * elapsed / 32)
                            | (((source >> 5 & 31) * elapsed / 32) << 5)
                            | (((source >> 10 & 31) * elapsed / 32) << 10);
                        AssertEqual((ushort)expectedColor, colors[i].ToWord(), "native 8.8 waiting-backdrop palette fade");
                    }
                }
                bool sample = firstPhaseFrame || tick % 97 == 0;
                if (legacy.Stage.Phase == EndingCreditsPhase.ZebesExplosionAnimation &&
                    Live().LayersOf<AddedPicturedPlaneLayer>().Any(layer => layer.GlowsOverSprites is not null))
                {
                    var burst = Live();
                    var sub = burst.LayersOf<AddedPicturedPlaneLayer>().Single();
                    AssertTrue(ShowsOnly(legacy, sub.Plane, SpriteSheetId.EndingExplosionPictures, EndingPictureNames.ExplosionMap(EndingObjectFragmentId.Blank)),
                        "F2B7 selects burst BG2 map");
                    AssertTrue(ShowsOnly(legacy, (sub.GlowsOver ?? throw new InvalidOperationException("Verification requires sub.GlowsOver.")).Plane,
                            SpriteSheetId.EndingExplosionPictures, EndingPictureNames.ExplosionMap(EndingObjectFragmentId.WideExplosion)),
                        "the burst glows over the planet's map, not over the sky");

                    burstDisplayFrames++;
                }
                if (legacy.Stage.Phase == EndingCreditsPhase.ZebesExplosionAnimation
                    && Live().LayersOf<AddedPicturedPlaneLayer>().Any(layer => layer is { GlowsOver: null, GlowsOverSprites: null }))
                {
                    var finale = Live();
                    var backgrounds = finale.LayersOf<PicturedPlaneLayer>();
                    AssertTrue(backgrounds.Length == 2 && backgrounds.Zip(new[] { EndingObjectFragmentId.Eclipse, EndingObjectFragmentId.ConcentricExplosion })
                            .All(shown => ShowsOnly(legacy, shown.First.Plane, SpriteSheetId.EndingExplosionPictures, EndingPictureNames.ExplosionMap(shown.Second))),
                        "the explosion finale shows its glow's map behind the planet's");
                    AssertTrue(finale.LayersOf<SpritesLayer>().All(layer => layer.Blend == SpriteBlend.Glows),
                        "the finale's debris only glows, never covering the planet");
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
                    var gestureFrame = Live();
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
                        referenceJump = new EndingRewardJump(legacy.Stage.EndingReward);
                    else
                        (referenceJump ?? throw new InvalidOperationException("Verification requires referenceJump.")).Step();
                    var jumpFrame = Live();
                    // The reference jump draws the jumping Samus from the reward's sheet and the falling Samus from hers (#1626).
                    var rewardArt = EndingSheetFixtures.Drawer(legacy, EndingSpriteSheets.Reward(legacy.Stage.EndingReward), CinematicSpriteFamily.EndingReward);
                    var fallingArt = EndingSheetFixtures.Drawer(legacy, EndingSpriteSheets.FallingSamusAndLogo, CinematicSpriteFamily.EndingReward);
                    var expectedOam = ScreenSpritesFixtures.Frame(objects => referenceJump.Draw(objects, rewardArt, fallingArt));
                    // The live ending also appends the restored shooting stars after the reference jump actors.
                    PlacedSpritePart[] jumpParts = PlacedSpritePart.Of(expectedOam.Draws);
                    PlacedSpritePart[] liveParts = [.. jumpFrame.SpriteParts().Take(jumpParts.Length)];
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
                    var flash = Live();
                    var add = flash.LayersOf<ScreenTintLayer>().Single();
                    AssertEqual(WhiteWash.Colour(Math.Max(0, WhiteWash.Steps - elapsed)), add.Colour, "native post-shot white wash, one step a frame");
                    var textPlane = flash.LayersOf<PicturedPlaneLayer>().Single();
                    // E48A clears the copyright before the final page is first shown, so the final page shows no text yet.
                    AssertTrue(textPlane.Plane.Pictures.IsEmpty, "final text page starts cleared");
                    // The text is one screen at screen pixels, moved down without wrapping (#1555), not a two-screen page.
                    AssertEqual(Offset16<PlanePixels>.Zero, textPlane.Scroll, "the final text plane is not scrolled");
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
                    var shotFrame = Live();
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
                    foreach (int start in hours < 3 ? new[] { 32, 208 } : new[] { 32, 208, 224 })
                    {
                        int factor = start == 32 ? 32 - fadeStep : fadeStep;
                        EndingFade group = start == 32 ? EndingFade.WaitingBackdrop : start == 208 ? EndingFade.SuitlessSamus : EndingFade.SuitedSamus;
                        SheetShade level = installedShades.Single(shade => shade.Shade == EndingShades.Faded(group, factor));
                        AssertTrue(legacy.screen.Colours.Shading.Shades.Contains(level.Shade), "the screen names the reward dissolve's fade level");
                        for (int i = start; i < start + 16; i++)
                        {
                            ushort source = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), 0x8ce7e9 + i * 2);
                            ushort expectedColor = (ushort)(((source & 31) * factor / 32)
                                | (((source >> 5 & 31) * factor / 32) << 5)
                                | (((source >> 10 & 31) * factor / 32) << 10));
                            AssertEqual(expectedColor, level.Colours[i - start].ToWord(), "native reward dissolve pauses and resumes around copyright");
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
                    var copyright = Live();
                    AssertTrue(!copyright.Layers.Any(layer => layer is SpritesLayer),
                        "native E293 hides reward actors during copyright panel");
                    AssertTrue(ShowsOnly(legacy, copyright.LayersOf<PicturedPlaneLayer>().Single().Plane, SpriteSheetId.EndingPostCreditsPictures,
                            EndingPictureNames.CopyrightPanel),
                        "native E293 selects copyright BG1 after first 64 reveal frames");
                }
                if (legacy.Stage.Phase is EndingCreditsPhase.WaitForPlanetEscapeMusic or EndingCreditsPhase.WaitForPlanetEscapeMusicQueue)
                {
                    var flash = Live();
                    AssertEqual(0, flash.Layers.Length, "native F32B disables both screens for explosion whiteout");
                    AssertEqual(0x7fff, flash.Paint.Backdrop.ToWord(), "native explosion whiteout backdrop");
                    if (firstPhaseFrame)
                        AssertTrue(legacy.Render().All(pixel => pixel == new RgbaPixel(255, 255, 255)),
                            "explosion whiteout is white across the actual rendered viewport");
                }
                if (legacy.Stage.Phase == EndingCreditsPhase.PostCreditsReward)
                {
                    var reveal = Live();
                    var backgrounds = reveal.LayersOf<PicturedPlaneLayer>();
                    AssertEqual(hours >= 10 ? 0 : 1, backgrounds.Length, "native reward reveal background enable mask");
                    if (hours < 10)
                    {
                        AssertTrue(ShowsOnly(legacy, backgrounds[0].Plane, SpriteSheetId.EndingPostCreditsPictures, EndingPictureNames.WaitingBackdrop),
                            "reward reveal selects the waiting backdrop, not the producer panel");
                    }
                }
                if (legacy.Stage.Phase is EndingCreditsPhase.PlanetEscapeFast or EndingCreditsPhase.PlanetEscapeSlow or EndingCreditsPhase.PlanetEscapeAccelerating)
                {
                    gunshipPalettes.Add(string.Join(',', legacy.screen.Colours.Shading.Shades
                        .Where(shade => shade.Effect == PaletteFxShades.Of(PaletteFxColours.GunshipEmergingFromExplosion, new PaletteFxFrame(PaletteFxProgram.GunshipEmergingFromZebesExplosion, 0)).Effect)));
                    int elapsed = tick - phaseEntryFrames[EndingCreditsPhase.PlanetEscapeFast];
                    int expectedWhite = elapsed <= 192 ? WhiteWash.Steps : Math.Max(0, WhiteWash.Steps - 1 - (elapsed - 193) / 8);
                    var flight = Live();
                    var fixedColors = flight.LayersOf<ScreenTintLayer>();
                    AssertEqual(expectedWhite > 0 ? 1 : 0, fixedColors.Length, "native flyaway disables fixed color after final decrement");
                    if (expectedWhite > 0)
                    {
                        AssertEqual(WhiteWash.Colour(expectedWhite), fixedColors[0].Colour, "native flyaway holds 192 calls then fades a step every eight");
                        RgbColor white = fixedColors[0].Colour;
                        AssertTrue(flight.LayersOf<SpritesLayer>().All(layer => layer.Tint == white), "the sprites and the sky share the flyaway's white");
                        AssertTrue(flight.Layers[0] is ScreenTintLayer && flight.Layers[1] is SpritesLayer
                            && flight.Layers[2] is TurningFloorLayer,
                            "the white sky and the sprites behind the gunship are composed before its untinted floor");
                    }

                }
                if (legacy.Stage.Phase == EndingCreditsPhase.OperationSuccessfulText && tick % 97 == 0)
                {
                    AssertTrue(Live().SpriteParts().All(part => part.Part.Palette <= 2),
                        "explosion actors delete before operation text, leaving only text OBJ palettes");
                }
                if (legacy.Stage.Phase >= EndingCreditsPhase.ItemPercentage)
                {
                    // E58A clears cinematic actors; the independent star field survives.
                    var stars = legacy.CreditsRoll.ShootingStars;
                    var starOnlyOam = ScreenSpritesFixtures.Blank();
                    starOnlyOam.BeginFrame();
                    // The stars draw from the reward's sheet, whatever scene shows them (#1626).
                    stars?.Draw(starOnlyOam, InstalledFixtures.SpriteSheets.Get(EndingSpriteSheets.Reward(legacy.Stage.EndingReward)), legacy.screen.Colours.Drawn(EndingShades.Drawn(EndingSpriteColouring.ShootingStars)));
                    starOnlyOam.FinalizeFrame();
                    var actual = Live();
                    AssertTrue(PlacedSpritePart.Of(starOnlyOam.Draws).SequenceEqual(actual.SpriteParts()),
                        "native E58A leaves only the independent shooting stars before final percentage text");
                }
                if (legacy.Stage.Phase == EndingCreditsPhase.PlanetEscapeFast && sample)
                {
                    var memory = Live().Paint;
                    byte[] vram = legacy.ShownVram();
                    byte[] characters = RomDataReader.Decompress(bus, 0x95a82f, 0x8000);
                    byte[] map = RomDataReader.Decompress(bus, 0x96fe69, 0x8000);
                    for (int word = 0; word < 0x4000; word++)
                    {
                        AssertEqual(characters[word], vram[word * 2 + 1], "native flyaway character upload");
                        AssertEqual(word < 0x300 ? map[word] : (byte)0x8c, vram[word * 2],
                            "native flyaway map upload and padded background");
                    }
                    AssertEqual(RgbColor.Black, memory.Backdrop, "flyaway clears explosion backdrop");
                    if (firstPhaseFrame)
                    {
                        // Every picture coloured from the gunship's emergence colour shows it white (#1512).
                        RgbColor[] emergence = SceneShownColours.ShownAt(Live(), 81);
                        AssertTrue(emergence.Length > 0 && emergence.All(colour => colour == new RgbColor(255, 255, 255)),
                            "native gunship emergence palette starts white");
                    }
                }
                if (legacy.Stage.Phase == EndingCreditsPhase.PostCreditsBlank)
                {
                    // Every picture the blank shows, at every place of the native image it is coloured from, shows the
                    // cartridge's Intro4 colour there (#1512); a place no picture is coloured from is not shown.
                    LayeredScene blank = Live();
                    IReadOnlyList<IReadOnlyList<ShownColourSource>> blankSources = SceneShownColours.SourcesByPlace(blank);
                    for (int color = 4; color < 256; color++)
                    {
                        RgbColor native = RgbColor.FromWord(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), 0x8ce7e9 + color * 2));
                        ShownColourSource[] wrong = [.. blankSources[color].Where(source => source.Colour != native)];
                        AssertTrue(wrong.Length == 0, $"native end-credits Intro4 palette colour {color} ({native}): " +
                            string.Join("; ", wrong.Select(source => $"{source.Picture} colour key {source.Key} under {source.Shading} shows {source.Colour}")));
                    }
                    byte[] reward = RomDataReader.Decompress(bus, hours < 3 ? 0x97b957 : 0x979803, 0x8000);
                    var shownLegacy8 = legacy.ShownVram();
                    AssertTrue(reward.AsSpan(0, 0x4000).SequenceEqual(shownLegacy8.AsSpan(0, 0x4000)),
                        "reward branch uses native contiguous OBJ characters");
                }
                DisplayPacket? packet = null;
                byte[] held = [];
                if (sample)
                {
                    // IntroDiscoverySprite.Draw only writes OAM; it does not advance
                    // this ending's sprite state. Independent owners still guard against
                    // an accidental producer-side mutation introduced by extraction.
                    var expected = new DisplayPacket(tick, Live());
                    samples++;
                    packet = new(tick, EndingCreditsPresentation.CaptureRenderSnapshot(captured));
                    held = GraphDigest(packet);
                    AssertTrue(ScenePixels.Same(expected, packet),
                        $"ending reward {hours}, phase {legacy.Stage.Phase}, tick {tick} pixel parity");
                }
                audio.AdvanceFrame(default); otherAudio.AdvanceFrame(default);
                AssertEqual(legacy.Stage.Phase, captured.Stage.Phase, "ending capture preserves phase");
                AssertEqual(legacy.Flyaway.CinematicFrame, captured.Flyaway.CinematicFrame, "ending capture preserves timer");
                AssertEqual(legacy.CreditsVerticalScroll, captured.CreditsVerticalScroll, "ending capture preserves half-pixel scroll result");
                previousPacket = packet;
                previousHeld = held;
                if (legacy.Stage.Phase == EndingCreditsPhase.SeeYouNextMission) break;
            }
            AssertEqual(EndingCreditsPhase.SeeYouNextMission, legacy.Stage.Phase, "ending capture fixture completes");
            AssertTrue(gunshipPalettes.Count >= 10, "gunship emergence executes changing native palette records");
            AssertTrue(finaleDisplayFrames > 10, "ending executes the actual callback-driven Mode1 explosion finale");
            AssertTrue(burstDisplayFrames > 10, "ending executes the callback-driven explosion burst before its finale");
            AssertTrue(phases.Contains(EndingCreditsPhase.Credits) && phases.Contains(EndingCreditsPhase.PostCreditsReward)
                && phases.Contains(EndingCreditsPhase.ZebesExplosionAnimation), "ending covers all composition families");
            LayeredScene final = EndingCreditsPresentation.CaptureRenderSnapshot(legacy);
            LayeredScene retained = EndingCreditsPresentation.CaptureRenderSnapshot(captured);
            AssertTrue(ScenePixels.Same(final, retained), "ending final packet shows the legacy frame's pixels");
            byte[] retainedHeld = GraphDigest(retained);
            for (int i = 0; i < 600; i++) captured.Step();

            AssertTrue(retainedHeld.AsSpan().SequenceEqual(GraphDigest(retained)), "ending final packet survives simulation advance");
        }
        Console.WriteLine($"  Ending snapshots: {samples} sampled frames cover escape, credits and all three reward branches.");
    }

    /// <summary>Whether <paramref name="plane"/> shows exactly the installed picture <paramref name="picture"/> of <paramref name="sheet"/> (#1467).</summary>
    private static bool ShowsOnly(EndingCreditsState ending, PicturedPlane plane, SpriteSheetId sheet, string picture) =>
        plane.Pictures is [var only] && only.Picture == ending.InstalledArtwork.SpriteSheet(sheet).Frame(picture);
}
