using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Tooling;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Hardware.ColorWordsVerification;
using static SuperMetroid.Verification.Bosses.Kraid.KraidPaletteDefinitionsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Bosses.Kraid.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Core.World;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Bosses;

namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>Checks installed and edited Kraid RGB5 rows through the palette consumers that write CGRAM.</summary>
internal static class EnemyTileArtworkKraidColorsVerification
{
    /// <summary>Compares every stock color with ROM, checks guarded production writes, and confirms edited colors survive reload.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for installed Kraid palette rows")]
    internal static void VerifyInstalledKraidColors(
        SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace rom, string directory, InstalledEnemyArtwork stock)
    {
        Suite(nameof(VerifyKraidPaletteSourceAddresses), () => VerifyKraidPaletteSourceAddresses(rom));
        Suite(nameof(VerifyKraidPaletteSourceLengths), () => VerifyKraidPaletteSourceLengths());
        KraidColorCatalog catalog = stock.Bosses.KraidColors
            ?? throw new InvalidDataException("Installed enemy artwork lacks Kraid colors.");
        var guardForAllColors = NewKraidPaletteSourceGuard(rom);
        var colorReaderOwner = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance) { TileArtwork = stock.Enemies, BossArtwork = stock.Bosses, AreaArtwork = stock.Areas };
        Func<KraidPaletteSource, int, Bgr555> productionReader = colorReaderOwner.KraidBehavior.Background.ReadKraidColor;
        foreach (KraidPaletteSource source in Enum.GetValues<KraidPaletteSource>())
        {
            int count = KraidPalettes.ColorCount(source);
            byte[] native = RomDataReader.ReadFixedBank(rom,
                KraidPaletteSources.SourceAddress(source), count * sizeof(ushort));
            for (int index = 0; index < count; index++)
                AssertEqual(SnesLittleEndian.Word(native[index * 2], native[index * 2 + 1]),
                    catalog.Resolve(source, index),
                    $"Kraid {source} stock RGB5 color {index}");
            for (int index = 0; index < count; index++)
                AssertEqual(catalog.Resolve(source, index), productionReader(source, index),
                    $"Kraid {source} installed production color {index}");
            AssertThrows<ArgumentOutOfRangeException>(() => catalog.Resolve(source, count),
                $"Kraid {source} rejects adjacent ROM words");
            AssertThrows<ArgumentOutOfRangeException>(() => productionReader(source, count),
                $"Kraid {source} production lookup rejects adjacent ROM words");
        }
        AssertEqual(0, guardForAllColors.ForbiddenReadAttempts,
            "all installed Kraid palette rows avoid their ROM sources");

        foreach (KraidPaletteConsumer consumer in Enum.GetValues<KraidPaletteConsumer>())
        {
            ushort[] expected = ReferenceKraidPaletteConsumer(rom, consumer);
            var guard = NewKraidPaletteSourceGuard(rom);
            Bgr555[] actual = CaptureKraidPaletteConsumer(stock, consumer);
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
        InstalledEnemyArtwork edited = EnemyTileArtworkFiles.Load(directory, overrides);
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
            Bgr555[] changed = CaptureKraidPaletteConsumer(edited, consumer);
            AssertEqual(0, guard.ForbiddenReadAttempts,
                $"edited Kraid {consumer} avoids palette ROM reads");
            AssertEqual((ushort)(baseline[cgramIndex] ^ 1), changed[cgramIndex],
                $"edited Kraid {consumer} changes CGRAM {cgramIndex}");
        }
        InstalledEnemyArtwork reloaded = EnemyTileArtworkFiles.Load(directory, overrides);
        AssertTrue(CaptureKraidPaletteConsumer(reloaded,
                KraidPaletteConsumer.HealthNormal)
            .SequenceEqual(CaptureKraidPaletteConsumer(edited, KraidPaletteConsumer.HealthNormal)),
            "Kraid RGB5 override survives catalog reload");

