using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.Tooling;
using System.Text.Json;
using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using static SuperMetroid.Verification.Hardware.ColorWordsVerification;
using static SuperMetroid.Verification.Bosses.Kraid.KraidPaletteDefinitionsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Core.World;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Bosses;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.AssetExtraction.Bosses.Kraid.Artwork;
using SuperMetroid.AssetExtraction.Bosses.Kraid;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.Kraid.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>Checks installed Kraid RGB5 rows through the palette consumers that write CGRAM.</summary>
internal static class EnemyTileArtworkKraidColorsVerification
{
    /// <summary>Compares every stock color with ROM, checks guarded production writes, and rejects out-of-range channels.</summary>
    internal static void VerifyInstalledKraidColors(
        SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace rom, string directory, ImportEnemyArtwork stock)
    {
        Suite(nameof(VerifyKraidPaletteSourceAddresses), () => VerifyKraidPaletteSourceAddresses(rom));
        Suite(nameof(VerifyKraidPaletteSourceLengths), () => VerifyKraidPaletteSourceLengths());
        Suite(nameof(VerifyKraidBackdropSurvivesReinitialization), () => VerifyKraidBackdropSurvivesReinitialization(stock));
        Suite(nameof(VerifyKraidDeathFadeFromRoomColours), () => VerifyKraidDeathFadeFromRoomColours(stock));
        Suite(nameof(VerifyKraidFadeBackFromKraidColours), () => VerifyKraidFadeBackFromKraidColours(stock));
        Suite(nameof(VerifyKraidDeathFadeDuringGrowthFade), () => VerifyKraidDeathFadeDuringGrowthFade(stock));
        Suite(nameof(VerifyKraidFadeBackDuringDoorFade), () => VerifyKraidFadeBackDuringDoorFade(stock));
        Suite(nameof(VerifyPictureShadeFadeColours), VerifyPictureShadeFadeColours);
        Suite(nameof(VerifyKraidHealthBands), VerifyKraidHealthBands);
        KraidColorCatalog catalog = stock.Bosses.KraidColors
            ?? throw new InvalidDataException("Installed enemy artwork lacks Kraid colors.");
        var guardForAllColors = NewKraidPaletteSourceGuard(rom);
        foreach (KraidPaletteSource source in Enum.GetValues<KraidPaletteSource>())
        {
            int count = KraidPalettes.ColorCount(source);
            byte[] native = RomDataReader.ReadFixedBank(rom,
                KraidPaletteSources.SourceAddress(source), count * sizeof(ushort));
            for (int index = 0; index < count; index++)
                AssertEqual(LittleEndianBytes.Word(native[index * 2], native[index * 2 + 1]),
                    catalog.Resolve(source, index),
                    $"Kraid {source} stock RGB5 color {index}");
            AssertThrows<ArgumentOutOfRangeException>(() => catalog.Resolve(source, count),
                $"Kraid {source} rejects adjacent ROM words");
        }
        // Gameplay keeps only Kraid's eye colours (#1512): the hurt flash's and each health band's, which his eye glow
        // reads back. They are the installed health colours at the eyes' inks.
        var eyeReaderOwner = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(stock.Enemies), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, installedBosses: stock.Bosses);
        KraidEyeColours eyes = eyeReaderOwner.KraidBehavior.Background.EyeColours;
        int GroupOf(PictureShade shade) => KraidPalettes.HealthSequence.Single(band => band.Shade == shade).ColourGroup;
        AssertEyes(catalog, eyes.Hurt.ToArray(), GroupOf(KraidShades.HurtFlash), "the hurt flash");
        foreach (KraidHealthBand band in KraidHealthBandDefinitions.HighestFirst)
            AssertEyes(catalog, eyes.Of(band).ToArray(), GroupOf(KraidShades.Health(band)), $"band {band}");
        // The game reads the eye colours from their own installed document by purpose (#1637), and no other Kraid colour.
        SuperMetroid.AssetExtraction.Installation.GameInstallation installation = SuperMetroid.TestSupport.RepositoryInstallation.RepositoryInstallation.Installation;
        AssertTrue(!File.Exists(Path.Combine(installation.EnemyTileDirectory, KraidColorFormat.FileName)),
            "the installation holds none of Kraid's colours but his eyes'");
        KraidEyeColours installedEyes = installation.LoadEnemyTiles().KraidEyes;
        AssertEqual(KraidPalettes.ColorCount(KraidPaletteSource.Health) / KraidPalettes.BandColors, KraidPalettes.HealthSequence.Count,
            "his health colours are the hurt flash's and the eight health bands'");
        AssertEqual(0, GroupOf(KraidShades.HurtFlash), "the hurt flash's colours come first among his health colours");
        AssertTrue(installedEyes.Hurt.ToArray().SequenceEqual(eyes.Hurt.ToArray()), "Kraid's installed hurt flash eye colours are the eyes' colours of his health colours");
        foreach (KraidHealthBand band in KraidHealthBandDefinitions.HighestFirst)
        {
            AssertTrue(KraidPalettes.HealthSequence.Any(sequenced => sequenced.Shade == KraidShades.Health(band)), $"Kraid band {band}'s shade is among his health colours");
            AssertTrue(installedEyes.Of(band).ToArray().SequenceEqual(eyes.Of(band).ToArray()),
                $"Kraid band {band}'s installed eye colours are the eyes' colours of his health colours");
        }
        AssertEqual(0, guardForAllColors.ForbiddenReadAttempts,
            "all installed Kraid palette rows avoid their ROM sources");

