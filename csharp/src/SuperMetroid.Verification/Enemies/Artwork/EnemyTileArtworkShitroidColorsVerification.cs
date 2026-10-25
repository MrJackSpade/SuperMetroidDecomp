using SuperMetroid.Core.Samus.Palettes;
using System.Text.Json;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Core.World;
using SuperMetroid.AssetExtraction.Bosses.MotherBrain;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>Checks Shitroid's normal palette cycle and three target fades against installed art and native RGB5 data.</summary>
internal static class EnemyTileArtworkShitroidColorsVerification
{
    /// <summary>JSON options: Case-insensitive property matching.</summary>
    private static readonly JsonSerializerOptions CaseInsensitiveJson = new() { PropertyNameCaseInsensitive = true };

    /// <summary>Exercises edited colors through live CGRAM consumers while preserving native timer selection and forbidding source reads.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for installed Shitroid colors")]
    internal static void VerifyInstalledShitroidColors(
        ISnesAddressSpace rom, string stockDirectory, InstalledEnemyArtwork stock)
    {
        ShitroidColorCatalog native = stock.Bosses.ShitroidColors ??
            throw new InvalidDataException("Installed enemy art has no Shitroid colors.");
        for (int frame = 0; frame < ShitroidColorDefinitions.NormalFrameCount; frame++)
        for (int color = 0; color < ShitroidColorDefinitions.NormalColorsPerFrame; color++)
            AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom),
                    MotherBrainSourceDefinitions.Shitroid.NormalCycle +
                    (frame * ShitroidColorDefinitions.NormalColorsPerFrame + color) * 2),
                native.NormalColor(frame, color),
                $"installed Shitroid normal frame {frame} color {color}");
        Suite(nameof(VerifyTarget), () => VerifyTarget(ShitroidColorTarget.Sidehopper, MotherBrainSourceDefinitions.Shitroid.SidehopperTarget));
        Suite(nameof(VerifyTarget), () => VerifyTarget(ShitroidColorTarget.Shitroid, MotherBrainSourceDefinitions.Shitroid.ShitroidTarget));
        Suite(nameof(VerifyTarget), () => VerifyTarget(ShitroidColorTarget.DeadSidehopper,
            MotherBrainSourceDefinitions.Shitroid.DeadSidehopperTarget));

        string file = Path.Combine(stockDirectory, ShitroidColorFormat.FileName);
        byte[] stockJson = File.ReadAllBytes(file);
        ShitroidColorDocument visual = JsonSerializer.Deserialize<ShitroidColorDocument>(
            stockJson, CaseInsensitiveJson) ??
            throw new InvalidDataException("Stock Shitroid color JSON is null.");
        foreach (PaletteRgb5[] frame in visual.Normal) frame[2] = ChangeRed(frame[2]);
        visual.Sidehopper[5] = ChangeRed(visual.Sidehopper[5]);
        visual.Shitroid[5] = ChangeRed(visual.Shitroid[5]);
        visual.DeadSidehopper[5] = ChangeRed(visual.DeadSidehopper[5]);
        string overrides = Path.Combine(stockDirectory, "shitroid-color-overrides");
        Directory.CreateDirectory(overrides);
        string overrideFile = Path.Combine(overrides, ShitroidColorFormat.FileName);
        File.WriteAllBytes(overrideFile, ShitroidColorFile.Write(visual));
        InstalledEnemyArtwork edited = EnemyTileArtworkFiles.Load(stockDirectory, overrides);
        ShitroidColorCatalog colors = edited.Bosses.ShitroidColors ??
            throw new InvalidDataException("Edited enemy art has no Shitroid colors.");
        for (int frame = 0; frame < ShitroidColorDefinitions.NormalFrameCount; frame++)
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
            (EnemyTileArtworkFiles.Load(stockDirectory, overrides)
                .Bosses.ShitroidColors ?? throw new InvalidOperationException("Verification requires EnemyTileArtworkFiles.Load(stockDirectory, overrides) .ShitroidColors.")).NormalColor(7, 2),
            "Shitroid color override survives catalog reload");

        CartridgeReadGuard guard = new CartridgeReadGuard(rom).Forbid(
            address => address is >= MotherBrainSourceDefinitions.Shitroid.NormalCycle and
                    < (MotherBrainSourceDefinitions.Shitroid.NormalCycle +
                        ShitroidColorDefinitions.NormalFrameCount *
                        ShitroidColorDefinitions.NormalColorsPerFrame * sizeof(ushort))
                    or >= MotherBrainSourceDefinitions.Shitroid.DeadSidehopperTarget and
                    < (MotherBrainSourceDefinitions.Shitroid.ShitroidTarget +
                        ShitroidColorDefinitions.TargetColorCount * sizeof(ushort)),
            address => $"Live Shitroid read migrated color ROM ${address:X6}.");
        var cgram = new SamusColourRig();
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance) { TileArtwork = edited.Enemies, BossArtwork = edited.Bosses, AreaArtwork = edited.Areas };
        Type type = typeof(RoomEnemySystem);
        enemies.Presentation.BindColours(cgram.Gameplay);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        enemies.ShitroidBehavior.InitializeShitroid(slot, 0);
        ShitroidEnemyState state = enemies.Shitroid ??
            throw new InvalidDataException("Shitroid initialization produced no state.");
        CheckLiveTarget(ShitroidColorTarget.Sidehopper, 0x90);
        CheckLiveTarget(ShitroidColorTarget.Shitroid, 0xa0);
        CheckLiveTarget(ShitroidColorTarget.DeadSidehopper, 0xf0);
        for (int frame = 0; frame < ShitroidColorDefinitions.NormalFrameCount; frame++)
        {
            state.PalettePhase = (frame + 7) % 8;
            state.PaletteTimer = 0;
            enemies.ShitroidBehavior.StepShitroidNormalPalette(state);
            AssertEqual(frame, state.PalettePhase, $"Shitroid normal frame {frame} advances to the next palette phase");
            AssertEqual(state.PaletteDelay, state.PaletteTimer, $"Shitroid normal frame {frame} reloads the palette timer");
            for (int color = 0; color < ShitroidColorDefinitions.NormalColorsPerFrame; color++)
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
        AssertThrows<InvalidDataException>(() => ShitroidColorFile.Write(visual),
            "Shitroid RGB5 channel outside five-bit precision is rejected");
        File.Delete(overrideFile);
        File.WriteAllBytes(file, [0]);
        AssertThrows<InvalidDataException>(() => EnemyTileArtworkFiles.Load(stockDirectory, null),
            "corrupt stock Shitroid colors fail manifest hash validation");
        File.WriteAllBytes(file, stockJson);
        Console.WriteLine("  Shitroid colors: 80 native RGB5 words, native timer selection, three live target fades, persistent override, ROM guard and strict failures pass.");

        void VerifyTarget(ShitroidColorTarget target, int source)
        {
            for (int color = 0; color < ShitroidColorDefinitions.TargetColorCount; color++)
                AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom), source + color * 2),
                    native.TargetColor(target, color),
                    $"installed Shitroid {target} target color {color}");
        }

        void CheckLiveTarget(ShitroidColorTarget target, int destination)
        {
            for (int color = 0; color < ShitroidColorDefinitions.TargetColorCount; color++)
            {
                RgbColor expected = colors.TargetColor(target, color);
                AssertEqual(expected, state.TargetColours(target)[color],
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
}
