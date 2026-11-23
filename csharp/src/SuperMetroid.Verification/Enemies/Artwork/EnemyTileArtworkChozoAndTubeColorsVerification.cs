using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Animation;
using System.Text.Json;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Chozo;
using SuperMetroid.Core.Enemies.Chozo.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Core.World;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Enemies.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>Checks installed Chozo statue and n00b-tube palette bands against native RGB5 data and the shading their live initializers publish.</summary>
internal static class EnemyTileArtworkChozoAndTubeColorsVerification
{
    /// <summary>JSON options: Case-insensitive property matching.</summary>
    private static readonly JsonSerializerOptions CaseInsensitiveJson = new() { PropertyNameCaseInsensitive = true };

    /// <summary>Checks three installed color bands, and real initializers while preserving native instructions and PLM requests.</summary>
    internal static void VerifyInstalledChozoAndTubeColors(
        ISnesAddressSpace rom, string stockDirectory, ImportEnemyArtwork stock)
    {
        ChozoAndTubeColorCatalog native = stock.Enemies.ChozoAndTubeColors ??
            throw new InvalidDataException("Installed enemy art has no Chozo/tube colors.");
        Suite(nameof(VerifyBand), () => VerifyBand(EnemySpeciesSourceDefinitions.ChozoAndTube.TubeCracksColors,
            native.ResolveTubeCracks, "tube cracks"));
        Suite(nameof(VerifyBand), () => VerifyBand(EnemySpeciesSourceDefinitions.ChozoAndTube.WreckedShipColors,
            native.ResolveWreckedShip, "Wrecked Ship Chozo"));
        Suite(nameof(VerifyBand), () => VerifyBand(EnemySpeciesSourceDefinitions.ChozoAndTube.LowerNorfairColors,
            native.ResolveLowerNorfair, "Lower Norfair Chozo"));

        string file = Path.Combine(stockDirectory, ChozoAndTubeColorFormat.FileName);
        byte[] stockJson = File.ReadAllBytes(file);
        ChozoAndTubeColorDocument visual =
            JsonSerializer.Deserialize<ChozoAndTubeColorDocument>(stockJson, global::SuperMetroid.Verification.VerificationJson.Info<global::SuperMetroid.AssetExtraction.Enemies.ChozoAndTubeColorDocument>(CaseInsensitiveJson)) ??
            throw new InvalidDataException("Stock Chozo/tube color JSON is null.");
        ChozoAndTubeColorCatalog colors = native;

        // Every live initializer must use the installed data, while variant selection,
        // instruction identity and native PLM requests remain engine-owned.
        Type type = typeof(RoomEnemySystem);
        var guarded = NewChozoAndTubePaletteReadGuard(rom);
        Suite(nameof(VerifyLive), () => VerifyLive(0, PictureShading.Shaded(ChozoAndTubeShades.WreckedShipStatue),
            "Wrecked Ship Chozo", (enemies, statue) => enemies.ChozoStatueBehavior.InitializeChozoStatue(statue.HeldOccupant().SpawnSettings<ChozoStatueSpawnSettings>(), statue),
            ChozoStatueSegment.WreckedShipIdle));
        Suite(nameof(VerifyLive), () => VerifyLive(2, PictureShading.Shaded(ChozoAndTubeShades.LowerNorfairStatue),
            "Lower Norfair Chozo", (enemies, statue) => enemies.ChozoStatueBehavior.InitializeChozoStatue(statue.HeldOccupant().SpawnSettings<ChozoStatueSpawnSettings>(), statue),
            ChozoStatueSegment.LowerNorfairIdle));

        visual.TubeCracks[5] = visual.TubeCracks[5] with { Blue = 32 };
        AssertThrows<InvalidDataException>(() => ChozoAndTubeColorFile.Read(new MemoryStream(ChozoAndTubeColorFile.Write(visual))),
            "Chozo/tube RGB5 channel outside five-bit precision is rejected");
        Console.WriteLine("  Chozo/tube colors: 96 native RGB5 words, all three live initializers, ROM guard, and strict failures pass.");

        void VerifyBand(int source, Func<int, RgbColor> resolve, string name)
        {
            for (int color = 0; color < ChozoAndTubeColorDefinitions.ColorCount; color++)
                AssertEqual(RomDataReader.ReadWordFixedBank(rom,
                    source + color * sizeof(ushort)), resolve(color),
                    $"installed {name} color {color} preserves native RGB5");
        }

        void VerifyLive(ushort variant, PictureShading expectedShading, string name,
            Func<RoomEnemySystem, RoomEnemyPlace, ChozoStatueState> initialize, ChozoStatueSegment? expectedProgram)
        {
            var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(stock.Enemies), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, installedBosses: stock.Bosses);
            RoomEnemyPlace statue = enemies.Places.InUpdateOrder[0];
            statue.SetSpawnParameter2(EnemyDefinitionId.Chozo, variant);
            // The statue's setup builds the state its animation runs on.
            ChozoStatueState state = initialize(enemies, statue);
            // The import pre-colours each shade from the installed band whose words VerifyBand matched to the cartridge.
            AssertEqual(expectedShading, statue.Shading, $"live {name} shows its installed shade");
            if (expectedProgram is not null)
            {
                AssertEqual(new AnimationPlayback<ChozoStatueSegment>(ChozoStatueAnimations.StartingWith(expectedProgram.Value), AnimationPlace.StartOfAnimation),
                    state.Playback,
                    $"{name} keeps native animation selection");
                // The native PLM request is the statue's hand, which every site asks its room for (#2562).
                AssertTrue(enemies.PassEvents.Published.OfType<SuperMetroid.Core.Enemies.Chozo.ChozoStatueHandAsked>().Any(),
                    $"{name} still publishes its native PLM request");
            }
        }
    }

    /// <summary>Rejects reads of the migrated palette region during enemy initialization.</summary>
    private static CartridgeReadGuard NewChozoAndTubePaletteReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => address is >= EnemySpeciesSourceDefinitions.ChozoAndTube.TubeCracksColors and < (EnemySpeciesSourceDefinitions.ChozoAndTube.LowerNorfairColors + ChozoAndTubeColorDefinitions.ColorCount * sizeof(ushort)),
                address => $"Chozo/tube accessed migrated palette ROM ${address:X6}.");
}
