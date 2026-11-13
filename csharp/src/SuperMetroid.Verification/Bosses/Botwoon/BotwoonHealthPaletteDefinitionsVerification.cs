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

namespace SuperMetroid.Verification.Bosses.Botwoon;

/// <summary>Checks Botwoon palette-phase health thresholds and the runtime's ROM-free progression.</summary>
internal static class BotwoonHealthPaletteDefinitionsVerification
{
    /// <summary>Compares all signed health decisions with native thresholds and drives the production palette update.</summary>
    internal static void VerifyBotwoonHealthPaletteDefinitions(CartridgeImportAddressSpace rom)
    {

        for (int phase = 0; phase < BotwoonHealthBandDefinitions.BandCount; phase++)
        {
            ushort threshold = CartridgeImportSource.Require(rom).ReadCartridgeWord(BotwoonHealthBandDefinitions.NativeThresholdAddress + phase * sizeof(ushort));
            // Every health a boss holds: the signed difference the game compares is then the plain difference.
            for (int health = 0; health <= short.MaxValue; health++)
            {
                bool expected = unchecked((short)((ushort)health - threshold)) < 0;
                AssertEqual(expected,
                    BotwoonHealthBands.ShouldAdvance(phase, health),
                    $"Botwoon phase ${phase:X2} health {health} threshold decision");
            }
        }

        foreach (int invalid in new[] { -1, 8, 9, int.MaxValue })
        {
            AssertThrows<InvalidDataException>(
                () => BotwoonHealthBands.ShouldAdvance(invalid, 0),
                $"Botwoon rejects restored palette phase {invalid}");
        }

        var guard = NewBotwoonHealthThresholdReadGuard(rom);
        using var colors = new MemoryStream(BotwoonColorExtractor.Extract(rom), writable: false);
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledFrames(EnemyTileSourceArtwork.FromArtworkForVerification(new EnemyTileArtworkParts { Sheets = new Dictionary<EnemyDefinitionId, RoomCharacterAtlas>(), Palettes = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>() })), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance)
        {
            BossArtwork = new BossSourceArtwork { BotwoonColors = BotwoonColorFile.Read(colors) },
        };
        Action<RoomEnemySlot, BotwoonEnemyState> update = BotwoonBehavior.UpdateBotwoonHealthBand;
        RoomEnemySlot head = enemies.SlotTable.Slots[0];
        var state = new BotwoonEnemyState();

        for (int phase = 0; phase < BotwoonHealthBandDefinitions.BandCount; phase++)
        {
            ushort threshold = CartridgeImportSource.Require(rom).ReadCartridgeWord(BotwoonHealthBandDefinitions.NativeThresholdAddress + phase * sizeof(ushort));
            state.HealthBandsPassed = phase;
            head.Health = threshold;
            update(head, state);
            AssertEqual(phase, state.HealthBandsPassed,
                $"production Botwoon phase ${phase:X2} holds at threshold");

            state.HealthBandsPassed = phase;
            head.Health = threshold - 1;
            update(head, state);
            AssertEqual(phase + 1, state.HealthBandsPassed,
                $"production Botwoon phase ${phase:X2} advances below threshold");
        }

        state.HealthBandsPassed = BotwoonHealthBandDefinitions.BandCount;
        update(head, state);
        AssertEqual(BotwoonHealthBandDefinitions.BandCount,
            state.HealthBandsPassed,
            "production Botwoon completed palette phase remains terminal");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production Botwoon palette progression performs no threshold reads");

        foreach (int invalid in new[] { -1, 9 })
        {
            state.HealthBandsPassed = invalid;
            AssertThrows<InvalidDataException>(
                () => update(head, state),
                $"production Botwoon rejects restored palette phase {invalid}");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "malformed Botwoon palette phase performs no threshold reads");

        Console.WriteLine(
            "Botwoon health palette definitions: eight native thresholds and all " +
            "524,288 signed health/phase decisions match; real hold, one-band advance, " +
            "terminal, and malformed paths run with the threshold table forbidden.");
    }

    /// <summary>Forbids runtime access to the 16-byte native Botwoon threshold table.</summary>
    private static CartridgeReadGuard NewBotwoonHealthThresholdReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).ForbidRange(BotwoonHealthBandDefinitions.NativeThresholdAddress, 16,
            address => $"Botwoon palette progression attempted threshold read ${address:X6}.");
}
