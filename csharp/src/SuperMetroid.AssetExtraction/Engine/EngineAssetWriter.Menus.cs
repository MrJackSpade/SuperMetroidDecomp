using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Escape;
using SuperMetroid.Core.Escape.Artwork;
using SuperMetroid.Core.Frontend.Artwork;
using SuperMetroid.Core.Frontend.GameOver;
using SuperMetroid.Core.Frontend.GameOver.Artwork;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.Core.Frontend.Title;
using SuperMetroid.Core.Frontend.Title.Artwork;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime.Attract;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>
/// What is shown outside play, and the words and messages shown over it: the game's font, the pictures of its
/// messages, the warnings typed across the screen, the title with its demos, and the game over screen.
/// </summary>
public sealed partial class EngineAssetWriter
{
    /// <summary>The scene of one of the old game's rooms, as the Godot project's room generator names it.</summary>
    private static string RoomScene(RoomDefinition room) =>
        $"res://game/rooms/{EngineSheets.FileName(room.Area.ToString())}/{EngineSheets.FileName(room.Name)}.tscn";

    /// <summary>The letters, figures and marks the opening's pages are typed in, each with the frame that shows it.</summary>
    private List<(char Character, string Frame)> NarrationGlyphs()
    {
        var glyphs = new List<(char, string)>();
        string prefix = IntroNarrationPictures.Letter('A')[..^1];
        foreach (string frame in _sheets.SpriteNames(SpriteSheetId.IntroNarrationPictures).Where(frame => frame.StartsWith(prefix, StringComparison.Ordinal)))
            glyphs.Add((frame[prefix.Length..].Single(), frame));
        return glyphs;
    }

    /// <summary>Writes what is shown outside play and the words and messages shown over it.</summary>
    private void WriteMenus()
    {
        // The game's own font: the old game keeps one whole set of letters, the one its opening is typed in, as they
        // look once typed. Its menus and messages are pictures of words, not words.
        _sheets.WriteFont(_tree, "text/font.json", SpriteSheetId.IntroNarrationPictures, NarrationGlyphs(), null, IntroShades.Glow(IntroTextGlowStep.All[^1]).Key);
        WriteMessages();
        WriteTypedWarnings();
        WriteTitle();
        WriteGameOver();
    }

