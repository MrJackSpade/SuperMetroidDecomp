using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

internal static class ReflectedSuperTrailAudit
{
    public static int Run(string rom)
    {
        var failures = new List<string>();
        for (byte direction = 0; direction < 10; direction++)
        {
            try { VerifyDirection(rom, direction); }
            catch (Exception error) { failures.Add($"Direction {direction}: {error.Message}"); }
        }
        if (failures.Count != 0) throw new InvalidDataException(string.Join(Environment.NewLine, failures));
        return 0;
    }

    private static void VerifyDirection(string rom, byte direction)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        var runtime = FlatFloorMovementFixture.Create(bus, false);
        var samus = runtime.Samus!;
        samus.SelectedHudItem = 2;
        samus.SuperMissiles = samus.MaxSuperMissiles = 5;
        var projectiles = runtime.Projectiles;
        var result = projectiles.StepFrame(bus, runtime.LevelData!, samus,
            (ushort)SnesButton.X, (ushort)SnesButton.X, 0, 0, sharedProjectiles: runtime.BombProjectiles);
        int index = result.FiredSlot ?? throw new InvalidDataException("No Super Missile fired.");
        var shot = projectiles.Slots[index];
        shot.Direction = direction;
        shot.TrailTimer = 1;
        projectiles.ReflectFromEnemy(bus, index);
        ushort x = shot.XPosition, y = shot.YPosition;
        ushort reflectedList = shot.InstructionPointer;
        Console.WriteLine($"Reflection direction={direction} list={reflectedList:X4}");
        if (shot.InstructionTimer != 1 || shot.TrailTimer != 1)
            throw new InvalidDataException("Reflection did not retain the reported pending-trail boundary.");
        projectiles.StepFrame(bus, runtime.LevelData!, samus, 0, 0, 0, 0, sharedProjectiles: runtime.BombProjectiles);
        var trails = projectiles.TrailSlots.Where(t => t.Left.InstructionTimer != 0).ToArray();
        if (trails.Length != 1 || trails[0].Left.XPosition != x - 4 || trails[0].Right.XPosition != x - 4 ||
            trails[0].Left.YPosition != y - 4 || trails[0].Right.YPosition != y - 4)
            throw new InvalidDataException("Reflected Super Missile did not preserve native open-bus trail coordinates.");
        if (shot.InstructionPointer != reflectedList + 8 || shot.InstructionTimer != 15 || shot.TrailTimer != 2)
            throw new InvalidDataException("Reflection trail correction altered projectile animation/cadence.");
        if (SnesCpuOperandRead.ReadAbsoluteIndexedWord(bus, 0x9b, 0x2100, 0xdb) != 0x2121 ||
            SnesCpuOperandRead.ReadAbsoluteIndexedWord(bus, 0x9b, 0, 0x21db) != 0)
            throw new InvalidDataException("Open bus ignored the fetched operand rather than retaining MDR.");
        bool strict = false;
        try { bus.ReadByte(0x9b21db); } catch (InvalidOperationException) { strict = true; }
        if (!strict) throw new InvalidDataException("Ordinary data reads lost strict address validation.");
        Console.WriteLine("PASS reflected Super Missile: exact pending-trail boundary, four coordinates, next animation record, trail cadence, operand-dependent open bus, strict ordinary reads.");
    }
}
