using System.Reflection;
using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    /// <summary>Checks installed Shitroid colors against ROM and verifies edits reach live palette consumers without ROM reads.</summary>
    /// <param name="rom">Cartridge address space used to compare extracted color definitions with the original tables.</param>
    /// <param name="stockDirectory">Directory containing the stock enemy artwork and color document.</param>
    /// <param name="stock">Installed stock artwork catalog whose compiled Shitroid colors form the comparison baseline.</param>
    private static void VerifyInstalledShitroidColors(
        ISnesAddressSpace rom, string stockDirectory, EnemyTileArtworkCatalog stock)
    {
        ShitroidColorCatalog native = stock.ShitroidColors ??
            throw new InvalidDataException("Installed enemy art has no Shitroid colors.");
        for (int frame = 0; frame < ShitroidColorRomData.NormalFrameCount; frame++)
        for (int color = 0; color < ShitroidColorRomData.NormalColorsPerFrame; color++)
            AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom),
                    ShitroidColorRomData.NormalCycle +
                    (frame * ShitroidColorRomData.NormalColorsPerFrame + color) * 2),
                native.NormalColor(frame, color),
                $"installed Shitroid normal frame {frame} color {color}");
        Suite(nameof(VerifyTarget), () => VerifyTarget(ShitroidColorTarget.Sidehopper, ShitroidColorRomData.SidehopperTarget));
        Suite(nameof(VerifyTarget), () => VerifyTarget(ShitroidColorTarget.Shitroid, ShitroidColorRomData.ShitroidTarget));
        Suite(nameof(VerifyTarget), () => VerifyTarget(ShitroidColorTarget.DeadSidehopper,
            ShitroidColorRomData.DeadSidehopperTarget));

        string file = Path.Combine(stockDirectory, ShitroidColorFormat.FileName);
        byte[] stockJson = File.ReadAllBytes(file);
        ShitroidColorDocument visual = JsonSerializer.Deserialize<ShitroidColorDocument>(
            stockJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ??
            throw new InvalidDataException("Stock Shitroid color JSON is null.");
        foreach (PaletteRgb5[] frame in visual.Normal) frame[2] = ChangeRed(frame[2]);
        visual.Sidehopper[5] = ChangeRed(visual.Sidehopper[5]);
        visual.Shitroid[5] = ChangeRed(visual.Shitroid[5]);
        visual.DeadSidehopper[5] = ChangeRed(visual.DeadSidehopper[5]);
        string overrides = Path.Combine(stockDirectory, "shitroid-color-overrides");
        Directory.CreateDirectory(overrides);
        string overrideFile = Path.Combine(overrides, ShitroidColorFormat.FileName);
        File.WriteAllBytes(overrideFile, ShitroidColorCatalog.Write(visual));
        EnemyTileArtworkCatalog edited = EnemyTileArtworkFiles.Load(stockDirectory, overrides);
        ShitroidColorCatalog colors = edited.ShitroidColors ??
            throw new InvalidDataException("Edited enemy art has no Shitroid colors.");
        for (int frame = 0; frame < ShitroidColorRomData.NormalFrameCount; frame++)
        {
            AssertTrue(native.NormalColor(frame, 2) != colors.NormalColor(frame, 2),
                $"Shitroid normal frame {frame} edit takes effect");
            AssertEqual(native.NormalColor(frame, 1), colors.NormalColor(frame, 1),
                $"Shitroid normal frame {frame} adjacent color remains native");
        }
        foreach (ShitroidColorTarget target in Enum.GetValues<ShitroidColorTarget>())
        {
            AssertTrue(native.TargetColor(target, 5) != colors.TargetColor(target, 5),
                $"Shitroid {target} target edit takes effect");
            AssertEqual(native.TargetColor(target, 4), colors.TargetColor(target, 4),
                $"Shitroid {target} adjacent target color remains native");
        }
        AssertEqual(colors.NormalColor(7, 2),
            EnemyTileArtworkFiles.Load(stockDirectory, overrides)
                .ShitroidColors!.NormalColor(7, 2),
            "Shitroid color override survives catalog reload");

        var guard = new ShitroidPaletteReadGuard(rom);
        var cgram = new SnesCgram();
        var enemies = new RoomEnemySystem { TileArtwork = edited };
        BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        Type type = typeof(RoomEnemySystem);
        type.GetField("_bus", flags)!.SetValue(enemies, guard);
        type.GetField("_cgram", flags)!.SetValue(enemies, cgram);
        RoomEnemySlot slot = enemies.Slots[0];
        MethodInfo initialize = type.GetMethod("InitializeShitroid", flags)!;
        initialize.Invoke(enemies, [slot, (ushort)0]);
        ShitroidEnemyState state = enemies.Shitroid ??
            throw new InvalidDataException("Shitroid initialization produced no state.");
        CheckLiveTarget(ShitroidColorTarget.Sidehopper, 0x90);
        CheckLiveTarget(ShitroidColorTarget.Shitroid, 0xa0);
        CheckLiveTarget(ShitroidColorTarget.DeadSidehopper, 0xf0);
        MethodInfo step = type.GetMethod("StepShitroidNormalPalette", flags)!;
        for (int frame = 0; frame < ShitroidColorRomData.NormalFrameCount; frame++)
        {
            state.PaletteTimerAndPhase = (ushort)((frame + 7) & 7);
            step.Invoke(enemies, [state]);
            AssertEqual((ushort)((state.PaletteDelay << 8) | frame),
                state.PaletteTimerAndPhase,
                $"Shitroid normal frame {frame} retains native timer/phase selection");
            for (int color = 0; color < ShitroidColorRomData.NormalColorsPerFrame; color++)
                AssertEqual(colors.NormalColor(frame, color), cgram.Colors[165 + color],
                    $"live Shitroid normal frame {frame} color {color}");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "live Shitroid palette consumers avoid migrated RGB5 ROM reads");

        File.WriteAllBytes(overrideFile, [0]);
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(stockDirectory, overrides),
            "invalid Shitroid color override fails loudly");
        File.WriteAllBytes(overrideFile,
            "{\"version\":1,\"version\":1}"u8.ToArray());
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(stockDirectory, overrides),
            "duplicate Shitroid color property fails loudly");
        visual.Normal[0][2] = visual.Normal[0][2] with { Green = 32 };
        AssertThrows<InvalidDataException>(() => ShitroidColorCatalog.Write(visual),
            "Shitroid RGB5 channel outside five-bit precision is rejected");
        File.Delete(overrideFile);
        File.WriteAllBytes(file, [0]);
        AssertThrows<InvalidDataException>(() => EnemyTileArtworkFiles.Load(stockDirectory, null),
            "corrupt stock Shitroid colors fail manifest hash validation");
        File.WriteAllBytes(file, stockJson);
        Console.WriteLine("  Shitroid colors: 80 native RGB5 words, native timer selection, three live target fades, persistent override, ROM guard and strict failures pass.");

        void VerifyTarget(ShitroidColorTarget target, int source)
        {
            for (int color = 0; color < ShitroidColorRomData.TargetColorCount; color++)
                AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom), source + color * 2),
                    native.TargetColor(target, color),
                    $"installed Shitroid {target} target color {color}");
        }

        void CheckLiveTarget(ShitroidColorTarget target, int destination)
        {
            for (int color = 0; color < ShitroidColorRomData.TargetColorCount; color++)
            {
                ushort expected = colors.TargetColor(target, color);
                AssertEqual(expected, state.TargetPalette.Span[destination + color],
                    $"live Shitroid {target} target buffer color {color}");
                AssertEqual(expected, cgram.Colors[destination + color],
                    $"live Shitroid {target} CGRAM color {color}");
            }
        }

        static PaletteRgb5 ChangeRed(PaletteRgb5 rgb) => rgb with
        {
            Red = rgb.Red == 31 ? 30 : rgb.Red + 1,
        };
    }

    /// <summary>Rejects live reads from the migrated Shitroid RGB5 cycle and target palette tables.</summary>
    /// <param name="source">Address space that supplies unrelated reads and receives writes.</param>
    private sealed class ShitroidPaletteReadGuard(ISnesAddressSpace source)
        : ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Number of attempted reads from a migrated Shitroid color table.</summary>
        internal int ForbiddenReadAttempts { get; private set; }

        /// <summary>Routes importer reads through the guarded address-space path.</summary>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Blocks reads from the normal cycle and target palettes while delegating other addresses.</summary>
        /// <param name="address">Full cartridge address requested by the caller.</param>
        /// <returns>The source byte when the address is outside the migrated color ranges.</returns>
        public byte ReadByte(int address)
        {
            bool normal = address >= ShitroidColorRomData.NormalCycle &&
                address < ShitroidColorRomData.NormalCycle +
                    ShitroidColorRomData.NormalFrameCount *
                    ShitroidColorRomData.NormalColorsPerFrame * sizeof(ushort);
            bool targets = address >= ShitroidColorRomData.DeadSidehopperTarget &&
                address < ShitroidColorRomData.ShitroidTarget +
                    ShitroidColorRomData.TargetColorCount * sizeof(ushort);
            if (normal || targets)
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Live Shitroid read migrated color ROM ${address:X6}.");
            }
            return source.ReadByte(address);
        }

        /// <summary>Forwards writes unchanged to the wrapped address space.</summary>
        /// <param name="address">Full cartridge address to write.</param>
        /// <param name="value">Byte written to that address.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
