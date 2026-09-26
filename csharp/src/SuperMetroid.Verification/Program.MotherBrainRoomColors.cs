using System.Reflection;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyMotherBrainRoomColors(ISnesAddressSpace rom, string stock,
        string overrides, AreaMapPresentationCatalog original)
    {
        var nativeCgram = new SnesCgram();
        var installedCgram = new SnesCgram();
        Seed(nativeCgram);
        Seed(installedCgram);
        var native = CreateEnemy(rom, nativeCgram, null);
        var installed = CreateEnemy(new PaletteReadForbiddenBus(), installedCgram,
            original.MotherBrainRoomColors);
        Method("LoadMotherBrainRoomEntryColors").Invoke(native, null);
        Method("LoadMotherBrainRoomEntryColors").Invoke(installed, null);
        AssertTrue(nativeCgram.Colors.SequenceEqual(installedCgram.Colors),
            "installed Mother Brain room-entry glass and tube palettes match full native CGRAM");
        var nativeState = new MotherBrainEnemyState(native.Slots[0])
        {
            RoomPaletteInstructionPointer = MotherBrainRoomPaletteProgramDefinitions.FlashStart,
        };
        var installedState = new MotherBrainEnemyState(installed.Slots[0])
        {
            RoomPaletteInstructionPointer = MotherBrainRoomPaletteProgramDefinitions.FlashStart,
        };
        MethodInfo run = Method("RunMotherBrainRoomPalette");
        for (int frame = 0; frame < 48; frame++)
        {
            run.Invoke(native, [nativeState]);
            run.Invoke(installed, [installedState]);
            AssertTrue(nativeCgram.Colors.SequenceEqual(installedCgram.Colors),
                $"installed Mother Brain room flash matches full native CGRAM on frame {frame}");
            AssertEqual(nativeState.RoomPaletteInstructionPointer,
                installedState.RoomPaletteInstructionPointer,
                "room-flash presentation leaves bytecode pointer unchanged");
            AssertEqual(nativeState.RoomPaletteInstructionTimer,
                installedState.RoomPaletteInstructionTimer,
                "room-flash presentation leaves bytecode timer unchanged");
        }
        Method("StopMotherBrainRoomPalette").Invoke(native, [nativeState]);
        Method("StopMotherBrainRoomPalette").Invoke(installed, [installedState]);
        AssertTrue(nativeCgram.Colors.SequenceEqual(installedCgram.Colors),
            "installed final grey room colors match all native CGRAM words");
        AssertEqual(nativeState.RoomPaletteInstructionPointer,
            installedState.RoomPaletteInstructionPointer,
            "final room palette preserves bytecode stop state");

        Seed(nativeCgram);
        Seed(installedCgram);
        Method("SetupMotherBrainPhaseTwoGraphics").Invoke(native, [nativeState]);
        Method("SetupMotherBrainPhaseTwoGraphics").Invoke(installed, [installedState]);
        AssertTrue(nativeCgram.Colors.SequenceEqual(installedCgram.Colors),
            "installed phase-two attack/rear-leg colors match full native CGRAM");
        AssertEqual(nativeState.Function, installedState.Function,
            "phase-two colors do not alter the next body function");
        AssertEqual(nativeState.EnableUnpauseHook, installedState.EnableUnpauseHook,
            "phase-two colors do not alter the unpause hook");

        string replacement = Path.Combine(overrides, MotherBrainRoomColorFormat.FileName);
        MotherBrainRoomColorDocument document =
            JsonSerializer.Deserialize<MotherBrainRoomColorDocument>(
                File.ReadAllBytes(Path.Combine(stock, MotherBrainRoomColorFormat.FileName)),
                MapPresentationFormat.JsonOptions)
            ?? throw new InvalidDataException("Extracted Mother Brain room colors are null.");
        document.Flash[0][0] = Change(document.Flash[0][0]);
        document.FinalRoom[MotherBrainRoomColorRomData.SliceColors] =
            Change(document.FinalRoom[MotherBrainRoomColorRomData.SliceColors]);
        document.PhaseTwoAttack[0] = Change(document.PhaseTwoAttack[0]);
        document.PhaseTwoRearLeg[0] = Change(document.PhaseTwoRearLeg[0]);
        document.InitialGlassShard![0] = Change(document.InitialGlassShard[0]);
        document.InitialTubeProjectile![0] = Change(document.InitialTubeProjectile[0]);
        using (var file = File.Create(replacement))
            MotherBrainRoomColorPresentation.Write(file, document);
        AreaMapPresentationCatalog edited = AreaMapPresentationCatalog.Load(stock, overrides);
        AssertTrue(edited.ContentIdentity != original.ContentIdentity,
            "Mother Brain room-color override changes selected-content identity");
        var runtime = new SuperMetroid.Core.Runtime.SuperMetroidRuntime(rom)
        {
            MapPresentation = edited,
        };
        AssertTrue(ReferenceEquals(runtime.Enemies.MotherBrainRoomColors,
            edited.MotherBrainRoomColors),
            "installed Mother Brain room colors reach the production enemy binding");
        AssertEdited("RunMotherBrainRoomPalette", original, edited,
            MotherBrainRoomColorRomData.FirstColor, 1, flash: true);
        AssertEdited("StopMotherBrainRoomPalette", original, edited,
            MotherBrainRoomColorRomData.SecondColor, 2);
        AssertEdited("SetupMotherBrainPhaseTwoGraphics", original, edited,
            MotherBrainRoomColorRomData.PhaseTwoAttackColor, 2);
        AssertRoomEntryEdited(original, edited);

        AssertThrows<InvalidDataException>(() => MotherBrainRoomColorPresentation.Load(
            new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(document with
            {
                Flash = document.Flash.Take(13).ToArray(),
            }, MapPresentationFormat.JsonOptions))), "reject truncated Mother Brain room-flash list");
        AssertThrows<InvalidDataException>(() => MotherBrainRoomColorPresentation.Load(
            new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(document with
            {
                PhaseTwoAttack = document.PhaseTwoAttack.Take(14).ToArray(),
            }, MapPresentationFormat.JsonOptions))), "reject truncated phase-two attack colors");
        AssertThrows<InvalidDataException>(() => MotherBrainRoomColorPresentation.Load(
            new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(document with
            {
                InitialGlassShard = document.InitialGlassShard!.Take(14).ToArray(),
            }, MapPresentationFormat.JsonOptions))), "reject truncated room-entry glass colors");
        byte[] legacy = JsonSerializer.SerializeToUtf8Bytes(document with
        {
            Version = MotherBrainRoomColorFormat.PreRoomEntryVersion,
            InitialGlassShard = null,
            InitialTubeProjectile = null,
        }, MapPresentationFormat.JsonOptions);
        File.WriteAllBytes(replacement, legacy);
        AreaMapPresentationCatalog migrated = AreaMapPresentationCatalog.Load(stock, overrides);
        var migratedFlash = new SnesCgram();
        var originalFlash = new SnesCgram();
        migrated.MotherBrainRoomColors.ApplyFlash(migratedFlash,
            MotherBrainRoomPaletteProgramDefinitions.FlashStart);
        original.MotherBrainRoomColors.ApplyFlash(originalFlash,
            MotherBrainRoomPaletteProgramDefinitions.FlashStart);
        AssertTrue(!migratedFlash.Colors.SequenceEqual(originalFlash.Colors),
            "version-one room-color override retains its existing flash edit");
        var migratedEntry = new SnesCgram();
        var originalEntry = new SnesCgram();
        migrated.MotherBrainRoomColors.ApplyRoomEntry(migratedEntry);
        original.MotherBrainRoomColors.ApplyRoomEntry(originalEntry);
        AssertTrue(migratedEntry.Colors.SequenceEqual(originalEntry.Colors),
            "version-one room-color override inherits only the new room-entry colors from verified stock");
        File.WriteAllText(replacement, "{ broken JSON");
        AssertThrows<InvalidDataException>(() => AreaMapPresentationCatalog.Load(stock, overrides),
            "corrupt Mother Brain room override fails loudly");
        AssertEqual("{ broken JSON", File.ReadAllText(replacement),
            "corrupt Mother Brain override remains available for repair");
        File.Delete(replacement);
        AssertEqual(original.ContentIdentity, AreaMapPresentationCatalog.Load(stock, overrides)
            .ContentIdentity, "removing Mother Brain room override restores stock identity");
        Console.WriteLine("Mother Brain room colors: entry, 14 flash frames, final grey and phase-two palettes match native CGRAM; ROM-free production paths, isolated edits, legacy override and strict validation pass.");

        static PaletteRgb5 Change(PaletteRgb5 color) => color with
        {
            Red = color.Red == 31 ? 30 : color.Red + 1,
        };
    }

    private static void AssertRoomEntryEdited(AreaMapPresentationCatalog original,
        AreaMapPresentationCatalog edited)
    {
        var stockCgram = new SnesCgram();
        var editedCgram = new SnesCgram();
        var stockEnemy = CreateEnemy(new PaletteReadForbiddenBus(), stockCgram,
            original.MotherBrainRoomColors);
        var editedEnemy = CreateEnemy(new PaletteReadForbiddenBus(), editedCgram,
            edited.MotherBrainRoomColors);
        Method("LoadMotherBrainRoomEntryColors").Invoke(stockEnemy, null);
        Method("LoadMotherBrainRoomEntryColors").Invoke(editedEnemy, null);
        AssertTrue(stockCgram.Colors[MotherBrainRoomColorRomData.InitialGlassShardColor] !=
            editedCgram.Colors[MotherBrainRoomColorRomData.InitialGlassShardColor],
            "room-entry glass edit reaches the production color copy");
        AssertTrue(stockCgram.Colors[MotherBrainRoomColorRomData.InitialTubeProjectileColor] !=
            editedCgram.Colors[MotherBrainRoomColorRomData.InitialTubeProjectileColor],
            "room-entry tube edit reaches the production color copy");
        AssertEqual(2, stockCgram.Colors.ToArray().Zip(editedCgram.Colors.ToArray())
            .Count(pair => pair.First != pair.Second),
            "room-entry edits change only the two selected CGRAM colors");
    }

    private static void AssertEdited(string methodName, AreaMapPresentationCatalog original,
        AreaMapPresentationCatalog edited, int expectedFirstColor, int expectedDifferences,
        bool flash = false)
    {
        var stockCgram = new SnesCgram();
        var editedCgram = new SnesCgram();
        var stockEnemy = CreateEnemy(new PaletteReadForbiddenBus(), stockCgram,
            original.MotherBrainRoomColors);
        var editedEnemy = CreateEnemy(new PaletteReadForbiddenBus(), editedCgram,
            edited.MotherBrainRoomColors);
        var stockState = new MotherBrainEnemyState(stockEnemy.Slots[0]);
        var editedState = new MotherBrainEnemyState(editedEnemy.Slots[0]);
        if (flash)
        {
            stockState.RoomPaletteInstructionPointer =
                MotherBrainRoomPaletteProgramDefinitions.FlashStart;
            editedState.RoomPaletteInstructionPointer =
                MotherBrainRoomPaletteProgramDefinitions.FlashStart;
        }
        MethodInfo method = Method(methodName);
        method.Invoke(stockEnemy, [stockState]);
        method.Invoke(editedEnemy, [editedState]);
        AssertTrue(stockCgram.Colors[expectedFirstColor] !=
            editedCgram.Colors[expectedFirstColor],
            $"edited {methodName} color reaches live CGRAM");
        AssertEqual(expectedDifferences, stockCgram.Colors.ToArray().Zip(editedCgram.Colors.ToArray())
            .Count(pair => pair.First != pair.Second),
            $"edited {methodName} changes only the selected CGRAM destinations");
        AssertEqual(stockState.Function, editedState.Function,
            $"edited {methodName} preserves body phase");
        AssertEqual(stockState.RoomPaletteInstructionPointer,
            editedState.RoomPaletteInstructionPointer,
            $"edited {methodName} preserves palette-program cursor");
        AssertEqual(stockState.RoomPaletteInstructionTimer,
            editedState.RoomPaletteInstructionTimer,
            $"edited {methodName} preserves palette-program timer");
    }

    private static RoomEnemySystem CreateEnemy(ISnesAddressSpace bus, SnesCgram cgram,
        MotherBrainRoomColorPresentation? colors)
    {
        var enemies = new RoomEnemySystem { MotherBrainRoomColors = colors };
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, bus);
        typeof(RoomEnemySystem).GetField("_cgram", flags)!.SetValue(enemies, cgram);
        return enemies;
    }

    private static MethodInfo Method(string name) => typeof(RoomEnemySystem).GetMethod(name,
        BindingFlags.Instance | BindingFlags.NonPublic)!;

    private static void Seed(SnesCgram cgram)
    {
        for (int index = 0; index < SnesCgram.ColorCount; index++)
            cgram.SetColor(index, (ushort)(index * 71 & 0x7fff));
    }
}
