using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

/// <summary>Remote projectile impact must reach Yard's separate airborne owner without direct damage.</summary>
internal static class MissileYardDetachmentAudit
{
    public static int Run(string rom)
    {
        foreach (ushort selection in new ushort[] { 1, 2 })
        {
            var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
            var runtime = FlatFloorMovementFixture.Create(bus, false, wideRunway: true);
            var level = runtime.LevelData!;
            for (int y = 0; y < 16; y++)
                level.SetForegroundEntry(level.GetBlockIndex(18, y), RoomLevelWord.Create(0, 0, RoomCollisionType.SolidBlock).Raw);
            var samus = runtime.Samus!;
            var population = new PopulationSelectionAddressSpace(bus,
                [new RoomEnemyPopulationRecord(RoomEnemySystem.YardDefinition, 64, 120, 0,
                    (ushort)EnemyProperties.ProcessOffScreen, 0, 0, 0)]);
            runtime.Enemies.Load(population, PopulationSelectionAddressSpace.PopulationPointer,
                PopulationSelectionAddressSpace.TilesetPointer, runtime.Vram, runtime.Cgram, () => 1, samus: samus);
            var actor = runtime.Enemies.Slots[0];
            var state = runtime.Enemies.YardStates[0] ?? throw new InvalidDataException("Yard missing.");
            // Isolate the pending attached owner from animated crawl movement.
            // The actor is behind Samus and above the horizontal shot trajectory.
            actor.CurrentInstruction = 0;
            state.Behavior = 0;
            state.MovementFunction = YardMovementFunction.InstructionPending;
            state.HidingInstructionList = (ushort)YardMovementFunction.InstructionPending;
            ushort initialHealth = actor.Health;
            samus.SelectedHudItem = selection;
            samus.Missiles = samus.MaxMissiles = samus.SuperMissiles = samus.MaxSuperMissiles = 10;
            int impact = -1, detached = -1;
            for (int frame = 0; frame < 25; frame++)
            {
                runtime.StepFrame(frame == 0 ? (ushort)SnesButton.X : (ushort)0);
                if (impact < 0 && runtime.Projectiles.Slots.Any(p => p.IsActive &&
                    p.PackedType.IsFamily(SamusProjectileFamily.MissileExplosion))) impact = frame;
                if (detached < 0 && state.MovementFunction == YardMovementFunction.Airborne) detached = frame;
                if (actor.Health != initialHealth || actor.XPosition != 64)
                    throw new InvalidDataException("Remote Yard fixture incurred direct contact or horizontal displacement.");
                if (detached >= 0 && (selection != 2 || detached != impact || state.Behavior != 3))
                    throw new InvalidDataException("Yard detached without the qualifying remote Super Missile impact.");
                int elapsed = detached < 0 ? 0 : frame - detached;
                // Unlike the shared crawler, Yard immediately executes the newly
                // selected airborne owner on the impact frame: move zero, add gravity.
                uint expected = (120u << 16) + (uint)(elapsed * (elapsed + 1) / 2) * YardImpactFixtureData.Gravity;
                uint actual = ((uint)actor.YPosition << 16) | actor.YSubposition;
                if (actual != expected)
                    throw new InvalidDataException($"Yard impact trajectory frame {frame}: expected {expected:X8}, actual {actual:X8}.");
            }
            if (impact < 0 || (selection == 2 ? detached != impact : detached != -1))
                throw new InvalidDataException($"Yard impact handoff failed: missile={selection}, impact={impact}, detach={detached}.");
            Console.WriteLine($"Yard remote impact: missile={selection}, impact={impact}, detach={detached}; all 25 positions match, no direct damage.");
        }
        return 0;
    }
}

/// <summary>Native Yard airborne integration quantities for the impact fixture.</summary>
internal static class YardImpactFixtureData
{
    /// <summary>A3:D1B3 adds one eighth pixel per frame to airborne vertical velocity.</summary>
    public const uint Gravity = 0x2000;
}
