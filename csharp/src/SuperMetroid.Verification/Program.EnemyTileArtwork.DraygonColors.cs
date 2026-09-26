using System.Reflection;
using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyInstalledDraygonColors(
        ISnesAddressSpace rom, string stockDirectory, EnemyTileArtworkCatalog stock)
    {
        DraygonColorCatalog native = stock.DraygonColors ??
            throw new InvalidDataException("Installed enemy art has no Draygon colors.");
        VerifyBand(DraygonColorRomData.IntroSource, DraygonColorRomData.IntroCount,
            native.ResolveIntro, "intro");
        VerifyBand(DraygonColorRomData.BackgroundSource,
            DraygonColorRomData.BackgroundCount, native.ResolveBackground, "background");
        VerifyBand(DraygonColorRomData.SpriteSource,
            DraygonColorRomData.SpriteCount, native.ResolveSprite, "sprite");
        VerifyBand(DraygonColorRomData.WhiteFlashSource,
            DraygonColorRomData.WhiteFlashCount, native.ResolveWhiteFlash, "white flash");
        for (int band = 0; band < DraygonColorRomData.HealthBandCount; band++)
        for (int color = 0; color < DraygonColorRomData.HealthBandColorCount; color++)
            AssertEqual(RomDataReader.ReadWordFixedBank(rom,
                    DraygonColorRomData.HealthBandsSource +
                    (band * DraygonColorRomData.HealthBandColorCount + color) * sizeof(ushort)),
                native.ResolveHealthBand(band, color),
                $"installed Draygon health band {band} color {color} preserves native RGB5");

        string file = Path.Combine(stockDirectory, DraygonColorFormat.FileName);
        byte[] stockJson = File.ReadAllBytes(file);
        DraygonColorDocument visual = JsonSerializer.Deserialize<DraygonColorDocument>(
            stockJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ??
            throw new InvalidDataException("Stock Draygon color JSON is null.");
        visual.Intro[1] = ChangeRed(visual.Intro[1]);
        visual.Background[1] = ChangeRed(visual.Background[1]);
        visual.Sprite[1] = ChangeRed(visual.Sprite[1]);
        visual.WhiteFlash[1] = ChangeRed(visual.WhiteFlash[1]);
        visual.HealthBands[1][1] = ChangeRed(visual.HealthBands[1][1]);
        string overrides = Path.Combine(stockDirectory, "draygon-color-overrides");
        Directory.CreateDirectory(overrides);
        string overrideFile = Path.Combine(overrides, DraygonColorFormat.FileName);
        File.WriteAllBytes(overrideFile, DraygonColorCatalog.Write(visual));
        EnemyTileArtworkCatalog edited = EnemyTileArtworkFiles.Load(stockDirectory, overrides);
        DraygonColorCatalog colors = edited.DraygonColors ??
            throw new InvalidDataException("Edited enemy art has no Draygon colors.");
        CheckEditedBand(DraygonColorRomData.IntroCount,
            native.ResolveIntro, colors.ResolveIntro, "intro");
        CheckEditedBand(DraygonColorRomData.BackgroundCount,
            native.ResolveBackground, colors.ResolveBackground, "background");
        CheckEditedBand(DraygonColorRomData.SpriteCount,
            native.ResolveSprite, colors.ResolveSprite, "sprite");
        CheckEditedBand(DraygonColorRomData.WhiteFlashCount,
            native.ResolveWhiteFlash, colors.ResolveWhiteFlash, "white flash");
        for (int band = 0; band < DraygonColorRomData.HealthBandCount; band++)
        for (int color = 0; color < DraygonColorRomData.HealthBandColorCount; color++)
            if (band == 1 && color == 1)
                AssertTrue(native.ResolveHealthBand(band, color) !=
                    colors.ResolveHealthBand(band, color),
                    "Draygon edited health-band color changes");
            else
                AssertEqual(native.ResolveHealthBand(band, color),
                    colors.ResolveHealthBand(band, color),
                    $"Draygon health band {band} color {color} remains unchanged");
        AssertEqual(colors.ResolveHealthBand(1, 1),
            EnemyTileArtworkFiles.Load(stockDirectory, overrides)
                .DraygonColors!.ResolveHealthBand(1, 1),
            "Draygon color override survives catalog reload");

        // Invoke the actual body initializer and hurt program with the visual source
        // windows forbidden. Only the resource may supply colors during installed play.
        var guarded = new DraygonPaletteReadGuard(rom);
        var cgram = new SnesCgram();
        var enemies = new RoomEnemySystem { TileArtwork = edited };
        BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        Type type = typeof(RoomEnemySystem);
        type.GetField("_bus", flags)!.SetValue(enemies, guarded);
        type.GetField("_cgram", flags)!.SetValue(enemies, cgram);
        type.GetField("_vram", flags)!.SetValue(enemies, new SnesVram());
        RoomEnemySlot body = enemies.Slots[0];
        type.GetMethod("InitializeDraygonBody", flags)!.Invoke(enemies, [body]);
        DraygonEnemyState state = enemies.Draygon ??
            throw new InvalidOperationException("Draygon body initialization lost its owner.");
        AssertEqual(DraygonAiFunction.IntroInitialDelay, state.Function,
            "installed Draygon colors leave intro mechanics unchanged");
        AssertBand(DraygonColorRomData.IntroDestination,
            DraygonColorRomData.IntroCount, colors.ResolveIntro, "intro CGRAM");

        state.HealthPaletteTableByteIndex = 2;
        MethodInfo hurt = type.GetMethod("ApplyDraygonHurt", flags)!;
        hurt.Invoke(enemies, [body, state, null]);
        for (int color = 0; color < DraygonColorRomData.BackgroundCount; color++)
        {
            ushort expected = color is >= 9 and <= 12
                ? colors.ResolveHealthBand(1, color - 9)
                : colors.ResolveBackground(color);
            AssertEqual(expected,
                cgram.Colors[DraygonColorRomData.BackgroundDestination + color],
                $"Draygon hurt background color {color}");
        }
        AssertBand(DraygonColorRomData.SpriteDestination,
            DraygonColorRomData.SpriteCount, colors.ResolveSprite,
            "normal hurt sprite CGRAM");
        body.FlashTimer = 2;
        hurt.Invoke(enemies, [body, state, null]);
        AssertBand(DraygonColorRomData.BackgroundDestination,
            DraygonColorRomData.WhiteFlashCount, colors.ResolveWhiteFlash,
            "white hurt background CGRAM");
        AssertBand(DraygonColorRomData.SpriteDestination,
            DraygonColorRomData.WhiteFlashCount, colors.ResolveWhiteFlash,
            "white hurt sprite CGRAM");
        MethodInfo health = type.GetMethod("CopyDraygonHealthColors", flags)!;
        for (int band = 0; band < DraygonColorRomData.HealthBandCount; band++)
        {
            state.HealthPaletteTableByteIndex = unchecked((ushort)(band * sizeof(ushort)));
            health.Invoke(enemies, [state]);
            for (int color = 0; color < DraygonColorRomData.HealthBandColorCount; color++)
                AssertEqual(colors.ResolveHealthBand(band, color),
                    cgram.Colors[DraygonColorRomData.HealthDestination + color],
                    $"Draygon live health band {band} color {color}");
        }
        AssertEqual(DraygonAiFunction.IntroInitialDelay, state.Function,
            "visual health-band checks leave Draygon AI unchanged");
        AssertThrows<InvalidDataException>(() => colors.ApplyHealthBand(cgram, 1),
            "odd Draygon health-band index fails loudly");
        AssertThrows<InvalidDataException>(() => colors.ApplyHealthBand(cgram, 16),
            "out-of-range Draygon health-band index fails loudly");

        File.WriteAllBytes(overrideFile, [0]);
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(stockDirectory, overrides),
            "invalid Draygon color override fails loudly");
        File.WriteAllBytes(overrideFile,
            "{\"version\":1,\"version\":1}"u8.ToArray());
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(stockDirectory, overrides),
            "duplicate Draygon color property fails loudly");
        visual.HealthBands[1][1] = visual.HealthBands[1][1] with { Blue = 32 };
        AssertThrows<InvalidDataException>(() => DraygonColorCatalog.Write(visual),
            "Draygon RGB5 channel outside five-bit precision is rejected");
        File.Delete(overrideFile);
        File.WriteAllBytes(file, [0]);
        AssertThrows<InvalidDataException>(() => EnemyTileArtworkFiles.Load(stockDirectory, null),
            "corrupt stock Draygon colors fail manifest hash validation");
        File.WriteAllBytes(file, stockJson);
        Console.WriteLine("  Draygon colors: 105 native RGB5 words, live intro/hurt/flash/eight health bands, persistent override, ROM guard, and strict failures pass.");

        void VerifyBand(int source, int count, Func<int, ushort> resolve, string name)
        {
            for (int color = 0; color < count; color++)
                AssertEqual(RomDataReader.ReadWordFixedBank(rom,
                    source + color * sizeof(ushort)), resolve(color),
                    $"installed Draygon {name} color {color} preserves native RGB5");
        }

        void AssertBand(int destination, int count, Func<int, ushort> resolve, string name)
        {
            for (int color = 0; color < count; color++)
                AssertEqual(resolve(color), cgram.Colors[destination + color],
                    $"installed Draygon {name} color {color}");
        }

        static PaletteRgb5 ChangeRed(PaletteRgb5 rgb) => rgb with
        {
            Red = rgb.Red == 31 ? 30 : rgb.Red + 1,
        };

        static void CheckEditedBand(int count, Func<int, ushort> original,
            Func<int, ushort> changed, string name)
        {
            AssertTrue(original(1) != changed(1),
                $"Draygon {name} edit changes selected color");
            for (int color = 0; color < count; color++)
                if (color != 1)
                    AssertEqual(original(color), changed(color),
                        $"Draygon {name} edit leaves color {color} unchanged");
        }
    }

    private sealed class DraygonPaletteReadGuard(ISnesAddressSpace source)
        : ISnesAddressSpace
    {
        public byte ReadByte(int address)
        {
            bool healthTable = address >= DraygonColorRomData.HealthBandsSource &&
                address < DraygonColorRomData.HealthBandsSource +
                    DraygonColorRomData.HealthBandCount *
                    DraygonColorRomData.HealthBandColorCount * sizeof(ushort);
            bool palettes = address >= DraygonColorRomData.SpriteSource &&
                address < DraygonColorRomData.WhiteFlashSource +
                    DraygonColorRomData.WhiteFlashCount * sizeof(ushort);
            if (healthTable || palettes)
                throw new InvalidOperationException(
                    $"Draygon accessed migrated palette ROM ${address:X6}.");
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
