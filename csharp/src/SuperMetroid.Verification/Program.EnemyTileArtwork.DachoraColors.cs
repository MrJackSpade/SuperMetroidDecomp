using System.Reflection;
using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyInstalledDachoraColors(
        ISnesAddressSpace rom, string stockDirectory, EnemyTileArtworkCatalog stock)
    {
        DachoraColorCatalog native = stock.DachoraColors ??
            throw new InvalidDataException("Installed enemy art has no Dachora colors.");
        VerifyFrame(DachoraPalettePhase.Default, 0);
        for (int frame = 0; frame < DachoraColorRomData.AnimatedFrameCount; frame++)
        {
            VerifyFrame(DachoraPalettePhase.Speed, frame);
            VerifyFrame(DachoraPalettePhase.Shine, frame);
            AssertEqual(unchecked((ushort)DachoraColorRomData.Source(
                    DachoraPalettePhase.Speed, frame)),
                RomDataReader.ReadWordFixedBank(rom,
                    DachoraColorRomData.SpeedPointerTable + frame * sizeof(ushort)),
                $"native Dachora speed selector {frame}");
            AssertEqual(unchecked((ushort)DachoraColorRomData.Source(
                    DachoraPalettePhase.Shine, frame)),
                RomDataReader.ReadWordFixedBank(rom,
                    DachoraColorRomData.ShinePointerTable + frame * sizeof(ushort)),
                $"native Dachora shine selector {frame}");
        }

        string file = Path.Combine(stockDirectory, DachoraColorFormat.FileName);
        byte[] stockJson = File.ReadAllBytes(file);
        DachoraColorDocument visual = JsonSerializer.Deserialize<DachoraColorDocument>(
            stockJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ??
            throw new InvalidDataException("Stock Dachora color JSON is null.");
        visual.Normal[5] = ChangeRed(visual.Normal[5]);
        foreach (PaletteRgb5[] frame in visual.Speed)
            frame[5] = ChangeRed(frame[5]);
        foreach (PaletteRgb5[] frame in visual.Shine)
            frame[5] = ChangeRed(frame[5]);
        string overrides = Path.Combine(stockDirectory, "dachora-color-overrides");
        Directory.CreateDirectory(overrides);
        string overrideFile = Path.Combine(overrides, DachoraColorFormat.FileName);
        File.WriteAllBytes(overrideFile, DachoraColorCatalog.Write(visual));
        EnemyTileArtworkCatalog edited = EnemyTileArtworkFiles.Load(stockDirectory, overrides);
        DachoraColorCatalog colors = edited.DachoraColors ??
            throw new InvalidDataException("Edited enemy art has no Dachora colors.");
        CheckEditedFrame(DachoraPalettePhase.Default, 0);
        for (int frame = 0; frame < DachoraColorRomData.AnimatedFrameCount; frame++)
        {
            CheckEditedFrame(DachoraPalettePhase.Speed, frame);
            CheckEditedFrame(DachoraPalettePhase.Shine, frame);
        }
        AssertEqual(colors.Resolve(DachoraPalettePhase.Shine, 3, 5),
            EnemyTileArtworkFiles.Load(stockDirectory, overrides)
                .DachoraColors!.Resolve(DachoraPalettePhase.Shine, 3, 5),
            "Dachora color override survives catalog reload");

        var guard = new DachoraPaletteReadGuard(rom);
        var cgram = new SnesCgram();
        var enemies = new RoomEnemySystem { TileArtwork = edited };
        BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        Type type = typeof(RoomEnemySystem);
        type.GetField("_bus", flags)!.SetValue(enemies, guard);
        type.GetField("_cgram", flags)!.SetValue(enemies, cgram);
        RoomEnemySlot slot = enemies.Slots[0];
        slot.PaletteIndex = 0x0200;
        slot.XPosition = 96;
        slot.YPosition = 96;
        slot.XRadius = 8;
        slot.YRadius = 8;
        var state = new DachoraEnemyState(slot);
        MethodInfo load = type.GetMethod("LoadDachoraPalette", flags)!;
        load.Invoke(enemies, [slot, DachoraPalettePhase.Default, 0]);
        CheckLive(DachoraPalettePhase.Default, 0);

        var empty = new RoomLevelData(16, 16, new ushort[256], new byte[256],
            new ushort[256], new byte[8]);
        MethodInfo accelerate = type.GetMethod("AccelerateDachora", flags)!;
        for (int frame = 0; frame < DachoraColorRomData.AnimatedFrameCount; frame++)
        {
            state.SpeedOrTimer = 8;
            state.Subspeed = 0;
            state.PaletteAnimationTimer = (ushort)((frame << 8) | 1);
            accelerate.Invoke(enemies, [slot, state, empty]);
            CheckLive(DachoraPalettePhase.Speed, frame);
        }

        MethodInfo shine = type.GetMethod("StepDachoraShinePalette", flags)!;
        for (int frame = 0; frame < DachoraColorRomData.AnimatedFrameCount; frame++)
        {
            state.PaletteAnimationTimer = (ushort)(frame << 8);
            shine.Invoke(enemies, [slot, state]);
            CheckLive(DachoraPalettePhase.Shine, frame);
            AssertEqual((ushort)(((frame + 1) % 4) << 8),
                state.PaletteAnimationTimer,
                $"Dachora shine frame {frame} retains native next-frame selection");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "live Dachora color consumers avoid RGB5 and selector ROM reads");

        File.WriteAllBytes(overrideFile, [0]);
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(stockDirectory, overrides),
            "invalid Dachora color override fails loudly");
        File.WriteAllBytes(overrideFile,
            "{\"version\":1,\"version\":1}"u8.ToArray());
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(stockDirectory, overrides),
            "duplicate Dachora color property fails loudly");
        visual.Speed[0][5] = visual.Speed[0][5] with { Blue = 32 };
        AssertThrows<InvalidDataException>(() => DachoraColorCatalog.Write(visual),
            "Dachora RGB5 channel outside five-bit precision is rejected");
        File.Delete(overrideFile);
        File.WriteAllBytes(file, [0]);
        AssertThrows<InvalidDataException>(() => EnemyTileArtworkFiles.Load(stockDirectory, null),
            "corrupt stock Dachora colors fail manifest hash validation");
        File.WriteAllBytes(file, stockJson);
        Console.WriteLine("  Dachora colors: 144 native RGB5 words, eight compiled selector pointers, live speed/shine phase selection, persistent override, ROM guard and strict failures pass.");

        void VerifyFrame(DachoraPalettePhase phase, int frame)
        {
            int source = DachoraColorRomData.Source(phase, frame);
            for (int color = 0; color < DachoraColorRomData.ColorsPerFrame; color++)
                AssertEqual(RomDataReader.ReadWordFixedBank(rom,
                    source + color * sizeof(ushort)),
                    native.Resolve(phase, frame, color),
                    $"installed Dachora {phase} frame {frame} color {color}");
        }

        void CheckEditedFrame(DachoraPalettePhase phase, int frame)
        {
            AssertTrue(native.Resolve(phase, frame, 5) != colors.Resolve(phase, frame, 5),
                $"Dachora {phase} frame {frame} edit changes selected color");
            AssertEqual(native.Resolve(phase, frame, 4), colors.Resolve(phase, frame, 4),
                $"Dachora {phase} frame {frame} edit preserves adjacent color");
        }

        void CheckLive(DachoraPalettePhase phase, int frame)
        {
            int destination = 128 + ((slot.PaletteIndex >> 9) & 7) * 16;
            for (int color = 0; color < DachoraColorRomData.ColorsPerFrame; color++)
                AssertEqual(colors.Resolve(phase, frame, color),
                    cgram.Colors[destination + color],
                    $"live Dachora {phase} frame {frame} color {color}");
        }

        static PaletteRgb5 ChangeRed(PaletteRgb5 rgb) => rgb with
        {
            Red = rgb.Red == 31 ? 30 : rgb.Red + 1,
        };
    }

    private sealed class DachoraPaletteReadGuard(ISnesAddressSpace source)
        : ISnesAddressSpace
    {
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            bool colors = address >= DachoraColorRomData.DefaultSource &&
                address < DachoraColorRomData.ShineSource +
                    DachoraColorRomData.AnimatedFrameCount * DachoraColorRomData.FrameByteCount;
            bool speedSelectors = address >= DachoraColorRomData.SpeedPointerTable &&
                address < DachoraColorRomData.SpeedPointerTable +
                    DachoraColorRomData.AnimatedFrameCount * sizeof(ushort);
            bool shineSelectors = address >= DachoraColorRomData.ShinePointerTable &&
                address < DachoraColorRomData.ShinePointerTable +
                    DachoraColorRomData.AnimatedFrameCount * sizeof(ushort);
            if (colors || speedSelectors || shineSelectors)
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Live Dachora read migrated color/selector ROM ${address:X6}.");
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
