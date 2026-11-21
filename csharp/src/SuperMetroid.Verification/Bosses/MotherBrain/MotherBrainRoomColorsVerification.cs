using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Frontend.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.World;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Bosses;
using SuperMetroid.AssetExtraction.Bosses.MotherBrain;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Verification.Harness;

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
        InstalledScreenContent original)
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
        enemies.MotherBrainEncounter.BeginAsSetUp(state);
        state.RoomFlash = MotherBrainRoomFlashAnimations.Starting();
        for (int frame = 0; frame < 48; frame++)
        {
            MotherBrainBehavior.RunMotherBrainRoomFlash(state);
            // The flash shows its first image on its first update alone, then each image for two updates, looping (#2404).
            MotherBrainRoomFlashImage shown = MotherBrainRoomFlashImage.All[(frame + 1) / 2 % MotherBrainRoomFlashDefinitions.FlashFrameCount];
            AssertEqual(MotherBrainShades.Flash(shown), state.Colours.RoomShading.Shades[^1],
                $"Mother Brain's room flash shows its image {shown.Step} on update {frame}");
        }
        MotherBrainFakeDeath.StopMotherBrainRoomFlash(state);
        AssertEqual(MotherBrainShades.FinalRoom, state.Colours.RoomShading.Shades[^1], "stopping the flash shows the steady grey room");
        AssertTrue(state.RoomFlash is null, "stopping the flash stops it playing");
        MotherBrainAscent.SetupMotherBrainPhaseTwoGraphics(state);
        AssertEqual(MotherBrainShades.PhaseTwo, state.Colours.OwnShading.Shades[^1], "the ascent loads the phase-two attack and rear-leg colours");
        Suite(nameof(VerifyMotherBrainRecoveryLights), () => VerifyMotherBrainRecoveryLights(state));

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

    /// <summary>Checks the seven recovery-light images the Baby's cutscene lays over her room.</summary>
    private static void VerifyMotherBrainRecoveryLights(MotherBrainEnemyState state)
    {
        foreach (MotherBrainFadeImage lights in MotherBrainFadeDefinitions.RecoveryLights.Images)
        {
            state.Colours.ShowFade(lights);
            AssertEqual(MotherBrainShades.Fade(lights), state.Colours.RoomShading.Shades[^1],
                $"Mother Brain recovery-light image {lights.Step} is laid over her room");
        }
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
        return a.InkColours.Count(entry => b.InkColours[entry.Key] != entry.Value);
    }

    /// <summary>
    /// Mother Brain's state in <paramref name="enemies"/>' first slot, with the body her room's entry attaches, not shown
    /// until her phase-two transition reveals it (#1515).
    /// </summary>
    private static MotherBrainEnemyState StagedState(RoomEnemySystem enemies)
    {
        enemies.Presentation.AttachBossBody(BossBodyPartsConstants.Hidden(BossBodyOwner.MotherBrain));
        return new(enemies.Places.InUpdateOrder[0].BoundFor(EnemyDefinitionId.MotherBrainBody), enemies.Places.InUpdateOrder[1], new MotherBrainWalkCounter(), new MotherBrainBurstCycle(), new MotherBrainBrainAnimation(), RoomWarningFixtures.Zebes());
    }

    /// <summary>A fresh Mother Brain enemy system over the installed body sheets.</summary>
    private static RoomEnemySystem CreateEnemy()
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        enemies.Presentation.BindBackgrounds(BgArtworkFixtures.For(new SnesVram()));
        return enemies;
    }
}
