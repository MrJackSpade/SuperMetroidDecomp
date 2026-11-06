using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.AssetExtraction.Enemies.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.TestSupport.VerificationOracles;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Enemies.Corpses;

/// <summary>
/// Confirms every rotting corpse's frame draws, at every rot call, as the game's character rot leaves it (#1531): the
/// installed sheet's one picture of the frame, drawn through the rows Core's rot leaves and where the sheet says the
/// corpse lies in it, paints at every screen pixel the colour the frame's parts paint over the sprite characters the
/// character rot oracle leaves (<see cref="CorpseCharacterRot"/>). It covers both dead Sidehoppers, every dead Zoomer, Ripper
/// and Skree, the dead Torizo's body and its hook's sand, each half as its main-AI calls last uploaded it, and Mother
/// Brain's corpse, whose last rot call is never shown, and which keeps its whole picture through her escape's pages.
/// </summary>
internal static class CorpseRotDrawingVerification
{
    /// <summary>The screen point every compared draw's origin lands on.</summary>
    private static readonly Position16<ScreenPixels> Origin = new(128, 112);

    /// <summary>Draws every corpse frame at every rot call and compares it with the character rot oracle.</summary>
    internal static void VerifyCorpseRotDrawing()
    {
        InstalledEnemySheets sheets = InstalledFixtures.EnemySheets;
        EnemyTileSourceArtwork enemies = InstalledFixtures.EnemyTiles;
        int compared = 0;
        foreach (int graphicsVariant in new[] { 0, 2 })
        {
            DeadSidehopperCorpseDefinition definition = graphicsVariant == 0
                ? DeadSidehopperCorpseDefinitions.InitiallyAlive : DeadSidehopperCorpseDefinitions.InitiallyDead;
            compared += VerifyDeadMonster(sheets, EnemyDefinitionId.CorpseSidehopper, definition.RowCount,
                CorpseCharacterRot.Sidehopper(enemies, graphicsVariant));
        }
        foreach (EnemyDefinitionId enemy in new[] { EnemyDefinitionId.CorpseZoomer, EnemyDefinitionId.CorpseRipper, EnemyDefinitionId.CorpseSkree })
        {
            DeadTourianBehavior.DeadTourianCorpseProfile profile = DeadTourianBehavior.ProfileForDeadTourianCorpse(enemy);
            for (int variant = 0; variant < profile.VariantCount; variant++)
            {
                compared += VerifyDeadMonster(sheets, enemy, DeadTourianCorpses.For(profile.Species, variant).RowCount,
                    CorpseCharacterRot.Tourian(enemies, profile.Species, variant));
            }
        }
        compared += VerifyDeadTorizo(sheets, CorpseCharacterRot.Torizo(enemies));
        compared += VerifyMotherBrain(sheets, CorpseCharacterRot.MotherBrain(InstalledFixtures.BossArtwork));
        Console.WriteLine($"  Corpse rot drawing: {compared} draws of every corpse frame at every rot call paint as the game's character rot leaves them.");
    }

    /// <summary>
    /// Rots one dead monster's rows as Core does, its rows uploaded after every rot call, and compares each of its frames
    /// that reads them at every call.
    /// </summary>
    private static int VerifyDeadMonster(InstalledEnemySheets sheets, EnemyDefinitionId enemy, int rowCount, CorpseCharacterRot oracle)
    {
        RoomEnemyGraphicsSetDefinition set = SetDrawing(sheets, enemy);
        byte[] loaded = Loaded(set, []);
        EnemyFrameId[] frames = [.. CorpseFrames(sheets, set, enemy).Where(frame => Reads(set, enemy, frame, oracle, loaded))];
        AssertTrue(frames.Length > 0, $"{oracle.Name}'s rows show in a frame of {enemy}");
        var rows = new RottingCorpseRows(rowCount);
        var schedule = new CorpseRotSchedule(rowCount);
        int compared = 0;
        for (int call = 0; call <= oracle.CallCount; call++)
        {
            if (call > 0)
            {
                bool rotting = schedule.Step(rows.MoveRow, _ => { });
                AssertEqual(call < oracle.CallCount, rotting, $"{oracle.Name} rots for as many calls as the oracle's");
            }
            byte[] characters = oracle.Characters(loaded, call);
            foreach (EnemyFrameId frame in frames)
                compared += AssertDrawsAsRotted(sheets.For(set, enemy), sheets.CorpseLayout(set, enemy, EnemySheetDefinitions.FrameName(frame)),
                    EnemySheetDefinitions.FrameName(frame), rows.Shown, characters, frame, TileBase(set, enemy), $"{oracle.Name} {frame} after rot call {call}");
        }
        return compared;
    }