    /// <summary>
    /// Writes the two warnings typed across the screen as an escape begins: each one's words, the font they are typed
    /// in and where they begin; and for the one aboard the station its heading and the pictures of its second wording.
    /// </summary>
    private void WriteTypedWarnings()
    {
        const string family = GameInstallationLayout.RoomBlockSheetDirectoryName, folder = "text/typed";
        const string front = "^front";
        EscapeTypewriterPresentation typed = _installation.LoadMaps().EscapeTypewriter;
        IReadOnlyList<string> written = _sheets.SheetsBeginning(family, "Written.");

        // The typed letters and the warning's pictures are on the sheet of what is written over the room they are
        // typed in; where several rooms' sheets have them, they are taken from the first.
        string SheetWith(string frame) => written.FirstOrDefault(sheet => _sheets.Shows(family, sheet, frame))
            ?? throw new InvalidDataException($"No sheet of what is written over a room has the picture {frame}.");
        string Whole(string frame, string path)
        {
            (RgbaPixel[] pixels, int width, int height) = _sheets.PictureOf(family, SheetWith(frame), frame);
            _tree.WritePicture(path, width, height, pixels);
            return path;
        }

        foreach ((EscapeTypewriterMessageId id, string name) in new[]
            { (EscapeTypewriterMessageId.Ceres, "ceres_self_destruct_warning"), (EscapeTypewriterMessageId.Zebes, "zebes_time_bomb_warning") })
        {
            EscapeTypewriterMessage message = typed.Get(id);
            if (message.Lines.Count == 0)
                throw new InvalidDataException($"The warning {id} has no lines.");
            int left = message.Lines[0].Start.X, top = message.Lines[0].Start.Y;
            int apart = message.Lines.Count > 1 ? message.Lines[1].Start.Y - top : EscapeTypewriterDefinitions.GlyphAdvance;
            for (int line = 0; line < message.Lines.Count; line++)
            {
                if (message.Lines[line].Start.X != left || message.Lines[line].Start.Y != top + line * apart)
                    throw new InvalidDataException($"The lines of the warning {id} do not lie evenly under one another, as the Godot game types them.");
            }

            string words = string.Join((char)10, message.Lines.Select(line => line.Text));
            char[] characters = [.. words.Where(character => EscapeTypewriterGlyph.Types(character) && character != (char)10).Distinct()];
            string fontSheet = SheetWith(EscapeTypewriterGlyph.PictureOf(id, characters[0]) + front);
            (RgbaPixel[] _, int cell, int cellHeight) = _sheets.PictureOf(family, fontSheet, EscapeTypewriterGlyph.PictureOf(id, characters[0]) + front);
            if (cell != EscapeTypewriterDefinitions.GlyphAdvance)
                throw new InvalidDataException($"The typed letters of the warning {id} are {cell} wide, and each is typed {EscapeTypewriterDefinitions.GlyphAdvance} from the last.");
            var strip = new RgbaPixel[cell * characters.Length * cellHeight];
            for (int index = 0; index < characters.Length; index++)
            {
                (RgbaPixel[] pixels, int width, int height) = _sheets.PictureOf(family, fontSheet, EscapeTypewriterGlyph.PictureOf(id, characters[index]) + front);
                if (width != cell || height != cellHeight)
                    throw new InvalidDataException($"The typed letters of the warning {id} are not all of one size.");
                for (int y = 0; y < cellHeight; y++)
                    pixels.AsSpan(y * cell, cell).CopyTo(strip.AsSpan((y * characters.Length + index) * cell, cell));
            }

            string font = $"{folder}/{name}_font";
            _tree.WritePicture(font + ".png", cell * characters.Length, cellHeight, strip);
            _tree.WriteData(font + ".json", new JsonObject
            {
                ["picture"] = font + ".png",
                ["cell"] = new JsonObject { ["width"] = cell, ["height"] = cellHeight },
                ["characters"] = new string(characters),
                ["line_height"] = apart,
            });

            // A letter takes the update it is typed on and the wait after it; the first letters of all are followed
            // at once; and the click comes with every so many letters typed, counted from the first.
            if (EscapeTypewriterDefinitions.GlyphClick.Phase != 0)
                throw new InvalidDataException("The typed warnings click on other than every so many letters counted from the first, which the Godot game does not do.");
            var layout = new JsonObject
            {
                ["font"] = font + ".json", ["x"] = left, ["y"] = top,
                ["updates_per_letter"] = EscapeTypewriterDefinitions.CharacterDelayFrames + 1,
                ["undelayed_letters"] = EscapeTypewriterDefinitions.UndelayedOpeningCharacters,
                ["letters_per_sound"] = EscapeTypewriterDefinitions.GlyphClick.Period,
            };
            if (id == EscapeTypewriterMessageId.Ceres)
            {
                var lines = new JsonArray();
                foreach (CeresEscapeWarningPicture line in CeresEscapeWarningPictures.JapaneseSubtitle)
                {
                    JsonObject at = At(line.Corner.X, line.Corner.Y);
                    at["picture"] = Whole(line.Picture + front, $"{folder}/{name}_{line.Name}.png");
                    lines.Add(at);
                }

                layout["second_wordings"] = new JsonObject
                {
                    ["ceres_self_destruct_subtitle"] = new JsonObject { ["languages"] = new JsonArray(Languages[1]), ["pictures"] = lines },
                };
            }

            _tree.WriteData($"{folder}/{Words(name, words)}.json", layout);
        }

        Whole(CeresEscapeWarningPictures.Emergency.Picture + front, "text/emergency.png");
    }

