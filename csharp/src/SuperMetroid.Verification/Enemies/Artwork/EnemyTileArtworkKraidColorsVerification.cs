using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.Tooling;
using System.Text.Json;
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

namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>Checks installed and edited Kraid RGB5 rows through the palette consumers that write CGRAM.</summary>
internal static class EnemyTileArtworkKraidColorsVerification
{
    /// <summary>Compares every stock color with ROM, checks guarded production writes, and confirms edited colors survive reload.</summary>
    internal static void VerifyInstalledKraidColors(
        SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace rom, string directory, InstalledEnemyArtwork stock)
    {
        Suite(nameof(VerifyKraidPaletteSourceAddresses), () => VerifyKraidPaletteSourceAddresses(rom));
        Suite(nameof(VerifyKraidPaletteSourceLengths), () => VerifyKraidPaletteSourceLengths());
        Suite(nameof(VerifyKraidBackdropSurvivesReinitialization), () => VerifyKraidBackdropSurvivesReinitialization(stock));
        Suite(nameof(VerifyKraidDeathFadeFromRoomColours), () => VerifyKraidDeathFadeFromRoomColours(stock));
        Suite(nameof(VerifyKraidFadeBackFromKraidColours), () => VerifyKraidFadeBackFromKraidColours(stock));
        Suite(nameof(VerifyKraidDeathFadeDuringGrowthFade), () => VerifyKraidDeathFadeDuringGrowthFade(stock));
        Suite(nameof(VerifyKraidFadeBackDuringDoorFade), () => VerifyKraidFadeBackDuringDoorFade(stock));
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
        var eyeReaderOwner = new RoomEnemySystem(new EnemySession(new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()), definitions: CompiledRoomEnemyDefinitions.Instance) { TileArtwork = stock.Enemies, BossArtwork = stock.Bosses };
        KraidEyeColours eyes = eyeReaderOwner.KraidBehavior.Background.EyeColours;
        for (int band = -1; band < KraidHealthThresholdDefinitions.EighthCount; band++)
        {
            RgbColor[] shown = (band < 0 ? eyes.Hurt : eyes.HealthBand(band)).ToArray();
            for (int eye = 0; eye < KraidEyeGlow.Eyes; eye++)
                AssertEqual(catalog.Resolve(KraidPaletteSource.Health, KraidPalettes.BandColors * (band + 1) + KraidPalettes.FirstEyeColour + eye),
                    shown[eye], $"Kraid band {band} eye {eye} installed production colour");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => eyes.HealthBand(KraidHealthThresholdDefinitions.EighthCount).ToArray(),
            "Kraid's eye colours reject a ninth health band");
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

        string stockPath = Path.Combine(directory, KraidColorFormat.FileName);
        byte[] original = File.ReadAllBytes(stockPath);
        KraidColorDocument document = RequiredJson.Deserialize<KraidColorDocument>(
            original, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        static PaletteRgb5[] EditFirst(PaletteRgb5[] source, int index)
        {
            PaletteRgb5[] colors = (PaletteRgb5[])source.Clone();
            colors[index] = colors[index] with { Red = colors[index].Red ^ 1 };
            return colors;
        }
        var editedDocument = document with
        {
            RoomBackdrop = EditFirst(document.RoomBackdrop, 0),
            InitialTarget = EditFirst(document.InitialTarget, 0),
            Health = EditFirst(document.Health, 128),
            Secondary = EditFirst(document.Secondary, 128),
            DeathArm = EditFirst(document.DeathArm, 0),
        };
        string overrides = Path.Combine(directory, "kraid-color-overrides");
        Directory.CreateDirectory(overrides);
        string overridePath = Path.Combine(overrides, KraidColorFormat.FileName);
        File.WriteAllBytes(overridePath, KraidColorFile.Write(editedDocument));
        InstalledEnemyArtwork edited = stock with { Bosses = stock.Bosses with { KraidColors = EnemyTileArtworkFiles.Open(directory, overrides).KraidColors() } };
        foreach ((KraidPaletteConsumer consumer, int cgramIndex) in new[]
        {
            new VerifyInstalledKraidColorsCase(KraidPaletteConsumer.BackdropLoad, 96),
            new VerifyInstalledKraidColorsCase(KraidPaletteConsumer.InitialTargetLoad, 176),
            new VerifyInstalledKraidColorsCase(KraidPaletteConsumer.BackdropFade, 96),
            new VerifyInstalledKraidColorsCase(KraidPaletteConsumer.HealthNormal, 112),
            new VerifyInstalledKraidColorsCase(KraidPaletteConsumer.HealthNormal, 240),
            new VerifyInstalledKraidColorsCase(KraidPaletteConsumer.DeathArm, 112),
        })
        {
            ushort[] baseline = ReferenceKraidPaletteConsumer(rom, consumer);
            var guard = NewKraidPaletteSourceGuard(rom);
            RgbColor[] changed = CaptureKraidPaletteConsumer(edited, consumer);
            AssertEqual(0, guard.ForbiddenReadAttempts,
                $"edited Kraid {consumer} avoids palette ROM reads");
            AssertEqual((ushort)(baseline[cgramIndex] ^ 1), changed[cgramIndex],
                $"edited Kraid {consumer} changes CGRAM {cgramIndex}");
        }
        InstalledEnemyArtwork reloaded = stock with { Bosses = stock.Bosses with { KraidColors = EnemyTileArtworkFiles.Open(directory, overrides).KraidColors() } };
        AssertTrue(CaptureKraidPaletteConsumer(reloaded,
                KraidPaletteConsumer.HealthNormal)
            .SequenceEqual(CaptureKraidPaletteConsumer(edited, KraidPaletteConsumer.HealthNormal)),
            "Kraid RGB5 override survives catalog reload");

        File.WriteAllBytes(stockPath, [0]);
        AssertThrows<InvalidDataException>(() => EnemyTileArtworkFiles.Open(directory, null).KraidColors(),
            "tampered stock Kraid color file fails loudly where it is read");
        File.WriteAllBytes(stockPath, original);
        File.WriteAllBytes(overridePath, [0]);
        AssertThrows<InvalidDataException>(() => EnemyTileArtworkFiles.Open(directory, overrides).KraidColors(),
            "malformed Kraid color override fails at load");
        PaletteRgb5[] invalidHealth = (PaletteRgb5[])editedDocument.Health.Clone();
        invalidHealth[0] = invalidHealth[0] with { Red = 32 };
        AssertThrows<InvalidDataException>(() => KraidColorFile.Read(new MemoryStream(KraidColorFile.Write(
                editedDocument with { Health = invalidHealth }), writable: false)),
            "reading Kraid colors rejects RGB5 channels outside native precision");
    }

    /// <summary>
    /// Kraid's backdrop keeps what his growth fade showed when Kraid is initialized again, which replaces his encounter
    /// state, as the cartridge's colour memory keeps it; his death fade then starts from his colours instead of failing
    /// on the room's (#1532).
    /// </summary>
    private static void VerifyKraidBackdropSurvivesReinitialization(InstalledEnemyArtwork artwork)
    {
        var enemies = new RoomEnemySystem(new EnemySession(new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()), definitions: CompiledRoomEnemyDefinitions.Instance) { TileArtwork = artwork.Enemies, BossArtwork = artwork.Bosses };
        enemies.KraidBehavior.Access.Kraid = new KraidEnemyState();
        var fade = new Func<KraidBackdropFade, bool>(enemies.KraidBehavior.Background.AdvanceKraidRoomBackgroundFade);
        int growthSteps = 0;
        while (!fade(KraidBackdropFade.ToRoomColors))
            AssertTrue(++growthSteps <= KraidShades.BackdropFadeShades, "Kraid's growth fade completes after its shades");
        AssertEqual(SettledBackdrop(KraidBackdropSettledColours.KraidColours), enemies.KraidBehavior.Access.Backdrop.Shows, "Kraid's growth fade ends on his colours");
        // Kraid's body initializer replaces his encounter state; the room's backdrop colours outlive it.
        enemies.KraidBehavior.Access.Kraid = new KraidEnemyState();
        AssertEqual(SettledBackdrop(KraidBackdropSettledColours.KraidColours), enemies.KraidBehavior.Access.Backdrop.Shows,
            "re-initializing Kraid keeps the backdrop his growth fade showed");
        int deathSteps = 0;
        while (!fade(KraidBackdropFade.ToBlack))
            AssertTrue(++deathSteps <= KraidShades.BackdropFadeShades, "Kraid's death fade completes after its shades");
        AssertEqual(SettledBackdrop(KraidBackdropSettledColours.Black), enemies.KraidBehavior.Access.Backdrop.Shows, "Kraid's death fade ends on black");
    }

    /// <summary>
    /// Kraid can die before his growth fades the backdrop in (the growth waits 180 updates after his health falls below six
    /// eighths, and he takes damage meanwhile): his death fade then fades the room's colours to black, as the cartridge's
    /// gradual colour change fades the backdrop's colour memory from whatever it holds (#1532). Each step's shade the import
    /// makes is the room's colours after the cartridge's steps toward black, and the fade ends on black.
    /// </summary>
    private static void VerifyKraidDeathFadeFromRoomColours(InstalledEnemyArtwork artwork)
    {
        KraidBackdropFixture fixture = NewKraidBackdropFixture(artwork);
        RgbColor[] expected = (RgbColor[])fixture.Room.Clone();
        StepKraidBackdropFade(fixture, KraidBackdropFade.ToBlack, expected, fixture.Black, untilCounter: null,
            "Kraid's death fade from the room's colours");
        AssertEqual(SettledBackdrop(KraidBackdropSettledColours.Black), fixture.Enemies.KraidBehavior.Access.Backdrop.Shows,
            "Kraid's death fade from the room's colours ends on black");
    }

    /// <summary>
    /// The fade back toward the room's colours once Kraid's body has sunk can start while the backdrop still shows his
    /// colours (#1512): his growth fade steps through its last value, he dies before it completes, so his death fade
    /// completes without a step and leaves his colours; the death zeroes the counter and the fade back then steps from
    /// his colours, each step showing what the cartridge's gradual colour change computes, and ends on them. His defeated
    /// room, which shows his colours at once, takes the same fade back.
    /// </summary>
    private static void VerifyKraidFadeBackFromKraidColours(InstalledEnemyArtwork artwork)
    {
        KraidBackdropFixture fixture = NewKraidBackdropFixture(artwork);
        RgbColor[] expected = (RgbColor[])fixture.Room.Clone();
        StepKraidBackdropFade(fixture, KraidBackdropFade.ToRoomColors, expected, fixture.Kraid,
            untilCounter: KraidBackgroundDefinitions.BackdropFadeLastStep, "Kraid's growth fade");
        var fade = new Func<KraidBackdropFade, bool>(fixture.Enemies.KraidBehavior.Background.AdvanceKraidRoomBackgroundFade);
        AssertTrue(fade(KraidBackdropFade.ToBlack), "Kraid's death fade after his growth fade's last step completes without a step");
        AssertEqual(SettledBackdrop(KraidBackdropSettledColours.KraidColours), fixture.Enemies.KraidBehavior.Access.Backdrop.Shows,
            "a death fade that took no step leaves Kraid's colours");
        // The death zeroes the shared counter once his body has sunk, before the fade back.
        fixture.Enemies.Presentation.GradualColorChange.Reset();
        StepKraidBackdropFade(fixture, KraidBackdropFade.ToRoomColors, expected, fixture.Kraid, untilCounter: null,
            "Kraid's fade back from his colours");
        AssertEqual(SettledBackdrop(KraidBackdropSettledColours.KraidColours), fixture.Enemies.KraidBehavior.Access.Backdrop.Shows,
            "Kraid's fade back from his colours ends on them");

        KraidBackdropFixture defeated = NewKraidBackdropFixture(artwork);
        defeated.Enemies.KraidBehavior.Access.Backdrop.ShowKraidColours();
        RgbColor[] defeatedExpected = (RgbColor[])defeated.Kraid.Clone();
        StepKraidBackdropFade(defeated, KraidBackdropFade.ToRoomColors, defeatedExpected, defeated.Kraid, untilCounter: null,
            "Kraid's defeated room's fade back");
    }

    /// <summary>
    /// Kraid can die during his growth fade (#1512): his death fade then starts from the growth fade's colours at its last
    /// step and continues its counter, each step showing those colours after the cartridge's gradual colour change toward
    /// black, and ends on black.
    /// </summary>
    private static void VerifyKraidDeathFadeDuringGrowthFade(InstalledEnemyArtwork artwork)
    {
        KraidBackdropFixture fixture = NewKraidBackdropFixture(artwork);
        RgbColor[] expected = (RgbColor[])fixture.Room.Clone();
        StepKraidBackdropFade(fixture, KraidBackdropFade.ToRoomColors, expected, fixture.Kraid, untilCounter: 5, "Kraid's growth fade");
        AssertTrue(fixture.Enemies.KraidBehavior.Access.Backdrop.Shows is KraidBackdropShown.Fading,
            "Kraid's growth fade is partway when he dies");
        StepKraidBackdropFade(fixture, KraidBackdropFade.ToBlack, expected, fixture.Black, untilCounter: null,
            "Kraid's death fade during his growth fade");
        AssertEqual(SettledBackdrop(KraidBackdropSettledColours.Black), fixture.Enemies.KraidBehavior.Access.Backdrop.Shows,
            "Kraid's death fade during his growth fade ends on black");
    }

    /// <summary>
    /// Samus can leave Kraid's room during the fade back toward his colours (#1512; the 13% and 100% movies do): the
    /// door's fade advances the shared counter after each of the backdrop's steps, so the fade back takes every other
    /// value, each step showing what the cartridge's gradual colour change computes at the values taken, and it completes
    /// when the counter passes the last moving value without a step there, keeping its colours partway.
    /// </summary>
    private static void VerifyKraidFadeBackDuringDoorFade(InstalledEnemyArtwork artwork)
    {
        const int denominator = KraidBackgroundDefinitions.BackdropFadeLastStep - 1;
        const int lastAlone = 6;
        KraidBackdropFixture fixture = NewKraidBackdropFixture(artwork);
        RgbColor[] expected = (RgbColor[])fixture.Room.Clone();
        StepKraidBackdropFade(fixture, KraidBackdropFade.ToBlack, expected, fixture.Black, untilCounter: null, "Kraid's death fade");
        fixture.Enemies.Presentation.GradualColorChange.Reset();
        StepKraidBackdropFade(fixture, KraidBackdropFade.ToRoomColors, expected, fixture.Kraid, untilCounter: lastAlone,
            "Kraid's fade back before the door");
        var advance = new Func<KraidBackdropFade, bool>(fixture.Enemies.KraidBehavior.Background.AdvanceKraidRoomBackgroundFade);
        while (true)
        {
            // The door's fade takes its step on the shared counter between the backdrop's.
            fixture.Enemies.Presentation.GradualColorChange.Advance();
            int value = fixture.Enemies.Presentation.GradualColorChange.Numerator;
            if (advance(KraidBackdropFade.ToRoomColors))
            {
                AssertTrue(value > KraidBackgroundDefinitions.BackdropFadeLastStep, "the fade back completes once the counter passes its last moving value");
                AssertTrue(fixture.Enemies.KraidBehavior.Access.Backdrop.Shows is KraidBackdropShown.Fading,
                    "a fade back whose steps skipped the last moving value keeps its colours partway");
                AssertKraidBackdropShows(fixture, expected, "Kraid's fade back completed during the door's fade");
                return;
            }
            AssertTrue(value % 2 == 0, $"the fade back takes every other counter value during the door's fade, not {value}");
            for (int colour = 0; colour < expected.Length; colour++)
                expected[colour] = GradualColourFade.Step(value, expected[colour], fixture.Kraid[colour], denominator);
            AssertKraidBackdropShows(fixture, expected, $"Kraid's fade back during the door's fade at counter {value}");
        }
    }

    /// <summary>A fresh Kraid room's enemies, the import's backdrop shades for a room of distinct colours, and those colours.</summary>
    /// <param name="Enemies">The room's enemies, Kraid loaded.</param>
    /// <param name="Shades">The import's backdrop shades.</param>
    /// <param name="Room">The room's own backdrop colours.</param>
    /// <param name="Kraid">Kraid's backdrop colours.</param>
    /// <param name="Black">Black.</param>
    private sealed record KraidBackdropFixture(RoomEnemySystem Enemies, IReadOnlyList<SheetShade> Shades, RgbColor[] Room, RgbColor[] Kraid, RgbColor[] Black);

    /// <summary>The first ink of a room picture Kraid's backdrop recolours.</summary>
    private const int KraidBackdropFirstInk = 96;

    /// <summary>Loads Kraid into a fresh room and makes the import's backdrop shades for a room whose fifth colour group holds distinct colours.</summary>
    private static KraidBackdropFixture NewKraidBackdropFixture(InstalledEnemyArtwork artwork)
    {
        var enemies = new RoomEnemySystem(new EnemySession(new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()), definitions: CompiledRoomEnemyDefinitions.Instance) { TileArtwork = artwork.Enemies, BossArtwork = artwork.Bosses };
        enemies.KraidBehavior.Access.Kraid = new KraidEnemyState();
        // A room whose fifth colour group holds distinct colours, so every step of a fade moves them.
        var standard = new RgbColor[SpriteImage.SpriteColourCount];
        for (int colour = 0; colour < standard.Length; colour++)
            standard[colour] = new RgbColor(colour % 32, (colour * 7) % 32, (31 - colour % 32));
        KraidColorCatalog kraid = artwork.Bosses.KraidColors ?? throw new InvalidOperationException("The installed artwork has no Kraid colours.");
        var room = new RgbColor[KraidPalettes.BandColors];
        Array.Copy(standard, KraidBackdropFirstInk, room, 0, room.Length);
        RgbColor[] backdrop = [.. Enumerable.Range(0, KraidPalettes.BandColors).Select(colour => kraid.Resolve(KraidPaletteSource.RoomBackdrop, colour))];
        return new KraidBackdropFixture(enemies, KraidBackdropShades.Of(standard, kraid), room, backdrop, new RgbColor[KraidPalettes.BandColors]);
    }

    /// <summary>
    /// Steps a fade of Kraid's backdrop through production, through the step at counter <paramref name="untilCounter"/>
    /// or until it completes, moving <paramref name="expected"/> toward <paramref name="goal"/> by the cartridge's gradual
    /// colour change at each counter value, and checks that what the backdrop shows names an import shade of those colours.
    /// </summary>
    private static void StepKraidBackdropFade(KraidBackdropFixture fixture, KraidBackdropFade fade, RgbColor[] expected, RgbColor[] goal,
        int? untilCounter, string label)
    {
        const int denominator = KraidBackgroundDefinitions.BackdropFadeLastStep - 1;
        var advance = new Func<KraidBackdropFade, bool>(fixture.Enemies.KraidBehavior.Background.AdvanceKraidRoomBackgroundFade);
        int steps = 0;
        while (true)
        {
            int value = fixture.Enemies.Presentation.GradualColorChange.Numerator;
            if (advance(fade))
            {
                AssertTrue(untilCounter is null, $"{label} completes only when stepped to its end");
                AssertTrue(expected.SequenceEqual(goal), $"{label} reaches its goal");
                AssertKraidBackdropShows(fixture, expected, $"{label} completed");
                return;
            }
            for (int colour = 0; colour < expected.Length; colour++)
                expected[colour] = GradualColourFade.Step(value, expected[colour], goal[colour], denominator);
            AssertKraidBackdropShows(fixture, expected, $"{label} at counter {value}");
            AssertTrue(++steps <= KraidShades.BackdropFadeShades, $"{label} completes after its shades");
            if (value == untilCounter)
                return;
        }
    }

    /// <summary>Checks that what Kraid's backdrop shows is <paramref name="expected"/>: the room's own colours, or the import's shade it names.</summary>
    private static void AssertKraidBackdropShows(KraidBackdropFixture fixture, RgbColor[] expected, string label)
    {
        KraidBackdropShown shown = fixture.Enemies.KraidBehavior.Access.Backdrop.Shows;
        RgbColor[] colours = shown == SettledBackdrop(KraidBackdropSettledColours.RoomColours)
            ? fixture.Room
            : fixture.Shades.Single(candidate => candidate.Shade == KraidShades.BackdropShowing(shown)).Colours;
        AssertTrue(colours.SequenceEqual(expected), $"{label} shows the cartridge's colours");
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
        InstalledEnemyArtwork artwork,
        KraidPaletteConsumer consumer)
    {
        var enemies = new RoomEnemySystem(new EnemySession(new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()), definitions: CompiledRoomEnemyDefinitions.Instance) { TileArtwork = artwork.Enemies, BossArtwork = artwork.Bosses };
        var colours = new RgbColor[SnesCgram.ColorCount];
        RoomEnemySlot body = enemies.SlotTable.Slots[0];
        body.Health = 1000;
        var state = new KraidEnemyState();
        // The death setup restarts the arm's typed program (#641), which lives in the room's Kraid state.
        enemies.KraidBehavior.Access.Kraid = state;
        KraidColorCatalog kraid = artwork.Bosses.KraidColors ?? throw new InvalidOperationException("The installed artwork has no Kraid colours.");
        switch (consumer)
        {
            case KraidPaletteConsumer.BackdropLoad:
                // The defeated room shows Kraid's backdrop: the import's shade of it lays his colours over the backdrop's inks.
                enemies.KraidBehavior.Access.Backdrop.ShowKraidColours();
                AssertEqual(SettledBackdrop(KraidBackdropSettledColours.KraidColours), enemies.KraidBehavior.Access.Backdrop.Shows, "Kraid's defeated room shows his backdrop");
                Lay(colours, KraidBackdropShades.Of(new RgbColor[SpriteImage.SpriteColourCount], kraid), KraidShades.Backdrop, 0);
                break;
            case KraidPaletteConsumer.InitialTargetLoad:
                // His rocks draw in the import's shade of his initialization's colours, over the sprite row they draw in.
                Lay(colours, BossSheetShades.SpawnedObjects(artwork.Bosses), KraidShades.Rocks, SpriteImage.SpriteColourCount);
                break;
            case KraidPaletteConsumer.BackdropFade:
                // Step thirteen of the growth fade shows the import's shade of the fade's end: Kraid's colours exactly.
                for (int step = 0; step < 13; step++)
                    enemies.Presentation.GradualColorChange.Advance();
                _ = new Func<KraidBackdropFade, bool>(enemies.KraidBehavior.Background.AdvanceKraidRoomBackgroundFade)(KraidBackdropFade.ToRoomColors);
                AssertEqual(SettledBackdrop(KraidBackdropSettledColours.KraidColours), enemies.KraidBehavior.Access.Backdrop.Shows,
                    "Kraid's growth fade's last step lands on his colours");
                Lay(colours, KraidBackdropShades.Of(new RgbColor[SpriteImage.SpriteColourCount], kraid),
                    KraidShades.BackdropShowing(enemies.KraidBehavior.Access.Backdrop.Shows), 0);
                break;
            case KraidPaletteConsumer.HealthNormal:
            case KraidPaletteConsumer.HealthFlash:
            {
                state.HurtFrame = consumer == KraidPaletteConsumer.HealthFlash
                    ? (ushort)1 : (ushort)0;
                new Action<RoomEnemySlot, KraidEnemyState>(enemies.KraidBehavior.Head.UpdateKraidHealthPalettes)(body, state);
                int band = state.Colours.Band ?? throw new InvalidOperationException("Kraid's health update names no band.");
                AssertEqual(PictureShading.Shaded(KraidShades.Health(band)), body.Shading, $"Kraid's body slot draws in band {band}");
                // His body shows the band's shade over its sixth tile colours, his parts over their sprite row.
                Lay(colours, BossSheetShades.Body(BossBodyOwner.Kraid, artwork.Bosses, [], new RgbColor[SpriteImage.SpriteColourCount]),
                    KraidShades.Health(band), 0);
                Lay(colours, BossSheetShades.OwnRow(artwork.Bosses, EnemyDefinitionId.Kraid, KraidPartsRow * 16), KraidShades.Health(band),
                    SpriteImage.SpriteColourCount);
                // Every body shade draws his eyes in one set of colours, which his eye glow replaces with the band's or the flash's.
                ReadOnlySpan<RgbColor> glow = state.Colours.EyeGlow.Shown;
                for (int eye = 0; eye < glow.Length; eye++)
                    colours[113 + eye] = glow[eye];
                break;
            }
            case KraidPaletteConsumer.EyeUnglow:
            {
                // The glow's raised red and green channels, then one step of the fade: the eye glow service owns the colours.
                state.Colours.EyeGlow.ShowBand([RgbColor.FromWord(0x03ff), RgbColor.FromWord(0x03ff), RgbColor.FromWord(0x03ff)]);
                new Action<RoomEnemySlot, KraidEnemyState>(enemies.KraidBehavior.Head.UnglowKraidEye)(body, state);
                ReadOnlySpan<RgbColor> eyes = state.Colours.EyeGlow.Shown;
                for (int eye = 0; eye < eyes.Length; eye++)
                    colours[113 + eye] = eyes[eye];
                break;
            }
            case KraidPaletteConsumer.DeathArm:
                new Action<RoomEnemySlot, KraidEnemyState>(enemies.KraidBehavior.Death.InitializeKraidDeath)(body, state);
                AssertTrue(state.Colours.ShowsDeathArm, "Kraid's death shows his death colours");
                Lay(colours, BossSheetShades.Body(BossBodyOwner.Kraid, artwork.Bosses, [], new RgbColor[SpriteImage.SpriteColourCount]),
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