        foreach (KraidPaletteConsumer consumer in Enum.GetValues<KraidPaletteConsumer>())
        {
            ushort[] expected = ReferenceKraidPaletteConsumer(rom, consumer);
            var guard = NewKraidPaletteSourceGuard(rom);
            RgbColor[] actual = CaptureKraidPaletteConsumer(stock, consumer);
            AssertEqual(0, guard.ForbiddenReadAttempts,
                $"installed Kraid {consumer} avoids all palette ROM sources");
            AssertTrue(actual.SequenceEqual(ToColors(expected)),
                $"installed Kraid {consumer} preserves full native CGRAM state");
        }

        KraidColorDocument document = RequiredJson.Deserialize<KraidColorDocument>(
            File.ReadAllBytes(Path.Combine(directory, KraidColorFormat.FileName)), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        PaletteRgb5[] invalidHealth = (PaletteRgb5[])document.Health.Clone();
        invalidHealth[0] = invalidHealth[0] with { Red = 32 };
        AssertThrows<InvalidDataException>(() => KraidColorFile.Read(new MemoryStream(KraidColorFile.Write(
                document with { Health = invalidHealth }), writable: false)),
            "reading Kraid colors rejects RGB5 channels outside native precision");
    }

    /// <summary>
    /// Kraid's backdrop keeps what his growth fade showed when Kraid is initialized again, which replaces his encounter
    /// state, as the cartridge's colour memory keeps it; his death fade then starts from his colours instead of failing
    /// on the room's (#1532).
    /// </summary>
    private static void VerifyKraidBackdropSurvivesReinitialization(ImportEnemyArtwork artwork)
    {
        RoomEnemySystem enemies = NewKraidRoom(artwork);
        var fade = new Func<KraidBackdropSettledColours, bool>(enemies.KraidBehavior.Background.AdvanceKraidRoomBackgroundFade);
        int growthSteps = 0;
        while (!fade(KraidBackdropSettledColours.KraidColours))
            AssertTrue(++growthSteps <= FadeCounterValues, "Kraid's growth fade completes once the counter passes its last moving value");
        AssertEqual(SettledBackdrop(KraidBackdropSettledColours.KraidColours), enemies.KraidBehavior.Access.Backdrop.Shows, "Kraid's growth fade ends on his colours");
        // Kraid's body initializer replaces his encounter state; the room's backdrop colours outlive it.
        _ = enemies.KraidBehavior.BeginEncounterInRoomOrder(enemies.Places);
        AssertEqual(SettledBackdrop(KraidBackdropSettledColours.KraidColours), enemies.KraidBehavior.Access.Backdrop.Shows,
            "re-initializing Kraid keeps the backdrop his growth fade showed");
        int deathSteps = 0;
        while (!fade(KraidBackdropSettledColours.Black))
            AssertTrue(++deathSteps <= FadeCounterValues, "Kraid's death fade completes once the counter passes its last moving value");
        AssertEqual(SettledBackdrop(KraidBackdropSettledColours.Black), enemies.KraidBehavior.Access.Backdrop.Shows, "Kraid's death fade ends on black");
    }

    /// <summary>
    /// Kraid can die before his growth fades the backdrop in (#1532): his death fade then fades the room's own colours to
    /// black (#2215), each step publishing a fade from the room's standard colouring toward the black shade, the counter's
    /// moving values its parts, and it ends on black.
    /// </summary>
    private static void VerifyKraidDeathFadeFromRoomColours(ImportEnemyArtwork artwork)
    {
        RoomEnemySystem enemies = NewKraidRoom(artwork);
        StepKraidBackdropFade(enemies, KraidBackdropSettledColours.Black, PictureShading.Standard, PictureShading.Shaded(KraidShades.BackdropBlack),
            untilCounter: null, "Kraid's death fade from the room's colours");
        AssertEqual(SettledBackdrop(KraidBackdropSettledColours.Black), enemies.KraidBehavior.Access.Backdrop.Shows,
            "Kraid's death fade from the room's colours ends on black");
    }