        File.WriteAllBytes(stockPath, [0]);
        AssertThrows<InvalidDataException>(() => EnemyTileArtworkFiles.Load(directory, null),
            "tampered stock Kraid color file fails manifest hash");
        File.WriteAllBytes(stockPath, original);
        File.WriteAllBytes(overridePath, [0]);
        AssertThrows<InvalidDataException>(() => EnemyTileArtworkFiles.Load(directory, overrides),
            "malformed Kraid color override fails at load");
        PaletteRgb5[] invalidHealth = (PaletteRgb5[])editedDocument.Health.Clone();
        invalidHealth[0] = invalidHealth[0] with { Red = 32 };
        AssertThrows<InvalidDataException>(() => KraidColorFile.Write(
                editedDocument with { Health = invalidHealth }),
            "Kraid RGB5 channels outside native precision are rejected");
    }

    /// <summary>Builds an independent CGRAM reference from pinned sm_a7.c palette writes; zero thresholds select health band eight at 1000 HP and odd hurt frames select flash.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for the Kraid palette consumer reference")]
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

    /// <summary>Runs one Kraid palette consumer and captures the resulting complete CGRAM image.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    private static Bgr555[] CaptureKraidPaletteConsumer(
        InstalledEnemyArtwork artwork,
        KraidPaletteConsumer consumer)
    {
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance) { TileArtwork = artwork.Enemies, BossArtwork = artwork.Bosses, AreaArtwork = artwork.Areas };
        var cgram = new SamusColourRig();
        enemies.Presentation.BindColours(cgram.Gameplay);
        RoomEnemySlot body = enemies.SlotTable.Slots[0];
        body.Health = 1000;
        var state = new KraidEnemyState();
        // The death setup restarts the arm's typed program (#641), which lives in the room's Kraid state.
        enemies.KraidBehavior.Access.Kraid = state;
        switch (consumer)
        {
            case KraidPaletteConsumer.BackdropLoad:
            case KraidPaletteConsumer.InitialTargetLoad:
                new Action<KraidPaletteSource, ColourRun>(enemies.KraidBehavior.Rise.LoadKraidColorBand)(
                        consumer == KraidPaletteConsumer.BackdropLoad
                            ? KraidPaletteSource.RoomBackdrop
                            : KraidPaletteSource.InitialTarget,
                        consumer == KraidPaletteConsumer.BackdropLoad
                            ? new BossColours(cgram.Gameplay).KraidRoomBackdrop
                            : new BossColours(cgram.Gameplay).KraidSprites);
                break;
            case KraidPaletteConsumer.BackdropFade:
                for (int step = 0; step < 13; step++)
                    enemies.Presentation.GradualColorChange.Advance();
                _ = new Func<KraidBackdropFade, bool>(enemies.KraidBehavior.Background.AdvanceKraidRoomBackgroundFade)(KraidBackdropFade.ToRoomColors);
                break;
            case KraidPaletteConsumer.HealthNormal:
            case KraidPaletteConsumer.HealthFlash:
                state.HurtFrame = consumer == KraidPaletteConsumer.HealthFlash
                    ? (ushort)1 : (ushort)0;
                new Action<RoomEnemySlot, KraidEnemyState>(enemies.KraidBehavior.Head.UpdateKraidHealthPalettes)(body, state);
                break;
            case KraidPaletteConsumer.EyeUnglow:
                for (int eye = 0; eye < 3; eye++)
                    cgram.SetColor(113 + eye, Bgr555.FromWord(0x03ff));
                new Action<RoomEnemySlot, KraidEnemyState>(enemies.KraidBehavior.Head.UnglowKraidEye)(body, state);
                break;
            case KraidPaletteConsumer.DeathArm:
                new Action<RoomEnemySlot, KraidEnemyState>(enemies.KraidBehavior.Death.InitializeKraidDeath)(body, state);
                break;
            default:
                throw new InvalidOperationException($"Unknown Kraid palette consumer {consumer}.");
        }
        return cgram.Colors.ToArray();
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
