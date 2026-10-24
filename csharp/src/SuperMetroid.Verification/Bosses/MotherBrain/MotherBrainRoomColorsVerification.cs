using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.World;
namespace SuperMetroid.Verification.Bosses.MotherBrain;

/// <summary>Installed and edited Mother Brain room-color checks across entry, flash, phase two, and recovery-light CGRAM writes.</summary>
internal static class MotherBrainRoomColorsVerification
{
    /// <summary>Compares installed colors with extracted retail rows through production enemy paths, then checks isolated edits, legacy migration, and malformed overrides.</summary>
    internal static void VerifyMotherBrainRoomColors(ISnesAddressSpace rom, string stock,
        string overrides, AreaMapPresentationCatalog original,
        GameplayBasePaletteCatalog initialPalettes)
    {
        var nativeCgram = new SamusColourRig();
        var installedCgram = new SamusColourRig();
        MotherBrainRoomColorPresentation extracted = MotherBrainRoomColorPresentation.Load(
            new MemoryStream(SuperMetroid.AssetExtraction.Bosses.MotherBrain.MotherBrainRoomColorExtractor.Extract(rom),
                writable: false));
        Seed(nativeCgram);
        Seed(installedCgram);
        // Independently import the cartridge colors for the comparison path;
        // the gameplay palette runner has no runtime ROM fallback.
        var native = CreateEnemy(nativeCgram, extracted);
        var installed = CreateEnemy(installedCgram,
            original.MotherBrainRoomColors);
        native.MotherBrainBehavior.Body.LoadMotherBrainRoomEntryColors();
        installed.MotherBrainBehavior.Body.LoadMotherBrainRoomEntryColors();
        AssertTrue(new CgramPalettes(nativeCgram).ImageEquals(new CgramPalettes(installedCgram)),
            "installed Mother Brain room-entry glass and tube palettes match full native CGRAM");
        Suite(nameof(VerifyMotherBrainRecoveryLights), () => VerifyMotherBrainRecoveryLights(original.MotherBrainRoomColors, extracted));
        var nativeState = new MotherBrainEnemyState(native.SlotTable.Slots[0])
        {
            RoomFlashFrame = 0,
        };
        var installedState = new MotherBrainEnemyState(installed.SlotTable.Slots[0])
        {
            RoomFlashFrame = 0,
        };
        for (int frame = 0; frame < 48; frame++)
        {
            native.MotherBrainBehavior.RunMotherBrainRoomPalette(nativeState);
            installed.MotherBrainBehavior.RunMotherBrainRoomPalette(installedState);
            AssertTrue(new CgramPalettes(nativeCgram).ImageEquals(new CgramPalettes(installedCgram)),
                $"installed Mother Brain room flash matches full native CGRAM on frame {frame}");
            AssertEqual(nativeState.RoomFlashFrame,
                installedState.RoomFlashFrame,
                "room-flash presentation leaves the flash frame unchanged");
            AssertEqual(nativeState.RoomFlashTimer,
                installedState.RoomFlashTimer,
                "room-flash presentation leaves the flash timer unchanged");
        }
        native.MotherBrainBehavior.FakeDeath.StopMotherBrainRoomPalette(nativeState);
        installed.MotherBrainBehavior.FakeDeath.StopMotherBrainRoomPalette(installedState);
        AssertTrue(new CgramPalettes(nativeCgram).ImageEquals(new CgramPalettes(installedCgram)),
            "installed final grey room colors match all native CGRAM words");
        AssertEqual(nativeState.RoomFlashFrame,
            installedState.RoomFlashFrame,
            "final room palette preserves the flash stop state");

        Seed(nativeCgram);
        Seed(installedCgram);
        native.MotherBrainBehavior.Ascent.SetupMotherBrainPhaseTwoGraphics(nativeState);
        installed.MotherBrainBehavior.Ascent.SetupMotherBrainPhaseTwoGraphics(installedState);
        AssertTrue(new CgramPalettes(nativeCgram).ImageEquals(new CgramPalettes(installedCgram)),
            "installed phase-two attack/rear-leg colors match full native CGRAM");
        AssertEqual(nativeState.Function, installedState.Function,
            "phase-two colors do not alter the next body function");
        AssertEqual(nativeState.EnableUnpauseHook, installedState.EnableUnpauseHook,
            "phase-two colors do not alter the unpause hook");

        string replacement = Path.Combine(overrides, MotherBrainRoomColorFormat.FileName);
        MotherBrainRoomColorDocument document =
            JsonSerializer.Deserialize<MotherBrainRoomColorDocument>(
                File.ReadAllBytes(Path.Combine(stock, MotherBrainRoomColorFormat.FileName)),
                ArtworkJson.Options)
            ?? throw new InvalidDataException("Extracted Mother Brain room colors are null.");
        document.Flash[0][0] = Change(document.Flash[0][0]);
        document.FinalRoom[MotherBrainRoomColorRomData.SliceColors] =
            Change(document.FinalRoom[MotherBrainRoomColorRomData.SliceColors]);
        document.PhaseTwoAttack[0] = Change(document.PhaseTwoAttack[0]);
        document.PhaseTwoRearLeg[0] = Change(document.PhaseTwoRearLeg[0]);
        (document.InitialGlassShard ?? throw new InvalidOperationException("Verification requires document.InitialGlassShard."))[0] = Change(document.InitialGlassShard[0]);
        (document.InitialTubeProjectile ?? throw new InvalidOperationException("Verification requires document.InitialTubeProjectile."))[0] = Change(document.InitialTubeProjectile[0]);
        (document.RecoveryLights ?? throw new InvalidOperationException("Verification requires document.RecoveryLights."))[0][0] = Change(document.RecoveryLights[0][0]);
        document.RecoveryLights[0][MotherBrainRoomColorRomData.RecoveryLightsSliceColors] =
            Change(document.RecoveryLights[0][MotherBrainRoomColorRomData.RecoveryLightsSliceColors]);
        using (var file = File.Create(replacement))
            MotherBrainRoomColorPresentation.Write(file, document);
        AreaMapPresentationCatalog edited = AreaMapPresentationCatalog.Load(stock, overrides);
        AssertTrue(edited.ContentIdentity != original.ContentIdentity,
            "Mother Brain room-color override changes selected-content identity");
        var runtime = SuperMetroid.Core.Runtime.SuperMetroidRuntime.Create(new SuperMetroid.Core.Gameplay.HostGameplayOverrides(), initialPaletteArt: initialPalettes);
        runtime.Artwork.MapPresentation = edited;
        AssertTrue(ReferenceEquals(runtime.BossArtwork.MotherBrainRoomColors,
            edited.MotherBrainRoomColors),
            "installed Mother Brain room colors reach the production enemy binding");
        AssertEdited(nameof(MotherBrainBehavior.RunMotherBrainRoomPalette), behavior => behavior.RunMotherBrainRoomPalette, original, edited,
            MotherBrainRoomColorRomDataConstants.FirstColor, 1, flash: true);
        AssertEdited(nameof(MotherBrainFakeDeath.StopMotherBrainRoomPalette), behavior => behavior.FakeDeath.StopMotherBrainRoomPalette, original, edited,
            MotherBrainRoomColorRomDataConstants.SecondColor, 2);
        AssertEdited(nameof(MotherBrainAscent.SetupMotherBrainPhaseTwoGraphics), behavior => behavior.Ascent.SetupMotherBrainPhaseTwoGraphics, original, edited,
            MotherBrainRoomColorRomDataConstants.PhaseTwoAttackColor, 2);
        AssertRoomEntryEdited(original, edited);
        AssertRecoveryLightsEdited(original, edited);

        AssertThrows<InvalidDataException>(() => MotherBrainRoomColorPresentation.Load(
            new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(document with
            {
                Flash = document.Flash.Take(13).ToArray(),
            }, ArtworkJson.Options))), "reject truncated Mother Brain room-flash list");
        AssertThrows<InvalidDataException>(() => MotherBrainRoomColorPresentation.Load(
            new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(document with
            {
                PhaseTwoAttack = document.PhaseTwoAttack.Take(14).ToArray(),
            }, ArtworkJson.Options))), "reject truncated phase-two attack colors");
        AssertThrows<InvalidDataException>(() => MotherBrainRoomColorPresentation.Load(
            new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(document with
            {
                InitialGlassShard = document.InitialGlassShard.Take(14).ToArray(),
            }, ArtworkJson.Options))), "reject truncated room-entry glass colors");
        AssertThrows<InvalidDataException>(() => MotherBrainRoomColorPresentation.Load(
            new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(document with
            {
                RecoveryLights = document.RecoveryLights.Take(6).ToArray(),
            }, ArtworkJson.Options))), "reject truncated room-light recovery sequence");
        byte[] previousVersion = JsonSerializer.SerializeToUtf8Bytes(document with
        {
            Version = MotherBrainRoomColorFormatConstants.PreRecoveryLightsVersion,
            RecoveryLights = null,
        }, ArtworkJson.Options);
        File.WriteAllBytes(replacement, previousVersion);
        AreaMapPresentationCatalog migratedV2 = AreaMapPresentationCatalog.Load(stock, overrides);
        var migratedRecovery = new SamusColourRig();
        var originalRecovery = new SamusColourRig();
        migratedV2.MotherBrainRoomColors.ApplyRecoveryLights(migratedRecovery.Gameplay, 0);
        original.MotherBrainRoomColors.ApplyRecoveryLights(originalRecovery.Gameplay, 0);
        AssertTrue(new CgramPalettes(migratedRecovery).ImageEquals(new CgramPalettes(originalRecovery)),
            "version-two override inherits new recovery-light colors from stock");
        var migratedV2Entry = new SamusColourRig();
        var originalV2Entry = new SamusColourRig();
        migratedV2.MotherBrainRoomColors.ApplyRoomEntry(migratedV2Entry.Gameplay);
        original.MotherBrainRoomColors.ApplyRoomEntry(originalV2Entry.Gameplay);
        AssertTrue(!new CgramPalettes(migratedV2Entry).ImageEquals(new CgramPalettes(originalV2Entry)),
            "version-two override keeps its edited room-entry colors");
        byte[] legacy = JsonSerializer.SerializeToUtf8Bytes(document with
        {
            Version = MotherBrainRoomColorFormatConstants.PreRoomEntryVersion,
            InitialGlassShard = null,
            InitialTubeProjectile = null,
            RecoveryLights = null,
        }, ArtworkJson.Options);
        File.WriteAllBytes(replacement, legacy);
        AreaMapPresentationCatalog migrated = AreaMapPresentationCatalog.Load(stock, overrides);
        var migratedFlash = new SamusColourRig();
        var originalFlash = new SamusColourRig();
        migrated.MotherBrainRoomColors.ApplyFlash(migratedFlash.Gameplay, 0);
        original.MotherBrainRoomColors.ApplyFlash(originalFlash.Gameplay, 0);
        AssertTrue(!new CgramPalettes(migratedFlash).ImageEquals(new CgramPalettes(originalFlash)),
            "version-one room-color override retains its existing flash edit");
        var migratedEntry = new SamusColourRig();
        var originalEntry = new SamusColourRig();
        migrated.MotherBrainRoomColors.ApplyRoomEntry(migratedEntry.Gameplay);
        original.MotherBrainRoomColors.ApplyRoomEntry(originalEntry.Gameplay);
        AssertTrue(new CgramPalettes(migratedEntry).ImageEquals(new CgramPalettes(originalEntry)),
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

        // Change one red RGB5 channel while keeping it within the legal zero-to-31 range.
        static PaletteRgb5 Change(PaletteRgb5 color) => color with
        {
            Red = color.Red == 31 ? 30 : color.Red + 1,
        };
    }

    /// <summary>Compares all seven two-slice recovery transfers with retail CGRAM and rejects a nonnative source before copying colors.</summary>
    private static void VerifyMotherBrainRecoveryLights(MotherBrainRoomColorPresentation installed,
        MotherBrainRoomColorPresentation extracted)
    {
        var nativeCgram = new SamusColourRig();
        var installedCgram = new SamusColourRig();
        Seed(nativeCgram);
        Seed(installedCgram);
        var native = CreateEnemy(nativeCgram, extracted);
        var runtime = CreateEnemy(installedCgram, installed);
        for (ushort frame = 0; frame < MotherBrainRoomColorRomData.RecoveryLightsFrames; frame++)
        {
            native.BabyMetroidCutsceneBehavior.LoadMotherBrainRecoveryLights(frame);
            runtime.BabyMetroidCutsceneBehavior.LoadMotherBrainRecoveryLights(frame);
            AssertTrue(new CgramPalettes(nativeCgram).ImageEquals(new CgramPalettes(installedCgram)),
                $"installed Mother Brain recovery-light frame {frame} matches full native CGRAM");
        }
        try
        {
            runtime.BabyMetroidCutsceneBehavior.LoadMotherBrainRecoveryLights(MotherBrainRoomColorRomData.RecoveryLightsFrames);
            throw new InvalidOperationException("A Mother Brain recovery-light frame past the seventh was accepted.");
        }
        catch (InvalidDataException)
        {
            // A frame past the authored images is refused before touching installed colors.
        }
    }

    /// <summary>Checks edits in both recovery-light slices reach exactly two live CGRAM words.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    private static void AssertRecoveryLightsEdited(AreaMapPresentationCatalog original,
        AreaMapPresentationCatalog edited)
    {
        var stockCgram = new SamusColourRig();
        var editedCgram = new SamusColourRig();
        var stock = CreateEnemy(stockCgram,
            original.MotherBrainRoomColors);
        var replacement = CreateEnemy(editedCgram,
            edited.MotherBrainRoomColors);
        stock.BabyMetroidCutsceneBehavior.LoadMotherBrainRecoveryLights(0);
        replacement.BabyMetroidCutsceneBehavior.LoadMotherBrainRecoveryLights(0);
        AssertTrue(stockCgram.Colors[MotherBrainRoomColorRomData.RecoveryLightsFirstColor] !=
            editedCgram.Colors[MotherBrainRoomColorRomData.RecoveryLightsFirstColor],
            "first recovery-light slice edit reaches live CGRAM");
        AssertTrue(stockCgram.Colors[MotherBrainRoomColorRomData.RecoveryLightsSecondColor] !=
            editedCgram.Colors[MotherBrainRoomColorRomData.RecoveryLightsSecondColor],
            "second recovery-light slice edit reaches live CGRAM");
        AssertEqual(2, stockCgram.Colors.CountDifferences(editedCgram.Colors),
            "recovery-light edits change only two selected CGRAM words");
    }

    /// <summary>Checks room-entry glass and tube edits change exactly their two selected CGRAM words.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    private static void AssertRoomEntryEdited(AreaMapPresentationCatalog original,
        AreaMapPresentationCatalog edited)
    {
        var stockCgram = new SamusColourRig();
        var editedCgram = new SamusColourRig();
        var stockEnemy = CreateEnemy(stockCgram,
            original.MotherBrainRoomColors);
        var editedEnemy = CreateEnemy(editedCgram,
            edited.MotherBrainRoomColors);
        stockEnemy.MotherBrainBehavior.Body.LoadMotherBrainRoomEntryColors();
        editedEnemy.MotherBrainBehavior.Body.LoadMotherBrainRoomEntryColors();
        AssertTrue(stockCgram.Colors[MotherBrainRoomColorRomDataConstants.InitialGlassShardColor] !=
            editedCgram.Colors[MotherBrainRoomColorRomDataConstants.InitialGlassShardColor],
            "room-entry glass edit reaches the production color copy");
        AssertTrue(stockCgram.Colors[MotherBrainRoomColorRomDataConstants.InitialTubeProjectileColor] !=
            editedCgram.Colors[MotherBrainRoomColorRomDataConstants.InitialTubeProjectileColor],
            "room-entry tube edit reaches the production color copy");
        AssertEqual(2, stockCgram.Colors.CountDifferences(editedCgram.Colors),
            "room-entry edits change only the two selected CGRAM colors");
    }

    /// <summary>Checks a selected flash/final/phase-two color edit reaches CGRAM without changing body phase or palette-program timing.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    private static void AssertEdited(string methodName, Func<MotherBrainBehavior, Action<MotherBrainEnemyState>> select,
        AreaMapPresentationCatalog original,
        AreaMapPresentationCatalog edited, int expectedFirstColor, int expectedDifferences,
        bool flash = false)
    {
        var stockCgram = new SamusColourRig();
        var editedCgram = new SamusColourRig();
        var stockEnemy = CreateEnemy(stockCgram,
            original.MotherBrainRoomColors);
        var editedEnemy = CreateEnemy(editedCgram,
            edited.MotherBrainRoomColors);
        var stockState = new MotherBrainEnemyState(stockEnemy.SlotTable.Slots[0]);
        var editedState = new MotherBrainEnemyState(editedEnemy.SlotTable.Slots[0]);
        if (flash)
        {
            stockState.RoomFlashFrame = 0;
            editedState.RoomFlashFrame = 0;
        }
        select(stockEnemy.MotherBrainBehavior)(stockState);
        select(editedEnemy.MotherBrainBehavior)(editedState);
        AssertTrue(stockCgram.Colors[expectedFirstColor] !=
            editedCgram.Colors[expectedFirstColor],
            $"edited {methodName} color reaches live CGRAM");
        AssertEqual(expectedDifferences, stockCgram.Colors.CountDifferences(editedCgram.Colors),
            $"edited {methodName} changes only the selected CGRAM destinations");
        AssertEqual(stockState.Function, editedState.Function,
            $"edited {methodName} preserves body phase");
        AssertEqual(stockState.RoomFlashFrame,
            editedState.RoomFlashFrame,
            $"edited {methodName} preserves palette-program cursor");
        AssertEqual(stockState.RoomFlashTimer,
            editedState.RoomFlashTimer,
            $"edited {methodName} preserves palette-program timer");
    }

    /// <summary>Binds one installed room-color catalog and CGRAM destination to a fresh Mother Brain enemy system.</summary>
    private static RoomEnemySystem CreateEnemy(SamusColourRig cgram,
        MotherBrainRoomColorPresentation? colors)
    {
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance) { BossArtworkBinding = { MotherBrainRoomColors = colors } };
        enemies.Presentation.BindCgram(cgram.Gameplay);
        enemies.Presentation.BindVram(BgArtworkFixtures.For(new SnesVram()));
        return enemies;
    }

    /// <summary>Seeds every CGRAM word with a distinct bounded color so unintended palette writes remain observable.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    private static void Seed(SamusColourRig cgram)
    {
        for (int index = 0; index < SnesCgram.ColorCount; index++)
            cgram.SetColor(index, Bgr555.FromWord((ushort)(index * 71 & 0x7fff)));
    }
}
