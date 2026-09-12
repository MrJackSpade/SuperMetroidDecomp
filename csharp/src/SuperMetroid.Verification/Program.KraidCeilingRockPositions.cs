using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyKraidCeilingRockPositions(SuperMetroidAddressSpace rom)
    {
        ushort Word(int a) => (ushort)(rom.ReadByte(a) | rom.ReadByte(a + 1) << 8);
        for (int offset = 0; offset < 18; offset++)
            AssertEqual(Word(EnemyRomTablePointers.Kraid.CeilingRockXWords + offset),
                KraidCeilingRockPositions.AtByteOffset(offset), "Native byte-addressed ceiling rock lookup");
        var enemies = new RoomEnemySystem();
        var state = new KraidEnemyState();
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, new KraidCeilingReadGuard(rom));
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
            body.XPosition = 128;
            body.YPosition = (ushort)raw;
            body.VariableF = (ushort)(index * 2);
            body.VariableA = (ushort)KraidAiFunction.GrowBreakCeilingPlatforms;
            step(body, state);
            ushort y = unchecked((ushort)(raw - 1));
            bool spawn = (y & 3) == 0;
            AssertEqual(y, body.YPosition, "Ceiling break moves before cadence test");
            AssertEqual(spawn ? 1 : 0, requests.Count, "Ceiling PLM cadence");
            AssertEqual((ushort)(index * 2 + (spawn ? 2 : 0)), body.VariableF, "Ceiling byte selector advances only on emission");
            if (!spawn) continue;
            var rock = enemies.EnemyProjectiles[^1];
            AssertEqual(Word(EnemyRomTablePointers.Kraid.CeilingRockXWords + index * 2), rock.XPosition, "Native ceiling X reaches actual projectile");
            AssertEqual((ushort)312, rock.YPosition, "Ceiling projectile origin Y");
            AssertEqual((ushort)127, rock.YVelocity, "Ceiling projectile retains current RNG velocity");
            AssertEqual(KraidPlmDefinitions.GrowthCeiling[index], requests[0], "Rock and ceiling mutation stay paired");
        }
        Console.WriteLine("Kraid ceiling rocks: all 18 byte reads and 589824 actual growth updates match with placement reads forbidden.");
    }

    private sealed class KraidCeilingReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        public byte ReadByte(int address) => address is >= 0xa7acb3 and <= 0xa7acc5
            ? throw new InvalidOperationException("Unexpected migrated ceiling placement read.") : source.ReadByte(address);
        public void WriteByte(int address, byte value) => throw new InvalidOperationException("Unexpected ceiling bus write.");
    }
}