    /// <summary>
    /// Rots the dead Torizo as Core does, showing one half each main-AI call, and compares its body frame and its hook at
    /// each call, from its intact calls to the calls after its rot has finished.
    /// </summary>
    private static int VerifyDeadTorizo(InstalledEnemySheets sheets, CorpseCharacterRot oracle)
    {
        RoomEnemyGraphicsSetDefinition set = SetDrawing(sheets, EnemyDefinitionId.CorpseTorizo);
        byte[] loaded = Loaded(set, []);
        var enemies = new RoomEnemySystem(new EnemySession(new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()));
        var state = new DeadTorizoEnemyState(enemies.SlotTable.Slots[0], new CorpseRotSchedule(DeadTorizoCorpseDefinitions.RowCount));
        string body = EnemySheetDefinitions.FrameName(EnemyFrameId.DeadTorizoStationary);
        string hook = EnemySheetDefinitions.EncounterFrameName(DeadTorizoArtworkDefinitions.HookFrame);
        SpriteSheet bodySheet = sheets.For(set, EnemyDefinitionId.CorpseTorizo);
        SpriteSheet hookSheet = sheets.Spawned(set, SpawnedObjectOwner.Shared).Sheet;
        int tileBase = TileBase(set, EnemyDefinitionId.CorpseTorizo);
        int compared = 0;
        int update = 0;

        // Each main-AI call shows the next half: two while the corpse is intact, one after each rot call, and two once its
        // rot has finished.
        void ShowNextHalf()
        {
            DeadTorizoBehavior.ShowNextRotHalf(state);
            byte[] characters = oracle.Characters(loaded, state.EvenHalfStep, state.OddHalfStep);
            string context = $"dead Torizo after main-AI call {update++}";
            compared += AssertDrawsAsRotted(bodySheet, sheets.CorpseLayout(set, EnemyDefinitionId.CorpseTorizo, body), body,
                DeadTorizoBehavior.ShownBody(state), characters, EnemyFrameId.DeadTorizoStationary, tileBase, context);
            compared += AssertDrawsAsRotted(hookSheet, sheets.SpawnedCorpseLayout(set, SpawnedObjectOwner.Shared, hook), hook,
                DeadTorizoBehavior.ShownSand(state), characters, DeadTorizoArtworkDefinitions.HookFrame, 0, $"{context}'s hook");
        }

        ShowNextHalf();
        ShowNextHalf();
        bool rotting = true;
        while (rotting)
        {
            state.ProcessCallCount++;
            rotting = state.Rot.Step((row, operation) => state.Rows.MoveRow(row, operation), _ => { });
            ShowNextHalf();
        }
        ShowNextHalf();
        ShowNextHalf();
        AssertEqual(oracle.CallCount, checked((int)state.ProcessCallCount), "the dead Torizo rots for as many calls as the oracle's");
        return compared;
    }

