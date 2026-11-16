using SuperMetroid.Core.Bosses.Botwoon;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Bosses;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Core.World;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.AssetExtraction.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Enemies.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Bosses.Botwoon;

/// <summary>Checks Botwoon's typed health bands against the cartridge's thresholds through the production band update.</summary>
internal static class BotwoonHealthPaletteDefinitionsVerification
{
    /// <summary>
    /// Each band's threshold is the cartridge's, and the production update shows the next band at exactly the healths the
    /// cartridge's signed comparison does, holds at the threshold, advances one band an update and stops after the last.
    /// </summary>
    internal static void VerifyBotwoonHealthPaletteDefinitions(CartridgeImportAddressSpace rom)
    {
        var guard = NewBotwoonHealthThresholdReadGuard(rom);
        using var colors = new MemoryStream(BotwoonColorExtractor.Extract(rom), writable: false);
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledFrames(EnemyTileSourceArtwork.FromArtworkForVerification(new EnemyTileArtworkParts { Sheets = new Dictionary<EnemyDefinitionId, RoomCharacterAtlas>(), Palettes = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>() })), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, installedBosses: new BossSourceArtwork { BotwoonColors = BotwoonColorFile.Read(colors) });
        Action<RoomEnemyPlace, BotwoonEnemyState> update = BotwoonBehavior.UpdateBotwoonHealthBand;
        RoomEnemyPlace head = enemies.Places.InUpdateOrder[0];
        var state = new BotwoonEnemyState(body: []);
        IReadOnlyList<BotwoonHealthBand> bands = BotwoonHealthBandDefinitions.InOrderShown;
        AssertEqual(8, bands.Count, "Botwoon has eight health bands");

        for (int index = 0; index < bands.Count; index++)
        {
            BotwoonHealthBand? before = index == 0 ? null : bands[index - 1];
            ushort threshold = CartridgeImportSource.Require(rom).ReadCartridgeWord(BotwoonHealthBandDefinitions.NativeThresholdAddress + index * sizeof(ushort));
            AssertEqual((int)threshold, bands[index].HealthBelow, $"Botwoon band {bands[index]} threshold");
            // Every health a boss holds: the signed difference the game compares is then the plain difference.
            for (int health = 0; health <= short.MaxValue; health++)
            {
                bool expected = unchecked((short)((ushort)health - threshold)) < 0;
                state.HealthBand = before;
                head.Health = health;
                update(head, state);
                AssertEqual(expected ? bands[index] : before, state.HealthBand,
                    $"Botwoon after {before?.ToString() ?? "his own colours"} at health {health}");
            }
        }

        state.HealthBand = null;
        head.Health = 0;
        update(head, state);
        AssertEqual(bands[0], state.HealthBand, "production Botwoon advances one band an update");
        state.HealthBand = BotwoonHealthBandDefinitions.BelowEighth;
        update(head, state);
        AssertEqual(BotwoonHealthBandDefinitions.BelowEighth, state.HealthBand,
            "production Botwoon's last band remains terminal");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production Botwoon palette progression performs no threshold reads");

        Console.WriteLine(
            "Botwoon health palette definitions: eight native thresholds and all " +
            "262,144 signed health/band decisions match through the production update; one-band advance " +
            "and the terminal band run with the threshold table forbidden.");
    }

    /// <summary>Forbids runtime access to the 16-byte native Botwoon threshold table.</summary>
    private static CartridgeReadGuard NewBotwoonHealthThresholdReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).ForbidRange(BotwoonHealthBandDefinitions.NativeThresholdAddress, 16,
            address => $"Botwoon palette progression attempted threshold read ${address:X6}.");
}
