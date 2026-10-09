using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Compares the launch-speed lookup against the cartridge table for every possible 16-bit RNG word.</summary>
    /// <param name="rom">Address space containing the eight native signed 8.8 velocity choices.</param>
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

    /// <summary>Verifies the speed mapping and, unless limited to definitions, exercises actual rock allocation with forbidden table reads and RNG advances.</summary>
    /// <param name="rom">Address space used for native reference words and the production system's other data.</param>
    /// <param name="definitionsOnly">When <see langword="true"/>, runs only the lookup comparison and skips projectile-spawn checks.</param>
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

    /// <summary>Wraps the address space while rejecting Kraid rock velocity-table reads and bus writes.</summary>
    /// <param name="source">Underlying cartridge space used for reads outside the migrated velocity table.</param>
    private sealed class KraidRockReadGuard(ISnesAddressSpace source) : ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Routes importer reads through the guarded address-space read path.</summary>
        /// <param name="address">Cartridge address requested by the importer.</param>
        /// <returns>The requested byte when it is outside the migrated velocity table.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects reads of the migrated velocity table and forwards other reads.</summary>
        /// <param name="address">Cartridge address to read.</param>
        /// <returns>The requested byte when the address is outside the guarded table.</returns>
        /// <exception cref="InvalidOperationException">The address belongs to the migrated Kraid rock velocity table.</exception>
        public byte ReadByte(int address) => address is >= 0xa7bc65 and < 0xa7bc75
            ? throw new InvalidOperationException("Unexpected migrated Kraid rock velocity read.") : source.ReadByte(address);

        /// <summary>Rejects writes because this fixture only expects read access during the launch check.</summary>
        /// <param name="address">Cartridge address the caller attempted to modify.</param>
        /// <param name="value">Byte the caller attempted to write.</param>
        /// <exception cref="InvalidOperationException">A bus write was attempted.</exception>
        public void WriteByte(int address, byte value) => throw new InvalidOperationException("Unexpected Kraid rock bus write.");
    }
}
