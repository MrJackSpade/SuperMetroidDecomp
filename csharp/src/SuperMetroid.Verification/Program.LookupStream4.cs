using SuperMetroid.Core.Rooms;
using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
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
        VerifyLookupStream4MessageDispatch(rom);
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
}