    /// <summary>
    /// Rots Mother Brain's corpse as Core does, at her last corpse page, and compares her corpse frame at every call; then
    /// requires every later page step it is drawn at to show the same whole corpse picture (#1513).
    /// </summary>
    private static int VerifyMotherBrain(InstalledEnemySheets sheets, CorpseCharacterRot oracle)
    {
        RoomEnemyGraphicsSetDefinition set = SetDrawing(sheets, EnemyDefinitionId.MotherBrainHead);
        int lastCorpsePage = MotherBrainSpritePageDefinitions.Sequence.ToList().FindLastIndex(page => page.Sheet == MotherBrainSpritePageSheet.Corpse);
        AssertTrue(lastCorpsePage >= 0, "Mother Brain's pages load her corpse");
        string name = EnemySheetDefinitions.FrameName(EnemyFrameId.MotherBrainHead12);
        FrameArtworkVariation pages = sheets.Variations(set, EnemyDefinitionId.MotherBrainHead, name)
            .Single(variation => variation.Sequence == EncounterArtworkSequence.MotherBrainPages);
        SpriteSheet sheet = sheets.For(set, EnemyDefinitionId.MotherBrainHead);
        int tileBase = TileBase(set, EnemyDefinitionId.MotherBrainHead);
        byte[] loaded = Loaded(set, [new EncounterArtworkStep(EncounterArtworkSequence.MotherBrainPages, lastCorpsePage)]);
        var rotting = new MotherBrainCorpseRottingState();
        int compared = 0;
        for (int call = 0; call <= oracle.CallCount; call++)
        {
            if (call > 0)
                _ = rotting.Step(new Position16<RoomPixels>(128, 112), MotherBrainRandomSample.FromGenerator(unchecked((ushort)(call * 37))), call);
            string corpsePicture = PictureAt(name, pages, lastCorpsePage);
            compared += AssertDrawsAsRotted(sheet, sheets.CorpseLayout(set, EnemyDefinitionId.MotherBrainHead, corpsePicture), corpsePicture, rotting.Shown, oracle.Characters(loaded, call),
                EnemyFrameId.MotherBrainHead12, tileBase, $"Mother Brain's corpse after rot call {call}");
        }
        // Her escape's pages load over her corpse's characters once it has rotted; the corpse frame keeps showing her whole
        // corpse's picture through its rows, never the escape's pages (#1513).
        string corpse = PictureAt(name, pages, lastCorpsePage);
        for (int step = lastCorpsePage + 1; step < MotherBrainSpritePageDefinitions.Sequence.Count; step++)
        {
            if (pages.DrawnAt(step))
                AssertEqual(corpse, PictureAt(name, pages, step), $"Mother Brain's rotted corpse after page step {step} shows her whole corpse's picture");
        }
        return compared;
    }

    /// <summary>
    /// Checks picture <paramref name="picture"/> of <paramref name="sheet"/>, drawn through <paramref name="rows"/> as the
    /// sheet's <paramref name="layout"/> says, paints the colour <paramref name="frame"/>'s parts paint over
    /// <paramref name="characters"/> at every screen pixel; before the corpse's rows rot it draws as itself.
    /// </summary>
    private static int AssertDrawsAsRotted(SpriteSheet sheet, CorpsePictureLayout? layout, string picture, CorpseRotRows? rows, byte[] characters,
        EnemyFrameId frame, int tileBase, string context)
    {
        AssertTrue(layout is not null, $"{context}: the sheet says where the corpse lies in {picture}");
        SpriteSheetFrame image = sheet.Frame(picture);
        var draw = new ImageSpriteDraw(new SpriteSource(SpriteSourceKind.Enemy, 0), image, Origin, SpriteMirror.None)
        {
            Shading = PictureShading.Standard,
            CorpseRot = layout is not null && rows is not null ? CorpseRotDrawing.Picture(layout, rows, image.Size, picture) : null,
        };
        CharacterSpriteDraw parts = EnemyPartComposition.Spritemap(ObjCharacterPage.Create(new SpriteArtworkId(SpriteArtworkKind.GameplayObjects), characters),
            draw.Source, InstalledFixtures.EnemyTiles.Spritemaps ?? throw new InvalidOperationException("The installation holds the enemy compositions."),
            new EnemySpritemapDraw { Frame = frame, Origin = new(Origin.X, Origin.Y), PaletteBits = 0, BaseTileIndex = tileBase })
            ?? throw new InvalidOperationException($"{context}: {frame} has parts.");
        var expected = new Dictionary<ScreenPixel, int>();
        for (int part = 0; part < parts.Frame.Parts.Length; part++)
        {
            SpriteScreenPart placed = parts.Place(part);
            for (ushort y = 0; y < placed.Size.Y; y++)
            for (ushort x = 0; x < placed.Size.X; x++)
            {
                int character = placed.CharacterAt(new(x, y), out Position16<CharacterPixels> within);
                int colour = parts.Artwork.ColorIndex(character, within);
                if (colour != 0)
                    expected.TryAdd(new ScreenPixel(placed.Position.X + x, placed.Position.Y + y), colour);
            }
        }
        var actual = new Dictionary<ScreenPixel, int>();
        for (ushort y = 0; y < image.Size.Y; y++)
        for (ushort x = 0; x < image.Size.X; x++)
        {
            int entry = draw.PaletteIndexAt(new Position16<ScreenPixels>(x, y));
            if (entry != 0)
                actual.TryAdd(new ScreenPixel(draw.Corner.X + x, draw.Corner.Y + y), entry % 16);
        }
        foreach (ScreenPixel pixel in expected.Keys.Union(actual.Keys))
        {
            int? want = expected.TryGetValue(pixel, out int e) ? e : null, got = actual.TryGetValue(pixel, out int g) ? g : null;
            if (want != got)
                throw new InvalidOperationException($"{context}: {picture} paints colour {got?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "none"} at screen pixel ({pixel.X},{pixel.Y}), where the character rot leaves {want?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "none"}.");
        }
        if (draw.CorpseRot is { } rot)
        {
            rottedComparisons[picture] = rottedComparisons.GetValueOrDefault(picture) + 1;
            if (rottedComparisons[picture] == ShaderSampleComparison)
                AssertShaderDrawsAsMapped(draw, rot, context);
        }
        return 1;
    }