    /// <summary>Writes the title screen as it rests, and the demos it shows when left alone.</summary>
    private void WriteTitle()
    {
        const string folder = "menus/title";
        InstalledScreenContent screens = _installation.LoadMaps();
        TitleFloorSceneDefinition resting = TitleSequenceDefinitions.Scenes.FinalZoom;
        if (TitleSequenceDefinitions.Scenes.Rotation != RotationAngle.Zero)
            throw new InvalidDataException("The title's backdrop is turned, which the Godot title does not turn it.");

        JsonObject Sprite(string frame, Offset16<ScreenPixels> origin, TitleSpriteColouring colouring, string name, params string[] over)
        {
            (int _, int _, int originX, int originY) = _sheets.SpriteSize(SpriteSheetId.Title, frame);
            JsonObject at = At(origin.X - originX, origin.Y - originY);
            at["picture"] = _sheets.WriteSprite(_tree, SpriteSheetId.Title, frame, $"{folder}/{name}.png", [TitleShades.Drawn(colouring).Key, .. over]);
            return at;
        }

        var sets = new JsonArray();
        foreach (AttractDemoSet set in StockAttractDemoScenes.Playlist(completionSetUnlocked: false))
        {
            var files = new JsonArray();
            for (int scene = 0; scene < set.Scenes.Count; scene++)
            {
                string file = $"menus/demos/{EngineSheets.FileName(set.Name)}_{scene + 1}.json";
                _tree.WriteData(file, Demo(set.Scenes[scene]));
                files.Add(file);
            }

            sets.Add(files);
        }

        // The title's backdrop comes to rest at its own size.
        _tree.WriteData("menus/title.json", new JsonObject
        {
            ["music"] = MusicName(TitleSequenceDefinitions.Music.ImmediateTitleTrack),
            ["backdrop"] = Colour(screens.TitleBackdrop),
            ["floor"] = _sheets.WriteSprite(_tree, SpriteSheetId.TitleFloor, TitleFloors.FloorFrameName, folder + "/floor.png"),
            ["floor_per_pixel"] = 1,
            ["floor_centre"] = At(TitleSequenceDefinitions.Scenes.FloorCentre.X, TitleSequenceDefinitions.Scenes.FloorCentre.Y),
            ["floor_scroll"] = At(resting.Position.X, resting.Position.Y),
            // The copyright is never at rest in its drawn colours alone: come to rest by itself it keeps its fade's last
            // look, which lies over the restored colours, and rebuilt it shows the restored colours.
            ["pictures"] = new JsonArray(
                Sprite("Title.SuperMetroidLogo", TitleSequenceDefinitions.Sprites.LogoOrigin, TitleSequenceDefinitions.Sprites.LogoColouring, "logo")),
            // The copyright with its colours as the old game restores them once a skip has rebuilt the title.
            ["restored_copyright"] = Sprite("Title.NintendoCopyright", TitleSequenceDefinitions.Sprites.CopyrightOrigin,
                TitleSequenceDefinitions.Sprites.CopyrightColouring, "copyright_restored", TitleShades.RestoredCopyright.Key),
            ["opening"] = TitleOpening(folder),
            ["baby"] = TitleBaby(folder),
            ["demo_after_updates"] = TitleSequenceDefinitions.Timing.TitleScreenIdleTimeoutFrames,
            ["demo_sets"] = sets,
        });
    }

