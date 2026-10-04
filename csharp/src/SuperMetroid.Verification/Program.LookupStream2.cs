using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyLookupStream2PaletteMechanics(ISnesAddressSpace rom)
    {
        int upperWords = 0, oldWords = 0, bellyWords = 0;
        for (int address = 0; address <= ushort.MaxValue; address++)
        {
            ushort pointer = (ushort)address;
            if (UpperCrateriaEscapeRedFlashPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(pointer, out ushort upper))
            {
                AssertEqual(ReadVerificationWord(rom, 0x8d0000 | pointer), upper, "Upper Crateria mechanics original word");
                upperWords++;
            }
            if (OldTourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(pointer, out ushort old))
            {
                AssertEqual(ReadVerificationWord(rom, 0x8d0000 | pointer), old, "Old Tourian mechanics original word");
                oldWords++;
            }
            if (TorizoBellyPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(pointer, out ushort belly))
            {
                AssertEqual(ReadVerificationWord(rom, 0x8d0000 | pointer), belly, "Torizo belly mechanics original word");
                bellyWords++;
            }
        }
        AssertEqual(32, upperWords, "Upper Crateria complete mechanics coverage");
        AssertEqual(60, oldWords, "Old Tourian complete mechanics coverage");
        AssertEqual(36, bellyWords, "Torizo belly complete mechanics coverage");
        int upperDuration = 0;
        for (int frame = 0; frame < 14; frame++)
        {
            ushort pointer = UpperCrateriaEscapeRedFlashPaletteFxProgramMechanicsDefinitions.FramePointer(frame);
            AssertEqual((ushort)(0xfd01 + 18 * frame), pointer, "Upper Crateria frame address");
            UpperCrateriaEscapeRedFlashPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(pointer, out ushort duration);
            upperDuration += duration;
            for (int color = 0; color < 8; color++)
            {
                int[] nativeOffsets = [2, 4, 6, 10, 12, 14, 16, 20];
                ushort actual = OldTourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions.ColorPointer(frame, color);
                AssertEqual((ushort)(0xfa6d + 24 * frame + nativeOffsets[color]), actual, "Old Tourian color address around inline skips");
                AssertTrue(!OldTourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(actual, out _), "Supplied old Tourian colors remain live");
            }
        }
        AssertEqual(63, upperDuration, "Upper Crateria cycle duration");
        var definitions = TorizoBellyPaletteFxProgramMechanicsDefinitions.All;
        AssertEqual(2, definitions.Count, "Both Torizo programs enumerated");
        for (int index = 0; index < definitions.Count; index++)
        {
            var definition = definitions[index];
            AssertEqual((TorizoBellyPaletteOwner)index, definition.Owner, "Torizo program owner ordering");
            AssertEqual(ReadVerificationWord(rom, 0x8d0000 | definition.DefinitionPointer + 2), definition.ProgramStart, "Original Torizo definition list operand");
            int total = 0;
            for (int frame = 0; frame < 6; frame++)
            {
                definition.TryReadMechanicsWord(definition.FramePointer(frame), out ushort duration);
                total += duration;
                for (int color = 0; color < 3; color++)
                    AssertTrue(!definition.TryReadMechanicsWord(definition.ColorPointer(frame, color), out _), "Supplied Torizo colors remain live");
            }
            AssertEqual(52, total, "Torizo belly cycle duration");
            AssertThrows<ArgumentOutOfRangeException>(() => definition.FramePointer(6), "Torizo frame upper bound");
            AssertThrows<ArgumentOutOfRangeException>(() => definition.FramePointer(-1), "Torizo frame lower bound");
        }
        int enumerated = 0;
        foreach (var definition in definitions)
        {
            AssertEqual(definitions[enumerated].Owner, definition.Owner, "Torizo enumerated owner");
            enumerated++;
        }
        AssertEqual(2, enumerated, "Torizo definition enumeration");
        AssertThrows<ArgumentOutOfRangeException>(() => { _ = definitions[-1]; }, "Torizo definition lower bound");
        AssertThrows<ArgumentOutOfRangeException>(() => { _ = definitions[2]; }, "Torizo definition upper bound");
        AssertThrows<ArgumentOutOfRangeException>(() => UpperCrateriaEscapeRedFlashPaletteFxProgramMechanicsDefinitions.FramePointer(-1), "Upper Crateria lower frame bound");
        AssertThrows<ArgumentOutOfRangeException>(() => UpperCrateriaEscapeRedFlashPaletteFxProgramMechanicsDefinitions.FramePointer(14), "Upper Crateria upper frame bound");
        AssertThrows<ArgumentOutOfRangeException>(() => OldTourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions.ColorPointer(0, -1), "Old Tourian lower color bound");
        AssertThrows<ArgumentOutOfRangeException>(() => OldTourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions.ColorPointer(0, 8), "Old Tourian upper color bound");
        Console.WriteLine("Stream 2 palette mechanics: original mechanics, inline color offsets, durations, dispatch and bounds pass.");
    }
    private static void VerifyLookupStream2CrystalBody(ISnesAddressSpace rom)
    {
        byte[] source = CrystalFlashColorExtractor.Extract(rom);
        var document = System.Text.Json.JsonSerializer.Deserialize<CrystalFlashColorDocument>(
            source, MapPresentationFormat.JsonOptions)!;
        var stock = CrystalFlashColorCatalog.Load(new MemoryStream(source));
        var storedBody = typeof(CrystalFlashColorCatalog).GetField("body",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
        AssertTrue(storedBody.GetValue(stock) is null, "Original Crystal Flash body selects calculation without stored rows");
        var cgram = new SnesCgram();
        for (int frame = 0; frame < CrystalFlashColorFormat.BodyFrameCount; frame++)
        {
            ushort pointer = ReadVerificationWord(rom, SamusPaletteRomData.CrystalFlash.BodyRecords + frame * 4);
            stock.ApplyBody(cgram, frame);
            for (int color = 0; color < CrystalFlashColorFormat.BodyColorCount; color++)
            {
                ushort expected = ReadVerificationWord(rom, 0x9b0000 | pointer + 2 * color);
                AssertEqual(expected, stock.ResolveBody(frame, color), "Original Crystal Flash body RGB5 word");
                AssertEqual(expected, cgram.Colors[SamusPaletteRomData.CrystalFlash.BodyCgramStart + color], "Calculated body color reaches CGRAM");
                var rows = document.Body.Select(row => (PaletteRgb5[])row.Clone()).ToArray();
                rows[frame][color] = rows[frame][color] with { Red = (rows[frame][color].Red + 1) % 32 };
                var changedDocument = document with { Body = rows };
                var changed = CrystalFlashColorCatalog.Load(new MemoryStream(CrystalFlashColorCatalog.Write(changedDocument)));
                AssertTrue(storedBody.GetValue(changed) is not null, "Independent supplied body edit retains its rows");
                for (int verifyFrame = 0; verifyFrame < rows.Length; verifyFrame++)
                {
                    changed.ApplyBody(cgram, verifyFrame);
                    for (int verifyColor = 0; verifyColor < rows[verifyFrame].Length; verifyColor++)
                    {
                        var rgb = rows[verifyFrame][verifyColor];
                        ushort word = (ushort)(rgb.Red | rgb.Green << 5 | rgb.Blue << 10);
                        AssertEqual(word, changed.ResolveBody(verifyFrame, verifyColor), "All supplied body fields preserved");
                        AssertEqual(word, cgram.Colors[SamusPaletteRomData.CrystalFlash.BodyCgramStart + verifyColor], "Supplied body color reaches CGRAM");
                    }
                }
                // Restore stock CGRAM for the remaining checks in this original frame.
                stock.ApplyBody(cgram, frame);
            }
        }
        for (int frame = 0; frame < CrystalFlashColorFormat.BubbleFrameCount; frame++)
        for (int color = 0; color < CrystalFlashColorFormat.BubbleColorCount; color++)
        {
            ushort pointer = ReadVerificationWord(rom, SamusPaletteRomData.CrystalFlash.BubblePointers + frame * 2);
            AssertEqual(ReadVerificationWord(rom, 0x9b0000 | pointer + 2 * color), stock.ResolveBubble(frame, color), "Independent bubble payload preserved");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveBody(-1, 0), "Calculated body lower frame bound");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveBody(10, 0), "Calculated body upper frame bound");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveBody(0, -1), "Calculated body lower color bound");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveBody(0, 10), "Calculated body upper color bound");
        Console.WriteLine("Stream 2 Crystal Flash body: all100 native colors, CGRAM application and independent supplied edits pass; bubble36 colors unchanged.");
    }}