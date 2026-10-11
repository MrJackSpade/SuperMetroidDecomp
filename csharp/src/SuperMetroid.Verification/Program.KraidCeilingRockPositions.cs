using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyKraidCeilingRockCoordinates(SuperMetroidAddressSpace rom)
    {
        ushort Word(int a) => (ushort)(KraidCeilingRockCoordinatesRomByte(rom, a) | KraidCeilingRockCoordinatesRomByte(rom, a + 1) << 8);
        for (int offset = 0; offset <= 18; offset++)
            AssertEqual(Word(EnemyRomTablePointers.Kraid.CeilingRockXWords + offset),
                KraidCeilingRockPositions.AtByteOffset(offset), "Native byte-addressed ceiling rock lookup");
        foreach (int invalid in new[] { int.MinValue, -1, 19, 20, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => KraidCeilingRockPositions.AtByteOffset(invalid),
                "Ceiling rock byte-window bounds");
    }

    [CpuBusBoundary("exact bus test: retail ROM oracle for Kraid ceiling rock coordinates")]
    private static byte KraidCeilingRockCoordinatesRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    private static void VerifyKraidCeilingRockPositions(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyKraidCeilingRockCoordinates), () => VerifyKraidCeilingRockCoordinates(rom));
        ushort Word(int a) => (ushort)(KraidCeilingRockPositionsRomByte(rom, a) | KraidCeilingRockPositionsRomByte(rom, a + 1) << 8);
        var enemies = new RoomEnemySystem();
        var state = new KraidEnemyState();
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, NewKraidCeilingGuard(rom));
        typeof(RoomEnemySystem).GetField("_readRandomNumber", flags)!.SetValue(enemies, (Func<ushort>)(() => 63));
        var step = typeof(RoomEnemySystem).GetMethod("RunKraidGrowthFunction", flags)!
            .CreateDelegate<Action<RoomEnemySlot, KraidEnemyState>>(enemies);
        var requests = (List<KraidPlmRequest>)typeof(RoomEnemySystem).GetField("_kraidPlmRequests", flags)!.GetValue(enemies)!;
        var body = enemies.Slots[0];
        body.FrameCounter = 1; // Isolate the ceiling placement from the independent rising-rock cadence.
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        for (int index = 0; index < 9; index++)
        {
            foreach (var occupied in enemies.EnemyProjectiles) occupied.Clear();
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
            var rock = enemies.EnemyProjectiles[^1];
            AssertEqual(Word(EnemyRomTablePointers.Kraid.CeilingRockXWords + index * 2), rock.Position.X.Whole, "Native ceiling X reaches actual projectile");
            AssertEqual((ushort)312, rock.Position.Y.Whole, "Ceiling projectile origin Y");
            AssertEqual((ushort)127, rock.Velocity.Y.RawValue, "Ceiling projectile retains current RNG velocity");
            AssertEqual(KraidPlmDefinitions.GrowthCeiling[index], requests[0], "Rock and ceiling mutation stay paired");
        }
        Console.WriteLine("Kraid ceiling rocks: all 18 byte reads and 589824 actual growth updates match with placement reads forbidden.");
    }

    [CpuBusBoundary("exact bus test: retail ROM oracle for Kraid ceiling rock positions")]
    private static byte KraidCeilingRockPositionsRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    private static CartridgeReadGuard NewKraidCeilingGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => address is >= 0xa7acb3 and <= 0xa7acc5, _ => "Unexpected migrated ceiling placement read.")
            .RejectWrites(_ => "Unexpected ceiling bus write.");
}
