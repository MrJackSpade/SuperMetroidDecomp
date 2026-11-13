using SuperMetroid.Core.Assets;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Enemies.Drawing;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Enemies.EnemyDropsVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rendering;
using SuperMetroid.AssetExtraction.Enemies;

namespace SuperMetroid.Verification.Samus.Projectiles;
using SuperMetroid.Verification.Harness;
using SuperMetroid.AssetExtraction.Enemies.Artwork;

/// <summary>Checks enemy drawing after the power-bomb damage phase removes queued actors.</summary>
internal static class PowerBombDeathDrawingVerification
{
    /// <summary>The palette row the fixture's one-member graphics set loads its member's colours into.</summary>
    private const int PowerBombFixtureRow = 1;

    /// <summary>Reproduces issue #1168's queue, damage, and draw order for lethal and surviving enemies.</summary>
    internal static void VerifyPowerBombDeathDrawing()
    {
        EnemyTileSourceArtwork artwork = InstalledFixtures.EnemyTiles;
        EnemySpritemapCatalog spritemaps = artwork.Spritemaps ?? throw new InvalidOperationException("The installation has no enemy spritemaps.");
        RoomLevelData openRoom = RoomEnemyFrameFixtures.OpenRoom(new(16, 16));

        Confirm(EnemyDefinitionId.Atomic, respawns: false, lethal: true);
        Confirm(EnemyDefinitionId.Sidehopper, respawns: false, lethal: true);
        Confirm(EnemyDefinitionId.Sidehopper, respawns: true, lethal: true);
        Confirm(EnemyDefinitionId.Atomic, respawns: false, lethal: false);
        Console.WriteLine("Power-bomb death drawing: cleared slots and respawn placeholders emit no OAM; surviving enemies retain their display; missing live artwork still fails.");

        void Confirm(EnemyDefinitionId header, bool respawns, bool lethal)
        {
            var fixture = CreateEnemyDropFixture(CreateDropTestSamus(), [1]);
            var enemies = fixture.System;
            // The room's graphics set loads the enemy, as every room that holds one does.
            var set = new RoomEnemyGraphicsSetDefinition($"power-bomb {header}");
            FixtureGraphicsSetMembers.Record(set, [new RoomEnemyGraphicsSetMember(header)]);
            // The fixture's set loads its one member's colours into row one, as its record would name.
            FixtureGraphicsSetRows.Record(set, [PowerBombFixtureRow]);
            enemies.DrawUnder(set);
            var actor = enemies.SlotTable.Slots[0];
            actor.Bind(header, actor.Definition);
            actor.Bind(actor.DefinitionId, RoomEnemyHeaders.Get(header));
            actor.Shading = PictureShading.Standard;
            actor.Health = lethal ? (ushort)1 : (ushort)1000;
            actor.Position = actor.Position.WithWhole(new(128, 128));
            actor.Radius = new(actor.Definition.Radius.X, actor.Definition.Radius.Y);
            actor.Contact = EnemyContact.None;
            actor.Processing = EnemyProcessing.None;
            actor.Presence = respawns ? EnemyPresence.RespawnsIfKilled : EnemyPresence.None;
            actor.Frame = EnemyDefinitionFrameDefinitions.Frames[header]
                .First(frame => spritemaps.TryGetDisplay(frame, out EnemySpritemapParts parts) && parts.Count != 0);

            // Freeze only the AI while producing the real draw queue. The following
            // explicit damage call represents the later runtime power-bomb phase.
            enemies.StepStandaloneFrame(enemies.StandaloneFrame(openRoom) with { Camera = new(0, 0), TimeIsFrozen = true });
            var before = new OamBuffer();
            enemies.Drawing.DrawAsOam(before, new(0, 0));
            AssertTrue(before.NextByteOffset != 0, "live enemy was queued and drawn before power-bomb damage");
            AssertEqual(1, enemies.Collision.ResolveOrdinaryPowerBombHits(new(128, 128), 64),
                "power bomb reaches the queued enemy");
            var after = new OamBuffer();
            enemies.Drawing.DrawAsOam(after, new(0, 0));
            if (lethal)
            {
                AssertEqual(respawns ? EnemyDefinitionId.Respawn : 0,
                    actor.DefinitionId, "lethal power bomb clears or reserves the physical slot");
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
                AssertThrows<InvalidDataException>(() => enemies.Drawing.DrawAsOam(new OamBuffer(), new(0, 0)),
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
