using SuperMetroid.AssetExtraction.Enemies.Corpses;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.AssetExtraction.Enemies.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Enemies.Corpses;

/// <summary>
/// Confirms every rotting corpse is drawn whole, from its own rot state, at every rot call (#1539): the installed sheet's one
/// picture of each corpse frame, drawn through the rows the corpse's own rot leaves and where the sheet says the corpse
/// lies in it, shows at every pixel the unrotted picture's pixel of the row the authored rot has moved there, across the
/// whole picture, or nothing where it has cleared it. The authored rot is restated here: each move copies a row onto the
/// row below it, the corpse's last row landing nowhere, and a move, rather than a copy, then clears it. It covers both dead
/// Sidehoppers, every dead Zoomer, Ripper and Skree, the dead Torizo's body and its hook's sand heap, and Mother Brain's
/// corpse; and that two corpses of one kind rot apart.
/// </summary>
internal static class CorpseRotDrawingVerification
{
    /// <summary>The screen point every compared draw's origin lands on.</summary>
    private static readonly Position16<ScreenPixels> Origin = new(128, 112);

    /// <summary>Draws every corpse frame at every rot call and compares it with the authored rot of its whole picture.</summary>
    internal static void VerifyCorpseRotDrawing()
    {
        InstalledEnemySheets sheets = InstalledFixtures.EnemySheets;
        int compared = VerifyDeadMonster(sheets, EnemyDefinitionId.CorpseSidehopper, DeadSidehopperCorpseDefinitions.RowCount);
        foreach (EnemyDefinitionId enemy in new[] { EnemyDefinitionId.CorpseZoomer, EnemyDefinitionId.CorpseRipper, EnemyDefinitionId.CorpseSkree })
        {
            DeadTourianBehavior.DeadTourianCorpseProfile profile = DeadTourianBehavior.ProfileForDeadTourianCorpse(enemy);
            foreach (int rowCount in Enumerable.Range(0, DeadTourianCorpses.VariantCount(profile.Species))
                .Select(variant => DeadTourianCorpses.For(profile.Species, variant).RowCount).Distinct())
                compared += VerifyDeadMonster(sheets, enemy, rowCount);
        }
        compared += VerifyDeadTorizo(sheets);
        compared += VerifyMotherBrain(sheets);
        VerifyCorpsesOfOneKindRotApart();
        Console.WriteLine($"  Corpse rot drawing: {compared} draws of every corpse frame at every rot call show the whole picture as its own rot leaves it.");
    }

    /// <summary>
    /// Rots a dead monster's own rows as its rot calls do and compares each of its frames that shows the corpse at every call,
    /// from before the first.
    /// </summary>
    private static int VerifyDeadMonster(InstalledEnemySheets sheets, EnemyDefinitionId enemy, int rowCount)
    {
        RoomEnemyGraphicsSetDefinition set = SetDrawing(sheets, enemy);
        EnemyFrameId[] frames = [.. CorpseFrames(sheets, set, enemy)];
        AssertTrue(frames.Length > 0, $"a frame of {enemy} shows its corpse");
        var rows = new RottingCorpseRows(rowCount);
        var authored = new AuthoredRot(rowCount);
        var schedule = new CorpseRotSchedule(rowCount);
        int compared = 0;
        bool rotting = true;
        for (int call = 0; rotting; call++)
        {
            if (call > 0)
                rotting = schedule.Step((row, operation) => { rows.MoveRow(row, operation); authored.Move(row, operation); }, _ => { });
            foreach (EnemyFrameId frame in frames)
            {
                string picture = EnemySheetDefinitions.FrameName(frame);
                compared += AssertDrawsWhole(sheets.For(set, enemy).Frame(picture), sheets.Under(set).Enemy(enemy).CorpsePlacement(picture), rows.Shown,
                    authored, $"{enemy} {frame} after rot call {call}");
            }
        }
        return compared;
    }

