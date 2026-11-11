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
/// before: each pixel the ink of the frame whose cells last reach it, a later frame's over an earlier's, nothing where that
/// frame paints nothing and nothing below the row the body shows above. Crocomire's melt replaces his body frame and each
/// dissolve step replaces the last; the acid cuts his sunk body; Kraid's head pose shows alone on the rows the head his body
/// rises with fills before it; Phantoon's eye shows in his body's socket. A part showing a frame that paints nothing is not
/// drawn and hides nothing beneath it.
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

        enemies.CrocomireBehavior.BodyLayer.ShowCrocomireBodyFrame(sheets.Of(BossBodyOwner.Crocomire).Frame(EnemyFrameId.CrocomireBody0));
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
    /// it. Returns the bodies compared.
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
                AssertLayered(layer, sources, KraidCanvas, $"Kraid's {appearance} body with head {head?.MouthStage.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "risen with it"}");
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
            EnemyFrameId.PhantoonClosedEye0, EnemyFrameId.PhantoonCenteredEye0, EnemyFrameId.PhantoonOpeningEye0, EnemyFrameId.PhantoonOpeningEye1,
            EnemyFrameId.PhantoonOpeningEye2, EnemyFrameId.PhantoonGaze0, EnemyFrameId.PhantoonGaze1, EnemyFrameId.PhantoonGaze2,
            EnemyFrameId.PhantoonGaze3, EnemyFrameId.PhantoonGaze4, EnemyFrameId.PhantoonGaze5, EnemyFrameId.PhantoonGaze6, EnemyFrameId.PhantoonGaze7,
        ];
        int compared = 0;
        foreach (EnemyFrameId body in new[] { EnemyFrameId.PhantoonInvulnerableBody0, EnemyFrameId.PhantoonFullHitboxBody0, EnemyFrameId.PhantoonEyeHitboxBody0 })
        {
            foreach (EnemyFrameId eye in eyes)
            {
                var parts = new BossBodyParts(BossBodyOwner.Phantoon);
                parts.Show(EnemyDefinitionId.PhantoonBody, sheet.Frame(body));
                parts.Show(EnemyDefinitionId.PhantoonEye, sheet.Frame(eye));
                parts.Show(EnemyDefinitionId.PhantoonTentacles, sheet.Frame(EnemyFrameId.PhantoonTentacle0));
                parts.Show(EnemyDefinitionId.PhantoonMouth, sheet.Frame(EnemyFrameId.PhantoonMouth0));
                AssertLayered(parts.Publish(), sources, SideBySideCanvas, $"Phantoon's {body} with his {eye}");
                compared++;
            }
        }
        return compared;
    }

    /// <summary>A part showing a frame that paints nothing is not drawn: the part beneath it shows whole.</summary>
    private static void CheckPartShowingNothing(BossBodySheet sheet)
    {
        BossBodyFrame body = sheet.Frame(EnemyFrameId.CrocomireBody0);
        var nothing = new BossBodyFrame("Nothing", picture: null, body.Corner);
        ShownBossBody shown = new BossBodyLayer([body, nothing]).Show();
        SpriteSheetFrame picture = body.Picture ?? throw new InvalidOperationException("Crocomire's body frame has a picture.");
        AssertTrue(shown.Parts.Length == 1 && shown.Parts[0] == new ShownBossBodyPart(picture, body.Corner, new Extent16<BackgroundPixels>(picture.Size.X, picture.Size.Y)),
            "a part showing nothing is not drawn, and the body beneath it shows whole");
    }

    /// <summary>The canvas the checks compose a body on whose room shows two screens of it side by side.</summary>
    private static readonly Extent16<BackgroundPixels> SideBySideCanvas = new(512, 256);

    /// <summary>The canvas the checks compose Kraid's body on: two screens across and two down.</summary>
    private static readonly Extent16<BackgroundPixels> KraidCanvas = new(512, 512);

    /// <summary>
    /// Fails unless <paramref name="layer"/>'s parts, layered on <paramref name="area"/>, show at every pixel what its frames
    /// composed there.
    /// </summary>
    private static void AssertLayered(BossBodyLayer layer, Dictionary<string, BossBodyFrameSource> sources, Extent16<BackgroundPixels> area, string context)
    {
        SpriteImageInk?[] composed = Composed(layer, sources, area);
        SpriteImageInk?[] layered = Layered(layer.Show(), area);
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
    /// What <paramref name="layer"/>'s frames compose on <paramref name="area"/>: each pixel shows the ink of the frame whose
    /// cells last reach it, a later frame's over an earlier's; nothing where that frame paints nothing, at or below the row
    /// the body shows above, or where no frame reaches.
    /// </summary>
    private static SpriteImageInk?[] Composed(BossBodyLayer layer, Dictionary<string, BossBodyFrameSource> sources, Extent16<BackgroundPixels> area)
    {
        var owners = new BossBodyFrame?[area.X * area.Y];
        foreach (BossBodyFrame frame in layer.Frames)
        {
            BossBodyFrameSource source = sources.TryGetValue(frame.Name, out BossBodyFrameSource? found) ? found
                : throw new InvalidOperationException($"No installed frame composes {frame.Name}.");
            foreach (int cell in source.Cells.Keys)
            {
                Position16<BackgroundPixels> at = BossBodyPainter.PixelOf(cell);
                for (int y = at.Y; y < at.Y + CellPixels && y < area.Y; y++)
                for (int x = at.X; x < at.X + CellPixels && x < area.X; x++)
                    owners[y * area.X + x] = frame;
            }
        }
        var inks = new SpriteImageInk?[owners.Length];
        for (int pixel = 0; pixel < owners.Length; pixel++)
        {
            int x = pixel % area.X, y = pixel / area.X;
            if (owners[pixel] is { Picture: { } picture } frame && (layer.ShownAbove is not { } shownAbove || y < shownAbove))
                inks[pixel] = InkAt(picture, x - frame.Corner.X, y - frame.Corner.Y);
        }
        return inks;
    }

    /// <summary>What <paramref name="shown"/>'s parts show on <paramref name="area"/>: each pixel the ink of the frontmost part inking it.</summary>
    private static SpriteImageInk?[] Layered(ShownBossBody shown, Extent16<BackgroundPixels> area)
    {
        var inks = new SpriteImageInk?[area.X * area.Y];
        foreach (ShownBossBodyPart part in shown.Parts)
        {
            for (int y = 0; y < part.Size.Y; y++)
            for (int x = 0; x < part.Size.X; x++)
            {
                if (InkAt(part.Picture, x, y) is { } ink)
                {
                    // A part reaching past the canvas shows nothing there to compare.
                    if (part.Corner.X + x < area.X && part.Corner.Y + y < area.Y)
                        inks[(part.Corner.Y + y) * area.X + part.Corner.X + x] = ink;
                }
            }
        }
        return inks;
    }

    /// <summary>The ink of <paramref name="picture"/>'s pixel at <paramref name="x"/>, <paramref name="y"/>, or null where it shows nothing.</summary>
    private static SpriteImageInk? InkAt(SpriteSheetFrame picture, int x, int y)
    {
        if (x < 0 || y < 0 || x >= picture.Size.X || y >= picture.Size.Y)
            throw new InvalidOperationException($"Pixel ({x}, {y}) lies off a {picture.Size.X}x{picture.Size.Y} picture.");
        SpriteImage image = picture.Sheet;
        SpriteImageInk ink = image.Inks[(picture.Corner.Y + y) * image.Size.X + picture.Corner.X + x];
        return ink.Clear ? null : ink;
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
