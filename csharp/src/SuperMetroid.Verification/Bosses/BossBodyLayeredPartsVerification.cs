using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Bosses;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Bosses;

/// <summary>
/// Confirms a boss body's parts, layered in draw order (#1540), draw what the frames they show composed on the boss's layer
/// before: each pixel the colour key of the frame whose cells last reach it, a later frame's over an earlier's, nothing where that
/// frame paints nothing and nothing below the row the body shows above. Crocomire's melt replaces his body frame and each
/// dissolve step replaces the last; the acid cuts his sunk body; Kraid's head pose shows alone on the rows the head his body
/// rises with fills before it; Phantoon's eye shows in his body's socket. Each pixel shows at the depth the cartridge's word
/// for its cell draws it at (#1634): in front of the sprites where the word is in its layer's upper tier, as Kraid's growth
/// writes every word of his body and the head it rises with, and behind them elsewhere. A part showing a frame that paints
/// nothing is not drawn and hides nothing beneath it.
/// </summary>
internal static class BossBodyLayeredPartsVerification
{
    /// <summary>Pixels across and down one cell of a boss's layer.</summary>
    private const int CellPixels = 8;

    /// <summary>Checks every case.</summary>
    internal static void VerifyBossBodyLayeredParts()
    {
        BossSourceArtwork frames = InstalledFixtures.BossArtwork;
        BossBodySheets sheets = InstalledFixtures.BossBodySheets;
        int bodies = CheckCrocomireMelt(frames, sheets) + CheckKraidHead(frames, sheets) + CheckPhantoonEye(frames, sheets);
        CheckPartShowingNothing(sheets.Of(BossBodyOwner.Crocomire));
        Console.WriteLine($"  Boss body parts: {bodies} bodies layer their parts as their frames composed them; a part showing nothing hides nothing.");
    }

    /// <summary>
    /// Crocomire's body frame, then each melt image and every state its dissolve leaves it in, through his melt's production
    /// steps, and then his body cut lower and lower as the acid hides it. Returns the bodies compared.
    /// </summary>
    private static int CheckCrocomireMelt(BossSourceArtwork frames, BossBodySheets sheets)
    {
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        enemies.Presentation.BindBackgrounds(BgArtworkFixtures.For(new SnesVram()));
        var state = new CrocomireEnemyState(enemies.SlotTable.Slots[0]);
        _ = BossBodyCanvasFixtures.AttachCrocomire(enemies, sheets);
        Dictionary<string, BossBodyFrameSource> sources = SourcesOf(frames, BossBodyOwner.Crocomire);
        Extent16<BackgroundPixels> area = SideBySideCanvas;
        int compared = 0;
        void Compare(string when)
        {
            AssertLayered(enemies.Presentation.RequireBossBody().Publish(), sources, area, $"Crocomire {when}");
            compared++;
        }

        enemies.CrocomireBehavior.BodyLayer.ShowCrocomireBodyFrame(sheets.Of(BossBodyOwner.Crocomire).Frame(EnemyFrameId.CrocomireBodyPowerBombReactionMouthNotOpenLoopFirst));
        Compare("showing his body");
        CrocomireMelting melting = enemies.CrocomireBehavior.Melting;
        foreach (CrocomireMeltingIllustration pass in Enum.GetValues<CrocomireMeltingIllustration>())
        {
            if (pass == CrocomireMeltingIllustration.First)
                melting.ShowFirstCrocomireMeltImage(state);
            else
                melting.ShowSecondCrocomireMeltImage(state);
            AssertEqual(1, enemies.Presentation.RequireBossBody().Publish().Frames.Length, $"Crocomire's {pass} melt image replaces what his body showed");
            Compare($"showing his {pass} melt image");
            melting.BeginCrocomireMeltPass(state, pass);
            for (int erased = 1; erased <= CrocomireMeltingDefinitions.ColumnCount; erased++)
            {
                melting.EraseNextCrocomireMeltingColumn();
                Compare($"after his {pass} melt's dissolve erases {erased} columns");
            }
        }
        foreach (int row in new[] { 64, 40, 8 })
        {
            enemies.CrocomireBehavior.BodyLayer.HideCrocomireBodyFrom(row);
            AssertEqual<int?>(row, enemies.Presentation.RequireBossBody().Publish().ShownAbove, $"the acid hides Crocomire's body from row {row}");
            Compare($"hidden from row {row}");
        }
        return compared;
    }

