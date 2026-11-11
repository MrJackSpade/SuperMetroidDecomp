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
using SuperMetroid.Core.Enemies.Corpses.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>Checks Shitroid's normal palette cycle and three target fades against installed art and native RGB5 data.</summary>
internal static class EnemyTileArtworkShitroidColorsVerification
{
    /// <summary>JSON options: Case-insensitive property matching.</summary>
    private static readonly JsonSerializerOptions CaseInsensitiveJson = new() { PropertyNameCaseInsensitive = true };

    /// <summary>Exercises installed colors through live CGRAM consumers while preserving native timer selection and forbidding source reads.</summary>
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
        ShitroidColorCatalog colors = native;

        CartridgeReadGuard guard = new CartridgeReadGuard(rom).Forbid(
            address => address is >= MotherBrainSourceDefinitions.Shitroid.NormalCycle and
                    < (MotherBrainSourceDefinitions.Shitroid.NormalCycle +
                        ShitroidColorDefinitions.NormalFrameCount *
                        ShitroidColorDefinitions.NormalColorsPerFrame * sizeof(ushort))
                    or >= MotherBrainSourceDefinitions.Shitroid.DeadSidehopperTarget and
                    < (MotherBrainSourceDefinitions.Shitroid.ShitroidTarget +
                        ShitroidColorDefinitions.TargetColorCount * sizeof(ushort)),
            address => $"Live Shitroid read migrated color ROM ${address:X6}.");
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(stock.Enemies), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance) { BossArtwork = stock.Bosses };
        Type type = typeof(RoomEnemySystem);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        enemies.ShitroidBehavior.InitializeShitroid(slot, 0);
        ShitroidEnemyState state = enemies.Shitroid ??
            throw new InvalidDataException("Shitroid initialization produced no state.");
        // The Shitroid draws in its entry's target colours, which the import pre-colours its sheets in (#1512); the drained
        // corpses' target is the corpses' own drained shade.
        AssertEqual(ShitroidShades.InTargetColours, slot.Shading, "the Shitroid draws in its entry's target colours");
        IReadOnlyList<SheetShade> shades = MotherBrainSheetShades.Shitroid(stock.Bosses);
        CheckLiveTarget(ShitroidColorTarget.Sidehopper, 0x90);
        CheckLiveTarget(ShitroidColorTarget.Shitroid, 0xa0);
        for (int frame = 0; frame < ShitroidColorDefinitions.NormalFrameCount; frame++)
        {
            state.MembranePulsePhase = (frame + 7) % 8;
            state.PaletteTimer = 0;
            enemies.ShitroidBehavior.StepShitroidMembranePulse(state);
            AssertEqual(frame, state.MembranePulsePhase, $"Shitroid normal frame {frame} advances to the next palette phase");
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

        visual.Normal[0][2] = visual.Normal[0][2] with { Green = 32 };
        AssertThrows<InvalidDataException>(() => ShitroidColorFile.Read(new MemoryStream(ShitroidColorFile.Write(visual), writable: false)),
            "reading Shitroid colors rejects an RGB5 channel outside five-bit precision");
        Console.WriteLine("  Shitroid colors: 80 native RGB5 words, native timer selection, three live target fades, ROM guard and strict failures pass.");

        void VerifyTarget(ShitroidColorTarget target, int source)
        {
            for (int color = 0; color < ShitroidColorDefinitions.TargetColorCount; color++)
                AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom), source + color * 2),
                    native.TargetColor(target, color),
                    $"installed Shitroid {target} target color {color}");
        }

        void CheckLiveTarget(ShitroidColorTarget target, int destination)
        {
            ushort[] cgram = BossShadeColours.Laid([TourianCorpseShades.ShitroidTargets], shades, BossShadeColours.Sprites, 0);
            // Colour zero of each row is the transparent backdrop, which the entry's load leaves alone.
            for (int color = 1; color < ShitroidColorDefinitions.TargetColorCount; color++)
                AssertEqual(colors.TargetColor(target, color).ToWord(), cgram[destination + color],
                    $"live Shitroid {target} colour {color}");
        }
    }
}