    /// <summary>
    /// A completed fade shows its goal (#2215): his growth fade steps through its last value, he dies before it completes,
    /// so his death fade completes without a step, on black; the death zeroes the counter and the fade back steps from
    /// black into his colours and ends on them. His defeated room, which shows his colours at once, takes a fade back that
    /// changes nothing.
    /// </summary>
    private static void VerifyKraidFadeBackFromKraidColours(ImportEnemyArtwork artwork)
    {
        RoomEnemySystem enemies = NewKraidRoom(artwork);
        StepKraidBackdropFade(enemies, KraidBackdropSettledColours.KraidColours, PictureShading.Standard, PictureShading.Shaded(KraidShades.Backdrop),
            untilCounter: KraidBackgroundDefinitions.BackdropFadeLastStep, "Kraid's growth fade");
        AssertEqual(SettledBackdrop(KraidBackdropSettledColours.KraidColours), enemies.KraidBehavior.Access.Backdrop.Shows,
            "Kraid's growth fade's last moving step lands on his colours");
        var fade = new Func<KraidBackdropSettledColours, bool>(enemies.KraidBehavior.Background.AdvanceKraidRoomBackgroundFade);
        AssertTrue(fade(KraidBackdropSettledColours.Black), "Kraid's death fade after his growth fade's last step completes without a step");
        AssertEqual(SettledBackdrop(KraidBackdropSettledColours.Black), enemies.KraidBehavior.Access.Backdrop.Shows,
            "a death fade that completes without a step shows its goal");
        // The death zeroes the shared counter once his body has sunk, before the fade back.
        enemies.Presentation.FadeStepCount.Reset();
        StepKraidBackdropFade(enemies, KraidBackdropSettledColours.KraidColours, PictureShading.Shaded(KraidShades.BackdropBlack),
            PictureShading.Shaded(KraidShades.Backdrop), untilCounter: null, "Kraid's fade back from black");
        AssertEqual(SettledBackdrop(KraidBackdropSettledColours.KraidColours), enemies.KraidBehavior.Access.Backdrop.Shows,
            "Kraid's fade back ends on his colours");

        RoomEnemySystem defeated = NewKraidRoom(artwork);
        defeated.KraidBehavior.Access.Backdrop.ShowKraidColours();
        var defeatedFade = new Func<KraidBackdropSettledColours, bool>(defeated.KraidBehavior.Background.AdvanceKraidRoomBackgroundFade);
        while (!defeatedFade(KraidBackdropSettledColours.KraidColours))
        {
            AssertEqual(SettledBackdrop(KraidBackdropSettledColours.KraidColours), defeated.KraidBehavior.Access.Backdrop.Shows,
                "Kraid's defeated room's fade back into his colours changes nothing");
        }
    }

    /// <summary>
    /// Kraid can die during his growth fade (#1512): his death fade then starts from the growth fade partway and continues
    /// its counter, so it completes sooner (#2215), its colours moving by its own steps until then (#2302), and ends on
    /// black.
    /// </summary>
    private static void VerifyKraidDeathFadeDuringGrowthFade(ImportEnemyArtwork artwork)
    {
        RoomEnemySystem enemies = NewKraidRoom(artwork);
        StepKraidBackdropFade(enemies, KraidBackdropSettledColours.KraidColours, PictureShading.Standard, PictureShading.Shaded(KraidShades.Backdrop),
            untilCounter: 5, "Kraid's growth fade");
        AssertTrue(enemies.KraidBehavior.Access.Backdrop.Shows is KraidBackdropShown.Fading,
            "Kraid's growth fade is partway when he dies");
        PictureShading growthPartway = PictureShading.Standard.With(
            new PictureShadeFade(PictureShading.Standard, PictureShading.Shaded(KraidShades.Backdrop), new FadeLevel(5, KraidBackgroundDefinitions.BackdropFadeLastStep)));
        StepKraidBackdropFade(enemies, KraidBackdropSettledColours.Black, growthPartway, PictureShading.Shaded(KraidShades.BackdropBlack),
            untilCounter: null, "Kraid's death fade during his growth fade");
        AssertEqual(SettledBackdrop(KraidBackdropSettledColours.Black), enemies.KraidBehavior.Access.Backdrop.Shows,
            "Kraid's death fade during his growth fade ends on black");
    }

