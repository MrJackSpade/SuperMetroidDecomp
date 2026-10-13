using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static SuiteTimingVerification;

/// <summary>Checks Kraid ceiling-rock X positions and their production spawn cadence against native data.</summary>
internal static class KraidCeilingRockPositionsVerification
{
    /// <summary>Compares every overlapping byte-addressed coordinate window with the compiled table and rejects incomplete windows.</summary>
    internal static void VerifyKraidCeilingRockCoordinates(SuperMetroidAddressSpace rom)
    {
        ushort Word(int a) => (ushort)(SnesLittleEndian.Word(KraidCeilingRockCoordinatesRomByte(rom, a), KraidCeilingRockCoordinatesRomByte(rom, a + 1)));
        for (int offset = 0; offset <= 18; offset++)
            AssertEqual(Word(EnemyRomTablePointers.Kraid.CeilingRockXWords + offset),
                KraidCeilingRockPositionDefinitions.AtByteOffset(offset), "Native byte-addressed ceiling rock lookup");
        foreach (int invalid in new[] { int.MinValue, -1, 19, 20, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => KraidCeilingRockPositionDefinitions.AtByteOffset(invalid),
                "Ceiling rock byte-window bounds");
    }

    /// <summary>Reads one bank-$A7 coordinate byte solely for the native oracle.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Kraid ceiling rock coordinates")]
    private static byte KraidCeilingRockCoordinatesRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Confirms each ceiling spawn advances the byte selector and pairs its native X coordinate with the PLM mutation while source-table reads are forbidden.</summary>
    internal static void VerifyKraidCeilingRockPositions(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyKraidCeilingRockCoordinates), () => VerifyKraidCeilingRockCoordinates(rom));
        ushort Word(int a) => (ushort)(SnesLittleEndian.Word(KraidCeilingRockPositionsRomByte(rom, a), KraidCeilingRockPositionsRomByte(rom, a + 1)));
        var enemies = new RoomEnemySystem();
        var state = new KraidEnemyState();
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        enemies.BindFixtureBus(NewKraidCeilingGuard(rom));
        enemies.VerificationServices().ReadRandom = (Func<ushort>)(() => 63);
        Action<RoomEnemySlot, KraidEnemyState> step = enemies.KraidBehavior.RunKraidGrowthFunction;
        var requests = (List<KraidPlmRequest>)enemies.KraidBehavior._kraidPlmRequests;
        var body = enemies.SlotTable.Slots[0];
        body.FrameCounter = 1; // Isolate the ceiling placement from the independent rising-rock cadence.
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        for (int index = 0; index < 9; index++)
        {
            foreach (var occupied in enemies.Projectiles.EnemyProjectiles) occupied.Clear();
            requests.Clear();
            state.CeilingRockSpawnCount = 0;
            body.Position = body.Position.WithWhole(new(128, (ushort)raw));
            body.VariableF = (ushort)(index * 2);
            body.VariableA = (ushort)KraidAiFunction.GrowBreakCeilingPlatforms;
            step(body, state);
            ushort y = unchecked((ushort)(raw - 1));
            bool spawn = (y & 3) == 0;
            AssertEqual(y, body.Position.Y.Whole, "Ceiling break moves before cadence test");
            AssertEqual(spawn ? 1 : 0, requests.Count, "Ceiling PLM cadence");
            AssertEqual((ushort)(index * 2 + (spawn ? 2 : 0)), body.VariableF, "Ceiling byte selector advances only on emission");
            if (!spawn) continue;
            var rock = enemies.Projectiles.EnemyProjectiles[^1];
            AssertEqual(Word(EnemyRomTablePointers.Kraid.CeilingRockXWords + index * 2), rock.Position.X.Whole, "Native ceiling X reaches actual projectile");
            AssertEqual((ushort)312, rock.Position.Y.Whole, "Ceiling projectile origin Y");
            AssertEqual((ushort)127, rock.Velocity.Y.RawValue, "Ceiling projectile retains current RNG velocity");
            AssertEqual(KraidPlmDefinitions.GrowthCeiling[index], requests[0], "Rock and ceiling mutation stay paired");
        }
        Console.WriteLine("Kraid ceiling rocks: all 18 byte reads and 589824 actual growth updates match with placement reads forbidden.");
    }

    /// <summary>Reads a retail placement byte for the production-spawn comparison.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Kraid ceiling rock positions")]
    private static byte KraidCeilingRockPositionsRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Rejects reads of the migrated ceiling-placement window and any attempted bus write.</summary>
    private static CartridgeReadGuard NewKraidCeilingGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => address is >= 0xa7acb3 and <= 0xa7acc5, _ => "Unexpected migrated ceiling placement read.")
            .RejectWrites(_ => "Unexpected ceiling bus write.");
}
