using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyEndingCompletionTextInstructions(ISnesAddressSpace bus)
    {
        for (int pointer = EndingCompletionTextInstructionDefinitions.Start;
             pointer < EndingCompletionTextInstructionDefinitions.End; pointer += sizeof(ushort))
        {
            int address = (int)new SnesAddress(0x8b, (ushort)pointer);
            ushort nativeWord = (ushort)(bus.ReadByte(address) |
                bus.ReadByte(address + 1) << 8);
            AssertEqual(nativeWord, EndingCompletionTextInstructionDefinitions.ReadWord((ushort)pointer),
                $"ending completion text instruction $8B:{pointer:X4} matches cartridge");
        }
        AssertThrows<InvalidDataException>(() =>
            EndingCompletionTextInstructionDefinitions.ReadWord(
                EndingCompletionTextInstructionDefinitions.End),
            "ending completion reader rejects an address after the colon list");
        AssertThrows<InvalidDataException>(() =>
            EndingCompletionTextInstructionDefinitions.ReadWord(
                unchecked((ushort)(EndingCompletionTextInstructionDefinitions.Start + 1))),
            "ending completion reader rejects an unaligned address");

        var catalog = EndingCompletionTextSpriteDefinitions.Frames;
        string[] expectedKeys =
        [
            "operation-00", "operation-01", "operation-02", "operation-03", "operation-04",
            "operation-05", "operation-06", "operation-07", "operation-08", "operation-09",
            "operation-10", "operation-11", "operation-12", "operation-13", "operation-14",
            "completed-00", "completed-01", "completed-02", "completed-03", "completed-04",
            "completed-05", "completed-06", "completed-07", "completed-08", "completed-09",
            "completed-10", "completed-11", "completed-12", "completed-13", "completed-14",
            "completed-15", "completed-16", "completed-17", "completed-18", "completed-19",
            "completed-20", "clear-time-00", "clear-time-01", "clear-time-02", "clear-time-03",
            "clear-time-04", "clear-time-05", "clear-time-06", "clear-time-07", "clear-time-08",
            "digit-00", "digit-01", "digit-02", "digit-03", "digit-04",
            "digit-05", "digit-06", "digit-07", "digit-08", "digit-09",
            "colon-00",
        ];
        AssertEqual(expectedKeys.Length, catalog.Count, "completion catalog count");
        int catalogIndex = 0;
        // Original OAM headers independently establish two sprites per revealed
        // letter, so packed maps grow by two header bytes plus five per sprite.
        foreach (var line in new[] { (Start: 0xeb91, Letters: 15), (Start: 0xebd7, Letters: 21), (Start: 0xec35, Letters: 9) })
        for (int letter = 0; letter < line.Letters; letter++)
        {
            int operand = 0x8b0000 | (line.Start + letter * 4 + 2);
            int map = bus.ReadByte(operand) | bus.ReadByte(operand + 1) << 8;
            int address = 0x8c0000 | map;
            int count = bus.ReadByte(address) | bus.ReadByte(address + 1) << 8;
            AssertEqual(2 * (letter + 1), count, "original prefix sprite count");
            AssertEqual((ushort)map, catalog[catalogIndex].Pointer, "completion prefix catalog pointer");
            AssertEqual(count, catalog[catalogIndex].StockPartCount, "completion prefix catalog part count");
            catalogIndex++;
        }
        for (int glyph = 0; glyph < 11; glyph++, catalogIndex++)
        {
            int operand = 0x8bec83 + glyph * 8;
            ushort map = (ushort)(bus.ReadByte(operand) | bus.ReadByte(operand + 1) << 8);
            int header = 0x8c0000 | map;
            int parts = bus.ReadByte(header) | bus.ReadByte(header + 1) << 8;
            AssertEqual(map, catalog[catalogIndex].Pointer, "completion glyph catalog pointer");
            AssertEqual(parts, catalog[catalogIndex].StockPartCount, "completion glyph catalog part count");
        }
        for (int i = 0; i < expectedKeys.Length; i++)
        {
            AssertEqual(expectedKeys[i], catalog[i].Name, "completion published artwork key");
            VerifyEndingCompletionTextParts(bus, i, catalog[i]);
        }
        AssertTrue(catalog.Select(frame => frame.Name).SequenceEqual(expectedKeys), "completion catalog enumeration order");
        foreach (int invalid in new[] { int.MinValue, -1, 56, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => _ = catalog[invalid], "completion catalog bounds");
        foreach (ushort invalid in new ushort[] { 0, 0xeb90, 0xecd9, 0xffff })
            AssertThrows<InvalidDataException>(() => EndingCompletionTextInstructionDefinitions.ReadWord(invalid), "completion program bounds");

        ushort[] starts =
        [
            0xeb91, 0xebd7, 0xec35, 0xec81, 0xec89, 0xec91, 0xec99, 0xeca1,
            0xeca9, 0xecb1, 0xecb9, 0xecc1, 0xecc9, 0xecd1,
        ];
        foreach (ushort start in starts)
        {
            var native = new IntroDiscoverySprite(120, 72, 0x0800, start);
            var installed = new IntroDiscoverySprite(120, 72, 0x0800, start);
            for (int frame = 0; frame < 480; frame++)
            {
                // The scene owns private callbacks; here both actor interpreters
                // advance across the same callback without duplicating its effects.
                native.Step((EndingSpriteInstruction _, ushort cursor) => cursor, pointer => (ushort)(
                    bus.ReadByte(0x8b0000 | pointer) | bus.ReadByte(0x8b0000 | (pointer + 1)) << 8));
                installed.Step((EndingSpriteInstruction _, ushort cursor) => cursor,
                    EndingCompletionTextInstructionDefinitions.ReadWord);
                AssertEqual(native.InstructionPointer, installed.InstructionPointer,
                    $"completion actor ${start:X4} cursor at frame {frame}");
                AssertEqual(native.SpriteMapPointer, installed.SpriteMapPointer,
                    $"completion actor ${start:X4} visual frame at frame {frame}");
                AssertEqual(native.IsActive, installed.IsActive,
                    $"completion actor ${start:X4} lifetime at frame {frame}");
            }
        }
    }

    private static void VerifyEndingCompletionTextParts(ISnesAddressSpace bus, int frame, EndingCompletionTextSpriteFrameDefinition definition)
    {
        SpriteVisualPart[] visual = IntroCinematicSpriteFrameExtractor.Extract(bus, definition.Pointer, definition.StockPartCount, definition.Name);
        SpriteComposition supplied = IntroCinematicSpriteCompiler.Compile(visual, definition.Name);
        var parts = new EndingCompletionTextParts(frame);
        SpriteComposition calculated = supplied.CalculateIfMatching(parts);
        AssertTrue(!ReferenceEquals(supplied, calculated), "original completion text uses calculated layout");
        string Identity(SpriteComposition value) => SelectedPresentationHash.Create("completion-text", value.AppendIdentity);
        AssertEqual(Identity(supplied), Identity(calculated), "all original completion text fields and order");
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
                "completion text preserves native OAM and clipping");
        }
        SpriteVisualPart first = visual[0];
        foreach (SpriteVisualPart edit in new[]
        {
            first with { OffsetX = first.OffsetX + 1 }, first with { OffsetY = first.OffsetY + 1 },
            first with { TileColumn = (first.TileColumn + 1) % 16 }, first with { TileRow = first.TileRow + 1 },
            first with { Size = 16, TileColumn = Math.Min(14, first.TileColumn) },
            first with { Priority = 2 }, first with { Palette = 3 },
            first with { FlipX = !first.FlipX }, first with { FlipY = !first.FlipY },
        })
        {
            visual[0] = edit;
            SpriteComposition edited = IntroCinematicSpriteCompiler.Compile(visual, "edited completion text");
            AssertTrue(ReferenceEquals(edited, edited.CalculateIfMatching(parts)), "independent completion artwork edits stay supplied");
        }
        foreach (int invalid in new[] { int.MinValue, -1, parts.Count, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => _ = parts[invalid], "completion part bounds");
    }
}