    /// <summary>
    /// Samus can leave Kraid's room during the fade back toward his colours (#1512; the 13% and 100% movies do): the
    /// door's fade advances the shared counter after each of the backdrop's steps, so the fade back takes every other
    /// value and completes sooner, when the counter passes the last moving value, on his colours (#2215); until then its
    /// colours move by its own steps (#2302).
    /// </summary>
    private static void VerifyKraidFadeBackDuringDoorFade(ImportEnemyArtwork artwork)
    {
        const int lastAlone = 6;
        RoomEnemySystem enemies = NewKraidRoom(artwork);
        StepKraidBackdropFade(enemies, KraidBackdropSettledColours.Black, PictureShading.Standard, PictureShading.Shaded(KraidShades.BackdropBlack),
            untilCounter: null, "Kraid's death fade");
        enemies.Presentation.FadeStepCount.Reset();
        PictureShading black = PictureShading.Shaded(KraidShades.BackdropBlack), kraid = PictureShading.Shaded(KraidShades.Backdrop);
        StepKraidBackdropFade(enemies, KraidBackdropSettledColours.KraidColours, black, kraid, untilCounter: lastAlone, "Kraid's fade back before the door");
        var advance = new Func<KraidBackdropSettledColours, bool>(enemies.KraidBehavior.Background.AdvanceKraidRoomBackgroundFade);
        int moved = lastAlone;
        while (true)
        {
            // The door's fade takes its step on the shared counter between the backdrop's.
            enemies.Presentation.FadeStepCount.Advance();
            int value = enemies.Presentation.FadeStepCount.Step;
            if (advance(KraidBackdropSettledColours.KraidColours))
            {
                AssertTrue(value > KraidBackgroundDefinitions.BackdropFadeLastStep, "the fade back completes once the counter passes its last moving value");
                AssertTrue(moved < KraidBackgroundDefinitions.BackdropFadeLastStep, "the door's fade brings the fade back's completion before its own steps come the whole way");
                AssertEqual(SettledBackdrop(KraidBackdropSettledColours.KraidColours), enemies.KraidBehavior.Access.Backdrop.Shows,
                    "a fade back whose steps skipped the last moving value completes on his colours");
                return;
            }
            AssertTrue(value % 2 == 0, $"the fade back takes every other counter value during the door's fade, not {value}");
            // Its colours move by its own steps, one part each, whatever values the counter takes (#2302).
            moved++;
            AssertEqual(PictureShading.Standard.With(new PictureShadeFade(black, kraid, new FadeLevel(moved, KraidBackgroundDefinitions.BackdropFadeLastStep))),
                PublishedRoomShading(enemies), $"Kraid's fade back during the door's fade at counter {value}");
        }
    }

