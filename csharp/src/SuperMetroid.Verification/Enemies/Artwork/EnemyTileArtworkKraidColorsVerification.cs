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
        AssertEqual(KraidBackdropShows.KraidColours, enemies.KraidBehavior.Access.Backdrop.Shows, "Kraid's growth fade ends on his colours");
        // Kraid's body initializer replaces his encounter state; the room's backdrop colours outlive it.
        enemies.KraidBehavior.Access.Kraid = new KraidEnemyState();
        AssertEqual(KraidBackdropShows.KraidColours, enemies.KraidBehavior.Access.Backdrop.Shows,
            "re-initializing Kraid keeps the backdrop his growth fade showed");
        int deathSteps = 0;
        while (!fade(KraidBackdropFade.ToBlack))
            AssertTrue(++deathSteps <= KraidShades.BackdropFadeShades, "Kraid's death fade completes after its shades");
        AssertEqual(KraidBackdropShows.Black, enemies.KraidBehavior.Access.Backdrop.Shows, "Kraid's death fade ends on black");
    }

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
                AssertEqual(KraidBackdropShows.KraidColours, enemies.KraidBehavior.Access.Backdrop.Shows, "Kraid's defeated room shows his backdrop");
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
                AssertEqual(KraidBackdropShows.FadingInFromRoomColours, enemies.KraidBehavior.Access.Backdrop.Shows, "Kraid's growth fades his backdrop in");
                Lay(colours, KraidBackdropShades.Of(new RgbColor[SpriteImage.SpriteColourCount], kraid),
                    KraidShades.BackdropFadingInFromRoomColours(enemies.KraidBehavior.Access.Backdrop.Steps), 0);
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
