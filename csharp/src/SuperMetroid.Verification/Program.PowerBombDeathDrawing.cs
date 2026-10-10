using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    // #1168: reproduce the runtime's queue -> power-bomb damage -> draw ordering.
    /// <summary>Verifies power-bomb death drawing for Atomic and Sidehopper actors, including respawn placeholders, surviving sprites, and failures for missing live artwork.</summary>
    private static void VerifyPowerBombDeathDrawing()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var spritemaps = EnemySpritemapCatalog.Load(new MemoryStream(EnemySpritemapFiles.Extract(rom)));
        var artwork = EnemyTileArtworkCatalog.FromArtworkForVerification(
            new Dictionary<ushort, RoomCharacterAtlas>(), new Dictionary<ushort, EnemyPaletteSheet>(),
            spritemaps: spritemaps);

        Confirm(RoomEnemySystem.AtomicDefinition, respawns: false, lethal: true);
        Confirm(RoomEnemySystem.SidehopperDefinition, respawns: false, lethal: true);
        Confirm(RoomEnemySystem.SidehopperDefinition, respawns: true, lethal: true);
        Confirm(RoomEnemySystem.AtomicDefinition, respawns: false, lethal: false);
        Console.WriteLine("Power-bomb death drawing: cleared slots and respawn placeholders emit no OAM; surviving enemies retain their display; missing live artwork still fails.");

        void Confirm(ushort header, bool respawns, bool lethal)
        {
            var fixture = CreateEnemyDropFixture(CreateDropTestSamus(), [1]);
            var enemies = fixture.System;
            enemies.TileArtwork = artwork;
            var actor = enemies.Slots[0];
            actor.EnemyDefinitionPointer = header;
            actor.Definition = RoomEnemyDefinitionCatalog.Get(header);
            actor.AiBank = actor.Definition.Bank;
            actor.Health = lethal ? (ushort)1 : (ushort)1000;
            actor.XPosition = 128;
            actor.YPosition = 128;
            actor.XRadius = actor.Definition.XRadius;
            actor.YRadius = actor.Definition.YRadius;
            actor.Properties = respawns ? (ushort)EnemyProperties.RespawnIfKilled : (ushort)0;
            actor.SpritemapPointer = EnemySpritemapDefinitions.Frames.ToArray()
                .First(frame => frame.Bank == actor.Definition.Bank &&
                    spritemaps.TryGetDisplay(frame.Bank, frame.Pointer, out var parts) && parts.Length != 0).Pointer;

            // Freeze only the AI while producing the real draw queue. The following
            // explicit damage call represents the later runtime power-bomb phase.
            enemies.StepFrame(0, 0, timeIsFrozen: true);
            var before = new OamBuffer();
            enemies.DrawLayers(before, 0, 0, 0, 7);
            AssertTrue(before.NextByteOffset != 0, "live enemy was queued and drawn before power-bomb damage");
            AssertEqual(1, enemies.ResolveOrdinaryPowerBombHits(fixture.Bus, 128, 128, 64),
                "power bomb reaches the queued enemy");
            var after = new OamBuffer();
            enemies.DrawLayers(after, 0, 0, 0, 7);
            if (lethal)
            {
                AssertEqual(respawns ? EnemyLifecycleDefinitions.RespawnPlaceholder : (ushort)0,
                    actor.EnemyDefinitionPointer, "lethal power bomb clears or reserves the physical slot");
                AssertEqual(0, after.NextByteOffset, "dead enemy emits no sprite records");
                AssertEqual((ushort)1, enemies.EnemiesKilled, "death is counted once");
                AssertTrue(enemies.EnemyProjectiles.Any(projectile =>
                    projectile.Kind == RoomEnemyProjectileKind.EnemyDeathExplosion), "death explosion survives actor removal");
            }
            else
            {
                AssertTrue(actor.Health is > 0 and < 1000, "nonlethal power bomb damages the live actor");
                AssertEqual(before.NextByteOffset, after.NextByteOffset, "survivor keeps its sprite count");
                AssertTrue(before.LowTable.SequenceEqual(after.LowTable) &&
                    before.HighTable.SequenceEqual(after.HighTable), "survivor keeps its sprite composition");
                actor.SpritemapPointer = 0;
                AssertThrows<InvalidDataException>(() => enemies.DrawLayers(new OamBuffer(), 0, 0, 0, 7),
                    "a live actor's missing composition must not be silently hidden");
            }
        }
    }
}
