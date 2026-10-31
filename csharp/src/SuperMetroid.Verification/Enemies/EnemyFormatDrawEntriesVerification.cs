using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core;
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

/// <summary>
/// Checks #1343's enemy-format draws as #1392 draws them: an ordinary enemy and each visible component of an extended
/// enemy publish the frames of the sheets their room's graphics set names, a draw in another palette row shows its
/// frame in that row, an enemy projectile publishes its frame from the set's spawned-object sheet, and the shake tick is
/// a simulation phase.
/// </summary>
internal static class EnemyFormatDrawEntriesVerification
{
    /// <summary>The row holding the projectile's colours, the enemy projectiles'.</summary>
    private const byte ProjectilePalette = 5;
    /// <summary>The projectile's tile base: character zero, where its set composes every object it can spawn.</summary>
    private const ushort ProjectileTileBase = 0;
    /// <summary>The projectile's graphics: its tile base in the enemy projectiles' colours.</summary>
    private static readonly SpriteGraphics ProjectileGraphics = new(ProjectileTileBase, SharedSpriteColouring.EnemyProjectiles);
    /// <summary>A palette row neither enemy is loaded in, which a draw then shows its frame in.</summary>
    private const int OtherRow = 6;

    /// <summary>Loads a room graphics set, publishes a constructed population and checks every entry.</summary>
    internal static void VerifyEnemyFormatDrawEntries()
    {
        InstalledEnemyArtwork artwork = InstalledFixtures.EnemyArtwork;
        var enemies = new RoomEnemySystem(new EnemySession(new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()), definitions: CompiledRoomEnemyDefinitions.Instance) { TileArtwork = artwork.Enemies, BossArtwork = artwork.Bosses, AreaArtwork = artwork.Areas };
        EnemySpritemapCatalog spritemaps = artwork.Enemies.Spritemaps ?? throw new InvalidOperationException("The installation has no enemy spritemaps.");
        EnemyExtendedFrameCatalog extendedFrames = artwork.Enemies.ExtendedFrames ?? throw new InvalidOperationException("The installation has no extended enemy frames.");
        EnemyProjectileSpritemapCatalog projectileSpritemaps = artwork.Enemies.ProjectileSpritemaps ?? throw new InvalidOperationException("The installation has no enemy projectile spritemaps.");

        // A room's graphics set: a Beetom, an ordinary enemy, and a walking space pirate, an extended one.
        RoomEnemyGraphicsSetDefinition set = RoomEnemyGraphicsSetDefinitions.GreenPiratesShaft;
        enemies.GraphicsSet.LoadGraphicsSet(CompiledRoomEnemyDefinitions.Instance, set, new SpriteColourSets());
        RoomEnemyGraphicsSetEntry beetom = enemies.GraphicsSet.Entry(EnemyDefinitionId.Beetom);
        RoomEnemyGraphicsSetEntry pirate = enemies.GraphicsSet.Entry(EnemyDefinitionId.PirateGreenWalking);

        // Ordinary enemy on layer zero, in the row its graphics set loaded it in.
        EnemyFrameId ordinaryFrame = EnemyDefinitionFrameDefinitions.Frames[EnemyDefinitionId.Beetom]
            .First(frame => spritemaps.TryGetDisplay(frame, out EnemySpritemapParts parts) && parts.Count > 0);
        RoomEnemySlot ordinary = enemies.SlotTable.Slots[0];
        ordinary.Bind(EnemyDefinitionId.Beetom, default(RoomEnemyDefinition));
        ordinary.Frame = ordinaryFrame;
        ordinary.TileIndexBase = checked((ushort)beetom.TileIndexBase);
        ordinary.Colouring = new GraphicsSetMemberColouring(beetom.DefinitionId);
        ordinary.Position = ordinary.Position.WithWhole(new(100, 80));
        enemies.Drawing.Enqueue(0, enemies.SlotTable.Slots[0]);

        // Extended enemy on layer one, showing a frame with several components.
        EnemyFrameId extendedFrame = EnemyDefinitionFrameDefinitions.Frames[EnemyDefinitionId.PirateGreenWalking]
            .First(frame => extendedFrames.TryGetDisplay(frame, out ReadOnlyMemory<EnemyExtendedDrawComponent> components) && components.Length > 1);
        RoomEnemySlot extended = enemies.SlotTable.Slots[1];
        extended.Bind(EnemyDefinitionId.PirateGreenWalking, default(RoomEnemyDefinition));
        extended.ExtraProperties = extended.ExtraProperties.With(EnemyExtraProperties.UsesExtendedSpritemap);
        extended.Frame = extendedFrame;
        extended.TileIndexBase = checked((ushort)pirate.TileIndexBase);
        extended.Colouring = new GraphicsSetMemberColouring(pirate.DefinitionId);
        extended.InstructionTimer = WrappingCountdown.Start(7);
        extended.Position = extended.Position.WithWhole(new(120, 110));
        enemies.Drawing.Enqueue(1, extended);

        // Enemy projectile in front of Samus, drawn from the set's spawned-object sheet.
        EnemyProjectileFrameId projectileFrame = SpawnedObjectFrameDefinitions.Anywhere.Projectiles
            .First(frame => EnemyProjectileFrames.Parts(projectileSpritemaps, frame).Length > 0);
        RoomEnemyProjectileSlot projectile = enemies.Projectiles.EnemyProjectiles[17];
        projectile.Kind = RoomEnemyProjectileKind.CeresRidleyFireball;
        projectile.Depth = EnemyProjectileDepth.InFrontOfSamus;
        projectile.Frame = projectileFrame;
        projectile.Graphics = ProjectileGraphics;
        projectile.Position = projectile.Position.WithWhole(new(30, 40));

        var sprites = new SpriteFrameBuffer();
        sprites.BeginFrame();
        enemies.Drawing.AdvanceLayers(Position16<RoomPixels>.Zero, 0, 7);
        enemies.Drawing.DrawLayers(sprites, Position16<RoomPixels>.Zero, 0, 7);
        enemies.Projectiles.DrawEnemyProjectilesAt(sprites, Position16<RoomPixels>.Zero, EnemyProjectileDepth.InFrontOfSamus, false);
        sprites.FinalizeFrame();
        SpriteDraw[] draws = [.. sprites.Draws];

        SpriteSheet beetomSheet = InstalledFixtures.EnemySheets.For(set, EnemyDefinitionId.Beetom);
        AssertImage(draws[0], new SpriteSource(SpriteSourceKind.Enemy, 0), beetomSheet.Frame(EnemySheetDefinitions.FrameName(ordinaryFrame)),
            new(100, 80), null, "Ordinary enemy");

        SpriteSheet pirateSheet = InstalledFixtures.EnemySheets.For(set, EnemyDefinitionId.PirateGreenWalking);
        extendedFrames.TryGetDisplay(extendedFrame, out ReadOnlyMemory<EnemyExtendedDrawComponent> components);
        int entry = 1;
        for (int component = 0; component < components.Length; component++)
        {
            Position16<SpritePixels> origin = new Position16<SpritePixels>(120, 110) + components.Span[component].Offset;
            if (!EnemyDrawRange.Contains(origin))
                continue;
            AssertImage(draws[entry++], new SpriteSource(SpriteSourceKind.Enemy, 1),
                pirateSheet.Frame(EnemySheetDefinitions.ComponentFrameName(extendedFrame, component)), new(origin.X, origin.Y), null,
                "Extended enemy component");
        }
        AssertTrue(entry > 2, "The extended enemy publishes one entry per visible component");

        SpawnedObjectSheet projectileSheet = InstalledFixtures.EnemySheets.Spawned(set, ProjectileTileBase);
        AssertImage(draws[entry++], new SpriteSource(SpriteSourceKind.EnemyProjectile, 17),
            projectileSheet.Sheet.Frame(EnemySheetDefinitions.ProjectileFrameName(projectileFrame)), new(30, 40),
            projectileSheet.ColourGroup.Number == ProjectilePalette ? null : ProjectilePalette, "Enemy projectile");
        AssertEqual(draws.Length, entry, "No other entries are published");

        // An enemy drawn in a row its set did not load it in shows its frame in that row.
        ordinary.Colouring = SpriteColouring.InFixtureSet(OtherRow);
        sprites.BeginFrame();
        enemies.Drawing.AdvanceLayers(Position16<RoomPixels>.Zero, 0, 0);
        enemies.Drawing.DrawLayers(sprites, Position16<RoomPixels>.Zero, 0, 0);
        sprites.FinalizeFrame();
        AssertImage(sprites.Draws[0], new SpriteSource(SpriteSourceKind.Enemy, 0), beetomSheet.Frame(EnemySheetDefinitions.FrameName(ordinaryFrame)),
            new(100, 80), OtherRow, "Ordinary enemy in another row");
        ordinary.Colouring = new GraphicsSetMemberColouring(beetom.DefinitionId);

        // The shake tick belongs to the advance: it is consumed identically whether or not the frame is drawn.
        ordinary.FrameCounter = 0;
        ordinary.ShakeTimer = WrappingCountdown.Start(2);
        enemies.Drawing.AdvanceLayers(Position16<RoomPixels>.Zero, 0, 7);
        ushort undisplayed = ordinary.ShakeTimer.RawValue;
        ordinary.ShakeTimer = WrappingCountdown.Start(2);
        sprites.BeginFrame();
        enemies.Drawing.AdvanceLayers(Position16<RoomPixels>.Zero, 0, 7);
        enemies.Drawing.DrawLayers(sprites, Position16<RoomPixels>.Zero, 0, 7);
        sprites.FinalizeFrame();
        AssertEqual(1, undisplayed, "An undrawn frame consumes one shake tick");
        AssertEqual(undisplayed, ordinary.ShakeTimer.RawValue, "A drawn frame consumes the same shake tick");
        AssertEqual(new Position16<ScreenPixels>(101, 80), sprites.Draws[0].Origin, "The drawn frame shows the shaken origin");
        Console.WriteLine($"Enemy-format draws: ordinary and {entry - 2}-component extended entries from their graphics set's sheets, a frame in another row, and a projectile over the enemy tile page; shake advance independent of display.");

        static void AssertImage(SpriteDraw draw, SpriteSource source, SpriteSheetFrame frame, Position16<ScreenPixels> origin, int? row, string subject)
        {
            var image = draw as ImageSpriteDraw ?? throw new InvalidOperationException($"{subject} is a {draw.GetType().Name}, not a sheet frame.");
            AssertEqual(source, image.Source, $"{subject} names its slot");
            AssertTrue(ReferenceEquals(frame, image.Image), $"{subject} shows its sheet's frame");
            AssertEqual(origin, image.Origin, $"{subject} is placed at its screen origin");
            AssertEqual(row, image.Colouring?.HoldingRow, $"{subject} shows its frame in the row holding the colours it is drawn in");
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
