using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.Frontend.Cinematics;
using SuperMetroid.Verification.Frontend.Intro;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Frontend.Ending.EndingLogoTablesVerification;
using static SuperMetroid.Verification.Frontend.Map.MapSpritesVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Frontend.Intro;
using SuperMetroid.AssetExtraction.Frontend.Ending;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.RepositoryInstallation;

namespace SuperMetroid.Verification.Frontend.Ending;

/// <summary>Compares compiled ending-logo actors, sprite relationships, and palette timing with cartridge execution.</summary>
internal static class EndingLogoVerification
{
    /// <summary>Steps the logo, comparing its crossfade palettes with the cartridge tables and its timing with the native scene.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots, native OAM tables")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for the ending logo crossfade palette tables")]
    internal static void VerifyEndingLogo(ISnesAddressSpace bus)
    {
        Suite(nameof(VerifyEndingLogoDefinitions), () => VerifyEndingLogoDefinitions(bus));
        Suite(nameof(VerifyEndingLogoPrograms), () => VerifyEndingLogoPrograms(bus));
        var guarded = NewEndingLogoDefinitionReadGuard(bus);
        var cgram = new ScreenPalette();
        int landings = 0;
        EndingPaletteCatalog palettes = RepositoryInstallation.Installation.LoadEndingPalettes();
        var logoSprites = CinematicSheetFixtures.Drawer(RepositoryInstallation.Installation.LoadEndingObjectArt().LogoSprites);
        var logo = new EndingLogo(cgram, () => landings++, palettes);
        int frame = 0, fadeStart = 0;
        var poses = new HashSet<string>();
        while (!logo.Completed && frame < 300)
        {
            // The actors run their typed programs (#641), which the cinematic sprite oracle matches to the native
            // lists; the timing and palette assertions below hold them to the cartridge's scene.
            logo.Step(cgram);
            frame++;
            if (logo.CrossfadeStarted && fadeStart == 0) fadeStart = frame;
            if (logo.PaletteStep > 0)
                for (int p = 0; p < 2; p++)
                {
                    int pointer = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus),
                        EndingPaletteSources.NativeTableAddress +
                        (logo.PaletteStep - 1) * 4 + p * 2);
                    for (int i = 0; i < 16; i++)
                        AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), 0x8c0000 | (pointer - 30 + i * 2)),
                            cgram.Colors[(p == 0 ? 16 : 240) + i], "native logo crossfade palette table entry");
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

    /// <summary>Checks logo frame operands, sprite relationships, and instruction cursors against cartridge words.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for the ending logo instruction words")]
    internal static void VerifyEndingLogoPrograms(ISnesAddressSpace bus)
    {
        ushort[] operands = [0xee5f, 0xee67, 0xee73, 0xee77, 0xee7b, 0xee8d, 0xee91, 0xee95];
        string[] names = ["s-upper", "s-lower", "circle-right-1", "circle-right-2", "circle-right-3",
            "circle-left-1", "circle-left-2", "circle-left-3"];
        var catalog = EndingLogoSpriteDefinitions.Frames;
        AssertEqual(8, catalog.Count, "logo frame count");
        SpriteComposition ReadComposition(int index)
        {
            var definition = catalog[index];
            return IntroCinematicSpriteCompiler.Compile(IntroCinematicSpriteFrameExtractor.Extract(
                bus, CinematicSpritemapSources.Of(definition.Pose), definition.StockPartCount, definition.Name), definition.Name);
        }
        SpriteComposition upper = ReadComposition(0).CalculateIfMatching(new EndingLogoUpperParts()),
            completeRight = EndingLogoWrapParts.CalculateIfMatching(ReadComposition(4));
        for (int i = 0; i < operands.Length; i++)
        {
            int operand = 0x8b0000 | operands[i];
            ushort pointer = SnesLittleEndian.Word(CpuBusRegionRead.Read(bus, operand), CpuBusRegionRead.Read(bus, operand + 1));
            int header = 0x8c0000 | pointer;
            int count = SnesLittleEndian.Word(CpuBusRegionRead.Read(bus, header), CpuBusRegionRead.Read(bus, header + 1));
            AssertEqual(pointer, CinematicSpritemapSources.Of(catalog[i].Pose), "logo frame address from original instruction operand");
            AssertEqual(count, catalog[i].StockPartCount, "logo part count from original OAM header");
            AssertEqual(names[i], catalog[i].Name, "logo published asset key");
            VerifyEndingLogoRelatedParts(bus, i, catalog[i], upper, completeRight);
        }
        AssertTrue(catalog.Select(frame => frame.Name).SequenceEqual(names), "logo catalog enumeration order");
        foreach (int invalid in new[] { int.MinValue, -1, 8, int.MaxValue })
        {
            AssertThrows<IndexOutOfRangeException>(() => _ = catalog[invalid], "logo catalog bounds");
        }
    }

    /// <summary>Checks calculated logo sprite relationships while preserving supplied edits and native OAM placement.</summary>
    private static void VerifyEndingLogoRelatedParts(ISnesAddressSpace bus, int frame,
        CinematicSpriteFrameDefinition definition, SpriteComposition upper, SpriteComposition completeRight)
    {
        SpriteVisualPart[] visual = IntroCinematicSpriteFrameExtractor.Extract(bus,
            CinematicSpritemapSources.Of(definition.Pose), definition.StockPartCount, definition.Name);
        SpriteComposition supplied = IntroCinematicSpriteCompiler.Compile(visual, definition.Name);
        if (frame == 4)
        {
            for (int index = 0; index < visual.Length; index++)
            {
                var choice = EndingLogoWrapParts.StockSelection(index);
                int originalTile = visual[index].TileRow * 16 + visual[index].TileColumn;
                AssertEqual(originalTile, choice.Tile, "original wrap tile selection");
                AssertEqual(originalTile == 0x48 && visual[index].Size == 8, choice.Cropped, "original cropped cap selection");
            }
            foreach (int invalid in new[] { int.MinValue, -1, 25, int.MaxValue })
                AssertThrows<ArgumentOutOfRangeException>(() => EndingLogoWrapParts.StockSelection(invalid), "wrap selection bounds");
        }
        SpriteComposition Calculate(SpriteComposition value) => frame == 0
            ? value.CalculateIfMatching(new EndingLogoUpperParts())
            : frame == 4 ? EndingLogoWrapParts.CalculateIfMatching(value)
            : EndingLogoRelatedParts.CalculateIfMatching(frame, value, upper, completeRight);
        SpriteComposition calculated = Calculate(supplied);
        AssertTrue(!ReferenceEquals(supplied, calculated), "logo calculated relationship selection");
        string Identity(SpriteComposition value) => SelectedPresentationHash.Create("logo-related", value.AppendIdentity);
        AssertEqual(Identity(supplied), Identity(calculated), "all original logo related fields and ordering");
        if (frame == 4)
        {
            SpriteVisualPart[] reordered = (SpriteVisualPart[])visual.Clone();
            {
                var held0 = reordered[1];
                var held1 = reordered[0];
                reordered[0] = held0;
                reordered[1] = held1;
            }
            foreach (SpriteVisualPart[] editedParts in new[] { reordered, visual[..^1] })
            {
                SpriteComposition edited = IntroCinematicSpriteCompiler.Compile(editedParts, "edited wrap selection");
                SpriteComposition editedCalculation = Calculate(edited);
                AssertTrue(!ReferenceEquals(edited, editedCalculation), "edited tile sequence retains calculated placement");
                AssertEqual(Identity(edited), Identity(editedCalculation), "edited tile order and count stay supplied");
            }
        }
        foreach (ushort y in new ushort[] { 72, 0xfff8 })
        {
            var originalOam = new OamBuffer();
            var calculatedOam = new OamBuffer();
            originalOam.BeginFrame(); calculatedOam.BeginFrame();
            DrawImportedSpritemap(bus, originalOam, 0x8c0000 | CinematicSpritemapSources.Of(definition.Pose), new(120, y), 0x0800, originIsOnScreen: y == 72);
            if (y == 72) calculated.DrawOnScreen(calculatedOam, new(120, y), 0x0800);
            else calculated.DrawOffScreen(calculatedOam, new(120, y), 0x0800);
            originalOam.FinalizeFrame(); calculatedOam.FinalizeFrame();
            AssertTrue(originalOam.ImageEquals(calculatedOam),
                "logo relationship preserves native OAM and clipping");
        }
        SpriteVisualPart first = visual[0];
        foreach (SpriteVisualPart edit in new[]
        {
            first with { OffsetX = first.OffsetX + 1 }, first with { OffsetY = first.OffsetY + 1 },
            first with { TileColumn = (first.TileColumn + 1) % 14 }, first with { TileRow = first.TileRow + 1 },
            first with { Size = first.Size == 16 ? 8 : 16, TileColumn = Math.Min(14, first.TileColumn) },
            first with { Priority = 2 }, first with { Palette = 3 },
            first with { FlipX = !first.FlipX }, first with { FlipY = !first.FlipY },
        })
        {
            visual[0] = edit;
            SpriteComposition edited = IntroCinematicSpriteCompiler.Compile(visual, "edited logo");
            AssertTrue(ReferenceEquals(edited, Calculate(edited)), "independent logo artwork edits stay supplied");
        }
        foreach (int invalid in new[] { int.MinValue, -1, calculated.PartCount, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => _ = calculated.Part(invalid), "logo part bounds");
    }

    /// <summary>Checks the four logo actor definitions, origins, and palette pointer bounds.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for ending logo definitions")]
    internal static void VerifyEndingLogoDefinitions(ISnesAddressSpace bus)
    {

        for (int index = 0; index < EndingLogoDefinitions.ActorCount; index++)
        {
            CinematicActorDefinition actual = EndingLogoDefinitions.Actor(index);
            int spawn = 0x8be554 + index * 6;
            AssertEqual((byte)0xa0, bus.ReadCartridgeByte(spawn), "native LDY actor definition" );
            ushort nativePointer = CartridgeImportSource.Require(bus).ReadCartridgeWord((spawn + 1));
            int address = EndingLogoDefinitions.NativeDefinitionBank | nativePointer;
            AssertEqual(CartridgeImportSource.Require(bus).ReadCartridgeWord((address + 2)), actual.PreInstruction.NativeAddress,
                $"logo actor {index} pre-instruction callback");
            AssertEqual(CartridgeImportSource.Require(bus).ReadCartridgeWord((address + 4)), CinematicSpriteNativeListAddresses.Entry(actual.Program),
                $"logo actor {index} initial instruction list");
            int initialize = 0x8b0000 | CartridgeImportSource.Require(bus).ReadCartridgeWord(address);
            AssertEqual((byte)0xa9, bus.ReadCartridgeByte(initialize), "native LDA X origin");
            AssertEqual((byte)0xa9, bus.ReadCartridgeByte(initialize + 6), "native LDA Y origin");
            var origin = EndingLogoDefinitions.Origin(index);
            AssertEqual(CartridgeImportSource.Require(bus).ReadCartridgeWord((initialize + 1)), origin.X, "native actor X origin");
            AssertEqual(CartridgeImportSource.Require(bus).ReadCartridgeWord((initialize + 7)), origin.Y, "native actor Y origin");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 4, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => EndingLogoDefinitions.Actor(invalid), "actor definition bounds");
            AssertThrows<ArgumentOutOfRangeException>(() => EndingLogoDefinitions.Origin(invalid), "actor origin bounds");
        }
        Suite(nameof(VerifyEndingLogoPaletteSources), () => VerifyEndingLogoPaletteSources(bus));
        AssertThrows<ArgumentOutOfRangeException>(
            () => EndingLogoDefinitions.Actor(4),
            "logo actor definition boundary");
        AssertThrows<ArgumentOutOfRangeException>(
            () => EndingPaletteSources.LogoCrossfadeSource(EndingLogoDefinitions.PaletteSteps, 0),
            "logo palette step boundary");
        AssertThrows<ArgumentOutOfRangeException>(
            () => EndingPaletteSources.LogoCrossfadeSource(0, 2),
            "logo palette selector boundary");
        Console.WriteLine(
            "  Logo definitions: twelve actor words and 32 palette pointers match the cartridge.");
    }

    /// <summary>Rejects runtime reads of compiled logo instructions, palette pointers, and actor definitions.</summary>
    private static CartridgeReadGuard NewEndingLogoDefinitionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(CinematicSpriteNativeListAddresses.InListRegion,
                address => $"Ending logo reread a native cinematic sprite list byte ${address:X6}.")
            .ForbidRange(EndingPaletteSources.NativeTableAddress,
                EndingLogoDefinitions.PaletteSteps * 2 * sizeof(ushort),
                address => $"Ending logo reread palette pointer byte ${address:X6}.")
            .Forbid(address => Enumerable.Range(0, EndingLogoDefinitions.ActorCount).Any(index =>
                {
                    // The four six-byte logo definitions start at $8B:EF81.
                    int start = EndingLogoDefinitions.NativeDefinitionBank | (0xef81 + index * 6);
                    return address >= start && address < start + 3 * sizeof(ushort);
                }),
                address => $"Ending logo reread definition byte ${address:X6}.");
}
