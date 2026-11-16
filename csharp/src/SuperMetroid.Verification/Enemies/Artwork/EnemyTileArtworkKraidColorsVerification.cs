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
        foreach (KraidColourBand band in KraidColourBand.All)
        {
            RgbColor[] shown = eyes.Of(band).ToArray();
            for (int eye = 0; eye < KraidEyeGlow.Eyes; eye++)
                AssertEqual(catalog.Resolve(KraidPaletteSource.Health, KraidPalettes.BandColors * band.ColourGroup + KraidPalettes.FirstEyeColour + eye),
                    shown[eye], $"Kraid band {band} eye {eye} installed production colour");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => eyes.HealthBand(KraidHealthThresholdDefinitions.EighthCount).ToArray(),
            "Kraid's eye colours reject a ninth health band");
        // The game reads the eye colours from their own installed document by purpose (#1637), and no other Kraid colour.
        SuperMetroid.AssetExtraction.Installation.GameInstallation installation = SuperMetroid.TestSupport.RepositoryInstallation.RepositoryInstallation.Installation;
        AssertTrue(!File.Exists(Path.Combine(installation.EnemyTileDirectory, KraidColorFormat.FileName)),
            "the installation holds none of Kraid's colours but his eyes'");
        KraidEyeColours installedEyes = installation.LoadEnemyTiles().KraidEyes;
        AssertEqual(KraidHealthThresholdDefinitions.EighthCount + 1, KraidColourBand.All.Count, "his colour bands are the hurt flash and the eight health bands");
        AssertEqual(0, KraidColourBand.HurtFlash.ColourGroup, "the hurt flash's colours come first among his health colours");
        foreach (KraidColourBand band in KraidColourBand.All)
            AssertTrue(installedEyes.Of(band).ToArray().SequenceEqual(eyes.Of(band).ToArray()),
                $"Kraid band {band}'s installed eye colours are the eyes' colours of his health colours");
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
        var fade = new Func<KraidBackdropFade, bool>(enemies.KraidBehavior.Background.AdvanceKraidRoomBackgroundFade);
        int growthSteps = 0;
        while (!fade(KraidBackdropFade.ToKraidColours))
            AssertTrue(++growthSteps <= FadeCounterValues, "Kraid's growth fade completes once the counter passes its last moving value");
        AssertEqual(SettledBackdrop(KraidBackdropSettledColours.KraidColours), enemies.KraidBehavior.Access.Backdrop.Shows, "Kraid's growth fade ends on his colours");
        // Kraid's body initializer replaces his encounter state; the room's backdrop colours outlive it.
        _ = enemies.KraidBehavior.BeginEncounterInRoomOrder(enemies.Places);
        AssertEqual(SettledBackdrop(KraidBackdropSettledColours.KraidColours), enemies.KraidBehavior.Access.Backdrop.Shows,
            "re-initializing Kraid keeps the backdrop his growth fade showed");
        int deathSteps = 0;
        while (!fade(KraidBackdropFade.ToBlack))
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
        StepKraidBackdropFade(enemies, KraidBackdropFade.ToBlack, PictureShading.Standard, PictureShading.Shaded(KraidShades.BackdropBlack),
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
        StepKraidBackdropFade(enemies, KraidBackdropFade.ToKraidColours, PictureShading.Standard, PictureShading.Shaded(KraidShades.Backdrop),
            untilCounter: KraidBackgroundDefinitions.BackdropFadeLastStep, "Kraid's growth fade");
        AssertEqual(SettledBackdrop(KraidBackdropSettledColours.KraidColours), enemies.KraidBehavior.Access.Backdrop.Shows,
            "Kraid's growth fade's last moving step lands on his colours");
        var fade = new Func<KraidBackdropFade, bool>(enemies.KraidBehavior.Background.AdvanceKraidRoomBackgroundFade);
        AssertTrue(fade(KraidBackdropFade.ToBlack), "Kraid's death fade after his growth fade's last step completes without a step");
        AssertEqual(SettledBackdrop(KraidBackdropSettledColours.Black), enemies.KraidBehavior.Access.Backdrop.Shows,
            "a death fade that completes without a step shows its goal");
        // The death zeroes the shared counter once his body has sunk, before the fade back.
        enemies.Presentation.FadeStepCount.Reset();
        StepKraidBackdropFade(enemies, KraidBackdropFade.ToKraidColours, PictureShading.Shaded(KraidShades.BackdropBlack),
            PictureShading.Shaded(KraidShades.Backdrop), untilCounter: null, "Kraid's fade back from black");
        AssertEqual(SettledBackdrop(KraidBackdropSettledColours.KraidColours), enemies.KraidBehavior.Access.Backdrop.Shows,
            "Kraid's fade back ends on his colours");

        RoomEnemySystem defeated = NewKraidRoom(artwork);
        defeated.KraidBehavior.Access.Backdrop.ShowKraidColours();
        var defeatedFade = new Func<KraidBackdropFade, bool>(defeated.KraidBehavior.Background.AdvanceKraidRoomBackgroundFade);
        while (!defeatedFade(KraidBackdropFade.ToKraidColours))
        {
            AssertEqual(SettledBackdrop(KraidBackdropSettledColours.KraidColours), defeated.KraidBehavior.Access.Backdrop.Shows,
                "Kraid's defeated room's fade back into his colours changes nothing");
        }
    }

    /// <summary>
    /// Kraid can die during his growth fade (#1512): his death fade then starts from the growth fade partway and continues
    /// its counter, so it has fewer parts to come (#2215), and ends on black.
    /// </summary>
    private static void VerifyKraidDeathFadeDuringGrowthFade(ImportEnemyArtwork artwork)
    {
        RoomEnemySystem enemies = NewKraidRoom(artwork);
        StepKraidBackdropFade(enemies, KraidBackdropFade.ToKraidColours, PictureShading.Standard, PictureShading.Shaded(KraidShades.Backdrop),
            untilCounter: 5, "Kraid's growth fade");
        AssertTrue(enemies.KraidBehavior.Access.Backdrop.Shows is KraidBackdropShown.Fading,
            "Kraid's growth fade is partway when he dies");
        PictureShading growthPartway = PictureShading.Standard.With(
            new PictureShadeFade(PictureShading.Standard, PictureShading.Shaded(KraidShades.Backdrop), 5, KraidBackgroundDefinitions.BackdropFadeLastStep));
        StepKraidBackdropFade(enemies, KraidBackdropFade.ToBlack, growthPartway, PictureShading.Shaded(KraidShades.BackdropBlack),
            untilCounter: null, "Kraid's death fade during his growth fade");
        AssertEqual(SettledBackdrop(KraidBackdropSettledColours.Black), enemies.KraidBehavior.Access.Backdrop.Shows,
            "Kraid's death fade during his growth fade ends on black");
    }

    /// <summary>
    /// Samus can leave Kraid's room during the fade back toward his colours (#1512; the 13% and 100% movies do): the
    /// door's fade advances the shared counter after each of the backdrop's steps, so the fade back takes every other
    /// value and comes on faster, and it completes when the counter passes the last moving value, on his colours (#2215).
    /// </summary>
    private static void VerifyKraidFadeBackDuringDoorFade(ImportEnemyArtwork artwork)
    {
        const int lastAlone = 6;
        RoomEnemySystem enemies = NewKraidRoom(artwork);
        StepKraidBackdropFade(enemies, KraidBackdropFade.ToBlack, PictureShading.Standard, PictureShading.Shaded(KraidShades.BackdropBlack),
            untilCounter: null, "Kraid's death fade");
        enemies.Presentation.FadeStepCount.Reset();
        PictureShading black = PictureShading.Shaded(KraidShades.BackdropBlack), kraid = PictureShading.Shaded(KraidShades.Backdrop);
        StepKraidBackdropFade(enemies, KraidBackdropFade.ToKraidColours, black, kraid, untilCounter: lastAlone, "Kraid's fade back before the door");
        var advance = new Func<KraidBackdropFade, bool>(enemies.KraidBehavior.Background.AdvanceKraidRoomBackgroundFade);
        while (true)
        {
            // The door's fade takes its step on the shared counter between the backdrop's.
            enemies.Presentation.FadeStepCount.Advance();
            int value = enemies.Presentation.FadeStepCount.Step;
            if (advance(KraidBackdropFade.ToKraidColours))
            {
                AssertTrue(value > KraidBackgroundDefinitions.BackdropFadeLastStep, "the fade back completes once the counter passes its last moving value");
                AssertEqual(SettledBackdrop(KraidBackdropSettledColours.KraidColours), enemies.KraidBehavior.Access.Backdrop.Shows,
                    "a fade back whose steps skipped the last moving value completes on his colours");
                return;
            }
            AssertTrue(value % 2 == 0, $"the fade back takes every other counter value during the door's fade, not {value}");
            AssertEqual(PictureShading.Standard.With(new PictureShadeFade(black, kraid, value, KraidBackgroundDefinitions.BackdropFadeLastStep)),
                PublishedRoomShading(enemies), $"Kraid's fade back during the door's fade at counter {value}");
        }
    }

    /// <summary>
    /// A fade between two shadings shows each colour partway between what each shading shows over it, each channel moved
    /// by the fade's part of the way and truncated; a fade from a fade starts from that fade's colours (#2215).
    /// </summary>
    private static void VerifyPictureShadeFadeColours()
    {
        SpriteImage picture = SpriteImageFixtures.Copied("Faded", new(2, 1), [new RgbaPixel(100, 0, 200), new RgbaPixel(10, 250, 31)],
            new PictureColours { Backdrop = ColourKeyConstants.Of(RgbColor.Black) });
        PictureShading white = PictureShading.Standard.With(new ColourFlash(RgbColor.White));
        PictureShading halfway = PictureShading.Standard.With(new PictureShadeFade(PictureShading.Standard, white, 1, 2));
        RgbColor[] shown = halfway.KeyColours(picture);
        for (int key = 0; key < shown.Length; key++)
        {
            RgbColor own = picture.ColourKeys[key].Colour;
            AssertEqual(new RgbColor(own.Red + ((255 - own.Red) / 2), own.Green + ((255 - own.Green) / 2), own.Blue + ((255 - own.Blue) / 2)),
                shown[key], $"key {key} shows halfway to white");
        }
        PictureShading black = PictureShading.Standard.With(new ColourFlash(RgbColor.Black));
        RgbColor[] fromHalfway = PictureShading.Standard.With(new PictureShadeFade(halfway, black, 3, 4)).KeyColours(picture);
        for (int key = 0; key < shown.Length; key++)
        {
            RgbColor start = shown[key];
            AssertEqual(new RgbColor(start.Red - (start.Red * 3 / 4), start.Green - (start.Green * 3 / 4), start.Blue - (start.Blue * 3 / 4)),
                fromHalfway[key], $"key {key} shows three quarters from the halfway colour to black");
        }
        AssertTrue(PictureShading.Standard.With(new PictureShadeFade(PictureShading.Standard, white, 0, 2)).KeyColours(picture)
            .SequenceEqual(PictureShading.Standard.KeyColours(picture)), "a fade that has not moved shows what it starts from");
        AssertTrue(PictureShading.Standard.With(new PictureShadeFade(PictureShading.Standard, white, 2, 2)).KeyColours(picture)
            .SequenceEqual(white.KeyColours(picture)), "a fade that has come the whole way shows what it ends on");
        AssertThrows<ArgumentOutOfRangeException>(() => new PictureShadeFade(PictureShading.Standard, white, 3, 2), "a fade comes no further than its length");
        AssertThrows<ArgumentOutOfRangeException>(() => new PictureShadeFade(PictureShading.Standard, white, 0, 0), "a fade has a length");
    }

    /// <summary>The counter values a backdrop fade can take before it completes: zero through the last moving value.</summary>
    private const int FadeCounterValues = KraidBackgroundDefinitions.BackdropFadeLastStep + 1;

    /// <summary>A fresh Kraid room's enemies, Kraid loaded.</summary>
    private static RoomEnemySystem NewKraidRoom(ImportEnemyArtwork artwork)
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(artwork.Enemies), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, installedBosses: artwork.Bosses);
        _ = enemies.KraidBehavior.BeginEncounterInRoomOrder(enemies.Places);
        return enemies;
    }

    /// <summary>
    /// Steps a fade of Kraid's backdrop through production, through the step at counter <paramref name="untilCounter"/>
    /// or until it completes, and checks that each step publishes the fade from <paramref name="from"/> toward
    /// <paramref name="to"/> the counter's part of the way (#2215): nothing before its first moving value, then one part per
    /// moving value from its first through the last, and <paramref name="to"/> once it has come the whole way.
    /// </summary>
    private static void StepKraidBackdropFade(RoomEnemySystem enemies, KraidBackdropFade fade, PictureShading from, PictureShading to,
        int? untilCounter, string label)
    {
        var advance = new Func<KraidBackdropFade, bool>(enemies.KraidBehavior.Background.AdvanceKraidRoomBackgroundFade);
        int firstMoving = Math.Max(enemies.Presentation.FadeStepCount.Step, 1);
        int length = KraidBackgroundDefinitions.BackdropFadeLastStep - firstMoving + 1;
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
            int moved = Math.Max(0, value - firstMoving + 1);
            PictureShading expected = moved == 0 ? from
                : moved == length ? to
                : PictureShading.Standard.With(new PictureShadeFade(from, to, moved, length));
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
        return frame.Build(SpriteImageFixtures.Copied("Room", new(1, 1), [default(RgbaPixel)], colours)).Of(GameplayPictures.Room);
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
                    ushort target = CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa7b3d3 + 256 + (eye + 1) * 2));
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
                colors[destination + color] = CartridgeImportSource.Require(rom).ReadCartridgeWord((source + color * 2));
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
                // Step thirteen of the growth fade lands on its goal, the import's shade of Kraid's colours exactly.
                for (int step = 0; step < 13; step++)
                    enemies.Presentation.FadeStepCount.Advance();
                _ = new Func<KraidBackdropFade, bool>(enemies.KraidBehavior.Background.AdvanceKraidRoomBackgroundFade)(KraidBackdropFade.ToKraidColours);
                AssertEqual(SettledBackdrop(KraidBackdropSettledColours.KraidColours), enemies.KraidBehavior.Access.Backdrop.Shows,
                    "Kraid's growth fade's last step lands on his colours");
                Lay(colours, KraidBackdropShades.Of(kraid), KraidShades.Backdrop, 0);
                break;
            case KraidPaletteConsumer.HealthNormal:
            case KraidPaletteConsumer.HealthFlash:
            {
                state.HurtFrame = consumer == KraidPaletteConsumer.HealthFlash
                    ? (ushort)1 : (ushort)0;
                new Action<RoomEnemyPlace, KraidEnemyState>(enemies.KraidBehavior.Head.UpdateKraidHealthColours)(body, state);
                KraidColourBand band = state.Colours.Band ?? throw new InvalidOperationException("Kraid's health update names no band.");
                AssertEqual(consumer == KraidPaletteConsumer.HealthFlash, band == KraidColourBand.HurtFlash, $"Kraid's {consumer} update shows the hurt flash only on its flash");
                AssertEqual(PictureShading.Shaded(KraidShades.Health(band)), body.Shading, $"Kraid's body slot draws in band {band}");
                // His body shows the band's shade over its sixth tile colours, his parts over their sprite row.
                Lay(colours, BossSheetShades.Body(BossBodyOwner.Kraid, artwork.Bosses, [], new RgbColor[ImageColourGroup.HalfColours]),
                    KraidShades.Health(band), 0);
                Lay(colours, BossSheetShades.OwnRow(artwork.Bosses, EnemyDefinitionId.Kraid, KraidPartsRow * 16), KraidShades.Health(band),
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
                new Action<RoomEnemyPlace, KraidEnemyState>(enemies.KraidBehavior.Head.UnglowKraidEye)(body, state);
                ImmutableArray<RgbColor> eyes = state.Colours.EyeGlow.Shown;
                for (int eye = 0; eye < eyes.Length; eye++)
                    colours[113 + eye] = eyes[eye];
                break;
            }
            case KraidPaletteConsumer.DeathArm:
                enemies.KraidBehavior.Death.InitializeKraidDeath(state, SuperMetroid.Core.Enemies.RoomEnemyFrameFixtures.StandaloneSamus());
                AssertTrue(state.Colours.ShowsDeathArm, "Kraid's death shows his death colours");
                Lay(colours, BossSheetShades.Body(BossBodyOwner.Kraid, artwork.Bosses, [], new RgbColor[ImageColourGroup.HalfColours]),
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
        for (int index = 0; index < found.Inks.Length; index++)
            colours[offset + found.Inks[index]] = found.Colours[index];
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
