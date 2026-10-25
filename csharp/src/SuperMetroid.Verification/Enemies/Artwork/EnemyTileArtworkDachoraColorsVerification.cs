using SuperMetroid.Core.Samus.Palettes;
using System.Text.Json;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Animals;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Core.World;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.CartridgeTables;

namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>Checks Dachora's default, speed, and shine palette frames against installed art and native selectors.</summary>
internal static class EnemyTileArtworkDachoraColorsVerification
{
    /// <summary>JSON options: Case-insensitive property matching.</summary>
    private static readonly JsonSerializerOptions CaseInsensitiveJson = new() { PropertyNameCaseInsensitive = true };

    /// <summary>Exercises edited RGB5 colors through live CGRAM updates while preserving phase and next-frame selection.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for installed Dachora colors and selector pointers")]
    internal static void VerifyInstalledDachoraColors(
        ISnesAddressSpace rom, string stockDirectory, EnemyTileArtworkCatalog stock)
    {
        DachoraColorCatalog native = stock.DachoraColors ??
            throw new InvalidDataException("Installed enemy art has no Dachora colors.");
        Suite(nameof(VerifyFrame), () => VerifyFrame(DachoraPalettePhase.Default, 0));
        for (int frame = 0; frame < DachoraColorDefinitions.AnimatedFrameCount; frame++)
        {
            VerifyFrame(DachoraPalettePhase.Speed, frame);
            VerifyFrame(DachoraPalettePhase.Shine, frame);
            AssertEqual(unchecked((ushort)DachoraColorSources.Source(
                    DachoraPalettePhase.Speed, frame)),
                RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom),
                    DachoraColorSources.SpeedPointerTable + frame * sizeof(ushort)),
                $"native Dachora speed selector {frame}");
            AssertEqual(unchecked((ushort)DachoraColorSources.Source(
                    DachoraPalettePhase.Shine, frame)),
                RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom),
                    DachoraColorSources.ShinePointerTable + frame * sizeof(ushort)),
                $"native Dachora shine selector {frame}");
        }

        string file = Path.Combine(stockDirectory, DachoraColorFormat.FileName);
        byte[] stockJson = File.ReadAllBytes(file);
        DachoraColorDocument visual = JsonSerializer.Deserialize<DachoraColorDocument>(
            stockJson, CaseInsensitiveJson) ??
            throw new InvalidDataException("Stock Dachora color JSON is null.");
        visual.Normal[5] = ChangeRed(visual.Normal[5]);
        foreach (PaletteRgb5[] frame in visual.Speed)
            frame[5] = ChangeRed(frame[5]);
        foreach (PaletteRgb5[] frame in visual.Shine)
            frame[5] = ChangeRed(frame[5]);
        string overrides = Path.Combine(stockDirectory, "dachora-color-overrides");
        Directory.CreateDirectory(overrides);
        string overrideFile = Path.Combine(overrides, DachoraColorFormat.FileName);
        File.WriteAllBytes(overrideFile, DachoraColorFile.Write(visual));
        InstalledEnemyArtwork edited = EnemyTileArtworkFiles.Load(stockDirectory, overrides);
        DachoraColorCatalog colors = edited.Enemies.DachoraColors ??
            throw new InvalidDataException("Edited enemy art has no Dachora colors.");
        CheckEditedFrame(DachoraPalettePhase.Default, 0);
        for (int frame = 0; frame < DachoraColorDefinitions.AnimatedFrameCount; frame++)
        {
            CheckEditedFrame(DachoraPalettePhase.Speed, frame);
            CheckEditedFrame(DachoraPalettePhase.Shine, frame);
        }
        AssertEqual(colors.Resolve(DachoraPalettePhase.Shine, 3, 5),
            (EnemyTileArtworkFiles.Load(stockDirectory, overrides)
                .Enemies.DachoraColors ?? throw new InvalidOperationException("Verification requires EnemyTileArtworkFiles.Load(stockDirectory, overrides) .DachoraColors.")).Resolve(DachoraPalettePhase.Shine, 3, 5),
            "Dachora color override survives catalog reload");

        CartridgeReadGuard guard = new CartridgeReadGuard(rom).Forbid(
            address => address is (>= EnemySpeciesSourceDefinitions.Dachora.DefaultColors and
                    < EnemySpeciesSourceDefinitions.Dachora.ShineColors +
                        (DachoraColorDefinitions.AnimatedFrameCount * EnemySpeciesSourceDefinitions.Dachora.ImageBytes))
                or (>= DachoraColorSources.SpeedPointerTable and
                    < DachoraColorSources.SpeedPointerTable + (DachoraColorDefinitions.AnimatedFrameCount * sizeof(ushort)))
                or (>= DachoraColorSources.ShinePointerTable and
                    < DachoraColorSources.ShinePointerTable + (DachoraColorDefinitions.AnimatedFrameCount * sizeof(ushort))),
            address => $"Live Dachora read migrated color/selector ROM ${address:X6}.");
        var cgram = new SamusColourRig();
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance) { TileArtwork = edited.Enemies, BossArtwork = edited.Bosses, AreaArtwork = edited.Areas };
        Type type = typeof(RoomEnemySystem);
        enemies.Presentation.BindColours(cgram.Gameplay);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.PaletteIndex = 0x0200;
        slot.Position = slot.Position.WithWhole(new(96, 96));
        slot.Radius = new(8, 8);
        var state = new DachoraEnemyState();
        enemies.DachoraBehavior.LoadDachoraPalette(slot, DachoraPalettePhase.Default, 0);
        CheckLive(DachoraPalettePhase.Default, 0);

        var empty = new RoomLevelData(new Extent16<RoomBlocks>(16, 16), new RoomLevelPlanes(new ushort[256], new byte[256], new ushort[256]), new byte[8]);
        for (int frame = 0; frame < DachoraColorDefinitions.AnimatedFrameCount; frame++)
        {
            state.Speed = SnesSignedSixteenSixteen.FromParts(8, 0);
            state.SetPaletteCycle(frame, ticks: 1);
            enemies.DachoraBehavior.AccelerateDachora(slot, state, empty);
            CheckLive(DachoraPalettePhase.Speed, frame);
        }

        for (int frame = 0; frame < DachoraColorDefinitions.AnimatedFrameCount; frame++)
        {
            state.SetPaletteCycle(frame, ticks: 0);
            enemies.DachoraBehavior.StepDachoraShinePalette(slot, state);
            CheckLive(DachoraPalettePhase.Shine, frame);
            AssertEqual((frame + 1) % 4, state.PaletteFrame,
                $"Dachora shine frame {frame} retains native next-frame selection");
            AssertEqual(0, state.PaletteTicks,
                $"Dachora shine frame {frame} keeps no speed ticks");
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
        AssertThrows<InvalidDataException>(() => DachoraColorFile.Write(visual),
            "Dachora RGB5 channel outside five-bit precision is rejected");
        File.Delete(overrideFile);
        File.WriteAllBytes(file, [0]);
        AssertThrows<InvalidDataException>(() => EnemyTileArtworkFiles.Load(stockDirectory, null),
            "corrupt stock Dachora colors fail manifest hash validation");
        File.WriteAllBytes(file, stockJson);
        Console.WriteLine("  Dachora colors: 144 native RGB5 words, eight compiled selector pointers, live speed/shine phase selection, persistent override, ROM guard and strict failures pass.");

        void VerifyFrame(DachoraPalettePhase phase, int frame)
        {
            int source = DachoraColorSources.Source(phase, frame);
            for (int color = 0; color < DachoraColorDefinitions.ColorsPerFrame; color++)
                AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom),
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
            for (int color = 0; color < DachoraColorDefinitions.ColorsPerFrame; color++)
                AssertEqual(colors.Resolve(phase, frame, color),
                    cgram.Colors[destination + color],
                    $"live Dachora {phase} frame {frame} color {color}");
        }

        static PaletteRgb5 ChangeRed(PaletteRgb5 rgb) => rgb with
        {
            Red = rgb.Red == 31 ? 30 : rgb.Red + 1,
        };
    }
}
