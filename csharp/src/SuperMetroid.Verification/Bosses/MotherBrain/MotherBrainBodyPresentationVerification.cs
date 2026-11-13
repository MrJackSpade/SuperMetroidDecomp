using SuperMetroid.AssetExtraction;
using SuperMetroid.AssetExtraction.World.Artwork;
using SuperMetroid.Core.Animation;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Enemies.EnemyContentIdentityFixtureVerification;
using static SuperMetroid.Verification.Bosses.MotherBrain.MotherBrainBodyPresentationFixtureVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Bosses.MotherBrain;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.World;
namespace SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Verification.Harness;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Enemies.BossBodies;

/// <summary>Checks Mother Brain's installed OAM limbs and BG2 body art while preserving animation mechanics.</summary>
internal static class MotherBrainBodyPresentationVerification
{
    /// <summary>
    /// Exercises the statically identified missing Mother Brain body draw path with
    /// a RAM-only fixture. This is not a gameplay search for cartridge accesses.
    /// </summary>
    internal static void VerifyMotherBrainBodyPresentation()
    {
        var author = new EnemyIdentityFixture();
        EnemyExtendedFrameDocument stockOam = BodyOam();
        EnemyBg2FrameDocument stockBg2 = BodyBg2();
        var baseline = new MotherBrainPresentationFixture(Build(stockOam, stockBg2));
        var physicalFrames = Enum.GetValues<MotherBrainBodySegment>()
            .SelectMany(program => MotherBrainAnimations.BodySegments.Beats(program))
            .OfType<AnimationBeat<MotherBrainBodyFrame, MotherBrainBodyCue>.Frame>()
            .Select(beat => MotherBrainBodyVisualDefinitionsConstants.Frame(beat.Shown.Index).Pointer).ToHashSet();
        AssertTrue(physicalFrames.SetEquals(MotherBrainBodyVisualDefinitions.Frames
            .Select(frame => frame.Pointer)), "installed body inventory covers every compiled program selector, not only exercised paths");
        foreach (NativeEnemySpritemapDefinition frame in MotherBrainBodyVisualDefinitions.Frames)
        {
            baseline.SetFrame(frame.Pointer, fresh: true);
            baseline.ClearBg2();
            AssertBodyOam(baseline.DrawInto, stockOam.Frames[frame.Name], baseline.Body, baseline.Inner.Enemies, () => baseline.BodyFrameShowsNothing, frame.Name);
            AssertBodyBg2(baseline.Bg2Words, stockBg2, frame.Name);

            baseline.SetFrame(frame.Pointer, fresh: false);
            baseline.MarkBg2();
            ushort[] retained = baseline.Bg2Words;
            AssertBodyOam(baseline.DrawInto, stockOam.Frames[frame.Name], baseline.Body, baseline.Inner.Enemies, () => baseline.BodyFrameShowsNothing, frame.Name);
            AssertTrue(retained.SequenceEqual(baseline.Bg2Words),
                "repeat OAM draws do not rewrite BG2 without the native new-frame bit");
        }
        int steps = 0;
        foreach (MotherBrainBodySegment program in new[] { MotherBrainBodySegment.WalkForwardMedium,
                     MotherBrainBodySegment.WalkBackwardFast, MotherBrainBodySegment.CrouchSlow })
        {
            baseline.SetProgram(program);
            for (int frame = 0; frame < 100; frame++, steps++)
            {
                baseline.Step();
                baseline.Draw();
            }
        }
        AssertTrue(baseline.ScrollCalls.Count > 0, "production body bytecode actually moves and counter-scrolls");
        // Her body's pictures are composed at install (#1466), so missing BG2 art fails there, before anything draws.
        AssertThrows<InvalidDataException>(() => BossBodySheetFixtures.For(Build(stockOam, bg2: null).Bosses, BossBodyOwner.MotherBrain),
            "missing mixed-frame BG2 art fails explicitly");
        // Installed artwork is not part of debugger states: the room binding holds it as non-serialized, and the host
        // reinstalls it after a restore.
        Console.WriteLine($"PASS Mother Brain: all 17 exact OAM/BG2 poses, new-frame gating, " +
            $"{steps} production instruction steps with counter-scroll, and missing-art rejection. RAM-only.");

        // She draws from sheets composed over the installed enemy artwork, so the fixture starts from it and replaces only
        // the extended frames and body BG2 frames it authors (#1459), as #1446's boss fixture does.
        ImportEnemyArtwork Build(EnemyExtendedFrameDocument oam, EnemyBg2FrameDocument? bg2)
        {
            EnemyTileSourceArtwork tiles = SuperMetroid.TestSupport.RepositoryInstallation.Verification.CartridgeImportContent.FreshEnemyArtwork().Enemies;
            tiles.ExtendedFrames = EnemyExtendedFrameDocuments.Load(author.Json(oam));
            return new(tiles,
                new BossSourceArtwork { MotherBrainBodyBg2Frames = bg2 is null ? null : MotherBrainBodyBg2FrameCatalog.Load(author.Json(bg2)) },
                new AreaSourceArtwork());
        }

        EnemyExtendedFrameDocument BodyOam()
        {
            EnemyExtendedFrameDocument document = author.ExtendedDocument();
            var retail = InstalledFixtures.EnemyArtwork.Enemies.ExtendedFrames
                ?? throw new InvalidOperationException("The installation holds the retail extended frames.");
            foreach (NativeEnemySpritemapDefinition frame in MotherBrainBodyVisualDefinitions.Frames)
            {
                // A frame the cartridge draws limbs for shows its first retail limb's character, which inks pixels under her
                // graphics set, so its picture shows something (#1538); placement and attributes are the fixture's own. A
                // frame with no retail limb keeps the fixture document's character.
                SpriteVisualPart authored = document.Frames[frame.Name][0].Parts[0];
                if (retail.TryGetDisplay(frame.Bank, frame.Pointer, out var shown) && shown.Length > 0 && shown.Span[0].Parts.Length > 0)
                    authored = authored with { TileColumn = shown.Span[0].Parts[0].Character % 16, TileRow = shown.Span[0].Parts[0].Character / 16 };
                SpriteVisualPart part = authored with
                { OffsetX = 2, OffsetY = 3, Size = 16, Palette = 1, Priority = 2, FlipX = false, FlipY = false };
                document.Frames[frame.Name] =
                [ new() { OffsetX = 0, OffsetY = 0, Parts = [part] },
                  new() { OffsetX = 6, OffsetY = 5, Parts = [part with { Size = 8 }] } ];
            }
            return document;
        }

        static EnemyBg2FrameDocument BodyBg2() => new()
        {
            Version = MotherBrainBodyBg2FrameDefinitions.Version,
            Frames = MotherBrainBodyBg2FrameDefinitions.Frames.SelectToArray((frame, index) =>
                KeyValuePair.Create(frame.Name, new EnemyBg2WriteDocument[]
                { new() { X = 0, Y = 0, Tiles = RoomBackgroundTilemapCellWords.Cells(100 + index, 200 + index) },
                  new() { X = 1, Y = 1, Tiles = RoomBackgroundTilemapCellWords.Cells(300 + index, 400 + index) } })).ToDictionary(),
        };
    }

