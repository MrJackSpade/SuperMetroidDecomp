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

/// <summary>Checks Shitroid's normal palette cycle and three target fades against installed art and native RGB5 data.</summary>
internal static class EnemyTileArtworkShitroidColorsVerification
{
    /// <summary>JSON options: Case-insensitive property matching.</summary>
    private static readonly JsonSerializerOptions CaseInsensitiveJson = new() { PropertyNameCaseInsensitive = true };

    /// <summary>Exercises installed colors through live CGRAM consumers while preserving native timer selection and forbidding source reads.</summary>
    internal static void VerifyInstalledShitroidColors(
        ISnesAddressSpace rom, string stockDirectory, ImportEnemyArtwork stock)
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
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(stock.Enemies), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, installedBosses: stock.Bosses);
        Type type = typeof(RoomEnemySystem);
        RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
        ShitroidBehavior.InitializeShitroid(place, 0);
        ShitroidEnemyState state = place.RequireSpeciesState<ShitroidEnemyState>();
        // The Shitroid draws in its entry's target colours, which the import pre-colours its sheets in (#1512); the drained
        // corpses' target is the corpses' own drained shade.
        AssertEqual(ShitroidShades.InTargetColours, place.Shading, "the Shitroid draws in its entry's target colours");
        IReadOnlyList<SheetShade> shades = MotherBrainSheetShades.Shitroid(stock.Bosses);
        CheckLiveTarget(ShitroidColorTarget.Sidehopper, 0x90);
        CheckLiveTarget(ShitroidColorTarget.Shitroid, 0xa0);
        for (int frame = 0; frame < ShitroidColorDefinitions.NormalFrameCount; frame++)
        {
            state.ResumeMembrane(ShitroidMembranePace.Calm, ShitroidMembranePulse.All[(frame + 7) % 8], 1);
            enemies.ShitroidBehavior.StepShitroidMembranePulse(place, state);
            AssertEqual<ShitroidMembranePulse?>(ShitroidMembranePulse.All[frame], state.Membrane.Shown, $"Shitroid normal frame {frame} advances to the next palette phase");
            // The calm pulse holds each phase as the native colour timer reloaded with ten lasts, and one update more (#2404).
            AssertEqual(10 + 1, state.Membrane.UpdatesLeft, $"Shitroid normal frame {frame} holds its phase as the native palette timer");
            AssertEqual(ShitroidShades.InTargetColours.With(ShitroidShades.Membrane(ShitroidMembranePulse.All[frame])), place.Shading,
                $"live Shitroid draws its membrane at normal frame {frame}");
            ushort[] cgram = BossShadeColours.Laid(place.Shading.Shades, shades, BossShadeColours.Sprites, 0);
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
