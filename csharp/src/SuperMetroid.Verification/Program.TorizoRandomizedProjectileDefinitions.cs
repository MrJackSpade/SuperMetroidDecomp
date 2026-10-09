using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Verifies randomized Bomb and Golden Torizo projectile tuples and exercises every production spawn variant.</summary>
    /// <param name="rom">Cartridge address space containing the original randomized-projectile tuples.</param>
    private static void VerifyTorizoRandomizedProjectileDefinitions(
        SuperMetroidAddressSpace rom)
    {
        (int Address, TorizoRandomizedProjectileDefinition Definition, string Name)[] definitions =
        [
            (0x86ac08, TorizoRandomizedProjectileDefinitions.BombChozoOrb(true), "Bomb orb right"),
            (0x86ac12, TorizoRandomizedProjectileDefinitions.BombChozoOrb(false), "Bomb orb left"),
            (0x86ac99, TorizoRandomizedProjectileDefinitions.GoldenChozoOrb(true), "Golden orb right"),
            (0x86aca3, TorizoRandomizedProjectileDefinitions.GoldenChozoOrb(false), "Golden orb left"),
            (0x86b02f, TorizoRandomizedProjectileDefinitions.GoldenEgg(true), "Golden egg right"),
            (0x86b039, TorizoRandomizedProjectileDefinitions.GoldenEgg(false), "Golden egg left"),
            (0x86b376, TorizoRandomizedProjectileDefinitions.GoldenEyeBeam(true), "Golden eye beam right"),
            (0x86b380, TorizoRandomizedProjectileDefinitions.GoldenEyeBeam(false), "Golden eye beam left"),
        ];
        BindingFlags instanceFlags = BindingFlags.Instance | BindingFlags.NonPublic;

        static ushort ReadWord(ISnesAddressSpace bus, int address) =>
            (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

        var guarded = new TorizoRandomizedProjectileReadGuard(rom);
        MethodInfo initialize = typeof(RoomEnemySystem).GetMethod(
            "InitializeTorizoRandomizedProjectile",
            instanceFlags)!;
        foreach ((int address, TorizoRandomizedProjectileDefinition definition, string name)
            in definitions)
        {
            AssertEqual(ReadWord(rom, address), definition.InstructionList,
                $"{name} instruction");
            AssertEqual(unchecked((short)ReadWord(rom, address + 2)), definition.XOffset,
                $"{name} X offset");
            AssertEqual(unchecked((short)ReadWord(rom, address + 4)), definition.BaseXVelocity,
                $"{name} base X velocity");
            AssertEqual(unchecked((short)ReadWord(rom, address + 6)), definition.YOffset,
                $"{name} Y offset");
            AssertEqual(unchecked((short)ReadWord(rom, address + 8)), definition.BaseYVelocity,
                $"{name} base Y velocity");

            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", instanceFlags)!
                .SetValue(enemies, guarded);
            int randomCall = 0;
            typeof(RoomEnemySystem).GetField("_nextRandom", instanceFlags)!
                .SetValue(enemies, (Func<ushort>)(() => randomCall++ == 0 ? (ushort)0 : (ushort)0x00ff));
            RoomEnemySlot torizo = enemies.Slots[0];
            torizo.XPosition = 1000;
            torizo.YPosition = 500;
            RoomEnemyProjectileSlot projectile = enemies.EnemyProjectiles[^1];
            initialize.Invoke(enemies, [projectile, torizo, definition]);

            AssertEqual(definition.InstructionList, projectile.InstructionPointer,
                $"{name} production instruction");
            AssertEqual(unchecked((ushort)(1000 + definition.XOffset)), projectile.XPosition,
                $"{name} production X");
            AssertEqual(unchecked((ushort)(definition.BaseXVelocity - 128)), projectile.XVelocity,
                $"{name} production X jitter");
            AssertEqual(unchecked((ushort)(500 + definition.YOffset)), projectile.YPosition,
                $"{name} production Y");
            AssertEqual(unchecked((ushort)(definition.BaseYVelocity + 127)), projectile.YVelocity,
                $"{name} production Y jitter");
            AssertEqual(2, randomCall, $"{name} independent RNG calls");
        }

        foreach (bool facingRight in new[] { false, true })
        {
            VerifyTorizoRandomizedSpawn(
                guarded,
                "SpawnBombTorizoChozoOrb",
                facingRight,
                TorizoRandomizedProjectileDefinitions.BombChozoOrb(facingRight),
                velocityIsReplaced: false);
            VerifyTorizoRandomizedSpawn(
                guarded,
                "SpawnGoldenTorizoChozoOrb",
                facingRight,
                TorizoRandomizedProjectileDefinitions.GoldenChozoOrb(facingRight),
                velocityIsReplaced: false);
            VerifyTorizoRandomizedSpawn(
                guarded,
                "SpawnGoldenTorizoEgg",
                facingRight,
                TorizoRandomizedProjectileDefinitions.GoldenEgg(facingRight),
                velocityIsReplaced: false);
            VerifyTorizoRandomizedSpawn(
                guarded,
                "SpawnGoldenTorizoEyeBeam",
                facingRight,
                TorizoRandomizedProjectileDefinitions.GoldenEyeBeam(facingRight),
                velocityIsReplaced: true);
        }

        Console.WriteLine(
            "Torizo randomized projectiles: all forty tuple words, signed-jitter boundaries, and eight real Bomb/Golden orb, egg and eye-beam spawns pass with every tuple forbidden.");
    }

    /// <summary>Invokes one production spawn routine and checks its selected definition, position, and applicable velocity jitter.</summary>
    /// <param name="guarded">Address space that rejects reads from the migrated projectile tuples.</param>
    /// <param name="methodName">Room-enemy spawn method to invoke by reflection.</param>
    /// <param name="facingRight"><see langword="true"/> to initialize the Torizo facing right; otherwise, facing left.</param>
    /// <param name="expected">Compiled tuple expected for the selected facing.</param>
    /// <param name="velocityIsReplaced">Whether the spawn routine subsequently replaces the tuple-derived velocities.</param>
    private static void VerifyTorizoRandomizedSpawn(
        ISnesAddressSpace guarded,
        string methodName,
        bool facingRight,
        TorizoRandomizedProjectileDefinition expected,
        bool velocityIsReplaced)
    {
        BindingFlags instanceFlags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", instanceFlags)!
            .SetValue(enemies, guarded);
        typeof(RoomEnemySystem).GetField("_nextRandom", instanceFlags)!
            .SetValue(enemies, (Func<ushort>)(() => 0));
        RoomEnemySlot torizo = enemies.Slots[0];
        torizo.XPosition = 1000;
        torizo.YPosition = 500;
        torizo.Parameter1 = facingRight ? (ushort)0x8000 : (ushort)0;

        MethodInfo spawn = typeof(RoomEnemySystem).GetMethod(methodName, instanceFlags)!;
        if (methodName == "SpawnGoldenTorizoEyeBeam")
            spawn.Invoke(enemies, [torizo, (ushort)0]);
        else
            spawn.Invoke(enemies, [torizo]);

        RoomEnemyProjectileSlot projectile = enemies.EnemyProjectiles[^1];
        AssertTrue(projectile.IsActive, $"{methodName} facing {facingRight} allocation");
        AssertEqual(expected.InstructionList, projectile.InstructionPointer,
            $"{methodName} facing {facingRight} instruction");
        AssertEqual(unchecked((ushort)(1000 + expected.XOffset)), projectile.XPosition,
            $"{methodName} facing {facingRight} X");
        AssertEqual(unchecked((ushort)(500 + expected.YOffset)), projectile.YPosition,
            $"{methodName} facing {facingRight} Y");
        if (!velocityIsReplaced)
        {
            AssertEqual(unchecked((ushort)(expected.BaseXVelocity - 128)), projectile.XVelocity,
                $"{methodName} facing {facingRight} X jitter");
            AssertEqual(unchecked((ushort)(expected.BaseYVelocity - 128)), projectile.YVelocity,
                $"{methodName} facing {facingRight} Y jitter");
        }
    }

    /// <summary>Forwards address-space operations but rejects reads from the compiled randomized-projectile tuples.</summary>
    /// <param name="source">Address space used for permitted reads and all writes.</param>
    private sealed class TorizoRandomizedProjectileReadGuard(ISnesAddressSpace source)
        : ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Routes an import-source byte request through the guarded read operation.</summary>
        /// <param name="address">Cartridge address to read.</param>
        /// <returns>The byte at the address when it lies outside the compiled tuple ranges.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Reads a permitted byte and rejects access to any migrated randomized-projectile tuple.</summary>
        /// <param name="address">Address to read.</param>
        /// <returns>The wrapped address space's byte for an address outside the tuple ranges.</returns>
        /// <exception cref="InvalidOperationException">The address belongs to a compiled projectile tuple.</exception>
        public byte ReadByte(int address) =>
            address is >= 0x86ac08 and < 0x86ac1c or
                >= 0x86ac99 and < 0x86acad or
                >= 0x86b02f and < 0x86b043 or
                >= 0x86b376 and < 0x86b38a
                ? throw new InvalidOperationException(
                    $"Torizo randomized projectile attempted migrated tuple read ${address:X6}.")
                : source.ReadByte(address);

        /// <summary>Forwards a byte write to the wrapped address space.</summary>
        /// <param name="address">Address to update.</param>
        /// <param name="value">Byte to store at that address.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
