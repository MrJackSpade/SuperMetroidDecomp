using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    /// <summary>Compares the compiled ending-logo actor, instruction, palette, and OAM behavior with the retail implementation.</summary>
    /// <param name="bus">Retail address space used as the reference while compiled execution is protected by a read guard.</param>
    private static void VerifyEndingLogo(ISnesAddressSpace bus)
    {
        Suite(nameof(VerifyEndingLogoDefinitions), () => VerifyEndingLogoDefinitions(bus));
        Suite(nameof(VerifyEndingLogoPrograms), () => VerifyEndingLogoPrograms(bus));
        var guarded = new EndingLogoDefinitionReadGuard(bus);
        var cgram = new SnesCgram();
        var nativeCgram = new SnesCgram();
        int landings = 0;
        EndingPaletteCatalog palettes = RepositoryInstallation.Installation.LoadEndingPalettes();
        EndingLogoSpritePresentation logoSprites = RepositoryInstallation.Installation.LoadEndingObjectArt().LogoSprites;
        var logo = new EndingLogo(guarded, cgram, () => landings++, palettes);
        var nativeLogo = new EndingLogo(bus, nativeCgram, () => { }, palettes);
        int frame = 0, fadeStart = 0;
        var poses = new HashSet<string>();
        while (!logo.Completed && frame < 300)
        {
            // The cartridge reference reads the logo program from the ROM; the compiled
            // logo below reads the checked-in definitions.
            nativeLogo.Step(nativeCgram, EndingCartridgeInstructionWord(bus));
            logo.Step(cgram, EndingLogoInstructionDefinitions.ReadWord);
            frame++;
            AssertEqual(nativeLogo.Completed, logo.Completed,
                $"compiled logo lifetime at frame {frame}");
            AssertEqual(nativeLogo.CrossfadeStarted, logo.CrossfadeStarted,
                $"compiled logo palette handoff at frame {frame}");
            AssertEqual(nativeLogo.PaletteStep, logo.PaletteStep,
                $"compiled logo palette step at frame {frame}");
            AssertTrue(nativeCgram.Colors.SequenceEqual(cgram.Colors) &&
                    nativeLogo.Draw(logoSprites).LowTable.SequenceEqual(logo.Draw(logoSprites).LowTable) &&
                    nativeLogo.Draw(logoSprites).HighTable.SequenceEqual(logo.Draw(logoSprites).HighTable),
                $"compiled logo palette and OAM at frame {frame}");
            if (logo.CrossfadeStarted && fadeStart == 0) fadeStart = frame;
            if (logo.PaletteStep > 0)
                for (int p = 0; p < 2; p++)
                {
                    int pointer = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus),
                        EndingLogoPalettePointerDefinitions.NativeTableAddress +
                        (logo.PaletteStep - 1) * 4 + p * 2);
                    for (int i = 0; i < 16; i++)
                        AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), 0x8c0000 | (pointer - 30 + i * 2)),
                            cgram.Colors[(p == 0 ? 16 : 240) + i], "native logo crossfade palette table entry");
                }
            poses.Add(Convert.ToHexString(logo.Draw(logoSprites).LowTable));
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

    /// <summary>Checks logo sprite selectors and instruction callbacks against the native programs and frame headers.</summary>
    /// <param name="bus">Retail address space containing the original sprite and instruction data.</param>
    private static void VerifyEndingLogoPrograms(ISnesAddressSpace bus)
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
                bus, definition.Pointer, definition.StockPartCount, definition.Name), definition.Name);
        }
        SpriteComposition upper = ReadComposition(0).CalculateIfMatching(new EndingLogoUpperParts()),
            completeRight = EndingLogoWrapParts.CalculateIfMatching(ReadComposition(4));
        for (int i = 0; i < operands.Length; i++)
        {
            int operand = 0x8b0000 | operands[i];
            ushort pointer = (ushort)(bus.ReadByte(operand) | bus.ReadByte(operand + 1) << 8);
            int header = 0x8c0000 | pointer;
            int count = bus.ReadByte(header) | bus.ReadByte(header + 1) << 8;
            AssertEqual(pointer, catalog[i].Pointer, "logo frame address from original instruction operand");
            AssertEqual(count, catalog[i].StockPartCount, "logo part count from original OAM header");
            AssertEqual(names[i], catalog[i].Name, "logo published asset key");
            VerifyEndingLogoRelatedParts(bus, i, catalog[i], upper, completeRight);
        }
        AssertTrue(catalog.Select(frame => frame.Name).SequenceEqual(names), "logo catalog enumeration order");
        foreach (int invalid in new[] { int.MinValue, -1, 8, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => _ = catalog[invalid], "logo catalog bounds");
            AssertThrows<ArgumentOutOfRangeException>(() => EndingLogoSpriteDefinitions.FramePointer(invalid), "logo frame address bounds");
        }
        for (int pointer = EndingLogoInstructionDefinitions.Start;
             pointer < EndingLogoInstructionDefinitions.End; pointer += sizeof(ushort))
            AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), 0x8b0000 | pointer),
                EndingLogoInstructionDefinitions.ReadWord((ushort)pointer),
                $"ending logo instruction $8B:{pointer:X4} matches cartridge");
        AssertThrows<InvalidDataException>(() =>
            EndingLogoInstructionDefinitions.ReadWord(EndingLogoInstructionDefinitions.End),
            "logo instruction reader rejects the following definition table");
        AssertThrows<InvalidDataException>(() =>
            EndingLogoInstructionDefinitions.ReadWord(
                unchecked((ushort)(EndingLogoInstructionDefinitions.Start + 1))),
            "logo instruction reader rejects an unaligned address");
        foreach (ushort start in new ushort[] { 0xee5d, 0xee65, 0xee6d, 0xee87 })
        {
            var native = new IntroDiscoverySprite(0, 0, 0, start);
            var generated = new IntroDiscoverySprite(0, 0, 0, start);
            for (int frame = 0; frame < 200; frame++)
            {
                ushort nativeCallback = 0, generatedCallback = 0;
                native.Step(bus, (opcode, cursor) => { nativeCallback = opcode; return cursor; },
                    pointer => (ushort)(bus.ReadByte(0x8b0000 | pointer) | bus.ReadByte(0x8b0000 | (pointer + 1)) << 8));
                generated.Step(bus, (opcode, cursor) => { generatedCallback = opcode; return cursor; },
                    EndingLogoInstructionDefinitions.ReadWord);
                AssertEqual(native.InstructionPointer, generated.InstructionPointer, "logo program cursor");
                AssertEqual(native.SpriteMapPointer, generated.SpriteMapPointer, "logo program frame");
                AssertEqual(nativeCallback, generatedCallback, "logo callback timing");
            }
        }
        foreach (ushort invalid in new ushort[] { 0, 0xee5c, 0xee9b, 0xffff })
            AssertThrows<InvalidDataException>(() => EndingLogoInstructionDefinitions.ReadWord(invalid), "logo program bounds");
    }

    /// <summary>Compares calculated logo-part relationships with supplied artwork, including ordering, clipping, and independent edits.</summary>
    /// <param name="bus">Retail source used to extract the selected frame's original sprite parts.</param>
    /// <param name="frame">Frame index whose relationship rule is being verified.</param>
    /// <param name="definition">Compiled pointer, part count, and stable asset identity for the frame.</param>
    /// <param name="upper">Calculated upper-logo composition used by related-part rules.</param>
    /// <param name="completeRight">Calculated complete right-circle composition used by the wrap selection rule.</param>
    private static void VerifyEndingLogoRelatedParts(ISnesAddressSpace bus, int frame,
        EndingLogoSpriteFrameDefinition definition, SpriteComposition upper, SpriteComposition completeRight)
    {
        SpriteVisualPart[] visual = IntroCinematicSpriteFrameExtractor.Extract(bus,
            definition.Pointer, definition.StockPartCount, definition.Name);
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
            (reordered[0], reordered[1]) = (reordered[1], reordered[0]);
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
            DrawImportedSpritemap(bus, originalOam, 0x8c0000 | definition.Pointer, 120, y, 0x0800, originIsOnScreen: y == 72);
            if (y == 72) calculated.DrawOnScreen(calculatedOam, 120, y, 0x0800);
            else calculated.DrawOffScreen(calculatedOam, 120, y, 0x0800);
            originalOam.FinalizeFrame(); calculatedOam.FinalizeFrame();
            AssertTrue(originalOam.LowTable.SequenceEqual(calculatedOam.LowTable) && originalOam.HighTable.SequenceEqual(calculatedOam.HighTable),
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

    /// <summary>Checks compiled actor initialization metadata and palette-pointer boundaries against the cartridge.</summary>
    /// <param name="bus">Retail address space containing actor definitions and palette pointer tables.</param>
    private static void VerifyEndingLogoDefinitions(ISnesAddressSpace bus)
    {
        static ushort ReadWord(ISnesAddressSpace source, int address) => unchecked((ushort)(
            source.ReadByte(address) | source.ReadByte(address + 1) << 8));

        for (int index = 0; index < EndingLogoDefinitions.ActorCount; index++)
        {
            EndingLogoActorDefinition actual = EndingLogoDefinitions.Actor(index);
            int spawn = 0x8be554 + index * 6;
            AssertEqual((byte)0xa0, bus.ReadByte(spawn), "native LDY actor definition" );
            ushort nativePointer = ReadWord(bus, spawn + 1);
            int address = EndingLogoDefinitions.NativeDefinitionBank | nativePointer;
            AssertEqual(ReadWord(bus, address + 2), actual.PreInstruction,
                $"logo actor {index} pre-instruction callback");
            AssertEqual(ReadWord(bus, address + 4), actual.InstructionList,
                $"logo actor {index} initial instruction list");
            int initialize = 0x8b0000 | ReadWord(bus, address);
            AssertEqual((byte)0xa9, bus.ReadByte(initialize), "native LDA X origin");
            AssertEqual((byte)0xa9, bus.ReadByte(initialize + 6), "native LDA Y origin");
            var origin = EndingLogoDefinitions.Origin(index);
            AssertEqual(ReadWord(bus, initialize + 1), origin.X, "native actor X origin");
            AssertEqual(ReadWord(bus, initialize + 7), origin.Y, "native actor Y origin");
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
            () => EndingLogoPalettePointerDefinitions.Source(EndingLogoDefinitions.PaletteSteps, 0),
            "logo palette step boundary");
        AssertThrows<ArgumentOutOfRangeException>(
            () => EndingLogoPalettePointerDefinitions.Source(0, 2),
            "logo palette selector boundary");
        Console.WriteLine(
            "  Logo definitions: twelve actor words and 32 palette pointers match the cartridge.");
    }

    /// <summary>Prevents compiled logo execution from rereading its instruction, actor-definition, or palette-pointer tables.</summary>
    /// <param name="source">Underlying address space for reads and writes outside the compiled definition ranges.</param>
    private sealed class EndingLogoDefinitionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Number of attempted reads from instruction, actor-definition, or palette-pointer bytes owned by compiled definitions.</summary>
        public int ForbiddenReadAttempts { get; private set; }

        /// <summary>Routes a cartridge import request through the guarded read path.</summary>
        /// <param name="address">Cartridge address requested by the importer.</param>
        /// <returns>The underlying byte when the address is outside guarded ranges.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects reads from compiled logo tables and delegates other addresses to the wrapped source.</summary>
        /// <param name="address">Address requested from the wrapped memory.</param>
        /// <returns>The underlying byte for an allowed read.</returns>
        public byte ReadByte(int address)
        {
            if (address >= 0x8b0000 + EndingLogoInstructionDefinitions.Start &&
                address < 0x8b0000 + EndingLogoInstructionDefinitions.End)
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Ending logo reread compiled instruction byte ${address:X6}.");
            }
            if (address >= EndingLogoPalettePointerDefinitions.NativeTableAddress &&
                address < EndingLogoPalettePointerDefinitions.NativeTableAddress +
                EndingLogoDefinitions.PaletteSteps * 2 * sizeof(ushort))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Ending logo reread palette pointer byte ${address:X6}.");
            }
            for (int index = 0; index < EndingLogoDefinitions.ActorCount; index++)
            {
                // The four six-byte logo definitions start at $8B:EF81.
                int start = EndingLogoDefinitions.NativeDefinitionBank | (0xef81 + index * 6);
                if (address >= start && address < start + 3 * sizeof(ushort))
                {
                    ForbiddenReadAttempts++;
                    throw new InvalidOperationException(
                        $"Ending logo reread definition byte ${address:X6}.");
                }
            }
            return source.ReadByte(address);
        }

        /// <summary>Forwards a write unchanged to the wrapped address space.</summary>
        /// <param name="address">Destination address.</param>
        /// <param name="value">Byte to store.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
