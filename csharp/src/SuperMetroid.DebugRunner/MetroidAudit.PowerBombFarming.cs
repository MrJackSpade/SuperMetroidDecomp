using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

internal static partial class MetroidAudit
{
    /// <summary>
    /// #441 connected candidates: two/three Power Bomb button presses with one pack,
    /// no Ice and no cheats. Captures damage and death/drop timing for CPU comparison.
    /// </summary>
    public static int CapturePowerBombFarming(string rom, string directory)
    {
        Directory.CreateDirectory(directory);
        foreach (ushort seed in new ushort[] { 1, 2, 3, 4 })
        foreach (int shots in new[] { 2, 3 })
            CapturePowerBombFarmingCase(rom, directory, seed, shots);
        return 0;
    }

    private static void CapturePowerBombFarmingCase(string rom, string directory, ushort seed, int shots)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        var runtime = FlatFloorMovementFixture.Create(bus, false, wideRunway: true);
        var samus = runtime.Samus!;
        samus.EquippedItems = (ushort)(SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.GravitySuit);
        samus.EquippedBeams = 0;
        samus.Health = samus.MaxHealth = 399;
        samus.PowerBombs = samus.MaxPowerBombs = 5;
        samus.Missiles = samus.MaxMissiles = 10;
        samus.SuperMissiles = samus.MaxSuperMissiles = 10;
        samus.SelectedHudItem = 3;
        samus.Pose = SamusPoseIds.MorphBallGroundRightPose;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        samus.YPosition = (ushort)(256 - samus.Kinematics.YRadius);
        var population = new PopulationSelectionAddressSpace(bus,
            [new RoomEnemyPopulationRecord(MetroidDefinition, samus.XPosition,
                (ushort)(samus.YPosition - 8), 0, (ushort)EnemyProperties.ProcessInstructions, 0, 0, 0)]);
        runtime.Enemies.Load(population, PopulationSelectionAddressSpace.PopulationPointer,
            PopulationSelectionAddressSpace.TilesetPointer, runtime.Vram, runtime.Cgram,
            runtime.System.NextRandom, runtime.System.SetRandomNumber,
            readRandomNumber: () => runtime.System.RandomNumber, samus: samus);
        runtime.System.SetRandomNumber(seed);
        var enemy = runtime.Enemies.Slots[0];
        string prefix = Path.Combine(directory, $"farming-{seed}-{shots}");
        RoomMovementSeedExporter.Write(runtime, prefix + ".movement-seed");
        File.WriteAllText(prefix + ".metadata.json", System.Text.Json.JsonSerializer.Serialize(new
        {
            seed, shots, samus.Health, samus.PowerBombs, samus.MaxPowerBombs,
            samus.Missiles, samus.SuperMissiles, Enemy = enemy,
            Scrolls = runtime.Camera!.Scrolls.Storage.ToArray(),
        }));
        using var output = new StreamWriter(prefix + ".jsonl");
        ushort previousHealth = enemy.Health;
        int damageEvents = 0;
        int pickupFrame = -1;
        EnemyPickupKind? collected = null;
        for (int frame = 0; frame < 1050; frame++)
        {
            ushort input = frame >= 5 && frame < 5 + shots * 345 && (frame - 5) % 345 == 0
                ? (ushort)SnesButton.X : (ushort)0;
            // Reach the actual drop above the floor with ordinary posture/jump input.
            // The same input runs in the surviving-enemy control; no pickup is injected.
            if (frame == 850) input |= (ushort)SnesButton.Up;
            if (frame >= 855 && frame < 885) input |= (ushort)SnesButton.A;
            runtime.StepFrame(input);
            if (runtime.Enemies.LastCollectedEnemyPickup is { } pickup)
            {
                if (pickupFrame >= 0) throw new InvalidDataException("A single Metroid produced multiple collected drops.");
                pickupFrame = frame;
                collected = pickup;
            }
            output.WriteLine(System.Text.Json.JsonSerializer.Serialize(new
            {
                Frame = frame, Input = input, samus.Health, samus.PowerBombs,
                samus.Kinematics.XFixed, samus.Kinematics.YFixed, samus.Pose,
                SamusRadiusX = samus.Kinematics.XRadius, SamusRadiusY = samus.Kinematics.YRadius,
                EnemyHealth = enemy.Health, enemy.EnemyDefinitionPointer,
                enemy.XPosition, enemy.YPosition, enemy.InvincibilityTimer,
                PowerBomb = runtime.BombProjectiles.PowerBombExplosion,
                runtime.Enemies.EnemiesKilled,
                runtime.System.RandomNumber,
                runtime.Enemies.LastCollectedEnemyPickup,
                Drops = runtime.Enemies.EnemyProjectiles.Select(p => new
                {
                    p.Kind, p.XPosition, p.YPosition, p.InstructionPointer,
                    p.InstructionTimer, p.PreInstruction, p.DirectionParameter,
                    p.Variable0, p.EnemyHeaderPointer,
                    p.XRadius, p.YRadius,
                }),
            }));
            if (enemy.Health != previousHealth)
            {
                Console.WriteLine($"PB FARM seed={seed} shots={shots} frame={frame} enemy={previousHealth}->{enemy.Health} ammo={samus.PowerBombs} health={samus.Health}");
                damageEvents++;
                previousHealth = enemy.Health;
            }
        }
        if (samus.Health == 0 || damageEvents != (shots == 2 ? 4 : 5) ||
            runtime.Enemies.EnemiesKilled != (shots == 2 ? 0 : 1))
            throw new InvalidDataException("Power Bomb farming candidate changed its damage/death boundary.");
        EnemyPickupKind? expectedPickup = shots == 2 ? null : seed switch
        {
            1 => EnemyPickupKind.PowerBomb,
            2 or 3 => EnemyPickupKind.BigEnergy,
            4 => EnemyPickupKind.SmallEnergy,
            _ => throw new InvalidDataException("Uncatalogued farming seed."),
        };
        if (collected != expectedPickup || pickupFrame != (shots == 2 ? -1 : 851) ||
            samus.PowerBombs != (shots == 2 || seed == 1 ? 3 : 2))
            throw new InvalidDataException("Native farming pickup kind, collection frame or ammunition differed.");
        Console.WriteLine($"PB FARM seed={seed} shots={shots}: damage events={damageEvents}, killed={runtime.Enemies.EnemiesKilled}, pickup={collected} at {pickupFrame}, ammo={samus.PowerBombs}. Managed regression passed; use the trace comparator for native parity.");
    }
}
