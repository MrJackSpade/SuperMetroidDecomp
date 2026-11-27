using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction.Frontend.FileSelect.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Frontend.Artwork;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.FileSelect.Artwork;
using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Saves;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>
/// The choice of saved game, drawn with the old game's own pictures: its backdrop, a whole picture for each of its
/// pages, the patches, digits, tanks and letters laid over a page for what each saved game holds, the helmets beside
/// them, the missile that points at a row, and the arrow of a copy.
/// </summary>
public sealed partial class EngineAssetWriter
{
    /// <summary>Writes the data file of the choice of saved game and the pictures it names.</summary>
    private void WriteFileSelect()
    {
        const string folder = "menus/file_select";
        InstalledScreenContent screens = _installation.LoadMaps();
        FileSelectPresentation placed = screens.FileSelect;

        string Picture(string frame, string name) => _sheets.WriteSprite(_tree, SpriteSheetId.MenuPictures, frame, $"{folder}/{name}.png");

        var pages = new JsonObject();
        foreach (FileSelectPage page in Enum.GetValues<FileSelectPage>())
            pages[Named(page)] = Picture(FileSelectParts.PageFrame(page), "page_" + Named(page));

        // The headings are sprites, kept about their middles; the Godot game draws each from its corner.
        // Where they and the helmets stand is in the installed layout alone.
        string layoutPath = Path.Combine(_installation.MapDirectory, FileSelectPresentationFormat.FileName);
        JsonNode layout = JsonNode.Parse(File.ReadAllText(layoutPath))
            ?? throw new InvalidDataException($"The layout of the choice of saved game, {layoutPath}, holds nothing.");
        JsonNode Listed(string group, string name) => layout[group]?[name]
            ?? throw new InvalidDataException($"The layout of the choice of saved game, {layoutPath}, places no \"{name}\" under \"{group}\".");
        var borders = new JsonObject();
        foreach (FileSelectBorder border in Enum.GetValues<FileSelectBorder>())
        {
            string frame = FileSelectParts.BorderSpriteName(border);
            JsonNode anchor = Listed("borderAnchors", FileSelectPresentationFormat.BorderName(border));
            (int _, int _, int originX, int originY) = _sheets.SpriteSize(SpriteSheetId.FileSelect, frame);
            borders[Named(border)] = new JsonObject
            {
                ["picture"] = _sheets.WriteSprite(_tree, SpriteSheetId.FileSelect, frame, $"{folder}/border_{Named(border)}.png"),
                ["x"] = anchor["x"]!.GetValue<int>() - originX,
                ["y"] = anchor["y"]!.GetValue<int>() - originY,
            };
        }

        JsonObject Slot(FileSelectCursorPage page, SaveSlot slot)
        {
            FileSelectSlotFieldDocument field = placed.Slot(page, slot);
            var cursor = placed.CursorPosition(page, FileSelectItemRows.ForSaveSlot(slot));
            var written = new JsonObject
            {
                ["energy"] = At(field.EnergyAnchor.X, field.EnergyAnchor.Y),
                ["health"] = At(field.HealthAnchor.X, field.HealthAnchor.Y),
                ["no_data"] = At(field.NoDataAnchor.X, field.NoDataAnchor.Y),
                ["time"] = At(field.TimeValueAnchor.X, field.TimeValueAnchor.Y),
                ["cursor"] = At(cursor.X, cursor.Y),
            };
            if (page == FileSelectCursorPage.Main)
            {
                JsonNode helmet = Listed("helmetAnchors", slot.ToString());
                written["helmet"] = At(helmet["x"]!.GetValue<int>(), helmet["y"]!.GetValue<int>());
            }

            return written;
        }

        var mainSlots = new JsonArray();
        var dataSlots = new JsonArray();
        var letters = new JsonArray();
        foreach (SaveSlot slot in SaveSlots.All)
        {
            mainSlots.Add(Slot(FileSelectCursorPage.Main, slot));
            dataSlots.Add(Slot(FileSelectCursorPage.DataManagement, slot));
            letters.Add(Picture(FileSelectParts.LetterFrame(slot), "letter_" + slot.ToString().ToLowerInvariant()));
        }

        var digits = new JsonArray();
        for (int digit = 0; digit < FileSelectPresentationDefinitions.DigitCount; digit++)
            digits.Add(Picture(FileSelectParts.DigitFrame(digit), $"digit_{digit}"));

        JsonObject Cursor(FileSelectCursorPage page, FileSelectItem item)
        {
            var at = placed.CursorPosition(page, item);
            return At(at.X, at.Y);
        }

        JsonObject Letter(FileSelectAnchor anchor)
        {
            var at = placed.DynamicAnchor(anchor);
            return At(at.X, at.Y);
        }

        var yes = placed.CursorPosition(YesNoChoice.Yes);
        var no = placed.CursorPosition(YesNoChoice.No);
        var places = new JsonObject
        {
            ["cursor_copy"] = Cursor(FileSelectCursorPage.Main, FileSelectItem.Copy),
            ["cursor_clear"] = Cursor(FileSelectCursorPage.Main, FileSelectItem.Clear),
            ["cursor_exit"] = Cursor(FileSelectCursorPage.Main, FileSelectItem.Exit),
            ["cursor_data_exit"] = Cursor(FileSelectCursorPage.DataManagement, FileSelectItem.Exit),
            ["cursor_yes"] = At(yes.X, yes.Y),
            ["cursor_no"] = At(no.X, no.Y),
            ["letter_copy_destination_source"] = Letter(FileSelectAnchor.CopyDestinationSource),
            ["letter_copy_confirm_source"] = Letter(FileSelectAnchor.CopyConfirmSource),
            ["letter_copy_confirm_destination"] = Letter(FileSelectAnchor.CopyConfirmDestination),
            ["letter_clear_confirm_source"] = Letter(FileSelectAnchor.ClearConfirmSource),
        };

        // The arrow of a copy from one saved game to another: which arrow, and where. A saved game is named by its
        // number, from one.
        var arrows = new JsonObject();
        foreach (FileCopyRoute route in Enum.GetValues<FileCopyRoute>())
        {
            FileCopyArrowPlacement arrow = FileCopyRoutes.ArrowOf(route);
            string name = $"{(int)FileCopyRoutes.SourceOf(route) + 1}_{(int)FileCopyRoutes.DestinationOf(route) + 1}";
            arrows[name] = "arrow_" + Named(arrow.Shape);
            places["arrow_" + name] = At(FileCopyArrowDefinitions.OriginX, arrow.Y);
        }

        var timelines = new JsonObject();

        EngineSheetPictures cursorPictures = _sheets.CutSprites(_tree, SpriteSheetId.FileSelect, folder + "/cursor", MenuMissileFrames.Names);
        var cursorSteps = new JsonArray();
        foreach (string frame in MenuMissileFrames.Names)
            cursorSteps.Add(new JsonObject { ["picture"] = cursorPictures.Of(frame), ["updates"] = placed.CursorFrameDuration });
        timelines["cursor"] = new JsonObject { ["steps"] = cursorSteps, ["loops_to"] = 0, ["offset"] = At(cursorPictures.OffsetX, cursorPictures.OffsetY) };

        // A helmet looks away until its saved game is chosen; then it turns to face out, and stays so.
        string[] helmetFrames = [.. FileSelectHelmetAnimation.Turn.Select(FileSelectParts.HelmetSpriteName)];
        EngineSheetPictures helmetPictures = _sheets.CutSprites(_tree, SpriteSheetId.FileSelect, folder + "/helmet", helmetFrames);
        // The cartridge starts the turn with its count at one ($81:A1C2), so the picture of the helmet looking away
        // lasts the one update and each after it the whole count.
        var turn = new JsonArray();
        foreach (string frame in helmetFrames)
            turn.Add(new JsonObject { ["picture"] = helmetPictures.Of(frame), ["updates"] = turn.Count == 0 ? 1 : placed.HelmetFrameDuration });
        JsonObject helmetOffset = At(helmetPictures.OffsetX, helmetPictures.OffsetY);
        // The turn stays on its last picture: the Godot screen counts the turn's length itself.
        timelines["helmet_turn"] = new JsonObject { ["steps"] = turn, ["loops_to"] = helmetFrames.Length - 1, ["offset"] = helmetOffset };
        timelines["helmet_away"] = new JsonObject
        {
            ["steps"] = new JsonArray(new JsonObject
            {
                ["picture"] = helmetPictures.Of(FileSelectParts.HelmetSpriteName(FileSelectHelmetFrame.TurnedAway)), ["updates"] = 1,
            }),
            ["loops_to"] = 0,
            ["offset"] = helmetOffset.DeepClone(),
        };

        // The arrow's colours run along it: its first look holds a little longer than the rest, and then they go
        // round for as long as the question stands.
        foreach (FileCopyArrowShape shape in FileCopyArrowDefinitions.Shapes)
        {
            string frame = FileCopyArrowDefinitions.FrameName(shape);
            (int _, int _, int originX, int originY) = _sheets.SpriteSize(SpriteSheetId.FileSelect, frame);
            string Look(FileCopyArrowRotationStep step) => _sheets.WriteSprite(
                _tree, SpriteSheetId.FileSelect, frame, $"{folder}/arrow_{Named(shape)}_{step.Step}.png", FileCopyArrowShades.Rotation(step).Key);
            var steps = new JsonArray(new JsonObject
            {
                ["picture"] = Look(FileCopyArrowRotationStep.Unrotated), ["updates"] = FileCopyArrowDefinitions.InitialGlowDelay,
            });
            FileCopyArrowRotationStep rotation = FileCopyArrowRotationStep.Unrotated;
            for (int turned = 0; turned < FileCopyArrowRotationStep.All.Count; turned++)
            {
                rotation = rotation.Next;
                steps.Add(new JsonObject { ["picture"] = Look(rotation), ["updates"] = FileCopyArrowDefinitions.GlowDelay });
            }

            timelines["arrow_" + Named(shape)] = new JsonObject { ["steps"] = steps, ["loops_to"] = 1, ["offset"] = At(-originX, -originY) };
        }

        _tree.WriteData("menus/file_select.json", new JsonObject
        {
            ["backdrop"] = Colour(screens.Backdrops.Menus),
            ["background"] = Picture(FileSelectParts.BackgroundFrame, "background"),
            ["pages"] = pages,
            ["borders"] = borders,
            ["energy"] = Picture(FileSelectParts.PatchFrame(FileSelectPatch.Energy), "energy"),
            ["no_data"] = Picture(FileSelectParts.PatchFrame(FileSelectPatch.NoData), "no_data"),
            ["time_colon"] = Picture(FileSelectParts.PatchFrame(FileSelectPatch.TimeColon), "time_colon"),
            ["tank_filled"] = Picture(FileSelectParts.TankFrame(true), "tank_filled"),
            ["tank_empty"] = Picture(FileSelectParts.TankFrame(false), "tank_empty"),
            ["digits"] = digits,
            ["letters"] = letters,
            ["main_slots"] = mainSlots,
            ["data_slots"] = dataSlots,
            ["places"] = places,
            ["arrows"] = arrows,
            ["glyph_width"] = FileSelectFieldDefinitions.GlyphWidthPixels,
            ["colon_column"] = FileSelectFieldDefinitions.ColonColumn,
            ["largest_time_part"] = FileSelectFieldDefinitions.MaximumTimePart,
            ["energy_per_tank"] = SamusEnergyDefinitions.EnergyPerTank,
            ["tank_size"] = FileSelectEnergyTankDefinitions.TankPixels,
            ["tank_column"] = FileSelectEnergyTankDefinitions.ColumnOffset,
            ["tank_row"] = FileSelectEnergyTankDefinitions.RowOffset,
            ["tanks_per_row"] = FileSelectEnergyTankDefinitions.TanksPerRow,
            // Black before it fades in: blanking, waiting out that update, the black hold, setting up its fades and
            // waiting out that update.
            ["entry_black_updates"] = FileSelectLayout.BlackHoldUpdates + 4,
            // Its fades go a level of brightness every so many updates.
            ["fade_levels"] = SuperMetroid.Core.Rendering.DisplayBrightness.StepsFromBlackToFull,
            ["fade_updates_per_level"] = FileSelectLayout.FadeUpdatesPerStep,
            ["move_sound"] = EngineNames.Sound(SamusSound.MenuCursor),
            ["select_sound"] = EngineNames.Sound(SamusSound.FileSelected),
            ["confirm_sound"] = EngineNames.Sound(SamusSound.MenuConfirm),
            ["timelines"] = timelines,
        });
    }
}
