using System.Text.Json.Nodes;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>
/// The ending: the climb out through the clouds, the planet blowing apart, the flight away with the mission's result,
/// the credits, and Samus's reward and sign-off.
/// </summary>
public sealed partial class EngineAssetWriter
{
    /// <summary>The shade a sprite of the ending is drawn in: its own colours, as the stage of the ending it appears in has loaded them.</summary>
    private static PictureShade EndingShade(EndingSpriteColouring colouring, EndingColourLoad stage) =>
        EndingShades.Under(EndingShades.Drawn(colouring), EndingShades.Loaded(stage));

    /// <summary>Writes the ending's data file and everything it names.</summary>
    private void WriteEnding()
    {
        const string folder = "cinematics/ending";
        var timelines = new JsonObject();

        // A sprite of the ending's scenes: the clouds, the planet and what lies over it, the result's figures, Samus waiting.
        void Scene(string name, EndingSceneAnimation animation, SpriteSheetId sheet, CinematicSpriteFamily family, PictureShade shade) =>
            timelines[name] = SpriteTimeline(
                EndingSceneAnimations.Pictures.Segments, EndingSceneAnimations.Pictures.StartingWith(animation), sheet, family, $"{folder}/{name}", shade,
                static (EndingSceneCue cue) => cue switch
                {
                    EndingSceneCue.SpawnExplosionSilhouette => "silhouette",
                    EndingSceneCue.StartZebesExplosion => "burst",
                    EndingSceneCue.ExplosionFinale => "finale",
                    EndingSceneCue.EndZebesExplosion => "end",
                    // The Godot ending fades the planet's colours in from the start of its scene, and sets the
                    // stars waiting as the blast ends.
                    EndingSceneCue.FadeExplosionColours or EndingSceneCue.WaitForFlyaway => null,
                    _ => throw new InvalidDataException($"A sprite of the ending raises {cue}, which this does not know."),
                });

        void Explosion(string name, EndingSpriteDefinition sprite) => Scene(
            name, sprite.Animation, EndingSpriteSheets.Explosion, CinematicSpriteFamily.EndingExplosion, EndingShade(sprite.Colouring, EndingColourLoad.Explosion));
        void Cloud(string name, EndingSpriteDefinition sprite) => Scene(
            name, sprite.Animation, EndingSpriteSheets.Clouds, CinematicSpriteFamily.EndingCloud, EndingShade(sprite.Colouring, EndingColourLoad.Escape));

        Cloud("right_side_cloud", EndingActorDefinitions.SideCloudRightLeading);
        Cloud("left_side_cloud", EndingActorDefinitions.SideCloudLeftLeading);
        Cloud("top_cloud_pattern", EndingActorDefinitions.TopCloudPattern);
        Cloud("top_cloud_edge", EndingActorDefinitions.TopCloudEdge);
        Cloud("bottom_cloud_edge", EndingActorDefinitions.BottomCloudEdge);
        Cloud("bottom_cloud_pattern", EndingActorDefinitions.BottomCloudPattern);
        Explosion("exploding_planet", EndingActorDefinitions.ExplodingZebes);
        Explosion("explosion_lava", EndingActorDefinitions.ExplosionLava);
        Explosion("explosion_glow", EndingActorDefinitions.ExplosionGlow);
        Explosion("stars", EndingActorDefinitions.ExplosionStars);
        Explosion("explosion_silhouette", EndingActorDefinitions.ExplosionSilhouette);
        Explosion("explosion_stars", EndingActorDefinitions.ExplosionStarsRight);
        Explosion("explosion_afterglow", EndingActorDefinitions.ExplosionAfterglow);
        if (EndingActorDefinitions.ExplosionStarsLeft.Animation != EndingSceneAnimation.ExplosionLeftStars
            || EndingSceneAnimations.Pictures.Segments.Beats(EndingSceneAnimation.ExplosionLeftStars) is not [AnimationBeat<CinematicSpritePose, EndingSceneCue>.HeldFrame left]
            || EndingSceneAnimations.Pictures.Segments.Beats(EndingSceneAnimation.ExplosionStars) is not [AnimationBeat<CinematicSpritePose, EndingSceneCue>.HeldFrame still]
            || left.Shown != still.Shown)
            throw new InvalidDataException("The stars left of the blast are not the still stars behind the planet, which the Godot ending shows them as.");

        Scene(
            "animal_escape", EndingAnimalEscapeDefinitions.Animation, EndingSpriteSheets.Explosion, CinematicSpriteFamily.EndingAnimalEscape,
            EndingShade(EndingAnimalEscapeDefinitions.Colouring, EndingColourLoad.Explosion));
        PictureShade figures = EndingShade(EndingActorDefinitions.ClearTimeDigitColouring, EndingColourLoad.Explosion);
        for (int digit = 0; digit <= 9; digit++)
        {
            Scene(
                $"digit_{digit}", Enum.Parse<EndingSceneAnimation>($"{nameof(EndingSceneAnimation.ClearTimeDigit0)[..^1]}{digit}"), EndingSpriteSheets.Explosion,
                CinematicSpriteFamily.EndingCompletionText, figures);
        }

        Scene("colon", EndingActorDefinitions.ClearTimeColon.Animation, EndingSpriteSheets.Explosion, CinematicSpriteFamily.EndingCompletionText, figures);

        // Samus as the clear time has earned her being seen: waiting, making her gesture, and leaping.
        SpriteSheetId suitless = EndingSpriteSheets.Reward(EndingReward.Suitless), suited = EndingSpriteSheets.Reward(EndingReward.Armored);
        if (EndingSpriteSheets.Reward(EndingReward.Helmetless) != suited)
            throw new InvalidDataException("Samus in her suit is on two sheets, where the Godot ending's data is written from one.");
        void Idle(string name, EndingSpriteDefinition sprite, SpriteSheetId sheet) => Scene(
            name, sprite.Animation, sheet, CinematicSpriteFamily.EndingReward, EndingShade(sprite.Colouring, EndingColourLoad.PostCredits));
        Idle("suitless_idle", EndingActorDefinitions.SuitlessRewardBody, suitless);
        Idle("suitless_legs_idle", EndingActorDefinitions.SuitlessRewardLegs, suitless);
        Idle("suited_idle", EndingActorDefinitions.ArmoredRewardBody, suited);
        Idle("helmet_idle", EndingActorDefinitions.ArmoredRewardHead, suited);
        Idle("head_idle", EndingActorDefinitions.HelmetlessRewardHead, suited);

        void Gesture(string name, EndingRewardActor actor, SpriteSheetId sheet)
        {
            EndingRewardActorDefinition gesture = EndingRewardActors.Get(actor);
            timelines[name] = SpriteTimeline(
                EndingRewardAnimations.Gesture.Segments, EndingRewardAnimations.Gesture.StartingWith(gesture.Animation), sheet, CinematicSpriteFamily.EndingReward,
                $"{folder}/{name}", EndingShade(gesture.Start.Colouring, EndingColourLoad.PostCredits),
                static (EndingRewardGestureCue cue) => cue == EndingRewardGestureCue.StartJump ? "jump" : throw new InvalidDataException($"Samus's gesture raises {cue}, which this does not know."));
        }

        Gesture("hair_release", EndingRewardActor.SuitlessUpper, suitless);
        Gesture("hair_release_legs", EndingRewardActor.SuitlessLower, suitless);
        Gesture("gesture_body", EndingRewardActor.SuitedBody, suited);
        Gesture("gesture_arm", EndingRewardActor.SuitedArm, suited);
        Gesture("gesture_helmet", EndingRewardActor.HelmetedHead, suited);
        Gesture("gesture_head", EndingRewardActor.HelmetlessHead, suited);

        static string? JumpCue(EndingRewardJumpCue cue) => cue switch
        {
            EndingRewardJumpCue.PrepareHead => "ready_head",
            EndingRewardJumpCue.LaunchHead => "launch_head",
            EndingRewardJumpCue.Launch => "launch",
            EndingRewardJumpCue.StartBodyFlight => "fly",
            EndingRewardJumpCue.StartHeadFlight => "head_fly",
            EndingRewardJumpCue.Shoot => "shoot",
            _ => throw new InvalidDataException($"Samus's leap raises {cue}, which this does not know."),
        };
        void Jump(string name, EndingRewardActor actor, SpriteSheetId sheet)
        {
            EndingRewardActorDefinition leap = EndingRewardActors.Get(actor);
            timelines[name] = SpriteTimeline(
                EndingRewardAnimations.Jump.Segments, EndingRewardAnimations.Jump.StartingWith(leap.Animation), sheet, CinematicSpriteFamily.EndingReward,
                $"{folder}/{name}", EndingShade(leap.Start.Colouring, EndingColourLoad.PostCredits), JumpCue);
        }

        Jump("suitless_jump", EndingRewardActor.JumpSuitlessBody, suitless);
        Jump("suited_jump", EndingRewardActor.JumpSuitedBody, suited);
        Jump("helmet_jump", EndingRewardActor.JumpHelmetedHead, suited);
        Jump("head_jump", EndingRewardActor.JumpHelmetlessHead, suited);

        // She falls and lands in one set of colours and fires in another, which she takes as she fires.
        PictureShade falling = EndingShade(EndingRewardJump.FallingColouring, EndingColourLoad.PostCredits);
        PictureShade shooting = EndingShade(EndingRewardJump.ShootingColouring, EndingColourLoad.PostCredits);
        timelines["falling"] = SpriteTimeline(
            EndingRewardAnimations.Jump.Segments, EndingRewardAnimations.Jump.StartingWith(EndingRewardAnimation.Falling), EndingSpriteSheets.FallingSamusAndLogo,
            CinematicSpriteFamily.EndingReward, folder + "/falling", falling, JumpCue);
        JsonObject landing = SpriteTimeline(
            EndingRewardAnimations.Jump.Segments, EndingRewardAnimations.Jump.StartingWith(EndingRewardAnimation.Landing), EndingSpriteSheets.FallingSamusAndLogo,
            CinematicSpriteFamily.EndingReward, folder + "/landing", falling, JumpCue);
        JsonObject landingFiring = SpriteTimeline(
            EndingRewardAnimations.Jump.Segments, EndingRewardAnimations.Jump.StartingWith(EndingRewardAnimation.Landing), EndingSpriteSheets.FallingSamusAndLogo,
            CinematicSpriteFamily.EndingReward, folder + "/landing_firing", shooting, JumpCue);
        bool fired = false;
        JsonArray landingSteps = landing["steps"]!.AsArray(), firingSteps = landingFiring["steps"]!.AsArray();
        for (int step = 0; step < landingSteps.Count; step++)
        {
            fired |= landingSteps[step]!["cues"] is JsonArray cues && cues.Any(cue => cue!.GetValue<string>() == "shoot");
            if (fired)
                landingSteps[step]!["picture"] = firingSteps[step]!["picture"]!.GetValue<string>();
        }

        if (!fired)
            throw new InvalidDataException("Samus lands without firing, which the Godot ending waits for.");
        timelines["landing"] = landing;

        // Her emblem.
        PictureShade emblem = EndingShade(EndingLogoDefinitions.Colouring, EndingColourLoad.LogoInitial);
        (string Name, EndingLogoActorDefinition Part)[] parts =
        [
            ("logo_upper_s", EndingLogoDefinitions.UpperS), ("logo_lower_s", EndingLogoDefinitions.LowerS),
            ("logo_upper_circle", EndingLogoDefinitions.UpperCircle), ("logo_lower_circle", EndingLogoDefinitions.LowerCircle),
        ];
        foreach ((string name, EndingLogoActorDefinition part) in parts)
        {
            timelines[name] = SpriteTimeline(
                EndingLogoAnimations.Logo.Segments, EndingLogoAnimations.Logo.StartingWith(part.Animation), EndingSpriteSheets.FallingSamusAndLogo,
                CinematicSpriteFamily.EndingLogo, $"{folder}/{name}", emblem,
                static (EndingLogoCue cue) => cue == EndingLogoCue.GreyOut ? "grey_out" : throw new InvalidDataException($"The emblem raises {cue}, which this does not know."));
        }

        // The shooting stars, from faintest to fullest.
        EngineSheetPictures stars = _sheets.CutSprites(
            _tree, suited, folder + "/shooting_star", EndingShootingStarFrames.Visible.Select(EndingShootingStarFrames.SheetFrameName),
            EndingShade(EndingShootingStarDefinitions.Colouring, EndingColourLoad.PostCredits).Key);
        var starPictures = new JsonArray();
        foreach (EndingShootingStarLook look in EndingShootingStarFrames.Visible)
            starPictures.Add(stars.Of(EndingShootingStarFrames.SheetFrameName(look)));

        // The credits: every line laid where the roll has it, on one picture as long as the roll.
        string credits = folder + "/credits_roll.png";
        var roll = new RgbaPixel[256 * CreditsRollDefinitions.Length];
        foreach (CreditsRollLine line in CreditsRollLines.Lines)
        {
            (RgbaPixel[] pixels, int width, int height) = _sheets.SpritePixels(SpriteSheetId.EndingCreditsRows, line.Picture, EndingShades.Loaded(EndingColourLoad.Credits).Key);
            if (width != 256 || height != line.Height)
                throw new InvalidDataException($"The credits' line {line.Picture} is {width} by {height}, not 256 by {line.Height}.");
            pixels.CopyTo(roll.AsSpan(line.Top * 256));
        }

        _tree.WritePicture(credits, 256, CreditsRollDefinitions.Length, roll);

        string Whole(SpriteSheetId sheet, string frame, string name, EndingColourLoad stage) =>
            _sheets.WriteSprite(_tree, sheet, frame, $"{folder}/{name}.png", EndingShades.Loaded(stage).Key);
        string Floor(EndingFloorShot shot, string name, EndingColourLoad stage) => Whole(EndingFloorDefinitions.Sheets[shot], EndingFloorDefinitions.FrameName, name, stage);
        string Blast(EndingExplosionPicture picture, string name) =>
            Whole(SpriteSheetId.EndingExplosionPictures, EndingPictureNames.ExplosionPicture(picture), name, EndingColourLoad.ExplosionBackgrounds);
        string After(string frame, string name) => Whole(SpriteSheetId.EndingPostCreditsPictures, frame, name, EndingColourLoad.PostCredits);

        // The gunship's floor is the one picture the old game loads other colours for while it is shown: those the
        // explosion's hold restored as it is first seen, and the final gunship's once it has flown clear. It is written
        // as its sheet has it, with each of those as a change of colours on it.
        SpriteSheetId gunshipSheet = EndingFloorDefinitions.Sheets[EndingFloorShot.Flyaway];
        string gunshipFloor = _sheets.WriteSpriteWithRegions(_tree, gunshipSheet, EndingFloorDefinitions.FrameName, $"{folder}/gunship_floor.png");
        var gunshipPlaces = new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal) { [gunshipFloor] = [EngineSheets.SheetOf(gunshipSheet)] };
        JsonObject GunshipColours(EndingColourLoad stage) => ShadeChange(EndingShades.Loaded(stage).Key, EngineSheets.SpriteFamily, gunshipPlaces);

