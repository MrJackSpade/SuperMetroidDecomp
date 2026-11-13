using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies.Drawing;

namespace SuperMetroid.Verification.Enemies;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.AssetExtraction.Enemies.Artwork;

/// <summary>
/// Checks #1343's enemy-format draws as #1392 draws them: an ordinary enemy and an extended enemy publish the whole
/// frames of the sheets their room's graphics set names (#1513), a draw in another palette row shows its
/// frame in that row, an enemy projectile publishes its frame from the set's spawned-object sheet, and the shake tick is
/// a simulation phase.
/// </summary>
internal static class EnemyFormatDrawEntriesVerification
{
    /// <summary>The row holding the projectile's colours, the enemy projectiles'.</summary>
    /// <summary>The projectile's graphics: the set's shared sheet, which holds every object it can spawn, in the enemy projectiles' colours.</summary>
    private static readonly SpriteGraphics ProjectileGraphics = SpriteGraphics.SharedIn(EnemyShades.InProjectileLook);

    /// <summary>Loads a room graphics set, publishes a constructed population and checks every entry.</summary>
    internal static void VerifyEnemyFormatDrawEntries()
    {
        ImportEnemyArtwork artwork = InstalledFixtures.EnemyArtwork;
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(artwork.Enemies), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance) { BossArtwork = artwork.Bosses };
        EnemySpritemapCatalog spritemaps = artwork.Enemies.Spritemaps ?? throw new InvalidOperationException("The installation has no enemy spritemaps.");
        EnemyExtendedFrameCatalog extendedFrames = artwork.Enemies.ExtendedFrames ?? throw new InvalidOperationException("The installation has no extended enemy frames.");
        EnemyProjectileSpritemapCatalog projectileSpritemaps = artwork.Enemies.ProjectileSpritemaps ?? throw new InvalidOperationException("The installation has no enemy projectile spritemaps.");

        // A room's graphics set: a Beetom, an ordinary enemy, and a walking space pirate, an extended one.
        RoomEnemyGraphicsSetDefinition set = RoomEnemyGraphicsSetDefinitions.GreenPiratesShaft;
        enemies.DrawUnder(set);

        // Ordinary enemy on layer zero, in the row its graphics set loaded it in.
        EnemyFrameId ordinaryFrame = EnemyDefinitionFrameDefinitions.Frames[EnemyDefinitionId.Beetom]
            .First(frame => spritemaps.TryGetDisplay(frame, out EnemySpritemapParts parts) && parts.Count > 0);
        RoomEnemySlot ordinary = enemies.SlotTable.Slots[0];
        ordinary.Bind(EnemyDefinitionId.Beetom, default(RoomEnemyDefinition));
        ordinary.Frame = ordinaryFrame;
        ordinary.Position = ordinary.Position.WithWhole(new(100, 80));
        enemies.Drawing.Enqueue(enemies.SlotTable.Slots[0], reached: true);

        // Extended enemy on layer one, showing a frame with several components.
        EnemyFrameId extendedFrame = EnemyDefinitionFrameDefinitions.Frames[EnemyDefinitionId.PirateGreenWalking]
            .First(frame => extendedFrames.TryGetDisplay(frame, out ReadOnlyMemory<EnemyExtendedDrawComponent> components) && components.Length > 1);
        RoomEnemySlot extended = enemies.SlotTable.Slots[1];
        extended.Bind(EnemyDefinitionId.PirateGreenWalking, default(RoomEnemyDefinition));
        extended.CollidesByFrameHitboxes = true;
        extended.Frame = extendedFrame;
        extended.AnimationClock.HoldFor(7);
        extended.Position = extended.Position.WithWhole(new(120, 110));
        enemies.Drawing.Enqueue(extended, reached: true);

        // Enemy projectile in front of Samus, drawn from the set's spawned-object sheet: one whose picture there shows something
        // (#1513), as a picture showing nothing draws nothing.
        EnemyProjectileFrameId projectileFrame = EnemySheetComposer.CommonSpawnersOf(set, RoomEnemyGraphicsSetLists.Members).SelectMany(common => common.Objects.Projectiles).Order()
            .First(frame => EnemyProjectileFrames.Parts(projectileSpritemaps, frame).Length > 0
                && !InstalledFixtures.EnemySheets.Spawned(set, SpawnedObjectOwner.Shared).ShowsNothing(EnemySheetDefinitions.ProjectileFrameName(frame)));
        EnemyProjectile projectile = enemies.Projectiles.EnemyProjectiles[17];
        projectile.Kind = RoomEnemyProjectileKind.CeresRidleyFireball;
        projectile.Depth = EnemyProjectileDepth.InFrontOfSamus;
        projectile.Frame = projectileFrame;
        projectile.Graphics = ProjectileGraphics;
        projectile.Position = projectile.Position.WithWhole(new(30, 40));

