using SuperMetroid.AssetExtraction.World.Artwork;
using SuperMetroid.Core.Programs;
using SuperMetroid.Tooling;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Enemies.EnemyAnimationIsolationFixtureVerification;
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
        EnemyExtendedFrameDocument stockOam = BodyOam(edited: false), editedOam = BodyOam(edited: true);
        EnemyBg2FrameDocument stockBg2 = BodyBg2(edited: false), editedBg2 = BodyBg2(edited: true);
        var baseline = new MotherBrainPresentationFixture(Build(stockOam, stockBg2));
        var mixed = MotherBrainBodyVisualDefinitions.Frames
            .Where(frame => MotherBrainBodyVisualDefinitionsConstants.HasBg2(frame.Pointer)).ToArray();
        var physicalFrames = Enum.GetValues<MotherBrainBodyProgram>()
            .SelectMany(program => MotherBrainAnimations.Body.Steps(program))
            .OfType<AnimationProgramStep<MotherBrainBodyProgram, MotherBrainBodyFrame, MotherBrainBodyCue>.ShowStep>()
            .Select(step => MotherBrainBodyVisualDefinitionsConstants.Frame(step.Frame.Index).Pointer).ToHashSet();
        AssertTrue(physicalFrames.SetEquals(MotherBrainBodyVisualDefinitions.Frames
            .Select(frame => frame.Pointer)), "installed body inventory covers every compiled program selector, not only exercised paths");
        // Remap the whole mixed pose, not only its limbs. The native selector must not change.
        for (int index = 0; index < mixed.Length; index++)
            (editedOam.DisplayFrames ?? throw new InvalidOperationException("Verification requires editedOam.DisplayFrames."))[mixed[index].Name] = mixed[(index + 1) % mixed.Length].Name;
        var replacement = new MotherBrainPresentationFixture(Build(editedOam, editedBg2));
        foreach (NativeEnemySpritemapDefinition frame in MotherBrainBodyVisualDefinitions.Frames)
        {
            baseline.SetFrame(frame.Pointer, fresh: true);
            replacement.SetFrame(frame.Pointer, fresh: true);
            baseline.ClearBg2(); replacement.ClearBg2();
            OamBuffer original = baseline.Draw(), changed = replacement.Draw();
            AssertBodyOam(original, stockOam.Frames[frame.Name], baseline.Body, baseline.Inner.Enemies);
            string selected = (editedOam.DisplayFrames ?? throw new InvalidOperationException("Verification requires editedOam.DisplayFrames."))[frame.Name];
            AssertBodyOam(changed, editedOam.Frames[selected], replacement.Body, replacement.Inner.Enemies);
            AssertBodyBg2(baseline.Bg2Words, stockBg2, frame.Name);
            AssertBodyBg2(replacement.Bg2Words, editedBg2, selected);
            AssertEnemyAnimationMechanics(baseline.Inner, replacement.Inner, frame.Pointer);
            AssertEqual(frame.Pointer, replacement.Body.SpritemapPointer, "display remap retains physical frame");

            replacement.SetFrame(frame.Pointer, fresh: false);
            replacement.MarkBg2();
            ushort[] retained = replacement.Bg2Words;
            AssertBodyOam(replacement.Draw(), editedOam.Frames[selected], replacement.Body, replacement.Inner.Enemies);
            AssertTrue(retained.SequenceEqual(replacement.Bg2Words),
                "repeat OAM draws do not rewrite BG2 without the native new-frame bit");
        }
        int steps = 0;
        foreach (MotherBrainBodyProgram program in new[] { MotherBrainBodyProgram.WalkForwardMedium,
                     MotherBrainBodyProgram.WalkBackwardFast, MotherBrainBodyProgram.CrouchSlow })
        {
            baseline.SetProgram(program); replacement.SetProgram(program);
            for (int frame = 0; frame < 100; frame++, steps++)
            {
                baseline.Step(); replacement.Step();
                baseline.Draw(); replacement.Draw();
                AssertEnemyAnimationMechanics(baseline.Inner, replacement.Inner, steps);
                AssertAnimationValues(baseline.State, replacement.State, "Mother Brain instruction state");
                AssertTrue(baseline.ScrollCalls.SequenceEqual(replacement.ScrollCalls),
                    "editable art preserves each body/BG2 counter-scroll");
            }
        }
        AssertTrue(baseline.ScrollCalls.Count > 0, "production body bytecode actually moves and counter-scrolls");
        // Her body's pictures are composed at install (#1466), so missing BG2 art fails there, before anything draws.
        AssertThrows<InvalidDataException>(() => BossBodySheetFixtures.For(Build(stockOam, bg2: null).Bosses, BossBodyOwner.MotherBrain),
            "missing mixed-frame BG2 art fails explicitly");
        // Installed artwork is not part of debugger states: the room binding holds it as non-serialized, and the host
        // reinstalls it after a restore.
        Console.WriteLine($"PASS Mother Brain: all 17 exact OAM/BG2 poses, mixed remaps, new-frame gating, " +
            $"{steps} production instruction steps with unchanged movement/timing/scroll/physics, and missing-art rejection. RAM-only.");

        // She draws from sheets composed over the installed enemy artwork, so the fixture starts from it and replaces only
        // the extended frames and body BG2 frames it authors (#1459), as #1446's boss fixture does.
        InstalledEnemyArtwork Build(EnemyExtendedFrameDocument oam, EnemyBg2FrameDocument? bg2)
        {
            EnemyTileSourceArtwork tiles = SuperMetroid.TestSupport.RepositoryInstallation.RepositoryInstallation.Installation.LoadEnemyTiles().Enemies;
            tiles.ExtendedFrames = EnemyExtendedFrameDocuments.Load(author.Json(oam));
            return new(tiles,
                new BossSourceArtwork { MotherBrainBodyBg2Frames = bg2 is null ? null : MotherBrainBodyBg2FrameCatalog.Load(author.Json(bg2)) },
                new AreaSourceArtwork());
        }

        EnemyExtendedFrameDocument BodyOam(bool edited)
        {
            EnemyExtendedFrameDocument document = author.ExtendedDocument();
            int index = 0;
            foreach (NativeEnemySpritemapDefinition frame in MotherBrainBodyVisualDefinitions.Frames)
            {
                SpriteVisualPart part = document.Frames[frame.Name][0].Parts[0] with
                { OffsetX = 2, OffsetY = 3, Size = 16, TileColumn = index % 16, TileRow = index / 16,
                    Palette = 1, Priority = 2, FlipX = edited, FlipY = edited };
                document.Frames[frame.Name] =
                [ new() { OffsetX = edited ? 5 : 0, OffsetY = edited ? -4 : 0, Parts = [part] },
                  new() { OffsetX = edited ? -7 : 6, OffsetY = edited ? 8 : 5, Parts = [part with { Size = 8 }] } ];
                index++;
            }
            return document;
        }

        static EnemyBg2FrameDocument BodyBg2(bool edited) => new()
        {
            Version = MotherBrainBodyBg2FrameDefinitions.Version,
            Frames = MotherBrainBodyBg2FrameDefinitions.Frames.SelectToArray((frame, index) =>
                KeyValuePair.Create(frame.Name, new EnemyBg2WriteDocument[]
                { new() { X = edited ? 3 : 0, Y = edited ? 2 : 0, Tiles = RoomBackgroundTilemapCellWords.Cells(100 + index, 200 + index) },
                  new() { X = edited ? 4 : 1, Y = edited ? 3 : 1, Tiles = RoomBackgroundTilemapCellWords.Cells(300 + index, edited ? 460 + index : 400 + index) } })).ToDictionary(),
        };
    }

    /// <summary>Checks each authored limb's OAM position, tile, palette, flip, size, and drawing order.</summary>
    internal static void AssertBodyOam(OamBuffer actual, EnemyExtendedVisualComponent[] authored, RoomEnemySlot body, RoomEnemySystem enemies)
    {
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
            ushort tile = checked((ushort)(part.TileRow * 16 + part.TileColumn + body.TileIndexBase | palette << 9 |
                part.Priority << 12 | (part.FlipX ? 0x4000 : 0) | (part.FlipY ? 0x8000 : 0)));
            expected[index * 4] = unchecked((byte)x); expected[index * 4 + 1] = unchecked((byte)y);
            expected[index * 4 + 2] = (byte)tile; expected[index * 4 + 3] = LittleEndianBytes.HighByte(tile);
            high |= (byte)(((LittleEndianBytes.HighByte(unchecked((ushort)x)) & 1) | (part.Size == 16 ? 2 : 0)) << (index * 2));
        }
        AssertEqual(expected.Length, actual.NextByteOffset, "body emits exactly the authored limb count");
        AssertTrue(expected.SequenceEqual(actual.LowTable[..expected.Length]), "body limb position/tile/palette/flip/order match authored art");
        AssertEqual(high, actual.HighTable[0], "body limb sizes and high X bits match authored art");
    }

    /// <summary>Checks the exact BG2 tilemap words, each authored cell's tile encoded through the codec, and destinations written for a selected body pose.</summary>
    private static void AssertBodyBg2(ushort[] actual, EnemyBg2FrameDocument authored, string oamName)
    {
        // Her body draws into the BG2 image she owns (#1459); cells no write reaches keep the image's fill.
        var expected = Enumerable.Repeat(BackgroundTileWordBytes.Words([MotherBrainPresentationFixture.Bg2FillTile])[0],
            EnemyBg2FrameLayout.TilemapWidth * EnemyBg2FrameLayout.TilemapHeight).ToArray();
        if (authored.Frames.TryGetValue(oamName.Replace("_oam_", "_bg2_", StringComparison.Ordinal), out var writes))
            foreach (EnemyBg2WriteDocument write in writes)
                for (int tile = 0; tile < write.Tiles.Length; tile++)
                    expected[write.Y * EnemyBg2FrameLayout.TilemapWidth + write.X + tile] = write.Tiles[tile].PackedWord;
        for (int index = 0; index < expected.Length; index++)
            AssertEqual(expected[index], actual[index], "body BG2 exact ordered runs and destinations");
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
        using var temporary = new TestScratchDirectory("map-catalog");
        string stockPath = Path.Combine(temporary.Root, "stock"), overridePath = Path.Combine(temporary.Root, "overrides");
        EnemyTileArtworkFiles.Extract(source, stockPath, SupportedCartridge.Sha256);
        EnemyTileArtworkFiles.ValidateStock(stockPath);
        InstalledEnemyArtwork installed = EnemyTileArtworkFiles.Load(stockPath, null);
        AssertEqual(bg2.ContentIdentity, (installed.Bosses.MotherBrainBodyBg2Frames ?? throw new InvalidOperationException("Verification requires installed.MotherBrainBodyBg2Frames.")).ContentIdentity,
            "manifest loads the exact imported Mother Brain body catalog");
        AssertEqual(oam.ContentIdentity, (installed.Enemies.ExtendedFrames ?? throw new InvalidOperationException("Verification requires installed.ExtendedFrames.")).ContentIdentity,
            "manifest loads the complete current multipart catalog");
        string file = Path.Combine(stockPath, MotherBrainBodyBg2FrameDefinitions.FileName);
        var author = new EnemyIdentityFixture();
        using var parsed = System.Text.Json.JsonDocument.Parse(File.ReadAllBytes(file));
        var document = RequiredJson.Deserialize<EnemyBg2FrameDocument>(parsed,
            new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase });
        EnemyBg2WriteDocument[] first = document.Frames.Values.First();
        first[0].Tiles[0] = RoomBackgroundTilemapCellWords.Cell(first[0].Tiles[0].PackedWord ^ 1);
        Directory.CreateDirectory(overridePath);
        string selectedFile = Path.Combine(overridePath, MotherBrainBodyBg2FrameDefinitions.FileName);
        using (var json = author.Json(document)) File.WriteAllBytes(selectedFile, json.ToArray());
        InstalledEnemyArtwork edited = EnemyTileArtworkFiles.Load(stockPath, overridePath);
        AssertTrue(installed.ContentIdentity != edited.ContentIdentity, "selected Mother Brain BG2 edit changes installed bundle identity");
        AssertEqual(installed.Enemies.ExtendedFrames.ContentIdentity, (edited.Enemies.ExtendedFrames ?? throw new InvalidOperationException("Verification requires edited.ExtendedFrames.")).ContentIdentity,
            "a BG2-only override does not alter limb compositions");
        using (var json = author.Json(document with { Version = document.Version + 1 }))
            File.WriteAllBytes(selectedFile, json.ToArray());
        AssertThrows<InvalidDataException>(() => EnemyTileArtworkFiles.Load(stockPath, overridePath),
            "malformed Mother Brain override fails at installation loading");
        using (var json = author.Json(document)) File.WriteAllBytes(file, json.ToArray());
        AssertThrows<InvalidDataException>(() => EnemyTileArtworkFiles.ValidateStock(stockPath),
            "modified Mother Brain stock fails its manifest hash");
        Console.WriteLine("PASS Mother Brain installation: stock hashes, selected overrides/identity and malformed/tampered-data rejection.");
    }

    /// <summary>Runs the RAM-only Mother Brain presentation suite through the verification dispatcher.</summary>
    [VerificationCommand("--mother-brain-body-presentation")]
    internal static int MotherBrainBodyPresentationCommand()
    {
        Suite(nameof(VerifyMotherBrainBodyPresentation), () => VerifyMotherBrainBodyPresentation());
        return 0;
    }

    /// <summary>Runs the stock import and override checks through the verification dispatcher.</summary>
    [VerificationCommand("--mother-brain-body-stock-presentation")]
    internal static int MotherBrainBodyStockPresentationCommand()
    {
        Suite(nameof(VerifyMotherBrainBodyStockPresentation), () => VerifyMotherBrainBodyStockPresentation());
        return 0;
    }
}
