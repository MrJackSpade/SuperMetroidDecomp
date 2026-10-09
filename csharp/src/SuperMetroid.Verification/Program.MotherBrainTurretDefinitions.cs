using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Compares turret and direction tables with the cartridge and checks production turret, rotation, and bullet behavior.</summary>
    /// <param name="rom">The retail address space containing Mother Brain's turret definition tables.</param>
    private static void VerifyMotherBrainTurretDefinitions(SuperMetroidAddressSpace rom)
    {
        const BindingFlags instance = BindingFlags.Instance | BindingFlags.NonPublic;
        MethodInfo spawnTurret = typeof(RoomEnemySystem).GetMethod(
            "SpawnMotherBrainTurret", instance)!;
        MethodInfo selectDirection = typeof(RoomEnemySystem).GetMethod(
            "SelectNextMotherBrainTurretDirection",
            BindingFlags.Static | BindingFlags.NonPublic)!;
        MethodInfo spawnBullet = typeof(RoomEnemySystem).GetMethod(
            "SpawnMotherBrainTurretBullet", instance)!;
        FieldInfo busField = typeof(RoomEnemySystem).GetField("_bus", instance)!;
        FieldInfo randomField = typeof(RoomEnemySystem).GetField("_nextRandom", instance)!;
        var guarded = new MotherBrainTurretReadGuard(rom);

        for (ushort parameter = 0; parameter < 12; parameter++)
        {
            MotherBrainTurretDefinition definition =
                MotherBrainTurretDefinitions.ForTurret(parameter);
            int wordOffset = parameter * 2;
            AssertEqual(ReadMotherBrainTurretWord(rom, 0x86be89 + wordOffset), definition.X,
                $"Mother Brain turret X {parameter}");
            AssertEqual(ReadMotherBrainTurretWord(rom, 0x86bea1 + wordOffset), definition.Y,
                $"Mother Brain turret Y {parameter}");
            AssertEqual(ReadMotherBrainTurretWord(rom, 0x86bec9 + wordOffset),
                definition.AllowedRotationPointer,
                $"Mother Brain turret rotation pointer {parameter}");
            AssertEqual(ReadMotherBrainTurretWord(rom, 0x86bee1 + wordOffset),
                (ushort)definition.InitialDirection,
                $"Mother Brain turret initial direction {parameter}");

            for (byte direction = 0; direction < 8; direction++)
            {
                bool nativeAllowed = rom.ReadByte(
                    0x860000 | (definition.AllowedRotationPointer + direction)) != 0;
                AssertEqual(nativeAllowed, MotherBrainTurretDefinitions.IsRotationAllowed(
                    definition.AllowedRotationPointer,
                    (MotherBrainTurretDirection)direction),
                    $"Mother Brain turret allowed direction {parameter}/{direction}");
            }

            var enemies = new RoomEnemySystem();
            busField.SetValue(enemies, guarded);
            randomField.SetValue(enemies, (Func<ushort>)(() => 0));
            spawnTurret.Invoke(enemies, [parameter]);

            RoomEnemyProjectileSlot turret =
                enemies.EnemyProjectiles.Single(projectile => projectile.IsActive);
            MotherBrainTurretDirectionDefinition directionDefinition =
                MotherBrainTurretDefinitions.ForDirection(definition.InitialDirection);
            AssertEqual(RoomEnemyProjectileKind.MotherBrainRoomTurret, turret.Kind,
                $"production Mother Brain turret kind {parameter}");
            AssertEqual(definition.X, turret.XPosition,
                $"production Mother Brain turret X {parameter}");
            AssertEqual(definition.Y, turret.YPosition,
                $"production Mother Brain turret Y {parameter}");
            AssertEqual(definition.AllowedRotationPointer, turret.XSubposition,
                $"production Mother Brain turret rotation pointer {parameter}");
            AssertEqual(unchecked((ushort)(0x0100 | (byte)definition.InitialDirection)),
                turret.YSubposition,
                $"production Mother Brain turret packed direction {parameter}");
            AssertEqual(directionDefinition.InstructionPointer, turret.InstructionPointer,
                $"production Mother Brain turret instruction {parameter}");
            AssertEqual(MotherBrainTurretDefinitions.MinimumRotationDelay, turret.XVelocity,
                $"production Mother Brain turret rotation timer {parameter}");
            AssertEqual(MotherBrainTurretDefinitions.MinimumFiringCooldown, turret.YVelocity,
                $"production Mother Brain turret cooldown {parameter}");

            foreach (sbyte delta in new sbyte[] { -1, 1 })
            for (byte current = 0; current < 8; current++)
            {
                var rotation = new RoomEnemyProjectileSlot(0)
                {
                    XSubposition = definition.AllowedRotationPointer,
                    YSubposition = unchecked((ushort)(unchecked((byte)delta) << 8 | current)),
                };
                byte candidate = unchecked((byte)((current + delta) & 7));
                bool allowed = rom.ReadByte(
                    0x860000 | (definition.AllowedRotationPointer + candidate)) != 0;
                sbyte expectedDelta = allowed ? delta : unchecked((sbyte)-delta);
                byte expectedDirection = allowed
                    ? candidate
                    : unchecked((byte)(current + expectedDelta));

                selectDirection.Invoke(null, [rotation]);
                AssertEqual(unchecked((ushort)(
                        unchecked((byte)expectedDelta) << 8 | expectedDirection)),
                    rotation.YSubposition,
                    $"production Mother Brain turret rotation {parameter}/{current}/{delta}");
            }
        }

        for (byte direction = 0; direction < 8; direction++)
        {
            MotherBrainTurretDirection typedDirection =
                (MotherBrainTurretDirection)direction;
            MotherBrainTurretDirectionDefinition definition =
                MotherBrainTurretDefinitions.ForDirection(typedDirection);
            int wordOffset = direction * 2;
            AssertEqual(ReadMotherBrainTurretWord(rom, 0x86beb9 + wordOffset),
                definition.InstructionPointer,
                $"Mother Brain turret initial instruction {direction}");
            AssertEqual(ReadMotherBrainTurretWord(rom, 0x86c040 + wordOffset),
                definition.InstructionPointer,
                $"Mother Brain turret runtime instruction {direction}");
            AssertEqual(unchecked((short)ReadMotherBrainTurretWord(rom, 0x86bf9f + wordOffset)),
                definition.BulletXOffset,
                $"Mother Brain turret bullet X offset {direction}");
            AssertEqual(unchecked((short)ReadMotherBrainTurretWord(rom, 0x86bfaf + wordOffset)),
                definition.BulletYOffset,
                $"Mother Brain turret bullet Y offset {direction}");
            AssertEqual(unchecked((short)ReadMotherBrainTurretWord(rom, 0x86bfbf + wordOffset)),
                definition.BulletXVelocity,
                $"Mother Brain turret bullet X velocity {direction}");
            AssertEqual(unchecked((short)ReadMotherBrainTurretWord(rom, 0x86bfcf + wordOffset)),
                definition.BulletYVelocity,
                $"Mother Brain turret bullet Y velocity {direction}");

            var enemies = new RoomEnemySystem();
            busField.SetValue(enemies, guarded);
            var turret = new RoomEnemyProjectileSlot(0)
            {
                XPosition = 0x0400,
                YPosition = 0x0200,
                YSubposition = direction,
            };
            spawnBullet.Invoke(enemies, [turret]);

            RoomEnemyProjectileSlot bullet =
                enemies.EnemyProjectiles.Single(projectile => projectile.IsActive);
            AssertEqual(RoomEnemyProjectileKind.MotherBrainRoomTurretBullet, bullet.Kind,
                $"production Mother Brain turret bullet kind {direction}");
            AssertEqual((ushort)direction, bullet.DirectionParameter,
                $"production Mother Brain turret bullet direction {direction}");
            AssertEqual((ushort)(direction * 2), bullet.Variable0,
                $"production Mother Brain turret bullet direction offset {direction}");
            AssertEqual(unchecked((ushort)(0x0400 + definition.BulletXOffset)), bullet.XPosition,
                $"production Mother Brain turret bullet X {direction}");
            AssertEqual(unchecked((ushort)(0x0200 + definition.BulletYOffset)), bullet.YPosition,
                $"production Mother Brain turret bullet Y {direction}");
            AssertEqual(unchecked((ushort)definition.BulletXVelocity), bullet.XVelocity,
                $"production Mother Brain turret bullet X velocity {direction}");
            AssertEqual(unchecked((ushort)definition.BulletYVelocity), bullet.YVelocity,
                $"production Mother Brain turret bullet Y velocity {direction}");
        }

        AssertThrows<ArgumentOutOfRangeException>(
            () => MotherBrainTurretDefinitions.ForTurret(12),
            "Mother Brain turret parameter past table");
        AssertThrows<ArgumentOutOfRangeException>(
            () => MotherBrainTurretDefinitions.ForDirection((MotherBrainTurretDirection)8),
            "Mother Brain turret direction past table");
        AssertThrows<InvalidDataException>(
            () => MotherBrainTurretDefinitions.IsRotationAllowed(
                0xbefa, MotherBrainTurretDirection.Left),
            "Mother Brain turret unaligned rotation pointer");

        Console.WriteLine(
            "Mother Brain turret definitions: 12 placements, 96 rotation bytes, 16 instruction selectors, 32 bullet words, 384 rotations and all 20 real spawns pass with source tables forbidden.");
    }

    /// <summary>Reads a little-endian word from the retail cartridge for comparison with a compiled turret definition.</summary>
    /// <param name="bus">The retail address space supplying the source bytes.</param>
    /// <param name="address">The byte address of the word's low byte.</param>
    /// <returns>The two bytes combined with the low byte first.</returns>
    private static ushort ReadMotherBrainTurretWord(SuperMetroidAddressSpace bus, int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    /// <summary>Rejects reads from migrated Mother Brain turret tables while forwarding unrelated cartridge access.</summary>
    /// <param name="source">The underlying bus used for addresses outside the compiled table ranges.</param>
    private sealed class MotherBrainTurretReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Routes cartridge-import reads through the same turret-table guard as ordinary bus reads.</summary>
        /// <param name="address">The bus address to read.</param>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects migrated turret-table reads and delegates all other reads to the underlying bus.</summary>
        /// <param name="address">The bus address to read.</param>
        /// <returns>The source byte when the address is outside the guarded table ranges.</returns>
        public byte ReadByte(int address) =>
            address is >= 0x86be89 and < 0x86bf59 ||
            address is >= 0x86bf9f and < 0x86bfdf ||
            address is >= 0x86c040 and < 0x86c050
                ? throw new InvalidOperationException(
                    $"Mother Brain turret attempted migrated table read ${address:X6}.")
                : source.ReadByte(address);

        /// <summary>Forwards a write to the underlying address space.</summary>
        /// <param name="address">The bus address to write.</param>
        /// <param name="value">The byte to store at that address.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
