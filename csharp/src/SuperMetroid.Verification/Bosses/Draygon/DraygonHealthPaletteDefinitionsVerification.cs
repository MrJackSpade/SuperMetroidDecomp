using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Bosses;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Core.World;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.AssetExtraction.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Enemies.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

namespace SuperMetroid.Verification.Bosses.Draygon;

/// <summary>Checks Draygon's compiled health bands through production palette selection.</summary>
internal static class DraygonHealthPaletteDefinitionsVerification
{
    /// <summary>Compares eight retail thresholds, all authored health values, and boundary updates without table reads.</summary>
    internal static void VerifyDraygonHealthPaletteDefinitions(CartridgeImportAddressSpace rom)
    {

        ushort[] thresholds = new ushort[8];
        for (int index = 0; index < thresholds.Length; index++)
        {
            thresholds[index] = CartridgeImportSource.Require(rom).ReadCartridgeWord((DraygonHealthShadeDefinitions.NativeThresholdAddress + index * 2));
        }
        AssertEqual((ushort)0xffff,
            CartridgeImportSource.Require(rom).ReadCartridgeWord((DraygonHealthShadeDefinitions.NativeThresholdAddress + 16)),
            "Draygon health threshold native terminator");

        for (ushort health = 0;
             health <= DraygonHealthShadeDefinitions.MaximumAuthoredHealth;
             health++)
        {
            int expectedIndex = Array.FindIndex(
                thresholds,
                threshold => unchecked((short)(health - threshold)) >= 0);
            AssertTrue(expectedIndex >= 0, $"Draygon health {health} matches a native band");
            AssertEqual(expectedIndex,
                DraygonHealthShades.BandForHealth(health),
                $"Draygon health {health} compiled palette band");
        }

        AssertThrows<InvalidDataException>(
            () => DraygonHealthShades.BandForHealth(6001),
            "Draygon rejects health above its authored enemy-header maximum");

        var guard = NewDraygonHealthThresholdReadGuard(rom);
        using var colors = new MemoryStream(DraygonColorExtractor.Extract(rom), writable: false);
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledFrames(EnemyTileSourceArtwork.FromArtworkForVerification(new EnemyTileArtworkParts { Sheets = new Dictionary<EnemyDefinitionId, RoomCharacterAtlas>(), Palettes = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>() })), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance)
        {
            BossArtwork = new BossSourceArtwork { DraygonColors = DraygonColorFile.Read(colors) },
        };
        Action<DraygonEnemyState> update = DraygonCombat.UpdateDraygonHealthBand;
        var state = new DraygonEnemyState(enemies.SlotTable.Slots[0]);

        ushort[] productionHealth =
            [6000, 5250, 5249, 4500, 4499, 3750, 3749, 3000, 2999,
             2250, 2249, 1500, 1499, 750, 749, 1, 0];
        foreach (ushort health in productionHealth)
        {
            state.Body.Health = health;
            state.HealthBand = null;
            update(state);
            AssertEqual(
                DraygonHealthShades.BandForHealth(health),
                state.HealthBand,
                $"production Draygon health {health} palette band");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production Draygon palette selection performs no threshold-table reads");

        state.Body.Health = 6001;
        state.HealthBand = null;
        AssertThrows<InvalidDataException>(
            () => update(state),
            "production Draygon rejects malformed restored health without a ROM fallback");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "malformed Draygon health performs no threshold-table read");

        Console.WriteLine(
            "Draygon health palette definitions: all eight reachable thresholds, 6,001 " +
            "authored health values and every band boundary pass; the production selector " +
            "runs with the threshold table and terminator forbidden.");
    }

    /// <summary>Forbids reads of the native threshold words and their terminator during palette selection.</summary>
    private static CartridgeReadGuard NewDraygonHealthThresholdReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .ForbidRange(DraygonHealthShadeDefinitions.NativeThresholdAddress, 18,
                address => $"Draygon palette selection attempted threshold read ${address:X6}.");
}
