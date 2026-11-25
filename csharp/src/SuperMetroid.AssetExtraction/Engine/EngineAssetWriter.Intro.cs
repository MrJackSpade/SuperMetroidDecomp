using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>
/// The opening: the card, Samus's pages and her eye, the two flashbacks with their rooms and what she met in them,
/// the two scenes in the laboratory, and the flight to the colony.
/// </summary>
public sealed partial class EngineAssetWriter
{
    /// <summary>The scenes of the two rooms the opening's flashbacks are played in.</summary>
    private const string IntroMotherBrainRoomScene = "res://game/cinematics/intro_mother_brain_room.tscn",
        IntroDiscoveryRoomScene = "res://game/cinematics/intro_discovery_room.tscn";

    /// <summary>The entry point Samus starts a flashback at, as both rooms' scenes name it.</summary>
    private const string IntroRoomEntry = "Start";

    /// <summary>Writes the opening's data file and everything it names.</summary>
    private void WriteIntro()
    {
        const string folder = "cinematics/intro";
        IntroCinematicArtwork artwork = _installation.LoadIntroCinematic();
        IntroNarrationPresentation narration = _installation.LoadMaps().IntroNarration;
        PictureShade crossFaded = IntroShades.Drawn(IntroSpriteColouring.CrossFadeActors);
        var timelines = new JsonObject();
        var places = new JsonObject();

        // Her eye is a part of her portrait, which lies raised as the flashbacks' rooms do.
        const int raise = IntroCinematicState.GameplayFlashbackVerticalScroll;
        EngineSheetPictures eyes = _sheets.CutSprites(
            _tree, SpriteSheetId.IntroScenePictures, folder + "/eye", Enum.GetValues<IntroEyeFrame>().Select(IntroScenePictures.Eye));
        foreach ((string name, IntroEyeSegment segment) in new[] { ("eye", IntroEyeSegment.Blinking), ("eye_closing", IntroEyeSegment.FinalPageClosing) })
        {
            JsonObject eye = EngineTimelines.Of(IntroEyeAnimations.Segments, IntroEyeAnimations.StartingWith(segment), frame => eyes.Of(IntroScenePictures.Eye(frame)), NoCue);
            eye["offset"] = At(eyes.OffsetX, eyes.OffsetY);
            timelines[name] = eye;
        }

        places["eye"] = At(IntroEyeAnimationDefinitions.PortraitEyeCorner.X, IntroEyeAnimationDefinitions.PortraitEyeCorner.Y - raise);
        places["portrait"] = At(0, -raise);

        // The caret and the arrow that asks for the page to be turned.
        (string Name, IntroNarrationAnimation Animation, CinematicSpriteFamily Family)[] marks =
        [
            ("caret", IntroNarrationAnimation.Caret, CinematicSpriteFamily.IntroCaret),
            ("caret_blinking", IntroNarrationAnimation.CaretBlinking, CinematicSpriteFamily.IntroCaret),
            ("next_page_arrow", IntroNarrationAnimation.NextPageArrow, CinematicSpriteFamily.IntroNextPageArrow),
            ("next_page_arrow_delayed", IntroNarrationAnimation.NextPageArrowDelayed, CinematicSpriteFamily.IntroNextPageArrow),
        ];
        foreach ((string name, IntroNarrationAnimation animation, CinematicSpriteFamily family) in marks)
        {
            timelines[name] = SpriteTimeline(
                IntroNarrationAnimations.Narration.Segments, IntroNarrationAnimations.Narration.StartingWith(animation), SpriteSheetId.IntroSprites, family,
                $"{folder}/{name}", crossFaded, NoCue);
        }

        places["next_page_arrow"] = At(IntroCinematicDefinitions.NarrationLayout.NextPageArrowX, IntroCinematicDefinitions.NarrationLayout.NextPageArrowY);
        places["words"] = At(
            IntroNarrationDefinitions.FirstColumn * IntroCinematicDefinitions.NarrationLayout.CharacterPixelSize,
            IntroNarrationDefinitions.FirstTextRow * IntroCinematicDefinitions.NarrationLayout.CharacterPixelSize - raise);
        places["final_line"] = At(0, IntroNarrationPictures.FinalLineRow * IntroCinematicDefinitions.NarrationLayout.CharacterPixelSize - raise);
        if (IntroCinematicDefinitions.NarrationLayout.CaretLeftX != IntroNarrationDefinitions.FirstColumn * IntroCinematicDefinitions.NarrationLayout.CharacterPixelSize)
            throw new InvalidDataException("The opening's caret does not start where its words do, which the Godot game takes it to.");

        // The larva in its jar, in the two scenes in the laboratory.
        (string Name, IntroBabyActorDefinition Baby)[] larvae = [("delivered_baby", IntroBabyActorDefinitions.DeliveredBaby), ("examined_baby", IntroBabyActorDefinitions.ExaminedBaby)];
        foreach ((string name, IntroBabyActorDefinition baby) in larvae)
        {
            timelines[name] = SpriteTimeline(
                IntroBabyAnimations.Laboratory.Segments, IntroBabyAnimations.Laboratory.StartingWith(baby.Animation), SpriteSheetId.IntroSprites,
                CinematicSpriteFamily.IntroScientist, $"{folder}/{name}", IntroShades.Drawn(baby.Colouring),
                static (IntroLaboratoryCue cue) => cue == IntroLaboratoryCue.StartNextPage ? "next_page" : throw new InvalidDataException($"The larva in its jar raises {cue}, which this does not know."));
            places[name] = At(baby.Position.X, baby.Position.Y);
        }

        // The flight: the stars the gunship rushes through, and what is seen from behind it.
        timelines["stars"] = CeresTimeline(CeresFlightActorDefinitions.FrontStars.Animation, CeresFlightActorDefinitions.FrontStars.Colouring, SpriteSheetId.CeresFlightSprites, folder + "/stars");
        places["stars"] = At(CeresFlightActorDefinitions.FrontStarsStart.X, CeresFlightActorDefinitions.FrontStarsStart.Y);
        var rearView = new JsonArray();
        foreach (CeresRearViewActorRole role in CeresFlightActorDefinitions.RearViewSpawnOrder)
        {
            string name = Named(role);
            CeresFlightActorDefinition seen = CeresFlightActors.RearViewActor(role);
            timelines[name] = CeresTimeline(seen.Animation, seen.Colouring, SpriteSheetId.CeresFlightSprites, $"{folder}/{name}");
            places[name] = At(artwork.CeresFlight.Actors[role].X, artwork.CeresFlight.Actors[role].Y);
            if (seen.InitialVelocity != default)
                throw new InvalidDataException($"The {role} seen from behind the gunship starts with a speed, which the Godot game does not give it.");
            rearView.Add(new JsonObject { ["name"] = name, ["drift"] = EngineNames.Number(seen.HorizontalDelta), ["loops"] = seen.DriftsRoundLoop });
        }

        places["caption"] = At(SpaceColonyCaption.Left(0), SpaceColonyCaptionDefinitions.Top);
        places["caption_second"] = At(SpaceColonyCaptionDefinitions.JapaneseLeft, SpaceColonyCaptionDefinitions.JapaneseTop);

        // The pages: each one's words, the scene it turns to, and whether a second press is waited for where the
        // old game showed a second wording.
        var pages = new JsonArray();
        foreach (IntroNarrationPageId id in Enum.GetValues<IntroNarrationPageId>())
        {
            IntroNarrationPageDefinition page = IntroNarrationPageDefinitions.Pages[id];
            var entry = new JsonObject { ["text"] = Words("intro_" + Named(id), NarrationWords(narration.GetLines(id))) };
            if (page.TurnsTo is { } scene)
            {
                entry["scene"] = scene switch
                {
                    IntroIllustration.MotherBrainFlashback => "mother_brain",
                    IntroIllustration.BabyDiscovery => "discovery",
                    IntroIllustration.BabyDelivery => "delivery",
                    IntroIllustration.BabyExamination => "examination",
                    _ => throw new InvalidDataException($"A page of the opening turns to {scene}, which this does not know."),
                };
            }

            if (page.Closing == IntroNarrationPageClosing.AwaitsSecondSubtitle)
                entry["awaits_second_press"] = true;
            pages.Add(entry);
        }

        if (IntroNarrationPageDefinitions.Pages[Enum.GetValues<IntroNarrationPageId>()[^1]].Closing != IntroNarrationPageClosing.EndsIntro)
            throw new InvalidDataException("The opening's last page does not end it, which the Godot game takes it to.");

        string colony = SpaceColonyCaptionDefinitions.Text;
        var captionLetters = new List<(char, string)>();
        for (int letter = 0; letter < SpaceColonyCaptionDefinitions.LetterCount; letter++)
        {
            char character = colony[SpaceColonyCaption.TextIndex(letter)];
            if (captionLetters.All(listed => listed.Item1 != character))
                captionLetters.Add((character, CeresCinematicPictures.CaptionLetter(letter)));
        }

        _sheets.WriteFont(_tree, folder + "/caption_font.json", SpriteSheetId.CeresCinematicPictures, captionLetters);

        // Typed letters glow and settle; the font is the colour they settle to.
        _sheets.WriteFont(_tree, folder + "/narration_font.json", SpriteSheetId.IntroNarrationPictures, NarrationGlyphs(), null, IntroShades.Glow(IntroTextGlowStep.All[^1]).Key);

        // The colourings a letter glows through before it settles, brightest first, each its own font.
        var glowFonts = new JsonArray();
        foreach (IntroTextGlowStep step in IntroTextGlowStep.All.SkipLast(1))
        {
            string glowFont = $"{folder}/narration_font_glow_{step.Step}.json";
            _sheets.WriteFont(_tree, glowFont, SpriteSheetId.IntroNarrationPictures, NarrationGlyphs(), null, IntroShades.Glow(step).Key);
            glowFonts.Add(glowFont);
        }

        _tree.WriteData("cinematics/demos/intro_mother_brain.json", Flashback(
            DemoInputRecordings.Of(DemoSignallingRecordingId.IntroMotherBrainFlashback).Recording, IntroMotherBrainRoomScene));
        _tree.WriteData("cinematics/demos/intro_discovery_run.json", Flashback(DemoInputRecordings.Of(DemoInputRecordingId.IntroBabyDiscoveryRunningLeft), IntroDiscoveryRoomScene));
        _tree.WriteData("cinematics/demos/intro_discovery_look.json", Flashback(DemoInputRecordings.Of(DemoInputRecordingId.IntroBabyDiscoveryStopAndLook), IntroDiscoveryRoomScene));

        string Whole(SpriteSheetId sheet, string frame, string name) => _sheets.WriteSprite(_tree, sheet, frame, $"{folder}/{name}.png");
        var data = new JsonObject
        {
            ["backdrop"] = Colour(artwork.Backdrop),
            ["first_card"] = Whole(SpriteSheetId.IntroNarrationPictures, IntroNarrationPictures.FirstCard, "first_card"),
            ["final_line"] = Whole(SpriteSheetId.IntroNarrationPictures, IntroNarrationPictures.FinalLine, "final_line"),
            ["portrait"] = Whole(SpriteSheetId.IntroScenePictures, IntroScenePictures.Portrait, "portrait"),
            ["font"] = folder + "/narration_font.json",
            ["typing_sound"] = EngineNames.Sound(IntroCinematicDefinitions.Sounds.Typewriter),
            ["pages"] = pages,
            ["subtitled_languages"] = new JsonArray(Languages[1]),
            ["glow_fonts"] = glowFonts,
            // How long a letter shows each colouring of its glow: the old game's five updates, which is also how
            // long it takes to type the next.
            ["glow_step_updates"] = 5,
            ["mother_brain_demo"] = "cinematics/demos/intro_mother_brain.json",
            ["discovery_run_demo"] = "cinematics/demos/intro_discovery_run.json",
            ["discovery_look_demo"] = "cinematics/demos/intro_discovery_look.json",
            ["delivery_picture"] = Whole(SpriteSheetId.IntroScenePictures, IntroScenePictures.Page(IntroScenePage.ScientistDelivery), "delivery"),
            ["examination_picture"] = Whole(SpriteSheetId.IntroScenePictures, IntroScenePictures.Page(IntroScenePage.ScientistExamination), "examination"),
            ["flight_backdrop"] = Colour(artwork.CeresFlight.Backdrop),
            ["front_floor"] = Whole(SpriteSheetId.CeresFloor, CeresFloors.FrameName(CeresFloorShot.FlightFront), "front_floor"),
            ["rear_floor"] = Whole(SpriteSheetId.CeresFloor, CeresFloors.FrameName(CeresFloorShot.FlightRear), "rear_floor"),
            ["rear_view"] = rearView,
            ["caption"] = Words("intro_space_colony", colony),
            ["caption_font"] = folder + "/caption_font.json",
            ["caption_second_picture"] = Whole(SpriteSheetId.CeresCinematicPictures, CeresCinematicPictures.JapaneseCaption, "caption_second"),
            ["places"] = places,
            ["timelines"] = timelines,
        };
        _tree.WriteData("cinematics/intro.json", data);

        WriteIntroEgg();
        WriteIntroMotherBrain();
        WriteIntroRoom(
            "cinematics/intro_mother_brain_room.json", IntroScenePage.MotherBrainRoom, IntroMotherBrainCollisionDefinitions.Columns,
            IntroMotherBrainCollisionDefinitions.RoomRows, IntroMotherBrainCollision.CreateCollision());

        // The lights behind the fight, shown on that room's pictures: the colours each of their frames changes on the
        // sheet the room's picture was cut from.
        var lit = new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal)
        {
            ["cinematics/intro_mother_brain_room"] = [SpriteSheetDefinitions.FileNames[SpriteSheetId.IntroScenePictures]],
        };
        _tree.WriteData("cinematics/intro_mother_brain_room_lights.json",
            ColourSteps(IntroCinematicDefinitions.FightLights, shade => ShadeChange(shade, SpriteFamily, lit)));
        WriteIntroRoom(
            "cinematics/intro_discovery_room.json", IntroScenePage.BabyDiscoveryRoom, IntroBabyDiscoveryCollisionDefinitions.Columns,
            IntroBabyDiscoveryCollisionDefinitions.RoomRows, IntroBabyDiscoveryCollision.CreateCollision());
    }

    /// <summary>
    /// A page's words as the Godot game types them: line under line from the first row of text, a line feed for
    /// every row down to the next line.
    /// </summary>
    private static string NarrationWords(IReadOnlyList<IntroNarrationLine> lines)
    {
        var words = new System.Text.StringBuilder();
        int row = IntroNarrationDefinitions.FirstTextRow;
        foreach (IntroNarrationLine line in lines)
        {
            if (line.Row < row || (words.Length == 0 && line.Row != row))
                throw new InvalidDataException($"A line of the opening's words is on row {line.Row}, which does not follow from row {row}.");
            words.Append((char)10, line.Row - row).Append(line.Text);
            row = line.Row;
        }

        return words.ToString();
    }

    /// <summary>The timeline of something seen in space in the scenes about Ceres, in the colours it is drawn in.</summary>
    private JsonObject CeresTimeline(CeresAnimation animation, CeresSpriteColouring colouring, SpriteSheetId sheet, string folder, Func<CeresCue, string?>? cue = null)
    {
        HashSet<CinematicSpritePose> ofDestruction = [.. CeresDestructionSpriteDefinitions.Poses];
        return SpriteTimeline(
            CeresAnimations.Scenes.Segments, CeresAnimations.Scenes.StartingWith(animation), sheet,
            pose => ofDestruction.Contains(pose) ? CinematicSpriteFamily.CeresDestruction : CinematicSpriteFamily.CeresFlight, folder, CeresShades.Drawn(colouring),
            cue ?? (static raised => throw new InvalidDataException($"A sprite of the scenes about Ceres raises {raised}, which this does not know.")));
    }

    /// <summary>A flashback's recording, with what Samus carries in it.</summary>
    private static JsonObject Flashback(DemoRecording recording, string room)
    {
        JsonObject data = Recording(recording, room, IntroRoomEntry);
        data["amounts"] = new JsonObject { ["missiles"] = IntroCinematicDefinitions.Flashback.Missiles };
        data["selected"] = "missiles";
        return data;
    }

    /// <summary>Writes the egg Samus finds, the larva that hatches from it, and what flies from them.</summary>
    private void WriteIntroEgg()
    {
        const string file = "cinematics/intro_egg.json", folder = "cinematics/intro_egg";
        PictureShade drawn = IntroShades.Drawn(IntroBabyActorDefinitions.Egg.Colouring);
        JsonObject Discovery(IntroBabyAnimation animation, string name) => SpriteTimeline(
            IntroBabyAnimations.Discovery.Segments, IntroBabyAnimations.Discovery.StartingWith(animation), SpriteSheetId.IntroSprites,
            CinematicSpriteFamily.IntroDiscoveryActor, $"{folder}/{name}", drawn,
            static (IntroDiscoveryCue cue) => cue switch
            {
                IntroDiscoveryCue.SpawnShellPieces => "burst",
                IntroDiscoveryCue.StartNextPage => "next_page",
                // The Godot opening takes the egg away itself once the page has come.
                IntroDiscoveryCue.DeleteEggAfterCrossFade => null,
                _ => throw new InvalidDataException($"The egg raises {cue}, which this does not know."),
            });

        EngineSheetPictures shell = _sheets.CutSprites(
            _tree, SpriteSheetId.IntroSprites, folder + "/shell",
            IntroEggEffectDefinitions.ShellFragments.Select(fragment => CinematicSpriteSheet.FrameName(
                CinematicSpriteFamily.IntroEggEffect, ShellPose(fragment))),
            drawn.Key);
        var pieces = new JsonArray();
        foreach (IntroEggShellFragment fragment in IntroEggEffectDefinitions.ShellFragments)
            pieces.Add(shell.Of(CinematicSpriteSheet.FrameName(CinematicSpriteFamily.IntroEggEffect, ShellPose(fragment))));

        string slime = folder + "/slime.json", splash = folder + "/slime_splash.json";
        _tree.WriteData(slime, SpriteTimeline(
            IntroEggEffectAnimations.Effects.Segments, IntroEggEffectAnimations.Effects.StartingWith(IntroEggEffectAnimation.SlimeDrop), SpriteSheetId.IntroSprites,
            CinematicSpriteFamily.IntroEggEffect, folder + "/slime", drawn, NoCue));
        _tree.WriteData(splash, SpriteTimeline(
            IntroEggEffectAnimations.Effects.Segments, IntroEggEffectAnimations.Effects.StartingWith(IntroEggEffectAnimation.SlimeImpact), SpriteSheetId.IntroSprites,
            CinematicSpriteFamily.IntroEggEffect, folder + "/slime_splash", drawn, NoCue));

        new EngineObjectFile(_tree, file, "IntroEgg")
            .Value("hatch_sound", EngineNames.Sound(IntroCinematicDefinitions.Sounds.EggHatch))
            .Value("cry_sound", EngineNames.Sound(IntroCinematicDefinitions.Sounds.BabyHighCry))
            .Value("slime", slime)
            .Value("slime_splash", splash)
            .Group("shell", pieces)
            .Group("shell_offset", At(shell.OffsetX, shell.OffsetY))
            .Group("shell_curve_updates", IntroEggMotionDefinitions.FragmentCurveFrames)
            .Timeline("intact", Discovery(IntroBabyAnimation.Egg, "intact"))
            .Timeline("hatching", Discovery(IntroBabyAnimation.EggHatching, "hatching"))
            .Timeline("baby", Discovery(IntroBabyAnimation.ConfusedBaby, "baby"))
            .Write();
    }

    /// <summary>The picture of one of the egg's pieces of shell.</summary>
    private static CinematicSpritePose ShellPose(IntroEggShellFragment fragment)
    {
        HidingAnimationSegments<IntroEggShellFragment, CinematicSpritePose, NoAnimationCue> segments = IntroEggEffectAnimations.ShellPieces.Segments;
        return segments.Beats(fragment) is [AnimationBeat<CinematicSpritePose, NoAnimationCue>.HeldFrame held]
            ? held.Shown
            : throw new InvalidDataException($"The piece of shell {fragment} is not one still picture.");
    }

    /// <summary>Writes Mother Brain as the opening remembers her, her rings and her blasts.</summary>
    private void WriteIntroMotherBrain()
    {
        const string file = "cinematics/intro_mother_brain.json", folder = "cinematics/intro_mother_brain";
        JsonObject pulsing = SpriteTimeline(
            IntroMotherBrainAnimations.MotherBrain.Segments, IntroMotherBrainAnimations.MotherBrain.StartingWith(IntroMotherBrainDefinitions.MotherBrain.Animation),
            SpriteSheetId.IntroSprites, CinematicSpriteFamily.IntroMotherBrain, folder + "/pulsing", IntroMotherBrainSpriteState.Colouring,
            static (IntroMotherBrainCue cue) => throw new InvalidDataException($"Mother Brain pulsing raises {cue}, which this does not know."));

        string ring = folder + "/ring.json", small = folder + "/small_explosion.json", big = folder + "/big_explosion.json";
        _tree.WriteData(ring, SpriteTimeline(
            IntroRinkaAnimations.Rinka.Segments, IntroRinkaAnimations.Rinka.StartingWith(IntroRinkaDefinitions.RinkaActor.Animation), SpriteSheetId.IntroSprites,
            CinematicSpriteFamily.IntroRinka, folder + "/ring", IntroShades.Drawn(IntroSpriteColouring.DiscoveryActors),
            // The Godot Mother Brain sets a ring moving herself, when it has had the time to appear.
            static (IntroRinkaCue cue) => cue == IntroRinkaCue.StartMoving ? null : throw new InvalidDataException($"A ring raises {cue}, which this does not know.")));
        PictureShade blasts = IntroShades.Drawn(IntroSpriteColouring.Explosions);
        _tree.WriteData(small, SpriteTimeline(
            IntroMotherBrainAnimations.Explosions.Segments, IntroMotherBrainAnimations.Explosions.StartingWith(IntroMotherBrainDefinitions.SmallExplosionActor.Animation),
            SpriteSheetId.IntroSprites, CinematicSpriteFamily.IntroMotherBrainExplosion, folder + "/small_explosion", blasts, NoCue));
        _tree.WriteData(big, SpriteTimeline(
            IntroMotherBrainAnimations.Explosions.Segments, IntroMotherBrainAnimations.Explosions.StartingWith(IntroMotherBrainDefinitions.BigExplosionActor.Animation),
            SpriteSheetId.IntroSprites, CinematicSpriteFamily.IntroMotherBrainExplosion, folder + "/big_explosion", blasts, NoCue));

        // A missile strikes her as it crosses a line of the room, whatever its height: her hurtbox is everything
        // left of that line. She stands in nothing's way.
        int line = IntroMotherBrainDefinitions.MissileImpactLine, x = IntroMotherBrainDefinitions.MotherBrainOrigin.X, y = IntroMotherBrainDefinitions.MotherBrainOrigin.Y;
        int height = IntroMotherBrainCollisionDefinitions.RoomRows * EngineLayoutBlock;
        new EngineObjectFile(_tree, file, "IntroMotherBrain")
            .Value("ring", ring)
            .Value("small_explosion", small)
            .Value("big_explosion", big)
            .Collider("Hurtbox", -x, -y, line, height)
            .Collider("Body", 0, 0, 1, 1)
            .Timeline("pulsing", pulsing)
            .Write();
    }

    /// <summary>The width and height of one block of a room, in pixels.</summary>
    private const int EngineLayoutBlock = 16;

    /// <summary>
    /// Writes the layout of one of the flashbacks' rooms: the old game draws the room as one picture and gives it
    /// blocks only to stand on and run into, so each block of the layout shows its part of the picture and collides
    /// as the old block does. Blocks beyond the picture show nothing.
    /// </summary>
    private void WriteIntroRoom(string file, IntroScenePage page, int columns, int rows, SuperMetroid.Core.Samus.Movement.PlainTerrainBlock[] collision)
    {
        const int block = EngineLayoutBlock;
        string folder = file[..file.LastIndexOf('.')];
        (RgbaPixel[] picture, int width, int height) = _sheets.SpritePixels(SpriteSheetId.IntroScenePictures, IntroScenePictures.Page(page));
        if (collision.Length != columns * rows)
            throw new InvalidDataException($"The room of {page} has {collision.Length} blocks, not {columns} by {rows}.");

        var slope = new JsonArray();
        SlopeGeometry flat = RoomBlockBehavior.None.Slope;
        for (int column = 0; column < SlopeShape.Columns; column++)
            slope.Add(Math.Clamp(flat.Shape.HeightAt(flat.FlipsHorizontally ? SlopeShape.Columns - 1 - column : column), 0, block));

        var legend = new JsonObject();
        var rowsWritten = new JsonArray();
        char next = 'À';
        var tile = new RgbaPixel[block * block];
        for (int row = 0; row < rows; row++)
        {
            var letters = new System.Text.StringBuilder(columns);
            for (int column = 0; column < columns; column++)
            {
                var entry = new JsonObject();
                switch (collision[row * columns + column])
                {
                    case SuperMetroid.Core.Samus.Movement.PlainTerrainBlock.Air:
                        entry["kind"] = "empty";
                        break;
                    case SuperMetroid.Core.Samus.Movement.PlainTerrainBlock.Solid:
                        entry["kind"] = "solid";
                        break;
                    case SuperMetroid.Core.Samus.Movement.PlainTerrainBlock.Slope:
                        entry["kind"] = "slope";
                        entry["solid"] = slope.DeepClone();
                        if (flat.FlipsVertically)
                            entry["from"] = "top";
                        break;
                    default:
                        throw new InvalidDataException($"The room of {page} has a block that collides in a way this does not know.");
                }

                if ((column + 1) * block <= width && (row + 1) * block <= height)
                {
                    for (int y = 0; y < block; y++)
                        picture.AsSpan((row * block + y) * width + column * block, block).CopyTo(tile.AsSpan(y * block, block));
                    string path = $"{folder}/{column}_{row}.png";
                    _tree.WritePicture(path, block, block, tile);
                    entry["picture"] = path;
                }

                legend[next.ToString()] = entry;
                letters.Append(next);
                next++;
            }

            rowsWritten.Add(letters.ToString());
        }

        _tree.WriteData(file, new JsonObject { ["tile_size"] = block, ["tiles"] = legend, ["rows"] = rowsWritten });
    }
}
