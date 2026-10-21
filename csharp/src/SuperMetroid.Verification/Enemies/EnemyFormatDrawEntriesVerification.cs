using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.TestSupport.VerificationOracles;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Hardware;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Enemies;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.Core.World.Artwork;

/// <summary>
/// Checks #1343's enemy-format draws: an ordinary enemy, an extended enemy and an enemy projectile publish whole-frame
/// entries over the OBJ page their room's uploaded enemy tile sets bind, and the shake tick is a simulation phase.
/// </summary>
internal static class EnemyFormatDrawEntriesVerification
{
    /// <summary>The ordinary enemy's packed tile base and palette bits.</summary>
    private const ushort OrdinaryTileBase = 64;
    /// <summary>Palette two, as packed attribute bits.</summary>
    private const ushort OrdinaryPaletteBits = 2 << 9;
    /// <summary>The projectile's palette.</summary>
    private const byte ProjectilePalette = 5;
    /// <summary>The projectile's palette as packed attribute bits.</summary>
    private const ushort ProjectilePaletteBits = ProjectilePalette << 9;
    /// <summary>The projectile's packed tile base.</summary>
    private const ushort ProjectileTileBase = 4;
    /// <summary>The projectile's graphics index: its palette bits with its tile base.</summary>
    private const ushort ProjectileGraphicsIndex = ProjectilePaletteBits | ProjectileTileBase;

    /// <summary>Publishes a constructed population and checks every entry and the bound tile set.</summary>
    internal static void VerifyEnemyFormatDrawEntries()
    {
        InstalledEnemyArtwork artwork = InstalledFixtures.EnemyArtwork;
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance) { TileArtwork = artwork.Enemies, BossArtwork = artwork.Bosses, AreaArtwork = artwork.Areas };
        var resolver = new EnemyTileTransfers(artwork);
        EnemySpritemapCatalog spritemaps = artwork.Enemies.Spritemaps ?? throw new InvalidOperationException("The installation has no enemy spritemaps.");
        EnemyExtendedFrameCatalog extendedFrames = artwork.Enemies.ExtendedFrames ?? throw new InvalidOperationException("The installation has no extended enemy frames.");
        EnemyProjectileSpritemapCatalog projectileSpritemaps = artwork.Enemies.ProjectileSpritemaps ?? throw new InvalidOperationException("The installation has no enemy projectile spritemaps.");

        // The room's enemy tile set reaches the OBJ characters only through the NMI queue, as a named transfer.
        RoomEnemyDefinition boyon = RoomEnemyDefinitionCatalog.Get(EnemyDefinitionId.Boyon);
        int boyonBytes = boyon.TileDataSize & short.MaxValue;
        var queue = new VramWriteQueue();
        queue.Enqueue((ushort)boyonBytes, boyon.TileDataAddress, EnemyVram.EnemyTilesDma(0));
        queue.DrainTo(new SnesVram(), resolver, enemies.Room.Session.ObjCharacters, BgArtworkFixtures.For(new SnesVram()));
        ObjCharacterUpload upload = enemies.Room.Session.ObjCharacters.Uploads.Single();
        AssertTrue(upload.Source is TransferObjCharacters, "Enemy tiles bind their installed artwork, not copied bytes");
        SpriteArtwork page = new ObjCharacterTablePage().Page(enemies.Room.Session.ObjCharacters, resolver);
        AssertTrue(artwork.Enemies.TryResolve(boyon.TileDataAddress, boyonBytes, out ReadOnlyMemory<byte> boyonTiles) &&
            page.Characters.Slice(upload.FirstByte, boyonBytes).SequenceEqual(boyonTiles.Span),
            "The OBJ page shows the room's enemy tile set where its upload lands");

        // Ordinary enemy on layer zero.
        EnemySpritemapDefinition ordinaryFrame = EnemySpritemapDefinitions.Frames.First(frame =>
            spritemaps.TryGetDisplay(frame.Bank, frame.Pointer, out EnemySpritemapParts parts) && parts.Count > 0);
        RoomEnemySlot ordinary = enemies.SlotTable.Slots[0];
        ordinary.Bind(EnemyDefinitionId.Boyon, default(RoomEnemyDefinition));
        ordinary.Frame = RoomEnemySlotFrameAccess.FrameAt(ordinaryFrame.Bank, ordinaryFrame.Pointer);
        ordinary.VramTilesIndex = OrdinaryTileBase;
        ordinary.PaletteIndex = OrdinaryPaletteBits;
        ordinary.Position = ordinary.Position.WithWhole(new(100, 80));
        enemies.Drawing.Enqueue(0, 0);

        // Extended enemy on layer one, showing a frame with several components.
        EnemySpritemapDefinition extendedFrame = EnemyExtendedFrameDefinitions.Frames.First(frame =>
            frame.Bank != EnemyExtendedFrameDefinitions.Bank &&
            extendedFrames.TryGetDisplay(frame.Bank, frame.Pointer, out ReadOnlyMemory<EnemyExtendedDrawComponent> components) &&
            components.Length > 1);
        RoomEnemySlot extended = enemies.SlotTable.Slots[1];
        extended.Bind(EnemyDefinitionId.Boyon, default(RoomEnemyDefinition));
        extended.ExtraProperties = extended.ExtraProperties.With(EnemyExtraProperties.UsesExtendedSpritemap);
        extended.Frame = RoomEnemySlotFrameAccess.FrameAt(extendedFrame.Bank, extendedFrame.Pointer);
        extended.InstructionTimer = WrappingCountdown.Start(7);
        extended.Position = extended.Position.WithWhole(new(120, 110));
        enemies.Drawing.Enqueue(1, EnemySlotTable.NativeSlotSize);