    /// <summary>
    /// Kraid's body in each appearance, with the head it rises with and then with each head pose, as his body layer draws
    /// it: his growth writes every word of his body and the head it rises with in the upper tier, which draws them in front of
    /// the sprites (#1634), and leaves each head pose's words as they are. Returns the bodies compared.
    /// </summary>
    private static int CheckKraidHead(BossSourceArtwork frames, BossBodySheets sheets)
    {
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        enemies.BossArtworkBinding.BodySheets = sheets;
        var state = new KraidEnemyState();
        state.InitializeBodyLayer();
        state.ShowBody();
        enemies.KraidBehavior.Access.Kraid = state;
        Dictionary<string, BossBodyFrameSource> sources = SourcesOf(frames, BossBodyOwner.Kraid);
        int compared = 0;
        foreach (KraidBodyAppearance appearance in new[] { KraidBodyAppearance.BehindSprites, KraidBodyAppearance.InFrontOfSprites })
        {
            state.BodyAppearance = appearance;
            foreach (KraidHeadFrame? head in new KraidHeadFrame?[] { null }.Concat(KraidHeadTilemapFormat.HeadsByMouthStage.Select(pose => (KraidHeadFrame?)pose)))
            {
                state.DisplayedHeadFrame = head;
                BossBodyLayer layer = enemies.KraidBehavior.BodyLayer.CaptureBodyLayer()
                    ?? throw new InvalidOperationException("Kraid showing his body layer draws his body.");
                AssertEqual(2, layer.Frames.Length, $"Kraid's {appearance} body shows his body and one head");
                AssertLayered(layer, sources, KraidCanvas, $"Kraid's {appearance} body with head {head?.MouthStage.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "risen with it"}",
                    appearance == KraidBodyAppearance.InFrontOfSprites ? [KraidBodyFrameNames.Body, KraidBodyFrameNames.RestingHead] : []);
                compared++;
            }
        }
        return compared;
    }

    /// <summary>Each of Phantoon's bodies with each of his eye's frames in its socket and his tentacles and mouth beneath. Returns the bodies compared.</summary>
    private static int CheckPhantoonEye(BossSourceArtwork frames, BossBodySheets sheets)
    {
        BossBodySheet sheet = sheets.Of(BossBodyOwner.Phantoon);
        Dictionary<string, BossBodyFrameSource> sources = SourcesOf(frames, BossBodyOwner.Phantoon);
        EnemyFrameId[] eyes =
        [
            EnemyFrameId.PhantoonEyeClosed, EnemyFrameId.PhantoonEyeballCentered, EnemyFrameId.PhantoonEyeOpenFirst, EnemyFrameId.PhantoonEyeOpenSecond,
            EnemyFrameId.PhantoonEyeOpenThird, EnemyFrameId.PhantoonEyeLookingUp, EnemyFrameId.PhantoonEyeLookingUpRight, EnemyFrameId.PhantoonEyeLookingRight,
            EnemyFrameId.PhantoonEyeLookingDownRight, EnemyFrameId.PhantoonEyeLookingDown, EnemyFrameId.PhantoonEyeLookingDownLeft, EnemyFrameId.PhantoonEyeLookingLeft, EnemyFrameId.PhantoonEyeLookingUpLeft,
        ];
        int compared = 0;
        foreach (EnemyFrameId body in new[] { EnemyFrameId.PhantoonInvulnerableBody, EnemyFrameId.PhantoonFullHitboxBody, EnemyFrameId.PhantoonEyeHitboxBody })
        {
            foreach (EnemyFrameId eye in eyes)
            {
                var parts = new BossBodyParts(BossBodyOwner.Phantoon);
                parts.Show(EnemyDefinitionId.PhantoonBody, sheet.Frame(body));
                parts.Show(EnemyDefinitionId.PhantoonEye, sheet.Frame(eye));
                parts.Show(EnemyDefinitionId.PhantoonTentacles, sheet.Frame(EnemyFrameId.PhantoonTentaclesFirst));
                parts.Show(EnemyDefinitionId.PhantoonMouth, sheet.Frame(EnemyFrameId.PhantoonMouthInitial));
                AssertLayered(parts.Publish(), sources, SideBySideCanvas, $"Phantoon's {body} with his {eye}");
                compared++;
            }
        }
        return compared;
    }

