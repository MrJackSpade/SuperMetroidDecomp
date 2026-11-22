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
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>Checks Baby Metroid's normal palette cycle and three target fades against installed art and native RGB5 data.</summary>
internal static class EnemyTileArtworkBabyMetroidColorsVerification
{
    /// <summary>JSON options: Case-insensitive property matching.</summary>
    private static readonly JsonSerializerOptions CaseInsensitiveJson = new() { PropertyNameCaseInsensitive = true };

    /// <summary>Exercises installed colors through live CGRAM consumers while preserving native timer selection and forbidding source reads.</summary>
    internal static void VerifyInstalledBabyMetroidColors(
        ISnesAddressSpace rom, string stockDirectory, ImportEnemyArtwork stock)
    {
        BabyMetroidColorCatalog native = stock.Bosses.BabyMetroidColors ??
            throw new InvalidDataException("Installed enemy art has no Shitroid colors.");
        for (int frame = 0; frame < BabyMetroidColorDefinitions.NormalFrameCount; frame++)
        for (int color = 0; color < BabyMetroidColorDefinitions.NormalColorsPerFrame; color++)
            AssertEqual(RomDataReader.ReadWordFixedBank(rom,
                    MotherBrainSourceDefinitions.BabyMetroidEnemy.NormalCycle +
                    (frame * BabyMetroidColorDefinitions.NormalColorsPerFrame + color) * 2),
                native.NormalColor(frame, color),
                $"installed Shitroid normal frame {frame} color {color}");
        Suite(nameof(VerifyTarget), () => VerifyTarget(BabyMetroidColorTarget.Sidehopper, MotherBrainSourceDefinitions.BabyMetroidEnemy.SidehopperTarget));
        Suite(nameof(VerifyTarget), () => VerifyTarget(BabyMetroidColorTarget.BabyMetroid, MotherBrainSourceDefinitions.BabyMetroidEnemy.BabyMetroidTarget));
        Suite(nameof(VerifyTarget), () => VerifyTarget(BabyMetroidColorTarget.DeadSidehopper,
            MotherBrainSourceDefinitions.BabyMetroidEnemy.DeadSidehopperTarget));

        string file = Path.Combine(stockDirectory, BabyMetroidColorFormat.FileName);
        byte[] stockJson = File.ReadAllBytes(file);
        BabyMetroidColorDocument visual = JsonSerializer.Deserialize<BabyMetroidColorDocument>(
            stockJson, CaseInsensitiveJson) ??
            throw new InvalidDataException("Stock Shitroid color JSON is null.");
        BabyMetroidColorCatalog colors = native;

        CartridgeReadGuard guard = new CartridgeReadGuard(rom).Forbid(
            address => address is >= MotherBrainSourceDefinitions.BabyMetroidEnemy.NormalCycle and
                    < (MotherBrainSourceDefinitions.BabyMetroidEnemy.NormalCycle +
                        BabyMetroidColorDefinitions.NormalFrameCount *
                        BabyMetroidColorDefinitions.NormalColorsPerFrame * sizeof(ushort))
                    or >= MotherBrainSourceDefinitions.BabyMetroidEnemy.DeadSidehopperTarget and
                    < (MotherBrainSourceDefinitions.BabyMetroidEnemy.BabyMetroidTarget +
                        BabyMetroidColorDefinitions.TargetColorCount * sizeof(ushort)),
            address => $"Live Shitroid read migrated color ROM ${address:X6}.");
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(stock.Enemies), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, installedBosses: stock.Bosses);
        // The room's population load sets the Baby Metroid up as its entry places it.
        RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
        place.SetSpawnParameter1(EnemyDefinitionId.BabyMetroid, 0);
        _ = enemies.LoadAuthoredPopulation();
        BabyMetroidEnemyState state = place.RequireSpeciesState<BabyMetroidEnemyState>();
        // The Baby Metroid draws in its entry's target colours, which the import pre-colours its sheets in (#1512); the drained
        // corpses' target is the corpses' own drained shade.
        AssertEqual(BabyMetroidShades.InTargetColours, place.Shading, "the Shitroid draws in its entry's target colours");
        IReadOnlyList<SheetShade> shades = MotherBrainSheetShades.BabyMetroid(stock.Bosses);
        CheckLiveTarget(BabyMetroidColorTarget.Sidehopper, 0x90);
        CheckLiveTarget(BabyMetroidColorTarget.BabyMetroid, 0xa0);
        for (int frame = 0; frame < BabyMetroidColorDefinitions.NormalFrameCount; frame++)
        {
            state.ResumeMembrane(BabyMetroidMembranePace.Calm, BabyMetroidMembranePulse.All[(frame + 7) % 8], 1);
            enemies.BabyMetroidBehavior.StepBabyMetroidMembranePulse(place, state);
            AssertEqual<BabyMetroidMembranePulse?>(BabyMetroidMembranePulse.All[frame], state.Membrane.Shown, $"Shitroid normal frame {frame} advances to the next palette phase");
            // The calm pulse holds each phase as the native colour timer reloaded with ten lasts, and one update more (#2404).
            AssertEqual(10 + 1, state.Membrane.UpdatesLeft, $"Shitroid normal frame {frame} holds its phase as the native palette timer");
            AssertEqual(BabyMetroidShades.InTargetColours.With(BabyMetroidShades.Membrane(BabyMetroidMembranePulse.All[frame])), place.Shading,
                $"live Shitroid draws its membrane at normal frame {frame}");
            ushort[] cgram = BossShadeColours.Laid(place.Shading.Shades, shades, BossShadeColours.Sprites, 0);
            for (int color = 0; color < BabyMetroidColorDefinitions.NormalColorsPerFrame; color++)
                AssertEqual(colors.NormalColor(frame, color).ToWord(), cgram[165 + color],
                    $"live Shitroid normal frame {frame} color {color}");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "live Shitroid palette consumers avoid migrated RGB5 ROM reads");

        visual.Normal[0][2] = visual.Normal[0][2] with { Green = 32 };
        AssertThrows<InvalidDataException>(() => BabyMetroidColorFile.Read(new MemoryStream(BabyMetroidColorFile.Write(visual), writable: false)),
            "reading Shitroid colors rejects an RGB5 channel outside five-bit precision");
        Console.WriteLine("  Shitroid colors: 80 native RGB5 words, native timer selection, three live target fades, ROM guard and strict failures pass.");

        void VerifyTarget(BabyMetroidColorTarget target, int source)
        {
            for (int color = 0; color < BabyMetroidColorDefinitions.TargetColorCount; color++)
                AssertEqual(RomDataReader.ReadWordFixedBank(rom, source + color * 2),
                    native.TargetColor(target, color),
                    $"installed Shitroid {target} target color {color}");
        }

        void CheckLiveTarget(BabyMetroidColorTarget target, int destination)
        {
            ushort[] cgram = BossShadeColours.Laid([TourianCorpseShades.BabyMetroidTargets], shades, BossShadeColours.Sprites, 0);
            // Colour zero of each row is the transparent backdrop, which the entry's load leaves alone.
            for (int color = 1; color < BabyMetroidColorDefinitions.TargetColorCount; color++)
                AssertEqual(colors.TargetColor(target, color).ToWord(), cgram[destination + color],
                    $"live Shitroid {target} colour {color}");
        }
    }
}