    /// <summary>
    /// A fade between two shadings shows each colour partway between what each shading shows over it, each fade level
    /// moved by the fade's part of the way and truncated to a whole level, so every colour it shows lies on the game's fade
    /// scale (#2294); a fade from a fade starts from that fade's colours (#2215).
    /// </summary>
    private static void VerifyPictureShadeFadeColours()
    {
        RgbColor first = new FadeLevelColour(12, 0, 25).ToColour(), second = new FadeLevelColour(1, 30, 4).ToColour();
        SpriteImage picture = SpriteImageFixtures.Copied("Faded", new(2, 1),
            [new RgbaPixel((byte)first.Red, (byte)first.Green, (byte)first.Blue), new RgbaPixel((byte)second.Red, (byte)second.Green, (byte)second.Blue)],
            new PictureColours { Backdrop = ColourKeyConstants.Of(RgbColor.Black) });
        PictureShading white = PictureShading.Standard.With(new ColourFlash(RgbColor.White));
        PictureShading halfway = PictureShading.Standard.With(new PictureShadeFade(PictureShading.Standard, white, new FadeLevel(1, 2)));
        RgbColor[] shown = halfway.KeyColours(picture, global::SuperMetroid.Core.Assets.ColourBlendOracle.Instance);
        const int full = FadeLevelColour.FullLevel;
        for (int key = 0; key < shown.Length; key++)
        {
            FadeLevelColour own = picture.ColourKeys[key].Colour.FadeLevels;
            AssertEqual(new FadeLevelColour(own.Red + ((full - own.Red) / 2), own.Green + ((full - own.Green) / 2), own.Blue + ((full - own.Blue) / 2)).ToColour(),
                shown[key], $"key {key} shows halfway to white on the fade levels");
        }
        PictureShading black = PictureShading.Standard.With(new ColourFlash(RgbColor.Black));
        RgbColor[] fromHalfway = PictureShading.Standard.With(new PictureShadeFade(halfway, black, new FadeLevel(3, 4))).KeyColours(picture, global::SuperMetroid.Core.Assets.ColourBlendOracle.Instance);
        for (int key = 0; key < shown.Length; key++)
        {
            FadeLevelColour start = shown[key].FadeLevels;
            AssertEqual(new FadeLevelColour(start.Red - (start.Red * 3 / 4), start.Green - (start.Green * 3 / 4), start.Blue - (start.Blue * 3 / 4)).ToColour(),
                fromHalfway[key], $"key {key} shows three quarters from the halfway colour to black on the fade levels");
        }
        AssertTrue(PictureShading.Standard.With(new PictureShadeFade(PictureShading.Standard, white, new FadeLevel(1, 3))).KeyColours(picture, global::SuperMetroid.Core.Assets.ColourBlendOracle.Instance)
            .All(colour => colour == colour.FadeLevels.ToColour()), "every colour a fade shows lies on the game's fade scale");
        AssertTrue(PictureShading.Standard.With(new PictureShadeFade(PictureShading.Standard, white, new FadeLevel(0, 2))).KeyColours(picture, global::SuperMetroid.Core.Assets.ColourBlendOracle.Instance)
            .SequenceEqual(PictureShading.Standard.KeyColours(picture, global::SuperMetroid.Core.Assets.ColourBlendOracle.Instance)), "a fade that has not moved shows what it starts from");
        AssertTrue(PictureShading.Standard.With(new PictureShadeFade(PictureShading.Standard, white, new FadeLevel(2, 2))).KeyColours(picture, global::SuperMetroid.Core.Assets.ColourBlendOracle.Instance)
            .SequenceEqual(white.KeyColours(picture, global::SuperMetroid.Core.Assets.ColourBlendOracle.Instance)), "a fade that has come the whole way shows what it ends on");
        AssertThrows<ArgumentOutOfRangeException>(() => new PictureShadeFade(PictureShading.Standard, white, new FadeLevel(3, 2)), "a fade comes no further than its length");
        AssertThrows<ArgumentOutOfRangeException>(() => new PictureShadeFade(PictureShading.Standard, white, new FadeLevel(0, 0)), "a fade has a length");
    }

    /// <summary>
    /// Requires <paramref name="shown"/>, gameplay's eye colours of <paramref name="label"/>, to be the installed health
    /// colours of band <paramref name="place"/> of his health colours at the eyes' inks.
    /// </summary>
    private static void AssertEyes(KraidColorCatalog catalog, RgbColor[] shown, int place, string label)
    {
        for (int eye = 0; eye < KraidEyeGlow.Eyes; eye++)
            AssertEqual(catalog.Resolve(KraidPaletteSource.Health, KraidPalettes.BandColors * place + KraidPalettes.FirstEyeColour + eye),
                shown[eye], $"Kraid {label} eye {eye} installed production colour");
    }

    /// <summary>
    /// Kraid's typed health bands (#2297) select the band the integer bands did, for his authored health and its
    /// neighbours: from the highest down, the first band n whose n + 1 eighths of his initial health his health reaches,
    /// else the lowest, band n being the n-th most hurt.
    /// </summary>
    private static void VerifyKraidHealthBands()
    {
        KraidHealthBand[] mostHurtFirst = [.. KraidHealthBandDefinitions.HighestFirst.Reverse()];
        foreach (int initial in new[] { 1000, 999, 1007, 8 })
        {
            for (int health = 0; health <= initial; health++)
            {
                int band = mostHurtFirst.Length - 1;
                while (band != 0 && health < KraidHealthThresholds.Eighths(initial, band + 1))
                    band--;
                AssertEqual(mostHurtFirst[band], KraidHealthBand.Of(health, initial),
                    $"Kraid at {health} of {initial} health is in his band {band}");
            }
        }
        AssertEqual(KraidHealthBandDefinitions.Unhurt, KraidHealthBand.Of(1000, 1000), "Kraid at his whole health is unhurt");
        AssertEqual(KraidHealthBandDefinitions.BelowWhole, KraidHealthBand.Of(999, 1000), "Kraid one below his whole health is below it");
        AssertEqual(KraidHealthBandDefinitions.BelowQuarter, KraidHealthBand.Of(249, 1000), "Kraid below a quarter of his health is in his most hurt band");
    }

