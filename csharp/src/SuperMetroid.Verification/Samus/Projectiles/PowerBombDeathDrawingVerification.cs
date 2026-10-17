using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Enemies.EnemyDropsVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rendering;
namespace SuperMetroid.Verification.Samus.Projectiles;
using SuperMetroid.Verification.Harness;

/// <summary>Checks enemy drawing after the power-bomb damage phase removes queued actors.</summary>
internal static class PowerBombDeathDrawingVerification
{
    /// <summary>Reproduces issue #1168's queue, damage, and draw order for lethal and surviving enemies.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables")]
    internal static void VerifyPowerBombDeathDrawing()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var spritemaps = EnemySpritemapCatalog.Load(new MemoryStream(EnemySpritemapFiles.Extract(rom)));
        var artwork = EnemyTileArtworkCatalog.FromArtworkForVerification(new EnemyTileArtworkParts { Sheets = new Dictionary<EnemyDefinitionId, RoomCharacterAtlas>(), Palettes = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>(), Spritemaps = spritemaps });

        Confirm(EnemyDefinitionId.Atomic, respawns: false, lethal: true);
        Confirm(EnemyDefinitionId.Sidehopper, respawns: false, lethal: true);
        Confirm(EnemyDefinitionId.Sidehopper, respawns: true, lethal: true);
        Confirm(EnemyDefinitionId.Atomic, respawns: false, lethal: false);
        Console.WriteLine("Power-bomb death drawing: cleared slots and respawn placeholders emit no OAM; surviving enemies retain their display; missing live artwork still fails.");

        void Confirm(EnemyDefinitionId header, bool respawns, bool lethal)
        {
            var fixture = CreateEnemyDropFixture(CreateDropTestSamus(), [1]);
            var enemies = fixture.System;
            enemies.TileArtwork = artwork;
            var actor = enemies.SlotTable.Slots[0];
            actor.Bind(header, actor.Definition);
            actor.Bind(actor.EnemyDefinitionPointer, RoomEnemyDefinitionCatalog.Get(header));
            actor.AiBank = actor.Definition.Bank;
            actor.Health = lethal ? (ushort)1 : (ushort)1000;
            actor.Position = actor.Position.WithWhole(new(128, 128));
            actor.Radius = new(actor.Definition.Radius.X, actor.Definition.Radius.Y);
            actor.Properties = EnemyPropertyWord.FromNativeWord((ushort)(respawns ? (ushort)EnemyProperties.RespawnIfKilled : (ushort)0));
            actor.SpritemapPointer = EnemySpritemapDefinitions.Frames
                .First(frame => frame.Bank == actor.Definition.Bank &&
                    spritemaps.TryGetDisplay(frame.Bank, frame.Pointer, out var parts) && parts.Length != 0).Pointer;

            // Freeze only the AI while producing the real draw queue. The following
            // explicit damage call represents the later runtime power-bomb phase.
            enemies.StepFrame(new RoomEnemyFrame { Camera = new(0, 0), TimeIsFrozen = true });
            var before = new OamBuffer();
            enemies.Drawing.DrawLayers(before, new(0, 0), 0, 7);
            AssertTrue(before.NextByteOffset != 0, "live enemy was queued and drawn before power-bomb damage");
            AssertEqual(1, enemies.Collision.ResolveOrdinaryPowerBombHits(new(128, 128), 64),
                "power bomb reaches the queued enemy");
            var after = new OamBuffer();
            enemies.Drawing.DrawLayers(after, new(0, 0), 0, 7);
            if (lethal)
            {
                AssertEqual(respawns ? EnemyDefinitionId.Respawn : (ushort)0,
                    actor.EnemyDefinitionPointer, "lethal power bomb clears or reserves the physical slot");
                AssertEqual(0, after.NextByteOffset, "dead enemy emits no sprite records");
                AssertEqual(1, enemies.RoomStatus.EnemiesKilled, "death is counted once");
                AssertTrue(enemies.Projectiles.EnemyProjectiles.Any(projectile =>
                    projectile.Kind == RoomEnemyProjectileKind.EnemyDeathExplosion), "death explosion survives actor removal");
            }
            else
            {
                AssertTrue(actor.Health is > 0 and < 1000, "nonlethal power bomb damages the live actor");
                AssertEqual(before.NextByteOffset, after.NextByteOffset, "survivor keeps its sprite count");
                AssertTrue(before.ImageEquals(after), "survivor keeps its sprite composition");
                actor.SpritemapPointer = 0;
                AssertThrows<InvalidDataException>(() => enemies.Drawing.DrawLayers(new OamBuffer(), new(0, 0), 0, 7),
                    "a live actor's missing composition must not be silently hidden");
            }
        }
    }

    /// <summary>Runs the focused power-bomb death and OAM publication check.</summary>
    [VerificationCommand("--power-bomb-death-drawing")]
    internal static int PowerBombDeathDrawingCommand()
    {
        Suite(nameof(VerifyPowerBombDeathDrawing), () => VerifyPowerBombDeathDrawing());
        return 0;
    }
}