    /// <summary>One of the title's demos: its recording, the room it plays in, where Samus starts and what she carries.</summary>
    private static JsonObject Demo(AttractDemoScene scene)
    {
        // A demo with a second recording plays that one: the old game puts it in the first's place before the first plays.
        DemoRecording recording = DemoInputRecordings.Of(scene.Input.Continuation ?? scene.Input.Recording);
        JsonObject data = Recording(recording, RoomScene(scene.Room), "");
        SamusPoseId? pose = scene.Samus.Setup switch
        {
            AttractDemoSamusSetup.Posed posed => posed.Pose,
            AttractDemoSamusSetup.Shinespark spark => spark.Launch.Pose,
            AttractDemoSamusSetup.ForwardFacing => null,
            _ => throw new InvalidDataException($"A demo starts Samus as {scene.Samus.Setup}, which this does not know."),
        };
        JsonObject start = At(scene.Samus.Position.X, scene.Samus.Position.Y);
        start["facing"] = pose is { } posedAs && posedAs.ToString().Contains("Left", StringComparison.Ordinal) ? "left" : "right";

        // How she is staged: the place given is her middle in that body.
        start["as"] = scene.Samus.Setup switch
        {
            AttractDemoSamusSetup.ForwardFacing => "facing_forward",
            AttractDemoSamusSetup.Posed { Pose: SamusPoseId.FacingRightNormalPose or SamusPoseId.FacingLeftNormalPose } => "standing",
            AttractDemoSamusSetup.Posed { Pose: SamusPoseId.MorphBallMovingLeftPose } => "rolling",
            AttractDemoSamusSetup.Posed { Pose: SamusPoseId.FallingLeftPose } => "falling",
            AttractDemoSamusSetup.Shinespark { Launch: var launch } when launch == SuperMetroid.Core.Samus.Abilities.ShinesparkLaunch.DiagonalRight => "shinespark_diagonal",
            AttractDemoSamusSetup.Shinespark { Launch: var launch } when launch == SuperMetroid.Core.Samus.Abilities.ShinesparkLaunch.HorizontalLeft => "shinespark_across",
            _ => throw new InvalidDataException($"A demo stages Samus as {scene.Samus.Setup}, which the Godot demo has no start for."),
        };
        data["start"] = start;

        // The demo says where the view is as it begins; it is not put about her.
        data["view"] = At(scene.CameraPosition.X, scene.CameraPosition.Y);
        data["lasts_updates"] = scene.Duration;

        // One demo plays in Kraid's room with Kraid already beaten.
        if (scene.RoomSetup == AttractDemoRoomSetup.DefeatedKraid)
            data["facts"] = new JsonArray(EngineSheets.FileName(nameof(SuperMetroid.Core.Progression.AreaBoss.Kraid)) + "_defeated");

        var equipment = new JsonArray();
        var off = new JsonArray();
        foreach (SamusEquipmentFlags item in Enum.GetValues<SamusEquipmentFlags>().Where(item => item != SamusEquipmentFlags.None && scene.Items.HasFlag(item)))
            equipment.Add(EquipmentName(item));
        foreach (SamusBeamFlags beam in Enum.GetValues<SamusBeamFlags>().Where(beam => beam != SamusBeamFlags.None && scene.CollectedBeams.HasFlag(beam)))
        {
            equipment.Add(BeamName(beam));
            if (!scene.EquippedBeams.HasFlag(beam))
                off.Add(BeamName(beam));
        }

        if ((scene.EquippedBeams & ~scene.CollectedBeams) != SamusBeamFlags.None)
            throw new InvalidDataException("A demo switches on a beam Samus has not got.");
        data["equipment"] = equipment;
        if (off.Count > 0)
            data["off"] = off;
        int energy = scene.Samus.Setup is AttractDemoSamusSetup.Posed { Energy: { } low } ? low : scene.Health;
        data["capacities"] = new JsonObject
        {
            ["energy"] = scene.Health, ["missiles"] = scene.Missiles, ["super_missiles"] = scene.SuperMissiles, ["power_bombs"] = scene.PowerBombs,
        };
        data["amounts"] = new JsonObject
        {
            ["energy"] = energy, ["missiles"] = scene.Missiles, ["super_missiles"] = scene.SuperMissiles, ["power_bombs"] = scene.PowerBombs,
        };
        return data;
    }