    /// <summary>The counter values a backdrop fade can take before it completes: zero through the last moving value.</summary>
    private const int FadeCounterValues = KraidBackgroundDefinitions.BackdropFadeLastStep + 1;

    /// <summary>A fresh Kraid room's enemies, Kraid loaded.</summary>
    private static RoomEnemySystem NewKraidRoom(ImportEnemyArtwork artwork)
    {
        // The room binds Kraid's parts once; a later initialization joins the same parts again.
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(artwork.Enemies), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, installedBosses: artwork.Bosses);
        enemies.Places.BindKraidPartsInRoomOrder();
        _ = enemies.KraidBehavior.BeginEncounterInRoomOrder(enemies.Places);
        return enemies;
    }

    /// <summary>
    /// Steps a fade of Kraid's backdrop through production, through the step at counter <paramref name="untilCounter"/>
    /// or until it completes, and checks that each step publishes the fade from <paramref name="from"/> toward
    /// <paramref name="to"/> by its own steps (#2215, #2302): nothing at its first step, then one of its thirteen parts per
    /// later step, and <paramref name="to"/> once it has come the whole way.
    /// </summary>
    private static void StepKraidBackdropFade(RoomEnemySystem enemies, KraidBackdropSettledColours fade, PictureShading from, PictureShading to,
        int? untilCounter, string label)
    {
        var advance = new Func<KraidBackdropSettledColours, bool>(enemies.KraidBehavior.Background.AdvanceKraidRoomBackgroundFade);
        const int length = KraidBackgroundDefinitions.BackdropFadeLastStep;
        int steps = 0;
        while (true)
        {
            int value = enemies.Presentation.FadeStepCount.Step;
            if (advance(fade))
            {
                AssertTrue(untilCounter is null, $"{label} completes only when stepped to its end");
                AssertEqual(to, PublishedRoomShading(enemies), $"{label} completed shows its goal");
                return;
            }
            int moved = Math.Min(steps, length);
            PictureShading expected = moved == 0 ? from
                : moved == length ? to
                : PictureShading.Standard.With(new PictureShadeFade(from, to, new FadeLevel(moved, length)));
            AssertEqual(expected, PublishedRoomShading(enemies), $"{label} at counter {value}");
            AssertTrue(++steps <= FadeCounterValues, $"{label} completes once the counter passes its last moving value");
            if (value == untilCounter)
                return;
        }
    }

    /// <summary>The room pictures' shading Kraid's backdrop publishes now.</summary>
    private static PictureShading PublishedRoomShading(RoomEnemySystem enemies)
    {
        var frame = new GameplayShadingBuilder();
        enemies.KraidBehavior.Access.Backdrop.ShadeGameplay(frame);
        var colours = new PictureColours
        {
            Backdrop = ColourKeyConstants.Of(RgbColor.Black),
            Shades = new Dictionary<PictureShade, PictureRecolouring>
            {
                [KraidShades.Backdrop] = new PictureRecolouring([]),
                [KraidShades.BackdropBlack] = new PictureRecolouring([]),
            }.ToImmutableDictionary(),
        };
        return frame.BuildOverFixture(SpriteImageFixtures.Copied("Room", new(1, 1), [default(RgbaPixel)], colours)).Of(GameplayPictures.Room);
    }

    /// <summary>The backdrop settled on <paramref name="colours"/>.</summary>
    private static KraidBackdropShown.Settled SettledBackdrop(KraidBackdropSettledColours colours) => new KraidBackdropShown.Settled(colours);

    /// <summary>Builds an independent CGRAM reference from pinned sm_a7.c palette writes; zero thresholds select health band eight at 1000 HP and odd hurt frames select flash.</summary>
    private static ushort[] ReferenceKraidPaletteConsumer(
        ISnesAddressSpace rom, KraidPaletteConsumer consumer)
    {
        var colors = new ushort[SnesCgram.ColorCount];
        switch (consumer)
        {
            case KraidPaletteConsumer.BackdropLoad:
            case KraidPaletteConsumer.BackdropFade:
                // Fade step thirteen assigns the target exactly.
                Copy(0xa786c7, 96);
                break;
            case KraidPaletteConsumer.InitialTargetLoad:
                Copy(0xa7aaa6, 176);
                break;
            case KraidPaletteConsumer.HealthNormal:
            case KraidPaletteConsumer.HealthFlash:
                int offset = consumer == KraidPaletteConsumer.HealthNormal ? 256 : 0;
                Copy(0xa7b3d3 + offset, 112);
                Copy(0xa7b513 + offset, 240);
                break;
            case KraidPaletteConsumer.EyeUnglow:
                // Start with the glow's raised red/green channels so this is a valid
                // contraction step, rather than subtracting from a black synthetic eye.
                for (int eye = 0; eye < 3; eye++)
                {
                    ushort target = rom.ReadCartridgeWord((0xa7b3d3 + 256 + (eye + 1) * 2));
                    ushort current = 0x03ff;
                    if ((current & 31) != (target & 31)) current--;
                    if ((current & 0x03e0) != (target & 0x03e0)) current -= 32;
                    colors[113 + eye] = current;
                }
                break;
            case KraidPaletteConsumer.DeathArm:
                Copy(0xa7b4f3, 112);
                break;
            default:
                throw new InvalidDataException($"Unknown Kraid reference consumer {consumer}.");
        }
        return colors;

        void Copy(int source, int destination)
        {
            for (int color = 0; color < 16; color++)
                colors[destination + color] = rom.ReadCartridgeWord((source + color * 2));
        }
    }

    /// <summary>Rejects runtime reads from every Kraid palette source range after artwork installation.</summary>
    private static CartridgeReadGuard NewKraidPaletteSourceGuard(ISnesAddressSpace rom)
    {
        var guard = new CartridgeReadGuard(rom);
        foreach (KraidPaletteSource palette in Enum.GetValues<KraidPaletteSource>())
        {
            int start = KraidPaletteSources.SourceAddress(palette);
            int end = start + KraidPalettes.ColorCount(palette) * sizeof(ushort);
            guard.Forbid(address => address >= start && address < end,
                address => $"Kraid {palette} reread palette ROM byte ${address:X6}.");
        }
        return guard;
    }

    /// <summary>
    /// Runs one Kraid colour consumer and lays the shades and colours his typed colour state names into a black colour
    /// image at the cartridge's colour positions (#1512): a background picture's ink is its position, a sprite picture's
    /// ink lies past the 128 background colours.
    /// </summary>
    private static RgbColor[] CaptureKraidPaletteConsumer(
        ImportEnemyArtwork artwork,
        KraidPaletteConsumer consumer)
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(artwork.Enemies), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, installedBosses: artwork.Bosses);
        var colours = new RgbColor[SnesCgram.ColorCount];
        enemies.Places.BindKraidPartsInRoomOrder();
        RoomEnemyPlace body = enemies.Places.InUpdateOrder[0];
        body.Health = 1000;
        // The death setup restarts the arm's typed program (#641), which lives in the room's Kraid state.
        KraidEnemyState state = enemies.KraidBehavior.BeginEncounterInRoomOrder(enemies.Places);
        KraidColorCatalog kraid = artwork.Bosses.KraidColors ?? throw new InvalidOperationException("The installed artwork has no Kraid colours.");
        switch (consumer)
        {
            case KraidPaletteConsumer.BackdropLoad:
                // The defeated room shows Kraid's backdrop: the import's shade of it lays his colours over the backdrop's inks.
                enemies.KraidBehavior.Access.Backdrop.ShowKraidColours();
                AssertEqual(SettledBackdrop(KraidBackdropSettledColours.KraidColours), enemies.KraidBehavior.Access.Backdrop.Shows, "Kraid's defeated room shows his backdrop");
                Lay(colours, KraidBackdropShades.Of(kraid), KraidShades.Backdrop, 0);
                break;
            case KraidPaletteConsumer.InitialTargetLoad:
                // His rocks draw in the import's shade of his initialization's colours, over the sprite row they draw in.
                Lay(colours, BossSheetShades.SpawnedObjects(artwork.Bosses, EnemyDefinitionId.Kraid), KraidShades.Rocks, ImageColourGroup.HalfColours);
                break;
            case KraidPaletteConsumer.BackdropFade:
                // The growth fade's last moving step, its fourteenth, lands on its goal, the import's shade of Kraid's
                // colours exactly (#2302).
                for (int step = 0; step <= KraidBackgroundDefinitions.BackdropFadeLastStep; step++)
                    AssertTrue(!new Func<KraidBackdropSettledColours, bool>(enemies.KraidBehavior.Background.AdvanceKraidRoomBackgroundFade)(KraidBackdropSettledColours.KraidColours),
                        "Kraid's growth fade moves through its last moving step");
                AssertEqual(SettledBackdrop(KraidBackdropSettledColours.KraidColours), enemies.KraidBehavior.Access.Backdrop.Shows,
                    "Kraid's growth fade's last step lands on his colours");
                Lay(colours, KraidBackdropShades.Of(kraid), KraidShades.Backdrop, 0);
                break;
            case KraidPaletteConsumer.HealthNormal:
            case KraidPaletteConsumer.HealthFlash:
            {
                KraidHurtFlashShade shown = consumer == KraidPaletteConsumer.HealthFlash
                    ? KraidHurtFlashShade.HurtColours : KraidHurtFlashShade.BandColours;
                new Action<RoomEnemyPlace, KraidEnemyState, KraidHurtFlashShade>(enemies.KraidBehavior.Head.UpdateKraidHealthColours)(body, state, shown);
                PictureShade shade = state.Colours.Shown.PartsShade ?? throw new InvalidOperationException("Kraid's health update shows no shade.");
                AssertEqual(consumer == KraidPaletteConsumer.HealthFlash, shade == KraidShades.HurtFlash, $"Kraid's {consumer} update shows the hurt flash only on its flash");
                AssertEqual(PictureShading.Shaded(shade), body.Shading, $"Kraid's body slot draws in {shade}");
                // His body shows the band's shade over its sixth tile colours, his parts over their sprite row.
                Lay(colours, BossSheetShades.Body(BossBodyOwner.Kraid, artwork.Bosses, [], SpriteImageColours.Background, new RgbColor[ImageColourGroup.HalfColours]),
                    shade, 0);
                Lay(colours, BossSheetShades.OwnRow(artwork.Bosses, EnemyDefinitionId.Kraid, KraidPartsRow * 16), shade,
                    ImageColourGroup.HalfColours);
                // Every body shade draws his eyes in one set of colours, which his eye glow replaces with the band's or the flash's.
                ImmutableArray<RgbColor> glow = state.Colours.EyeGlow.Shown;
                for (int eye = 0; eye < glow.Length; eye++)
                    colours[113 + eye] = glow[eye];
                break;
            }
            case KraidPaletteConsumer.EyeUnglow:
            {
                // The glow's raised red and green channels, then one step of the fade: the eye glow service owns the colours.
                state.Colours.ShowBandEyes([RgbColor.FromWord(0x03ff), RgbColor.FromWord(0x03ff), RgbColor.FromWord(0x03ff)]);
                enemies.KraidBehavior.Head.UnglowKraidEye(body, state, (KraidStageRun.Fighting)state.BodyState.Run);
                ImmutableArray<RgbColor> eyes = state.Colours.EyeGlow.Shown;
                for (int eye = 0; eye < eyes.Length; eye++)
                    colours[113 + eye] = eyes[eye];
                break;
            }
            case KraidPaletteConsumer.DeathArm:
                enemies.KraidBehavior.Death.InitializeKraidDeath(state, SuperMetroid.Core.Enemies.RoomEnemyFrameFixtures.StandaloneSamus());
                AssertTrue(state.Colours.Shown is KraidColours.DeathColours, "Kraid's death shows his death colours");
                Lay(colours, BossSheetShades.Body(BossBodyOwner.Kraid, artwork.Bosses, [], SpriteImageColours.Background, new RgbColor[ImageColourGroup.HalfColours]),
                    KraidShades.DeathArm, 0);
                break;
            default:
                throw new InvalidOperationException($"Unknown Kraid palette consumer {consumer}.");
        }
        return colours;
    }

    /// <summary>The sprite row Kraid's parts draw in, past the background colours.</summary>
    private const int KraidPartsRow = 7;

    /// <summary>Lays the import's <paramref name="shade"/> among <paramref name="made"/> into <paramref name="colours"/> from <paramref name="offset"/>.</summary>
    private static void Lay(RgbColor[] colours, IReadOnlyList<SheetShade> made, PictureShade shade, int offset)
    {
        SheetShade found = made.Single(candidate => candidate.Shade == shade);
        foreach ((int ink, RgbColor colour) in found.InkColours)
            colours[offset + ink] = colour;
    }

    /// <summary>Production paths that load or animate Kraid's installed palette rows.</summary>
    internal enum KraidPaletteConsumer
    {
        /// <summary>Initial room backdrop palette copy.</summary>
        BackdropLoad,
        /// <summary>Initial target sprite palette copy.</summary>
        InitialTargetLoad,
        /// <summary>Final step of the room backdrop fade.</summary>
        BackdropFade,
        /// <summary>Health-based normal body and secondary bands.</summary>
        HealthNormal,
        /// <summary>Odd hurt-frame flash band for both body palettes.</summary>
        HealthFlash,
        /// <summary>One contraction step of the glowing eye colors.</summary>
        EyeUnglow,
        /// <summary>Arm palette written during death initialization.</summary>
        DeathArm,
    }
}
