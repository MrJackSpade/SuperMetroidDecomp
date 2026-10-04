using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyLookupStream2GhostAndNorfair(ISnesAddressSpace rom)
    {
        const System.Reflection.BindingFlags methods = System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic;
        var spawn = typeof(RoomEnemySystem).GetMethod("SpawnWreckedShipGhost", methods)!
            .CreateDelegate<Action<RoomEnemySlot, WreckedShipGhostEnemyState, ushort, ushort>>();
        var flicker = typeof(RoomEnemySystem).GetMethod("AdvanceWreckedShipGhostFlicker", methods)!
            .CreateDelegate<Action<RoomEnemySlot, WreckedShipGhostEnemyState>>();
        var slot = new RoomEnemySystem().Slots[0];
        var state = new WreckedShipGhostEnemyState(slot);
        for (int index = 0; index < 9; index++)
        {
            short originalX = unchecked((short)ReadVerificationWord(rom, 0xa89aa8 + 4 * index));
            short originalY = unchecked((short)ReadVerificationWord(rom, 0xa89aaa + 4 * index));
            var calculated = WreckedShipGhostAppearanceDefinitions.SpawnOffset(index);
            AssertEqual(originalX, calculated.X, "Ghost original horizontal spawn offset");
            AssertEqual(originalY, calculated.Y, "Ghost original vertical spawn offset");
            state.HorizontalMovementClass = (ushort)(4 * (index % 3));
            state.VerticalMovementClass = (ushort)(12 * (index / 3));
            spawn(slot, state, 32, 65520);
            AssertEqual(unchecked((ushort)(32 + originalX)), slot.XPosition, "Ghost actual wrapped spawn X");
            AssertEqual(unchecked((ushort)(65520 + originalY)), slot.YPosition, "Ghost actual wrapped spawn Y");
            AssertEqual(WreckedShipGhostAiFunction.BrighteningAndFlickering, state.Function, "Ghost appearance phase");
            AssertEqual((ushort)64, state.StablePositionTimer, "Ghost appearance position timer");
            AssertEqual((ushort)16, state.StableDirectionTimer, "Ghost appearance direction timer");
        }
        for (int offset = 0; offset < 34; offset++)
        {
            short original = unchecked((short)ReadVerificationWord(rom, 0xa89acc + 2 * (offset / 2)));
            AssertEqual(original, WreckedShipGhostAppearanceDefinitions.FlickerDuration(offset / 2), "Ghost original flicker interval");
            slot.Properties = (ushort)(EnemyProperties.Invisible | EnemyProperties.ProcessOffScreen);
            state.PhaseTimer = 1;
            state.FlickerTableOffset = (ushort)offset;
            flicker(slot, state);
            AssertEqual(original < 0 ? (ushort)0 : (ushort)original, state.PhaseTimer, "Ghost actual next interval");
            AssertEqual(original < 0 ? (ushort)0 : (ushort)(offset + 2), state.FlickerTableOffset, "Ghost actual next offset including odd folding");
            bool remainsInvisible = original < 0 || (offset & 2) != 0;
            AssertEqual((ushort)(EnemyProperties.ProcessOffScreen | (remainsInvisible ? EnemyProperties.Invisible : EnemyProperties.None)), slot.Properties, "Ghost actual visibility preserves unrelated flags");
        }
        state.PhaseTimer = 2;
        state.FlickerTableOffset = 2;
        slot.Properties = (ushort)EnemyProperties.Invisible;
        flicker(slot, state);
        AssertEqual((ushort)1, state.PhaseTimer, "Ghost interval countdown");
        AssertEqual((ushort)2, state.FlickerTableOffset, "Ghost no early interval advance");
        AssertEqual((ushort)EnemyProperties.Invisible, slot.Properties, "Ghost no early visibility change");
        state.PhaseTimer = 0;
        flicker(slot, state);
        AssertEqual((ushort)EnemyProperties.None, slot.Properties, "Ghost terminal next call clears invisibility");
        state.PhaseTimer = 1;
        state.FlickerTableOffset = 34;
        AssertThrows<InvalidDataException>(() => flicker(slot, state), "Ghost malformed offset remains rejected");
        state.HorizontalMovementClass = 1;
        state.VerticalMovementClass = 0;
        AssertThrows<InvalidDataException>(() => spawn(slot, state, 0, 0), "Ghost unaligned spawn class remains rejected");
        AssertThrows<ArgumentOutOfRangeException>(() => WreckedShipGhostAppearanceDefinitions.SpawnOffset(-1), "Ghost spawn lower bound");
        AssertThrows<ArgumentOutOfRangeException>(() => WreckedShipGhostAppearanceDefinitions.SpawnOffset(9), "Ghost spawn upper bound");
        AssertThrows<ArgumentOutOfRangeException>(() => WreckedShipGhostAppearanceDefinitions.FlickerDuration(-1), "Ghost flicker lower bound");
        AssertThrows<ArgumentOutOfRangeException>(() => WreckedShipGhostAppearanceDefinitions.FlickerDuration(17), "Ghost flicker upper bound");
        foreach (var definition in NorfairEnvironmentalPaletteFxProgramMechanicsDefinitions.All)
        {
            int total = 0;
            for (int frame = 0; frame < 16; frame++)
            {
                ushort pointer = (ushort)(definition.FramePointer(frame) + (definition.PublishesHeatPhase ? 3 : 0));
                ushort original = ReadVerificationWord(rom, 0x8d0000 | pointer);
                AssertTrue(definition.TryReadMechanicsWord(pointer, out ushort duration), "Norfair duration mechanic resolves");
                AssertEqual(original, duration, "Norfair original duration");
                total += duration;
            }
            AssertEqual(116, total, "Norfair exact cycle duration");
        }
        AssertThrows<IndexOutOfRangeException>(() => NorfairEnvironmentalPaletteFxProgramMechanicsDefinitions.Duration(-1), "Norfair duration lower bound");
        AssertThrows<IndexOutOfRangeException>(() => NorfairEnvironmentalPaletteFxProgramMechanicsDefinitions.Duration(16), "Norfair duration upper bound");
        Console.WriteLine("Ghost appearance: 18 native offset fields, 17 native intervals, actual wrapped positions/timing/visibility and malformed states pass; Norfair: 64 native durations and four 116-tick cycles pass.");
    }
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
        var storedBubble = typeof(CrystalFlashColorCatalog).GetField("bubble",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
        var residuals = (Dictionary<int, ushort>)storedBubble.GetValue(stock)!;
        AssertEqual(1, residuals.Count, "One stock bubble residual remains pending");
        AssertEqual((ushort)0x7fff, residuals[30], "Final-frame leading white remains supplied data");
        for (int frame = 0; frame < CrystalFlashColorFormat.BubbleFrameCount; frame++)
        for (int color = 0; color < CrystalFlashColorFormat.BubbleColorCount; color++)
        {
            ushort pointer = ReadVerificationWord(rom, SamusPaletteRomData.CrystalFlash.BubblePointers + frame * 2);
            ushort original = ReadVerificationWord(rom, 0x9b0000 | pointer + 2 * color);
            AssertEqual(original, stock.ResolveBubble(frame, color), "Original bubble payload preserved");
            stock.ApplyBubble(cgram, frame);
            AssertEqual(original, cgram.Colors[SamusPaletteRomData.CrystalFlash.BubbleCgramStart + color], "Original bubble color reaches CGRAM");
            var rows = document.Bubble.Select(row => (PaletteRgb5[])row.Clone()).ToArray();
            rows[frame][color] = rows[frame][color] with { Green = (rows[frame][color].Green + 1) % 32 };
            var changedDocument = document with { Bubble = rows };
            var changed = CrystalFlashColorCatalog.Load(new MemoryStream(CrystalFlashColorCatalog.Write(changedDocument)));
            for (int verifyFrame = 0; verifyFrame < rows.Length; verifyFrame++)
            {
                changed.ApplyBubble(cgram, verifyFrame);
                for (int verifyColor = 0; verifyColor < rows[verifyFrame].Length; verifyColor++)
                {
                    var rgb = rows[verifyFrame][verifyColor];
                    ushort word = (ushort)(rgb.Red | rgb.Green << 5 | rgb.Blue << 10);
                    AssertEqual(word, changed.ResolveBubble(verifyFrame, verifyColor), "All supplied bubble fields preserved");
                    AssertEqual(word, cgram.Colors[SamusPaletteRomData.CrystalFlash.BubbleCgramStart + verifyColor], "Supplied bubble color reaches CGRAM");
                }
            }
            AssertTrue(storedBody.GetValue(changed) is null, "Independent bubble edit preserves body calculation");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveBubble(-1, 0), "Bubble lower frame bound");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveBubble(6, 0), "Bubble upper frame bound");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveBubble(0, -1), "Bubble lower color bound");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveBubble(0, 6), "Bubble upper color bound");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveBody(-1, 0), "Calculated body lower frame bound");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveBody(10, 0), "Calculated body upper frame bound");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveBody(0, -1), "Calculated body lower color bound");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveBody(0, 10), "Calculated body upper color bound");
        Console.WriteLine("Stream 2 Crystal Flash body: all100 body and36 bubble native colors, CGRAM application and every independent supplied edit pass; one bubble residual remains pending.");
    }    private static void VerifyLookupStream2KraidRamps(ISnesAddressSpace rom)
    {
        PaletteRgb5[] ReadSource(KraidPaletteSource source) => Enumerable.Range(0, KraidPaletteRomData.ColorCount(source))
            .Select(index =>
            {
                ushort word = ReadVerificationWord(rom, KraidPaletteRomData.SourceAddress(source) + 2 * index);
                return new PaletteRgb5 { Red = word & 31, Green = word >> 5 & 31, Blue = word >> 10 & 31 };
            }).ToArray();
        var document = new KraidColorDocument
        {
            Version = KraidColorFormat.Version,
            RoomBackdrop = ReadSource(KraidPaletteSource.RoomBackdrop),
            InitialTarget = ReadSource(KraidPaletteSource.InitialTarget),
            Health = ReadSource(KraidPaletteSource.Health),
            Secondary = ReadSource(KraidPaletteSource.Secondary),
            DeathArm = ReadSource(KraidPaletteSource.DeathArm),
        };
        var stock = KraidColorCatalog.Load(new MemoryStream(KraidColorCatalog.Write(document)));
        const System.Reflection.BindingFlags fields = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        object health = typeof(KraidColorCatalog).GetField("health", fields)!.GetValue(stock)!;
        object secondary = typeof(KraidColorCatalog).GetField("secondary", fields)!.GetValue(stock)!;
        AssertTrue(ReferenceEquals(health, secondary), "Identical Kraid secondary colors share calculated primary source");
        var deviations = (System.Collections.IDictionary)health.GetType().GetField("deviations", fields)!.GetValue(health)!;
        AssertEqual(2, deviations.Count, "Only the two original color-six deviations remain outside interpolation");
        Check(stock, document);
        string expectedIdentity = SelectedPresentationHash.Create("enemy-kraid-colors-v1", content =>
        {
            foreach (KraidPaletteSource source in Enum.GetValues<KraidPaletteSource>())
            {
                content.Append("source", (int)source);
                content.AppendWords("colors", Source(document, source).Select(Pack).ToArray());
            }
        });
        AssertEqual(expectedIdentity, stock.ContentIdentity, "Calculated Kraid colors preserve canonical content identity");
        foreach (KraidPaletteSource source in new[] { KraidPaletteSource.Health, KraidPaletteSource.Secondary })
        for (int index = 0; index < KraidPaletteRomData.ColorCount(source); index++)
        {
            var editedColors = (PaletteRgb5[])Source(document, source).Clone();
            editedColors[index] = editedColors[index] with { Red = (editedColors[index].Red + 1) % 32 };
            var editedDocument = source == KraidPaletteSource.Health
                ? document with { Health = editedColors } : document with { Secondary = editedColors };
            var edited = KraidColorCatalog.Load(new MemoryStream(KraidColorCatalog.Write(editedDocument)));
            Check(edited, editedDocument);
            AssertTrue(edited.ContentIdentity != stock.ContentIdentity, "Every independent Kraid color edit changes content identity");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => stock.Resolve(KraidPaletteSource.Health, -1), "Kraid calculated lower bound");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.Resolve(KraidPaletteSource.Secondary, 144), "Kraid calculated upper bound");
        Console.WriteLine("Stream 2 Kraid health ramps: original336 colors, both288 edited cells, source independence and canonical identity pass; endpoints and2 deviations remain pending.");

        static ushort Pack(PaletteRgb5 rgb) => (ushort)(rgb.Red | rgb.Green << 5 | rgb.Blue << 10);
        static PaletteRgb5[] Source(KraidColorDocument value, KraidPaletteSource source) => source switch
        {
            KraidPaletteSource.RoomBackdrop => value.RoomBackdrop,
            KraidPaletteSource.InitialTarget => value.InitialTarget,
            KraidPaletteSource.Health => value.Health,
            KraidPaletteSource.Secondary => value.Secondary,
            KraidPaletteSource.DeathArm => value.DeathArm,
            _ => throw new ArgumentOutOfRangeException(nameof(source)),
        };
        static void Check(KraidColorCatalog actual, KraidColorDocument expected)
        {
            foreach (KraidPaletteSource source in Enum.GetValues<KraidPaletteSource>())
            {
                var colors = Source(expected, source);
                for (int index = 0; index < colors.Length; index++)
                    AssertEqual(Pack(colors[index]), actual.Resolve(source, index), "Every original or independently supplied Kraid color");
            }
        }
    }}