        var data = new JsonObject
        {
            ["cloud_floor"] = Floor(EndingFloorShot.SideCloudEscape, "cloud_floor", EndingColourLoad.Escape),
            ["top_and_bottom_cloud_floor"] = Floor(EndingFloorShot.TopAndBottomCloudEscape, "top_and_bottom_cloud_floor", EndingColourLoad.Escape),
            ["planet_floor"] = Floor(EndingFloorShot.PlanetExplosion, "planet_floor", EndingColourLoad.Explosion),
            ["gunship_floor"] = gunshipFloor,
            ["gunship_colours"] = new JsonObject
            {
                ["flying_out"] = GunshipColours(EndingColourLoad.ExplosionBackgrounds),
                ["clear"] = GunshipColours(EndingColourLoad.FinalGunship),
            },
            ["shot_floor"] = Floor(EndingFloorShot.RewardIcon, "shot_floor", EndingColourLoad.RestoredShot),
            ["planet_explosion"] = Blast(EndingExplosionDisplayDefinitions.PlanetExplosion, "planet_explosion"),
            ["burst_glow"] = Blast(EndingExplosionDisplayDefinitions.BurstGlow, "burst_glow"),
            ["finale_explosion"] = Blast(EndingExplosionDisplayDefinitions.FinaleExplosion, "finale_explosion"),
            ["finale_glow"] = Blast(EndingExplosionDisplayDefinitions.FinaleGlow, "finale_glow"),
            ["credits_roll"] = credits,
            ["credits_scroll"] = CreditsRollLines.FinishingScroll,
            ["waiting_backdrop"] = After(EndingPictureNames.WaitingBackdrop, "waiting_backdrop"),
            ["wavy_samus"] = Whole(SpriteSheetId.EndingWavySamusPicture, EndingPictureNames.WavySamus, "wavy_samus", EndingColourLoad.PostCredits),
            ["result_panel"] = After(EndingPictureNames.ResultPanel, "result_panel"),
            ["copyright_panel"] = After(EndingPictureNames.CopyrightPanel, "copyright_panel"),
            ["logo"] = After(EndingPictureNames.Logo, "logo"),
            ["shooting_star"] = starPictures,
            ["shooting_star_offset"] = At(stars.OffsetX, stars.OffsetY),
            ["shot_sound"] = EngineNames.Sound(EndingPostShotDefinitions.ShotSound),
            ["result_texts"] = EndingResultTexts(folder),
            ["last_words"] = EndingLastWords(folder),
            ["timelines"] = timelines,
        };
        _tree.WriteData("cinematics/ending.json", data);
    }

    /// <summary>
    /// The three lines of the mission's result: each one's words, the font they are typed in and where its first
    /// character's cell lies. The old game keeps each letter as a sprite set off from one place; they lie cell by
    /// cell, so the letters are a font and the place of the first is the line's.
    /// </summary>
    private JsonObject EndingResultTexts(string folder)
    {
        (EndingTextDefinition Text, string Font)[] lines =
        [
            (EndingActorDefinitions.OperationWasText, "completion_font"), (EndingActorDefinitions.CompletedSuccessfullyText, "completion_font"),
            (EndingActorDefinitions.ClearTimeText, "clear_time_font"),
        ];
        var texts = new JsonObject();
        foreach (IGrouping<string, (EndingTextDefinition Text, string Font)> font in lines.GroupBy(line => line.Font))
        {
            EndingSpriteColouring colouring = font.Select(line => line.Text.Colouring).Distinct().Single();
            var characters = new List<(char, string)>();
            foreach ((EndingTextDefinition text, _) in font)
            {
                foreach ((char character, string frame) in ResultLetters(text.Text))
                {
                    if (characters.All(listed => listed.Item1 != character) && _sheets.SpriteShows(EndingSpriteSheets.Explosion, frame))
                        characters.Add((character, frame));
                }
            }

            _sheets.WriteFont(_tree, $"{folder}/{font.Key}.json", EndingSpriteSheets.Explosion, characters, null, EndingShade(colouring, EndingColourLoad.Explosion).Key);
        }

        foreach ((EndingTextDefinition text, string font) in lines)
        {
            string spelling = EndingTypedTextDefinitions.Spellings[text.Text];
            (int _, int _, int originX, int originY) = _sheets.SpriteSize(EndingSpriteSheets.Explosion, ResultLetters(text.Text)[0].Frame);
            (int cell, int _, int _, int _) = _sheets.SpriteSize(EndingSpriteSheets.Explosion, ResultLetters(text.Text)[0].Frame);
            int first = spelling.TakeWhile(character => character == ' ').Count();
            texts[Words("ending_" + Named(text.Text), spelling)] = new JsonObject
            {
                ["font"] = $"{folder}/{font}.json", ["x"] = text.Position.X - originX - first * cell, ["y"] = text.Position.Y - originY,
            };
        }

        return texts;
    }

    /// <summary>Each letter of one of the result's lines, in order, with the frame that shows it.</summary>
    private static List<(char Character, string Frame)> ResultLetters(EndingTypedText text)
    {
        var letters = new List<(char, string)>();
        foreach (char character in EndingTypedTextDefinitions.Spellings[text].Where(character => character != ' '))
            letters.Add((character, EndingTypedTextDrawing.LetterFrameName(new EndingTextTyping(text, letters.Count + 1))));
        return letters;
    }

    /// <summary>
    /// Samus's last words, typed in the middle of the screen: the share of items found, with its figures' font and
    /// place, and her sign-off. Each gives its words, their font and the row of its first line.
    /// </summary>
    private JsonObject EndingLastWords(string folder)
    {
        EndingTypewriterText typed = _installation.LoadMaps().EndingText;
        const int cell = EndingResultTextLayout.GlyphPixels;
        string after = EndingShades.Loaded(EndingColourLoad.PostCredits).Key;
        var words = new JsonObject();
        foreach ((EndingTextSequence sequence, string name) in new[] { (EndingTextSequence.ItemPercentage, "ending_item_percentage"), (EndingTextSequence.FinalMessage, "ending_final_message") })
        {
            EndingTextCharacter[] characters = typed.CharactersOf(sequence).ToArray();
            EndingTextStyle style = characters.Select(character => character.Style).Distinct().Single();
            var text = new System.Text.StringBuilder();
            int row = characters[0].Row, column = -1;
            foreach (EndingTextCharacter character in characters)
            {
                if (character.Row != row)
                {
                    text.Append((char)10, character.Row - row);
                    (row, column) = (character.Row, -1);
                }
                else if (column >= 0 && character.Column != column + 1)
                {
                    throw new InvalidDataException($"The ending's words {sequence} skip from column {column} to {character.Column} in one line.");
                }

                column = character.Column;
                text.Append(character.Character);
            }

            // The Godot ending lays each line in the middle of the screen by whole cells, as the old game laid them.
            foreach (IGrouping<int, EndingTextCharacter> line in characters.GroupBy(character => character.Row))
            {
                int width = line.Count();
                if (line.Min(character => character.Column) != (256 / cell - width) / 2)
                    throw new InvalidDataException($"A line of the ending's words {sequence} is not in the middle of the screen.");
            }

            var glyphs = new List<(char, string)>();
            foreach (char character in text.ToString().Where(character => character != ' ' && character != (char)10).Distinct())
                glyphs.Add((character, EndingPictureNames.Glyph(style, character)));
            string font = $"{folder}/{EngineSheets.FileName(style.ToString())}_font.json";
            (int _, int glyphHeight, int _, int _) = _sheets.SpriteSize(SpriteSheetId.EndingPostCreditsPictures, glyphs[0].Item2);
            // Rows are counted in cells, whatever the height of the glyphs on them.
            _sheets.WriteFont(_tree, font, SpriteSheetId.EndingPostCreditsPictures, glyphs, glyphHeight == cell ? null : cell, after);
            words[Words(name, text.ToString())] = new JsonObject { ["font"] = font, ["y"] = characters[0].Row * cell };
        }

        var figures = new List<(char, string)>();
        for (int digit = 0; digit <= 9; digit++)
            figures.Add(((char)('0' + digit), EndingPictureNames.PercentDigit(digit)));
        figures.Add(('%', EndingPictureNames.PercentSign));
        _sheets.WriteFont(_tree, folder + "/percent_font.json", SpriteSheetId.EndingPostCreditsPictures, figures, null, after);
        words["percentage"] = new JsonObject
        {
            ["font"] = folder + "/percent_font.json",
            ["x"] = EndingResultTextLayout.PercentageHundredsCorner.X,
            ["y"] = EndingResultTextLayout.PercentageHundredsCorner.Y,
        };
        // In Japanese a line is shown under the share of items found while it is held.
        words["japanese_subtitle"] = new JsonObject
        {
            ["picture"] = _sheets.WriteSprite(_tree, SpriteSheetId.EndingPostCreditsPictures, EndingPictureNames.JapaneseSubtitle, folder + "/japanese_subtitle.png", after),
            ["x"] = EndingResultTextLayout.JapaneseSubtitleCorner.X,
            ["y"] = EndingResultTextLayout.JapaneseSubtitleCorner.Y,
            ["languages"] = new JsonArray(Languages[1]),
        };
        return words;
    }
}
