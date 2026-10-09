using System.Reflection;
using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    /// <summary>Checks extracted Baby cutscene palettes, installed overrides, and live CGRAM updates.</summary>
    /// <param name="rom">Cartridge address space supplying the original RGB5 palette data.</param>
    /// <param name="stockDirectory">Directory containing the stock enemy-art files and manifest.</param>
    /// <param name="stock">Loaded stock catalog used as the baseline for override comparisons.</param>
    private static void VerifyInstalledBabyMetroidCutsceneColors(
        ISnesAddressSpace rom, string stockDirectory, EnemyTileArtworkCatalog stock)
    {
        BabyMetroidCutsceneColorCatalog native = stock.BabyMetroidCutsceneColors ??
            throw new InvalidDataException("Installed enemy art has no cutscene Baby colors.");
        for (int color = 0; color < BabyMetroidCutsceneColorRomData.InitialColorCount; color++)
            AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom),
                    BabyMetroidCutsceneColorRomData.InitialSource + color * 2),
                native.InitialColor(color),
                $"installed cutscene Baby initial color {color}");
        for (int index = 1; index <= BabyMetroidCutsceneColorRomData.FadeFrameCount; index++)
        {
            int source = BabyMetroidCutsceneColorRomData.FadeSource(index);
            AssertEqual(unchecked((ushort)source),
                RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom),
                    BabyMetroidCutsceneColorRomData.FadePointerTable + index * 2),
                $"native cutscene Baby fade selector {index}");
            for (int color = 0; color < BabyMetroidCutsceneColorRomData.FadeColorCount; color++)
                AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom), source + color * 2),
                    native.FadeColor(index, color),
                    $"installed cutscene Baby fade {index} color {color}");
        }

        string file = Path.Combine(stockDirectory,
            BabyMetroidCutsceneColorFormat.FileName);
        byte[] stockJson = File.ReadAllBytes(file);
        BabyMetroidCutsceneColorDocument visual =
            JsonSerializer.Deserialize<BabyMetroidCutsceneColorDocument>(stockJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ??
            throw new InvalidDataException("Stock cutscene Baby color JSON is null.");
        visual.Initial[5] = ChangeRed(visual.Initial[5]);
        foreach (PaletteRgb5[] frame in visual.Fade)
            frame[5] = ChangeRed(frame[5]);
        string overrides = Path.Combine(stockDirectory,
            "baby-metroid-cutscene-color-overrides");
        Directory.CreateDirectory(overrides);
        string overrideFile = Path.Combine(overrides,
            BabyMetroidCutsceneColorFormat.FileName);
        File.WriteAllBytes(overrideFile, BabyMetroidCutsceneColorCatalog.Write(visual));
        EnemyTileArtworkCatalog edited = EnemyTileArtworkFiles.Load(stockDirectory, overrides);
        BabyMetroidCutsceneColorCatalog colors = edited.BabyMetroidCutsceneColors ??
            throw new InvalidDataException("Edited enemy art has no cutscene Baby colors.");
        AssertTrue(native.InitialColor(5) != colors.InitialColor(5),
            "cutscene Baby initial edit takes effect");
        AssertEqual(native.InitialColor(4), colors.InitialColor(4),
            "cutscene Baby initial adjacent color stays native");
        for (int index = 1; index <= BabyMetroidCutsceneColorRomData.FadeFrameCount; index++)
        {
            AssertTrue(native.FadeColor(index, 5) != colors.FadeColor(index, 5),
                $"cutscene Baby fade {index} edit takes effect");
            AssertEqual(native.FadeColor(index, 4), colors.FadeColor(index, 4),
                $"cutscene Baby fade {index} adjacent color stays native");
        }
        AssertEqual(colors.FadeColor(6, 5),
            EnemyTileArtworkFiles.Load(stockDirectory, overrides)
                .BabyMetroidCutsceneColors!.FadeColor(6, 5),
            "cutscene Baby color override survives catalog reload");

        var guard = new BabyMetroidCutsceneColorReadGuard(rom);
        var cgram = new SnesCgram();
        var enemies = new RoomEnemySystem { TileArtwork = edited };
        BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        Type type = typeof(RoomEnemySystem);
        type.GetField("_bus", flags)!.SetValue(enemies, guard);
        type.GetField("_cgram", flags)!.SetValue(enemies, cgram);
        int destination = BabyMetroidCutsceneColorRomData.DestinationByteIndex / 2;
        cgram.SetColor(destination - 1, 0x1234);
        MethodInfo initial = type.GetMethod("LoadBabyMetroidCutsceneInitialPalette", flags)!;
        initial.Invoke(enemies, null);
        AssertEqual((ushort)0x1234, cgram.Colors[destination - 1],
            "cutscene Baby initial load preserves palette color zero");
        for (int color = 0; color < BabyMetroidCutsceneColorRomData.InitialColorCount; color++)
            AssertEqual(colors.InitialColor(color), cgram.Colors[destination + color],
                $"live cutscene Baby initial color {color}");
        ushort initialLastColor = cgram.Colors[destination + 14];

        MethodInfo fade = type.GetMethod("LoadBabyMetroidCutsceneFadePalette", flags)!;
        for (int index = 1; index <= BabyMetroidCutsceneColorRomData.FadeFrameCount; index++)
        {
            var transfer = new BabyMetroidPaletteTransferRequest(
                (ushort)index,
                (uint)BabyMetroidCutsceneColorRomData.FadeSource(index),
                BabyMetroidCutsceneColorRomData.DestinationByteIndex,
                BabyMetroidCutsceneColorRomData.FadeColorCount);
            fade.Invoke(enemies, [transfer]);
            for (int color = 0; color < BabyMetroidCutsceneColorRomData.FadeColorCount; color++)
                AssertEqual(colors.FadeColor(index, color), cgram.Colors[destination + color],
                    $"live cutscene Baby fade {index} color {color}");
            AssertEqual(initialLastColor, cgram.Colors[destination + 14],
                $"cutscene Baby fade {index} preserves color fifteen");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "live cutscene Baby palette consumers avoid migrated RGB5 ROM reads");

        File.WriteAllBytes(overrideFile, [0]);
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(stockDirectory, overrides),
            "invalid cutscene Baby color override fails loudly");
        File.WriteAllBytes(overrideFile,
            "{\"version\":1,\"version\":1}"u8.ToArray());
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(stockDirectory, overrides),
            "duplicate cutscene Baby color property fails loudly");
        visual.Fade[0][5] = visual.Fade[0][5] with { Blue = 32 };
        AssertThrows<InvalidDataException>(
            () => BabyMetroidCutsceneColorCatalog.Write(visual),
            "cutscene Baby RGB5 channel outside five-bit precision is rejected");
        File.Delete(overrideFile);
        File.WriteAllBytes(file, [0]);
        AssertThrows<InvalidDataException>(() => EnemyTileArtworkFiles.Load(stockDirectory, null),
            "corrupt stock cutscene Baby colors fail manifest hash validation");
        File.WriteAllBytes(file, stockJson);
        Console.WriteLine("  Cutscene Baby colors: 99 native RGB5 words, six selectors, live initial/fade CGRAM, persistent override, ROM guard and strict failures pass.");

        static PaletteRgb5 ChangeRed(PaletteRgb5 rgb) => rgb with
        {
            Red = rgb.Red == 31 ? 30 : rgb.Red + 1,
        };
    }

    /// <summary>Forwards address-space operations while rejecting runtime reads of migrated Baby cutscene colors.</summary>
    /// <param name="source">Address space used for permitted reads and all writes.</param>
    private sealed class BabyMetroidCutsceneColorReadGuard(ISnesAddressSpace source)
        : ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Number of attempted reads from the migrated initial-palette or fade-color ranges.</summary>
        internal int ForbiddenReadAttempts { get; private set; }

        /// <summary>Routes an import-source byte request through the guarded read operation.</summary>
        /// <param name="address">Cartridge address to read.</param>
        /// <returns>The byte at the requested address when it is outside the migrated color ranges.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Reads a permitted byte from the wrapped address space and rejects migrated palette reads.</summary>
        /// <param name="address">Address to read.</param>
        /// <returns>The wrapped byte when the address is outside the initial and fade-color tables.</returns>
        /// <exception cref="InvalidOperationException">The address belongs to a migrated Baby cutscene color range.</exception>
        public byte ReadByte(int address)
        {
            bool initial = address >= BabyMetroidCutsceneColorRomData.InitialSource &&
                address < BabyMetroidCutsceneColorRomData.InitialSource +
                    BabyMetroidCutsceneColorRomData.InitialColorCount * sizeof(ushort);
            bool fade = address >= BabyMetroidCutsceneColorRomData.FirstFadeSource &&
                address < BabyMetroidCutsceneColorRomData.FadeSource(
                    BabyMetroidCutsceneColorRomData.FadeFrameCount) +
                    BabyMetroidCutsceneColorRomData.FadeColorCount * sizeof(ushort);
            if (initial || fade)
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Live cutscene Baby read migrated color ROM ${address:X6}.");
            }
            return source.ReadByte(address);
        }

        /// <summary>Forwards a byte write to the wrapped address space.</summary>
        /// <param name="address">Address to update.</param>
        /// <param name="value">Byte to store at that address.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
