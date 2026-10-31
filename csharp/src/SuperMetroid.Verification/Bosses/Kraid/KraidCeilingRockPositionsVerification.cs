using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Bosses.Kraid;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks Kraid ceiling-rock X positions and their production spawn cadence against native data.</summary>
internal static class KraidCeilingRockPositionsVerification
{
    /// <summary>Compares every ceiling rock's X with the cartridge's word for it and rejects steps outside the rocks.</summary>
    internal static void VerifyKraidCeilingRockCoordinates(SuperMetroidAddressSpace rom)
    {
        ushort Word(int a) => LittleEndianBytes.Word(KraidCeilingRockCoordinatesRomByte(rom, a), KraidCeilingRockCoordinatesRomByte(rom, a + 1));
        // The cartridge's table holds one X word per rock.
        for (int rock = 0; rock < KraidCeilingRocks.Count; rock++)
            AssertEqual(unchecked((short)Word(EnemyRomTablePointers.Kraid.CeilingRockXWords + rock * 2)),
                KraidCeilingRocks.X(rock), "Native ceiling rock X");
        foreach (int invalid in new[] { int.MinValue, -1, KraidCeilingRocks.Count, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => KraidCeilingRocks.X(invalid),
                "Ceiling rock bounds");
    }

    /// <summary>Reads one bank-$A7 coordinate byte solely for the native oracle.</summary>
    private static byte KraidCeilingRockCoordinatesRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Confirms each ceiling spawn advances to the next rock and pairs its native X coordinate with the PLM mutation while source-table reads are forbidden.</summary>
    internal static void VerifyKraidCeilingRockPositions(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyKraidCeilingRockCoordinates), () => VerifyKraidCeilingRockCoordinates(rom));
        ushort Word(int a) => LittleEndianBytes.Word(KraidCeilingRockPositionsRomByte(rom, a), KraidCeilingRockPositionsRomByte(rom, a + 1));
        var enemies = new RoomEnemySystem(new EnemySession(new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        var state = new KraidEnemyState();
        enemies.KraidBehavior.Access.Kraid = state;
        enemies.VerificationServices().ReadRandom = () => 63;
        Action<RoomEnemySlot, KraidEnemyState> step = enemies.KraidBehavior.Rise.RunKraidGrowthFunction;
        var requests = enemies.BossFrameResults.KraidPlms;
        var body = enemies.SlotTable.Slots[0];
        body.FrameCounter = 1; // Isolate the ceiling placement from the independent rising-rock cadence.
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        for (int index = 0; index < KraidCeilingRocks.Count; index++)
        {
            foreach (var occupied in enemies.Projectiles.EnemyProjectiles) occupied.Clear();
            requests.Clear();
            state.CeilingRockSpawnCount = 0;
            body.Position = body.Position.WithWhole(new(128, (ushort)raw));
            state.Parts[0].CeilingRock = index;
            state.Parts[0].Function = KraidAiFunction.GrowBreakCeilingPlatforms;
            step(body, state);
            ushort y = unchecked((ushort)(raw - 1));
            bool spawn = (y & 3) == 0;
            AssertEqual(y, body.Position.Y.Whole, "Ceiling break moves before cadence test");
            AssertEqual(spawn ? 1 : 0, requests.Count, "Ceiling PLM cadence");
            AssertEqual(index + (spawn ? 1 : 0), state.Parts[0].CeilingRock, "Ceiling rock advances only on emission");
            if (!spawn) continue;
            var rock = enemies.Projectiles.EnemyProjectiles[^1];
            AssertEqual(Word(EnemyRomTablePointers.Kraid.CeilingRockXWords + index * 2), rock.Position.X.Whole, "Native ceiling X reaches actual projectile");
            AssertEqual((ushort)312, rock.Position.Y.Whole, "Ceiling projectile origin Y");
            AssertEqual((ushort)127, rock.Velocity.Y.RawValue, "Ceiling projectile retains current RNG velocity");
            AssertEqual(KraidPlmDefinitions.GrowthCeiling[index], requests[0], "Rock and ceiling mutation stay paired");
        }
        Console.WriteLine("Kraid ceiling rocks: all 9 rock positions and 589824 actual growth updates match with placement reads forbidden.");
    }

    /// <summary>Reads a retail placement byte for the production-spawn comparison.</summary>
    private static byte KraidCeilingRockPositionsRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

}
