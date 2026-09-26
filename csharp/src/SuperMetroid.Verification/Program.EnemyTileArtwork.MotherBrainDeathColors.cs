using System.Reflection;
using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyInstalledMotherBrainDeathColors(
        ISnesAddressSpace rom, string stockDirectory, EnemyTileArtworkCatalog stock)
    {
        MotherBrainDeathColorCatalog native = stock.MotherBrainDeathColors ??
            throw new InvalidDataException("Installed enemy art has no Mother Brain death colors.");
        for (int frame = 0; frame < MotherBrainDeathRomData.BodyFadeFrameCount; frame++)
        {
            int source = MotherBrainDeathRomData.BodyFadeSource(frame);
            AssertEqual(unchecked((ushort)source),
                RomDataReader.ReadWordFixedBank(rom,
                    MotherBrainDeathRomData.BodyFadeTable + frame * sizeof(ushort)),
                $"native Mother Brain body death-fade selector {frame}");
            for (int color = 0; color < MotherBrainDeathRomData.BodyColorCount; color++)
            {
                AssertEqual(RomDataReader.ReadWordFixedBank(rom, source + color * 2),
                    native.BodyColor(frame, color),
                    $"installed Mother Brain body death frame {frame} color {color}");
                AssertEqual(RomDataReader.ReadWordFixedBank(rom,
                        source + (MotherBrainDeathRomData.BodyColorCount + color) * 2),
                    native.LegColor(frame, color),
                    $"installed Mother Brain leg death frame {frame} color {color}");
            }
        }
        for (int frame = 0; frame < MotherBrainDeathRomData.CorpseFadeFrameCount; frame++)
        {
            int source = MotherBrainDeathRomData.CorpseFadeSource(frame);
            AssertEqual(unchecked((ushort)source),
                RomDataReader.ReadWordFixedBank(rom,
                    MotherBrainDeathRomData.CorpseFadeTable + frame * sizeof(ushort)),
                $"native Mother Brain corpse death-fade selector {frame}");
            for (int color = 0; color < MotherBrainDeathRomData.CorpseColorCount; color++)
                AssertEqual(RomDataReader.ReadWordFixedBank(rom, source + color * 2),
                    native.CorpseColor(frame, color),
                    $"installed Mother Brain corpse death frame {frame} color {color}");
        }
        for (int color = 0; color < MotherBrainDeathRomData.BodyColorCount; color++)
            AssertEqual(RomDataReader.ReadWordFixedBank(rom,
                    MotherBrainDeathRomData.DoorPalette + color * 2),
                native.ExplodedDoorColor(color),
                $"installed Mother Brain exploded-door color {color}");

        string file = Path.Combine(stockDirectory, MotherBrainDeathColorFormat.FileName);
        byte[] stockJson = File.ReadAllBytes(file);
        MotherBrainDeathColorDocument visual =
            JsonSerializer.Deserialize<MotherBrainDeathColorDocument>(stockJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ??
            throw new InvalidDataException("Stock Mother Brain death color JSON is null.");
        foreach (PaletteRgb5[] frame in visual.BodyFade)
            frame[5] = ChangeRed(frame[5]);
        foreach (PaletteRgb5[] frame in visual.LegFade)
            frame[5] = ChangeRed(frame[5]);
        foreach (PaletteRgb5[] frame in visual.CorpseFade)
            frame[5] = ChangeRed(frame[5]);
        visual.ExplodedDoor[5] = ChangeRed(visual.ExplodedDoor[5]);
        string overrides = Path.Combine(stockDirectory,
            "mother-brain-death-color-overrides");
        Directory.CreateDirectory(overrides);
        string overrideFile = Path.Combine(overrides,
            MotherBrainDeathColorFormat.FileName);
        File.WriteAllBytes(overrideFile, MotherBrainDeathColorCatalog.Write(visual));
        EnemyTileArtworkCatalog edited = EnemyTileArtworkFiles.Load(stockDirectory, overrides);
        MotherBrainDeathColorCatalog colors = edited.MotherBrainDeathColors ??
            throw new InvalidDataException("Edited enemy art has no Mother Brain death colors.");
        for (int frame = 0; frame < MotherBrainDeathRomData.BodyFadeFrameCount; frame++)
        {
            AssertTrue(native.BodyColor(frame, 5) != colors.BodyColor(frame, 5),
                $"Mother Brain body death frame {frame} edit takes effect");
            AssertTrue(native.LegColor(frame, 5) != colors.LegColor(frame, 5),
                $"Mother Brain leg death frame {frame} edit takes effect");
            AssertEqual(native.BodyColor(frame, 4), colors.BodyColor(frame, 4),
                $"Mother Brain body death frame {frame} adjacent color stays native");
        }
        for (int frame = 0; frame < MotherBrainDeathRomData.CorpseFadeFrameCount; frame++)
            AssertTrue(native.CorpseColor(frame, 5) != colors.CorpseColor(frame, 5),
                $"Mother Brain corpse death frame {frame} edit takes effect");
        AssertTrue(native.ExplodedDoorColor(5) != colors.ExplodedDoorColor(5),
            "Mother Brain exploded-door edit takes effect");
        AssertEqual(colors.CorpseColor(7, 5),
            EnemyTileArtworkFiles.Load(stockDirectory, overrides)
                .MotherBrainDeathColors!.CorpseColor(7, 5),
            "Mother Brain death color override survives catalog reload");

        var guard = new MotherBrainDeathColorReadGuard(rom);
        var cgram = new SnesCgram();
        var enemies = new RoomEnemySystem { TileArtwork = edited };
        BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        Type type = typeof(RoomEnemySystem);
        type.GetField("_bus", flags)!.SetValue(enemies, guard);
        type.GetField("_cgram", flags)!.SetValue(enemies, cgram);
        MethodInfo body = type.GetMethod("LoadMotherBrainDeathBodyFade", flags)!;
        MethodInfo corpse = type.GetMethod("LoadMotherBrainDeathCorpseFade", flags)!;
        MethodInfo door = type.GetMethod("LoadMotherBrainDeathDoorPalette", flags)!;
        cgram.SetColor(MotherBrainDeathRomData.BodyColors - 1, 0x1234);
        cgram.SetColor(MotherBrainDeathRomData.BrainColors - 1, 0x1234);
        cgram.SetColor(MotherBrainDeathRomData.LegColors - 1, 0x1234);
        cgram.SetColor(MotherBrainDeathRomData.CorpseColors - 1, 0x1234);
        for (int frame = 0; frame < MotherBrainDeathRomData.BodyFadeFrameCount; frame++)
        {
            body.Invoke(enemies, [frame]);
            for (int color = 0; color < MotherBrainDeathRomData.BodyColorCount; color++)
            {
                ushort shared = colors.BodyColor(frame, color);
                AssertEqual(shared, cgram.Colors[MotherBrainDeathRomData.BodyColors + color],
                    $"live Mother Brain body death frame {frame} color {color}");
                AssertEqual(shared, cgram.Colors[MotherBrainDeathRomData.BrainColors + color],
                    $"live Mother Brain brain death frame {frame} color {color}");
                AssertEqual(colors.LegColor(frame, color),
                    cgram.Colors[MotherBrainDeathRomData.LegColors + color],
                    $"live Mother Brain leg death frame {frame} color {color}");
            }
        }
        for (int frame = 0; frame < MotherBrainDeathRomData.CorpseFadeFrameCount; frame++)
        {
            corpse.Invoke(enemies, [frame]);
            for (int color = 0; color < MotherBrainDeathRomData.CorpseColorCount; color++)
                AssertEqual(colors.CorpseColor(frame, color),
                    cgram.Colors[MotherBrainDeathRomData.CorpseColors + color],
                    $"live Mother Brain corpse death frame {frame} color {color}");
        }
        door.Invoke(enemies, null);
        for (int color = 0; color < MotherBrainDeathRomData.BodyColorCount; color++)
            AssertEqual(colors.ExplodedDoorColor(color),
                cgram.Colors[MotherBrainDeathRomData.BrainColors + color],
                $"live Mother Brain exploded-door color {color}");
        foreach (int destination in new[] { MotherBrainDeathRomData.BodyColors,
            MotherBrainDeathRomData.BrainColors, MotherBrainDeathRomData.LegColors,
            MotherBrainDeathRomData.CorpseColors })
            AssertEqual((ushort)0x1234, cgram.Colors[destination - 1],
                $"Mother Brain death palette preserves adjacent color {destination - 1}");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "live Mother Brain death palettes avoid migrated RGB5/selector ROM reads");

        File.WriteAllBytes(overrideFile, [0]);
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(stockDirectory, overrides),
            "invalid Mother Brain death override fails loudly");
        File.WriteAllBytes(overrideFile,
            "{\"version\":1,\"version\":1}"u8.ToArray());
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(stockDirectory, overrides),
            "duplicate Mother Brain death property fails loudly");
        visual.BodyFade[0][5] = visual.BodyFade[0][5] with { Blue = 32 };
        AssertThrows<InvalidDataException>(
            () => MotherBrainDeathColorCatalog.Write(visual),
            "Mother Brain death RGB5 channel outside five-bit precision is rejected");
        File.Delete(overrideFile);
        File.WriteAllBytes(file, [0]);
        AssertThrows<InvalidDataException>(() => EnemyTileArtworkFiles.Load(stockDirectory, null),
            "corrupt stock Mother Brain death colors fail manifest hash validation");
        File.WriteAllBytes(file, stockJson);
        Console.WriteLine("  Mother Brain death colors: 582 native RGB5 words, 24 selectors, live body/leg/corpse/door CGRAM, persistent override, ROM guard and strict failures pass.");

        static PaletteRgb5 ChangeRed(PaletteRgb5 rgb) => rgb with
        {
            Red = rgb.Red == 31 ? 30 : rgb.Red + 1,
        };
    }

    private sealed class MotherBrainDeathColorReadGuard(ISnesAddressSpace source)
        : ISnesAddressSpace
    {
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            bool body = address >= MotherBrainDeathRomData.BodyFadeTable &&
                address < MotherBrainDeathRomData.BodyFadeSource(
                    MotherBrainDeathRomData.BodyFadeFrameCount - 1) +
                    MotherBrainDeathRomData.BodyColorCount * 2 * sizeof(ushort);
            bool corpse = address >= MotherBrainDeathRomData.CorpseFadeTable &&
                address < MotherBrainDeathRomData.CorpseFadeSource(
                    MotherBrainDeathRomData.CorpseFadeFrameCount - 1) +
                    MotherBrainDeathRomData.CorpseColorCount * sizeof(ushort);
            bool door = address >= MotherBrainDeathRomData.DoorPalette &&
                address < MotherBrainDeathRomData.DoorPalette +
                    MotherBrainDeathRomData.BodyColorCount * sizeof(ushort);
            if (body || corpse || door)
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Live Mother Brain death read migrated ROM ${address:X6}.");
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
