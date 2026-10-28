using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Bosses;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Bosses.Draygon;

/// <summary>Checks Draygon's compiled health bands through production palette selection.</summary>
internal static class DraygonHealthPaletteDefinitionsVerification
{
    /// <summary>Compares eight retail thresholds, all authored health values, and boundary updates without table reads.</summary>
    internal static void VerifyDraygonHealthPaletteDefinitions(SuperMetroidAddressSpace rom)
    {

        ushort[] thresholds = new ushort[8];
        for (int index = 0; index < thresholds.Length; index++)
        {
            thresholds[index] = CartridgeImportSource.Require(rom).ReadCartridgeWord((DraygonHealthPaletteDefinitions.NativeThresholdAddress + index * 2));
        }
        AssertEqual((ushort)0xffff,
            CartridgeImportSource.Require(rom).ReadCartridgeWord((DraygonHealthPaletteDefinitions.NativeThresholdAddress + 16)),
            "Draygon health threshold native terminator");

        for (ushort health = 0;
             health <= DraygonHealthPaletteDefinitions.MaximumAuthoredHealth;
             health++)
        {
            int expectedIndex = Array.FindIndex(
                thresholds,
                threshold => unchecked((short)(health - threshold)) >= 0);
            AssertTrue(expectedIndex >= 0, $"Draygon health {health} matches a native band");
            AssertEqual(expectedIndex,
                DraygonHealthPalette.BandForHealth(health),
                $"Draygon health {health} compiled palette band");
        }

        AssertThrows<InvalidDataException>(
            () => DraygonHealthPalette.BandForHealth(6001),
            "Draygon rejects health above its authored enemy-header maximum");

        var guard = NewDraygonHealthThresholdReadGuard(rom);
        using var colors = new MemoryStream(DraygonColorExtractor.Extract(rom), writable: false);
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance)
        {
            TileArtwork = EnemyTileArtworkCatalog.FromArtworkForVerification(new EnemyTileArtworkParts { Sheets = new Dictionary<EnemyDefinitionId, RoomCharacterAtlas>(), Palettes = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>() }), BossArtwork = new BossArtworkCatalog { DraygonColors = DraygonColorFile.Read(colors) },
        };
        enemies.Presentation.BindColours(new GameplayColours(new SpriteColourSets()));
        Action<DraygonEnemyState> update = enemies.DraygonBehavior.Combat.UpdateDraygonHealthPalette;
        var state = new DraygonEnemyState(enemies.SlotTable.Slots[0]);

        ushort[] productionHealth =
            [6000, 5250, 5249, 4500, 4499, 3750, 3749, 3000, 2999,
             2250, 2249, 1500, 1499, 750, 749, 1, 0];
        foreach (ushort health in productionHealth)
        {
            state.Body.Health = health;
            state.HealthPaletteBand = null;
            update(state);
            AssertEqual(
                DraygonHealthPalette.BandForHealth(health),
                state.HealthPaletteBand,
                $"production Draygon health {health} palette band");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production Draygon palette selection performs no threshold-table reads");

        state.Body.Health = 6001;
        state.HealthPaletteBand = null;
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
            .ForbidRange(DraygonHealthPaletteDefinitions.NativeThresholdAddress, 18,
                address => $"Draygon palette selection attempted threshold read ${address:X6}.");
}
