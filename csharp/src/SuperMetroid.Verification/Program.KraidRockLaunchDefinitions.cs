using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyKraidRockLaunchSpeedSelection(SuperMetroidAddressSpace rom)
    {
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            int address = 0xa7bc65 + (raw & 14);
            ushort expected = (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
            AssertEqual(expected, KraidRockLaunchDefinitions.FromRandom((ushort)raw),
                "Every native Kraid rock velocity selection");
        }
    }

    private static void VerifyKraidRockLaunchDefinitions(SuperMetroidAddressSpace rom, bool definitionsOnly = false)
    {
        Suite(nameof(VerifyKraidRockLaunchSpeedSelection), () => VerifyKraidRockLaunchSpeedSelection(rom));
        if (definitionsOnly)
        {
            Console.WriteLine("Kraid spit speeds: all65536 RNG words match the eight native signed8.8 choices.");
            return;
        }
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        var enemies = new RoomEnemySystem();
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, NewKraidRockGuard(rom));
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
            rock.Clear();
            rock.Position = rock.Position.WithFractionY(0xffff).WithFractionX(0xffff);
            body.Position = body.Position.WithWhole(new((ushort)raw, (ushort)(ushort.MaxValue - raw)));
            reads = 0;
            AssertTrue(spawn(body), "Kraid rock allocation succeeds");
            AssertEqual(1, reads, "Kraid rock reads current RNG once");
            AssertEqual(RoomEnemyProjectileKind.KraidSpitRock, rock.Kind, "Native reverse allocation slot");
            AssertEqual(expected, rock.Velocity.X.RawValue, "Native selected speed reaches actual projectile");
            AssertEqual((ushort)0xfc00, rock.Velocity.Y.RawValue, "Native vertical launch");
            AssertEqual(unchecked((ushort)(raw + 16)), rock.Position.X.Whole, "Kraid mouth X offset and wrap");
            AssertEqual(unchecked((ushort)(ushort.MaxValue - raw - 96)), rock.Position.Y.Whole, "Kraid mouth Y offset and wrap");
            AssertEqual((ushort)0, rock.Position.X.Fraction, "Clear stale rock X fraction");
            AssertEqual((ushort)0, rock.Position.Y.Fraction, "Clear stale rock Y fraction");
            AssertEqual((ushort)0x0600, rock.GraphicsIndex, "Native rock palette/tile binding");
        }
        foreach (var occupied in enemies.EnemyProjectiles) occupied.Kind = RoomEnemyProjectileKind.KraidSpitRock;
        ushort previousX = rock.Position.X.Whole;
        reads = 0;
        AssertTrue(!spawn(body), "Full pool rejects Kraid rock");
        AssertEqual(0, reads, "Full pool performs no initializer RNG read");
        AssertEqual(previousX, rock.Position.X.Whole, "Full pool preserves occupied rock");
        Console.WriteLine("Kraid rock launch: eight native words and 65536 actual wrapped spawns pass with table reads and RNG advances forbidden.");
    }

    private static CartridgeReadGuard NewKraidRockGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .ForbidRange(0xa7bc65, 0xa7bc75 - 0xa7bc65, _ => "Unexpected migrated Kraid rock velocity read.")
            .RejectWrites(_ => "Unexpected Kraid rock bus write.");
}
