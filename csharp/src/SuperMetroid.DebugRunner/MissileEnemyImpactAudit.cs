using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

/// <summary>Real enemy overlap/shot dispatcher, including a missile-immune Ripper control.</summary>
internal static class MissileEnemyImpactAudit
{
    public static void Verify(string rom)
    {
        foreach (ushort definition in new[] { DownbackFixtureData.ZoomerDefinition, RoomEnemySystem.RipperDefinition })
        foreach (ushort selection in new ushort[] { 1, 2 })
        {
            var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
            var runtime = FlatFloorMovementFixture.Create(bus, false, wideRunway: true);
            var samus = runtime.Samus!;
            samus.SelectedHudItem = selection;
            samus.Missiles = samus.MaxMissiles = samus.SuperMissiles = samus.MaxSuperMissiles = 10;
            runtime.StepFrame((ushort)SnesButton.X);
            var missile = runtime.Projectiles.Slots.First(s => s.IsActive && s.PackedType.IsFamily(
                selection == 1 ? SamusProjectileFamily.Missile : SamusProjectileFamily.SuperMissile));
            var selected = new PopulationSelectionAddressSpace(bus,
                [new RoomEnemyPopulationRecord(definition, missile.XPosition, missile.YPosition,
                    0, (ushort)EnemyProperties.ProcessInstructions, 0, 0, 0)]);
            runtime.Enemies.Load(selected, PopulationSelectionAddressSpace.PopulationPointer,
                PopulationSelectionAddressSpace.TilesetPointer, new SnesVram(), new SnesCgram(), () => 0, samus: samus);
            runtime.Enemies.StepFrame(0, 0, false, level: runtime.LevelData);
            var enemy = runtime.Enemies.Slots[0];
            enemy.XPosition = missile.XPosition; enemy.YPosition = missile.YPosition;
            ushort health = enemy.Health;
            runtime.Projectiles.BeginImpactAudioFrame(false);
            int hits = runtime.Enemies.ResolveOrdinaryProjectileHits(selected, runtime.Projectiles, runtime.BombProjectiles, samus);
            if (hits == 0 || !missile.PackedType.IsFamily(SamusProjectileFamily.MissileExplosion))
                throw new InvalidDataException($"Enemy ${definition:X4} missile {selection} did not enter impact through overlap dispatch.");
            if (runtime.Projectiles.ImpactSoundRequests.Count != 1 ||
                runtime.Enemies.EarthquakeTimer != (selection == 2 ? 30 : 0))
                throw new InvalidDataException("Enemy impact did not publish the expected sound and shared quake.");
            bool immuneControl = definition == RoomEnemySystem.RipperDefinition && selection == 1;
            if (immuneControl ? enemy.Health != health : enemy.Health >= health && enemy.EnemyDefinitionPointer != 0)
                throw new InvalidDataException("Enemy impact damage/immune control differs from the expected retail vulnerability.");
            Console.WriteLine($"Enemy ${definition:X4}, missile {selection}: overlap hit, health {health}->{enemy.Health}, quake {runtime.Enemies.EarthquakeTimer}, one impact sound.");
        }
    }
}
