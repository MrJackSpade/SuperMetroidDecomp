using System.Reflection;
using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyInstalledPhantoonColors(
        ISnesAddressSpace rom, string stockDirectory, EnemyTileArtworkCatalog stock)
    {
        PhantoonColorCatalog native = stock.PhantoonColors ??
            throw new InvalidDataException("Installed enemy art has no Phantoon colors.");
        VerifyBand(PhantoonColorRomData.FadeOutSource,
            PhantoonColorRomData.FadeOutCount, native.ResolveFadeOut, "fade-out");
        VerifyBand(PhantoonColorRomData.PowerOnSource,
            PhantoonColorRomData.PowerOnCount, native.ResolvePowerOn, "power-on");
        for (int band = 0; band < PhantoonColorRomData.HealthBandCount; band++)
        for (int color = 0; color < PhantoonColorRomData.HealthBandColorCount; color++)
            AssertEqual(RomDataReader.ReadWordFixedBank(rom,
                    PhantoonColorRomData.HealthBandsSource +
                    (band * PhantoonColorRomData.HealthBandColorCount + color) *
                        sizeof(ushort)),
                native.ResolveHealth(band, color),
                $"installed Phantoon health palette {band} color {color} preserves RGB5");

        string file = Path.Combine(stockDirectory, PhantoonColorFormat.FileName);
        byte[] stockJson = File.ReadAllBytes(file);
        PhantoonColorDocument visual = JsonSerializer.Deserialize<PhantoonColorDocument>(
            stockJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ??
            throw new InvalidDataException("Stock Phantoon color JSON is null.");
        visual.HealthBands[2][1] = ChangeRed(visual.HealthBands[2][1]);
        visual.FadeOut[1] = ChangeRed(visual.FadeOut[1]);
        visual.PowerOn[1] = ChangeRed(visual.PowerOn[1]);
        string overrides = Path.Combine(stockDirectory, "phantoon-color-overrides");
        Directory.CreateDirectory(overrides);
        string overrideFile = Path.Combine(overrides, PhantoonColorFormat.FileName);
        File.WriteAllBytes(overrideFile, PhantoonColorCatalog.Write(visual));
        EnemyTileArtworkCatalog edited = EnemyTileArtworkFiles.Load(stockDirectory, overrides);
        PhantoonColorCatalog colors = edited.PhantoonColors ??
            throw new InvalidDataException("Edited enemy art has no Phantoon colors.");
        AssertTrue(colors.ResolveHealth(2, 1) != native.ResolveHealth(2, 1),
            "Phantoon health color edit changes selected RGB5 word");
        AssertTrue(colors.ResolveFadeOut(1) != native.ResolveFadeOut(1),
            "Phantoon fade-out edit changes selected RGB5 word");
        AssertTrue(colors.ResolvePowerOn(1) != native.ResolvePowerOn(1),
            "Phantoon power-on edit changes selected RGB5 word");
        for (int band = 0; band < PhantoonColorRomData.HealthBandCount; band++)
        for (int color = 0; color < PhantoonColorRomData.HealthBandColorCount; color++)
            if (band != 2 || color != 1)
                AssertEqual(native.ResolveHealth(band, color),
                    colors.ResolveHealth(band, color),
                    $"Phantoon health edit leaves palette {band} color {color} unchanged");
        CheckUnchangedBand(PhantoonColorRomData.FadeOutCount,
            native.ResolveFadeOut, colors.ResolveFadeOut, "fade-out");
        CheckUnchangedBand(PhantoonColorRomData.PowerOnCount,
            native.ResolvePowerOn, colors.ResolvePowerOn, "power-on");
        AssertEqual(colors.ResolvePowerOn(1),
            EnemyTileArtworkFiles.Load(stockDirectory, overrides)
                .PhantoonColors!.ResolvePowerOn(1),
            "Phantoon override survives catalog reload");

        // Drive the actual boss palette handlers. The complete authored color range is
        // forbidden on the bus, proving no installed runtime path silently falls back.
        var guarded = new PhantoonPaletteReadGuard(rom);
        var cgram = new SnesCgram();
        var enemies = new RoomEnemySystem { TileArtwork = edited };
        BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        Type type = typeof(RoomEnemySystem);
        type.GetField("_bus", flags)!.SetValue(enemies, guarded);
        type.GetField("_cgram", flags)!.SetValue(enemies, cgram);
        RoomEnemySlot body = enemies.Slots[0];
        RoomEnemySlot eye = enemies.Slots[1];
        RoomEnemySlot tentacles = enemies.Slots[2];
        var state = new PhantoonEnemyState(body)
        {
            Eye = eye,
            Tentacles = tentacles,
        };
        MethodInfo copyHealth = type.GetMethod("CopyPhantoonHealthPalette", flags)!;
        MethodInfo fadeIn = type.GetMethod("AdvancePhantoonFadeIn", flags)!;
        for (int band = 0; band < PhantoonColorRomData.HealthBandCount; band++)
        {
            body.Health = unchecked((ushort)(band * 312 + 1));
            copyHealth.Invoke(enemies, [body]);
            AssertBodyBand(band, $"Phantoon live health restore band {band}");
            for (int color = 0; color < PhantoonColorRomData.HealthBandColorCount; color++)
                cgram.SetColor(PhantoonColorRomData.BodyDestination + color, 0);
            eye.VariableE = 1;
            eye.VariableF = 0;
            fadeIn.Invoke(enemies, [body, state, (ushort)0, (byte)0]);
            AssertBodyBand(band, $"Phantoon live materialization band {band}");
            AssertEqual((ushort)2, eye.VariableE,
                $"Phantoon fade-in band {band} retains native numerator advance");
        }

        body.Health = unchecked((ushort)(2 * 312 + 1));
        body.FlashTimer = 8;
        tentacles.Parameter2 = 0x0101;
        type.GetMethod("ApplyPhantoonHurt", flags)!.Invoke(enemies, [body, state]);
        AssertBodyBand(2, "Phantoon live hurt restore");
        AssertEqual((ushort)0x0001, tentacles.Parameter2,
            "Phantoon hurt restore clears only the flash-latch byte");

        eye.VariableE = 1;
        eye.VariableF = 0;
        type.GetMethod("AdvancePhantoonFadeOut", flags)!
            .Invoke(enemies, [state, (ushort)0, (byte)0]);
        for (int color = 0; color < PhantoonColorRomData.FadeOutCount; color++)
            AssertEqual(colors.ResolveFadeOut(color),
                cgram.Colors[PhantoonColorRomData.BodyDestination + color],
                $"Phantoon live fade-out target {color}");
        AssertEqual((ushort)2, eye.VariableE,
            "Phantoon fade-out retains native numerator advance");

        eye.VariableD = 0;
        eye.VariableE = 1;
        bool complete = (bool)type.GetMethod("AdvanceWreckedShipPowerPalette", flags)!
            .Invoke(enemies, [eye])!;
        AssertTrue(!complete, "Phantoon ship power-on remains active on target frame");
        for (int color = 0; color < PhantoonColorRomData.PowerOnCount; color++)
            AssertEqual(colors.ResolvePowerOn(color),
                cgram.Colors[PhantoonColorRomData.PowerOnDestination + color],
                $"Phantoon live ship power-on target {color}");
        AssertEqual((ushort)2, eye.VariableE,
            "Phantoon ship power-on retains native numerator advance");

        File.WriteAllBytes(overrideFile, [0]);
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(stockDirectory, overrides),
            "invalid Phantoon override fails loudly");
        File.WriteAllBytes(overrideFile,
            "{\"version\":1,\"version\":1}"u8.ToArray());
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(stockDirectory, overrides),
            "duplicate Phantoon color property fails loudly");
        visual.HealthBands[2][1] = visual.HealthBands[2][1] with { Green = 32 };
        AssertThrows<InvalidDataException>(() => PhantoonColorCatalog.Write(visual),
            "Phantoon RGB5 channel outside five-bit precision is rejected");
        File.Delete(overrideFile);
        File.WriteAllBytes(file, [0]);
        AssertThrows<InvalidDataException>(() => EnemyTileArtworkFiles.Load(stockDirectory, null),
            "corrupt stock Phantoon colors fail manifest hash validation");
        File.WriteAllBytes(file, stockJson);
        Console.WriteLine("  Phantoon colors: 256 native RGB5 targets, all health bands and three live transitions, persistent override, ROM guard, and strict failures pass.");

        void VerifyBand(int source, int count, Func<int, ushort> resolve, string name)
        {
            for (int color = 0; color < count; color++)
                AssertEqual(RomDataReader.ReadWordFixedBank(rom,
                    source + color * sizeof(ushort)), resolve(color),
                    $"installed Phantoon {name} color {color} preserves native RGB5");
        }

        void AssertBodyBand(int band, string name)
        {
            for (int color = 0; color < PhantoonColorRomData.HealthBandColorCount; color++)
                AssertEqual(colors.ResolveHealth(band, color),
                    cgram.Colors[PhantoonColorRomData.BodyDestination + color],
                    $"{name} color {color}");
        }

        static PaletteRgb5 ChangeRed(PaletteRgb5 rgb) => rgb with
        {
            Red = rgb.Red == 31 ? 30 : rgb.Red + 1,
        };

        static void CheckUnchangedBand(int count, Func<int, ushort> original,
            Func<int, ushort> changed, string name)
        {
            for (int color = 0; color < count; color++)
                if (color != 1)
                    AssertEqual(original(color), changed(color),
                        $"Phantoon {name} edit leaves color {color} unchanged");
        }
    }

    private sealed class PhantoonPaletteReadGuard(ISnesAddressSpace source)
        : ISnesAddressSpace
    {
        public byte ReadByte(int address) =>
            address >= PhantoonColorRomData.FadeOutSource &&
            address < PhantoonColorRomData.HealthBandsSource +
                PhantoonColorRomData.HealthBandCount *
                PhantoonColorRomData.HealthBandColorCount * sizeof(ushort)
                ? throw new InvalidOperationException(
                    $"Phantoon accessed migrated palette ROM ${address:X6}.")
                : source.ReadByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
