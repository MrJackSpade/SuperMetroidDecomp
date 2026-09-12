using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyKraidRockLaunchDefinitions(SuperMetroidAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        var enemies = new RoomEnemySystem();
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, new KraidRockReadGuard(rom));
        ushort random = 0;
        int reads = 0;
        typeof(RoomEnemySystem).GetField("_readRandomNumber", flags)!.SetValue(enemies, (Func<ushort>)(() => { reads++; return random; }));
        typeof(RoomEnemySystem).GetField("_nextRandom", flags)!.SetValue(enemies, (Func<ushort>)(() => throw new InvalidOperationException("Kraid rock must not advance RNG.")));
        var spawn = typeof(RoomEnemySystem).GetMethod("SpawnKraidSpitRock", flags)!.CreateDelegate<Func<RoomEnemySlot, bool>>(enemies);
        var body = enemies.Slots[0];
        var rock = enemies.EnemyProjectiles[^1];
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            random = (ushort)raw;
            ushort expected = Word(EnemyRomTablePointers.Kraid.RockXVelocityWords + (raw & 14));
            AssertEqual(expected, KraidRockLaunchDefinitions.FromRandom(random), "Every native Kraid rock velocity selection");
            rock.Clear();
            rock.XSubposition = rock.YSubposition = 0xffff;
            body.XPosition = (ushort)raw;
            body.YPosition = (ushort)(ushort.MaxValue - raw);
            reads = 0;
            AssertTrue(spawn(body), "Kraid rock allocation succeeds");
            AssertEqual(1, reads, "Kraid rock reads current RNG once");
            AssertEqual(RoomEnemyProjectileKind.KraidSpitRock, rock.Kind, "Native reverse allocation slot");
            AssertEqual(expected, rock.XVelocity, "Native selected speed reaches actual projectile");
            AssertEqual((ushort)0xfc00, rock.YVelocity, "Native vertical launch");
            AssertEqual(unchecked((ushort)(raw + 16)), rock.XPosition, "Kraid mouth X offset and wrap");
            AssertEqual(unchecked((ushort)(ushort.MaxValue - raw - 96)), rock.YPosition, "Kraid mouth Y offset and wrap");
            AssertEqual((ushort)0, rock.XSubposition, "Clear stale rock X fraction");
            AssertEqual((ushort)0, rock.YSubposition, "Clear stale rock Y fraction");
            AssertEqual((ushort)0x0600, rock.GraphicsIndex, "Native rock palette/tile binding");
        }
        foreach (var occupied in enemies.EnemyProjectiles) occupied.Kind = RoomEnemyProjectileKind.KraidSpitRock;
        ushort previousX = rock.XPosition;
        reads = 0;
        AssertTrue(!spawn(body), "Full pool rejects Kraid rock");
        AssertEqual(0, reads, "Full pool performs no initializer RNG read");
        AssertEqual(previousX, rock.XPosition, "Full pool preserves occupied rock");
        Console.WriteLine("Kraid rock launch: eight native words and 65536 actual wrapped spawns pass with table reads and RNG advances forbidden.");
    }

    private sealed class KraidRockReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        public byte ReadByte(int address) => address is >= 0xa7bc65 and < 0xa7bc75
            ? throw new InvalidOperationException("Unexpected migrated Kraid rock velocity read.") : source.ReadByte(address);
        public void WriteByte(int address, byte value) => throw new InvalidOperationException("Unexpected Kraid rock bus write.");
    }
}
