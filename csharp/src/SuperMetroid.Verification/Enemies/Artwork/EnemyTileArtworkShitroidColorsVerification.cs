using SuperMetroid.AssetExtraction.Video;
using System.Text.Json;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Core.World;
using SuperMetroid.AssetExtraction.Bosses.MotherBrain;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;
using SuperMetroid.Verification.Bosses;
using SuperMetroid.AssetExtraction.Bosses;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;

namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>Checks Shitroid's normal palette cycle and three target fades against installed art and native RGB5 data.</summary>
internal static class EnemyTileArtworkShitroidColorsVerification
{
    /// <summary>JSON options: Case-insensitive property matching.</summary>
    private static readonly JsonSerializerOptions CaseInsensitiveJson = new() { PropertyNameCaseInsensitive = true };

    /// <summary>Exercises edited colors through live CGRAM consumers while preserving native timer selection and forbidding source reads.</summary>
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
        InstalledEnemyArtwork edited = stock with { Bosses = stock.Bosses with { ShitroidColors = EnemyTileArtworkFiles.Open(stockDirectory, overrides).ShitroidColors() } };
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
            EnemyTileArtworkFiles.Open(stockDirectory, overrides).ShitroidColors().NormalColor(7, 2),
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
        var enemies = new RoomEnemySystem(new EnemySession(new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()), definitions: CompiledRoomEnemyDefinitions.Instance) { TileArtwork = edited.Enemies, BossArtwork = edited.Bosses };
        Type type = typeof(RoomEnemySystem);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        enemies.ShitroidBehavior.InitializeShitroid(slot, 0);
        ShitroidEnemyState state = enemies.Shitroid ??
            throw new InvalidDataException("Shitroid initialization produced no state.");
        // The Shitroid draws in its entry's target colours, which the import pre-colours its sheets in (#1512); the drained
        // corpses' target is the corpses' own drained shade.
        AssertEqual(ShitroidShades.InTargetColours, slot.Shading, "the Shitroid draws in its entry's target colours");
        IReadOnlyList<SheetShade> shades = MotherBrainSheetShades.Shitroid(edited.Bosses);
        CheckLiveTarget(ShitroidColorTarget.Sidehopper, 0x90);
        CheckLiveTarget(ShitroidColorTarget.Shitroid, 0xa0);
        for (int frame = 0; frame < ShitroidColorDefinitions.NormalFrameCount; frame++)
        {
            state.PalettePhase = (frame + 7) % 8;
            state.PaletteTimer = 0;
            enemies.ShitroidBehavior.StepShitroidNormalPalette(state);
            AssertEqual(frame, state.PalettePhase, $"Shitroid normal frame {frame} advances to the next palette phase");
            AssertEqual(state.PaletteDelay, state.PaletteTimer, $"Shitroid normal frame {frame} reloads the palette timer");
            AssertEqual(ShitroidShades.InTargetColours.With(ShitroidShades.Membrane(frame)), slot.Shading,
                $"live Shitroid draws its membrane at normal frame {frame}");
            ushort[] cgram = BossShadeColours.Laid(slot.Shading.Shades, shades, BossShadeColours.Sprites, 0);
            for (int color = 0; color < ShitroidColorDefinitions.NormalColorsPerFrame; color++)
                AssertEqual(colors.NormalColor(frame, color).ToWord(), cgram[165 + color],
                    $"live Shitroid normal frame {frame} color {color}");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "live Shitroid palette consumers avoid migrated RGB5 ROM reads");

        File.WriteAllBytes(overrideFile, [0]);
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Open(stockDirectory, overrides).ShitroidColors(),
            "invalid Shitroid color override fails loudly");
        File.WriteAllBytes(overrideFile,
            "{\"version\":1,\"version\":1}"u8.ToArray());
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Open(stockDirectory, overrides).ShitroidColors(),
            "duplicate Shitroid color property fails loudly");
        visual.Normal[0][2] = visual.Normal[0][2] with { Green = 32 };
        AssertThrows<InvalidDataException>(() => ShitroidColorFile.Read(new MemoryStream(ShitroidColorFile.Write(visual), writable: false)),
            "reading Shitroid colors rejects an RGB5 channel outside five-bit precision");
        File.Delete(overrideFile);
        File.WriteAllBytes(file, [0]);
        AssertThrows<InvalidDataException>(() => EnemyTileArtworkFiles.Open(stockDirectory, null).ShitroidColors(),
            "corrupt stock Shitroid colors fail loudly where it is read");
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
            ushort[] cgram = BossShadeColours.Laid([ShitroidShades.Targets], shades, BossShadeColours.Sprites, 0);
            // Colour zero of each row is the transparent backdrop, which the entry's load leaves alone.
            for (int color = 1; color < ShitroidColorDefinitions.TargetColorCount; color++)
                AssertEqual(colors.TargetColor(target, color).ToWord(), cgram[destination + color],
                    $"live Shitroid {target} colour {color}");
        }

        static PaletteRgb5 ChangeRed(PaletteRgb5 rgb) => rgb with
        {
            Red = rgb.Red == 31 ? 30 : rgb.Red + 1,
        };
    }
}
