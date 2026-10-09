using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Exercises Bomb Torizo's production drool-spawn path across its random values and facing-dependent angle modes.</summary>
    /// <param name="rom">Cartridge address space supplying reference sine-table values.</param>
    private static void VerifyBombTorizoDroolSine(SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        MethodInfo spawn = typeof(RoomEnemySystem).GetMethod(
            "SpawnBombTorizoLowHealthDrool",
            flags)!;

        for (int random = byte.MinValue; random <= byte.MaxValue; random++)
        {
            VerifyBombTorizoDroolSpawn(
                rom,
                spawn,
                random,
                parameter1: 0x4000,
                expectedAngle: unchecked((byte)random),
                "full-circle left-facing");
            VerifyBombTorizoDroolSpawn(
                rom,
                spawn,
                random,
                parameter1: 0xc000,
                expectedAngle: unchecked((byte)random),
                "full-circle right-facing");
        }

        for (int randomNibble = 0; randomNibble < 16; randomNibble++)
        {
            VerifyBombTorizoDroolSpawn(
                rom,
                spawn,
                randomNibble,
                parameter1: 0,
                expectedAngle: unchecked((byte)(224 + randomNibble - 8)),
                "left-facing cone");
            VerifyBombTorizoDroolSpawn(
                rom,
                spawn,
                randomNibble,
                parameter1: 0x8000,
                expectedAngle: unchecked((byte)(32 + randomNibble - 8)),
                "right-facing cone");
        }

        Console.WriteLine(
            "Bomb Torizo drool sine: 544 real random/facing spawns preserve native XY velocity with the entire signed-sine ROM table forbidden.");
    }

    /// <summary>Runs one drool spawn and checks allocation, sine-derived velocity, and facing-adjusted origin.</summary>
    /// <param name="rom">Reference cartridge data used to calculate the expected sine values.</param>
    /// <param name="spawn">Production enemy-system method that creates the drool projectile.</param>
    /// <param name="random">Queued random value consumed by the spawn routine.</param>
    /// <param name="parameter1">Bomb Torizo's native facing and angle-mode parameter.</param>
    /// <param name="expectedAngle">Expected eight-bit angle selected by this case.</param>
    /// <param name="scenario">Human-readable description included in assertion failures.</param>
    private static void VerifyBombTorizoDroolSpawn(
        SuperMetroidAddressSpace rom,
        MethodInfo spawn,
        int random,
        ushort parameter1,
        byte expectedAngle,
        string scenario)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(
            enemies,
            new BombTorizoDroolSineReadGuard(rom));
        typeof(RoomEnemySystem).GetField("_nextRandom", flags)!.SetValue(
            enemies,
            (Func<ushort>)(new Queue<ushort>([0, unchecked((ushort)random)]).Dequeue));

        RoomEnemySlot torizo = enemies.Slots[0];
        torizo.XPosition = 0x0400;
        torizo.YPosition = 0x0200;
        torizo.Parameter1 = parameter1;

        spawn.Invoke(enemies, [torizo]);

        RoomEnemyProjectileSlot projectile = enemies.EnemyProjectiles[^1];
        AssertTrue(projectile.IsActive,
            $"Bomb Torizo {scenario} random {random:X2} allocation");
        AssertEqual(
            ReadBombTorizoDroolSineWord(
                rom,
                unchecked((byte)(expectedAngle + 64))),
            projectile.XVelocity,
            $"Bomb Torizo {scenario} random {random:X2} X velocity");
        AssertEqual(
            ReadBombTorizoDroolSineWord(rom, expectedAngle),
            projectile.YVelocity,
            $"Bomb Torizo {scenario} random {random:X2} Y velocity");
        AssertEqual(
            (parameter1 & 0x8000) != 0 ? (ushort)0x0408 : (ushort)0x03f8,
            projectile.XPosition,
            $"Bomb Torizo {scenario} random {random:X2} X origin");
    }

    /// <summary>Reads the signed sine-table word used as an expected drool velocity component.</summary>
    /// <param name="rom">Cartridge address space containing the reference table.</param>
    /// <param name="angle">Eight-bit sine-table selector.</param>
    /// <returns>The raw 16-bit table word at the selected angle.</returns>
    private static ushort ReadBombTorizoDroolSineWord(
        SuperMetroidAddressSpace rom,
        byte angle)
    {
        int address = EnemyMathReferenceData.SignedSine + angle * 2;
        return unchecked((ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8));
    }

    /// <summary>Wraps cartridge access and throws if the production spawn path reads the migrated signed-sine table.</summary>
    /// <param name="source">Underlying address space used for allowed reads and forwarded writes.</param>
    private sealed class BombTorizoDroolSineReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Routes importer reads through the same sine-table guard as normal address-space reads.</summary>
        /// <param name="address">Cartridge address requested by the importer.</param>
        /// <returns>The requested byte when its address is outside the migrated sine table.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects reads from the migrated signed-sine table and delegates all other reads.</summary>
        /// <param name="address">Cartridge address to read.</param>
        /// <returns>The byte returned by the wrapped address space for an allowed address.</returns>
        /// <exception cref="InvalidOperationException">The requested address is within the signed-sine table range.</exception>
        public byte ReadByte(int address) =>
            address is >= 0xa0b443 and < 0xa0b643
                ? throw new InvalidOperationException(
                    $"Bomb Torizo drool attempted migrated sine read ${address:X6}.")
                : source.ReadByte(address);

        /// <summary>Forwards a memory write to the wrapped cartridge address space.</summary>
        /// <param name="address">Cartridge address to update.</param>
        /// <param name="value">Byte written at that address.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
