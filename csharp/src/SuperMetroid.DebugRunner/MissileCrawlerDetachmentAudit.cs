using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

/// <summary>Controller firing, wall collision and crawler AI execute together in one constructed room.</summary>
internal static class MissileCrawlerDetachmentAudit
{
    public static int Run(string rom)
    {
        foreach (ushort definition in MissileCrawlerFixtureData.Definitions)
        foreach (ushort selection in new ushort[] { 1, 2 })
        foreach (bool frozen in new[] { false, true })
        {
            // The frozen Zoomer is a state-dispatch control, not a claim that
            // Ice can freeze every member of the shared crawler family.
            if (frozen && definition != DownbackFixtureData.ZoomerDefinition) continue;
            var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
            var runtime = FlatFloorMovementFixture.Create(bus, false, wideRunway: true);
            var level = runtime.LevelData!;
            // The ceiling crawler is behind and well above the projectile's path.
            // A right-hand wall causes a remote impact; direct enemy damage cannot
            // substitute for observing the shared room earthquake.
            for (int y = 0; y < 16; y++)
                level.SetForegroundEntry(level.GetBlockIndex(18, y), RoomLevelWord.Create(0, 0, RoomCollisionType.SolidBlock).Raw);
            for (int x = 2; x < 11; x++)
                level.SetForegroundEntry(level.GetBlockIndex(x, 6), RoomLevelWord.Create(0, 0, RoomCollisionType.SolidBlock).Raw);
            var samus = runtime.Samus!;
            // Native frozen AI thaws immediately if Ice is not equipped. Retain
            // the same Ice loadout in both controls so only frozen state differs.
            samus.EquippedBeams |= (ushort)SamusBeamFlags.Ice;
            var population = new PopulationSelectionAddressSpace(bus,
                [new RoomEnemyPopulationRecord(definition, 128, 120,
                    0, (ushort)(EnemyProperties.ProcessInstructions | EnemyProperties.ProcessOffScreen), 0, 0, 0)]);
            runtime.Enemies.Load(population, PopulationSelectionAddressSpace.PopulationPointer,
                PopulationSelectionAddressSpace.TilesetPointer, runtime.Vram, runtime.Cgram, () => 1, samus: samus);
            var enemy = runtime.Enemies.Slots[0];
            var state = runtime.Enemies.CrawlerStates[0] ?? throw new InvalidDataException("No crawler loaded.");
            state.Function = definition == RoomEnemySystem.HZoomerDefinition
                ? CrawlerEnemyFunction.HZoomerCrawlingHorizontally : CrawlerEnemyFunction.CrawlingHorizontally;
            state.XVelocity = 0;
            state.YVelocity = unchecked((ushort)-128);
            enemy.YPosition = (ushort)(112 + enemy.YRadius);
            enemy.InstructionTimer = ushort.MaxValue;
            enemy.FrozenTimer = frozen ? (ushort)200 : (ushort)0;
            enemy.AiHandlerBits = frozen ? MissileCrawlerFixtureData.FrozenAiBit : (ushort)0;
            ushort initialHealth = enemy.Health, initialY = enemy.YPosition;
            samus.SelectedHudItem = selection;
            samus.Missiles = samus.MaxMissiles = samus.SuperMissiles = samus.MaxSuperMissiles = 10;
            int impact = -1, detached = -1;
            for (int frame = 0; frame < 70; frame++)
            {
                runtime.StepFrame(frame == 0 ? (ushort)SnesButton.X : (ushort)0);
                if (impact < 0 && runtime.Projectiles.Slots.Any(p => p.IsActive &&
                        p.PackedType.IsFamily(SamusProjectileFamily.MissileExplosion))) impact = frame;
                if (detached < 0 && state.Function == CrawlerEnemyFunction.Falling) detached = frame;
                if (enemy.Health != initialHealth)
                    throw new InvalidDataException("Remote-impact fixture accidentally damaged its crawler.");
                if (frozen && enemy.FrozenTimer == 0)
                    throw new InvalidDataException("Frozen control thawed before the test completed.");
                if (detached >= 0 && (selection != 2 || frozen || impact < 0))
                    throw new InvalidDataException($"Unexpected detachment: selection={selection}, frozen={frozen}, impact={impact}, detach={detached}.");
                if (detached >= 0 && frame - detached <= 8)
                {
                    int elapsed = frame - detached;
                    // Alpha handles the impact before EnemyMain, so the falling
                    // function is installed that frame. Its first call next frame
                    // moves by zero, then gains the native half-pixel acceleration.
                    uint expected = ((uint)initialY << 16) + (uint)(elapsed * (elapsed - 1) / 2) * CrawlerQuakeAuditData.FallingAcceleration;
                    uint actual = ((uint)enemy.YPosition << 16) | enemy.YSubposition;
                    if (actual != expected)
                        throw new InvalidDataException($"Impact-to-fall trajectory elapsed={elapsed}: expected {expected:X8}, actual {actual:X8}.");
                }
            }
            bool shouldFall = selection == 2 && !frozen;
            if (impact < 0 || (shouldFall ? detached != impact || enemy.YPosition <= initialY : detached >= 0 || enemy.YPosition != initialY))
                throw new InvalidDataException($"Remote quake response: selection={selection}, frozen={frozen}, impact={impact}, detach={detached}, Y={initialY}->{enemy.YPosition}.");
            Console.WriteLine($"Remote wall impact: enemy={definition:X4}, missile={selection}, frozen={frozen}, impact={impact}, detach={detached}, Y={initialY}->{enemy.YPosition}; no direct damage.");
        }
        return 0;
    }
}

/// <summary>Native state required by the constructed frozen-enemy control.</summary>
internal static class MissileCrawlerFixtureData
{
    /// <summary>The seven shared crawler wrappers and the separate HZoomer AI.</summary>
    public static ReadOnlySpan<ushort> Definitions => [
        RoomEnemySystem.SciserDefinition, RoomEnemySystem.ZeroDefinition,
        RoomEnemySystem.ViolaDefinition, RoomEnemySystem.ZeelaDefinition,
        RoomEnemySystem.SovaDefinition, RoomEnemySystem.ZoomerDefinition,
        RoomEnemySystem.StoneZoomerDefinition, RoomEnemySystem.HZoomerDefinition];
    /// <summary>Enemy AI-handler bit two selects the frozen handler and suppresses instruction animation.</summary>
    public const ushort FrozenAiBit = 4;
}