    /// <summary>A part showing a frame that paints nothing is not drawn: the part beneath it shows whole.</summary>
    private static void CheckPartShowingNothing(BossBodySheet sheet)
    {
        BossBodyFrame body = sheet.Frame(EnemyFrameId.CrocomireBodyPowerBombReactionMouthNotOpenLoopFirst);
        var nothing = new BossBodyFrame("Nothing", picture: null, body.Corner, depth: null);
        ShownBossBody shown = new BossBodyLayer([body, nothing]).Show();
        SpriteSheetFrame picture = body.Picture ?? throw new InvalidOperationException("Crocomire's body frame has a picture.");
        BossBodyDepth depth = body.Depth ?? throw new InvalidOperationException("Crocomire's body frame has a depth.");
        AssertTrue(shown.Parts.Length == 1 && shown.Parts[0] == new ShownBossBodyPart(picture, body.Corner, new Extent16<BackgroundPixels>(picture.Size.X, picture.Size.Y), depth),
            "a part showing nothing is not drawn, and the body beneath it shows whole at its depth");
    }

    /// <summary>The canvas the checks compose a body on whose room shows two screens of it side by side.</summary>
    private static readonly Extent16<BackgroundPixels> SideBySideCanvas = new(512, 256);

    /// <summary>The canvas the checks compose Kraid's body on: two screens across and two down.</summary>
    private static readonly Extent16<BackgroundPixels> KraidCanvas = new(512, 512);

    /// <summary>
    /// Fails unless <paramref name="layer"/>'s parts, layered on <paramref name="area"/>, show at every pixel what its frames
    /// composed there, at the depth the word composing it draws it at; the frames named in <paramref name="raised"/> are
    /// written with every word in the upper tier, as Kraid's growth writes his body.
    /// </summary>
    private static void AssertLayered(BossBodyLayer layer, Dictionary<string, BossBodyFrameSource> sources, Extent16<BackgroundPixels> area, string context,
        string[]? raised = null)
    {
        ShownPixel?[] composed = Composed(layer, sources, area, raised ?? []);
        ShownPixel?[] layered = Layered(layer.Show(), area);
        int differing = 0, first = -1;
        for (int pixel = 0; pixel < composed.Length; pixel++)
        {
            if (composed[pixel] != layered[pixel])
            {
                differing++;
                if (first < 0)
                    first = pixel;
            }
        }
        AssertTrue(differing == 0,
            $"{context}: the layered parts differ from the composed frames at {differing} pixels, first at ({first % area.X}, {first / area.X})");
    }