    /// <summary>
    /// Checks that <paramref name="draw"/> stands for each authored limb's OAM position, tile, palette, flip, size, and
    /// drawing order. A sheet lays out identical pictures once (#1513), so a draw of a picture several frames share stands
    /// for any of those frames' OAM.
    /// </summary>
    /// <param name="draw">Publishes the body's frame into a sprite frame, once.</param>
    /// <param name="authored">The frame's authored limbs, in drawing order.</param>
    /// <param name="body">The body's slot, whose position, tile base and palette row place the limbs.</param>
    /// <param name="enemies">The room's enemy system, which places the body's artwork.</param>
    /// <param name="pictureShowsNothing">
    /// Whether the frame's finished picture shows nothing, so that it draws nothing (#1538); asked only of a frame with limbs.
    /// </param>
    /// <param name="context">Names the frame in a failure message.</param>
    internal static void AssertBodyOam(Action<SpriteFrameBuffer> draw, EnemyExtendedVisualComponent[] authored, RoomEnemySlot body, RoomEnemySystem enemies,
        Func<bool> pictureShowsNothing, string context)
    {
        if (authored.Length == 0 || pictureShowsNothing())
        {
            AssertEqual(0, SpriteFrameBuffer.DrawnOam(draw).NextByteOffset, $"a frame whose picture shows nothing draws nothing: {context}");
            return;
        }
        var expected = new byte[authored.Length * 4];
        byte high = 0;
        for (int index = 0; index < authored.Length; index++)
        {
            EnemyExtendedVisualComponent component = authored[index];
            SpriteVisualPart part = component.Parts.Single();
            int x = body.Position.X.Whole + component.OffsetX + part.OffsetX;
            int y = body.Position.Y.Whole + component.OffsetY + part.OffsetY;
            // The cartridge adds the body's tile base to each part and ORs the row holding its colours into the part's (#1446).
            int palette = (part.Palette ?? throw new InvalidDataException("Body OAM fixture requires an explicit palette.")) | body.PaletteRowIn(enemies);
            ushort tile = checked((ushort)(part.TileRow * 16 + part.TileColumn + body.NativeTileBaseIn(enemies) | palette << 9 |
                part.Priority << 12 | (part.FlipX ? 0x4000 : 0) | (part.FlipY ? 0x8000 : 0)));
            expected[index * 4] = unchecked((byte)x); expected[index * 4 + 1] = unchecked((byte)y);
            expected[index * 4 + 2] = (byte)tile; expected[index * 4 + 3] = LittleEndianBytes.HighByte(tile);
            high |= (byte)(((LittleEndianBytes.HighByte(unchecked((ushort)x)) & 1) | (part.Size == 16 ? 2 : 0)) << (index * 2));
        }
        var native = new OamBuffer();
        native.BeginFrame();
        expected.CopyTo(native.LowTableBytes, 0);
        native.HighTableBytes[0] = high;
        native.NextByteOffset = expected.Length;
        if (SpriteFrameBufferAccess.DrawnStandsFor(draw, native))
            return;
        // Only on failure: draw once more as the sheet's first composition of each picture, to say where it departs.
        OamBuffer drawn = SpriteFrameBuffer.DrawnOam(draw);
        int differs = Enumerable.Range(0, OamBuffer.LowTableByteCount).FirstOrDefault(index => native.LowTable[index] != drawn.LowTable[index], -1);
        AssertTrue(false, $"body limbs stand for the authored art's count, position, tile, palette, flip, size and order: {context}; " +
            $"{authored.Length} limbs authored, {drawn.NextByteOffset / 4} drawn; " +
            (differs < 0 ? "low tables agree, high tables differ"
                : $"first difference at limb {differs / 4} byte {differs % 4}: authored ${native.LowTable[differs]:X2}, drawn ${drawn.LowTable[differs]:X2}"));
    }

