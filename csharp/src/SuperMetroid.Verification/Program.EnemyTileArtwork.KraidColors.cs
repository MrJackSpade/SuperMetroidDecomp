using System.Reflection;
using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyInstalledKraidColors(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace rom, string directory, EnemyTileArtworkCatalog stock)
    {
        Suite(nameof(VerifyKraidPaletteSourceAddresses), () => VerifyKraidPaletteSourceAddresses(rom));
        Suite(nameof(VerifyKraidPaletteSourceLengths), () => VerifyKraidPaletteSourceLengths());
        KraidColorCatalog catalog = stock.KraidColors
            ?? throw new InvalidDataException("Installed enemy artwork lacks Kraid colors.");
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var guardForAllColors = new KraidPaletteSourceGuard(rom);
        var colorReaderOwner = new RoomEnemySystem { TileArtwork = stock };
        typeof(RoomEnemySystem).GetField("_bus", flags)!
            .SetValue(colorReaderOwner, guardForAllColors);
        var productionReader = typeof(RoomEnemySystem)
            .GetMethod("ReadKraidColor", flags)!
            .CreateDelegate<Func<KraidPaletteSource, int, ushort>>(colorReaderOwner);
        foreach (KraidPaletteSource source in Enum.GetValues<KraidPaletteSource>())
        {
            int count = KraidPaletteRomData.ColorCount(source);
            byte[] native = RomDataReader.ReadFixedBank(rom,
                KraidPaletteRomData.SourceAddress(source), count * sizeof(ushort));
            for (int index = 0; index < count; index++)
                AssertEqual((ushort)(native[index * 2] | native[index * 2 + 1] << 8),
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
            var guard = new KraidPaletteSourceGuard(rom);
            ushort[] actual = CaptureKraidPaletteConsumer(guard, stock, consumer);
            AssertEqual(0, guard.ForbiddenReadAttempts,
                $"installed Kraid {consumer} avoids all palette ROM sources");
            AssertTrue(actual.SequenceEqual(expected),
                $"installed Kraid {consumer} preserves full native CGRAM state");
        }

        string stockPath = Path.Combine(directory, KraidColorFormat.FileName);
        byte[] original = File.ReadAllBytes(stockPath);
        KraidColorDocument document = JsonSerializer.Deserialize<KraidColorDocument>(
            original, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
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
        File.WriteAllBytes(overridePath, KraidColorCatalog.Write(editedDocument));
        EnemyTileArtworkCatalog edited = EnemyTileArtworkFiles.Load(directory, overrides);
        foreach ((KraidPaletteConsumer consumer, int cgramIndex) in new[]
        {
            (KraidPaletteConsumer.BackdropLoad, 96),
            (KraidPaletteConsumer.InitialTargetLoad, 176),
            (KraidPaletteConsumer.BackdropFade, 96),
            (KraidPaletteConsumer.HealthNormal, 112),
            (KraidPaletteConsumer.HealthNormal, 240),
            (KraidPaletteConsumer.DeathArm, 112),
        })
        {
            ushort[] baseline = ReferenceKraidPaletteConsumer(rom, consumer);
            var guard = new KraidPaletteSourceGuard(rom);
            ushort[] changed = CaptureKraidPaletteConsumer(guard, edited, consumer);
            AssertEqual(0, guard.ForbiddenReadAttempts,
                $"edited Kraid {consumer} avoids palette ROM reads");
            AssertEqual((ushort)(baseline[cgramIndex] ^ 1), changed[cgramIndex],
                $"edited Kraid {consumer} changes CGRAM {cgramIndex}");
        }
        EnemyTileArtworkCatalog reloaded = EnemyTileArtworkFiles.Load(directory, overrides);
        AssertTrue(CaptureKraidPaletteConsumer(new KraidPaletteSourceGuard(rom), reloaded,
                KraidPaletteConsumer.HealthNormal)
            .SequenceEqual(CaptureKraidPaletteConsumer(new KraidPaletteSourceGuard(rom),
                edited, KraidPaletteConsumer.HealthNormal)),
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
        AssertThrows<InvalidDataException>(() => KraidColorCatalog.Write(
                editedDocument with { Health = invalidHealth }),
            "Kraid RGB5 channels outside native precision are rejected");
    }

    // Independent palette writes from pinned sm_a7.c. This fixture's zero health
    // thresholds select band eight at 1000 HP; odd hurt frames select the flash band.
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
                    ushort target = Word(0xa7b3d3 + 256 + (eye + 1) * 2);
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

        ushort Word(int address) =>
            (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);

        void Copy(int source, int destination)
        {
            for (int color = 0; color < 16; color++)
                colors[destination + color] = Word(source + color * 2);
        }
    }

    private static ushort[] CaptureKraidPaletteConsumer(
        ISnesAddressSpace bus, EnemyTileArtworkCatalog artwork,
        KraidPaletteConsumer consumer)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem { TileArtwork = artwork };
        var cgram = new SnesCgram();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, bus);
        typeof(RoomEnemySystem).GetField("_cgram", flags)!.SetValue(enemies, cgram);
        RoomEnemySlot body = enemies.Slots[0];
        body.Health = 1000;
        var state = new KraidEnemyState();
        switch (consumer)
        {
            case KraidPaletteConsumer.BackdropLoad:
            case KraidPaletteConsumer.InitialTargetLoad:
                typeof(RoomEnemySystem).GetMethod("LoadKraidColorBand", flags)!
                    .CreateDelegate<Action<KraidPaletteSource, int>>(enemies)(
                        consumer == KraidPaletteConsumer.BackdropLoad
                            ? KraidPaletteSource.RoomBackdrop
                            : KraidPaletteSource.InitialTarget,
                        consumer == KraidPaletteConsumer.BackdropLoad ? 96 : 176);
                break;
            case KraidPaletteConsumer.BackdropFade:
                state.RoomBackgroundFadeStep = 13;
                _ = typeof(RoomEnemySystem).GetMethod("AdvanceKraidRoomBackgroundFade", flags)!
                    .CreateDelegate<Func<KraidEnemyState, bool, bool>>(enemies)(state, false);
                break;
            case KraidPaletteConsumer.HealthNormal:
            case KraidPaletteConsumer.HealthFlash:
                state.HurtFrame = consumer == KraidPaletteConsumer.HealthFlash
                    ? (ushort)1 : (ushort)0;
                typeof(RoomEnemySystem).GetMethod("UpdateKraidHealthPalettes", flags)!
                    .CreateDelegate<Action<RoomEnemySlot, KraidEnemyState>>(enemies)(body, state);
                break;
            case KraidPaletteConsumer.EyeUnglow:
                for (int eye = 0; eye < 3; eye++)
                    cgram.SetColor(113 + eye, 0x03ff);
                typeof(RoomEnemySystem).GetMethod("UnglowKraidEye", flags)!
                    .CreateDelegate<Action<RoomEnemySlot, KraidEnemyState>>(enemies)(body, state);
                break;
            case KraidPaletteConsumer.DeathArm:
                typeof(RoomEnemySystem).GetMethod("InitializeKraidDeath", flags)!
                    .CreateDelegate<Action<RoomEnemySlot, KraidEnemyState>>(enemies)(body, state);
                break;
            default:
                throw new InvalidOperationException($"Unknown Kraid palette consumer {consumer}.");
        }
        return cgram.Colors.ToArray();
    }

    private enum KraidPaletteConsumer
    {
        BackdropLoad,
        InitialTargetLoad,
        BackdropFade,
        HealthNormal,
        HealthFlash,
        EyeUnglow,
        DeathArm,
    }

    private sealed class KraidPaletteSourceGuard(ISnesAddressSpace source) : ISnesAddressSpace, IImportCartridgeSource
    {
        public int ForbiddenReadAttempts { get; private set; }

        public byte ReadCartridgeByte(int address) => ReadByte(address);

        public byte ReadByte(int address)
        {
            foreach (KraidPaletteSource palette in Enum.GetValues<KraidPaletteSource>())
            {
                int start = KraidPaletteRomData.SourceAddress(palette);
                if (address < start || address >= start +
                        KraidPaletteRomData.ColorCount(palette) * sizeof(ushort))
                    continue;
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Kraid {palette} reread palette ROM byte ${address:X6}.");
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
