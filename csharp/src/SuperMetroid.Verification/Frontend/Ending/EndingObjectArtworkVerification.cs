using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Ending;
using SuperMetroid.Core.Frontend;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.Frontend.Cinematics;
using SuperMetroid.Verification.Frontend.Intro;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Frontend.Ending.EndingCompletionTextInstructionsVerification;
using static SuperMetroid.Verification.Frontend.Ending.EndingCompletionTextSpriteArtworkVerification;
using static SuperMetroid.Verification.Frontend.Ending.EndingExplosionSpriteArtworkVerification;
using static SuperMetroid.Verification.Frontend.Ending.EndingLogoSpriteArtworkVerification;
using static SuperMetroid.Verification.Frontend.Ending.EndingRewardInstructionsVerification;
using static SuperMetroid.Verification.Frontend.Ending.EndingRewardSpriteArtworkVerification;
using static SuperMetroid.Verification.Frontend.Map.MapSpritesVerification;
using static SuperMetroid.Verification.Frontend.Ending.PostCreditsCharacterArtworkVerification;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Frontend.Intro;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.FrameRendering;
using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction.Rooms.Artwork;
using SuperMetroid.AssetExtraction;

namespace SuperMetroid.Verification.Frontend.Ending;

/// <summary>Checks installed ending actors and sheets against cartridge sources and guarded runtime use.</summary>
internal static class EndingObjectArtworkVerification
{
    /// <summary>Matches four explosion-fragment source pointers and filenames to their native loader operands.</summary>
    internal static void VerifyEndingObjectFragmentMetadata(ISnesAddressSpace bus)
    {
        string[] names = ["ending-explosion-fragment-70.json", "ending-explosion-fragment-74.json",
            "ending-explosion-fragment-78.json", "ending-explosion-fragment-7c.json"];
        AssertEqual(names.Length, EndingObjectArtworkFormat.FragmentCount, "ending fragment count");
        for (int index = 0; index < names.Length; index++)
        {
            int load = 0x8bd5af + 17 * index;
            AssertEqual((byte)0xa9, bus.ReadCartridgeByte(load), "native fragment bank LDA");
            AssertEqual((byte)0xa9, bus.ReadCartridgeByte(load + 5), "native fragment address LDA");
            int source = (int)new SnesAddress(bus.ReadCartridgeByte(load + 2), CartridgeImportSource.Require(bus).ReadCartridgeWord(load + 6));
            AssertEqual(source, EndingArtworkSources.ObjectFragment((EndingObjectFragmentId)index), "native fragment compressed source");
            AssertEqual(names[index], EndingObjectArtworkFormat.FragmentFileName(index), "published fragment filename");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 4, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => EndingObjectArtworkFormat.FragmentFileName(invalid), "fragment filename bounds");
            AssertThrows<ArgumentOutOfRangeException>(() => EndingArtworkSources.ObjectFragment((EndingObjectFragmentId)invalid), "fragment source bounds");
        }
    }

    /// <summary>Compares the six cloud compositions with native OAM.</summary>
    internal static void VerifyEndingCloudDefinitions(ISnesAddressSpace bus)
    {
        string[] names = ["top-cloud-pattern", "top-cloud-edge", "bottom-cloud-edge", "bottom-cloud-pattern", "right-side-cloud", "left-side-cloud"];
        var catalog = EndingCloudSpriteDefinitions.Frames;
        AssertEqual(6, catalog.Count, "cloud catalog count");
        for (int index = 0; index < catalog.Count; index++)
        {
            int operand = 0x8becef + index * 8;
            ushort map = CartridgeImportSource.Require(bus).ReadCartridgeWord(operand);
            int header = 0x8c0000 | map;
            int count = CartridgeImportSource.Require(bus).ReadCartridgeWord(header);
            AssertEqual(map, CinematicSpritemapSources.Of(catalog[index].Pose), "cloud native pointer operand");
            AssertEqual(count, catalog[index].StockPartCount, "cloud native OAM count");
            AssertEqual(names[index], catalog[index].Name, "cloud published asset key");
            SpriteVisualPart[] visual = IntroCinematicSpriteFrameExtractor.Extract(bus, map, count, names[index]);
            SpriteComposition supplied = IntroCinematicSpriteCompiler.Compile(visual, names[index]);
            foreach (ushort y in new ushort[] { 72, 0xfff8 })
            {
                var originalOam = new OamBuffer();
                var compiledOam = new OamBuffer();
                originalOam.BeginFrame(); compiledOam.BeginFrame();
                DrawImportedSpritemap(bus, originalOam, header, new(120, y), 0x0800, originIsOnScreen: y == 72);
                if (y == 72) supplied.DrawOnScreen(compiledOam, new(120, y), 0x0800);
                else supplied.DrawOffScreen(compiledOam, new(120, y), 0x0800);
                originalOam.FinalizeFrame(); compiledOam.FinalizeFrame();
                AssertTrue(originalOam.ImageEquals(compiledOam),
                    "cloud composition preserves native OAM and clipping");
            }
        }
        AssertTrue(catalog.Select(frame => frame.Name).SequenceEqual(names), "cloud catalog enumeration order");
    }

    /// <summary>Checks ending OBJ sheets, sprite actors, backgrounds, and guarded runtime reads.</summary>
    internal static void VerifyEndingObjectArtwork()
    {
        ImportEndingObjectArtwork stock = SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.EndingObjects;
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        Suite(nameof(VerifyEndingObjectFragmentMetadata), () => VerifyEndingObjectFragmentMetadata(bus));
        AssertSheet(stock.Clouds, EndingSourceDefinitions.Artwork.EscapeCloudCharacters,
            EndingObjectArtworkFormat.CloudByteCount, "clouds");
        AssertSheet(stock.Explosion, EndingSourceDefinitions.Artwork.ExplosionObjectCharacters,
            EndingObjectArtworkFormat.ExplosionByteCount, "explosion");
        for (int index = 0; index < EndingObjectArtworkFormat.FragmentCount; index++)
            AssertMap(stock.Fragment((EndingObjectFragmentId)index),
                EndingArtworkSources.ObjectFragment((EndingObjectFragmentId)index), EndingObjectArtworkFormat.FragmentByteCount,
                $"explosion fragment {index}");
        AssertSheet(stock.WaitingSamus,
            EndingSourceDefinitions.Artwork.WaitingForCreditsCharacters,
            EndingObjectArtworkFormat.RewardByteCount, "waiting Samus");
        AssertSheet(stock.ShootingScreen,
            EndingSourceDefinitions.Artwork.ShootingScreenCharacters,
            EndingObjectArtworkFormat.RewardByteCount, "shooting screen");
        AssertSheet(stock.SuitlessSamus,
            EndingSourceDefinitions.Artwork.SuitlessSamusCharacters,
            EndingObjectArtworkFormat.RewardByteCount, "suitless Samus");
        byte[] nativeWaitingMap = RomDataReader.Decompress(bus,
            EndingSourceDefinitions.Artwork.WaitingForCreditsTilemap,
            SuperMetroid.AssetExtraction.Frontend.Ending.EndingTransferExtentDefinitions.ObjectFragmentLimit);
        AssertTrue(BackgroundTileWordBytes.Of(stock.WaitingTilemap.Tiles.Span).AsSpan().SequenceEqual(
                nativeWaitingMap.AsSpan(0, EndingObjectArtworkFormat.WaitingTilemapByteCount)),
            "installed waiting-Samus map preserves all native BG2 tile words");
        AssertSheet(stock.PostCreditsFragmentA,
            EndingSourceDefinitions.Artwork.TransformationCharacters,
            EndingObjectArtworkFormat.PostCreditsFragmentAByteCount,
            "post-credits fragment A");
        AssertMap(stock.PostCreditsFragmentB,
            EndingSourceDefinitions.Artwork.TransformationTilemap,
            EndingObjectArtworkFormat.PostCreditsFragmentBByteCount,
            "post-credits fragment B");
        AssertSheet(stock.PostShotLogoTiles, EndingSourceDefinitions.Artwork.PostShotLogoTiles,
            EndingObjectArtworkFormat.PostShotLogoTileByteCount,
            "post-shot logo tiles");
        byte[] nativeLogoMap = RomDataReader.Decompress(bus,
            EndingSourceDefinitions.Artwork.PostShotLogoMap,
            EndingSourceDefinitions.Artwork.DecompressionLimit);
        AssertTrue(BackgroundTileWordBytes.Of(stock.PostShotLogoMap.Tiles.Span).AsSpan().SequenceEqual(
                nativeLogoMap.AsSpan(0, EndingObjectArtworkFormat.PostShotLogoMapByteCount)),
            "installed post-shot logo map preserves all native BG2 tile words");

        Suite(nameof(VerifyEndingCloudDefinitions), () => VerifyEndingCloudDefinitions(bus));
        for (int index = 0; index < EndingCloudSpriteDefinitions.Frames.Count; index++)
        {
            CinematicSpriteFrameDefinition definition = EndingCloudSpriteDefinitions.Frames[index];
            foreach (ushort y in new ushort[] { 0x0048, 0xfff8 })
            {
                bool onScreen =
                    (short)y >= 0;
                int source = (int)new SnesAddress(
                    IntroCinematicSourceDefinitions.SpritemapBank, CinematicSpritemapSources.Of(definition.Pose));
                var nativeOam = new OamBuffer();
                nativeOam.BeginFrame();
                if (onScreen)
                    DrawImportedSpritemap(bus, nativeOam, source, new(120, y), 0x0800);
                else
                    DrawImportedSpritemap(bus, nativeOam, source, new(120, y), 0x0800, originIsOnScreen: false);
                nativeOam.FinalizeFrame();
                var installedOam = ScreenSpritesFixtures.Blank();
                installedOam.BeginFrame();
                CinematicSheetFixtures.Drawer(stock.CloudSprites).Draw(definition.Pose, installedOam,
                    new(120, y), EndingShades.Drawn(EndingSpriteColouring.ShootingStars));
                installedOam.FinalizeFrame();
                AssertTrue(nativeOam.ImageEquals(installedOam) &&
                        nativeOam.LastFinalizedSpriteCount == installedOam.LastFinalizedSpriteCount,
                    $"ending cloud {definition.Name} at Y=${y:X4} preserves cartridge OAM ({nativeOam.FirstDifference(installedOam)}; " +
                    $"{nativeOam.LastFinalizedSpriteCount} vs {installedOam.LastFinalizedSpriteCount} sprites)");
            }
        }
        Suite(nameof(VerifyEndingExplosionActorArtwork), () => VerifyEndingExplosionActorArtwork(bus, stock));
        Suite(nameof(VerifyEndingCompletionTextInstructions), () => VerifyEndingCompletionTextInstructions(bus));
        Suite(nameof(VerifyEndingCompletionTextSpriteArtwork), () => VerifyEndingCompletionTextSpriteArtwork(bus, stock));
        Suite(nameof(VerifyEndingRewardInstructions), () => VerifyEndingRewardInstructions(bus));
        Suite(nameof(VerifyEndingRewardSpriteArtwork), () => VerifyEndingRewardSpriteArtwork(bus, stock));
        Suite(nameof(VerifyEndingLogoSpriteArtwork), () => VerifyEndingLogoSpriteArtwork(bus, stock));

        var guard = NewEndingObjectSourceReadGuard(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc"));
        var nativeAudio = AudioQueues.Standalone();
        var installedAudio = AudioQueues.Standalone();
        var native = CreateRetailEndingFixture(nativeAudio, 0, 0);
        var installed = CreateRetailEndingFixture(installedAudio, 0, 0);
        installed.InstalledArtwork.BindFixtureObjectArtwork(stock);
        var phases = new HashSet<EndingCreditsPhase>();
        for (int frame = 0; frame < 12000 &&
            native.Stage.Phase != EndingCreditsPhase.FadeOutToCredits; frame++)
        {
            AssertEqual(native.Stage.Phase, installed.Stage.Phase,
                $"installed ending OBJ art preserves phase at frame {frame}");
            if (phases.Add(native.Stage.Phase) || frame % 211 == 0)
            {
                LayeredScene expected = EndingCreditsPresentation.CaptureRenderSnapshot(native);
                RgbColor[] expectedColours = EndingShownColours.Of(native);
                byte[] expectedVram = native.ShownVram();
                LayeredScene actual = EndingCreditsPresentation.CaptureRenderSnapshot(installed);
                RgbColor[] actualColours = EndingShownColours.Of(installed);
                byte[] actualVram = installed.ShownVram();
                AssertTrue(actualVram.SequenceEqual(expectedVram) &&
                        actualColours.SequenceEqual(expectedColours) &&
                        ScenePixels.Same(actual, expected),
                    $"ending OBJ PNGs preserve native VRAM/pixels at frame {frame}");
            }
            native.Step();
            installed.Step();
            nativeAudio.AdvanceFrame(default);
            installedAudio.AdvanceFrame(default);
        }
        AssertEqual(EndingCreditsPhase.FadeOutToCredits, native.Stage.Phase,
            "ending OBJ fixture reaches the completed-time credits handoff");
        AssertEqual(EndingCreditsPhase.FadeOutToCredits, installed.Stage.Phase,
            "installed OBJ art retains the native completed-time credits handoff");
        AssertTrue(phases.Contains(EndingCreditsPhase.WaitForEscapeMusic) &&
                phases.Contains(EndingCreditsPhase.FadeInTopAndBottomCloudEscape) &&
                phases.Contains(EndingCreditsPhase.FadeInZebesExplosion) &&
                phases.Contains(EndingCreditsPhase.PlanetEscapeFast) &&
                phases.Contains(EndingCreditsPhase.OperationSuccessfulText),
            "ending clouds, explosion and completion text were exercised in their actual scenes");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "installed ending OBJ scenes never reread compiled instructions, sprite maps or character sources");

        EndingCloudSpriteDocument cloudDocument =
            JsonSerializer.Deserialize<EndingCloudSpriteDocument>(
                File.ReadAllBytes(Path.Combine(SuperMetroid.TestSupport.RepositoryInstallation.Verification.CartridgeImportContent.ImportData(SuperMetroid.AssetExtraction.Installation.GameInstallationLayout.EndingObjectDirectoryName), EndingCloudSpriteFormat.FileName)), ArtworkJson.Options) ??
            throw new InvalidDataException("Stock ending cloud sprite JSON is empty.");
        cloudDocument.Frames.Remove("right-side-cloud");
        AssertThrows<InvalidDataException>(() =>
        {
            using var invalid = new MemoryStream();
            EndingCloudSpritePresentation.Write(invalid, cloudDocument);
            _ = EndingCloudSpritePresentation.Load(new MemoryStream(invalid.ToArray(), writable: false));
        }, "ending cloud sprites reject missing named art on read");
        Suite(nameof(VerifyEndingExplosionSpritesRejectMissingFrame), () => VerifyEndingExplosionSpritesRejectMissingFrame());
        Suite(nameof(VerifyEndingCompletionTextSpritesRejectMissingFrame), () => VerifyEndingCompletionTextSpritesRejectMissingFrame());
        Suite(nameof(VerifyEndingRewardSpritesRejectMissingFrame), () => VerifyEndingRewardSpritesRejectMissingFrame());
        Suite(nameof(VerifyEndingLogoSpritesRejectMissingFrame), () => VerifyEndingLogoSpritesRejectMissingFrame());
        Suite(nameof(VerifyPostCreditsCharacterArtwork), () => VerifyPostCreditsCharacterArtwork());
        Console.WriteLine("Ending/credits art: compiled cloud, explosion, completion-text, reward and logo actors, twelve native sheets and two BG maps, codec rejections and guarded runtime pass.");

        void AssertSheet(RoomCharacterAtlas sheet, int source, int bytes, string name)
        {
            byte[] native = RomDataReader.Decompress(bus, source,
                EndingSourceDefinitions.Artwork.DecompressionLimit);
            AssertTrue(sheet.Characters.Span.SequenceEqual(native.AsSpan(0, bytes)),
                $"installed ending {name} PNG preserves every native OBJ byte");
        }

        void AssertMap(RoomBackgroundTilemapAtlas map, int source, int bytes, string name)
        {
            byte[] native = RomDataReader.Decompress(bus, source,
                EndingSourceDefinitions.Artwork.DecompressionLimit);
            AssertTrue(BackgroundTileWordBytes.Of(map.Tiles.Span).AsSpan().SequenceEqual(native.AsSpan(0, bytes)),
                $"installed ending {name} map, encoded through the codec, preserves every native tilemap byte");
        }
    }

    /// <summary>Forbids runtime reads of compiled ending lists, installed spritemaps, and extracted art sources.</summary>
    internal static MutableCartridgeReadGuard NewEndingObjectSourceReadGuard(ISnesAddressSpace source)
    {
        var guard = new MutableCartridgeReadGuard(source);
        guard.Forbid(CinematicSpriteNativeListAddresses.InListRegion,
            address => $"Ending actor reread native cinematic sprite list ${address:X6}.");
        guard.Forbid(address => EndingCloudSpriteDefinitions.Frames.Any(frame =>
                {
                    int start = (int)new SnesAddress(
                        IntroCinematicSourceDefinitions.SpritemapBank, CinematicSpritemapSources.Of(frame.Pose));
                    return address >= start && address < start + 2 + frame.StockPartCount * 5;
                }),
            address => $"Ending cloud reread installed spritemap ${address:X6}.");
        guard.Forbid(address => EndingExplosionSpriteDefinitions.Frames.Any(frame =>
                {
                    int start = (int)new SnesAddress(
                        IntroCinematicSourceDefinitions.SpritemapBank, CinematicSpritemapSources.Of(frame.Pose));
                    return address >= start && address < start + 2 + frame.StockPartCount * 5;
                }),
            address => $"Ending explosion reread installed spritemap ${address:X6}.");
        guard.Forbid(address => EndingCompletionTextSpriteDefinitions.Frames.Any(frame =>
                {
                    int start = (int)new SnesAddress(
                        IntroCinematicSourceDefinitions.SpritemapBank, CinematicSpritemapSources.Of(frame.Pose));
                    return address >= start && address < start + 2 + frame.StockPartCount * 5;
                }),
            address => $"Ending completion text reread installed spritemap ${address:X6}.");
        guard.Forbid(address => EndingRewardSpriteDefinitions.Frames.Any(frame =>
                {
                    int start = (int)new SnesAddress(
                        IntroCinematicSourceDefinitions.SpritemapBank, CinematicSpritemapSources.Of(frame.Pose));
                    return address >= start && address < start + 2 + frame.StockPartCount * 5;
                }),
            address => $"Ending reward reread installed spritemap ${address:X6}.");
        guard.Forbid(address => EndingLogoSpriteDefinitions.Frames.Any(frame =>
                {
                    int start = (int)new SnesAddress(
                        IntroCinematicSourceDefinitions.SpritemapBank, CinematicSpritemapSources.Of(frame.Pose));
                    return address >= start && address < start + 2 + frame.StockPartCount * 5;
                }),
            address => $"Ending logo reread installed spritemap ${address:X6}.");
        guard.Forbid(address => address is EndingSourceDefinitions.Artwork.EscapeCloudCharacters or
                EndingSourceDefinitions.Artwork.ExplosionObjectCharacters or
                EndingSourceDefinitions.Artwork.WideExplosionTilemap or
                EndingSourceDefinitions.Artwork.ConcentricExplosionTilemap or
                EndingSourceDefinitions.Artwork.EclipseTilemap or
                EndingSourceDefinitions.Artwork.BlankTilemap or
                EndingSourceDefinitions.Artwork.WaitingForCreditsCharacters or
                EndingSourceDefinitions.Artwork.ShootingScreenCharacters or
                EndingSourceDefinitions.Artwork.SuitlessSamusCharacters or
                EndingSourceDefinitions.Artwork.WaitingForCreditsTilemap or
                EndingSourceDefinitions.Artwork.TransformationCharacters or
                EndingSourceDefinitions.Artwork.TransformationTilemap or
                EndingSourceDefinitions.Artwork.PostCreditsFloorCharacters or
                EndingSourceDefinitions.Artwork.PostShotLogoTiles or
                EndingSourceDefinitions.Artwork.PostShotLogoMap,
            address => $"Ending OBJ reread cartridge source ${address:X6}.");
        return guard;
    }

}
