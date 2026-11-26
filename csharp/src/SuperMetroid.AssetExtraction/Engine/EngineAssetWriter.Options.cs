using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction.Frontend.Options.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend.Artwork;
using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.Core.Frontend.Options;
using SuperMetroid.Core.Frontend.Options.Artwork;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>
/// The options, drawn with the old game's own pictures: its backdrop, the first page with the language chosen laid
/// over it, the page of special settings in each language with each setting's state laid over it, the heading of
/// each page and the missile that points at a row. The old page of buttons is pictures of the old controller's
/// buttons, which the Godot game's buttons are not; the Godot game lists its own under that page's heading.
/// </summary>
public sealed partial class EngineAssetWriter
{
    /// <summary>The Godot game's name for each of the old special settings.</summary>
    private static readonly IReadOnlyDictionary<GameOptionsToggle, string> SpecialSettingNames = new Dictionary<GameOptionsToggle, string>
    {
        [GameOptionsToggle.IconCancel] = "manual_item_cancel",
        [GameOptionsToggle.Moonwalk] = "moonwalk",
    };

    /// <summary>Writes the data file of the options and the pictures it names.</summary>
    private void WriteOptions()
    {
        const string folder = "menus/options";
        InstalledScreenContent screens = _installation.LoadMaps();
        GameOptionsPresentation placed = screens.GameOptions;

        // A picture of the menus' sheet, and where its corner lies on the page.
        JsonObject Laid(string frame, string name)
        {
            (int _, int _, int originX, int originY) = _sheets.SpriteSize(SpriteSheetId.MenuPictures, frame);
            return new JsonObject
            {
                ["picture"] = _sheets.WriteSprite(_tree, SpriteSheetId.MenuPictures, frame, $"{folder}/{name}.png"),
                ["x"] = -originX,
                ["y"] = -originY,
            };
        }

        string layoutPath = Path.Combine(_installation.MapDirectory, GameOptionsPresentationFormat.FileName);
        JsonNode layout = JsonNode.Parse(File.ReadAllText(layoutPath))
            ?? throw new InvalidDataException($"The layout of the options, {layoutPath}, holds nothing.");
        var headings = new JsonObject();
        foreach (GameOptionsPage page in Enum.GetValues<GameOptionsPage>())
        {
            string frame = GameOptionsNames.HeadingSpriteName(page);
            JsonNode anchor = layout["headingAnchors"]?[page.ToString()]
                ?? throw new InvalidDataException($"The layout of the options, {layoutPath}, places no heading for its page {page}.");
            (int _, int _, int originX, int originY) = _sheets.SpriteSize(SpriteSheetId.GameOptions, frame);
            headings[Named(page)] = new JsonObject
            {
                ["picture"] = _sheets.WriteSprite(_tree, SpriteSheetId.GameOptions, frame, $"{folder}/heading_{Named(page)}.png"),
                ["x"] = anchor["x"]!.GetValue<int>() - originX,
                ["y"] = anchor["y"]!.GetValue<int>() - originY,
            };
        }

        var special = new JsonObject();
        var languages = new JsonObject();
        var toggles = new JsonObject();
        foreach (TextLanguage language in Enum.GetValues<TextLanguage>())
        {
            string name = Named(language);
            GameOptionsScreen screen = GameOptionsScreen.Shown(GameOptionsPage.Special, language);
            special[name] = Laid(GameOptionsNames.PageFrame(screen), "special_" + name)["picture"]!.DeepClone();
            languages[name] = Laid(GameOptionsNames.LanguageFrame(language), "language_" + name);
            var states = new JsonObject();
            foreach ((GameOptionsToggle toggle, string setting) in SpecialSettingNames)
            {
                // The old setting is whether a chosen weapon is put away of itself; the Godot game's is whether it is
                // left to the player, which is the other way about.
                bool sameSense = toggle != GameOptionsToggle.IconCancel;
                states[setting] = new JsonObject
                {
                    ["on"] = Laid(GameOptionsNames.ToggleFrame(screen, toggle, sameSense), $"{setting}_{name}_on"),
                    ["off"] = Laid(GameOptionsNames.ToggleFrame(screen, toggle, !sameSense), $"{setting}_{name}_off"),
                };
            }

            toggles[name] = states;
        }

        JsonObject Cursor(GameOptionsRow row)
        {
            var at = placed.CursorPosition(row);
            return At(at.X, at.Y);
        }

        var places = new JsonObject
        {
            ["cursor_start_game"] = Cursor(new GameOptionsRow.OnPrimaryPage(GameOptionsPrimaryRow.StartGame)),
            ["cursor_english_text"] = Cursor(new GameOptionsRow.OnPrimaryPage(GameOptionsPrimaryRow.EnglishText)),
            ["cursor_japanese_text"] = Cursor(new GameOptionsRow.OnPrimaryPage(GameOptionsPrimaryRow.JapaneseText)),
            ["cursor_controller_settings"] = Cursor(new GameOptionsRow.OnPrimaryPage(GameOptionsPrimaryRow.ControllerSettings)),
            ["cursor_special_settings"] = Cursor(new GameOptionsRow.OnPrimaryPage(GameOptionsPrimaryRow.SpecialSettings)),
            ["cursor_special_exit"] = Cursor(new GameOptionsRow.OnSpecialPage(GameOptionsSpecialRow.Exit)),
        };

        // The page of buttons: its picture in each language, which has its heading's words and its rows' words; the
        // cursor's place beside each of its rows, from the top; and where, on each action's row, the cartridge names
        // the button bound to it, which is where the Godot page writes the name of whatever button is.
        var controller = new JsonObject();
        foreach (TextLanguage language in Enum.GetValues<TextLanguage>())
        {
            controller[Named(language)] = Laid(
                GameOptionsNames.PageFrame(GameOptionsScreen.Shown(GameOptionsPage.Controller, language)), "controller_" + Named(language))["picture"]!.DeepClone();
        }

        var buttonRows = new JsonArray();
        foreach (GameOptionsControllerRow row in GameOptionsPageRows.Controller)
            buttonRows.Add(Cursor(new GameOptionsRow.OnControllerPage(row)));
        var buttonNames = new JsonArray();
        foreach (ControllerAction action in GameOptionsPageRows.ControllerActions)
        {
            (int _, int _, int originX, int originY) = _sheets.SpriteSize(SpriteSheetId.MenuPictures, GameOptionsNames.ControllerLabelFrame(action, ControllerButton.A));
            buttonNames.Add(At(-originX, -originY));
        }

        // The settings of the special page, in the order of its rows.
        var specialRows = new JsonArray();
        foreach (GameOptionsSpecialRow row in GameOptionsPageRows.Special)
        {
            if (row == GameOptionsSpecialRow.Exit)
                continue;
            string setting = SpecialSettingNames[row switch
            {
                GameOptionsSpecialRow.IconCancel => GameOptionsToggle.IconCancel,
                GameOptionsSpecialRow.Moonwalk => GameOptionsToggle.Moonwalk,
                _ => throw new InvalidDataException($"The options' special page has a row {row}, which this does not know."),
            }];
            specialRows.Add(setting);
            places["cursor_special_" + setting] = Cursor(new GameOptionsRow.OnSpecialPage(row));
        }

        EngineSheetPictures cursorPictures = _sheets.CutSprites(_tree, SpriteSheetId.GameOptions, folder + "/cursor", MenuMissileFrames.Names);
        var cursorSteps = new JsonArray();
        foreach (string frame in MenuMissileFrames.Names)
            cursorSteps.Add(new JsonObject { ["picture"] = cursorPictures.Of(frame), ["updates"] = placed.CursorFrameDuration });

        _tree.WriteData("menus/options.json", new JsonObject
        {
            ["backdrop"] = Colour(screens.Backdrops.Menus),
            ["background"] = _sheets.WriteSprite(_tree, SpriteSheetId.MenuPictures, GameOptionsNames.BackgroundFrame, folder + "/background.png"),
            ["headings"] = headings,
            ["primary"] = _sheets.WriteSprite(
                _tree, SpriteSheetId.MenuPictures, GameOptionsNames.PageFrame(GameOptionsScreen.Shown(GameOptionsPage.Primary, TextLanguage.English)), folder + "/primary.png"),
            ["special"] = special,
            ["controller"] = controller,
            ["button_rows"] = buttonRows,
            ["button_names"] = buttonNames,
            ["buttons_scroll"] = GameOptionsDefinitions.ControllerScrollLimit,
            ["buttons_scroll_step"] = GameOptionsDefinitions.ControllerScrollPixelsPerFrame,
            ["languages"] = languages,
            ["toggles"] = toggles,
            ["special_rows"] = specialRows,
            ["places"] = places,
            ["move_sound"] = EngineNames.Sound(SamusSound.MenuCursor),
            ["select_sound"] = EngineNames.Sound(SamusSound.MenuConfirm),
            ["entry_black_updates"] = GameOptionsDefinitions.BlackHoldUpdates,
            // It fades a level of brightness an update.
            ["fade_levels"] = SuperMetroid.Core.Rendering.DisplayBrightness.StepsFromBlackToFull,
            ["timelines"] = new JsonObject
            {
                ["cursor"] = new JsonObject { ["steps"] = cursorSteps, ["loops_to"] = 0, ["offset"] = At(cursorPictures.OffsetX, cursorPictures.OffsetY) },
            },
        });
    }
}