    /// <summary>Which rotted comparison of each picture the renderer also draws: one part-way through its rot.</summary>
    private const int ShaderSampleComparison = 8;

    /// <summary>How many rotted comparisons each picture has had, so each is drawn through the renderer once.</summary>
    private static readonly Dictionary<string, int> rottedComparisons = new(StringComparer.Ordinal);

    /// <summary>
    /// Checks the renderer draws <paramref name="draw"/> through its column tables (#1531): every screen pixel of the rotted
    /// draw shows the pixel the unrotted picture shows at the row its column's table maps that row to, and the backdrop
    /// where the table clears the row. The unrotted picture is drawn once as the reference, so the comparison does not
    /// depend on how the renderer converts colours.
    /// </summary>
    private static void AssertShaderDrawsAsMapped(ImageSpriteDraw draw, CorpseRotPicture rot, string context)
    {
        RgbaPixel[] plain = SuperMetroid.TestSupport.FrameRendering.DisplayRendering.Render(
            new LayeredScene(new FramePaint(RgbColor.Black, [draw with { CorpseRot = null }]), [], FullBrightness));
        RgbaPixel[] rotted = SuperMetroid.TestSupport.FrameRendering.DisplayRendering.Render(
            new LayeredScene(new FramePaint(RgbColor.Black, [draw]), [], FullBrightness));
        RgbaPixel backdrop = plain[0];
        Extent16<ImagePixels> size = draw.Image.Size;
        for (int y = 0; y < size.Y; y++)
        {
            for (int x = 0; x < size.X; x++)
            {
                int screenX = draw.Corner.X + x, screenY = draw.Corner.Y + y;
                if ((uint)screenX >= DisplayScreen.Width || (uint)screenY >= DisplayScreen.Height)
                    continue;
                int? source = rot.Shown(rot.TableOf(x / CorpseRotPicture.ColumnPixels), y);
                RgbaPixel want = source is int row && draw.Corner.Y + row is >= 0 and < DisplayScreen.Height
                    ? plain[(draw.Corner.Y + row) * DisplayScreen.Width + screenX]
                    : backdrop;
                RgbaPixel got = rotted[screenY * DisplayScreen.Width + screenX];
                if (got != want)
                    throw new InvalidOperationException($"{context}: the renderer draws {got} at screen pixel ({screenX},{screenY}), where the column table shows {want}.");
            }
        }
    }

    /// <summary>The brightness the renderer comparison draws at: full, so colours are unchanged.</summary>
    private const int FullBrightness = 15;

