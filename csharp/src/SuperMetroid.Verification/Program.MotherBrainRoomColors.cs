using System.Reflection;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>
    /// Compares extracted and installed Mother Brain palettes across room entry, recovery
    /// lights, flash, final-room colors, and phase two, then checks edited and legacy overrides.
    /// </summary>
    /// <param name="rom">Retail address space used for reference colors and runtime fixture setup.</param>
    /// <param name="stock">Directory containing the extracted stock map and color documents.</param>
    /// <param name="overrides">Directory where temporary edited or legacy color documents are written.</param>
    /// <param name="original">Stock map presentation catalog used to create the initial runtime binding.</param>
    /// <param name="initialPalettes">Installed base palettes passed to the runtime fixture.</param>
    private static void VerifyMotherBrainRoomColors(ISnesAddressSpace rom, string stock,
        string overrides, AreaMapPresentationCatalog original,
        GameplayBasePaletteCatalog initialPalettes)
    {
        var nativeCgram = new SnesCgram();
        var installedCgram = new SnesCgram();
        MotherBrainRoomColorPresentation extracted = MotherBrainRoomColorPresentation.Load(
            new MemoryStream(SuperMetroid.AssetExtraction.MotherBrainRoomColorExtractor.Extract(rom),
                writable: false));
        Seed(nativeCgram);
        Seed(installedCgram);
        // Independently import the cartridge colors for the comparison path;
        // the gameplay palette runner has no runtime ROM fallback.
        var native = CreateEnemy(rom, nativeCgram, extracted);
        var installed = CreateEnemy(new PaletteReadForbiddenBus(), installedCgram,
            original.MotherBrainRoomColors);
        Method("LoadMotherBrainRoomEntryColors").Invoke(native, null);
        Method("LoadMotherBrainRoomEntryColors").Invoke(installed, null);
        AssertTrue(nativeCgram.Colors.SequenceEqual(installedCgram.Colors),
            "installed Mother Brain room-entry glass and tube palettes match full native CGRAM");
        Suite(nameof(VerifyMotherBrainRecoveryLights), () => VerifyMotherBrainRecoveryLights(rom, original.MotherBrainRoomColors, extracted));
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
        document.RecoveryLights![0][0] = Change(document.RecoveryLights[0][0]);
        document.RecoveryLights[0][MotherBrainRoomColorRomData.RecoveryLightsColorsPerDestination] =
            Change(document.RecoveryLights[0][MotherBrainRoomColorRomData.RecoveryLightsColorsPerDestination]);
        using (var file = File.Create(replacement))
            MotherBrainRoomColorPresentation.Write(file, document);
        AreaMapPresentationCatalog edited = AreaMapPresentationCatalog.Load(stock, overrides);
        AssertTrue(edited.ContentIdentity != original.ContentIdentity,
            "Mother Brain room-color override changes selected-content identity");
        var runtime = new SuperMetroid.Core.Runtime.SuperMetroidRuntime(rom,
            initialPaletteArt: initialPalettes)
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
        AssertRecoveryLightsEdited(original, edited);

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
        AssertThrows<InvalidDataException>(() => MotherBrainRoomColorPresentation.Load(
            new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(document with
            {
                RecoveryLights = document.RecoveryLights!.Take(6).ToArray(),
            }, MapPresentationFormat.JsonOptions))), "reject truncated room-light recovery sequence");
        byte[] previousVersion = JsonSerializer.SerializeToUtf8Bytes(document with
        {
            Version = MotherBrainRoomColorFormat.PreRecoveryLightsVersion,
            RecoveryLights = null,
        }, MapPresentationFormat.JsonOptions);
        File.WriteAllBytes(replacement, previousVersion);
        AreaMapPresentationCatalog migratedV2 = AreaMapPresentationCatalog.Load(stock, overrides);
        var migratedRecovery = new SnesCgram();
        var originalRecovery = new SnesCgram();
        migratedV2.MotherBrainRoomColors.ApplyRecoveryLights(migratedRecovery, 0);
        original.MotherBrainRoomColors.ApplyRecoveryLights(originalRecovery, 0);
        AssertTrue(migratedRecovery.Colors.SequenceEqual(originalRecovery.Colors),
            "version-two override inherits new recovery-light colors from stock");
        var migratedV2Entry = new SnesCgram();
        var originalV2Entry = new SnesCgram();
        migratedV2.MotherBrainRoomColors.ApplyRoomEntry(migratedV2Entry);
        original.MotherBrainRoomColors.ApplyRoomEntry(originalV2Entry);
        AssertTrue(!migratedV2Entry.Colors.SequenceEqual(originalV2Entry.Colors),
            "version-two override keeps its edited room-entry colors");
        byte[] legacy = JsonSerializer.SerializeToUtf8Bytes(document with
        {
            Version = MotherBrainRoomColorFormat.PreRoomEntryVersion,
            InitialGlassShard = null,
            InitialTubeProjectile = null,
            RecoveryLights = null,
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
        Console.WriteLine("Mother Brain room colors: entry, 14 flash frames, final grey, phase-two and seven recovery-light palettes match native CGRAM; ROM-free production paths, isolated edits, legacy overrides and strict validation pass.");

        static PaletteRgb5 Change(PaletteRgb5 color) => color with
        {
            Red = color.Red == 31 ? 30 : color.Red + 1,
        };
    }

    /// <summary>Compares every recovery-light frame against cartridge colors and rejects malformed transfer requests.</summary>
    /// <param name="rom">Retail address space supplying native palette data.</param>
    /// <param name="installed">Installed room-color presentation being verified.</param>
    /// <param name="extracted">Presentation extracted from the same cartridge for comparison.</param>
    private static void VerifyMotherBrainRecoveryLights(ISnesAddressSpace rom,
        MotherBrainRoomColorPresentation installed,
        MotherBrainRoomColorPresentation extracted)
    {
        var nativeCgram = new SnesCgram();
        var installedCgram = new SnesCgram();
        Seed(nativeCgram);
        Seed(installedCgram);
        var native = CreateEnemy(rom, nativeCgram, extracted);
        var runtime = CreateEnemy(new PaletteReadForbiddenBus(), installedCgram, installed);
        MethodInfo apply = Method("LoadMotherBrainRecoveryLights");
        for (ushort frame = 0; frame < MotherBrainRoomColorRomData.RecoveryLightsFrames; frame++)
        {
            var request = RecoveryRequest(frame);
            apply.Invoke(native, [request]);
            apply.Invoke(runtime, [request]);
            AssertTrue(nativeCgram.Colors.SequenceEqual(installedCgram.Colors),
                $"installed Mother Brain recovery-light frame {frame} matches full native CGRAM");
        }
        var invalid = RecoveryRequest(0) with { SourceAddress = 0 };
        try
        {
            apply.Invoke(runtime, [invalid]);
            throw new InvalidOperationException("Malformed Mother Brain recovery-light source was accepted.");
        }
        catch (TargetInvocationException error) when (error.InnerException is InvalidDataException)
        {
            // Reject malformed requests before touching installed colors or the bus.
        }
    }

    /// <summary>Confirms edited recovery-light data changes only the two CGRAM destinations selected by one request.</summary>
    /// <param name="original">Stock catalog supplying the unedited recovery-light colors.</param>
    /// <param name="edited">Catalog containing the edited recovery-light colors.</param>
    private static void AssertRecoveryLightsEdited(AreaMapPresentationCatalog original,
        AreaMapPresentationCatalog edited)
    {
        var stockCgram = new SnesCgram();
        var editedCgram = new SnesCgram();
        var stock = CreateEnemy(new PaletteReadForbiddenBus(), stockCgram,
            original.MotherBrainRoomColors);
        var replacement = CreateEnemy(new PaletteReadForbiddenBus(), editedCgram,
            edited.MotherBrainRoomColors);
        MethodInfo apply = Method("LoadMotherBrainRecoveryLights");
        var request = RecoveryRequest(0);
        apply.Invoke(stock, [request]);
        apply.Invoke(replacement, [request]);
        AssertTrue(stockCgram.Colors[MotherBrainRoomColorRomData.RecoveryLightsFirstColor] !=
            editedCgram.Colors[MotherBrainRoomColorRomData.RecoveryLightsFirstColor],
            "first recovery-light slice edit reaches live CGRAM");
        AssertTrue(stockCgram.Colors[MotherBrainRoomColorRomData.RecoveryLightsSecondColor] !=
            editedCgram.Colors[MotherBrainRoomColorRomData.RecoveryLightsSecondColor],
            "second recovery-light slice edit reaches live CGRAM");
        AssertEqual(2, stockCgram.Colors.ToArray().Zip(editedCgram.Colors.ToArray())
            .Count(pair => pair.First != pair.Second),
            "recovery-light edits change only two selected CGRAM words");
    }

    /// <summary>Builds the transfer request for one authored recovery-light frame.</summary>
    /// <param name="frame">Zero-based frame within the recovery-light sequence.</param>
    /// <returns>A request with that frame's source address, two CGRAM destinations, and per-destination color count.</returns>
    private static MotherBrainBackgroundPaletteTransferRequest RecoveryRequest(ushort frame) =>
        new(frame, (uint)MotherBrainRoomColorRomData.RecoveryLightsSource(frame),
            (ushort)(MotherBrainRoomColorRomData.RecoveryLightsFirstColor * sizeof(ushort)),
            (ushort)(MotherBrainRoomColorRomData.RecoveryLightsSecondColor * sizeof(ushort)),
            MotherBrainRoomColorRomData.RecoveryLightsColorsPerDestination);

    /// <summary>Confirms room-entry overrides change the glass and tube colors and no other CGRAM entries.</summary>
    /// <param name="original">Stock catalog used for the reference room-entry palette.</param>
    /// <param name="edited">Catalog containing the replacement room-entry colors.</param>
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

    /// <summary>
    /// Invokes a room-palette operation with stock and edited catalogs, checking that only
    /// the expected colors change and that Mother Brain's control state remains identical.
    /// </summary>
    /// <param name="methodName">Private room-palette operation to invoke on each fixture.</param>
    /// <param name="original">Catalog supplying the stock room colors.</param>
    /// <param name="edited">Catalog supplying the replacement room colors.</param>
    /// <param name="expectedFirstColor">CGRAM index expected to differ after the operation.</param>
    /// <param name="expectedDifferences">Exact number of CGRAM entries allowed to differ.</param>
    /// <param name="flash">Whether to initialize both palette cursors at the flash program's start.</param>
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

    /// <summary>Creates an enemy-system fixture with its bus, CGRAM, VRAM, and optional room-color provider installed.</summary>
    /// <param name="bus">Address space used by the fixture's enemy logic.</param>
    /// <param name="cgram">Color memory to receive palette writes.</param>
    /// <param name="colors">Optional installed room-color presentation; null leaves the fixture on its native source path.</param>
    /// <returns>A room-enemy system wired to the supplied rendering and presentation state.</returns>
    private static RoomEnemySystem CreateEnemy(ISnesAddressSpace bus, SnesCgram cgram,
        MotherBrainRoomColorPresentation? colors)
    {
        var enemies = new RoomEnemySystem { MotherBrainRoomColors = colors };
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, bus);
        typeof(RoomEnemySystem).GetField("_cgram", flags)!.SetValue(enemies, cgram);
        typeof(RoomEnemySystem).GetField("_vram", flags)!.SetValue(enemies, new SnesVram());
        return enemies;
    }

    /// <summary>Finds a nonpublic instance method on the room-enemy system for a focused reflection fixture.</summary>
    /// <param name="name">Exact method name to resolve.</param>
    /// <returns>The matching method metadata.</returns>
    private static MethodInfo Method(string name) => typeof(RoomEnemySystem).GetMethod(name,
        BindingFlags.Instance | BindingFlags.NonPublic)!;

    /// <summary>Fills every CGRAM entry with a repeatable nonuniform value before palette comparisons.</summary>
    /// <param name="cgram">Color memory to seed.</param>
    private static void Seed(SnesCgram cgram)
    {
        for (int index = 0; index < SnesCgram.ColorCount; index++)
            cgram.SetColor(index, (ushort)(index * 71 & 0x7fff));
    }
}
