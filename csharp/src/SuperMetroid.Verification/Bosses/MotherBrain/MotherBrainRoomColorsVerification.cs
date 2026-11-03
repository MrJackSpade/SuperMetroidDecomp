using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.World;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Bosses;
using SuperMetroid.AssetExtraction.Bosses.MotherBrain;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;
using SuperMetroid.AssetExtraction;

namespace SuperMetroid.Verification.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's room colours (#1512): her encounter's production paths lay the room entry's, flash's, steady grey's,
/// phase two's and the recovering lights' shades, and the import pre-colours each from the installed colours, which
/// match the cartridge's, take an override's isolated edits, migrate older overrides and reject malformed ones.
/// </summary>
internal static class MotherBrainRoomColorsVerification
{
    /// <summary>Checks the shades the production paths lay, the import's colours of them against the extracted cartridge rows, isolated edits, legacy migration and malformed overrides.</summary>
    internal static void VerifyMotherBrainRoomColors(ISnesAddressSpace rom, string stock,
        string overrides, AreaMapPresentation original)
    {
        ArgumentNullException.ThrowIfNull(original);
        MotherBrainRoomColorPresentation extracted = MotherBrainRoomColorFile.Read(
            new MemoryStream(SuperMetroid.AssetExtraction.Bosses.MotherBrain.MotherBrainRoomColorExtractor.Extract(rom),
                writable: false));
        // The installed colours' shades are the cartridge's.
        AssertSameShades(MotherBrainSheetShades.RoomSprites(extracted), MotherBrainSheetShades.RoomSprites(original.MotherBrainRoomColors),
            "installed Mother Brain room-entry and phase-two sprite colours match the cartridge's");
        AssertSameShades(MotherBrainSheetShades.RoomBackground(extracted), MotherBrainSheetShades.RoomBackground(original.MotherBrainRoomColors),
            "installed Mother Brain flash, grey and recovery-light colours match the cartridge's");

        RoomEnemySystem enemies = CreateEnemy();
        MotherBrainEnemyState state = StagedState(enemies);
        enemies.MotherBrainEncounter.MotherBrain = state;
        state.RoomFlashFrame = 0;
        for (int frame = 0; frame < 48; frame++)
        {
            enemies.MotherBrainBehavior.RunMotherBrainRoomPalette(state);
            int shown = state.RoomFlashFrame ?? throw new InvalidOperationException("The room flash stopped on its own.");
            AssertEqual(MotherBrainShades.Flash(shown), state.Colours.Background.Laid[^1],
                $"Mother Brain's room flash shows its frame {shown} on update {frame}");
        }
        enemies.MotherBrainBehavior.FakeDeath.StopMotherBrainRoomPalette(state);
        AssertEqual(MotherBrainShades.FinalRoom, state.Colours.Background.Laid[^1], "stopping the flash shows the steady grey room");
        AssertEqual(null, state.RoomFlashFrame, "stopping the flash clears its frame");
        enemies.MotherBrainBehavior.Ascent.SetupMotherBrainPhaseTwoGraphics(state);
        AssertEqual(MotherBrainShades.PhaseTwo, state.Colours.Sprites.Laid[^1], "the ascent loads the phase-two attack and rear-leg colours");
        Suite(nameof(VerifyMotherBrainRecoveryLights), () => VerifyMotherBrainRecoveryLights(enemies, state));

        string replacement = Path.Combine(overrides, MotherBrainRoomColorFormat.FileName);
        MotherBrainRoomColorDocument document =
            JsonSerializer.Deserialize<MotherBrainRoomColorDocument>(
                File.ReadAllBytes(Path.Combine(stock, MotherBrainRoomColorFormat.FileName)),
                ArtworkJson.Options)
            ?? throw new InvalidDataException("Extracted Mother Brain room colors are null.");
        document.Flash[0][0] = Change(document.Flash[0][0]);
        document.FinalRoom[MotherBrainRoomColorDefinitions.SliceColors] =
            Change(document.FinalRoom[MotherBrainRoomColorDefinitions.SliceColors]);
        document.PhaseTwoAttack[0] = Change(document.PhaseTwoAttack[0]);
        document.PhaseTwoRearLeg[0] = Change(document.PhaseTwoRearLeg[0]);
        (document.InitialGlassShard ?? throw new InvalidOperationException("Verification requires document.InitialGlassShard."))[0] = Change(document.InitialGlassShard[0]);
        (document.InitialTubeProjectile ?? throw new InvalidOperationException("Verification requires document.InitialTubeProjectile."))[0] = Change(document.InitialTubeProjectile[0]);
        (document.RecoveryLights ?? throw new InvalidOperationException("Verification requires document.RecoveryLights."))[0][0] = Change(document.RecoveryLights[0][0]);
        document.RecoveryLights[0][MotherBrainRoomColorDefinitions.RecoveryLightsSliceColors] =
            Change(document.RecoveryLights[0][MotherBrainRoomColorDefinitions.RecoveryLightsSliceColors]);
        using (var file = File.Create(replacement))
            MotherBrainRoomColorFile.Write(file, document);
        AreaMapPresentation edited = MapPresentationFiles.Load(stock, overrides);
        AssertTrue(edited.ContentIdentity != original.ContentIdentity,
            "Mother Brain room-color override changes selected-content identity");
        IReadOnlyList<SheetShade> stockSprites = MotherBrainSheetShades.RoomSprites(original.MotherBrainRoomColors);
        IReadOnlyList<SheetShade> editedSprites = MotherBrainSheetShades.RoomSprites(edited.MotherBrainRoomColors);
        IReadOnlyList<SheetShade> stockBackground = MotherBrainSheetShades.RoomBackground(original.MotherBrainRoomColors);
        IReadOnlyList<SheetShade> editedBackground = MotherBrainSheetShades.RoomBackground(edited.MotherBrainRoomColors);
        // The flash's first colour, the grey's second slice's first colour and its mirror, both phase-two colours, both
        // room-entry colours and both recovery-light slices' first colours change; nothing else does.
        AssertEqual(1, Differences(stockBackground, editedBackground, MotherBrainShades.Flash(0)), "the flash edit changes one colour");
        AssertEqual(2, Differences(stockBackground, editedBackground, MotherBrainShades.FinalRoom), "the grey edit changes its colour and its mirror");
        AssertEqual(2, Differences(stockSprites, editedSprites, MotherBrainShades.PhaseTwo), "the phase-two edits change their two colours");
        AssertEqual(2, Differences(stockSprites, editedSprites, MotherBrainShades.RoomEntry), "the room-entry edits change their two colours");
        AssertEqual(2, Differences(stockBackground, editedBackground, MotherBrainShades.RecoveryLights(0)), "the recovery-light edits change their two colours");
        AssertEqual(0, Differences(stockBackground, editedBackground, MotherBrainShades.Flash(1)), "the flash edit leaves its other frames alone");

        AssertThrows<InvalidDataException>(() => MotherBrainRoomColorFile.Read(
            new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(document with
            {
                Flash = document.Flash.Take(13).ToArray(),
            }, ArtworkJson.Options))), "reject truncated Mother Brain room-flash list");
        AssertThrows<InvalidDataException>(() => MotherBrainRoomColorFile.Read(
            new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(document with
            {
                PhaseTwoAttack = document.PhaseTwoAttack.Take(14).ToArray(),
            }, ArtworkJson.Options))), "reject truncated phase-two attack colors");
        AssertThrows<InvalidDataException>(() => MotherBrainRoomColorFile.Read(
            new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(document with
            {
                InitialGlassShard = document.InitialGlassShard.Take(14).ToArray(),
            }, ArtworkJson.Options))), "reject truncated room-entry glass colors");
        AssertThrows<InvalidDataException>(() => MotherBrainRoomColorFile.Read(
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
        AreaMapPresentation migratedV2 = MapPresentationFiles.Load(stock, overrides);
        AssertEqual(0, Differences(stockBackground, MotherBrainSheetShades.RoomBackground(migratedV2.MotherBrainRoomColors), MotherBrainShades.RecoveryLights(0)),
            "version-two override inherits new recovery-light colors from stock");
        AssertTrue(Differences(stockSprites, MotherBrainSheetShades.RoomSprites(migratedV2.MotherBrainRoomColors), MotherBrainShades.RoomEntry) != 0,
            "version-two override keeps its edited room-entry colors");
        byte[] legacy = JsonSerializer.SerializeToUtf8Bytes(document with
        {
            Version = MotherBrainRoomColorFormatConstants.PreRoomEntryVersion,
            InitialGlassShard = null,
            InitialTubeProjectile = null,
            RecoveryLights = null,
        }, ArtworkJson.Options);
        File.WriteAllBytes(replacement, legacy);
        AreaMapPresentation migrated = MapPresentationFiles.Load(stock, overrides);
        AssertTrue(Differences(stockBackground, MotherBrainSheetShades.RoomBackground(migrated.MotherBrainRoomColors), MotherBrainShades.Flash(0)) != 0,
            "version-one room-color override retains its existing flash edit");
        AssertEqual(0, Differences(stockSprites, MotherBrainSheetShades.RoomSprites(migrated.MotherBrainRoomColors), MotherBrainShades.RoomEntry),
            "version-one room-color override inherits only the new room-entry colors from verified stock");
        File.WriteAllText(replacement, "{ broken JSON");
        AssertThrows<InvalidDataException>(() => MapPresentationFiles.Load(stock, overrides),
            "corrupt Mother Brain room override fails loudly");
        AssertEqual("{ broken JSON", File.ReadAllText(replacement),
            "corrupt Mother Brain override remains available for repair");
        File.Delete(replacement);
        AssertEqual(original.ContentIdentity, MapPresentationFiles.Load(stock, overrides)
            .ContentIdentity, "removing Mother Brain room override restores stock identity");
        Console.WriteLine("Mother Brain room colors: entry, 14 flash frames, final grey, phase-two and seven recovery-light shades match the cartridge's colours and the production paths that lay them; isolated edits, legacy overrides and strict validation pass.");

        // Change one red RGB5 channel while keeping it within the legal zero-to-31 range.
        static PaletteRgb5 Change(PaletteRgb5 color) => color with
        {
            Red = color.Red == 31 ? 30 : color.Red + 1,
        };
    }

    /// <summary>Checks the seven recovery-light steps the Baby's cutscene lays, and that a step past the seventh is refused.</summary>
    private static void VerifyMotherBrainRecoveryLights(RoomEnemySystem enemies, MotherBrainEnemyState state)
    {
        for (int frame = 0; frame < MotherBrainRoomColorDefinitions.RecoveryLightsFrames; frame++)
        {
            enemies.BabyMetroidCutsceneBehavior.LoadMotherBrainRecoveryLights(frame);
            AssertEqual(MotherBrainShades.RecoveryLights(frame), state.Colours.Background.Laid[^1],
                $"Mother Brain recovery-light frame {frame} is laid over her room");
        }
        AssertThrows<InvalidDataException>(
            () => enemies.BabyMetroidCutsceneBehavior.LoadMotherBrainRecoveryLights(MotherBrainRoomColorDefinitions.RecoveryLightsFrames),
            "a Mother Brain recovery-light frame past the seventh is refused");
    }

    /// <summary>Asserts that two lists of the import's shades hold the same shades with the same colours.</summary>
    private static void AssertSameShades(IReadOnlyList<SheetShade> expected, IReadOnlyList<SheetShade> actual, string label)
    {
        AssertEqual(expected.Count, actual.Count, $"{label}: shade count");
        foreach (SheetShade shade in expected)
            AssertEqual(0, Differences(expected, actual, shade.Shade), $"{label}: {shade.Shade.Key} step {shade.Shade.Step}");
    }

    /// <summary>The number of inks whose colour differs between <paramref name="shade"/> among <paramref name="first"/> and among <paramref name="second"/>.</summary>
    private static int Differences(IReadOnlyList<SheetShade> first, IReadOnlyList<SheetShade> second, PictureShade shade)
    {
        SheetShade a = first.Single(candidate => candidate.Shade == shade);
        SheetShade b = second.Single(candidate => candidate.Shade == shade);
        AssertTrue(a.Inks.SequenceEqual(b.Inks), $"shade {shade.Key} recolours the same inks");
        return a.Colours.Zip(b.Colours).Count(pair => pair.First != pair.Second);
    }

    /// <summary>
    /// Mother Brain's state in <paramref name="enemies"/>' first slot, her body staged on a cleared canvas over the installed
    /// sheets as her room's entry stages it, which her phase-two transition shows.
    /// </summary>
    private static MotherBrainEnemyState StagedState(RoomEnemySystem enemies) =>
        new(enemies.SlotTable.Slots[0])
        {
            StagedBody = new BossBodyCanvas(BossBodySheetFixtures.Stock.Sheets.Of(BossBodyOwner.MotherBrain), BossBodyDefinitions.SharedBodyRows),
        };

    /// <summary>A fresh Mother Brain enemy system over the installed body sheets.</summary>
    private static RoomEnemySystem CreateEnemy()
    {
        var enemies = new RoomEnemySystem(new EnemySession(new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()), definitions: CompiledRoomEnemyDefinitions.Instance) { BossArtworkBinding = { BodySheets = BossBodySheetFixtures.Stock.Sheets } };
        enemies.Presentation.BindBackgrounds(BgArtworkFixtures.For(new SnesVram()));
        return enemies;
    }
}