        // Enemy projectile in the high-priority pass.
        EnemyProjectileFrameId projectileFrame = EnemyProjectilePresentationFrameSets.All.ToArray()
            .Select(frame => EnemyProjectileFrames.Identity(frame.Name))
            .First(frame => EnemyProjectileFrames.Parts(projectileSpritemaps, frame).Length > 0);
        RoomEnemyProjectileSlot projectile = enemies.Projectiles.EnemyProjectiles[17];
        projectile.Kind = RoomEnemyProjectileKind.CeresRidleyFireball;
        projectile.DrawPriority = EnemyProjectileDrawPriority.High;
        projectile.Frame = projectileFrame;
        projectile.GraphicsIndex = ProjectileGraphicsIndex;
        projectile.Position = projectile.Position.WithWhole(new(30, 40));

        var sprites = new SpriteFrameBuffer();
        sprites.BeginFrame();
        enemies.Drawing.AdvanceLayers(Position16<RoomPixels>.Zero, 0, 7);
        enemies.Drawing.DrawLayers(sprites, page, Position16<RoomPixels>.Zero, 0, 7);
        enemies.Projectiles.DrawEnemyProjectilePass(sprites, page, Position16<RoomPixels>.Zero, EnemyProjectileDrawPriority.High, false);
        sprites.FinalizeFrame();
        SpriteDraw[] draws = [.. sprites.Draws];
        AssertTrue(draws.All(draw => ReferenceEquals(page, draw.Artwork)), "Every entry binds the room's OBJ page");

        spritemaps.TryGetDisplay(ordinaryFrame.Bank, ordinaryFrame.Pointer, out EnemySpritemapParts ordinaryParts);
        AssertEntry(draws[0], new SpriteSource(SpriteSourceKind.Enemy, 0), EnemySpriteFrames.Frame(ordinaryParts, OrdinaryTileBase, OrdinaryPaletteBits),
            new(100, 80), 2, "Ordinary enemy");

        extendedFrames.TryGetDisplay(extendedFrame.Bank, extendedFrame.Pointer, out ReadOnlyMemory<EnemyExtendedDrawComponent> components);
        int entry = 1;
        foreach (EnemyExtendedDrawComponent component in components.Span)
        {
            Position16<OamPixels> origin = new Position16<OamPixels>(120, 110) + component.Offset;
            if (((origin.X + 128) & EnemyDrawMasks.OffscreenBits) != 0 || ((origin.Y + 128) & EnemyDrawMasks.OffscreenBits) != 0)
                continue;
            AssertEntry(draws[entry++], new SpriteSource(SpriteSourceKind.Enemy, 1), EnemySpriteFrames.Frame(component.Parts, 0, 0),
                new(origin.X, origin.Y), 0, "Extended enemy component");
        }
        AssertTrue(entry > 2, "The extended enemy publishes one entry per visible component");

        AssertEntry(draws[entry++], new SpriteSource(SpriteSourceKind.EnemyProjectile, 17),
            EnemySpriteFrames.Frame(EnemyProjectileFrames.Parts(projectileSpritemaps, projectileFrame), ProjectileTileBase, ProjectilePaletteBits), new(30, 40), ProjectilePalette, "Enemy projectile");
        AssertEqual(draws.Length, entry, "No other entries are published");

        // The shake tick belongs to the advance: it is consumed identically whether or not the frame is drawn.
        ordinary.FrameCounter = 0;
        ordinary.ShakeTimer = WrappingCountdown.Start(2);
        enemies.Drawing.AdvanceLayers(Position16<RoomPixels>.Zero, 0, 7);
        ushort undisplayed = ordinary.ShakeTimer.RawValue;
        ordinary.ShakeTimer = WrappingCountdown.Start(2);
        sprites.BeginFrame();
        enemies.Drawing.AdvanceLayers(Position16<RoomPixels>.Zero, 0, 7);
        enemies.Drawing.DrawLayers(sprites, page, Position16<RoomPixels>.Zero, 0, 7);
        sprites.FinalizeFrame();
        AssertEqual(1, undisplayed, "An undrawn frame consumes one shake tick");
        AssertEqual(undisplayed, ordinary.ShakeTimer.RawValue, "A drawn frame consumes the same shake tick");
        AssertEqual(new Position16<ScreenPixels>(101, 80), sprites.Draws[0].Origin, "The drawn frame shows the shaken origin");
        Console.WriteLine($"Enemy-format draws: ordinary, {entry - 2}-component extended and projectile entries over the bound enemy tile page; shake advance independent of display.");

        static void AssertEntry(SpriteDraw draw, SpriteSource source, SpriteFrame frame, Position16<ScreenPixels> origin, byte palette, string subject)
        {
            AssertEqual(source, draw.Source, $"{subject} names its slot");
            AssertTrue(ReferenceEquals(frame, draw.Frame), $"{subject} shows its composition's frame");
            AssertEqual(origin, draw.Origin, $"{subject} is placed at its screen origin");
            AssertEqual(palette, draw.Palette, $"{subject} binds its selected palette");
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