    /// <summary>
    /// What <paramref name="layer"/>'s frames compose on <paramref name="area"/>: each pixel shows the colour key of the frame whose
    /// cells last reach it, a later frame's over an earlier's, at the depth its cell's word draws it at, in front of the
    /// sprites where the word is in the upper tier or the frame is one of <paramref name="raised"/>; nothing where that frame
    /// paints nothing, at or below the row the body shows above, or where no frame reaches.
    /// </summary>
    private static ShownPixel?[] Composed(BossBodyLayer layer, Dictionary<string, BossBodyFrameSource> sources, Extent16<BackgroundPixels> area,
        string[] raised)
    {
        var owners = new BossBodyFrame?[area.X * area.Y];
        var depths = new BossBodyDepth[owners.Length];
        foreach (BossBodyFrame frame in layer.Frames)
        {
            BossBodyFrameSource source = sources.TryGetValue(frame.Name, out BossBodyFrameSource? found) ? found
                : throw new InvalidOperationException($"No installed frame composes {frame.Name}.");
            bool inFront = raised.Contains(frame.Name, StringComparer.Ordinal);
            foreach ((int cell, BackgroundTile tile) in source.Cells)
            {
                Position16<BackgroundPixels> at = BossBodyPainter.PixelOf(cell);
                BossBodyDepth depth = inFront || tile.HighPriority ? BossBodyDepth.InFrontOfSprites : BossBodyDepth.BehindSprites;
                for (int y = at.Y; y < at.Y + CellPixels && y < area.Y; y++)
                for (int x = at.X; x < at.X + CellPixels && x < area.X; x++)
                {
                    owners[y * area.X + x] = frame;
                    depths[y * area.X + x] = depth;
                }
            }
        }
        var shown = new ShownPixel?[owners.Length];
        for (int pixel = 0; pixel < owners.Length; pixel++)
        {
            int x = pixel % area.X, y = pixel / area.X;
            if (owners[pixel] is { Picture: { } picture } frame && (layer.ShownAbove is not { } shownAbove || y < shownAbove)
                && KeyAt(picture, x - frame.Corner.X, y - frame.Corner.Y) is { } key)
                shown[pixel] = new ShownPixel(key, depths[pixel]);
        }
        return shown;
    }

    /// <summary>
    /// What <paramref name="shown"/>'s parts show on <paramref name="area"/>: each pixel the colour key of the frontmost part painting
    /// it, at that part's depth.
    /// </summary>
    private static ShownPixel?[] Layered(ShownBossBody shown, Extent16<BackgroundPixels> area)
    {
        var pixels = new ShownPixel?[area.X * area.Y];
        foreach (ShownBossBodyPart part in shown.Parts)
        {
            for (int y = 0; y < part.Size.Y; y++)
            for (int x = 0; x < part.Size.X; x++)
            {
                if (KeyAt(part.Picture, x, y) is { } key)
                {
                    // A part reaching past the canvas shows nothing there to compare.
                    if (part.Corner.X + x < area.X && part.Corner.Y + y < area.Y)
                        pixels[(part.Corner.Y + y) * area.X + part.Corner.X + x] = new ShownPixel(key, part.Depth);
                }
            }
        }
        return pixels;
    }

    /// <summary>A pixel a body shows: its colour key and the depth it is drawn at (#1634, #1633).</summary>
    /// <param name="Key">The pixel's colour with its recolour region.</param>
    /// <param name="Depth">The depth it is drawn at.</param>
    private readonly record struct ShownPixel(ColourKey Key, BossBodyDepth Depth);

    /// <summary>The colour key of <paramref name="picture"/>'s pixel at <paramref name="x"/>, <paramref name="y"/>, or null where it is clear.</summary>
    private static ColourKey? KeyAt(SpriteSheetFrame picture, int x, int y)
    {
        if (x < 0 || y < 0 || x >= picture.Size.X || y >= picture.Size.Y)
            throw new InvalidOperationException($"Pixel ({x}, {y}) lies off a {picture.Size.X}x{picture.Size.Y} picture.");
        SpriteImage image = picture.Sheet;
        int pixel = (picture.Corner.Y + y) * image.Size.X + picture.Corner.X + x;
        return image.Pixels[pixel].A == 0 ? null : image.KeyAt(pixel);
    }

    /// <summary>The frames <paramref name="frames"/> compose <paramref name="owner"/>'s sheet from, by name.</summary>
    private static Dictionary<string, BossBodyFrameSource> SourcesOf(BossSourceArtwork frames, BossBodyOwner owner) =>
        BossBodySheetComposer.Frames(frames, owner).ToDictionary(frame => frame.Name, StringComparer.Ordinal);

    /// <summary>Runs the boss body layered-parts check.</summary>
    [VerificationCommand("--boss-body-layered-parts")]
    internal static int BossBodyLayeredPartsCommand()
    {
        Suite(nameof(VerifyBossBodyLayeredParts), VerifyBossBodyLayeredParts);
        return 0;
    }
}