    /// <summary>
    /// Rots the dead Torizo through its own rot calls and compares its body frame and its hook's sand heap at each call,
    /// from the intact corpse to its finished rot: the body is its own rows whole, and the heap shows a line more every
    /// fifteenth call.
    /// </summary>
    private static int VerifyDeadTorizo(InstalledEnemySheets sheets)
    {
        RoomEnemyGraphicsSetDefinition set = SetDrawing(sheets, EnemyDefinitionId.CorpseTorizo);
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom());
        var state = new DeadTorizoEnemyState(enemies.SlotTable.Slots[0], new CorpseRotSchedule(DeadTorizoCorpseDefinitions.RowCount));
        var authored = new AuthoredRot(DeadTorizoCorpseDefinitions.RowCount);
        string body = EnemySheetDefinitions.FrameName(EnemyFrameId.DeadTorizoStationary);
        string hook = EnemySheetDefinitions.EncounterFrameName(DeadTorizoArtworkDefinitions.SandHeapFrame);
        SpriteSheet bodySheet = sheets.For(set, EnemyDefinitionId.CorpseTorizo);
        SpawnedObjectSheet hookSheet = sheets.Spawned(set, SpawnedObjectOwner.Shared);
        int compared = 0;
        bool rotting = true;
        while (true)
        {
            int calls = checked((int)state.ProcessCallCount);
            compared += AssertDrawsWhole(bodySheet.Frame(body), sheets.Under(set).Enemy(EnemyDefinitionId.CorpseTorizo).CorpsePlacement(body), state.Rows.Shown,
                authored, $"dead Torizo after rot call {calls}");
            compared += AssertDrawsWhole(hookSheet.Frame(hook), sheets.Under(set).Spawned(SpawnedObjectOwner.Shared).CorpsePlacement(hook),
                DeadTorizoBehavior.ShownSand(state), AuthoredRot.Sand(calls), $"dead Torizo's hook after rot call {calls}");
            if (!rotting)
                break;
            state.ProcessCallCount++;
            rotting = state.Rot.Step((row, operation) => { state.Rows.MoveRow(row, operation); authored.Move(row, operation); }, _ => { });
        }
        return compared;
    }

    /// <summary>Rots Mother Brain's corpse through its own rot calls and compares her corpse frame at every call, from before the first.</summary>
    private static int VerifyMotherBrain(InstalledEnemySheets sheets)
    {
        RoomEnemyGraphicsSetDefinition set = SetDrawing(sheets, EnemyDefinitionId.MotherBrainHead);
        string name = EnemySheetDefinitions.FrameName(EnemyFrameId.MotherBrainHeadCorpseLoop);
        AssertEqual(0, sheets.Under(set).Enemy(EnemyDefinitionId.MotherBrainHead).ChangingEncounters(name).Count,
            "Mother Brain's corpse frame has one finished picture, which no later look changes");
        SpriteSheet sheet = sheets.For(set, EnemyDefinitionId.MotherBrainHead);
        var rotting = new MotherBrainCorpseRottingState();
        var authored = new AuthoredRot(MotherBrainCorpseRottingState.EntryCount);
        int compared = 0;
        bool still = true;
        for (int call = 0; still; call++)
        {
            if (call > 0)
            {
                still = rotting.Step(new Position16<RoomPixels>(128, 112), MotherBrainRandomSample.FromGenerator(unchecked((ushort)(call * 37))), call).StillRotting;
                ReplaySchedule(authored, rotting);
            }
            compared += AssertDrawsWhole(sheet.Frame(name), sheets.Under(set).Enemy(EnemyDefinitionId.MotherBrainHead).CorpsePlacement(name), rotting.Shown,
                authored, $"Mother Brain's corpse after rot call {call}");
        }
        return compared;
    }

    /// <summary>
    /// Brings <paramref name="authored"/> to the rot Mother Brain's corpse has taken: her state runs its schedule itself, so the
    /// authored rot follows a schedule of its own through as many calls.
    /// </summary>
    private static void ReplaySchedule(AuthoredRot authored, MotherBrainCorpseRottingState rotting)
    {
        while (authored.Calls < rotting.ProcessCallCount)
            authored.Call();
    }

    /// <summary>
    /// Two dead Sidehoppers of one kind rot apart (#1539): rotting one through its whole schedule leaves the other's rows every
    /// one itself.
    /// </summary>
    private static void VerifyCorpsesOfOneKindRotApart()
    {
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom());
        var rotting = new DeadSidehopperEnemyState(enemies.SlotTable.Slots[0], 2, new CorpseRotSchedule(DeadSidehopperCorpseDefinitions.RowCount));
        var intact = new DeadSidehopperEnemyState(enemies.SlotTable.Slots[1], 2, new CorpseRotSchedule(DeadSidehopperCorpseDefinitions.RowCount));
        while (rotting.Rot.Step(rotting.Rows.MoveRow, _ => { }))
        {
        }
        AssertTrue(Enumerable.Range(0, DeadSidehopperCorpseDefinitions.RowCount).Any(row => rotting.Rows.Shown.Shown(row) != row),
            "the rotted corpse's rows have moved");
        for (int row = 0; row < DeadSidehopperCorpseDefinitions.RowCount; row++)
            AssertEqual<int?>(row, intact.Rows.Shown.Shown(row), $"the other corpse of the kind still shows its row {row} itself");
    }

    /// <summary>
    /// Checks <paramref name="image"/>, in which the corpse lies as <paramref name="placement"/> says, drawn through
    /// <paramref name="rows"/> as the game draws it, shows at every pixel the unrotted picture's pixel of the row
    /// <paramref name="authored"/> says has moved there, the same row across the whole picture, or nothing where it has
    /// cleared it; the renderer draws it so once, part-way through the rot.
    /// </summary>
    private static int AssertDrawsWhole(SpriteSheetFrame image, CorpsePicturePlacement? placement, CorpseRotRows rows, AuthoredRot authored, string context)
    {
        if (placement is null)
            throw new InvalidOperationException($"{context}: the sheet does not say where the corpse lies in its picture.");
        var plain = new ImageSpriteDraw(new SpriteSource(SpriteSourceKind.Enemy, 0), image, Origin, SpriteMirror.None) { Shading = PictureShading.Standard };
        ImageSpriteDraw draw = plain with { CorpseRot = CorpseRotDrawing.Picture(placement, rows, image.Size) };
        for (ushort y = 0; y < image.Size.Y; y++)
        {
            int? source = authored.PictureRow(placement.Top, y, image.Size.Y);
            for (ushort x = 0; x < image.Size.X; x++)
            {
                RgbaPixel want = source is int row ? plain.PixelAt(new Position16<ScreenPixels>(x, checked((ushort)row))) : default;
                RgbaPixel got = draw.PixelAt(new Position16<ScreenPixels>(x, y));
                if (want != got)
                    throw new InvalidOperationException($"{context}: pixel ({x},{y}) shows {got}, where the corpse's own rot leaves {want}.");
            }
        }
        string key = context[..context.IndexOf(" after", StringComparison.Ordinal)];
        rottedComparisons[key] = rottedComparisons.GetValueOrDefault(key) + 1;
        if (rottedComparisons[key] == ShaderSampleComparison && draw.CorpseRot is { } rot)
            AssertShaderDrawsAsMapped(draw, rot, context);
        return 1;
    }

    /// <summary>Which comparison of each corpse picture the renderer also draws: one part-way through its rot.</summary>
    private const int ShaderSampleComparison = 8;

    /// <summary>How many comparisons each corpse picture has had, so each is drawn through the renderer once.</summary>
    private static readonly Dictionary<string, int> rottedComparisons = new(StringComparer.Ordinal);

    /// <summary>
    /// Checks the renderer draws <paramref name="draw"/> through its one table of rows (#1539): every screen pixel of the
    /// rotted draw shows the pixel the unrotted picture shows at the row its table maps that row to, in the same column, and
    /// the backdrop where the table clears the row. The unrotted picture is drawn once as the reference, so the comparison
    /// does not depend on how the renderer converts colours.
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
                RgbaPixel want = rot.Shown(y) is int row && draw.Corner.Y + row is >= 0 and < DisplayScreen.Height
                    ? plain[(draw.Corner.Y + row) * DisplayScreen.Width + screenX]
                    : backdrop;
                RgbaPixel got = rotted[screenY * DisplayScreen.Width + screenX];
                if (got != want)
                    throw new InvalidOperationException($"{context}: the renderer draws {got} at screen pixel ({screenX},{screenY}), where the corpse's rows show {want}.");
            }
        }
    }

    /// <summary>The brightness the renderer comparison draws at: full, so colours are unchanged.</summary>
    private const int FullBrightness = 15;

    /// <summary>The frames of <paramref name="enemy"/>'s sheet under <paramref name="set"/> the sheet says a rotting corpse lies in.</summary>
    private static IEnumerable<EnemyFrameId> CorpseFrames(InstalledEnemySheets sheets, RoomEnemyGraphicsSetDefinition set, EnemyDefinitionId enemy) =>
        EnemyDefinitionFrameDefinitions.Frames[enemy].Where(frame => sheets.Under(set).Enemy(enemy).CorpsePlacement(EnemySheetDefinitions.FrameName(frame)) is not null);

    /// <summary>The first graphics set rooms draw under whose installed artwork holds a variant of <paramref name="enemy"/>.</summary>
    private static RoomEnemyGraphicsSetDefinition SetDrawing(InstalledEnemySheets sheets, EnemyDefinitionId enemy) =>
        RoomEnemyGraphicsSetLists.DrawnUnder.First(set => sheets.Under(set).enemies.ContainsKey(enemy));

    /// <summary>
    /// The authored rot, restated apart from Core: for each corpse row, the corpse row it shows, or none. A move copies a row
    /// onto the row below it, the last row landing nowhere, and a move, rather than a copy, then clears it.
    /// </summary>
    private sealed class AuthoredRot
    {
        /// <summary>The corpse row each corpse row shows, or none.</summary>
        private readonly int?[] shown;

        /// <summary>The schedule the authored rot follows itself, for a corpse whose state runs its own.</summary>
        private readonly CorpseRotSchedule schedule;

        /// <summary>A corpse <paramref name="rowCount"/> rows tall, each row itself.</summary>
        internal AuthoredRot(int rowCount)
        {
            shown = [.. Enumerable.Range(0, rowCount).Select(row => (int?)row)];
            schedule = new CorpseRotSchedule(rowCount);
        }

        /// <summary>The rot calls it has followed its own schedule through.</summary>
        internal uint Calls { get; private set; }

        /// <summary>Moves corpse row <paramref name="row"/> by <paramref name="operation"/>.</summary>
        internal void Move(int row, CorpseRowOperation operation)
        {
            if (row + 1 < shown.Length)
                shown[row + 1] = shown[row];
            if (operation == CorpseRowOperation.Move)
                shown[row] = null;
        }

        /// <summary>Runs one rot call of its own schedule.</summary>
        internal void Call()
        {
            Calls++;
            _ = schedule.Step(Move, _ => { });
        }

        /// <summary>
        /// The picture row that picture row <paramref name="y"/> of a picture <paramref name="height"/> rows tall, whose corpse
        /// begins at <paramref name="top"/>, shows: itself outside the corpse, else the picture row of the corpse row moved there,
        /// or none where it is cleared or lies outside the picture.
        /// </summary>
        internal int? PictureRow(int top, int y, int height)
        {
            int corpseRow = y - top;
            if (corpseRow < 0 || corpseRow >= shown.Length)
                return y;
            return shown[corpseRow] is int source && source + top >= 0 && source + top < height ? source + top : null;
        }

        /// <summary>
        /// The dead Torizo's sand heap after <paramref name="calls"/> rot calls: each of its sixteen rows shows itself once that
        /// many lines have fallen from the last up, one every fifteen calls up to fifteen, and nothing before.
        /// </summary>
        internal static AuthoredRot Sand(int calls)
        {
            var sand = new AuthoredRot(16);
            int lines = Math.Min(15, calls / 15);
            for (int row = 0; row < 16 - lines; row++)
                sand.shown[row] = null;
            return sand;
        }
    }

    /// <summary>Runs the corpse rot drawing check.</summary>
    [VerificationCommand("--corpse-rot-drawing")]
    internal static int CorpseRotDrawingCommand()
    {
        Suite(nameof(VerifyCorpseRotDrawing), VerifyCorpseRotDrawing);
        return 0;
    }
}