    /// <summary>Whether <paramref name="frame"/> of <paramref name="enemy"/> reads characters the oracle's rot changes at some call.</summary>
    private static bool Reads(RoomEnemyGraphicsSetDefinition set, EnemyDefinitionId enemy, EnemyFrameId frame, CorpseCharacterRot oracle, byte[] loaded)
    {
        byte[] last = oracle.Characters(loaded, oracle.CallCount);
        EnemySpritemapCatalog spritemaps = InstalledFixtures.EnemyTiles.Spritemaps
            ?? throw new InvalidOperationException("The installation holds the enemy compositions.");
        if (!spritemaps.TryGetDisplay(frame, out EnemySpritemapParts display))
            throw new InvalidOperationException($"{frame} has no installed composition.");
        foreach (SpriteFramePart part in EnemySpriteFrames.Frame(display, TileBase(set, enemy), 0).Parts)
        {
            for (int row = 0; row < part.Size.Y / SpriteArtwork.CharacterPixels; row++)
            {
                for (int column = 0; column < part.Size.X / SpriteArtwork.CharacterPixels; column++)
                {
                    int first = (part.Character + row * SpriteArtwork.CharactersPerRow + column) * SpriteArtwork.CharacterByteCount;
                    if (first < SpriteCharacterTableLayout.TableByteCount
                        && !last.AsSpan(first, SpriteArtwork.CharacterByteCount).SequenceEqual(loaded.AsSpan(first, SpriteArtwork.CharacterByteCount)))
                        return true;
                }
            }
        }
        return false;
    }

    /// <summary>The frames of <paramref name="enemy"/>'s sheet under <paramref name="set"/> the sheet says a rotting corpse lies in.</summary>
    private static IEnumerable<EnemyFrameId> CorpseFrames(InstalledEnemySheets sheets, RoomEnemyGraphicsSetDefinition set, EnemyDefinitionId enemy) =>
        EnemyDefinitionFrameDefinitions.Frames[enemy].Where(frame => sheets.CorpseLayout(set, enemy, EnemySheetDefinitions.FrameName(frame)) is not null);

    /// <summary>The name of <paramref name="frame"/>'s picture once its pages have reached <paramref name="step"/>.</summary>
    private static string PictureAt(string frame, FrameArtworkVariation pages, int step) => FrameArtworkVariation.PictureName(frame,
        pages.PictureAt(step, frame) is int picture ? [new EncounterArtworkStep(pages.Sequence, picture)] : []);

    /// <summary>The first graphics set that installs a sheet of <paramref name="enemy"/>.</summary>
    private static RoomEnemyGraphicsSetDefinition SetDrawing(InstalledEnemySheets sheets, EnemyDefinitionId enemy)
    {
        string name = sheets.sheets.Keys.First(key => key.Enemy == enemy).Set;
        return RoomEnemyGraphicsSetLists.DrawnUnder.First(set => set.Name == name);
    }

    /// <summary>The character a frame of <paramref name="enemy"/> under <paramref name="set"/> counts its parts' characters from.</summary>
    private static int TileBase(RoomEnemyGraphicsSetDefinition set, EnemyDefinitionId enemy) => GraphicsSetCharacterLayout.Of(set).TileBaseOf(enemy);

    /// <summary>The sprite characters <paramref name="set"/>'s load and <paramref name="reached"/> steps leave.</summary>
    private static byte[] Loaded(RoomEnemyGraphicsSetDefinition set, IReadOnlyList<EncounterArtworkStep> reached) =>
        EnemySheetFixtures.LivePage(set, reached).Characters[..SpriteCharacterTableLayout.TableByteCount].ToArray();

    /// <summary>One screen pixel.</summary>
    /// <param name="X">Its column.</param>
    /// <param name="Y">Its row.</param>
    private readonly record struct ScreenPixel(int X, int Y);

    /// <summary>Runs the corpse rot drawing check.</summary>
    [VerificationCommand("--corpse-rot-drawing")]
    internal static int CorpseRotDrawingCommand()
    {
        Suite(nameof(VerifyCorpseRotDrawing), VerifyCorpseRotDrawing);
        return 0;
    }
}
