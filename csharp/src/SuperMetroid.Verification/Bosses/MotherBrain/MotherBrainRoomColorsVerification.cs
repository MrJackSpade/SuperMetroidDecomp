using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.World;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Bosses;
using SuperMetroid.AssetExtraction.Bosses.MotherBrain;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;
using SuperMetroid.AssetExtraction;

namespace SuperMetroid.Verification.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's room colours (#1512): her encounter's production paths lay the room entry's, flash's, steady grey's,
/// phase two's and the recovering lights' shades, and the import pre-colours each from the installed colours, which
/// match the cartridge's; the colour codec rejects malformed documents.
/// </summary>
internal static class MotherBrainRoomColorsVerification
{
    /// <summary>Checks the shades the production paths lay, the import's colours of them against the extracted cartridge rows, and malformed documents.</summary>
    internal static void VerifyMotherBrainRoomColors(ISnesAddressSpace rom, string stock,
        AreaMapPresentation original)
    {
        ArgumentNullException.ThrowIfNull(original);
        MotherBrainRoomColorPresentation extracted = MotherBrainRoomColorFile.Read(
            new MemoryStream(SuperMetroid.AssetExtraction.Bosses.MotherBrain.MotherBrainRoomColorExtractor.Extract(rom),
                writable: false));
        // The installed colours' shades are the cartridge's.
        AssertSameShades(MotherBrainSheetShades.RoomSprites(extracted), MotherBrainSheetShades.RoomSprites(InstalledFixtures.ImportOf(original).MotherBrainRoomColors),
            "installed Mother Brain room-entry and phase-two sprite colours match the cartridge's");
        AssertSameShades(MotherBrainSheetShades.RoomBackground(extracted), MotherBrainSheetShades.RoomBackground(InstalledFixtures.ImportOf(original).MotherBrainRoomColors),
            "installed Mother Brain flash, grey and recovery-light colours match the cartridge's");

        RoomEnemySystem enemies = CreateEnemy();
        MotherBrainEnemyState state = StagedState(enemies);
        enemies.MotherBrainEncounter.MotherBrain = state;
        state.RoomFlashFrame = 0;
        for (int frame = 0; frame < 48; frame++)
        {
            MotherBrainBehavior.RunMotherBrainRoomFlash(state);
            int shown = state.RoomFlashFrame ?? throw new InvalidOperationException("The room flash stopped on its own.");
            AssertEqual(MotherBrainShades.Flash(shown), state.Colours.RoomShading.Shades[^1],
                $"Mother Brain's room flash shows its frame {shown} on update {frame}");
        }
        MotherBrainFakeDeath.StopMotherBrainRoomFlash(state);
        AssertEqual(MotherBrainShades.FinalRoom, state.Colours.RoomShading.Shades[^1], "stopping the flash shows the steady grey room");
        AssertEqual(null, state.RoomFlashFrame, "stopping the flash clears its frame");
        enemies.MotherBrainBehavior.Ascent.SetupMotherBrainPhaseTwoGraphics(state);
        AssertEqual(MotherBrainShades.PhaseTwo, state.Colours.OwnShading.Shades[^1], "the ascent loads the phase-two attack and rear-leg colours");
        Suite(nameof(VerifyMotherBrainRecoveryLights), () => VerifyMotherBrainRecoveryLights(enemies, state));

        MotherBrainRoomColorDocument document =
            JsonSerializer.Deserialize<MotherBrainRoomColorDocument>(
                File.ReadAllBytes(Path.Combine(stock, MotherBrainRoomColorFormat.FileName)),
                ArtworkJson.Options)
            ?? throw new InvalidDataException("Extracted Mother Brain room colors are null.");
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
                InitialGlassShard = (document.InitialGlassShard ?? throw new InvalidOperationException("Verification requires document.InitialGlassShard.")).Take(14).ToArray(),
            }, ArtworkJson.Options))), "reject truncated room-entry glass colors");
        AssertThrows<InvalidDataException>(() => MotherBrainRoomColorFile.Read(
            new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(document with
            {
                RecoveryLights = (document.RecoveryLights ?? throw new InvalidOperationException("Verification requires document.RecoveryLights.")).Take(6).ToArray(),
            }, ArtworkJson.Options))), "reject truncated room-light recovery sequence");
        Console.WriteLine("Mother Brain room colors: entry, 14 flash frames, final grey, phase-two and seven recovery-light shades match the cartridge's colours and the production paths that lay them; strict validation passes.");
    }

    /// <summary>Checks the seven recovery-light steps the Baby's cutscene lays, and that a step past the seventh is refused.</summary>
    private static void VerifyMotherBrainRecoveryLights(RoomEnemySystem enemies, MotherBrainEnemyState state)
    {
        for (int frame = 0; frame < MotherBrainRoomColorDefinitions.RecoveryLightsFrames; frame++)
        {
            enemies.BabyMetroidCutsceneBehavior.LoadMotherBrainRecoveryLights(frame);
            AssertEqual(MotherBrainShades.RecoveryLights(frame), state.Colours.RoomShading.Shades[^1],
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
    /// Mother Brain's state in <paramref name="enemies"/>' first slot, with the body her room's entry attaches, not shown
    /// until her phase-two transition reveals it (#1515).
    /// </summary>
    private static MotherBrainEnemyState StagedState(RoomEnemySystem enemies)
    {
        enemies.Presentation.AttachBossBody(BossBodyParts.Hidden(BossBodyOwner.MotherBrain));
        return new(enemies.SlotTable.Slots[0]);
    }

    /// <summary>A fresh Mother Brain enemy system over the installed body sheets.</summary>
    private static RoomEnemySystem CreateEnemy()
    {
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance) { BossArtworkBinding = { BodySheets = BossBodySheetFixtures.Stock.Sheets } };
        enemies.Presentation.BindBackgrounds(BgArtworkFixtures.For(new SnesVram()));
        return enemies;
    }
}