        var sprites = new SpriteFrameBuffer();
        sprites.BeginFrame();
        enemies.Drawing.Advance(Position16<RoomPixels>.Zero);
        enemies.Drawing.Draw(sprites, Position16<RoomPixels>.Zero);
        enemies.Projectiles.DrawEnemyProjectilesAt(sprites, Position16<RoomPixels>.Zero, EnemyProjectileDepth.InFrontOfSamus, false);
        sprites.FinalizeFrame();
        SpriteDraw[] draws = [.. sprites.Draws];

        SpriteSheet beetomSheet = InstalledFixtures.EnemySheets.For(set, EnemyDefinitionId.Beetom);
        AssertImage(draws[0], new SpriteSource(SpriteSourceKind.Enemy, 0), beetomSheet.Frame(EnemySheetDefinitions.FrameName(ordinaryFrame)),
            new(100, 80), PictureShading.Standard, "Ordinary enemy");

        // The extended frame's pieces are one whole picture, drawn once at the enemy's origin (#1513).
        SpriteSheet pirateSheet = InstalledFixtures.EnemySheets.For(set, EnemyDefinitionId.PirateGreenWalking);
        AssertImage(draws[1], new SpriteSource(SpriteSourceKind.Enemy, 1),
            pirateSheet.Frame(EnemySheetDefinitions.FrameName(extendedFrame)), new(120, 110), PictureShading.Standard,
            "Extended enemy");
        int entry = 2;

        SpawnedObjectSheet projectileSheet = InstalledFixtures.EnemySheets.Spawned(set, SpawnedObjectOwner.Shared);
        AssertImage(draws[entry++], new SpriteSource(SpriteSourceKind.EnemyProjectile, 17),
            projectileSheet.Frame(EnemySheetDefinitions.ProjectileFrameName(projectileFrame)), new(30, 40),
            EnemyShades.InProjectileLook, "Enemy projectile");
        AssertEqual(draws.Length, entry, "No other entries are published");

        // An enemy drawn in another look shows its frame under that look's shading (#1512).
        ordinary.Shading = EnemyShades.FrozenLook;
        sprites.BeginFrame();
        enemies.Drawing.Advance(Position16<RoomPixels>.Zero);
        enemies.Drawing.Draw(sprites, Position16<RoomPixels>.Zero);
        sprites.FinalizeFrame();
        AssertImage(sprites.Draws[0], new SpriteSource(SpriteSourceKind.Enemy, 0), beetomSheet.Frame(EnemySheetDefinitions.FrameName(ordinaryFrame)),
            new(100, 80), EnemyShades.FrozenLook, "Ordinary enemy in another look");
        ordinary.Shading = PictureShading.Standard;

        // The shake tick belongs to the advance: it is consumed identically whether or not the frame is drawn.
        ordinary.FrameCounter = 0;
        ordinary.ShakeTimer = 2;
        enemies.Drawing.Advance(Position16<RoomPixels>.Zero);
        int undisplayed = ordinary.ShakeTimer;
        ordinary.ShakeTimer = 2;
        sprites.BeginFrame();
        enemies.Drawing.Advance(Position16<RoomPixels>.Zero);
        enemies.Drawing.Draw(sprites, Position16<RoomPixels>.Zero);
        sprites.FinalizeFrame();
        AssertEqual(1, undisplayed, "An undrawn frame consumes one shake tick");
        AssertEqual(undisplayed, ordinary.ShakeTimer, "A drawn frame consumes the same shake tick");
        AssertEqual(new Position16<ScreenPixels>(101, 80), sprites.Draws[0].Origin, "The drawn frame shows the shaken origin");
        Console.WriteLine($"Enemy-format draws: ordinary and whole extended entries from their graphics set's sheets, a frame in another look, and a projectile over the enemy tile page; shake advance independent of display.");

        static void AssertImage(SpriteDraw draw, SpriteSource source, SpriteSheetFrame frame, Position16<ScreenPixels> origin, PictureShading shading, string subject)
        {
            var image = draw as ImageSpriteDraw ?? throw new InvalidOperationException($"{subject} is a {draw.GetType().Name}, not a sheet frame.");
            AssertEqual(source, image.Source, $"{subject} names its slot");
            AssertTrue(ReferenceEquals(frame, image.Image), $"{subject} shows its sheet's frame");
            AssertEqual(origin, image.Origin, $"{subject} is placed at its screen origin");
            AssertEqual(shading, image.Shading, $"{subject} shows its frame under the shading it is drawn in");
        }
    }

    /// <summary>Runs the enemy-format draw-entry confirmation.</summary>
    [VerificationCommand("--enemy-format-draw-entries")]
    internal static int EnemyFormatDrawEntriesCommand()
    {
        Suite(nameof(VerifyEnemyFormatDrawEntries), VerifyEnemyFormatDrawEntries);
        return 0;
    }
}
