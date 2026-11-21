using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.Frontend.Cinematics;
using SuperMetroid.Verification.Frontend.Intro;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Frontend.Ending.EndingLogoTablesVerification;
using static SuperMetroid.Verification.Frontend.Map.MapSpritesVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Frontend.Intro;
using SuperMetroid.AssetExtraction.Frontend.Ending;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.Verification.Frontend.Ending;

/// <summary>Compares compiled ending-logo actors, sprite compositions, and palette timing with cartridge execution.</summary>
internal static class EndingLogoVerification
{
    /// <summary>Steps the logo, comparing its crossfade palettes with the cartridge tables and its timing with the native scene.</summary>
    internal static void VerifyEndingLogo(ISnesAddressSpace bus)
    {
        Suite(nameof(VerifyEndingLogoDefinitions), () => VerifyEndingLogoDefinitions(bus));
        Suite(nameof(VerifyEndingLogoPrograms), () => VerifyEndingLogoPrograms(bus));
        var guarded = NewEndingLogoDefinitionReadGuard(bus);
        var screen = new EndingScreen();
        var stage = new EndingStage(0, screen, [], InstalledFixtures.EndingBackdrops);
        int landings = 0;
        EndingPaletteSet palettes = InstalledFixtures.EndingColours;
        IReadOnlyList<SheetShade> installedShades = EndingScreenShades.Screen(palettes, InstalledFixtures.ImportMaps.RoomPaletteFx);
        var logoSprites = CinematicSheetFixtures.Drawer(SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.EndingObjects.LogoSprites);
        var logo = new EndingLogo(stage, () => landings++);
        AssertTrue(screen.Colours.Shading.Shades.SequenceEqual([EndingShades.Loaded(EndingColourLoad.LogoInitial), EndingShades.Flashed(EndingFlash.LogoBackground)]),
            "the logo shows its first sprite colours and its background colours blacked out (#1597: loads lie under flashes)");
        int frame = 0, fadeStart = 0;
        var poses = new HashSet<string>();
        while (!logo.Completed && frame < 300)
        {
            // The actors run their typed programs (#641), which the cinematic sprite oracle matches to the native
            // lists; the timing and palette assertions below hold them to the cartridge's scene.
            logo.Step(screen.Colours);
            frame++;
            if (logo.CrossfadeStarted && fadeStart == 0) fadeStart = frame;
            if (logo.ShownCrossfade is { } shownStep)
            {
                // The screen names the step shown; the import's shade of that step holds the cartridge's table entry (#1512).
                AssertEqual(EndingShades.LogoCrossfade(shownStep), screen.Colours.Shading.Shades[^1], "the logo shows its latest crossfade step");
                SheetShade step = installedShades.Single(candidate => candidate.Shade == EndingShades.LogoCrossfade(shownStep));
                AssertTrue(step.Inks.SequenceEqual([.. Enumerable.Range(16, 16), .. Enumerable.Range(240, 16)]),
                    "a crossfade step covers the logo's background and sprite colours");
                for (int p = 0; p < 2; p++)
                {
                    int pointer = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus),
                        EndingPaletteSources.NativeTableAddress +
                        shownStep.Step * 4 + p * 2);
                    for (int i = 0; i < 16; i++)
                        AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), 0x8c0000 | (pointer - 30 + i * 2)),
                            step.Colours.ElementAt(p * 16 + i).ToWord(), "native logo crossfade palette table entry");
                }
            }
            poses.Add(Convert.ToHexString(ScreenSpritesFixtures.Frame(objects => logo.Draw(objects, logoSprites)).LowTable));
        }
        AssertTrue(logo.Completed, "logo actors reach the final palette handoff");
        AssertEqual(171, fadeStart, "native circle list waits 96+5+5+64 frames before grey-out instruction");
        AssertEqual(187, frame, "logo palette handoff follows sixteen function calls");
        AssertEqual(1, landings, "upper logo half spawns its palette FX exactly once on landing");
        AssertTrue(poses.Count >= 10, "logo approach and circle animation produce changing OAM positions/maps");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "ending logo never rereads compiled actor definitions");
        Console.WriteLine($"  Logo: {fadeStart} actor frames, sixteen exact palette pairs, {poses.Count} OAM poses.");
    }

    /// <summary>Checks logo frame operands, their compositions' OAM, and instruction cursors against cartridge words.</summary>
    internal static void VerifyEndingLogoPrograms(ISnesAddressSpace bus)
    {
        ushort[] operands = [0xee5f, 0xee67, 0xee73, 0xee77, 0xee7b, 0xee8d, 0xee91, 0xee95];
        string[] names = ["s-upper", "s-lower", "circle-right-1", "circle-right-2", "circle-right-3",
            "circle-left-1", "circle-left-2", "circle-left-3"];
        var catalog = EndingLogoSpriteDefinitions.Frames;
        AssertEqual(8, catalog.Count, "logo frame count");
        for (int i = 0; i < operands.Length; i++)
        {
            int operand = 0x8b0000 | operands[i];
            ushort pointer = LittleEndianBytes.Word(CpuBusRegionRead.Read(bus, operand), CpuBusRegionRead.Read(bus, operand + 1));
            int header = 0x8c0000 | pointer;
            int count = LittleEndianBytes.Word(CpuBusRegionRead.Read(bus, header), CpuBusRegionRead.Read(bus, header + 1));
            AssertEqual(pointer, CinematicSpritemapSources.Of(catalog[i].Pose), "logo frame address from original instruction operand");
            AssertEqual(count, catalog[i].StockPartCount, "logo part count from original OAM header");
            AssertEqual(names[i], catalog[i].Name, "logo published asset key");
            VerifyEndingLogoComposition(bus, catalog[i]);
        }
        AssertTrue(catalog.Select(frame => frame.Name).SequenceEqual(names), "logo catalog enumeration order");
        foreach (int invalid in new[] { int.MinValue, -1, 8, int.MaxValue })
        {
            AssertThrows<IndexOutOfRangeException>(() => _ = catalog[invalid], "logo catalog bounds");
        }
    }

    /// <summary>Checks the extracted logo composition draws the native OAM placement.</summary>
    private static void VerifyEndingLogoComposition(ISnesAddressSpace bus, CinematicSpriteFrameDefinition definition)
    {
        SpriteVisualPart[] visual = IntroCinematicSpriteFrameExtractor.Extract(bus,
            CinematicSpritemapSources.Of(definition.Pose), definition.StockPartCount, definition.Name);
        SpriteComposition supplied = IntroCinematicSpriteCompiler.Compile(visual, definition.Name);
        foreach (ushort y in new ushort[] { 72, 0xfff8 })
        {
            var originalOam = new OamBuffer();
            var compiledOam = new OamBuffer();
            originalOam.BeginFrame(); compiledOam.BeginFrame();
            DrawImportedSpritemap(bus, originalOam, 0x8c0000 | CinematicSpritemapSources.Of(definition.Pose), new(120, y), 0x0800, originIsOnScreen: y == 72);
            if (y == 72) supplied.DrawOnScreen(compiledOam, new(120, y), 0x0800);
            else supplied.DrawOffScreen(compiledOam, new(120, y), 0x0800);
            originalOam.FinalizeFrame(); compiledOam.FinalizeFrame();
            AssertTrue(originalOam.ImageEquals(compiledOam),
                "logo composition preserves native OAM and clipping");
        }
    }

    /// <summary>Checks the four logo actor definitions, origins, and palette pointer bounds.</summary>
    internal static void VerifyEndingLogoDefinitions(ISnesAddressSpace bus)
    {

        AssertEqual(EndingLogoDefinitions.ActorCount, EndingLogoDefinitions.Actors.Count, "the logo's named actors are the native spawns");
        for (int index = 0; index < EndingLogoDefinitions.ActorCount; index++)
        {
            EndingLogoActorDefinition actual = EndingLogoDefinitions.Actors[index];
            int spawn = 0x8be554 + index * 6;
            AssertEqual((byte)0xa0, bus.ReadCartridgeByte(spawn), "native LDY actor definition" );
            ushort nativePointer = CartridgeImportSource.Require(bus).ReadCartridgeWord((spawn + 1));
            int address = EndingLogoDefinitions.NativeDefinitionBank | nativePointer;
            AssertEqual(CartridgeImportSource.Require(bus).ReadCartridgeWord((address + 2)), index < CinematicActorNativeMotions.LogoHalves.Count ? CinematicActorNativeMotions.LogoHalves[index] : CinematicActorNativeMotions.NoMotion,
                $"logo actor {index} routine: the S halves move into place, the circles hold still");
            AssertEqual(CartridgeImportSource.Require(bus).ReadCartridgeWord((address + 4)), CinematicSpriteNativeListAddresses.Start(actual.Animation),
                $"logo actor {index} initial instruction list");
            int initialize = 0x8b0000 | CartridgeImportSource.Require(bus).ReadCartridgeWord(address);
            AssertEqual((byte)0xa9, bus.ReadCartridgeByte(initialize), "native LDA X origin");
            AssertEqual((byte)0xa9, bus.ReadCartridgeByte(initialize + 6), "native LDA Y origin");
            // The logo scene sets both camera coordinates to 256, so each native origin lies 256 pixels right of and below
            // its place on the screen.
            Offset16<ScreenPixels> origin = actual.Origin;
            AssertEqual(CartridgeImportSource.Require(bus).ReadCartridgeWord((initialize + 1)) - 256, (int)origin.X, "native actor X origin, less the logo's camera");
            AssertEqual(CartridgeImportSource.Require(bus).ReadCartridgeWord((initialize + 7)) - 256, (int)origin.Y, "native actor Y origin, less the logo's camera");
        }
        Suite(nameof(VerifyEndingLogoPaletteSources), () => VerifyEndingLogoPaletteSources(bus));
        AssertThrows<ArgumentOutOfRangeException>(
            () => EndingPaletteSources.LogoCrossfadeSource(EndingLogoDefinitions.CrossfadeSteps, SuperMetroid.AssetExtraction.Frontend.Ending.Artwork.EndingLogoLayer.Background),
            "logo palette step boundary");
        Console.WriteLine(
            "  Logo definitions: twelve actor words and 32 palette pointers match the cartridge.");
    }

    /// <summary>Rejects runtime reads of compiled logo instructions, palette pointers, and actor definitions.</summary>
    private static CartridgeReadGuard NewEndingLogoDefinitionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(CinematicSpriteNativeListAddresses.InListRegion,
                address => $"Ending logo reread a native cinematic sprite list byte ${address:X6}.")
            .ForbidRange(EndingPaletteSources.NativeTableAddress,
                EndingLogoDefinitions.CrossfadeSteps * 2 * sizeof(ushort),
                address => $"Ending logo reread palette pointer byte ${address:X6}.")
            .Forbid(address => Enumerable.Range(0, EndingLogoDefinitions.ActorCount).Any(index =>
                {
                    // The four six-byte logo definitions start at $8B:EF81.
                    int start = EndingLogoDefinitions.NativeDefinitionBank | (0xef81 + index * 6);
                    return address >= start && address < start + 3 * sizeof(ushort);
                }),
                address => $"Ending logo reread definition byte ${address:X6}.");
}
