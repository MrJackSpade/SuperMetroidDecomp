using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Rooms;
using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyLookupStream4CeresAlarm(ISnesAddressSpace rom)
    {
        ushort Native(int row, int color)
        {
            int address = 0xA6C1DF + (row * 3 + color) * 2;
            return (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        }
        byte[] json = CeresRidleyColorExtractor.Extract(rom);
        var document = JsonSerializer.Deserialize<CeresRidleyColorDocument>(json, MapPresentationFormat.JsonOptions)!;
        var stock = CeresRidleyColorCatalog.Load(new MemoryStream(json));
        var definition = (CeresRidleyAlarmColorDefinitions)typeof(CeresRidleyColorCatalog)
            .GetField("alarm", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(stock)!;
        var inputs = (Dictionary<int, ushort>)typeof(CeresRidleyAlarmColorDefinitions)
            .GetField("inputs", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(definition)!;
        AssertEqual(27, inputs.Count, "alarm stores exactly its required forward basis with zero stock reflected overrides");
        for (int row = 0; row < 16; row++)
        for (int color = 0; color < 3; color++)
        {
            AssertEqual(Native(row, color), Native(CeresRidleyAlarmColorDefinitions.SourceRow(row), color), "direct reflected/native relationship");
            AssertEqual(row <= 8, inputs.ContainsKey(row * 3 + color), "exact required source membership");
            AssertEqual(Native(row, color), stock.ResolveAlarm(row, color), "installed native alarm color");
        }
        for (int edit = -1; edit < 48; edit++)
        {
            var selected = stock;
            if (edit >= 0)
            {
                var rows = document.Alarm!.Select(row => row.ToArray()).ToArray();
                rows[edit / 3][edit % 3] = rows[edit / 3][edit % 3] with { Red = rows[edit / 3][edit % 3].Red ^ 1 };
                selected = CeresRidleyColorCatalog.Load(new MemoryStream(CeresRidleyColorCatalog.Write(document with { Alarm = rows })));
            }
            for (int row = 0; row < 16; row++)
            {
                var cgram = new SnesCgram();
                cgram.SetColor(96, 0x1234); cgram.SetColor(100, 0x2345);
                selected.ApplyAlarm(cgram, row);
                for (int color = 0; color < 3; color++)
                {
                    ushort expected = (ushort)(Native(row, color) ^ (row * 3 + color == edit ? 1 : 0));
                    AssertEqual(expected, selected.ResolveAlarm(row, color), "independent alarm source/derived edit");
                    AssertEqual(expected, cgram.Colors[97 + color], "actual alarm CGRAM write");
                    AssertEqual(Native(row, color), stock.ResolveAlarm(row, color), "stock alarm remains immutable");
                }
                AssertEqual((ushort)0x1234, cgram.Colors[96], "alarm preceding CGRAM unchanged");
                AssertEqual((ushort)0x2345, cgram.Colors[100], "alarm following CGRAM unchanged");
            }
        }
        foreach (int version in new[] { 1, 2 })
        {
            byte[] legacyJson = JsonSerializer.SerializeToUtf8Bytes(document with { Version = version, Alarm = null }, MapPresentationFormat.JsonOptions);
            var legacy = CeresRidleyColorCatalog.Load(new MemoryStream(legacyJson), stock);
            for (int row = 0; row < 16; row++)
            for (int color = 0; color < 3; color++)
                AssertEqual(Native(row, color), legacy.ResolveAlarm(row, color), "legacy alarm fallback preserves immutable calculated source");
        }
        foreach (int row in new[] { -1, 16, int.MinValue, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveAlarm(row, 0), "alarm row bounds");
        foreach (int color in new[] { -1, 3, int.MinValue, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveAlarm(0, color), "alarm color bounds");
        AssertThrows<ArgumentNullException>(() => stock.ApplyAlarm(null!, 0), "alarm null CGRAM");
        Console.WriteLine("Ceres alarm:48 native colors, exact27-word required basis, all48 independent edits,784 actual row writes, legacy fallback and bounds pass.");
    }
    private static void VerifyLookupStream4MaridiaPaletteDefinitions(CartridgeImportAddressSpace rom)
    {
        ushort Word(int pointer) => (ushort)(rom.ReadByte(0x8D0000 | pointer) | rom.ReadByte(0x8D0000 | (pointer + 1)) << 8);
        var all = MaridiaEnvironmentalPaletteFxProgramMechanicsDefinitions.All;
        AssertEqual(3, all.Count, "three native environmental owners");
        int colors = 0;
        for (int index = 0; index < 3; index++)
        {
            var definition = all[index];
            int pointer = Word(0xF795 + index * 4 + 2);
            AssertEqual((ushort)pointer, definition.ProgramStart, "native definition selects calculated setup");
            AssertEqual((MaridiaEnvironmentalPaletteOwner)index, definition.Owner, "owner order");
            AssertEqual(Word(pointer + 2), definition.ColorByteIndex, "native destination palette bytes");
            pointer += 4;
            int frame = 0;
            while (Word(pointer) < 0x8000)
            {
                AssertEqual((ushort)pointer, definition.FramePointer(frame), "native timed record stride");
                AssertEqual(Word(pointer), definition.Duration, "required native cadence");
                pointer += 2;
                int color = 0;
                while (Word(pointer) != PaletteFxInstructionCodes.Wait)
                {
                    AssertEqual((ushort)pointer, definition.ColorPointer(frame, color), "native color identity");
                    color++; colors++; pointer += 2;
                }
                AssertEqual(definition.ColorsPerFrame, color, "native color group extent");
                pointer += 2; frame++;
            }
            AssertEqual(definition.FrameCount, frame, "native rotation cycle extent");
            AssertEqual((ushort)pointer, definition.LoopInstructionPointer, "native loop opcode location");
            AssertEqual(PaletteFxInstructionCodes.Goto, Word(pointer), "native goto identity");
            AssertEqual(definition.FirstFramePointer, Word(pointer + 2), "native loop target");
            AssertThrows<ArgumentOutOfRangeException>(() => definition.FramePointer(-1), "negative frame rejection");
            AssertThrows<ArgumentOutOfRangeException>(() => definition.FramePointer(frame), "exhausted frame rejection");
            AssertThrows<ArgumentOutOfRangeException>(() => definition.ColorPointer(0, color: -1), "negative color rejection");
            AssertThrows<ArgumentOutOfRangeException>(() => definition.ColorPointer(0, definition.ColorsPerFrame), "exhausted color rejection");
        }
        AssertEqual(112, colors, "native presentation words remain independently owned");
        foreach (int index in new[] { -1, 3, int.MinValue, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => _ = all[index], "owner selector bounds");
        VerifyMaridiaEnvironmentalPaletteFxProgramMechanicsDefinitions(rom);
        Console.WriteLine("Maridia palette definitions: three native programs,44 mechanics words,112 color identities and actual complete loops pass; timing/destination/group inputs remain required.");
    }
    private static void VerifyLookupStream4EndingShake(ISnesAddressSpace rom)
    {
        int Native(int address)
        {
            ushort whole = (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
            ushort fraction = (ushort)(rom.ReadByte(address + 2) | rom.ReadByte(address + 3) << 8);
            return unchecked((int)((uint)whole << 16 | fraction));
        }
        for (int index = 0; index < 16; index++)
            AssertEqual(Native(0x8BDD02 + index * 4), EndingCreditsRomData.Motion.PlanetFastDelta(index), "native fast shake signed fixed-point value");
        for (int index = 0; index < 8; index++)
            AssertEqual(Native(0x8BDDAD + index * 4), EndingCreditsRomData.Motion.PlanetSlowDelta(index), "native slow shake signed fixed-point value");
        AssertEqual(32768, EndingCreditsRomData.Motion.PlanetFastDelta(9), "native DD26 fast sample9 is positive half-pixel");
        AssertEqual(-32768, EndingCreditsRomData.Motion.PlanetFastDelta(11), "native DD2E fast sample11 is negative half-pixel");
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        foreach (bool fast in new[] { true, false })
        {
            int count = fast ? 16 : 8, address = fast ? 0x8BDD02 : 0x8BDDAD;
            var state = new EndingCreditsState(new FrontendCartridgeReadGuard(rom), new SuperMetroid.Core.Audio.CartridgeAudioState(), 0, 0);
            var type = typeof(EndingCreditsState);
            void Set(string name, object value) => type.GetField(name, flags)!.SetValue(state, value);
            ushort Get(string name) => (ushort)type.GetField(name, flags)!.GetValue(state)!;
            Set("mode7X", (ushort)100); Set("mode7XSubposition", (ushort)0xC000);
            Set("mode7Zoom", (ushort)0xC00); Set("phaseTimer", 100);
            type.GetProperty(nameof(EndingCreditsState.Phase))!.SetValue(state,
                fast ? EndingCreditsPhase.PlanetEscapeFast : EndingCreditsPhase.PlanetEscapeSlow);
            var step = type.GetMethod(fast ? "StepPlanetEscapeFast" : "StepPlanetEscapeSlow", flags)!.CreateDelegate<Action>(state);
            uint expected = 100u << 16 | 0xC000;
            for (int frame = 0; frame <= count; frame++)
            {
                expected = unchecked(expected + (uint)Native(address + (frame % count) * 4));
                step();
                AssertEqual((ushort)(expected >> 16), Get("mode7X"), "production shake whole position follows native indexed values");
                AssertEqual((ushort)expected, Get("mode7XSubposition"), "production shake fraction preserves carry/borrow");
                AssertEqual((ushort)((frame + 1) % count), Get("planetMotionIndex"), "production shake advances after selection and wraps native mask");
            }
        }
        foreach (int index in new[] { -1, 16, int.MinValue, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => EndingCreditsRomData.Motion.PlanetFastDelta(index), "fast shake bounds");
        foreach (int index in new[] { -1, 8, int.MinValue, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => EndingCreditsRomData.Motion.PlanetSlowDelta(index), "slow shake bounds");
        Console.WriteLine("Ending shake:24 native signed values, corrected samples9/11, both production cycles/index wraps and fixed-point carries pass.");
    }
    private static void VerifyLookupStream4SporeHealthyAlias(ISnesAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        byte[] source = SporeSpawnColorExtractor.Extract(rom);
        var stock = SporeSpawnColorCatalog.Load(new MemoryStream(source));
        var values = (Dictionary<int, ushort>)typeof(SporeSpawnColorCatalog).GetField("spores",
            BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(stock)!;
        AssertEqual(0, values.Count, "Stock spores retain no duplicate healthy-body colors");
        for (int color = 0; color < 16; color++)
            AssertEqual(Word(0xa5e379 + color * 2), Word(0xa5e359 + color * 2), "Original separate spore and healthy-body rows have identical coloring");
        Verify(stock, false, false);
        foreach (bool editSpore in new[] { false, true })
        {
            var document = JsonSerializer.Deserialize<SporeSpawnColorDocument>(source,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
            if (editSpore) document.Spores[5] = document.Spores[5] with { Red = document.Spores[5].Red ^ 1 };
            else document.Health[0][5] = document.Health[0][5] with { Red = document.Health[0][5].Red ^ 1 };
            var edited = SporeSpawnColorCatalog.Load(new MemoryStream(SporeSpawnColorCatalog.Write(document)));
            Verify(edited, editSpore, !editSpore);
            Verify(stock, false, false);
        }
        foreach (int invalid in new[] { int.MinValue, -1, 16, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveSpore(invalid), "Spore alias color bounds remain exact");
        Console.WriteLine("Spore healthy-palette alias: all16 native words, zero stock overrides and separate spore/healthy-body edits pass.");

        void Verify(SporeSpawnColorCatalog catalog, bool editSpore, bool editHealth)
        {
            ushort[] Read(int address) => Enumerable.Range(0, 16).Select(color => Word(address + color * 2)).ToArray();
            ushort[][] Rows(int address, int count) => Enumerable.Range(0, count).Select(frame => Read(address + frame * 32)).ToArray();
            ushort[] spores = Read(0xa5e359);
            ushort[][] health = Rows(0xa5e379, 4);
            if (editSpore) spores[5] ^= 1;
            if (editHealth) health[0][5] ^= 1;
            for (int color = 0; color < 16; color++)
            {
                AssertEqual(spores[color], catalog.ResolveSpore(color), "Independent health edit does not recolor spores");
                for (int frame = 0; frame < 4; frame++)
                    AssertEqual(health[frame][color], catalog.ResolveHealth(frame, color), "Independent spore edit leaves all body health rows intact");
            }
            string identity = SelectedPresentationHash.Create("SporeSpawnColorCatalog-v1", content =>
            {
                content.AppendWords("spores", spores);
                content.AppendWordFrames("health", health);
                content.AppendWordFrames("deathSprite", Rows(0xa5e3f9, 8));
                content.AppendWordFrames("deathLevel", Rows(0xa5e4f9, 7));
                content.AppendWordFrames("deathBackground", Rows(0xa5e5d9, 7));
            });
            AssertEqual(identity, catalog.ContentIdentity, "Healthy-palette alias preserves canonical content hash for independent edits");
        }
    }
    private static void VerifyLookupStream4SporeLevelFade(ISnesAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        byte[] palette = SuperMetroid.Core.Rom.RomDataReader.Decompress(
            SuperMetroid.Core.Rom.CartridgeImportSource.Require(rom), SporeSpawnDeathColorDefinitions.OriginalRoomPaletteSource);
        var basis = (ushort[])typeof(SporeSpawnDeathColorDefinitions).GetField("InitialRoomLevelColors",
            BindingFlags.Static | BindingFlags.NonPublic)!.GetValue(null)!;
        AssertEqual(12, basis.Length, "Level fade retains twelve required original room-color sites");
        byte[] source = SporeSpawnColorExtractor.Extract(rom);
        var stock = SporeSpawnColorCatalog.Load(new MemoryStream(source));
        var values = (Dictionary<int, ushort>)typeof(SporeSpawnColorCatalog).GetField("deathLevel",
            BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(stock)!;
        AssertEqual(40, values.Count, "Level fade stores28 unresolved trajectory words and12 other final colors");
        for (int color = 0; color < 16; color++)
        {
            bool unresolved = color is 0 or 7 or 11 or 15;
            if (!unresolved)
            {
                int offset = SporeSpawnDeathColorDefinitions.LevelPaletteByteOffset + color * 2;
                AssertEqual((ushort)(palette[offset] | palette[offset + 1] << 8),
                    SporeSpawnDeathColorDefinitions.InitialLevelColor(color), "Level basis matches exact original decoded room color");
            }
            for (int frame = 0; frame < 7; frame++)
            {
                AssertEqual(frame == 6 || unresolved, values.ContainsKey(frame * 16 + color), "Only explicit unresolved columns and final colors remain stored");
                bool calculated = SporeSpawnDeathColorDefinitions.TryLevelColor(Word(0xa5e5b9 + color * 2), frame, color, out ushort value);
                AssertEqual(!unresolved, calculated, "Required trajectories are never disguised as calculated defaults");
                if (calculated)
                    AssertEqual(Word(0xa5e4f9 + frame * 32 + color * 2), value, "Direct level interpolation matches original native words");
            }
        }
        Verify(stock, -1, -1);
        foreach (var selected in new[] { (Frame: 6, Color: 3), (Frame: 2, Color: 3), (Frame: 1, Color: 0), (Frame: 3, Color: 7), (Frame: 4, Color: 11), (Frame: 5, Color: 15) })
        {
            var document = JsonSerializer.Deserialize<SporeSpawnColorDocument>(source,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
            var old = document.DeathLevel[selected.Frame][selected.Color];
            document.DeathLevel[selected.Frame][selected.Color] = old with { Red = old.Red ^ 1 };
            var edited = SporeSpawnColorCatalog.Load(new MemoryStream(SporeSpawnColorCatalog.Write(document)));
            Verify(edited, selected.Frame, selected.Color);
            Verify(stock, -1, -1);
        }
        foreach (int invalid in new[] { int.MinValue, -1, 7, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveDeathLevel(invalid, 0), "Level frame bounds remain exact");
        foreach (int invalid in new[] { int.MinValue, -1, 16, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveDeathLevel(0, invalid), "Level color bounds remain exact");
        Console.WriteLine("Spore level fade: twelve required room-color sites, forty required death words and72 calculated interiors; native values, exact basis, edits and identity pass.");

        void Verify(SporeSpawnColorCatalog catalog, int editedFrame, int editedColor)
        {
            ushort[] Read(int address) => Enumerable.Range(0, 16).Select(color => Word(address + color * 2)).ToArray();
            ushort[][] Rows(int address, int count) => Enumerable.Range(0, count).Select(frame => Read(address + frame * 32)).ToArray();
            ushort[][] level = Rows(0xa5e4f9, 7);
            if (editedFrame >= 0) level[editedFrame][editedColor] ^= 1;
            for (int frame = 0; frame < 7; frame++)
                for (int color = 0; color < 16; color++)
                {
                    AssertEqual(level[frame][color], catalog.ResolveDeathLevel(frame, color), "Calculated and unresolved level edits retain every other selected word");
                    AssertEqual(Word(0xa5e5d9 + frame * 32 + color * 2), catalog.ResolveDeathBackground(frame, color), "Level edit leaves background layer independent");
                }
            string expectedIdentity = SelectedPresentationHash.Create("SporeSpawnColorCatalog-v1", content =>
            {
                content.AppendWords("spores", Read(0xa5e359));
                content.AppendWordFrames("health", Rows(0xa5e379, 4));
                content.AppendWordFrames("deathSprite", Rows(0xa5e3f9, 8));
                content.AppendWordFrames("deathLevel", level);
                content.AppendWordFrames("deathBackground", Rows(0xa5e5d9, 7));
            });
            AssertEqual(expectedIdentity, catalog.ContentIdentity, "Level calculation preserves canonical selected content identity");
        }
    }
    private static void VerifyLookupStream4SporeBackgroundFade(ISnesAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        byte[] palette = SuperMetroid.Core.Rom.RomDataReader.Decompress(
            SuperMetroid.Core.Rom.CartridgeImportSource.Require(rom), SporeSpawnDeathColorDefinitions.OriginalRoomPaletteSource);
        var basis = (ushort[])typeof(SporeSpawnDeathColorDefinitions).GetField("InitialRoomBackgroundColors",
            BindingFlags.Static | BindingFlags.NonPublic)!.GetValue(null)!;
        AssertEqual(13, basis.Length, "Background fade retains exactly thirteen required original room-color sites");
        foreach (int color in Enumerable.Range(1, 11).Concat(new[] { 14, 15 }))
        {
            int offset = SporeSpawnDeathColorDefinitions.BackgroundPaletteByteOffset + color * 2;
            AssertEqual((ushort)(palette[offset] | palette[offset + 1] << 8),
                SporeSpawnDeathColorDefinitions.InitialBackgroundColor(color), "Every required initial color matches the original decoded room palette");
        }
        byte[] source = SporeSpawnColorExtractor.Extract(rom);
        var stock = SporeSpawnColorCatalog.Load(new MemoryStream(source));
        var values = (Dictionary<int, ushort>)typeof(SporeSpawnColorCatalog).GetField("deathBackground",
            BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(stock)!;
        AssertEqual(16, values.Count, "Stock background fade stores exactly sixteen required final words");
        for (int frame = 0; frame < 7; frame++)
            for (int color = 0; color < 16; color++)
            {
                AssertEqual(frame == 6, values.ContainsKey(frame * 16 + color), "Stock background has no interior fallback samples");
                AssertEqual(Word(0xa5e5d9 + frame * 32 + color * 2),
                    SporeSpawnDeathColorDefinitions.BackgroundColor(Word(0xa5e699 + color * 2), frame, color),
                    "Direct background calculation matches every original word independently of catalog loading");
            }
        Verify(stock, -1, -1);
        foreach (var selected in new[] { (Frame: 6, Color: 1), (Frame: 6, Color: 12), (Frame: 0, Color: 1), (Frame: 3, Color: 13) })
        {
            var document = JsonSerializer.Deserialize<SporeSpawnColorDocument>(source,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
            var old = document.DeathBackground[selected.Frame][selected.Color];
            document.DeathBackground[selected.Frame][selected.Color] = old with { Red = old.Red ^ 1 };
            var edited = SporeSpawnColorCatalog.Load(new MemoryStream(SporeSpawnColorCatalog.Write(document)));
            Verify(edited, selected.Frame, selected.Color);
            Verify(stock, -1, -1);
        }
        foreach (int invalid in new[] { int.MinValue, -1, 7, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveDeathBackground(invalid, 0), "Background frame bounds remain exact");
        foreach (int invalid in new[] { int.MinValue, -1, 16, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveDeathBackground(0, invalid), "Background color bounds remain exact");
        Console.WriteLine("Spore background fade: thirteen required room-color sites and sixteen final words; all112 native defaults, exact stored basis, independent edits and identities pass.");

        void Verify(SporeSpawnColorCatalog catalog, int editedFrame, int editedColor)
        {
            ushort[] Read(int address) => Enumerable.Range(0, 16).Select(color => Word(address + color * 2)).ToArray();
            ushort[][] Rows(int address, int count) => Enumerable.Range(0, count).Select(frame => Read(address + frame * 32)).ToArray();
            ushort[][] background = Rows(0xa5e5d9, 7);
            if (editedFrame >= 0) background[editedFrame][editedColor] ^= 1;
            for (int frame = 0; frame < 7; frame++)
                for (int color = 0; color < 16; color++)
                {
                    AssertEqual(background[frame][color], catalog.ResolveDeathBackground(frame, color), "Final and interior background edits preserve all other selected colors");
                    AssertEqual(Word(0xa5e4f9 + frame * 32 + color * 2), catalog.ResolveDeathLevel(frame, color), "Background edits leave level palette independent");
                }
            for (int frame = 0; frame < 8; frame++)
                for (int color = 0; color < 16; color++)
                    AssertEqual(Word(0xa5e3f9 + frame * 32 + color * 2), catalog.ResolveDeathSprite(frame, color), "Background edits leave sprite fade independent");
            string expectedIdentity = SelectedPresentationHash.Create("SporeSpawnColorCatalog-v1", content =>
            {
                content.AppendWords("spores", Read(0xa5e359));
                content.AppendWordFrames("health", Rows(0xa5e379, 4));
                content.AppendWordFrames("deathSprite", Rows(0xa5e3f9, 8));
                content.AppendWordFrames("deathLevel", Rows(0xa5e4f9, 7));
                content.AppendWordFrames("deathBackground", background);
            });
            AssertEqual(expectedIdentity, catalog.ContentIdentity, "Background fade preserves exact selected canonical identity");
        }
    }
    private static void VerifyLookupStream4SporeSpriteFade(ISnesAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        byte[] source = SporeSpawnColorExtractor.Extract(rom);
        var stock = SporeSpawnColorCatalog.Load(new MemoryStream(source));
        var values = (Dictionary<int, ushort>)typeof(SporeSpawnColorCatalog).GetField("deathSprite",
            BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(stock)!;
        AssertEqual(16, values.Count, "Stock sprite fade stores only sixteen final colors; initial colors share required critical-health row");
        for (int frame = 0; frame < 8; frame++)
            for (int color = 0; color < 16; color++)
            {
                AssertEqual(frame == 7, values.ContainsKey(frame * 16 + color), "Only final endpoint row remains stored in stock fade");
                AssertEqual(Word(0xa5e3f9 + frame * 32 + color * 2),
                    SporeSpawnColorCatalog.CalculateDeathSpriteColor(Word(0xa5e3f9 + color * 2), Word(0xa5e4d9 + color * 2), frame),
                    "Direct channel interpolation equals every original sprite death word without loading fallback samples");
            }
        for (int color = 0; color < 16; color++)
            AssertEqual(Word(0xa5e3d9 + color * 2), Word(0xa5e3f9 + color * 2), "Death fade starts at original critical-health coloring");
        var criticalDocument = JsonSerializer.Deserialize<SporeSpawnColorDocument>(source,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        criticalDocument.Health[3][1] = criticalDocument.Health[3][1] with { Red = criticalDocument.Health[3][1].Red ^ 1 };
        var criticalEdit = SporeSpawnColorCatalog.Load(new MemoryStream(SporeSpawnColorCatalog.Write(criticalDocument)));
        Verify(criticalEdit, -1, -1, criticalEdit: true);
        Verify(stock, -1, -1);
        foreach (var selected in new[] { (Frame: 0, Color: 1), (Frame: 7, Color: 15), (Frame: 3, Color: 6) })
        {
            var document = JsonSerializer.Deserialize<SporeSpawnColorDocument>(source,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
            var old = document.DeathSprite[selected.Frame][selected.Color];
            document.DeathSprite[selected.Frame][selected.Color] = old with { Red = old.Red ^ 1 };
            var edited = SporeSpawnColorCatalog.Load(new MemoryStream(SporeSpawnColorCatalog.Write(document)));
            Verify(edited, selected.Frame, selected.Color);
            Verify(stock, -1, -1);
        }
        foreach (int invalid in new[] { int.MinValue, -1, 8, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveDeathSprite(invalid, 0), "Sprite fade frame bounds remain exact");
        foreach (int invalid in new[] { int.MinValue, -1, 16, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveDeathSprite(0, invalid), "Sprite fade color bounds remain exact");
        Console.WriteLine("Spore sprite fade: sixteen required final words, shared critical-health endpoint and96 interpolated words; direct native defaults, sparse basis, independent edits and canonical identity pass.");

        void Verify(SporeSpawnColorCatalog catalog, int editedFrame, int editedColor, bool criticalEdit = false)
        {
            ushort[] Read(int address) => Enumerable.Range(0, 16).Select(color => Word(address + color * 2)).ToArray();
            ushort[][] Rows(int address, int count) => Enumerable.Range(0, count).Select(frame => Read(address + frame * 32)).ToArray();
            ushort[][] sprite = Rows(0xa5e3f9, 8);
            ushort[][] health = Rows(0xa5e379, 4);
            if (criticalEdit) health[3][1] ^= 1;
            if (editedFrame >= 0) sprite[editedFrame][editedColor] ^= 1;
            for (int frame = 0; frame < 8; frame++)
                for (int color = 0; color < 16; color++)
                    AssertEqual(sprite[frame][color], catalog.ResolveDeathSprite(frame, color), "Endpoint and intermediate edits preserve every other original selected word");
            for (int color = 0; color < 16; color++)
            {
                AssertEqual(Word(0xa5e359 + color * 2), catalog.ResolveSpore(color), "Sprite fade does not modify independent spore palette");
                for (int frame = 0; frame < 4; frame++)
                    AssertEqual(health[frame][color], catalog.ResolveHealth(frame, color), "Sprite fade and health edits remain independent");
                for (int frame = 0; frame < 7; frame++)
                {
                    AssertEqual(Word(0xa5e4f9 + frame * 32 + color * 2), catalog.ResolveDeathLevel(frame, color), "Sprite fade leaves level colors independent");
                    AssertEqual(Word(0xa5e5d9 + frame * 32 + color * 2), catalog.ResolveDeathBackground(frame, color), "Sprite fade leaves background colors independent");
                }
            }
            string expectedIdentity = SelectedPresentationHash.Create("SporeSpawnColorCatalog-v1", content =>
            {
                content.AppendWords("spores", Read(0xa5e359));
                content.AppendWordFrames("health", health);
                content.AppendWordFrames("deathSprite", sprite);
                content.AppendWordFrames("deathLevel", Rows(0xa5e4f9, 7));
                content.AppendWordFrames("deathBackground", Rows(0xa5e5d9, 7));
            });
            AssertEqual(expectedIdentity, catalog.ContentIdentity, "Calculated fade retains exact selected canonical identity framing");
        }
    }
    private static void VerifyLookupStream4EndingResultPanel(ISnesAddressSpace rom)
    {
        byte[] source = EndingTextExtractor.Extract(rom);
        ushort[] native = ReadEndingWords(rom, EndingTextDefinitions.Native.ResultPanel,
            EndingTextDefinitions.ResultPanelRows * EndingTextDefinitions.TilemapWidth);
        var stock = EndingTextPresentation.Load(new MemoryStream(source));
        AssertTrue(native.AsSpan().SequenceEqual(stock.BuildResultPanel()), "Calculated credit lines preserve all288 original producer-panel cells");
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var stockOverrides = (Dictionary<int, ushort>)typeof(EndingTextPresentation).GetField("resultOverrides", flags)!.GetValue(stock)!;
        AssertEqual(0, stockOverrides.Count, "Every stock producer-panel cell is calculated, without fallback samples");
        for (int cell = 0; cell < native.Length; cell++)
            AssertEqual(native[cell], EndingTextLayoutDefinitions.ResultWord(cell, "PRODUCED BY"), "Direct credit layout agrees with original cell independently of overrides");
        foreach (int editedCell in new[] { 0, 75, 107, 233 })
        {
            var document = System.Text.Json.Nodes.JsonNode.Parse(source)!;
            document["resultPanel"]!["template"]![editedCell]!["raw"] = native[editedCell] ^ 0x4000;
            byte[] selected = System.Text.Encoding.UTF8.GetBytes(document.ToJsonString());
            var edited = EndingTextPresentation.Load(new MemoryStream(selected));
            ushort[] expected = native.ToArray();
            expected[editedCell] ^= 0x4000;
            AssertTrue(expected.AsSpan().SequenceEqual(edited.BuildResultPanel()), "Blank, group top/bottom and team edits remain independent of calculated panel cells");
            AssertEqual(Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(selected)), edited.ContentIdentity, "Result layout preserves raw document identity");
            ushort[] mutableOutput = edited.BuildResultPanel();
            mutableOutput[editedCell] ^= 1;
            AssertTrue(expected.AsSpan().SequenceEqual(edited.BuildResultPanel()), "Result output snapshots remain independent of the catalog");
            AssertTrue(native.AsSpan().SequenceEqual(stock.BuildResultPanel()), "Independent result edits leave the original catalog immutable");
        }
        var changedHeading = System.Text.Json.Nodes.JsonNode.Parse(source)!;
        changedHeading["resultPanel"]!["text"] = "TEST";
        changedHeading["resultPanel"]!["template"]![10]!["raw"] = 0xffff;
        var heading = EndingTextPresentation.Load(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(changedHeading.ToJsonString())));
        ushort[] expectedHeading = native.ToArray();
        var region = EndingTextDefinitions.ResultProducedBy;
        Array.Fill(expectedHeading, EndingTextDefinitions.ResultBlankWord, region.Column, region.Width);
        for (int letter = 0; letter < 4; letter++)
            expectedHeading[region.Column + letter] = EndingTextDefinitions.CompileGlyph("TEST"[letter], EndingTextStyle.ResultSmall);
        AssertTrue(expectedHeading.AsSpan().SequenceEqual(heading.BuildResultPanel()), "Provided heading retains original fixed origin, padding and precedence over template words");
        foreach (int invalid in new[] { -1, native.Length })
            AssertThrows<IndexOutOfRangeException>(() => EndingTextLayoutDefinitions.ResultWord(invalid, "PRODUCED BY"), "Calculated result layout bounds");
        Console.WriteLine("Ending result panel:288 direct native cells, zero stock fallback samples, independent edits, heading precedence and immutable output snapshots pass; chosen wording has a narrow nonsense disposition; palette choices and font pixels remain required.");
    }

    private static void VerifyLookupStream4EndingSubtitle(ISnesAddressSpace rom)
    {
        byte[] source = EndingTextExtractor.Extract(rom);
        ushort[] native = ReadEndingWords(rom, EndingTextDefinitions.Native.JapaneseSubtitle,
            EndingTextDefinitions.JapaneseSubtitleRows * EndingTextDefinitions.TilemapWidth);
        var stock = EndingTextPresentation.Load(new MemoryStream(source));
        var stockOverrides = (Dictionary<int, ushort>)typeof(EndingTextPresentation).GetField("subtitleOverrides", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(stock)!;
        AssertEqual(0, stockOverrides.Count, "Every stock subtitle cell is calculated without fallback samples");
        for (int cell = 0; cell < native.Length; cell++)
            AssertEqual(native[cell], EndingTextLayoutDefinitions.SubtitleWord(cell), "Direct subtitle calculation agrees with native independently of overrides");
        Verify(stock, -1);
        AssertEqual(Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(source)), stock.ContentIdentity, "Subtitle conversion preserves complete document identity");
        foreach (int editedCell in new[] { 0, 9, 14, 41 })
        {
            var document = System.Text.Json.Nodes.JsonNode.Parse(source)!;
            document["japaneseSubtitle"]![editedCell]!["raw"] = native[editedCell] ^ 0x4000;
            byte[] editedSource = System.Text.Encoding.UTF8.GetBytes(document.ToJsonString());
            var edited = EndingTextPresentation.Load(new MemoryStream(editedSource));
            Verify(edited, editedCell);
            Verify(stock, -1);
        }
        foreach (int invalid in new[] { int.MinValue, -1, native.Length, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => _ = stock.JapaneseSubtitle[invalid], "Calculated subtitle preserves indexed bounds");
        ushort[] shortBuffer = new ushort[native.Length - 1];
        Array.Fill(shortBuffer, (ushort)0x1234);
        AssertThrows<ArgumentException>(() => stock.JapaneseSubtitle.CopyTo(shortBuffer), "Subtitle rejects short output before writing");
        AssertTrue(shortBuffer.All(value => value == 0x1234), "Failed subtitle copy is atomic");
        Console.WriteLine("Ending subtitle:64 native cells, atlas wrapping, centered placement, independent blank/top/wrap/bottom edits and actual draw/clear transfer pass.");

        void Verify(EndingTextPresentation presentation, int editedCell)
        {
            ushort[] expected = native.ToArray();
            if (editedCell >= 0) expected[editedCell] ^= 0x4000;
            AssertEqual(expected.Length, presentation.JapaneseSubtitle.Count, "Subtitle view keeps native dimensions");
            AssertTrue(expected.SequenceEqual(presentation.JapaneseSubtitle), "Calculated subtitle enumerates exact native or independently edited cells");
            ushort[] copied = Enumerable.Repeat((ushort)0x5678, expected.Length + 1).ToArray();
            presentation.JapaneseSubtitle.CopyTo(copied);
            AssertTrue(expected.AsSpan().SequenceEqual(copied.AsSpan(0, expected.Length)), "Subtitle direct copy preserves every cell");
            AssertEqual((ushort)0x5678, copied[^1], "Subtitle copy leaves the destination suffix untouched");
            ushort[] tilemap = Enumerable.Repeat((ushort)0x2222, EndingCreditsRomData.Rendering.TilemapWords).ToArray();
            var state = new EndingBackgroundTextState(new FrontendCartridgeReadGuard(rom), tilemap,
                EndingTextDefinitions.Native.ItemPercentage, default, japaneseText: true,
                presentation: presentation, installedSequence: EndingTextSequence.ItemPercentage);
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            typeof(EndingBackgroundTextState).GetField("installedInitialMarkerPending", flags)!.SetValue(state, false);
            typeof(EndingBackgroundTextState).GetField("installedCharacterIndex", flags)!.SetValue(state,
                presentation.Compile(EndingTextSequence.ItemPercentage).Length);
            var vram = new SnesVram();
            state.Step(vram);
            int destination = EndingCreditsRomData.Text.JapaneseSubtitleDestination;
            AssertTrue(expected.AsSpan().SequenceEqual(tilemap.AsSpan(destination, expected.Length)), "Actual ending state draws the calculated/edited subtitle");
            for (int cell = 0; cell < expected.Length; cell++)
            {
                int address = (EndingCreditsRomData.Rendering.PostCreditsTilemapWord + destination + cell) * 2;
                AssertEqual(expected[cell], (ushort)(vram.Bytes[address] | vram.Bytes[address + 1] << 8), "Actual subtitle reaches VRAM unchanged");
            }
            typeof(EndingBackgroundTextState).GetField("instructionTimer", flags)!.SetValue(state, (ushort)1);
            state.Step(vram);
            AssertTrue(tilemap.AsSpan(destination, expected.Length).ToArray().All(word => word == EndingCreditsRomData.Rendering.BlankTile), "Actual ending state clears the subtitle at its hold boundary");
            AssertTrue(state.Completed && state.RequestedItemPercentageScroll, "Subtitle clear preserves completion and scroll handoff");
        }
    }

    private static void VerifyLookupStream4TailRestGeometry(ISnesAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        VerifyLookupStream4TailAngles(rom);
        for (int link = 0; link < 7; link++)
            AssertEqual(Word(0xa6d37c + link * 2), RidleyTailDefinitions.RestDistance(link), "Shared tail rest geometry preserves the native initial separation");
        for (int link = 1; link < 7; link++)
            AssertEqual(Word(0xa6cf7f + (link - 1) * 0x36), RidleyTailDefinitions.RestDistance(link), "Native shrink threshold equals initialized rest geometry");
        foreach (int invalid in new[] { int.MinValue, -1, 7, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => RidleyTailDefinitions.RestDistance(invalid), "Shared rest geometry preserves bounded array selection");
        var update = typeof(RoomEnemySystem).GetMethod("UpdateRidleyTailDistances", BindingFlags.Static | BindingFlags.NonPublic)!
            .CreateDelegate<Action<RidleyEnemyState>>();
        foreach (int delta in new[] { -1, 0, 1 })
        {
            var state = new RidleyEnemyState
            {
                TailSegments = Enumerable.Range(0, 7).Select(link => new RidleyTailSegment
                {
                    Distance = (ushort)(Word(0xa6d37c + link * 2) + delta),
                }).ToArray(),
            };
            update(state);
            for (int link = 0; link < 7; link++)
            {
                int before = Word(0xa6d37c + link * 2) + delta;
                ushort expected = (ushort)(before - (link != 0 && delta > 0 ? Word(0xa6cf8b + (link - 1) * 0x36) : 0));
                AssertEqual(expected, state.TailSegments[link].Distance, "Actual tail shrinking preserves strict threshold, native decrement and excluded base");
            }
        }
        var extending = new RidleyEnemyState
        {
            TailExtensionSpeed = 0xf0,
            TailSegments = Enumerable.Range(0, 7).Select(link => new RidleyTailSegment
            {
                Distance = link == 0 ? (ushort)123 : (ushort)(Word(0xa6cf72 + (link - 1) * 0x36) - 1),
                TargetDistance = 1,
            }).ToArray(),
        };
        update(extending);
        for (int link = 1; link < 7; link++)
        {
            AssertEqual((ushort)0, extending.TailSegments[link].TargetDistance, "Actual tail extension clears the passed target first");
            AssertEqual(Word(0xa6cf72 + (link - 1) * 0x36), extending.TailSegments[link].Distance, "Actual tail still extends and clamps on the target-clearing frame");
        }
        AssertEqual((ushort)123, extending.TailSegments[0].Distance, "Tail base stays outside extension updates");
        Console.WriteLine("Tail rest geometry: seven initial distances, six original shrink thresholds and actual boundary/extension ordering pass; chosen lengths remain required.");
    }

    private static void VerifyLookupStream4BabyTransferPhase(ISnesAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        for (int phase = 0; phase < 4; phase++)
            AssertEqual(Word(0xa6acda + phase * 2), CeresMode7TransferDefinitions.BabyFrameForPhase(phase), "Baby reflected phase preserves native transfer identity");
        foreach (int invalid in new[] { int.MinValue, -1, 4, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => CeresMode7TransferDefinitions.BabyFrameForPhase(invalid), "Baby transfer phase preserves bounded selection");
        var colors = CeresRidleyMode7ColorCatalog.Load(new MemoryStream(CeresRidleyMode7ColorExtractor.Extract(rom)));
        var enemies = new RoomEnemySystem { CeresRidleyMode7Colors = colors };
        var vram = new SnesVram();
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        typeof(RoomEnemySystem).GetField("_vram", flags)!.SetValue(enemies, vram);
        typeof(RoomEnemySystem).GetField("_cgram", flags)!.SetValue(enemies, new SnesCgram());
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, new FrontendCartridgeReadGuard(rom));
        var tick = typeof(RoomEnemySystem).GetMethod("TickCeresRidleyMode7Getaway", flags)!
            .CreateDelegate<Action<RidleyEnemyState, SamusState?, ushort>>(enemies);
        var state = new RidleyEnemyState { Mode7TableByteIndex = 2 };
        byte[] untouched = new byte[SnesVram.ByteCount];
        Array.Fill(untouched, (byte)0x5a);
        int expectedPhase = 0;
        for (ushort nmi = 0; nmi < 16; nmi++)
        {
            vram.LoadBytes(0, untouched);
            bool advances = (nmi & 3) == 0;
            if (advances) expectedPhase = (expectedPhase + 1) & 3;
            tick(state, null, nmi);
            AssertEqual((ushort)expectedPhase, state.Mode7BabyFrame, "Actual getaway advances Baby before selection only on every fourth NMI");
            int pointer = 0xa60000 | Word(0xa6acda + expectedPhase * 2);
            for (int row = 0; row < 2; row++, pointer += 9)
            {
                int source = rom.ReadByte(pointer + 1) | rom.ReadByte(pointer + 2) << 8 | rom.ReadByte(pointer + 3) << 16;
                int destination = Word(pointer + 6);
                for (int column = 0; column < 2; column++)
                {
                    AssertEqual(advances ? rom.ReadByte(source + column) : (byte)0x5a,
                        vram.Bytes[(destination + column) * 2], "Actual getaway selects native capsule rows and preserves non-update NMIs");
                    AssertEqual((byte)0x5a, vram.Bytes[(destination + column) * 2 + 1], "Baby transfer preserves Mode7 character high bytes");
                }
            }
        }
        Console.WriteLine("Baby reflected phase: four original pointers and one actual16-NMI cycle preserve advance-before-select, transfer bytes and high-byte isolation.");
    }

    private static void VerifyFlyFrameIdentities(SuperMetroidAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        EnemySpritemapDefinition[] flyFrames = FlyVisualDefinitions.Frames();
        AssertEqual(4, flyFrames.Length, "fly calculated frame catalog count");
        for (int frame = 0; frame < flyFrames.Length; frame++)
        {
            AssertEqual((byte)0xa2, flyFrames[frame].Bank, "fly native frame bank");
            AssertEqual(Word(0xa2b015 + frame * 4), flyFrames[frame].Pointer, "fly native instruction frame identity");
            AssertEqual($"fly_shared_{frame}", flyFrames[frame].Name, "fly editable frame name");
            AssertEqual((ushort)1, Word(0xa20000 | flyFrames[frame].Pointer), "fly native single OAM object");
        }
    }
    private static void VerifyLookupStream4HudAutoCells(ISnesAddressSpace rom)
    {
        byte[] json = GameplayHudPresentationExtractor.Extract(rom);
        var document = JsonSerializer.Deserialize<GameplayHudPresentationDocument>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        GameplayHudPresentation stock = GameplayHudPresentation.Load(new MemoryStream(json));
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        AssertEqual(4, ((ushort[])typeof(GameplayHudPresentation).GetField("autoReserveBasis", flags)!.GetValue(stock)!).Length,
            "AUTO retains only four independent input cells");
        AssertEqual(0, ((Dictionary<int, ushort>)typeof(GameplayHudPresentation).GetField("autoReserveOverrides", flags)!.GetValue(stock)!).Count,
            "AUTO eight repeated stock words calculate without stored overrides");
        ushort[] tiles = new ushort[96];
        for (int state = 0; state < 2; state++)
        {
            stock.ApplyAutoReserve(tiles, state == 0);
            for (int cell = 0; cell < 6; cell++)
            {
                int address = 0x80998b + state * 12 + cell * 2;
                ushort native = (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
                AssertEqual(native, tiles[GameplayHudDefinitions.AutoReserveCellIndex(cell)], "AUTO actual native cell draw");
            }
        }
        document.AutoReserve.ContainsEnergy[0] = document.AutoReserve.ContainsEnergy[0] with { Palette = 2 };
        document.AutoReserve.ContainsEnergy[5] = document.AutoReserve.ContainsEnergy[5] with { TileColumn = 3 };
        document.AutoReserve.Empty[1] = document.AutoReserve.Empty[1] with { FlipX = true };
        document.AutoReserve.Empty[4] = document.AutoReserve.Empty[4] with { Priority = false };
        using var editedJson = new MemoryStream();
        GameplayHudPresentation.Write(editedJson, document);
        editedJson.Position = 0;
        GameplayHudPresentation edited = GameplayHudPresentation.Load(editedJson);
        for (int state = 0; state < 2; state++)
        {
            edited.ApplyAutoReserve(tiles, state == 0);
            for (int cell = 0; cell < 6; cell++)
            {
                GameplayHudCell supplied = (state == 0 ? document.AutoReserve.ContainsEnergy : document.AutoReserve.Empty)[cell];
                ushort expected = SnesBgTilemapWord.Create(supplied.TileRow * 32 + supplied.TileColumn, supplied.Palette,
                    supplied.Priority, (supplied.FlipX ? SnesTileFlipFlags.Horizontal : 0) |
                    (supplied.FlipY ? SnesTileFlipFlags.Vertical : 0)).Raw;
                AssertEqual(expected, tiles[GameplayHudDefinitions.AutoReserveCellIndex(cell)],
                    "AUTO preserves each independent edit and every unedited neighbor");
            }
        }
        foreach (int invalid in new[] { -1, 6, int.MinValue, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => GameplayHudDefinitions.AutoReserveWord(new ushort[4], invalid, true),
                "AUTO calculated cell exact domain");
    }
    private static void VerifyLookupStream4HudAnchors(ISnesAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        byte[] json = GameplayHudPresentationExtractor.Extract(rom);
        var document = JsonSerializer.Deserialize<GameplayHudPresentationDocument>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        GameplayHudPresentation stock = GameplayHudPresentation.Load(new MemoryStream(json));
        int Stored(GameplayHudPresentation value, string name) =>
            ((Dictionary<int, int>)typeof(GameplayHudPresentation).GetField(name,
                BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(value)!).Count;
        AssertEqual(0, Stored(stock, "energyTankAnchors"), "HUD stock tank positions are calculated");
        AssertEqual(0, Stored(stock, "autoAnchors"), "HUD stock AUTO positions are calculated");
        for (int tank = 0; tank < 14; tank++)
        {
            int native = Word(0x809cce + tank * 2);
            AssertEqual((ushort)native, GameplayHudDefinitions.EnergyTankByteOffset(tank), "native HUD tank offset");
            AssertEqual(native / 2, document.EnergyTanks.Anchors[tank].Y * 32 + document.EnergyTanks.Anchors[tank].X,
                "extracted native HUD tank anchor");
        }
        for (int item = 0; item < 5; item++)
            AssertEqual(Word(0x809d6e + item * 2), GameplayHudDefinitions.ItemByteOffset(item), "native HUD item offset");
        for (int cell = 0; cell < 6; cell++)
        {
            int native = (Word(0x809b65 + cell * 7) - 0xc608) / 2;
            AssertEqual(native, GameplayHudDefinitions.AutoReserveCellIndex(cell), "native AUTO store destination");
        }
        ushort[] tiles = new ushort[96];
        stock.ApplyEnergy(tiles, 100, 1400);
        for (int tank = 0; tank < 14; tank++)
            AssertEqual(tank == 0 ? stock.FilledEnergyTank : stock.EmptyEnergyTank,
                tiles[Word(0x809cce + tank * 2) / 2], "native tank draw destination");
        stock.ApplyAutoReserve(tiles, containsEnergy: true);
        for (int cell = 0; cell < 6; cell++)
            AssertEqual(Word(0x80998b + cell * 2), tiles[GameplayHudDefinitions.AutoReserveCellIndex(cell)], "native AUTO draw destination");
        stock.ClearAutoReserve(tiles);
        for (int cell = 0; cell < 6; cell++)
            AssertEqual(stock.Blank, tiles[GameplayHudDefinitions.AutoReserveCellIndex(cell)], "native AUTO clear destination");

        (document.EnergyTanks.Anchors[0], document.EnergyTanks.Anchors[1]) =
            (document.EnergyTanks.Anchors[1], document.EnergyTanks.Anchors[0]);
        (document.AutoReserve.Anchors[0], document.AutoReserve.Anchors[1]) =
            (document.AutoReserve.Anchors[1], document.AutoReserve.Anchors[0]);
        using var editedJson = new MemoryStream();
        GameplayHudPresentation.Write(editedJson, document);
        editedJson.Position = 0;
        GameplayHudPresentation edited = GameplayHudPresentation.Load(editedJson);
        AssertEqual(2, Stored(edited, "energyTankAnchors"), "only edited tank positions are stored");
        AssertEqual(2, Stored(edited, "autoAnchors"), "only edited AUTO positions are stored");
        edited.ApplyEnergy(tiles, 100, 1400);
        AssertEqual(edited.FilledEnergyTank, tiles[0x44 / 2], "edited first tank moves to second native anchor");
        AssertEqual(edited.EmptyEnergyTank, tiles[0x42 / 2], "edited second tank moves to first native anchor");
        edited.ApplyAutoReserve(tiles, containsEnergy: false);
        AssertEqual(Word(0x809997), tiles[9], "edited AUTO first cell follows override");
        AssertEqual(Word(0x809999), tiles[8], "edited AUTO second cell follows override");
        for (int cell = 2; cell < 6; cell++)
            AssertEqual(Word(0x809997 + cell * 2), tiles[GameplayHudDefinitions.AutoReserveCellIndex(cell)], "unedited AUTO anchors unchanged");
        document.AutoReserve.Anchors[0] = document.AutoReserve.Anchors[1];
        AssertThrows<InvalidDataException>(() => GameplayHudPresentation.Write(new MemoryStream(), document),
            "sparse HUD anchor compilation preserves overlap rejection");
        foreach (int invalid in new[] { -1, 14, int.MinValue, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => GameplayHudDefinitions.EnergyTankByteOffset(invalid), "tank exact domain");
        foreach (int invalid in new[] { -1, 5, int.MinValue, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => GameplayHudDefinitions.ItemByteOffset(invalid), "item exact domain");
        foreach (int invalid in new[] { -1, 6, int.MinValue, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => GameplayHudDefinitions.AutoReserveCellIndex(invalid), "AUTO exact domain");
    }
    private static void VerifyLookupStream4HudDigits(ISnesAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        byte[] json = GameplayHudPresentationExtractor.Extract(rom);
        var document = JsonSerializer.Deserialize<GameplayHudPresentationDocument>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        GameplayHudPresentation stock = GameplayHudPresentation.Load(new MemoryStream(json));
        int Stored(GameplayHudPresentation value, string name) =>
            ((Dictionary<int, ushort>)typeof(GameplayHudPresentation).GetField(name,
                BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(value)!).Count;
        AssertEqual(0, Stored(stock, "healthDigits"), "stock health digits are calculated");
        AssertEqual(0, Stored(stock, "ammoDigits"), "stock ammo digits are calculated");
        ushort[] tiles = new ushort[96];
        for (int digit = 0; digit < 10; digit++)
        {
            AssertEqual(Word(0x809dbf + digit * 2), GameplayHudDefinitions.DigitWord(digit), "native health digit");
            AssertEqual(Word(0x809dd3 + digit * 2), GameplayHudDefinitions.DigitWord(digit), "native ammo digit");
            stock.ApplyEnergy(tiles, (ushort)(digit * 11), 99);
            for (int place = 0; place < 2; place++)
                AssertEqual(Word(0x809dbf + digit * 2), tiles[70 + place], "actual stock health digit draw");
            stock.ApplyAmmo(tiles, 0, (ushort)(digit * 111));
            for (int place = 0; place < 3; place++)
                AssertEqual(Word(0x809dd3 + digit * 2), tiles[74 + place], "actual stock ammo digit draw");
        }
        document.Digits.Health[3] = document.Digits.Health[3] with { Palette = 2 };
        document.Digits.Ammo[3] = document.Digits.Ammo[3] with { Palette = 4 };
        using var changedJson = new MemoryStream();
        GameplayHudPresentation.Write(changedJson, document);
        changedJson.Position = 0;
        GameplayHudPresentation changed = GameplayHudPresentation.Load(changedJson);
        AssertEqual(1, Stored(changed, "healthDigits"), "only edited health digit stored");
        AssertEqual(1, Stored(changed, "ammoDigits"), "only edited ammo digit stored");
        changed.ApplyEnergy(tiles, 23, 99);
        changed.ApplyAmmo(tiles, 0, 234);
        AssertEqual(Word(0x809dbf + 4), tiles[70], "unedited health digit unchanged");
        AssertEqual((ushort)((Word(0x809dbf + 6) & ~0x1c00) | 0x0800), tiles[71], "edited health digit palette applied");
        AssertEqual(Word(0x809dd3 + 4), tiles[74], "unedited leading ammo digit unchanged");
        AssertEqual((ushort)((Word(0x809dd3 + 6) & ~0x1c00) | 0x1000), tiles[75], "independent ammo digit palette applied");
        AssertEqual(Word(0x809dd3 + 8), tiles[76], "unedited trailing ammo digit unchanged");
        foreach (int invalid in new[] { int.MinValue, -1, 10, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => GameplayHudDefinitions.DigitWord(invalid), "HUD digit exact domain");
    }
    private static void VerifyTitleSpriteIdentities(SuperMetroidAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        var nativeTitlePointers = new HashSet<ushort> { 0x8103, Word(0x8ba0c7) };
        foreach (int start in new[] { 0x8ba03d, 0x8ba055, 0x8ba079, 0x8ba09d })
            for (int entry = start; Word(entry) < 0x8000; entry += 4)
                if (Word(entry + 2) != 0) nativeTitlePointers.Add(Word(entry + 2));
        AssertEqual(31, nativeTitlePointers.Count, "native title sprite catalog size");
        AssertTrue(nativeTitlePointers.Order().SequenceEqual(TitleSpriteDefinitions.NativePointers),
            "calculated title sprite sequence preserves exact distinct native order");
        AssertEqual(4, TitleSequenceRomData.Vram.BabyAnimationSourcePages.Length, "title baby animation phase count");
        for (int frame = 0; frame < 4; frame++)
        {
            ushort transfer = Word(0x8ba133 + frame * 4);
            int nativeSource = Word(0x8b0001 + transfer) | rom.ReadByte(0x8b0003 + transfer) << 16;
            int firstSource = Word(0x8ba338) | rom.ReadByte(0x8ba33a) << 16;
            AssertEqual((nativeSource - firstSource) / 256,
                (int)TitleSequenceRomData.Vram.BabyAnimationSourcePages[frame], "native title baby source page");
            AssertEqual((ushort)256, Word(0x8b0004 + transfer), "native baby transfer size");
            AssertEqual((ushort)0x3800, Word(0x8b0006 + transfer), "native baby transfer destination");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 4, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => _ = TitleSequenceRomData.Vram.BabyAnimationSourcePages[invalid], "title baby exact phase domain");
        Console.WriteLine("Title identities: 31 sorted native selectors, four Baby source pages, DMA sizes/destinations and invalid phases pass.");
    }
    private static void VerifyTitleCardLayout(SuperMetroidAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        for (int address = TitleSequenceInstructionDefinitions.StartAddress;
             address < TitleSequenceInstructionDefinitions.EndAddress; address++)
        {
            AssertEqual(rom.ReadByte(address), TitleSequenceInstructionDefinitions.ReadByte(address), "native title program byte");
            if (address + 1 < TitleSequenceInstructionDefinitions.EndAddress)
                AssertEqual(Word(address), TitleSequenceInstructionDefinitions.ReadWord(address), "native title aligned/unaligned word");
        }
        foreach (int invalid in new[] { int.MinValue, TitleSequenceInstructionDefinitions.StartAddress - 1,
            TitleSequenceInstructionDefinitions.EndAddress, int.MaxValue })
        {
            AssertThrows<InvalidDataException>(() => TitleSequenceInstructionDefinitions.ReadByte(invalid), "title byte domain");
            AssertThrows<InvalidDataException>(() => TitleSequenceInstructionDefinitions.ReadWord(invalid), "title word domain");
        }
        AssertThrows<InvalidDataException>(() => TitleSequenceInstructionDefinitions.ReadWord(
            TitleSequenceInstructionDefinitions.EndAddress - 1), "title trailing partial word rejected");
        Console.WriteLine("Title card layout: 140 native bytes, 139 aligned/unaligned word windows and bounds pass.");
    }
    private static void VerifyRoomSpriteDispatch(SuperMetroidAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        var spriteSpawn = typeof(RoomEnemySystem).GetMethod("SpawnRoomSpriteObject", BindingFlags.Instance | BindingFlags.NonPublic)!
            .CreateDelegate<Func<RoomEnemySystem, ushort, ushort, RoomSpriteObjectKind, ushort, RoomSpriteObjectSlot?>>();
        FieldInfo spriteBus = typeof(RoomEnemySystem).GetField("_bus", BindingFlags.Instance | BindingFlags.NonPublic)!;
        for (ushort objectNumber = 0; objectNumber < 62; objectNumber++)
        {
            var kind = (RoomSpriteObjectKind)objectNumber;
            ushort entry = Word(0xb4bda8 + 2 * objectNumber);
            AssertEqual(entry, RoomSpriteObjectDefinitions.InstructionPointer(kind), "room sprite native program case");
            var enemies = new RoomEnemySystem();
            spriteBus.SetValue(enemies, new FrontendCartridgeReadGuard(rom));
            RoomSpriteObjectSlot? spawned = spriteSpawn(enemies, 0x1234, 0x5678, kind, 0x0600);
            AssertTrue(spawned is not null, "room sprite native case allocates real slot");
            AssertEqual(entry, spawned!.InstructionPointer, "room sprite actual initial program");
            AssertEqual(Word(0xb40000 | entry), spawned.InstructionTimer, "room sprite actual initial native duration");
            AssertEqual(Word(0xb40000 | (entry + 2)), spawned.SpritemapPointer, "room sprite actual initial native visual");
            AssertEqual(kind, spawned.Kind, "room sprite selected kind preserved");
            AssertEqual((ushort)0x1234, spawned.XPosition, "room sprite spawn X preserved");
            AssertEqual((ushort)0x5678, spawned.YPosition, "room sprite spawn Y preserved");
            AssertEqual((ushort)0x0600, spawned.GraphicsIndex, "room sprite graphics argument preserved");
        }
        foreach (ushort invalid in new ushort[] { 62, 0x8000, 0xffff })
            AssertThrows<ArgumentOutOfRangeException>(
                () => RoomSpriteObjectDefinitions.InstructionPointer((RoomSpriteObjectKind)invalid),
                "room sprite native kind domain rejects invalid selector");
        Console.WriteLine("Room sprite dispatch: 62 native entries and actual spawns, initial timing/visuals, source-read guards and invalid selectors pass.");
    }
    private static void VerifyLookupStream4EnemyNameRecords(ISnesAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        ushort[] nativeNamePointers = RoomEnemyDefinitionCatalog.Pointers
            .Select(pointer => ReadNativeEnemyDefinition(rom, pointer).NamePointer)
            .Where(pointer => pointer != 0).Distinct().Order().ToArray();
        AssertTrue(nativeNamePointers.SequenceEqual(RoomEnemySpawnNameDefinitions.Pointers),
            "spawn-name exact native referenced identity order");
        foreach (ushort pointer in nativeNamePointers)
        {
            int address = 0xb40000 | pointer;
            var native = new RoomEnemySpawnNameWords(Word(address), Word(address + 2), Word(address + 4),
                Word(address + 6), Word(address + 8), Word(address + 12));
            AssertEqual(native, RoomEnemySpawnNameDefinitions.Get(pointer), "spawn-name native five text words and debug ordinal");
        }
        foreach (ushort invalid in new ushort[] { 0, 0xdd89, 0xdd98, 0xdda5, 0xffff })
            AssertThrows<InvalidDataException>(() => RoomEnemySpawnNameDefinitions.Get(invalid),
                "spawn-name unreferenced or unaligned identity rejected");
    }
    private static void VerifyLookupStream4RidleyFrameDomain(ISnesAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        var nativeFrames = new List<ushort>();
        var nativeLists = new SortedSet<ushort>();
        for (int frame = 0xe983; frame <= 0xead7;)
        {
            nativeFrames.Add((ushort)frame);
            int components = Word(0xa60000 | frame);
            for (int component = 0; component < components; component++)
                nativeLists.Add(Word(0xa60000 | (frame + 8 + component * 8)));
            frame += 2 + components * 8;
        }
        AssertEqual(nativeFrames.Count, RidleyCollisionDefinitions.FramePointers.Length, "Frame identity count follows native record extents");
        int ordinal = 0;
        foreach (ushort frame in RidleyCollisionDefinitions.FramePointers)
        {
            AssertEqual(nativeFrames[ordinal], frame, "Frame identity enumeration preserves native record order");
            AssertEqual(frame, RidleyCollisionDefinitions.FramePointers[ordinal++], "Frame identity indexing preserves order");
            var components = RidleyCollisionDefinitions.ComponentsAt(frame);
            foreach (int invalid in new[] { -1, components.Length })
                AssertThrows<IndexOutOfRangeException>(() => _ = components[invalid], "Calculated component view rejects out-of-range indices");
        }
        for (int pointer = 0; pointer <= ushort.MaxValue; pointer++)
            AssertEqual(nativeFrames.Contains((ushort)pointer), RidleyCollisionDefinitions.HasFrame((ushort)pointer), "Calculated frame membership preserves complete native identity domain");
        AssertTrue(nativeLists.SequenceEqual(RidleyCollisionDefinitions.HitboxPointers), "List identity enumeration derives the exact sorted native operand domain");
        foreach (int invalid in new[] { -1, nativeFrames.Count })
            AssertThrows<IndexOutOfRangeException>(() => _ = RidleyCollisionDefinitions.FramePointers[invalid], "Calculated frame view rejects out-of-range indices");
    }
    private static void VerifyLookupStream4BeamColorRelations(ISnesAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        byte[] source = BeamPaletteExtractor.Extract(rom);
        var stock = BeamPaletteCatalog.Load(new MemoryStream(source));
        var stored = (Dictionary<int, ushort>)typeof(BeamPaletteCatalog).GetField("palettes", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(stock)!;
        AssertEqual(43, stored.Count, "Beam stock stores exactly the43 independently required colors, with no derived fallback samples");
        for (int selection = 0; selection < 12; selection++)
        for (int color = 0; color < 16; color++)
        {
            bool basis = selection switch
            {
                0 => color <= 8 || color == 15,
                (int)SamusBeamFlags.Ice => color >= 2,
                (int)SamusBeamFlags.Wave or (int)SamusBeamFlags.Plasma => color is >= 2 and <= 5 or 7 or 8 or 15,
                (int)SamusBeamFlags.Spazer => color is >= 2 and <= 4 or 8 or 15,
                _ => false,
            };
            AssertEqual(basis, stored.ContainsKey(selection * 16 + color), "Beam stored membership is exactly the documented Power/Ice/Wave/Plasma/Spazer basis");
        }
        Verify(stock, -1, -1);
        foreach ((int selection, int color) in new[] { (0, 0), (2, 1), (3, 3), (1, 9), (8, 2), (4, 15), (8, 5), (8, 6), (4, 6) })
        {
            var document = System.Text.Json.Nodes.JsonNode.Parse(source)!;
            var selected = document["palettes"]![BeamPaletteDefinitions.Key(selection)]![color]!;
            selected["red"] = selected["red"]!.GetValue<int>() ^ 1;
            var edited = BeamPaletteCatalog.Load(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(document.ToJsonString())));
            Verify(edited, selection, color);
            Verify(stock, -1, -1);
        }
        foreach (int invalid in new[] { int.MinValue, -1, 12, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.LoadTo(new SnesCgram(), invalid), "Calculated beam colors preserve selection bounds");

        void Verify(BeamPaletteCatalog catalog, int editedSelection, int editedColor)
        {
            for (int selection = 0; selection < 12; selection++)
            {
                var actual = new SnesCgram();
                for (int index = 0; index < SnesCgram.ColorCount; index++) actual.SetColor(index, (ushort)(index * 31));
                catalog.LoadTo(actual, selection);
                int pointer = 0x900000 | Word(0x90c3c9 + selection * 2);
                for (int index = 0; index < SnesCgram.ColorCount; index++)
                {
                    int color = index - SamusProjectileRomData.Palettes.BeamDestinationIndex;
                    ushort expected = color is >= 0 and < 16 ? Word(pointer + color * 2) : (ushort)(index * 31);
                    if (selection == editedSelection && color == editedColor) expected ^= 1;
                    AssertEqual(expected, actual.Colors[index], "Derived beam color relationships preserve native rows, isolated edits and neighboring CGRAM");
                }
            }
        }
    }
    private static void VerifyLookupStream4RidleyMovementPolicy(ISnesAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        ushort morphedFrames = Word(0xa6bc99), ordinaryFrames = Word(0xa6bc9e);
        for (int raw = 0; raw <= byte.MaxValue; raw++)
        {
            byte flags = raw < 28 ? rom.ReadByte(0xa6bd04 + raw) : (byte)0;
            var movement = (SamusMovementType)raw;
            AssertEqual((flags & 0x80) != 0, RidleySamusInteractionDefinitions.CanGrab(movement), "Named movement policy preserves complete byte-domain grabbability");
            AssertEqual((flags & 0x40) != 0 ? morphedFrames : ordinaryFrames,
                RidleySamusInteractionDefinitions.ReleaseIntangibilityFrames(movement), "Named movement policy preserves complete byte-domain release duration");
        }
        const BindingFlags instance = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", instance)!.SetValue(enemies, new FrontendCartridgeReadGuard(rom));
        var canGrab = typeof(RoomEnemySystem).GetMethod("SamusMovementUsesRidleyGrab", instance)!
            .CreateDelegate<Func<SamusState?, bool>>(enemies);
        var release = typeof(RoomEnemySystem).GetMethod("ReleaseNorfairRidleyGrab", instance)!
            .CreateDelegate<Action<RidleyEnemyState, SamusState?>>(enemies);
        for (int pose = 0; pose <= 0xfc; pose++)
        {
            var samus = new SamusState { Pose = (byte)pose };
            byte flags = rom.ReadByte(0xa6bd04 + rom.ReadByte(0x91b62a + pose * 8));
            AssertEqual((flags & 0x80) != 0, canGrab(samus), "Actual grab policy preserves each native real-pose classification");
            var state = new RidleyEnemyState { GrabState = 1, TailWhipRequest = 0, TailFunctionIndex = 4, IntangibilityTimer = 99 };
            release(state, samus);
            AssertEqual((flags & 0x40) != 0 ? morphedFrames : ordinaryFrames, state.IntangibilityTimer, "Actual release preserves native movement-dependent intangibility");
            AssertEqual((ushort)0, state.GrabState, "Actual release clears grab");
            AssertEqual((ushort)1, state.TailWhipRequest, "Actual release requests tail whip");
            AssertEqual((ushort)1, state.TailFunctionIndex, "Actual release resets tail controller");
        }
        AssertTrue(!canGrab(null), "Missing Samus remains ungrabbable");
        var absentState = new RidleyEnemyState();
        release(absentState, null);
        AssertEqual(ordinaryFrames, absentState.IntangibilityTimer, "Missing Samus retains standing release fallback");
    }
    private static void VerifyLookupStream4TailAngles(ISnesAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        var create = typeof(RoomEnemySystem).GetMethod("CreateInitialRidleyTailSegments", BindingFlags.NonPublic | BindingFlags.Static)!
            .CreateDelegate<Func<RidleyTailSegment[]>>();
        var segments = create();
        AssertEqual(7, segments.Length, "Tail initialization retains seven links");
        AssertEqual(Word(0xa6d2fe), RidleyTailDefinitions.IdealInterSegmentAngle, "Tail separation matches the native initializer immediate");
        for (int index = 0; index < segments.Length; index++)
        {
            AssertEqual(Word(0xa6d38a + 2 * index), segments[index].Angle, "Actual initialized link angle matches native progression");
            AssertEqual(Word(0xa6d37c + 2 * index), segments[index].Distance, "Unresolved independent tail distance remains unchanged");
            AssertEqual(Word(0xa6d36e + 2 * index), segments[index].MovementDirection, "Initial rotation direction remains unchanged");
            AssertEqual((ushort)(Word(0xa6d2fe) + 1), segments[index].StaggerAngle, "Initial stagger remains ideal separation plus one");
        }
        var expectedFrames = new SortedSet<ushort> { 0xdc90, 0xdc97, 0xdc9e };
        for (int direction = 0; direction < 16; direction++)
        {
            ushort expected = Word(0xa6dcba + 2 * direction);
            AssertEqual(expected, RidleySupplementalVisualDefinitions.TailTipFrameAt(direction), "Calculated reversed tail-tip sector preserves native identity");
            AssertEqual((ushort)1, Word(0xa60000 | expected), "Each native tail-tip record contains exactly one OAM object");
            expectedFrames.Add(expected);
        }
        for (int frame = 0; frame < 20; frame++)
        {
            ushort expected = Word(0xa6db02 + frame * 2);
            AssertEqual(expected, RidleySupplementalVisualDefinitions.WingFrameAt(frame), "Calculated downstroke/upstroke and facing preserve each native wing identity");
            expectedFrames.Add(expected);
        }
        foreach (int invalid in new[] { int.MinValue, -1, 20, int.MaxValue })
            AssertThrows<InvalidDataException>(() => RidleySupplementalVisualDefinitions.WingFrameAt(invalid), "Wing phase/facing domain remains exact");
        AssertTrue(expectedFrames.SequenceEqual(RidleySupplementalVisualDefinitions.Frames().Select(frame => frame.Pointer)),
            "Calculated tail-tip source enumeration retains the complete ordered supplemental frame domain");
        foreach (int invalid in new[] { int.MinValue, -1, 16, int.MaxValue })
            AssertThrows<InvalidDataException>(() => RidleySupplementalVisualDefinitions.TailTipFrameAt(invalid), "Tail-tip selector domain remains exact");
        foreach (int invalid in new[] { -1, 7 })
            AssertThrows<ArgumentOutOfRangeException>(() => RidleyTailDefinitions.InitialAngle(invalid), "Initial angular domain contains exactly seven links");
    }
    private static void VerifyLookupStream4TailTerrain(ISnesAddressSpace rom)
    {
        var touches = typeof(RoomEnemySystem).GetMethod("RidleyTailTouchesTerrain", BindingFlags.NonPublic | BindingFlags.Static)!
            .CreateDelegate<Func<RidleyEnemyState, RoomLevelData?, bool>>();
        for (int solidRow = 0; solidRow < 2; solidRow++)
        {
            ushort[] blocks = new ushort[2];
            blocks[solidRow] = 0x8000;
            var level = new RoomLevelData(1, 2, blocks, new byte[2], new ushort[2], []);
            for (int segmentIndex = 0; segmentIndex < 7; segmentIndex++)
            foreach (ushort y in new ushort[] { 13, 14, 15, 0xfff0, 0xfff1 })
            {
                var state = new RidleyEnemyState
                {
                    TailSegments = Enumerable.Range(0, 7).Select(_ => new RidleyTailSegment { XPosition = 256 }).ToArray(),
                };
                state.TailSegments[segmentIndex].XPosition = 0;
                state.TailSegments[segmentIndex].YPosition = y;
                bool expected = false;
                if (segmentIndex >= 2)
                {
                    int operand = 0xa6b7f2 + (6 - segmentIndex) * 0x14;
                    int nativeOffset = rom.ReadByte(operand) | rom.ReadByte(operand + 1) << 8;
                    expected = (unchecked((ushort)(y + nativeOffset)) >> 4) == solidRow;
                }
                AssertEqual(expected, touches(state, level), "Actual tail terrain probes retain each native joint, ADC offset and word wrap");
            }
            AssertTrue(!touches(new RidleyEnemyState(), level), "Incomplete tail keeps terrain gate");
        }
        AssertTrue(!touches(new RidleyEnemyState(), null), "Missing terrain keeps tail gate");
    }
    private static void VerifyLookupStream4NoticeRegions(ISnesAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        byte[] bytes = GameplayMessageNoticeExtractor.Extract(rom);
        var document = JsonSerializer.Deserialize<GameplayMessageNoticeDocument>(bytes, MapPresentationFormat.JsonOptions)!;
        var stock = GameplayMessageNoticePresentation.Load(new MemoryStream(bytes));
        foreach (var id in GameplayMessageNoticeDefinitions.MessageIds)
        {
            var notice = document.Notices[id.ToString()];
            (int Row, int Column, int Width)[] expectedRegions = GameplayMessageNoticeDefinitions.IsSaveConfirmation(id)
                ? [(0, 8, 14), (1, 8, 8), (3, 10, 3), (3, 19, 2)]
                : [(0, 8, id == GameplayMessageId.MissileRechargeCompleted ? 14 : 15), (2, 10, 10)];
            AssertTrue(expectedRegions.SequenceEqual(notice.Text.Select(region => (region.Row, region.Column, region.Width))),
                "Calculated notice rectangles preserve every original row/column/width");
            AssertTrue(notice.Text.All(region => region.Alignment == GameplayMessageNoticeDefinitions.LeftAlignment),
                "Calculated notice regions retain left alignment");
            int source = 0x850000 | Word(0x85869f + ((byte)id - 1) * 6);
            var expected = new ushort[(notice.RowCount + 2) * 32];
            for (int column = 0; column < 32; column++)
                expected[column] = expected[expected.Length - 32 + column] = Word(0x858040 + column * 2);
            for (int cell = 0; cell < notice.RowCount * 32; cell++) expected[32 + cell] = Word(source + cell * 2);
            AssertTrue(expected.SequenceEqual(stock.Build(id)), "Derived notice export reproduces every native border/content word");
            var state = new GameplayMessageBoxState();
            state.BindPresentation(null, null, stock);
            state.Begin(new FrontendCartridgeReadGuard(rom), id);
            if (GameplayMessageNoticeDefinitions.IsSaveConfirmation(id))
                for (int column = 0; column < 32; column++) expected[128 + column] = Word(0x8595c1 + column * 2);
            AssertTrue(expected.AsSpan().SequenceEqual(state.Tilemap), "Actual notice initial tilemap preserves native YES row and all content");
            if (!GameplayMessageNoticeDefinitions.IsSaveConfirmation(id)) continue;
            for (int frame = 0; frame < 100 && state.Phase != GameplayMessageBoxPhase.AwaitingInput; frame++) state.Step(0);
            AssertEqual(GameplayMessageBoxPhase.AwaitingInput, state.Phase, "Save notice reaches existing input phase");
            state.Step((ushort)SuperMetroid.Core.Input.SnesButton.Right);
            for (int column = 0; column < 32; column++) expected[128 + column] = Word(0x859601 + column * 2);
            AssertTrue(expected.AsSpan().SequenceEqual(state.Tilemap), "Actual notice NO selection retains native row and neighboring content");
        }
        var editedJson = System.Text.Json.Nodes.JsonNode.Parse(bytes)!;
        editedJson["notices"]![GameplayMessageId.MapDataAccessCompleted.ToString()]!["text"]![0]!["text"] = "MAP TEST";
        var edited = GameplayMessageNoticePresentation.Load(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(editedJson.ToJsonString())));
        var active = new GameplayMessageBoxState();
        active.BindPresentation(null, null, stock);
        active.Begin(new FrontendCartridgeReadGuard(rom), GameplayMessageId.MapDataAccessCompleted);
        active.Step(0);
        var phase = active.Phase; int radius = active.RadiusPixels;
        ushort[] original = active.Tilemap.ToArray();
        active.BindPresentation(null, null, edited);
        AssertEqual(phase, active.Phase, "Derived notice region edit preserves live phase");
        AssertEqual(radius, active.RadiusPixels, "Derived notice region edit preserves window radius");
        for (int cell = 0; cell < original.Length; cell++)
        {
            ushort expected = original[cell];
            if (cell is >= 40 and < 55)
            {
                int textIndex = cell - 40;
                char character = textIndex < 8 ? "MAP TEST"[textIndex] : ' ';
                expected = (ushort)((original[40] & 0xfc00) | (character == ' ' ? 0x4e : 0xe0 + character - 'A'));
            }
            AssertEqual(expected, active.Tilemap[cell], "Independent notice text edit remains within its original calculated rectangle");
        }
        active.BindPresentation(null, null, stock);
        AssertTrue(original.AsSpan().SequenceEqual(active.Tilemap), "Restoring supplied stock notice restores every live word");
    }
    private static void VerifyLookupStream4MessageDispatch(ISnesAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        for (int id = 1; id <= 29; id++)
        {
            var definition = GameplayMessageDefinitions.AtNativeIndex(id);
            int address = 0x85869b + (id - 1) * 6;
            AssertEqual(Word(address), definition.ModifyFunction, "Native message setup callback case");
            AssertEqual(Word(address + 2), definition.DrawFunction, "Native message drawing callback case");
            AssertEqual(Word(address + 4), definition.ContentPointer, "Native message presentation identity case");
        }
        foreach (int invalid in new[] { int.MinValue, 0, 30, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => GameplayMessageDefinitions.AtNativeIndex(invalid), "Native message record domain");
        var resolve = typeof(GameplayMessageBoxState).GetMethod("ResolveButtonTilemapWord", BindingFlags.Static | BindingFlags.NonPublic)!
            .CreateDelegate<Func<ushort, ushort>>();
        for (int bits = 0; bits <= ushort.MaxValue; bits++)
        {
            int winner = 0;
            while (winner < 7 && (bits & Word(0x8583d5 + winner * 8)) == 0) winner++;
            ushort expected = Word(0x858426 + 2 * winner);
            AssertEqual(expected, GameplayMessageRomData.Buttons.ResolveGlyphWord((ushort)bits), "Native BIT order and all button glyph attributes");
            AssertEqual(expected, resolve((ushort)bits), "Actual message button resolver retains first-match precedence");
        }
        var patch = typeof(GameplayMessageBoxState).GetMethod("PatchConfiguredButton", BindingFlags.Instance | BindingFlags.NonPublic)!
            .CreateDelegate<Action<GameplayMessageBoxState, GameplayMessageId, ushort>>();
        var tilemapField = typeof(GameplayMessageBoxState).GetField("_tilemap", BindingFlags.Instance | BindingFlags.NonPublic)!;
        for (int raw = 0; raw <= byte.MaxValue; raw++)
        {
            var id = (GameplayMessageId)raw;
            if (raw is < 1 or > 27)
            {
                AssertThrows<IndexOutOfRangeException>(() => GameplayMessageRomData.Buttons.SpecialGlyphByteOffset(id), "Message button patch exact native domain");
                continue;
            }
            ushort expected = Word(0x858749 + (raw - 1) * 2);
            AssertEqual(expected, GameplayMessageRomData.Buttons.SpecialGlyphByteOffset(id), "Native message button placement case");
            var state = new GameplayMessageBoxState();
            var cells = Enumerable.Range(0, 192).Select(index => (ushort)(0x4000 + index)).ToArray();
            tilemapField.SetValue(state, cells);
            patch(state, id, ushort.MaxValue);
            for (int index = 0; index < cells.Length; index++)
                AssertEqual(index == expected / 2 ? Word(0x858426) : (ushort)(0x4000 + index), cells[index],
                    "Actual button patch preserves every independently supplied neighboring cell");
        }
    }
    private static void VerifyLookupStream4(ISnesAddressSpace rom)
    {
        VerifyLookupStream4EnemyNameRecords(rom);
        VerifyLookupStream4RidleyFrameDomain(rom);
        VerifyLookupStream4BeamColorRelations(rom);
        VerifyLookupStream4RidleyMovementPolicy(rom);
        VerifyLookupStream4TailAngles(rom);
        VerifyLookupStream4TailTerrain(rom);
        VerifyLookupStream4MessageDispatch(rom);
        VerifyLookupStream4NoticeRegions(rom);
        for (ushort offset = 0; offset <= 24; offset += 8)
        {
            var hole = BotwoonNavigationDefinitions.HoleForByteOffset(offset);
            ushort Native(int displacement) => (ushort)(rom.ReadByte(0xb3949b + offset + displacement)
                | rom.ReadByte(0xb3949c + offset + displacement) << 8);
            AssertEqual(Native(0), hole.Left, "Botwoon named hole native left boundary");
            AssertEqual(Native(4), hole.Top, "Botwoon named hole native top boundary");
            AssertEqual((ushort)(Native(0) + 4), hole.TargetX, "Botwoon named hole center X");
            AssertEqual((ushort)(Native(4) + 4), hole.TargetY, "Botwoon named hole center Y");
        }
        VerifyLookupStream4DarkLightningColors(rom);
        VerifyLookupStream4LightningColors(rom);
        VerifyLookupStream4BotwoonColors(rom);
        VerifyLookupStream4DraygonColors(rom);
        VerifyLookupStream4KzanCeresPrograms(rom);
        VerifyLookupStream4GeometryLayout(rom);
        VerifyLookupStream4PowerBombColors(rom);
        VerifyLookupStream4SporeAndFly(rom);
        VerifyLookupStream4Burial(rom);
        VerifyLookupStream4StatueColors(rom);
        VerifyLookupStream4Programs(rom);
        VerifyLookupStream4ProgramConsumers(rom);
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        for (byte theme = 0; theme < RoomTilesetDefinitions.Count; theme++)
        {
            ushort pointer = Word(0x8fe7a7 + 2 * theme);
            int source = 0x8f0000 | pointer;
            TilesetDefinition actual = RoomTilesetDefinitions.Get(theme);
            AssertEqual(pointer, actual.Pointer, "tileset native contiguous definition identity");
            AssertEqual(Long(source), actual.BlockDefinitionsAddress, "tileset native theme block resource");
            AssertEqual(Long(source + 3), actual.CharacterAddress, "tileset native theme character resource");
            AssertEqual(Long(source + 6), actual.PaletteAddress, "tileset native theme palette resource");
        }
        foreach (byte invalid in new byte[] { 0x1d, 0x7f, 0xff })
            AssertThrows<InvalidDataException>(() => RoomTilesetDefinitions.Get(invalid), "tileset exact graphics-theme domain");
        int Long(int address) => Word(address) | rom.ReadByte(address + 2) << 16;
        int[] nativeBackgroundSources = LibraryBackgroundSourceInventory.Scan(rom)
            .Where(instruction => instruction.Command == LibraryBackgroundCommand.DecompressToWorkRam)
            .Select(instruction => instruction.SourceAddress).Distinct().Order().ToArray();
        AssertEqual(58, nativeBackgroundSources.Length, "native distinct compressed background source count");
        AssertEqual(nativeBackgroundSources.Length, RoomBackgroundTilemapSources.All.Count, "calculated background source view count");
        AssertTrue(nativeBackgroundSources.SequenceEqual(RoomBackgroundTilemapSources.All), "background source view exact native distinct order");
        for (int index = 0; index < nativeBackgroundSources.Length; index++)
        {
            AssertEqual(nativeBackgroundSources[index], RoomBackgroundTilemapSources.All[index], "background source indexed view native identity");
            AssertTrue(RoomBackgroundTilemapSources.Contains(nativeBackgroundSources[index]), "background source native member accepted");
            AssertTrue(!RoomBackgroundTilemapSources.Contains(nativeBackgroundSources[index] + 1), "background source interior byte is not an identity");
        }
        foreach (int invalid in new[] { -1, 58, int.MinValue, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => _ = RoomBackgroundTilemapSources.All[invalid], "background source exact index domain");
        foreach (int invalid in new[] { -1, 0, 0xffffff, int.MaxValue })
            AssertTrue(!RoomBackgroundTilemapSources.Contains(invalid), "background source absent identity rejected");


        for (int pattern = 0; pattern < 4; pattern++)
        for (int stage = 0; stage < 6; stage++)
        {
            var actual = RidleyPogoDefinitions.Read(pattern, stage);
            int x = 0xa60000 | Word(0xa6b965 + 2 * pattern);
            int y = 0xa60000 | Word(0xa6b96d + 2 * pattern);
            AssertEqual(Word(x + 2 * stage), actual.X, "stream4 native pogo horizontal magnitude");
            AssertEqual(Word(y + 2 * stage), actual.Y, "stream4 native pogo signed vertical speed");
            AssertEqual(Word(0xa6b94d + 2 * stage), actual.UpwardAcceleration, "stream4 native pogo upward acceleration");
            AssertEqual(Word(0xa6b959 + 2 * stage), actual.DownwardAcceleration, "stream4 native pogo downward acceleration");
        }
        foreach (int invalid in new[] { -1, 4, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => RidleyPogoDefinitions.Read(invalid, 0), "stream4 invalid pogo pattern");
        foreach (int invalid in new[] { -1, 6, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => RidleyPogoDefinitions.Read(0, invalid), "stream4 invalid pogo stage");
        for (ushort parameter = 0; parameter <= 22; parameter += 2)
        {
            var actual = RidleyExplosionDefinitions.GetPart(parameter);
            AssertEqual(parameter, actual.Parameter, "stream4 breakup parameter identity");
            AssertEqual(Word(0xa6c6ce + parameter), actual.Lifetime, "stream4 native breakup lifetime");
            AssertEqual(Word(0xa6c6e6 + parameter), actual.InitializationRoutine, "stream4 native breakup initialization routine");
        }
        for (int orientation = 0; orientation < 16; orientation++)
            AssertEqual(Word(0xa6c7ba + 2 * orientation), RidleyExplosionDefinitions.SelectTailInstructionList(RidleyExplosionParts.TailTip, orientation), "stream4 native tail-tip orientation program");
        foreach (ushort invalid in new ushort[] { 1, 23, 24, ushort.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => RidleyExplosionDefinitions.GetPart(invalid), "stream4 invalid breakup parameter");
        foreach (int invalid in new[] { -1, 16, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => RidleyExplosionDefinitions.SelectTailInstructionList(RidleyExplosionParts.TailTip, invalid), "stream4 invalid tail-tip orientation");
    }
    private static void VerifyLookupStream4DarkLightningColors(ISnesAddressSpace rom)
    {
        var program = CrateriaLightningColorDefinitions.DarkProgram;
        byte[] originalJson = RoomPaletteFxPresentationExtractor.Extract(rom);
        using var originalStream = new MemoryStream(originalJson);
        var original = RoomPaletteFxPresentation.Load(originalStream);
        var stored = (Dictionary<ushort, ushort>)typeof(RoomPaletteFxPresentation).GetField("colors", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(original)!;
        for (int frame = 0; frame < 14; frame++)
        for (int index = 0; index < 7; index++)
        {
            ushort pointer = program.ColorPointer(frame, index);
            ushort native = (ushort)(rom.ReadByte(0x8d0000 | pointer) | rom.ReadByte(0x8d0000 | (pointer + 1)) << 8);
            AssertTrue(original.TryReadColor(pointer, out ushort actual), "dark lightning installed color exists");
            AssertEqual(native, actual, "dark lightning calculated native RGB5");
            AssertEqual(frame == 0, stored.ContainsKey(pointer), "dark lightning stores only unresolved base row");
            AssertEqual(1, original.ColorPointers.Count(p => p == pointer), "dark lightning audit identity enumerates once");
        }
        foreach ((int frame, int index) in new[] { (0, 0), (4, 6), (9, 2), (13, 5) })
        {
            var document = JsonSerializer.Deserialize<RoomPaletteFxPresentationDocument>(originalJson, MapPresentationFormat.JsonOptions)!;
            var source = document.CrateriaUnusedDarkLightning[frame][index];
            document.CrateriaUnusedDarkLightning[frame][index] = source with { Red = (source.Red + 9) & 31 };
            using var json = new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(document, MapPresentationFormat.JsonOptions));
            var edited = RoomPaletteFxPresentation.Load(json);
            for (int row = 0; row < 14; row++)
            for (int color = 0; color < 7; color++)
            {
                var expected = document.CrateriaUnusedDarkLightning[row][color];
                AssertTrue(edited.TryReadColor(program.ColorPointer(row, color), out ushort actual), "dark lightning edited color exists");
                AssertEqual((ushort)(expected.Red | expected.Green << 5 | expected.Blue << 10), actual,
                    "dark lightning independent base/sample edit and neighbors preserved");
            }
        }
        for (int address = 0xec6e; address <= 0xed83; address++)
        {
            bool expected = Enumerable.Range(1, 13).Any(frame => Enumerable.Range(0, 7).Any(index => program.ColorPointer(frame, index) == address));
            AssertEqual(expected, CrateriaLightningColorDefinitions.TryCalculatedDarkColor((ushort)address, stored, out _), "dark lightning exact calculated address domain");
        }
    }
    private static void VerifyLookupStream4LightningColors(ISnesAddressSpace rom)
    {
        var program = CrateriaLightningColorDefinitions.SurfaceProgram;
        byte[] originalJson = RoomPaletteFxPresentationExtractor.Extract(rom);
        using var originalStream = new MemoryStream(originalJson);
        var original = RoomPaletteFxPresentation.Load(originalStream);
        var stored = (Dictionary<ushort, ushort>)typeof(RoomPaletteFxPresentation).GetField("colors", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(original)!;
        for (int frame = 0; frame < 13; frame++)
        for (int index = 0; index < 8; index++)
        {
            ushort pointer = program.ColorPointer(frame, index);
            ushort native = (ushort)(rom.ReadByte(0x8d0000 | pointer) | rom.ReadByte(0x8d0000 | (pointer + 1)) << 8);
            AssertTrue(original.TryReadColor(pointer, out ushort actual), "surface lightning installed color exists");
            AssertEqual(native, actual, "surface lightning calculated native RGB5");
            AssertTrue(!stored.ContainsKey(pointer), "surface lightning stock words are not stored");
            AssertEqual(1, original.ColorPointers.Count(p => p == pointer), "surface lightning audit identity enumerates once");
        }
        foreach ((int frame, int index) in new[] { (0, 0), (4, 7), (8, 2), (12, 6) })
        {
            var document = JsonSerializer.Deserialize<RoomPaletteFxPresentationDocument>(originalJson, MapPresentationFormat.JsonOptions)!;
            var source = document.CrateriaSurfaceLightning[frame][index];
            document.CrateriaSurfaceLightning[frame][index] = source with { Red = (source.Red + 9) & 31 };
            using var json = new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(document, MapPresentationFormat.JsonOptions));
            var edited = RoomPaletteFxPresentation.Load(json);
            for (int row = 0; row < 13; row++)
            for (int color = 0; color < 8; color++)
            {
                var expected = document.CrateriaSurfaceLightning[row][color];
                AssertTrue(edited.TryReadColor(program.ColorPointer(row, color), out ushort actual), "surface lightning edited color exists");
                AssertEqual((ushort)(expected.Red | expected.Green << 5 | expected.Blue << 10), actual,
                    "surface lightning independent edit and neighbors preserved");
            }
        }
        for (int address = 0xeb3b; address <= 0xec58; address++)
        {
            bool expected = Enumerable.Range(0, 13).Any(frame => Enumerable.Range(0, 8).Any(index => program.ColorPointer(frame, index) == address));
            AssertEqual(expected, CrateriaLightningColorDefinitions.TryCalculatedColor((ushort)address, out _), "surface lightning exact color address domain");
        }
    }
    private static void VerifyLookupStream4Programs(ISnesAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);

        for (ushort phase = 0; phase < 4; phase++)
        {
            AssertEqual(Word(0xa3894e + 2 * phase), SkreeMetareeAnimationDefinitions.MetareeInstructionList((SkreeMetareeAnimationPhase)phase), "stream4 native Metaree phase program");
            AssertEqual(Word(0xa3c69c + 2 * phase), SkreeMetareeAnimationDefinitions.SkreeInstructionList((SkreeMetareeAnimationPhase)phase), "stream4 native Skree phase program");
        }
        foreach (ushort invalid in new ushort[] { 4, ushort.MaxValue })
        {
            AssertThrows<InvalidDataException>(() => SkreeMetareeAnimationDefinitions.MetareeInstructionList((SkreeMetareeAnimationPhase)invalid), "stream4 invalid Metaree phase");
            AssertThrows<InvalidDataException>(() => SkreeMetareeAnimationDefinitions.SkreeInstructionList((SkreeMetareeAnimationPhase)invalid), "stream4 invalid Skree phase");
        }
        int particleMechanics = 0, particlePresentation = 0;
        for (int address = 0x8abd; address < 0x8acd; address += 2)
        {
            bool presentation = address is 0x8abf or 0x8ac7;
            if (presentation)
            {
                AssertEqual((ushort)address, SkreeMetareeParticleInstructionProgramDefinitions.PresentationWordAddress(particlePresentation++), "stream4 debris presentation ordering");
                AssertThrows<InvalidDataException>(() => SkreeMetareeParticleInstructionProgramDefinitions.ReadMechanicsWord((ushort)address), "stream4 debris presentation not mechanics");
            }
            else
            {
                var actual = SkreeMetareeParticleInstructionProgramDefinitions.MechanicsWord(particleMechanics++);
                AssertEqual((ushort)address, actual.Address, "stream4 debris mechanics ordering");
                AssertEqual(Word(0x860000 | address), actual.Value, "stream4 native debris mechanics");
                AssertEqual(actual.Value, SkreeMetareeParticleInstructionProgramDefinitions.ReadMechanicsWord((ushort)address), "stream4 debris direct mechanics");
            }
            for (int half = 0; half < 2; half++)
                AssertEqual(!presentation, SkreeMetareeParticleInstructionProgramDefinitions.IsCompiledMechanicsByte(0x860000 | (address + half)), "stream4 debris byte ownership");
        }
        AssertEqual(particleMechanics, SkreeMetareeParticleInstructionProgramDefinitions.MechanicsWordCount, "stream4 debris mechanics count");
        AssertEqual(particlePresentation, SkreeMetareeParticleInstructionProgramDefinitions.PresentationWordCount, "stream4 debris presentation count");
        int electricMechanics = 0, electricPresentation = 0;
        for (int address = 0xe683; address < 0xe6ad; address += 2)
        {
            bool presentation = address >= 0xe689 && address <= 0xe6a5 && (address - 0xe689) % 4 == 0;
            if (presentation)
            {
                AssertEqual((ushort)address, SaveStationElectricityInstructionProgramDefinitions.PresentationWordAddress(electricPresentation++), "stream4 electricity presentation ordering");
                AssertThrows<InvalidDataException>(() => SaveStationElectricityInstructionProgramDefinitions.ReadMechanicsWord((ushort)address), "stream4 electricity presentation not mechanics");
            }
            else
            {
                var actual = SaveStationElectricityInstructionProgramDefinitions.MechanicsWord(electricMechanics++);
                AssertEqual((ushort)address, actual.Address, "stream4 electricity mechanics ordering");
                AssertEqual(Word(0x860000 | address), actual.Value, "stream4 native electricity mechanics");
                AssertEqual(actual.Value, SaveStationElectricityInstructionProgramDefinitions.ReadMechanicsWord((ushort)address), "stream4 electricity direct mechanics");
            }
            for (int half = 0; half < 2; half++)
                AssertEqual(!presentation, SaveStationElectricityInstructionProgramDefinitions.IsCompiledMechanicsByte(0x860000 | (address + half)), "stream4 electricity byte ownership");
        }
        AssertEqual(electricMechanics, SaveStationElectricityInstructionProgramDefinitions.MechanicsWordCount, "stream4 electricity mechanics count");
        AssertEqual(electricPresentation, SaveStationElectricityInstructionProgramDefinitions.PresentationWordCount, "stream4 electricity presentation count");
        foreach (int invalid in new[] { -1, int.MaxValue })
        {
            AssertThrows<IndexOutOfRangeException>(() => SkreeMetareeParticleInstructionProgramDefinitions.MechanicsWord(invalid), "stream4 debris mechanics bounds");
            AssertThrows<IndexOutOfRangeException>(() => SkreeMetareeParticleInstructionProgramDefinitions.PresentationWordAddress(invalid), "stream4 debris presentation bounds");
            AssertThrows<IndexOutOfRangeException>(() => SaveStationElectricityInstructionProgramDefinitions.MechanicsWord(invalid), "stream4 electricity mechanics bounds");
            AssertThrows<IndexOutOfRangeException>(() => SaveStationElectricityInstructionProgramDefinitions.PresentationWordAddress(invalid), "stream4 electricity presentation bounds");
        }
        AssertThrows<IndexOutOfRangeException>(() => SkreeMetareeParticleInstructionProgramDefinitions.MechanicsWord(particleMechanics), "stream4 debris mechanics end");
        AssertThrows<IndexOutOfRangeException>(() => SkreeMetareeParticleInstructionProgramDefinitions.PresentationWordAddress(particlePresentation), "stream4 debris presentation end");
        AssertThrows<IndexOutOfRangeException>(() => SaveStationElectricityInstructionProgramDefinitions.MechanicsWord(electricMechanics), "stream4 electricity mechanics end");
        AssertThrows<IndexOutOfRangeException>(() => SaveStationElectricityInstructionProgramDefinitions.PresentationWordAddress(electricPresentation), "stream4 electricity presentation end");
    }
    private static void VerifyLookupStream4ProgramConsumers(ISnesAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var process = typeof(RoomEnemySystem).GetMethod("ProcessEnemyProjectileInstructions", flags)!;

        var electricityGuard = new SaveStationElectricityInstructionReadGuard(rom);
        var electricitySystem = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(electricitySystem, electricityGuard);
        electricitySystem.SpawnSaveStationElectricity(42, 16);
        var electricity = electricitySystem.EnemyProjectiles.Single(p => p.Kind == RoomEnemyProjectileKind.SaveStationElectricity);
        for (int frame = 0; frame < 160; frame++)
        {
            electricity.InstructionTimer = 1;
            process.Invoke(electricitySystem, [electricity, null, (ushort)0, (ushort)0]);
            AssertTrue(electricity.IsActive, "stream4 electricity active throughout twenty cycles");
            AssertEqual((ushort)(0xe68b + 4 * (frame % 8)), electricity.InstructionPointer, "stream4 electricity exact frame order");
            AssertEqual((ushort)1, electricity.InstructionTimer, "stream4 electricity one-frame timing");
            AssertEqual((ushort)(0xe689 + 4 * (frame % 8)), electricity.PresentationOperandAddress, "stream4 electricity installed presentation operand identity");
        }
        electricity.InstructionTimer = 1;
        process.Invoke(electricitySystem, [electricity, null, (ushort)0, (ushort)0]);
        AssertTrue(!electricity.IsActive, "stream4 electricity deletes after twenty complete cycles");
        AssertEqual(0, electricityGuard.ForbiddenReadAttempts, "stream4 electricity no mechanics ROM reads");
        foreach (bool metaree in new[] { false, true })
        {
            var guard = new SkreeMetareeParticleInstructionReadGuard(rom);
            var system = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(system, guard);
            var spawn = typeof(RoomEnemySystem).GetMethod(metaree ? "SpawnMetareeParticleBurst" : "SpawnSkreeParticleBurst", flags)!.CreateDelegate<Action<RoomEnemySlot>>(system);
            spawn(new RoomEnemySlot(0) { XPosition = 128, YPosition = 96 });
            ushort start = metaree ? (ushort)0x8ac5 : (ushort)0x8abd;
            int active = 0;
            foreach (var particle in system.EnemyProjectiles)
            {
                if (!particle.IsActive) continue;
                active++;
                for (int loop = 0; loop < 2; loop++)
                {
                    particle.InstructionTimer = 1;
                    process.Invoke(system, [particle, null, (ushort)0, (ushort)0]);
                    AssertEqual((ushort)(start + 4), particle.InstructionPointer, "stream4 debris goto-self loop pointer");
                    AssertEqual((ushort)16, particle.InstructionTimer, "stream4 debris sixteen-frame duration");
                    ushort nativeSprite = (ushort)(rom.ReadByte(0x860000 | (start + 2)) | rom.ReadByte(0x860000 | (start + 3)) << 8);
                    AssertEqual(nativeSprite, particle.SpritemapPointer, "stream4 debris compiled presentation identity");
                }
            }
            AssertEqual(4, active, "stream4 debris native four-direction burst");
            AssertEqual(0, guard.ForbiddenReadAttempts, "stream4 debris no mechanics ROM reads");
        }
    }
    private static void VerifyLookupStream4StatueColors(ISnesAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);

        var native = new Dictionary<ushort, ushort>();
        for (int frame = 0; frame < 8; frame++)
        for (int color = 0; color < 8; color++)
            native.Add((ushort)(0xe240 + 20 * frame + 2 * color), Word(0x8de240 + 20 * frame + 2 * color));
        for (int raw = 0xe23e; raw <= 0xe2df; raw++)
        {
            ushort pointer = (ushort)raw;
            bool owned = native.ContainsKey(pointer);
            AssertEqual(owned, TourianStatueGreyColorDefinitions.TryCoordinates(pointer, out int frame, out int color), "stream4 statue exact color domain");
            bool intermediate = owned && frame is > 0 and < 7;
            AssertEqual(intermediate, TourianStatueGreyColorDefinitions.TryCalculatedColor(pointer, native, out ushort calculated), "stream4 statue intermediate domain");
            if (owned) AssertEqual((ushort)(0xe240 + 20 * frame + 2 * color), pointer, "stream4 statue color coordinates");
            if (intermediate) AssertEqual(native[pointer], calculated, "stream4 every original statue interpolation sample");
        }
        byte[] originalJson = RoomPaletteFxPresentationExtractor.Extract(rom);
        using var originalStream = new MemoryStream(originalJson);
        var original = RoomPaletteFxPresentation.Load(originalStream);
        var stored = (Dictionary<ushort, ushort>)typeof(RoomPaletteFxPresentation).GetField("colors", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(original)!;
        int storedStatueColors = 0;
        foreach (var pair in native)
        {
            AssertTrue(original.TryReadColor(pair.Key, out ushort value), "stream4 original statue color remains readable");
            AssertEqual(pair.Value, value, "stream4 loaded original statue color");
            if (stored.ContainsKey(pair.Key)) storedStatueColors++;
        }
        AssertEqual(16, storedStatueColors, "stream4 only endpoint rows stored, no generated intermediate cache");
        ushort[] keys = original.ColorPointers.Where(native.ContainsKey).ToArray();
        AssertEqual(64, keys.Length, "stream4 all statue pointers enumerable");
        AssertEqual(64, keys.Distinct().Count(), "stream4 statue pointer enumeration has no duplicates");
        foreach (var edit in new[] { (0, 0), (7, 0), (0, 2), (7, 3), (3, 4), (4, 0) })
        {
            var document = JsonSerializer.Deserialize<RoomPaletteFxPresentationDocument>(originalJson, MapPresentationFormat.JsonOptions)!;
            var source = document.TourianStatueGrey[edit.Item1][edit.Item2];
            document.TourianStatueGrey[edit.Item1][edit.Item2] = source with { Red = (source.Red + 9) & 31, Blue = (source.Blue + 5) & 31 };
            using var json = new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(document, MapPresentationFormat.JsonOptions));
            var edited = RoomPaletteFxPresentation.Load(json);
            for (int frame = 0; frame < 8; frame++)
            for (int color = 0; color < 8; color++)
            {
                var expected = document.TourianStatueGrey[frame][color];
                AssertTrue(edited.TryReadColor((ushort)(0xe240 + 20 * frame + 2 * color), out ushort actual), "stream4 edited statue color readable");
                AssertEqual((ushort)(expected.Red | expected.Green << 5 | expected.Blue << 10), actual, "stream4 endpoints and independent intermediate edits preserved");
            }
        }
        AssertTrue(!TourianStatueGreyColorDefinitions.TryCalculatedColor(0xe254, new Dictionary<ushort, ushort>(), out _), "stream4 absent statue endpoints are not invented");
    }
    private static void VerifyLookupStream4Burial(ISnesAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);

        for (int entry = 0; entry < 6; entry++)
        {
            var actual = DraygonBurialEvirDefinitions.ForEntry(entry);
            AssertEqual(Word(0xa5a1af + 4 * entry), actual.XSubspeed, "stream4 burial radial X subspeed");
            AssertEqual(Word(0xa5a1b1 + 4 * entry), actual.YSubspeed, "stream4 burial radial Y subspeed");
            AssertEqual(Word(0xa5a1c7 + 4 * entry), actual.InitialX, "stream4 burial radial spawn X including negative wrap");
            AssertEqual(Word(0xa5a1c9 + 4 * entry), actual.InitialY, "stream4 burial radial spawn Y");
            AssertEqual(Word(0xa5a1df + 4 * entry), actual.Angle, "stream4 burial radial angle");
        }
        foreach (int invalid in new[] { -1, 6, int.MaxValue })
            AssertThrows<InvalidDataException>(() => DraygonBurialEvirDefinitions.ForEntry(invalid), "stream4 burial exact six-record domain");
    }
    private static void VerifyLookupStream4SporeAndFly(ISnesAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);

        for (ushort index = 0; index < 4; index++)
        {
            AssertEqual(Word(0x86dcb9 + 2 * index), SporeSpawnProjectileDefinitions.StalkYOffset(index), "stream4 original stalk segment spacing");
            AssertEqual(Word(0x86dce6 + 2 * index), SporeSpawnProjectileDefinitions.SpawnerX(index), "stream4 original ceiling emitter spacing");
        }
        foreach (ushort invalid in new ushort[] { 4, ushort.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => SporeSpawnProjectileDefinitions.StalkYOffset(invalid), "stream4 stalk spawn domain");
            AssertThrows<ArgumentOutOfRangeException>(() => SporeSpawnProjectileDefinitions.SpawnerX(invalid), "stream4 emitter spawn domain");
        }
        Check(0xa20000, 0xb013, 0xb027, FlyInstructionProgramDefinitions.MechanicsWordCount,
            FlyInstructionProgramDefinitions.PresentationWordCount,
            i => { var word = FlyInstructionProgramDefinitions.MechanicsWord(i); return (word.Address, word.Value); },
            FlyInstructionProgramDefinitions.PresentationWordAddress, FlyInstructionProgramDefinitions.ReadMechanicsWord,
            FlyInstructionProgramDefinitions.IsCompiledMechanicsByte,
            a => a is 0xb015 or 0xb019 or 0xb01d or 0xb021);
        Check(0x860000, 0xb615, 0xb62d, EyeDoorSweatInstructionProgramDefinitions.MechanicsWordCount,
            EyeDoorSweatInstructionProgramDefinitions.PresentationWordCount,
            i => { var word = EyeDoorSweatInstructionProgramDefinitions.MechanicsWord(i); return (word.Address, word.Value); },
            EyeDoorSweatInstructionProgramDefinitions.PresentationWordAddress, EyeDoorSweatInstructionProgramDefinitions.ReadMechanicsWord,
            EyeDoorSweatInstructionProgramDefinitions.IsCompiledMechanicsByte,
            a => a is 0xb617 or 0xb621 or 0xb625 or 0xb629);
        for (int address = 0xb012; address <= 0xb027; address++)
            AssertEqual(address is 0xb015 or 0xb019 or 0xb01d or 0xb021,
                FlyInstructionProgramDefinitions.IsPresentationWord((ushort)address), "stream4 exact fly presentation classification");

        void Check(int bank, int first, int end, int mechanicsCount, int presentationCount,
            Func<int, (ushort Address, ushort Value)> mechanics, Func<int, ushort> presentation,
            Func<ushort, ushort> read, Func<int, bool> owns, Func<int, bool> isVisual)
        {
            int mechanical = 0, visual = 0;
            for (int address = first; address < end; address += 2)
            {
                bool selectedVisual = isVisual(address);
                if (selectedVisual)
                {
                    AssertEqual((ushort)address, presentation(visual++), "stream4 native ordered presentation operand");
                    AssertThrows<InvalidDataException>(() => read((ushort)address), "stream4 presentation excluded from mechanics");
                }
                else
                {
                    var actual = mechanics(mechanical++);
                    AssertEqual((ushort)address, actual.Address, "stream4 native ordered mechanics address");
                    AssertEqual(Word(bank | address), actual.Value, "stream4 original program mechanics value");
                    AssertEqual(actual.Value, read((ushort)address), "stream4 direct program mechanics value");
                }
                AssertEqual(!selectedVisual, owns(bank | address), "stream4 program low-byte ownership");
                AssertEqual(!selectedVisual, owns(bank | (address + 1)), "stream4 program high-byte ownership");
                AssertThrows<InvalidDataException>(() => read((ushort)(address + 1)), "stream4 unaligned program word rejected");
            }
            AssertEqual(mechanicsCount, mechanical, "stream4 exact mechanics count");
            AssertEqual(presentationCount, visual, "stream4 exact presentation count");
            AssertTrue(!owns(bank | (first - 1)) && !owns(bank | end) && !owns((bank ^ 0x10000) | first), "stream4 outside program bytes excluded");
            foreach (int invalid in new[] { -1, mechanicsCount, int.MaxValue })
                AssertThrows<IndexOutOfRangeException>(() => mechanics(invalid), "stream4 program mechanics enumeration bounds");
            foreach (int invalid in new[] { -1, presentationCount, int.MaxValue })
                AssertThrows<IndexOutOfRangeException>(() => presentation(invalid), "stream4 program presentation enumeration bounds");
        }
    }
    private static void VerifyLookupStream4PowerBombColors(ISnesAddressSpace rom)
    {
        byte[] original = PowerBombFixedColorExtractor.Extract(rom);
        var catalog = PowerBombFixedColorCatalog.Load(new MemoryStream(original));
        int Stored(PowerBombFixedColorCatalog value, string field) =>
            ((System.Collections.IDictionary)typeof(PowerBombFixedColorCatalog)
                .GetField(field, BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(value)!).Count;
        AssertEqual(0, Stored(catalog, "preExplosion"), "stock pre-explosion colors are entirely calculated");
        AssertEqual(11, Stored(catalog, "explosion"), "only unresolved explosion tail remains stored");
        foreach (var sequence in Enum.GetValues<PowerBombFixedColorSequence>())
        {
            int count = PowerBombFixedColorFormat.Count(sequence);
            for (int index = 0; index < count; index++)
            {
                int address = PowerBombFixedColorFormat.SourceAddress(sequence) + index * 3;
                var native = (rom.ReadByte(address), rom.ReadByte(address + 1), rom.ReadByte(address + 2));
                AssertEqual(native, catalog.Resolve(sequence, index), "installed Power Bomb native RGB channels");
                bool calculated = PowerBombFixedColorFormat.TryCalculateStock(sequence, index, out var color);
                AssertEqual(sequence == PowerBombFixedColorSequence.PreExplosion || index <= 20, calculated,
                    "Power Bomb exact calculated versus unresolved domain");
                if (calculated) AssertEqual(native, color, "Power Bomb direct color calculation");
            }
            foreach (int invalid in new[] { int.MinValue, -1, count, int.MaxValue })
                AssertThrows<ArgumentOutOfRangeException>(() => catalog.Resolve(sequence, invalid), "Power Bomb color index domain");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => catalog.Resolve((PowerBombFixedColorSequence)2, 0), "Power Bomb sequence domain");
        foreach (var edit in new[] { (PowerBombFixedColorSequence.PreExplosion, 0), (PowerBombFixedColorSequence.PreExplosion, 12),
            (PowerBombFixedColorSequence.Explosion, 0), (PowerBombFixedColorSequence.Explosion, 18), (PowerBombFixedColorSequence.Explosion, 31) })
        {
            var document = JsonSerializer.Deserialize<PowerBombFixedColorDocument>(original, MapPresentationFormat.JsonOptions)!;
            var rows = edit.Item1 == PowerBombFixedColorSequence.PreExplosion ? document.PreExplosion : document.Explosion;
            var before = rows[edit.Item2];
            rows[edit.Item2] = before with { Red = (before.Red + 3) & 31, Green = (before.Green + 7) & 31, Blue = (before.Blue + 11) & 31 };
            var installed = PowerBombFixedColorCatalog.Load(new MemoryStream(PowerBombFixedColorCatalog.Write(document)));
            foreach (var sequence in Enum.GetValues<PowerBombFixedColorSequence>())
            for (int index = 0; index < PowerBombFixedColorFormat.Count(sequence); index++)
            {
                var source = (sequence == PowerBombFixedColorSequence.PreExplosion ? document.PreExplosion : document.Explosion)[index];
                AssertEqual(((byte)source.Red, (byte)source.Green, (byte)source.Blue), installed.Resolve(sequence, index),
                    "Power Bomb supplied JSON color edits remain independent");
            }
            AssertEqual(edit.Item1 == PowerBombFixedColorSequence.PreExplosion ? 1 : 0, Stored(installed, "preExplosion"), "only changed pre-explosion content stays stored");
            AssertEqual(edit.Item1 == PowerBombFixedColorSequence.Explosion && edit.Item2 <= 20 ? 12 : 11, Stored(installed, "explosion"), "only changed calculated explosion content joins unresolved tail");
        }
    }
    private static void VerifyLookupStream4GeometryLayout(ISnesAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);

        for (int index = 0; index < 20; index++)
            AssertEqual(unchecked((short)Word(0x86b65b + index * 2)), EyeDoorEnemyProjectileRomData.ProjectileOriginWord(index), "eye-door native origin words including overlapping-pair selectors");
        for (int index = 0; index < 4; index++)
        {
            AssertEqual(unchecked((short)Word(0x86b6b1 + index * 2)), EyeDoorEnemyProjectileRomData.SweatVelocityWord(index), "eye-door native sweat velocity words");
            var target = DraygonCannonData.FiringTarget(index);
            AssertEqual((ushort)(0x8804 + 2 * index), target.DisabledWord, "Draygon original control-word selection");
            AssertEqual(Word(0xa587e4 + 4 * index), target.X, "Draygon original cannon X");
            AssertEqual(Word(0xa587e6 + 4 * index), target.Y, "Draygon original cannon Y");
        }
        for (ushort index = 0; index < 8; index++)
            AssertEqual(Word(0xa2cb77 + index * 2), MaridiaLargeSnailInstructionDefinitions.InstructionPointer(index), "Oum native action/facing dispatch");
        for (int slot = 0; slot < 3; slot++)
            AssertEqual(Word(0x81812b + slot * 2), SaveRamLayout.SlotOffset(slot), "native SRAM slot origins");
        foreach (int invalid in new[] { int.MinValue, -1, 20, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => EyeDoorEnemyProjectileRomData.ProjectileOriginWord(invalid), "eye-door origin word domain");
        foreach (int invalid in new[] { int.MinValue, -1, 4, int.MaxValue })
        {
            AssertThrows<IndexOutOfRangeException>(() => EyeDoorEnemyProjectileRomData.SweatVelocityWord(invalid), "eye-door sweat word domain");
            AssertThrows<IndexOutOfRangeException>(() => DraygonCannonData.FiringTarget(invalid), "Draygon cannon role domain");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 3, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => SaveRamLayout.SlotOffset(invalid), "save slot exact domain");
        foreach (ushort invalid in new ushort[] { 8, 255, ushort.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => MaridiaLargeSnailInstructionDefinitions.InstructionPointer(invalid), "Oum animation exact domain");
    }
    private static void VerifyLookupStream4KzanCeresPrograms(ISnesAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);

        int mechanical = 0, visual = 0;
        foreach (var program in new[] { (0x9552, 0x9574), (0x9574, 0x958c), (0x95a0, 0x95ba), (0x95d3, 0x95ed), (0x9606, 0x9620) })
        {
            for (int address = program.Item1; address < program.Item2; address += 2)
            {
                bool presentation = address is 0x9556 or 0x955e or 0x9562 or 0x9566 or 0x956a or 0x956e
                    or 0x9578 or 0x957c or 0x9580 or 0x9584 or 0x9588
                    or 0x95a4 or 0x95aa or 0x95ae or 0x95b2 or 0x95b6
                    or 0x95d7 or 0x95dd or 0x95e1 or 0x95e5 or 0x95e9
                    or 0x960a or 0x9610 or 0x9614 or 0x9618 or 0x961c;
                if (presentation)
                {
                    AssertEqual((ushort)address, CeresRidleyProjectileInstructionProgramDefinitions.PresentationWordAddress(visual++), "Ceres ordered presentation operand");
                    AssertThrows<InvalidDataException>(() => CeresRidleyProjectileInstructionProgramDefinitions.ReadMechanicsWord((ushort)address), "Ceres presentation excluded from control");
                }
                else
                {
                    var word = CeresRidleyProjectileInstructionProgramDefinitions.MechanicsWord(mechanical++);
                    AssertEqual((ushort)address, word.Address, "Ceres ordered control word");
                    AssertEqual(Word(0x860000 | address), word.Value, "Ceres original control word");
                    AssertEqual(word.Value, CeresRidleyProjectileInstructionProgramDefinitions.ReadMechanicsWord((ushort)address), "Ceres direct control word");
                }
                AssertEqual(!presentation, CeresRidleyProjectileInstructionProgramDefinitions.IsCompiledMechanicsByte(0x860000 | address), "Ceres control low byte");
                AssertEqual(!presentation, CeresRidleyProjectileInstructionProgramDefinitions.IsCompiledMechanicsByte(0x860000 | (address + 1)), "Ceres control high byte");
                AssertThrows<InvalidDataException>(() => CeresRidleyProjectileInstructionProgramDefinitions.ReadMechanicsWord((ushort)(address + 1)), "Ceres unaligned control word");
            }
            AssertTrue(!CeresRidleyProjectileInstructionProgramDefinitions.IsCompiledMechanicsByte(0x850000 | program.Item1), "Ceres wrong bank rejected");
        }
        AssertEqual(42, mechanical, "Ceres total control words");
        AssertEqual(26, visual, "Ceres total presentation words");
        foreach (int address in new[] { 0x9551, 0x958c, 0x959f, 0x95ba, 0x95d2, 0x95ed, 0x9605, 0x9620 })
            AssertTrue(!CeresRidleyProjectileInstructionProgramDefinitions.IsCompiledMechanicsByte(0x860000 | address), "Ceres bounded programs exclude adjacent code");
        for (int index = 0; index < 2; index++)
        {
            var word = KzanInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual((ushort)(0x8b29 + index * 4), word.Address, "Kzan draw then sleep addresses");
            AssertEqual(Word(0xa60000 | word.Address), word.Value, "Kzan original control word");
            AssertEqual(word.Value, KzanInstructionProgramDefinitions.ReadMechanicsWord(word.Address), "Kzan direct control word");
        }
        for (int offset = -1; offset <= 6; offset++)
            AssertEqual(offset is 0 or 1 or 4 or 5, KzanInstructionProgramDefinitions.IsCompiledMechanicsByte(0xa68b29 + offset), "Kzan exact byte coverage");
        foreach (int invalid in new[] { int.MinValue, -1, 42, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => CeresRidleyProjectileInstructionProgramDefinitions.MechanicsWord(invalid), "Ceres control index bounds");
        foreach (int invalid in new[] { int.MinValue, -1, 26, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => CeresRidleyProjectileInstructionProgramDefinitions.PresentationWordAddress(invalid), "Ceres presentation index bounds");
        foreach (int invalid in new[] { int.MinValue, -1, 2, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => KzanInstructionProgramDefinitions.MechanicsWord(invalid), "Kzan control index bounds");
    }
    private static void VerifyLookupStream4DraygonColors(ISnesAddressSpace rom)
    {
        byte[] original = DraygonColorExtractor.Extract(rom);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var stock = DraygonColorCatalog.Load(new MemoryStream(original));
        int Stored(string field) => ((System.Collections.IDictionary)typeof(DraygonColorCatalog)
            .GetField(field, BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(stock)!).Count;
        AssertEqual(0, Stored("whiteFlash"), "Draygon stock flash is calculated without stored colors");
        AssertEqual(8, Stored("healthBands"), "Draygon health stores only unresolved endpoint colors");
        for (int variation = -1; variation < 4; variation++)
        {
            var document = JsonSerializer.Deserialize<DraygonColorDocument>(original, options)!;
            var edited = new PaletteRgb5 { Red = 3, Green = 11, Blue = 21 };
            if (variation == 0) document.WhiteFlash[0] = edited;
            if (variation == 1) document.WhiteFlash[9] = edited;
            if (variation == 2) document.HealthBands[3][2] = edited;
            if (variation == 3) document.HealthBands[0][1] = edited;
            var catalog = DraygonColorCatalog.Load(new MemoryStream(DraygonColorCatalog.Write(document)));
            ushort Native(PaletteRgb5 color) => (ushort)(color.Red | color.Green << 5 | color.Blue << 10);
            string identity = SelectedPresentationHash.Create("DraygonColorCatalog-v1", content =>
            {
                content.AppendWords("intro", document.Intro.Select(Native).ToArray());
                content.AppendWords("background", document.Background.Select(Native).ToArray());
                content.AppendWords("sprite", document.Sprite.Select(Native).ToArray());
                content.AppendWords("whiteFlash", document.WhiteFlash.Select(Native).ToArray());
                content.AppendWordFrames("healthBands", document.HealthBands.Select(row => row.Select(Native).ToArray()).ToArray());
            });
            AssertEqual(identity, catalog.ContentIdentity, "Draygon calculated palette preserves selected content identity");
            for (int color = 0; color < 16; color++)
                AssertEqual(Native(document.WhiteFlash[color]), catalog.ResolveWhiteFlash(color), "Draygon native/edited flash remains independent");
            for (int band = 0; band < 8; band++)
            {
                var cgram = new SnesCgram();
                catalog.ApplyHealthBand(cgram, (ushort)(2 * band));
                for (int color = 0; color < 4; color++)
                {
                    ushort expected = Native(document.HealthBands[band][color]);
                    AssertEqual(expected, catalog.ResolveHealthBand(band, color), "Draygon native/edited health color remains independent");
                    AssertEqual(expected, cgram.Colors[89 + color], "Draygon health interpolation actual CGRAM transfer");
                }
            }
            var flashCgram = new SnesCgram();
            catalog.ApplyHurt(flashCgram, true, ushort.MaxValue);
            for (int color = 0; color < 16; color++)
            {
                AssertEqual(Native(document.WhiteFlash[color]), flashCgram.Colors[80 + color], "Draygon flash background transfer");
                AssertEqual(Native(document.WhiteFlash[color]), flashCgram.Colors[240 + color], "Draygon flash sprite transfer");
            }
        }
        foreach (int invalid in new[] { -1, 16, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveWhiteFlash(invalid), "Draygon flash bounds");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveHealthBand(8, 0), "Draygon health band bounds");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveHealthBand(0, 4), "Draygon health color bounds");
        AssertThrows<InvalidDataException>(() => stock.ApplyHealthBand(new SnesCgram(), 1), "Draygon odd native health selector rejected");
    }
    private static void VerifyLookupStream4BotwoonColors(ISnesAddressSpace rom)
    {
        byte[] original = BotwoonColorExtractor.Extract(rom);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var stock = BotwoonColorCatalog.Load(new MemoryStream(original));
        int stored = ((System.Collections.IDictionary)typeof(BotwoonColorCatalog)
            .GetField("health", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(stock)!).Count;
        AssertEqual(34, stored, "Botwoon only32 endpoint words and2 nonmatching words remain stored");
        for (int variation = -1; variation < 3; variation++)
        {
            var document = JsonSerializer.Deserialize<BotwoonColorDocument>(original, options)!;
            if (variation >= 0)
                document.Health[variation == 0 ? 0 : variation == 1 ? 3 : 1][variation == 2 ? 0 : 7]
                    = new PaletteRgb5 { Red = 3, Green = 11, Blue = 21 };
            var catalog = BotwoonColorCatalog.Load(new MemoryStream(BotwoonColorCatalog.Write(document)));
            ushort Native(PaletteRgb5 color) => (ushort)(color.Red | color.Green << 5 | color.Blue << 10);
            string identity = SelectedPresentationHash.Create("BotwoonColorCatalog-v1", content =>
                content.AppendWordFrames("health", document.Health.Select(row => row.Select(Native).ToArray()).ToArray()));
            AssertEqual(identity, catalog.ContentIdentity, "Botwoon calculated palette preserves selected identity");
            for (int band = 0; band < 8; band++)
            for (int color = 0; color < 16; color++)
                AssertEqual(Native(document.Health[band][color]), catalog.HealthColor(band, color),
                    "Botwoon native/edited colors preserve independent endpoint/middle/exception values");
        }
        foreach (int invalid in new[] { -1, 8, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.HealthColor(invalid, 0), "Botwoon health band domain");
        foreach (int invalid in new[] { -1, 16, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.HealthColor(0, invalid), "Botwoon health color domain");
    }
    private static void VerifyLookupStream4OumListLayout(ISnesAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        var nativeFrames = new Dictionary<ushort, ushort>();
        for (int frame = 0xcb87; frame <= 0xcca9; frame += 10)
            nativeFrames.Add((ushort)frame, Word(0xa20000 | (frame + 8)));
        AssertEqual(nativeFrames.Count, MaridiaLargeSnailCollisionDefinitions.FramePointers.Length, "Oum calculated frame count follows original extent");
        int ordinal = 0;
        foreach (ushort expectedFrame in nativeFrames.Keys)
            AssertEqual(expectedFrame, MaridiaLargeSnailCollisionDefinitions.FramePointers[ordinal++], "Oum calculated frame indexing preserves native order");
        foreach (int invalid in new[] { int.MinValue, -1, nativeFrames.Count, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => _ = MaridiaLargeSnailCollisionDefinitions.FramePointers[invalid], "Oum calculated frame view preserves index bounds");
        var selectedLists = new HashSet<ushort>();
        int rectangles = 0;
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            ushort frame = (ushort)raw;
            bool valid = nativeFrames.TryGetValue(frame, out ushort expected);
            AssertEqual(valid, MaridiaLargeSnailCollisionDefinitions.HasFrame(frame), "Oum frame domain remains exact");
            if (!valid)
            {
                AssertThrows<InvalidDataException>(() => MaridiaLargeSnailCollisionDefinitions.HitboxListAt(frame), "Oum rejects every non-frame pointer");
                continue;
            }
            ushort actual = MaridiaLargeSnailCollisionDefinitions.HitboxListAt(frame);
            AssertEqual(expected, actual, "Calculated Oum record identity matches original frame operand");
            selectedLists.Add(actual);
            var boxes = MaridiaLargeSnailCollisionDefinitions.HitboxesAt(actual);
            AssertEqual((int)Word(0xa20000 | expected), boxes.Length, "Calculated Oum pointer reaches the native rectangle count");
            for (int index = 0; index < boxes.Length; index++)
            {
                int pointer = 0xa20000 | (expected + 2 + index * 12);
                var box = boxes[index];
                AssertEqual(unchecked((short)Word(pointer)), box.Left, "Oum selected left bound");
                AssertEqual(unchecked((short)Word(pointer + 2)), box.Top, "Oum selected top bound");
                AssertEqual(unchecked((short)Word(pointer + 4)), box.Right, "Oum selected right bound");
                AssertEqual(unchecked((short)Word(pointer + 6)), box.Bottom, "Oum selected bottom bound");
                AssertEqual(Word(pointer + 8), box.TouchAi, "Oum selected touch callback");
                AssertEqual(Word(pointer + 10), box.ShotAi, "Oum selected shot callback");
                rectangles++;
            }
        }
        AssertTrue(nativeFrames.Keys.SequenceEqual(MaridiaLargeSnailCollisionDefinitions.FramePointers.ToArray()), "Oum frame enumeration preserves native order");
        AssertTrue(selectedLists.SetEquals(MaridiaLargeSnailCollisionDefinitions.HitboxPointers), "Oum list calculation covers the original independent payload exactly");
        Console.WriteLine($"Oum calculated list identities: {nativeFrames.Count} frames, {rectangles} native rectangles and complete ushort rejection domain pass.");
    }

    private static void VerifyLookupStream4BreakupOrder(ISnesAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        ushort[] expected = Enumerable.Range(0, 12).Select(index =>
            Word(0xa60000 | (Word(0xa6c933 + index * 7) + 12))).ToArray();
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        foreach (int freeSlots in new[] { 12, 2, 0 })
        {
            var enemies = new RoomEnemySystem();
            foreach (RoomEnemySlot slot in enemies.Slots) slot.EnemyDefinitionPointer = 0xffff;
            for (int index = 1; index <= freeSlots; index++) enemies.Slots[index].Clear();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, new FrontendCartridgeReadGuard(rom));
            int randomCalls = 0;
            typeof(RoomEnemySystem).GetField("_nextRandom", flags)!.SetValue(enemies,
                (Func<ushort>)(() => (ushort)(0x100 + 0x10 * randomCalls++)));
            var spawn = typeof(RoomEnemySystem).GetMethod("SpawnNorfairRidleyBreakupActors", flags)!
                .CreateDelegate<Action<RoomEnemySlot, RidleyEnemyState>>(enemies);
            RoomEnemySlot body = enemies.Slots[0];
            var state = new RidleyEnemyState
            {
                TailSegments = Enumerable.Range(0, 7).Select(index => new RidleyTailSegment
                {
                    XPosition = (ushort)(100 + index), YPosition = (ushort)(200 + index),
                }).ToArray(),
            };
            if (freeSlots < expected.Length)
                AssertThrows<InvalidOperationException>(() => spawn(body, state), "Breakup preserves partial allocation before pool exhaustion");
            else spawn(body, state);
            AssertTrue(state.DeathBreakupSpawned, "Breakup commits its one-shot guard before allocation");
            AssertEqual(freeSlots, randomCalls, "Breakup consumes RNG once per successfully allocated fragment only");
            for (int index = 0; index < freeSlots; index++)
            {
                RoomEnemySlot fragment = enemies.Slots[index + 1];
                AssertEqual(expected[index], fragment.Parameter1, "Actual breakup slot order follows original native spawn calls");
                AssertEqual((ushort)((0x100 + 0x10 * index) & 0x130), fragment.VariableB, "Actual breakup maps each random draw to its original fragment");
                AssertEqual(Word(0xa6c6ce + expected[index]), fragment.VariableF, "Actual breakup preserves each unresolved native lifetime");
            }
            spawn(body, state);
            AssertEqual(freeSlots, randomCalls, "Breakup retry after success or failure does not repeat allocation or RNG");
        }
        Console.WriteLine("Ridley ordered spawning: native twelve-call order, exact RNG assignment, partial/full pool exhaustion and one-shot retry behavior pass.");
    }

    private static void VerifyLookupStream4BeamTileGeometry(ISnesAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        int diagonalX = Word(0x93D110) & 0x1FF;
        if (diagonalX >= 0x100) diagonalX -= 0x200;
        AssertEqual(diagonalX, SpazerCompositionGeometryDefinitions.DiagonalFirstPairOriginX, "shared diagonal origin matches original component");
        AssertEqual((int)unchecked((sbyte)rom.ReadByte(0x93D112)), SpazerCompositionGeometryDefinitions.DiagonalFirstPairOriginY, "shared diagonal Y remains exact source input");
        AssertEqual((int)unchecked((sbyte)rom.ReadByte(0x93D6EE)), SpazerCompositionGeometryDefinitions.HorizontalStripOriginY, "horizontal strip centers the native small OBJ");
        AssertEqual(0x32, Word(0x93D113) & 0x1FF, "first H-flipped diagonal component is raw tile2");
        AssertEqual(0x31, Word(0x93D118) & 0x1FF, "second H-flipped diagonal component is raw tile1");
        AssertEqual(0x4000, Word(0x93D113) & 0x4000, "diagonal first component horizontal flip");
        AssertEqual(0x4000, Word(0x93D118) & 0x4000, "diagonal second component horizontal flip");
        AssertEqual(16, SpazerCompositionGeometryDefinitions.DiagonalStripWidth, "two adjacent small cells form the strip width");
        AssertEqual(2, BeamTileAtlasDefinitions.SpazerRibbonThickness, "required horizontal artwork thickness stays explicit");
        var files = BeamTileExtractor.Extract(rom);
        var palettes = BeamPaletteCatalog.Load(new MemoryStream(BeamPaletteExtractor.Extract(rom)));
        var catalog = BeamTileCatalog.Load(files, palettes);
        int derivedTotal = 0;
        int runtimeBasisTotal = 0;
        var compiled = (BeamTileAtlas[])typeof(BeamTileCatalog).GetField("sheets", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(catalog)!;
        for (int selection = 0; selection < 12; selection++)
        {
            int sourceAddress = 0x9a0000 | Word(0x90c3b1 + selection * 2);
            byte[] native = Enumerable.Range(0, 256).Select(offset => rom.ReadByte(sourceAddress + offset)).ToArray();
            byte[] png = files[BeamTileAtlasDefinitions.FileName(selection)];
            var atlas = BeamTileAtlas.Load(new MemoryStream(png), selection);
            var stored = (Dictionary<int, byte>)typeof(BeamTileAtlas).GetField("pixels",
                BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(atlas)!;
            int expectedBasis = selection switch { 0 => 224, 2 => 288, 1 or 3 => 400, >= 4 and <= 7 => 181, _ => 299 };
            AssertEqual(expectedBasis, stored.Count, "Beam sheet stores exactly its required pixel basis with no stock derived fallbacks");
            for (int pixel = 0; pixel < 512; pixel++)
            {
                bool derived = BeamTileAtlasDefinitions.TryDerivedPixelSource(selection, pixel, out int sourcePixel);
                if (selection is >= 4 and <= 7 && pixel % 64 / 8 is 1 or 2)
                    AssertEqual(pixel is not (9 or 74 or 139 or 140 or 399), derived,
                        "Spazer diagonal geometry and texture retain exactly five original ink seeds");
                bool ink = BeamTileAtlasDefinitions.TryStockPlasmaInk(selection, pixel, out byte selectedInk);
                AssertEqual((selection & 8) != 0 && pixel is 192 or 193 or 195 or 256 or 257, ink,
                    "Only the five independently reviewed original Plasma seed positions have retained pen values");
                AssertEqual(!derived && !ink, stored.ContainsKey(pixel), "Every calculated stock pixel is absent from stored basis");
                if (derived || ink)
                {
                    derivedTotal++;
                    AssertEqual(ReadPixel(native, pixel), ink ? selectedInk : sourcePixel < 0 ? (byte)0 : ReadPixel(native, sourcePixel),
                        "Direct transpose, quarter-turn or transparency equals independently decoded native pixels");
                }
            }
            int canonical = BeamTileAtlasDefinitions.CanonicalSelection(selection);
            AssertEqual(Word(0x90c3b1 + canonical * 2), Word(0x90c3b1 + selection * 2), "Canonical beam family matches original native source-pointer identity");
            var compiledPixels = (Dictionary<int, byte>)typeof(BeamTileAtlas).GetField("pixels",
                BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(compiled[selection])!;
            int expectedRuntimeBasis = selection switch { 0 => 224, 1 => 272, 2 => 288, 4 => 10, 8 => 299, _ => 0 };
            AssertEqual(expectedRuntimeBasis, compiledPixels.Count, "Runtime catalog stores only independent primary-family artwork pixels");
            runtimeBasisTotal += compiledPixels.Count;
            int sharedSelection = BeamTileAtlasDefinitions.SharedTileSourceSelection(selection);
            byte[] sharedNative = sharedSelection < 0 ? [] : Enumerable.Range(0, 256)
                .Select(offset => rom.ReadByte((0x9a0000 | Word(0x90c3b1 + sharedSelection * 2)) + offset)).ToArray();
            for (int pixel = 0; pixel < 512; pixel++)
            {
                bool shared = BeamTileAtlasDefinitions.IsSharedTilePixel(selection, pixel);
                bool geometry = BeamTileAtlasDefinitions.TryDerivedPixelSource(selection, pixel, out _) ||
                    BeamTileAtlasDefinitions.TryStockPlasmaInk(selection, pixel, out _);
                AssertEqual(selection == canonical && !shared && !geometry, compiledPixels.ContainsKey(pixel),
                    "No shared-source, rotation or blank stock pixel is hidden in runtime storage");
                if (shared)
                    AssertEqual(ReadPixel(native, pixel), ReadPixel(sharedNative, pixel), "Shared tile relationship matches both independent original ROM sheets");
            }
            AssertTrue(native.AsSpan().SequenceEqual(atlas.Transfer.Span), "Calculated DMA view matches all256 original bytes per beam selection");
            var directVram = new SnesVram();
            atlas.LoadTo(directVram);
            AssertTrue(native.AsSpan().SequenceEqual(directVram.Bytes.Slice(0x6300 * 2, 256)), "Actual direct beam upload uses calculated native bytes");
            VerifyQueued(catalog, selection, native);
            var image = IndexedPng.Read(new MemoryStream(png), 64, 8);
            foreach (int editedPixel in new[] { 0, 7, 16, 24, 32, 40, 192, 193, 195, 256, 257, 320, 296, 303, 304, 311, 312, 511, 96, 127, 289, 295, 160, 164, 8, 9, 466, 467, 74, 139, 140, 399, 141, 204, 416, 224, 352 })
            {
                byte[] pixels = image.Pixels.ToArray();
                pixels[editedPixel] ^= 1;
                using var changedPng = new MemoryStream();
                IndexedPng.Write(changedPng, 64, 8, pixels, image.Palette);
                byte[] changed = changedPng.ToArray();
                var edited = BeamTileAtlas.Load(new MemoryStream(changed), selection);
                byte[] expected = native.ToArray();
                int tile = editedPixel % 64 / 8;
                int y = editedPixel / 64;
                int x = editedPixel % 8;
                expected[tile * 32 + y * 2] ^= (byte)(1 << (7 - x));
                AssertTrue(expected.AsSpan().SequenceEqual(edited.Transfer.Span), "Editing basis, transpose, rotation or transparent sites changes only the supplied native pixel bit");
                var editedFiles = new Dictionary<string, byte[]>(files) { [BeamTileAtlasDefinitions.FileName(selection)] = changed };
                var editedCatalog = BeamTileCatalog.Load(editedFiles, palettes);
                VerifyQueued(editedCatalog, selection, expected);
                for (int other = 0; other < 12; other++)
                    if (other != selection)
                        AssertTrue(catalog.Resolve(BeamTileCatalog.AssetFor(other)).Span.SequenceEqual(editedCatalog.Resolve(BeamTileCatalog.AssetFor(other)).Span),
                            "Shared native beam shapes remain independently editable per installed selection");
                AssertTrue(native.AsSpan().SequenceEqual(atlas.Transfer.Span), "Pixel edits do not mutate original calculated atlas");
            }
        }
        AssertEqual(1093, runtimeBasisTotal, "Runtime native artwork basis excludes exactly the derived row and tile relationships");
        AssertEqual(2912, derivedTotal, "Twelve standalone selections calculate exactly2912 orientation, row-pattern, blank and approved pen occurrences");
        foreach (int invalid in new[] { int.MinValue, -1, 12, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => BeamTileAtlas.Load(new MemoryStream(), invalid), "Beam atlas rejects unsupported selection before decoding PNG");
        Console.WriteLine("Beam geometry: twelve native256-byte uploads,5046 calculated/shared pixels,5 narrowly retained pen roles and1093 required basis pixels; exact stock storage, independent edits and actual queued NMI drains pass.");

        void VerifyQueued(BeamTileCatalog selected, int selection, byte[] expected)
        {
            var queue = new VramWriteQueue();
            var cgram = new SnesCgram();
            SamusProjectileSystem.QueueBeamTilesAndLoadPalette(new ProjectileCompositionForbiddenBus(), queue, cgram, (ushort)selection, selected);
            AssertEqual(7, queue.TailInBytes, "Beam geometry retains native seven-byte queue record");
            AssertEqual((int)Word(0x90ac9f), (int)queue.Entries[0].SizeInBytes, "Actual beam DMA length matches native immediate0100");
            AssertEqual((int)Word(0x90acb6), (int)queue.Entries[0].EncodedVramDestination, "Actual beam DMA destination matches native immediate6300");
            var vram = new SnesVram();
            queue.DrainTo(vram, ReferenceMutableMemory.From(new ProjectileCompositionForbiddenBus()), selected);
            AssertTrue(expected.AsSpan().SequenceEqual(vram.Bytes.Slice(0x6300 * 2, 256)), "NMI resolves current calculated pixels without runtime cartridge reads");
            AssertEqual((byte)0, vram.ReadByte(0x6300 * 2 - 1), "Beam transfer preserves preceding VRAM byte");
            AssertEqual((byte)0, vram.ReadByte(0x6300 * 2 + 256), "Beam transfer preserves following VRAM byte");
            AssertEqual(0, queue.TailInBytes, "NMI clears the consumed calculated beam transfer");
        }

        static byte ReadPixel(byte[] native, int pixel)
        {
            int tile = pixel % 64 / 8, x = pixel % 8, y = pixel / 64, result = 0;
            for (int plane = 0; plane < 4; plane++)
                result |= ((native[tile * 32 + plane / 2 * 16 + y * 2 + plane % 2] >> (7 - x)) & 1) << plane;
            return (byte)result;
        }
    }
}
