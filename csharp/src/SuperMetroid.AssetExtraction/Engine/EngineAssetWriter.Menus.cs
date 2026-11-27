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
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.Scrolling;
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

        string headingFrame = CeresEscapeWarningPictures.Emergency.Picture + front;
        string heading = Whole(headingFrame, "text/emergency.png");
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
                // The station's click is asked for at the low priority, the planet's at the usual one.
                ["letter_sound"] = id == EscapeTypewriterMessageId.Ceres
                    ? EngineNames.Sound(EscapeTypewriterDefinitions.CeresClick, SuperMetroid.Core.Compatibility.SoundRequestPriority.Low)
                    : EngineNames.Sound(EscapeTypewriterDefinitions.ZebesClick, SuperMetroid.Core.Compatibility.SoundRequestPriority.Normal),
            };
            if (id == EscapeTypewriterMessageId.Zebes)
            {
                // The planet's second wording is one picture, put up where the old game pins it once the warning has
                // been typing as long as Mother Brain's wait before it.
                string frame = SuperMetroid.Core.Enemies.Artwork.EnemySheetDefinitions.ProjectileFrameName(
                    SuperMetroid.Core.Enemies.Projectiles.EnemyProjectileFrameId.MotherBrainEscapeSubtitle);
                EngineSheetPictures subtitle = _sheets.CutSpawned(_tree, $"{folder}/{name}_subtitle", EngineSheets.SharedSpawner, [frame]);
                JsonObject at = At(
                    SuperMetroid.Core.Bosses.MotherBrain.MotherBrainDeathDefinitions.SubtitleX + subtitle.OffsetX,
                    SuperMetroid.Core.Bosses.MotherBrain.MotherBrainDeathDefinitions.SubtitleY + subtitle.OffsetY);
                at["picture"] = subtitle.Of(frame);
                layout["second_wordings"] = new JsonObject
                {
                    ["zebes_time_bomb_subtitle"] = new JsonObject
                    {
                        ["languages"] = new JsonArray(Languages[1]), ["pictures"] = new JsonArray(at),
                        ["shown_after"] = SuperMetroid.Core.Bosses.MotherBrain.MotherBrainLateFightDefinitions.EscapeTextDelayUpdates,
                    },
                };
            }

            if (id == EscapeTypewriterMessageId.Ceres)
            {
                // Everything written on the station's warning flashes in the alarm's colours: its heading, its typed
                // letters and its second wording, each as its sheet has it.
                var flashing = new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal)
                {
                    [heading] = [SheetWith(headingFrame)],
                    [font + ".png"] = [fontSheet],
                };
                var lines = new JsonArray();
                foreach (CeresEscapeWarningPicture line in CeresEscapeWarningPictures.JapaneseSubtitle)
                {
                    JsonObject at = At(line.Corner.X, line.Corner.Y);
                    string picture = Whole(line.Picture + front, $"{folder}/{name}_{line.Name}.png");
                    at["picture"] = picture;
                    lines.Add(at);
                    flashing[picture] = [SheetWith(line.Picture + front)];
                }

                layout["second_wordings"] = new JsonObject
                {
                    ["ceres_self_destruct_subtitle"] = new JsonObject { ["languages"] = new JsonArray(Languages[1]), ["pictures"] = lines },
                };
                layout["alarm"] = CeresWarningAlarm(family, flashing);
            }

            _tree.WriteData($"{folder}/{Words(name, words)}.json", layout);
        }
    }

    /// <summary>
    /// The alarm of the station's warning as steps of colours on <paramref name="places"/>, one step for each step of the
    /// old alarm: the first shows its second colouring, and from then on it goes through all of them for good, its first
    /// after its last. Its owner steps it at the alarm's own pace.
    /// </summary>
    /// <exception cref="InvalidDataException">
    /// A sheet the places are cut from tells its pixels apart by region, which the warning's pictures are written without,
    /// or the alarm plays a beat that is not a colouring.
    /// </exception>
    private JsonObject CeresWarningAlarm(string family, IReadOnlyDictionary<string, IReadOnlyList<string>> places)
    {
        foreach (string sheet in places.Values.SelectMany(sheets => sheets).Distinct(StringComparer.Ordinal))
        {
            if (_sheets.HasRegions(family, sheet))
                throw new InvalidDataException($"The sheet {sheet} tells its pixels apart by region, and the station's warning is written without maps of regions.");
        }

        Animation<SuperMetroid.Core.Bosses.Ridley.RidleyFadeSegment> animation = SuperMetroid.Core.Bosses.Ridley.RidleyFadeAnimations.Alarm;
        var steps = new JsonArray();
        var firstStepOfPhase = new List<int>();
        foreach (AnimationPhase<SuperMetroid.Core.Bosses.Ridley.RidleyFadeSegment> phase in animation.Phases)
        {
            firstStepOfPhase.Add(steps.Count);
            var beats = SuperMetroid.Core.Bosses.Ridley.RidleyFadeAnimations.Segments.Beats(phase.Segment);
            for (int play = 0; play < phase.Plays; play++)
            {
                for (int beat = phase.FirstBeat; beat < phase.FirstBeat + phase.BeatCount; beat++)
                {
                    if (beats[beat] is not AnimationBeat<SuperMetroid.Core.Bosses.Ridley.RidleyFadeImage, NoAnimationCue>.Frame shown)
                        throw new InvalidDataException($"The station's warning's alarm plays a beat that is not a colouring: {beats[beat]}.");
                    steps.Add(new JsonObject
                    {
                        ["updates"] = shown.Duration,
                        ["colours"] = ShadeChange(SuperMetroid.Core.Bosses.Ridley.Artwork.RidleyShades.Fade(shown.Shown).Key, family, places),
                    });
                }
            }
        }

        var data = new JsonObject { ["steps"] = steps };
        if (animation.End is AnimationEnd.Loop loop)
            data["loops_to"] = firstStepOfPhase[loop.Phase];
        return data;
    }

    /// <summary>Writes the title screen as it rests, and the demos it shows when left alone.</summary>
    private void WriteTitle()
    {
        const string folder = "menus/title";
        InstalledScreenContent screens = _installation.LoadMaps();
        TitleFloorSceneDefinition resting = TitleSequenceDefinitions.Scenes.FinalZoom;
        if (TitleSequenceDefinitions.Scenes.Rotation != RotationAngle.Zero)
            throw new InvalidDataException("The title's backdrop is turned, which the Godot title does not turn it.");

        // The sets the title goes through, and those that follow them once the game has been completed: the old
        // playlist of a completed game is the ordinary one with more sets after it.
        IReadOnlyList<AttractDemoSet> ordinary = StockAttractDemoScenes.Playlist(completionSetUnlocked: false);
        IReadOnlyList<AttractDemoSet> completed = StockAttractDemoScenes.Playlist(completionSetUnlocked: true);
        if (completed.Count < ordinary.Count || !ordinary.SequenceEqual(completed.Take(ordinary.Count)))
            throw new InvalidDataException("The demos of a completed game are not the ordinary ones with more after them.");
        var sets = new JsonArray();
        var completedSets = new JsonArray();
        for (int index = 0; index < completed.Count; index++)
        {
            AttractDemoSet set = completed[index];
            var files = new JsonArray();
            for (int scene = 0; scene < set.Scenes.Count; scene++)
            {
                string file = $"menus/demos/{EngineSheets.FileName(set.Name)}_{scene + 1}.json";
                _tree.WriteData(file, Demo(set.Scenes[scene]));
                files.Add(file);
            }

            (index < ordinary.Count ? sets : completedSets).Add(files);
        }

        // The backdrop and the larva on it are written as their sheet has them, so that the console lights' changes
        // of colours meet them.
        string floor = _sheets.WriteSpriteWithRegions(_tree, SpriteSheetId.TitleFloor, TitleFloors.FloorFrameName, folder + "/floor.png");
        (JsonObject baby, IReadOnlyList<string> babyPictures) = TitleBaby(folder);

        // The title's backdrop comes to rest at its own size.
        _tree.WriteData("menus/title.json", new JsonObject
        {
            ["music"] = MusicName(TitleSequenceDefinitions.Music.ImmediateTitleTrack),
            ["backdrop"] = Colour(screens.TitleBackdrop),
            ["floor"] = floor,
            ["lights"] = TitleLights([floor, .. babyPictures]),
            ["floor_per_pixel"] = 1,
            ["floor_centre"] = At(TitleSequenceDefinitions.Scenes.FloorCentre.X, TitleSequenceDefinitions.Scenes.FloorCentre.Y),
            ["floor_scroll"] = At(resting.Position.X, resting.Position.Y),
            // At rest the title and the copyright show the colours their fades end on (the opening's "logo" and
            // "copyright"), which is how the old game restores them once a skip has rebuilt the title.
            ["opening"] = TitleOpening(folder),
            ["baby"] = baby,
            ["demo_after_updates"] = TitleSequenceDefinitions.Timing.TitleScreenIdleTimeoutFrames,
            ["demo_sets"] = sets,
            ["completed_demo_sets"] = completedSets,
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

        // The demo's room is loaded through a door, whose setup may set screens of the room; a demo's room is entered
        // by no door in the Godot game, so the demo says them itself.
        var screens = new JsonArray();
        void Sets(Position16<RoomScreens> screen, RoomScrollState state)
        {
            JsonObject set = At(screen.X, screen.Y);
            set["scroll"] = ScrollStateName(state);
            screens.Add(set);
        }

        if (!ReferenceEquals(scene.Door.Setup.RoomEvent, DoorRoomEvent.None))
            throw new InvalidDataException($"The door a demo's room {scene.Room.Name} is loaded through sets off an event, which the Godot demo has nothing for.");
        foreach (ScreenScrollChange change in scene.Door.Setup.ScreenChanges)
            Sets(change.Screen, change.State);

        // What a demo does to its room. One plays in the statues' hall with Kraid beaten: the old game counts him beaten
        // once the room is built, and nothing of the Godot hall is built by that fact, so it is so before the room is
        // entered. One closes a screen of its room to the view, and one has Kraid wait less before he rises.
        switch (scene.RoomSetup)
        {
            case null:
                break;
            case AttractDemoRoomSetup.DefeatedKraid:
                data["facts"] = new JsonArray(EngineSheets.FileName(nameof(SuperMetroid.Core.Progression.AreaBoss.Kraid)) + "_defeated");
                break;
            case AttractDemoRoomSetup.ChargeBeamScroll:
                Sets(AttractDemoDefinitions.ChargeBeamClosedScreen, RoomScrollState.Closed);
                break;
            case AttractDemoRoomSetup.KraidTimer:
                data["kraid_waits_updates"] = AttractDemoDefinitions.KraidPhaseWaitUpdates;
                break;
            default:
                throw new InvalidDataException($"A demo sets its room up by {scene.RoomSetup}, which the Godot demo has nothing for.");
        }

        if (screens.Count > 0)
            data["screens"] = screens;

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