    /// <summary>Writes the game over screen: its page, the larva crying over its egg, and the cursor beside each answer.</summary>
    private void WriteGameOver()
    {
        const string folder = "menus/game_over";
        InstalledScreenContent screens = _installation.LoadMaps();
        GameOverPresentation placed = screens.GameOver;

        // The larva is drawn in other colours as it cries: each look of it is a picture of its own.
        string Baby(GameOverBabyLook look)
        {
            string frame = GameOverPresentationNames.BabyFrameName(look.Frame);
            return _sheets.WriteSprite(
                _tree, SpriteSheetId.GameOver, frame, $"{folder}/baby_{Named(look.Frame)}_{Named(look.Colours)}.png", GameOverBabyShades.Colours(look.Colours).Key);
        }

        (List<IAnimationBeat<GameOverBabyLook, GameOverBabySound>> lead, List<IAnimationBeat<GameOverBabyLook, GameOverBabySound>>? cycle) =
            EngineTimelines.Played(GameOverBabyAnimation.Segments, GameOverBabyAnimation.Playing);
        JsonObject baby = EngineTimelines.Of(lead, cycle, Baby, static (GameOverBabySound cry) => EngineNames.SoundCue(GameOverMenuState.CrySound(cry)));
        (int _, int _, int babyX, int babyY) = _sheets.SpriteSize(SpriteSheetId.GameOver, GameOverPresentationNames.BabyFrameName(GameOverBabyFrame.Closed));
        foreach (GameOverBabyFrame frame in Enum.GetValues<GameOverBabyFrame>())
        {
            if (_sheets.SpriteSize(SpriteSheetId.GameOver, GameOverPresentationNames.BabyFrameName(frame)) is var (_, _, x, y) && (x != babyX || y != babyY))
                throw new InvalidDataException("The larva's pictures on the game over screen do not all lie alike about its place.");
        }

        baby["offset"] = At(-babyX, -babyY);

        (int _, int _, int eggX, int eggY) = _sheets.SpriteSize(SpriteSheetId.GameOver, GameOverPresentationDefinitions.EggFrame);
        var egg = new JsonObject
        {
            ["steps"] = new JsonArray(new JsonObject
            {
                ["picture"] = _sheets.WriteSprite(_tree, SpriteSheetId.GameOver, GameOverPresentationDefinitions.EggFrame, folder + "/egg.png"), ["updates"] = 1,
            }),
            ["loops_to"] = 0,
            ["offset"] = At(-eggX, -eggY),
        };

        EngineSheetPictures cursorPictures = _sheets.CutSprites(_tree, SpriteSheetId.GameOver, folder + "/cursor", MenuMissileFrames.Names);
        var cursorSteps = new JsonArray();
        foreach (string frame in MenuMissileFrames.Names)
            cursorSteps.Add(new JsonObject { ["picture"] = cursorPictures.Of(frame), ["updates"] = placed.CursorFrameDuration });
        var cursor = new JsonObject { ["steps"] = cursorSteps, ["loops_to"] = 0, ["offset"] = At(cursorPictures.OffsetX, cursorPictures.OffsetY) };

        _tree.WriteData("menus/game_over.json", new JsonObject
        {
            ["music"] = MusicName(GameOverMenuDefinitions.MenuSong),
            ["backdrop"] = Colour(screens.Backdrops.Menus),
            ["page"] = _sheets.WriteSprite(_tree, SpriteSheetId.MenuPictures, GameOverPresentationDefinitions.PageFrame, folder + "/page.png"),
            ["move_sound"] = EngineNames.Sound(SamusSound.MenuCursor),
            // Black before its set-up: the update that stops the sounds, and waiting that update out.
            ["entry_black_updates"] = 2,
            // It fades a level of brightness an update.
            ["fade_levels"] = SuperMetroid.Core.Rendering.DisplayBrightness.StepsFromBlackToFull,
            ["answer_hold_updates"] = GameOverMenuDefinitions.AcceptedAnswerHoldDuration,
            ["places"] = new JsonObject
            {
                ["baby"] = At(placed.BabyAnchor.X, placed.BabyAnchor.Y),
                ["cursor_yes"] = At(placed.CursorX, placed.YesCursorY),
                ["cursor_no"] = At(placed.CursorX, placed.NoCursorY),
            },
            ["timelines"] = new JsonObject { ["baby"] = baby, ["egg"] = egg, ["cursor"] = cursor },
        });
    }

}