    /// <summary>Checks the exact BG2 tilemap words, each authored cell's tile encoded through the codec, and destinations written for a selected body pose.</summary>
    private static void AssertBodyBg2(ushort[] actual, EnemyBg2FrameDocument authored, string oamName)
    {
        // Her body shows each of her frames whole (#1459, #1515): every word a frame writes shows, and a frame the cartridge
        // writes over another shows that frame elsewhere, so only the written cells are compared.
        if (!authored.Frames.TryGetValue(oamName.Replace("_oam_", "_bg2_", StringComparison.Ordinal), out var writes))
            return;
        foreach (EnemyBg2WriteDocument write in writes)
            for (int tile = 0; tile < write.Tiles.Length; tile++)
                AssertEqual(write.Tiles[tile].PackedWord, actual[write.Y * EnemyBg2FrameLayout.TilemapWidth + write.X + tile],
                    "body BG2 exact ordered runs and destinations");
    }

    /// <summary>Invokes import-only parsers for the 17 statically inventoried body roots.</summary>
    internal static void VerifyMotherBrainBodyStockPresentation()
    {
        var source = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        MotherBrainBodyBg2FrameCatalog bg2 = MotherBrainBodyBg2FrameCatalog.Load(new MemoryStream(
            MotherBrainBodyBg2FrameFiles.Extract(source), writable: false));
        EnemyExtendedFrameCatalog oam = EnemyExtendedFrameDocuments.Load(new MemoryStream(
            EnemyExtendedFrameFiles.Extract(source), writable: false));
        int limbs = 0, sprites = 0, runs = 0, tiles = 0;
        foreach (NativeEnemySpritemapDefinition frame in MotherBrainBodyVisualDefinitions.Frames)
        {
            AssertTrue(oam.TryGet(frame.Bank, frame.Pointer, out var parts), frame.Name + " imports its OAM half");
            limbs += parts.Length;
            foreach (EnemyExtendedDrawComponent component in parts.Span) sprites += component.Parts.Length;
            if (!MotherBrainBodyVisualDefinitionsConstants.HasBg2(frame.Pointer)) continue;
            AssertTrue(bg2.TryGet(frame.Pointer, out var writes), frame.Name + " imports its BG2 half");
            runs += writes.Length;
            foreach (EnemyBg2TilemapWrite write in writes.Span) tiles += write.Tiles.Length;
        }
        Console.WriteLine($"PASS stock import: 17 Mother Brain roots, {limbs} OAM limbs/{sprites} sprites, " +
            $"16 BG2 frames/{runs} ordered runs/{tiles} tile words. Production runtime receives no cartridge source.");
        ImportEnemyArtwork installed = InstalledFixtures.EnemyArtwork;
        AssertEqual(bg2.ContentIdentity, (installed.Bosses.MotherBrainBodyBg2Frames ?? throw new InvalidOperationException("Verification requires installed.MotherBrainBodyBg2Frames.")).ContentIdentity,
            "manifest loads the exact imported Mother Brain body catalog");
        AssertEqual(oam.ContentIdentity, (installed.Enemies.ExtendedFrames ?? throw new InvalidOperationException("Verification requires installed.ExtendedFrames.")).ContentIdentity,
            "manifest loads the complete current multipart catalog");
    }

    /// <summary>Runs the RAM-only Mother Brain presentation suite through the verification dispatcher.</summary>
    [VerificationCommand("--mother-brain-body-presentation")]
    internal static int MotherBrainBodyPresentationCommand()
    {
        Suite(nameof(VerifyMotherBrainBodyPresentation), () => VerifyMotherBrainBodyPresentation());
        return 0;
    }

    /// <summary>Runs the stock import checks through the verification dispatcher.</summary>
    [VerificationCommand("--mother-brain-body-stock-presentation")]
    internal static int MotherBrainBodyStockPresentationCommand()
    {
        Suite(nameof(VerifyMotherBrainBodyStockPresentation), () => VerifyMotherBrainBodyStockPresentation());
        return 0;
    }
}
