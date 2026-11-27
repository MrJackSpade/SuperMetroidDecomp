using System.Globalization;
using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Bosses.Torizo.Artwork;
using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.World.RoomStates;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>The two Torizos, the statue hand that holds the Bombs, and everything they throw.</summary>
public sealed partial class EngineAssetWriter
{
    /// <summary>The folder of the Godot project the Torizos' scenes are in.</summary>
    private const string TorizoScenes = "res://game/bosses/torizo/";

    /// <summary>The patch that makes the wall of Bomb Torizo's room blocks a shot breaks, during the escape.</summary>
    private const string WallShotBlocksPatch = "wall_shot_blocks";

    /// <summary>The fact recorded once the animals have a way out, as the Godot game's rooms and its ending name it.</summary>
    private const string AnimalsEscapedFact = "animals_escaped";

    /// <summary>Writes both Torizos, the hand, and the wall the animals escape through.</summary>
    private void WriteTorizos()
    {
        // What every plain thing a Torizo throws is: it has no timelines of its own to change to.
        var plain = new JsonObject();
        EngineDefaults.Fill(plain, "CuedProjectile");
        _tree.WriteData("bosses/torizo_shot.json", plain);

        WriteTorizo("bosses/bomb_torizo.json", EnemyDefinitionId.BombTorizo, TorizoVariant.Bomb, golden: false);
        WriteTorizo("bosses/golden_torizo.json", EnemyDefinitionId.GoldenTorizo, TorizoVariant.Golden, golden: true);
        WriteBombTorizoHand();

        // During the escape the old game makes a column of the wall of Bomb Torizo's room one block that any shot
        // breaks, for the animals to leave by.
        var wallRooms = EngineLayouts.States().Where(placed => placed.State.SetupBehavior == RoomSetupBehavior.TurnWallIntoShotBlocksDuringEscape)
            .Select(placed => placed.Room).Distinct().ToList();
        if (wallRooms.Count == 0)
            throw new InvalidDataException("No room turns its wall into shot blocks during the escape.");
        if (EscapeAnimalWallDefinitions.OriginCollisionType != RoomCollisionType.ShootableBlock || EscapeAnimalWallDefinitions.ExtensionCollisionType != RoomCollisionType.VerticalExtension)
            throw new InvalidDataException("The wall the animals escape through is not one block that shots meet and the blocks below that answer for it.");
        foreach ((RoomDefinition room, RoomStateSettings state) in EngineLayouts.States().Where(placed => wallRooms.Contains(placed.Room)))
        {
            // Its breaking is what lets the animals out: the old wall marks them escaped as its breaking ends. Every
            // shot and bomb breaks it; the grapple's touch does not (EscapeAnimalObjects). Struck, it is struck no more
            // and goes on standing: only its pictures change.
            var rules = new JsonObject
            {
                ["solid"] = true,
                ["stays_solid"] = true,
                ["broken_by"] = EngineLayouts.AnyShot(),
                ["records"] = AnimalsEscapedFact,
            };
            AddAnimalWallBreaking(rules, state);
            Layouts.PatchReactingBlock(room, state, WallShotBlocksPatch, new Position16<RoomBlocks>(EscapeAnimalWallDefinitions.WallX, EscapeAnimalWallDefinitions.WallY),
                EscapeAnimalWallDefinitions.HeightInBlocks, "shot_block_animal_escape", rules);
        }
    }

    /// <summary>
    /// Gives the animals' wall its breaking as the old wall plays it from the update it is struck: each of its pictures
    /// over its column, tile by tile, for as long as the old change shows it, and the crumble's sound, at the priority
    /// the change asks it at, with the first.
    /// </summary>
    /// <exception cref="InvalidDataException">The old change does not begin as the wall is struck, moves, lets something through or sounds later.</exception>
    private void AddAnimalWallBreaking(JsonObject rules, RoomStateSettings state)
    {
        TerrainChange change = ScriptedTerrainChangeDefinitions.CritterWallBreakthrough;
        if (change.FirstFrameDelay != 1)
            throw new InvalidDataException("The animals' wall does not show its first picture of breaking on the update it is struck.");
        var steps = new JsonArray();
        foreach (TerrainChangeFrame frame in change.Frames)
        {
            if (frame.MoveBefore != Offset16<RoomBlocks>.Zero)
                throw new InvalidDataException("A picture of the animals' wall breaking is drawn away from the wall.");
            if (frame.Draw is not RoomObjectFrameDraw<EscapeAnimalDraw> { Frame: var shown }
                || EscapeAnimalObjectDrawDefinitions.Shapes[shown].Cells.Any(cell => cell.Collision != RoomCollisionType.SolidBlock))
                throw new InvalidDataException($"The animals' wall draws {frame.Draw} as it breaks, which is not a picture of the wall standing solid.");
            if (frame.SoundBefore is { } sound)
            {
                if (steps.Count > 0)
                    throw new InvalidDataException("The animals' wall sounds after its first picture of breaking, which a block's breaking cannot say.");
                rules["break_sound"] = EngineNames.Sound(sound, frame.SoundPriority);
            }

            steps.Add(new JsonObject
            {
                ["pictures"] = Layouts.ColumnPictures(state, GameInstallationLayout.RoomObjectRevealAndWallVisualDirectoryName,
                    RoomObjectRevealAndWallVisualFiles.VisualId(frame.Draw), EscapeAnimalWallDefinitions.HeightInBlocks),
                ["updates"] = frame.Updates,
            });
        }

        rules["breaking"] = new JsonObject { ["steps"] = steps };
    }

    /// <summary>The name the Godot Torizo plays one of the old animations by.</summary>
    private static string TorizoTimeline(TorizoAnimation animation) => EngineSheets.FileName(animation.ToString());

    /// <summary>The name the Godot Torizo knows one of the looks of its body by.</summary>
    private static string TorizoLookName(string key) => EngineSheets.FileName(key);

    /// <summary>What one of the old Torizo's events becomes.</summary>
    private static string? TorizoCueOf(TorizoCue cue)
    {
        static string Side(HorizontalDirection side) => EngineNames.Side(side);
        switch (cue)
        {
            case TorizoCue.Branch branch:
                return "branch:" + branch.Kind switch
                {
                    TorizoBranchKind.BombJumpBackFromSamusInFront or TorizoBranchKind.GoldenJumpBackFromSamusInFront => "jump_back",
                    TorizoBranchKind.GoldenSitOnSamusMorphedBehind => "sit",
                    TorizoBranchKind.GoldenLeapAtDistantSamus => "leap",
                    _ => throw new InvalidDataException($"A Torizo's animation branches by {branch.Kind}, which this does not know."),
                };
            case TorizoCue.Interrupt interrupt:
                return "interrupt:" + interrupt.Kind switch
                {
                    TorizoInterruptKind.BombSwipeAtSamusBehind => "swipe",
                    TorizoInterruptKind.GoldenStaggerWhenStunned => "stagger",
                    TorizoInterruptKind.GoldenEyeBeamAtMediumRange => "eye_beam",
                    TorizoInterruptKind.GoldenEggsAtLowHealth => "eggs",
                    _ => throw new InvalidDataException($"A Torizo's animation breaks off for {interrupt.Kind}, which this does not know."),
                } + ":" + TorizoTimeline(interrupt.Animation);
            case TorizoCue.Choice { Kind: TorizoChoiceKind.LandFacelessOrGolden } landed:
                return $"landed:{TorizoTimeline(landed.Target)}:{TorizoTimeline(landed.Alternate)}";
            case TorizoCue.Choice { Kind: TorizoChoiceKind.BombAttack or TorizoChoiceKind.GoldenAttack } attack:
                return $"attack:{TorizoTimeline(attack.Target)}:{TorizoTimeline(attack.Alternate)}";
            case TorizoCue.LandIn landIn:
                return "land_in:" + TorizoTimeline(landIn.Landing);
            case TorizoCue.WalkOn walkOn:
                return "play:" + TorizoTimeline(walkOn.Walking);
            case TorizoCue.Turn turn:
                return $"turn:{TorizoTimeline(turn.WhenFacingRight)}:{TorizoTimeline(turn.WhenFacingLeft)}";
            case TorizoCue.Posture { Kind: TorizoPostureKind.StandUp } stand:
                return $"shift:{stand.Displacement.X}:{stand.Displacement.Y}";
            case TorizoCue.Posture { Kind: TorizoPostureKind.SitDown } sit:
                return $"shift:{-sit.Displacement.X}:{-sit.Displacement.Y}";
            // Which Torizo walks, and so how it turns about and how long it waits to, the Godot Torizo knows itself.
            case TorizoCue.Walk walk:
                return "walk:" + walk.Velocity.ToString(CultureInfo.InvariantCulture);
            case TorizoCue.Swipe swipe:
                return $"swipe:{swipe.Hand.X}:{swipe.Hand.Y}";
            case TorizoCue.LowHealthExplosions explosions:
                return "explosions:" + explosions.Site switch
                {
                    BombTorizoExplosionSite.Gut => "gut",
                    BombTorizoExplosionSite.Face => "face",
                    _ => throw new InvalidDataException($"A Torizo's {explosions.Site} blows up, which this does not know."),
                };
            case TorizoCue.SetPhase phase:
                return "stage:" + phase.Phase switch
                {
                    TorizoPhase.Idle => "idle",
                    TorizoPhase.Falling => "falling",
                    TorizoPhase.WaitForHandTrigger => "wait_for_hand",
                    TorizoPhase.Active => "active",
                    TorizoPhase.GoldenWaitForSamus => "golden_wait",
                    TorizoPhase.GoldenActive => "golden_active",
                    _ => throw new InvalidDataException($"A Torizo's animation sets the stage {phase.Phase}, which this does not know."),
                };
            case TorizoCue.ShowArtwork artwork:
                return "look:" + TorizoLookName(TorizoArtworkLookKeys.Installed.Of(artwork.Look));
            case TorizoCue.Stepped stepped:
                return $"stepped:{Side(stepped.Direction)}:{Side(stepped.Foot)}";
            case TorizoCue.Signal signal:
                return signal.Kind switch
                {
                    TorizoCueKind.StartWalking => "motion:walking",
                    TorizoCueKind.StartStanding => "motion:standing",
                    TorizoCueKind.StartLeaping => "motion:leaping",
                    TorizoCueKind.StopMoving => "motion:still",
                    TorizoCueKind.BlowUpGut => "blow_up_gut",
                    TorizoCueKind.MarkFaceBlownUp => "face_blown_up",
                    TorizoCueKind.MakeVisible => "show",
                    TorizoCueKind.MakeInvisible => "hide",
                    TorizoCueKind.FadeToBlack => "fade:" + TorizoLookKindKeys.Installed.Of(TorizoLookKind.Black),
                    TorizoCueKind.FadeToNormalColors => "fade:" + TorizoLookKindKeys.Installed.Of(TorizoLookKind.Normal),
                    TorizoCueKind.FadeToGoldenColors => "fade:" + TorizoLookKindKeys.Installed.Of(TorizoLookKind.Golden),
                    TorizoCueKind.AdvanceColourFade => "advance_fade",
                    TorizoCueKind.FinishDeath => "finish_death",
                    TorizoCueKind.StartBombTorizoFight or TorizoCueKind.StartGoldenTorizoFight => "start_fight",
                    TorizoCueKind.LockAnimation => "lock",
                    TorizoCueKind.UnlockAnimation => "unlock",
                    TorizoCueKind.ResumeWalk => "resume_walk",
                    TorizoCueKind.ResumeInterrupted => "resume_interrupted",
                    TorizoCueKind.SpawnDeathExplosion => "death_explosion",
                    TorizoCueKind.SpawnLandingDust => "landing_dust",
                    TorizoCueKind.SpawnLowHealthDrool => "drool",
                    TorizoCueKind.SetTurningAround => "turning_around",
                    TorizoCueKind.SpawnBombTorizoChozoOrbs or TorizoCueKind.SpawnGoldenTorizoChozoOrbs => "orbs",
                    TorizoCueKind.SpawnBombTorizoSonicBoom or TorizoCueKind.SpawnGoldenTorizoSonicBoom => "sonic_boom",
                    TorizoCueKind.SpawnEyeBeam => "eye_beam",
                    TorizoCueKind.SpawnEgg => "egg",
                    TorizoCueKind.SpawnSuperMissile => "super_missile",
                    TorizoCueKind.ClearCaughtSuperMissile => "clear_caught",
                    TorizoCueKind.DisableEyeBeamExplosions => "eye_beam_flames:off",
                    TorizoCueKind.EnableEyeBeamExplosions => "eye_beam_flames:on",
                    TorizoCueKind.UnmarkStunned => "unstun",
                    TorizoCueKind.LeapingPoseShown => "rising",
                    TorizoCueKind.DroppingPoseShown => "dropping",
                    TorizoCueKind.ChestHeldOpen => "chest_held_open",
                    // The sounds the old Torizo asks for by name as each of these comes.
                    TorizoCueKind.PlayShotSound => EngineNames.SoundCue(EnemySound.ShotTorizo),
                    TorizoCueKind.PlayFootstepSound => EngineNames.SoundCue(EnemySound.ChozoFootsteps),
                    TorizoCueKind.PlayEggReleasedSound => EngineNames.SoundCue(EnemySound.LowThump),
                    TorizoCueKind.PlayLaserSound => EngineNames.SoundCue(EnemySound.Laser),
                    TorizoCueKind.PlaySonicBoomSound => EngineNames.SoundCue(EnemySound.TorizoSonicBoom),
                    _ => throw new InvalidDataException($"A Torizo's animation raises {signal.Kind}, which this does not know."),
                };
            default:
                throw new InvalidDataException($"A Torizo's animation raises {cue}, which this does not know.");
        }
    }

    /// <summary>
    /// What one of the events of something a Torizo throws becomes. <paramref name="smoke"/> is the timeline a death's
    /// explosion sometimes turns to.
    /// </summary>
    private static string? TorizoShotCue(TorizoProjectileCue cue, string? smoke) => cue switch
    {
        TorizoProjectileCue.PlaySound sound => EngineNames.SoundCue(sound.Sound, sound.Priority),
        TorizoProjectileCue.Scatter scatter => $"scatter:{scatter.Spread.Span.X}:{scatter.Spread.Span.Y}:{scatter.Spread.Center.X}:{scatter.Spread.Center.Y}",
        TorizoProjectileCue.SetRadius radius => $"size:{radius.Radius.X * 2}:{radius.Radius.Y * 2}",
        TorizoProjectileCue.Signal signal => signal.Kind switch
        {
            TorizoProjectileCueKind.StopMoving => "stop",
            TorizoProjectileCueKind.StartDroolFall or TorizoProjectileCueKind.StartFragmentFall => "fall",
            TorizoProjectileCueKind.StartEggCharge => "charge",
            TorizoProjectileCueKind.Hatch => "hatch",
            TorizoProjectileCueKind.ThrowSuperMissile => "throw",
            TorizoProjectileCueKind.AimSuperMissileTowardSamus => "aim:right",
            TorizoProjectileCueKind.AimSuperMissileAwayFromSamus => "aim:left",
            TorizoProjectileCueKind.Delete => "end",
            TorizoProjectileCueKind.ReturnToOrigin => "origin",
            TorizoProjectileCueKind.RaiseDust => $"shift:{TorizoProjectilePlacementDefinitions.DustRise.X}:{TorizoProjectilePlacementDefinitions.DustRise.Y}",
            TorizoProjectileCueKind.StopHarmingSamus => "harmless",
            TorizoProjectileCueKind.DrawInFront => "front",
            TorizoProjectileCueKind.PersistOnContactAndDrawInFront => "stays" + EngineTimelines.And + "front",
            TorizoProjectileCueKind.DrawBehindSamus => "behind",
            TorizoProjectileCueKind.StopHarmingSamusAndDrawInFront => "harmless" + EngineTimelines.And + "front",
            TorizoProjectileCueKind.TurnHarmful => "harmful",
            TorizoProjectileCueKind.TurnHarmfulAndStopBlockingShots => "harmful" + EngineTimelines.And + "shots_pass",
            TorizoProjectileCueKind.StopBlockingShots => "shots_pass",
            TorizoProjectileCueKind.BlockShots => "shots_meet",
            TorizoProjectileCueKind.MayTurnToSmoke => "sometimes:" + (smoke ?? throw new InvalidDataException("Something a Torizo throws may turn to smoke, and no smoke is written for it.")),
            TorizoProjectileCueKind.FloorImpactWaited => "floor_flames",
            // What one shot down leaves is in its shot.
            TorizoProjectileCueKind.SpawnOrbDrops => null,
            _ => throw new InvalidDataException($"Something a Torizo throws raises {signal.Kind}, which this does not know."),
        },
        _ => throw new InvalidDataException($"Something a Torizo throws raises {cue}, which this does not know."),
    };

    /// <summary>
    /// Writes one timeline of something a Torizo throws, with pictures cut from the sheets of what
    /// <paramref name="spawner"/> spawns into a folder named as the file is, and returns the file. One that
    /// <paramref name="startsHarmless"/> is harmless from its first update, as the old projectile is made, and one
    /// that <paramref name="startsPassedByShots"/> is passed by Samus's shots from its first update, until its
    /// timelines have them meet it.
    /// </summary>
    private string TorizoShotTimeline(
        string file, string spawner, Animation<TorizoProjectileSegment> animation, string? smoke = null, bool startsHarmless = false, bool startsPassedByShots = false)
    {
        (List<IAnimationBeat<EnemyProjectileFrameId, TorizoProjectileCue>> lead, List<IAnimationBeat<EnemyProjectileFrameId, TorizoProjectileCue>>? cycle) =
            EngineTimelines.Played(TorizoProjectileAnimations.Segments, animation);
        IEnumerable<string> frames = lead.Concat(cycle ?? []).Select(beat => beat.ShownFrame).Where(frame => frame.HasValue)
            .Select(frame => EnemySheetDefinitions.ProjectileFrameName(frame!.Value));
        EngineSheetPictures pictures = _sheets.CutSpawned(_tree, file[..file.LastIndexOf('.')], spawner, frames);
        JsonObject timeline = EngineTimelines.Of(lead, cycle, frame => pictures.Of(EnemySheetDefinitions.ProjectileFrameName(frame)), cue => TorizoShotCue(cue, smoke));
        timeline["offset"] = new JsonObject { ["x"] = pictures.OffsetX, ["y"] = pictures.OffsetY };
        if (startsHarmless || startsPassedByShots)
        {
            JsonObject first = timeline["steps"]![0]!.AsObject();
            var cues = new JsonArray();
            if (startsHarmless)
                cues.Add("harmless");
            if (startsPassedByShots)
                cues.Add("shots_pass");
            foreach (JsonNode? cue in first["cues"]?.AsArray() ?? [])
                cues.Add(cue!.GetValue<string>());
            first["cues"] = cues;
        }

        _tree.WriteData(file, timeline);
        return file;
    }

    /// <summary>The data file of a projectile that is a scene of its own: the values its type reads, and the timelines it changes to.</summary>
    private void TorizoProjectileData(string file, string type, JsonObject timelines, params ReadOnlySpan<(string Name, JsonNode? Value)> values)
    {
        var data = new JsonObject();
        EngineDefaults.Fill(data, type);
        foreach ((string name, JsonNode? value) in values)
        {
            if (!data.ContainsKey(name))
                throw new InvalidDataException($"The data file {file} is given the value {name}, which its type does not read by that name.");
            data[name] = value;
        }

        if (timelines.Count > 0)
            data["timelines"] = timelines;
        _tree.WriteData(file, data);
    }

    /// <summary>Writes one Torizo: its values, its colours, every animation of its body as a timeline, and its shots.</summary>
    private void WriteTorizo(string file, EnemyDefinitionId id, TorizoVariant variant, bool golden)
    {
        const string family = GameInstallationLayout.EnemySheetDirectoryName;
        EngineEnemy torizo = Enemy(file, "Torizo", id);
        string sheet = _sheets.EnemySheet(EnemyDefinitionIdKeys.Installed.Of(id));
        string spawner = EnemyDefinitionIdKeys.Installed.Of(id);
        string artwork = EncounterArtworkKeys.Installed.Of(variant.Artwork);

        // A frame's picture. Most frames are kept once for each look of the body: those are named with the look left
        // open, and every look must name its picture alike.
        var frameOf = new Dictionary<string, EnemyFrameId>(StringComparer.Ordinal);
        var looks = new SortedSet<string>(StringComparer.Ordinal);
        string Picture(EnemyFrameId frame)
        {
            string name = EnemySheetDefinitions.FrameName(frame);
            string picture;
            if (torizo.Pictures.Paths.TryGetValue(name, out string? plainPicture))
            {
                picture = plainPicture;
            }
            else
            {
                string prefix = $"{name}|{artwork}=";
                string? shared = null;
                foreach ((string key, string path) in torizo.Pictures.Paths.Where(entry => entry.Key.StartsWith(prefix, StringComparison.Ordinal)))
                {
                    string look = TorizoLookName(key[prefix.Length..]);
                    string ending = $"_{look}.png";
                    if (!path.EndsWith(ending, StringComparison.Ordinal))
                        throw new InvalidDataException($"The Torizo's picture {path} is not named for its look, {look}.");
                    string open = path[..^ending.Length] + "_{look}.png";
                    if (shared is not null && shared != open)
                        throw new InvalidDataException($"The Torizo's pictures {shared} and {open} are not one picture named for each look.");
                    shared = open;
                    looks.Add(look);
                }

                picture = shared ?? throw new InvalidDataException($"The sheet {sheet} has no frame {name}, with a look or without.");
            }

            if (picture.Length > 0)
                frameOf[picture] = frame;
            return picture;
        }

        // The part of it that can be touched and shot goes with the frame it shows: a cue wherever that changes.
        static string Box(EnemyFrameId frame)
        {
            IReadOnlyList<EnemyCollisionHitbox> boxes = TorizoCollision.HitboxesAt(frame).Rectangles;
            if (boxes.Count == 0)
                return "box:off";
            if (boxes.Count != 1)
                throw new InvalidDataException($"The Torizo's frame {frame} has {boxes.Count} parts that can be hit, where the Godot Torizo has one.");
            EnemyCollisionHitbox box = boxes[0];
            bool sitting = box.Shot == TorizoReactions.StandUpSitDownShot;
            // The box is written as a shot meets it; her touch meets it a pixel down and right of that.
            JsonObject shot = EngineEnemy.ShotRect(box);
            return $"box:{shot["left"]}:{shot["top"]}:{shot["width"]}:{shot["height"]}:{(sitting ? 1 : 0)}";
        }

        foreach ((TorizoAnimation animation, Animation<TorizoBodySegment> played) in TorizoAnimations.StartingAt)
        {
            // Each Torizo has the animations named for it and those named for neither.
            string named = animation.ToString();
            bool goldenOnly = named.StartsWith("Golden", StringComparison.Ordinal), bombOnly = named.StartsWith("BombTorizo", StringComparison.Ordinal);
            if ((goldenOnly && !golden) || (bombOnly && golden))
                continue;
            JsonObject timeline = EngineTimelines.Of(TorizoAnimations.Segments, played, Picture, TorizoCueOf);
            string? box = null;
            foreach (JsonNode? step in timeline["steps"]!.AsArray())
            {
                string picture = step!["picture"]!.GetValue<string>();
                if (picture.Length == 0)
                    continue;
                string now = Box(frameOf[picture]);
                if (now == box)
                    continue;
                box = now;
                var cues = new JsonArray(now);
                foreach (JsonNode? cue in step["cues"]?.AsArray() ?? [])
                    cues.Add(cue!.GetValue<string>());
                step["cues"] = cues;
            }

            torizo.Timeline(TorizoTimeline(animation), timeline);
        }

        // Its colours, as changes of the colours of its own pictures: each settled colouring of its body and belly laid
        // over the colours both Torizos share, as the old game shows them; the golden one's by its health; the glowing
        // colours of its belly as each colouring a fade runs toward shows them; and the steps of its belly's glow. The
        // Godot Torizo works out its fades between them itself.
        var places = new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal) { [torizo.Folder] = [sheet] };
        JsonObject Colouring(TorizoLook look) => ShadeChange($"{TorizoShades.Shared.Key}+{TorizoShades.Body(look).Key}", family, places);
        var colourings = new JsonObject();
        foreach (TorizoLookKind kind in new[] { variant.StatueColouring.Kind, TorizoLookKind.Normal, TorizoLookKind.Golden, TorizoLookKind.White, TorizoLookKind.Black })
            colourings[TorizoLookKindKeys.Installed.Of(kind)] = Colouring(new TorizoLook(kind));
        torizo.Group("colourings", colourings);
        if (golden)
        {
            torizo.Group("health_colourings", EngineNames.List(GoldenTorizoHealthBandDefinitions.HighestFirst.Select(band => (JsonNode)new JsonObject
            {
                ["health"] = band.MinimumHealth,
                ["colours"] = Colouring(TorizoLook.GoldenHealth(band)),
            })));
        }

        // Only the colourings his awakenings and his death fade toward have glowing colours of their own.
        var glowingBellies = new JsonObject();
        foreach (TorizoLookKind kind in new[] { TorizoLookKind.Normal, TorizoLookKind.Golden, TorizoLookKind.Black })
            glowingBellies[TorizoLookKindKeys.Installed.Of(kind)] = ShadeChange(TorizoShades.GlowingBelly(new TorizoLook(kind)).Key, family, places);
        torizo.Group("glowing_bellies", glowingBellies);
        torizo.Group("belly_glow", ColourSteps(
            golden ? SuperMetroid.Core.Fx.ColourEffects.ColourEffectId.GoldenTorizoBelly : SuperMetroid.Core.Fx.ColourEffects.ColourEffectId.BombTorizoBelly,
            shade => ShadeChange(shade, family, places)));

        // The look it starts with: the one its first animation shows, or the one its pictures are loaded in.
        string startLook = TorizoAnimations.Segments.Beats(TorizoAnimations.StartingWith(variant.Start.Animation).Phases[0].Segment)
            .OfType<AnimationBeat<EnemyFrameId, TorizoCue>.Event>().Select(raised => raised.Raised).OfType<TorizoCue.ShowArtwork>()
            .Select(shown => TorizoArtworkLookKeys.Installed.Of(shown.Look)).FirstOrDefault(EncounterLook.LoadedName);
        if (!looks.Contains(TorizoLookName(startLook)))
            throw new InvalidDataException($"The Torizo of {file} starts with the look {startLook}, which its sheet has no pictures of.");

        TorizoLaunch orb = golden ? TorizoRandomizedProjectileDefinitions.GoldenChozoOrb : TorizoRandomizedProjectileDefinitions.BombChozoOrb;
        Extent16<RoomPixels> body = variant.Start.Radius;
        TorizoDropArena arena = variant.DropArena;
        static JsonObject Field(RandomSampleField field) => new() { ["from"] = field.Shift, ["digits"] = field.Width };
        torizo
            .Collider("Body", -body.X, -body.Y, body.X * 2, body.Y * 2)
            .Collider("Solid", -body.X, -body.Y, body.X * 2, body.Y * 2)
            .Value("start_timeline", TorizoTimeline(variant.Start.Animation))
            .Value("awakening_timeline", TorizoTimeline(golden ? TorizoAnimation.GoldenTorizoAwakening : TorizoAnimation.BombTorizoAwakening))
            .Value("start_look", TorizoLookName(startLook))
            .Value("start_colouring", TorizoLookKindKeys.Installed.Of(variant.StatueColouring.Kind))
            .Value("gut_health", TorizoDecisionDefinitions.GutBlowUpHealth)
            .Value("face_health", TorizoDecisionDefinitions.FaceBlowUpHealth)
            .Value("first_attack_missiles", variant.FirstAttackMissiles)
            .Value("turn_delay", variant.Walker.TurnDelay)
            // The golden one spews one orb at each cue.
            .Value("orbs_per_burst", golden ? 1 : TorizoChozoOrbDefinitions.BombTorizoOrbsPerBurst)
            .Value("orb_speed_across", EngineNames.Number(orb.BaseVelocity.X))
            .Value("orb_speed_up", EngineNames.Number(-orb.BaseVelocity.Y))
            .Value("close_in_front", TorizoDecisionDefinitions.CloseInFrontDistance)
            .Value("close_behind", TorizoDecisionDefinitions.CloseBehindDistance)
            .Value("eggs_health", TorizoDecisionDefinitions.GoldenEggsHealth)
            .Value("stagger_health", TorizoDecisionDefinitions.GoldenStaggerHealth)
            .Value("space_jump_updates", TorizoDecisionDefinitions.SpaceJumpLeapFrames)
            .Value("repeated_jump_steps", TorizoDecisionDefinitions.RepeatedJumpSteps)
            .Value("death_drops", TorizoDeathDefinitions.DeathDrops)
            .Group("scattered_drops", Drops(EnemyDrops.Rolling(TorizoDeathDefinitions.DeathDropChances))
                ?? throw new InvalidDataException("A Torizo scatters nothing when it is gone."))
            .Group("drop_scatter", new JsonObject
            {
                ["x"] = (int)arena.Corner.X,
                ["y"] = (int)arena.Corner.Y,
                ["across"] = Field(arena.HorizontalScatter),
                ["down"] = Field(arena.VerticalScatter),
            });
        if (golden)
        {
            Extent16<RoomPixels> fight = TorizoDecisionDefinitions.GoldenFightRadius;
            torizo.Group("fight_body", new JsonObject { ["left"] = -fight.X, ["top"] = -fight.Y, ["width"] = fight.X * 2, ["height"] = fight.Y * 2 });
        }

        // It leaves nothing of its own as it goes: what it scatters is all, and it blows apart in its own time.
        torizo.Data.Remove("drops");
        torizo.Data.Remove("death");

        string folder = torizo.Folder;
        string Timeline(string name, TorizoProjectileSegment segment, string? smoke = null, bool startsHarmless = false, bool startsPassedByShots = false) =>
            TorizoShotTimeline($"{folder}/{name}.json", spawner, TorizoProjectileAnimations.StartingAt[segment], smoke, startsHarmless, startsPassedByShots);

        // What every one of these is: a projectile of its own class that moves itself, shown where its timeline puts it.
        void Thrown(JsonObject shot, JsonObject projectile, string scene, string timeline)
        {
            shot["scene"] = TorizoScenes + scene;
            projectile["timeline"] = timeline;
            projectile["picture_offset"] = EngineEnemy.NoOffset();
            projectile["meeting_terrain"] = "passes_through";
            // One made harmless is made so by its timeline, which can make it harmful again.
            projectile.Remove("delivers_hit");
        }

        static bool Harmless(RoomEnemyProjectileKind kind) =>
            EnemyProjectileDefinitionCatalog.Definitions[kind].Properties.HasFlag(EnemyProjectilePropertyFlags.HarmlessToSamus);

        // The orbs: an orb shot down bursts and may leave something.
        RoomEnemyProjectileKind orbKind = golden ? RoomEnemyProjectileKind.GoldenTorizoChozoOrb : RoomEnemyProjectileKind.BombTorizoChozoOrb;
        string orbPrefix = golden ? "golden_torizo_" : "torizo_";
        TorizoProjectileData($"bosses/{orbPrefix}orb.json", "TorizoOrb",
            new JsonObject
            {
                ["wall_impact"] = Timeline("orb_wall_impact", TorizoProjectileSegment.ChozoOrbWallImpact),
                ["floor_impact"] = Timeline("orb_floor_impact", TorizoProjectileSegment.ChozoOrbFloorImpact),
            },
            ("gravity", EngineNames.Number(golden ? TorizoMotionDefinitions.GoldenOrbGravity : TorizoMotionDefinitions.BombOrbGravity)),
            ("bounces", golden),
            ("bounce_drag", EngineNames.Number(TorizoMotionDefinitions.GoldenOrbBounceDrag)));
        Timeline("orb_left", TorizoProjectileSegment.ChozoOrbMovingLeft, startsHarmless: Harmless(orbKind));
        Timeline("orb_right", TorizoProjectileSegment.ChozoOrbMovingRight, startsHarmless: Harmless(orbKind));
        // Shot down, it bursts where it is: the burst is the orb still, which stops shots no longer, harms to the touch,
        // and leaves what it drops once it is over.
        string orbShot = EffectOfTorizoShot($"{folder}/orb_shot.json", spawner, TorizoProjectileSegment.ChozoOrbShot);
        torizo.Shot("orb", orbKind, (_, shot, projectile) =>
        {
            Thrown(shot, projectile, $"{orbPrefix}orb.tscn", $"{folder}/orb_{{side}}.json");
            projectile["shot_timeline"] = orbShot;
            if (Drops(EnemyDrops.Rolling(golden ? TorizoChozoOrbDefinitions.GoldenTorizoOrbDropChances : TorizoChozoOrbDefinitions.BombTorizoOrbDropChances)) is { } drops)
                projectile["drops"] = drops;
        });

        // The sonic booms.
        RoomEnemyProjectileKind boomKind = golden ? RoomEnemyProjectileKind.GoldenTorizoSonicBoom : RoomEnemyProjectileKind.BombTorizoSonicBoom;
        TorizoProjectileData($"bosses/{orbPrefix}sonic_boom.json", "TorizoSonicBoom",
            new JsonObject { ["wall_impact"] = Timeline("sonic_boom_wall_impact", TorizoProjectileSegment.SonicBoomWallImpact) },
            ("quickening", EngineNames.Number(TorizoMotionDefinitions.SonicBoomAcceleration)));
        Timeline("sonic_boom_left", TorizoProjectileSegment.SonicBoomFiredLeft, startsHarmless: Harmless(boomKind));
        Timeline("sonic_boom_right", TorizoProjectileSegment.SonicBoomFiredRight, startsHarmless: Harmless(boomKind));
        torizo.Shot("sonic_boom", boomKind, (_, shot, projectile) =>
        {
            Thrown(shot, projectile, $"{orbPrefix}sonic_boom.tscn", $"{folder}/sonic_boom_{{side}}.json");
            shot["speed"] = EngineNames.Number(TorizoMotionDefinitions.SonicBoomLaunchSpeed);
        });

        // The drops that fall from one that is hurt.
        TorizoProjectileData($"bosses/{orbPrefix}drool.json", "TorizoDrool",
            new JsonObject
            {
                ["wall_impact"] = Timeline("drool_wall_impact", TorizoProjectileSegment.DroolWallImpact),
                ["floor_impact"] = Timeline("drool_floor_impact", TorizoProjectileSegment.DroolFloorImpact),
            },
            ("gravity", EngineNames.Number(TorizoMotionDefinitions.DroolAndMissileGravity)),
            ("drag", EngineNames.Number(TorizoMotionDefinitions.DroolDrag)),
            ("drift", EngineNames.Number(TorizoMotionDefinitions.DroolPastRestDrift)));
        string droolFalling = Timeline("drool", TorizoProjectileSegment.DroolNoDelay, startsHarmless: Harmless(RoomEnemyProjectileKind.TorizoInitialDrool));
        torizo.Shot("initial_drool", RoomEnemyProjectileKind.TorizoInitialDrool, (_, shot, projectile) => Thrown(shot, projectile, $"{orbPrefix}drool.tscn", droolFalling));

        // What bursts where it is, and the dust of its landing: plain projectiles their timelines drive.
        string smoke = Timeline("death_smoke", TorizoProjectileSegment.DeathSmoke);
        string deathExplosion = Timeline("death_explosion", TorizoProjectileSegment.DeathExplosion, smoke, Harmless(RoomEnemyProjectileKind.TorizoDeathExplosion));
        torizo.Shot("death_explosion", RoomEnemyProjectileKind.TorizoDeathExplosion, (_, shot, projectile) => Thrown(shot, projectile, "torizo_shot.tscn", deathExplosion));
        foreach ((string side, TorizoProjectileSegment segment) in new[] { ("left", TorizoProjectileSegment.LandingDustLeftFoot), ("right", TorizoProjectileSegment.LandingDustRightFoot) })
        {
            string dust = Timeline($"landing_dust_{side}", segment, startsHarmless: Harmless(RoomEnemyProjectileKind.TorizoLandingDust));
            torizo.Shot($"landing_dust_{side}", RoomEnemyProjectileKind.TorizoLandingDust, (_, shot, projectile) => Thrown(shot, projectile, "torizo_shot.tscn", dust));
        }

        if (!golden)
        {
            // The gobs from its broken gut: one of three, by how long each hangs before it falls.
            IReadOnlyList<Animation<TorizoProjectileSegment>> forms = [.. EngineNames.Each(TorizoRandomDefinitions.LowHealthDroolForm).Distinct()];
            if (forms.Count != 3)
                throw new InvalidDataException($"A Torizo's gut throws gobs of {forms.Count} kinds, where the Godot Torizo throws three.");
            for (int form = 0; form < forms.Count; form++)
            {
                string gob = TorizoShotTimeline($"{folder}/drool_{form}.json", spawner, forms[form], startsHarmless: Harmless(RoomEnemyProjectileKind.BombTorizoLowHealthDrool));
                torizo.Shot($"drool_{form}", RoomEnemyProjectileKind.BombTorizoLowHealthDrool, (_, shot, projectile) => Thrown(shot, projectile, "torizo_drool.tscn", gob));
            }

            string swipe = Timeline("swipe", TorizoProjectileSegment.ExplosiveSwipe, startsHarmless: Harmless(RoomEnemyProjectileKind.BombTorizoExplosiveSwipe));
            torizo.Shot("swipe", RoomEnemyProjectileKind.BombTorizoExplosiveSwipe, (_, shot, projectile) => Thrown(shot, projectile, "torizo_shot.tscn", swipe));
            string explosion = Timeline("low_health_explosion", TorizoProjectileSegment.LowHealthExplosion,
                startsHarmless: Harmless(RoomEnemyProjectileKind.BombTorizoLowHealthExplosion));
            torizo.Shot("low_health_explosion", RoomEnemyProjectileKind.BombTorizoLowHealthExplosion,
                (_, shot, projectile) => Thrown(shot, projectile, "torizo_shot.tscn", explosion));
        }
        else
        {
            // The eggs of its chest.
            TorizoProjectileData("bosses/torizo_egg.json", "TorizoEgg",
                new JsonObject
                {
                    ["hatching_left"] = Timeline("egg_hatching_left", TorizoProjectileSegment.EggHatchingLeft),
                    ["hatching_right"] = Timeline("egg_hatching_right", TorizoProjectileSegment.EggHatchingRight),
                    ["hatched_left"] = Timeline("egg_hatched_left", TorizoProjectileSegment.EggHatchedLeft),
                    ["hatched_right"] = Timeline("egg_hatched_right", TorizoProjectileSegment.EggHatchedRight),
                    ["break_left"] = Timeline("egg_break_left", TorizoProjectileSegment.EggBreakLeft),
                    ["break_right"] = Timeline("egg_break_right", TorizoProjectileSegment.EggBreakRight),
                },
                ("hatch_updates", TorizoMotionDefinitions.EggHatchUpdates),
                ("quickening", EngineNames.Number(TorizoMotionDefinitions.EggAcceleration)),
                ("bounce_drag", EngineNames.Number(TorizoMotionDefinitions.EggBounceDrag)));
            // The old egg is made not stopping her shots, and its hatching has them meet it: a hatchling can be shot.
            if (EnemyProjectileDefinitionCatalog.Definitions[RoomEnemyProjectileKind.GoldenTorizoEgg].Properties.HasFlag(EnemyProjectilePropertyFlags.BlocksShots))
                throw new InvalidDataException("The Golden Torizo's egg stops shots from the start, which its timelines are written not to.");
            Timeline("egg_left", TorizoProjectileSegment.EggBouncingLeft, startsHarmless: Harmless(RoomEnemyProjectileKind.GoldenTorizoEgg), startsPassedByShots: true);
            Timeline("egg_right", TorizoProjectileSegment.EggBouncingRight, startsHarmless: Harmless(RoomEnemyProjectileKind.GoldenTorizoEgg), startsPassedByShots: true);
            // Shot down, it bursts where it is as an orb bursts on a wall, harming to the touch, and leaves nothing.
            string eggShot = EffectOfTorizoShot($"{folder}/egg_shot.json", spawner, TorizoProjectileSegment.ChozoOrbWallImpact);
            torizo.Shot("egg", RoomEnemyProjectileKind.GoldenTorizoEgg, (_, shot, projectile) =>
            {
                Thrown(shot, projectile, "torizo_egg.tscn", $"{folder}/egg_{{side}}.json");
                projectile["met_by_shot"] = "destroyed";
                projectile["shot_timeline"] = eggShot;
            });

            // The super missile it catches and throws back.
            string heldRight = Timeline("super_missile_held_right", TorizoProjectileSegment.SuperMissileRight, startsHarmless: Harmless(RoomEnemyProjectileKind.GoldenTorizoSuperMissile));
            string impact = Timeline("super_missile_impact", TorizoProjectileSegment.SuperMissileImpact);
            TorizoProjectileData("bosses/torizo_super_missile.json", "TorizoSuperMissile",
                new JsonObject
                {
                    ["held_right"] = heldRight,
                    ["held_left"] = Timeline("super_missile_held_left", TorizoProjectileSegment.SuperMissileLeft, startsHarmless: Harmless(RoomEnemyProjectileKind.GoldenTorizoSuperMissile)),
                    ["impact"] = impact,
                },
                ("throw_speed", (int)TorizoMotionDefinitions.ThrownSuperMissileSpeed.Pixels),
                ("gravity", EngineNames.Number(TorizoMotionDefinitions.DroolAndMissileGravity)));
            // Shot down, held or thrown, it blows up where it is as it does on meeting a wall, harming to the touch.
            torizo.Shot("super_missile", RoomEnemyProjectileKind.GoldenTorizoSuperMissile, (_, shot, projectile) =>
            {
                Thrown(shot, projectile, "torizo_super_missile.tscn", heldRight);
                projectile["shot_timeline"] = impact;
            });

            // The beams of its eyes.
            string beam = Timeline("eye_beam", TorizoProjectileSegment.EyeBeam, startsHarmless: Harmless(RoomEnemyProjectileKind.GoldenTorizoEyeBeam));
            TorizoProjectileData("bosses/torizo_eye_beam.json", "TorizoEyeBeam",
                new JsonObject
                {
                    ["wall_impact"] = Timeline("eye_beam_wall_impact", TorizoProjectileSegment.EyeBeamWallImpact),
                    ["floor_impact"] = Timeline("eye_beam_floor_impact", TorizoProjectileSegment.EyeBeamFloorImpact),
                    ["floor_flames"] = Timeline("eye_beam_floor_flames", TorizoProjectileSegment.EyeBeamFloorFlames),
                });
            torizo.Shot("eye_beam", RoomEnemyProjectileKind.GoldenTorizoEyeBeam, (_, shot, projectile) =>
            {
                Thrown(shot, projectile, "torizo_eye_beam.tscn", beam);
                shot["speed"] = EngineNames.Number(TorizoMotionDefinitions.EyeBeamSpeed);
            });
        }

        torizo.Write();
    }

    /// <summary>
    /// Writes the timeline of something a Torizo throws as an effect left in the room: it shows itself and asks for its
    /// sounds, and that is all an effect does. Returns the file.
    /// </summary>
    private string EffectOfTorizoShot(string file, string spawner, TorizoProjectileSegment segment)
    {
        (List<IAnimationBeat<EnemyProjectileFrameId, TorizoProjectileCue>> lead, List<IAnimationBeat<EnemyProjectileFrameId, TorizoProjectileCue>>? cycle) =
            EngineTimelines.Played(TorizoProjectileAnimations.Segments, TorizoProjectileAnimations.StartingAt[segment]);
        if (cycle is not null)
            throw new InvalidDataException($"The Torizo's {segment} never ends, which an effect left in a room must.");
        EngineSheetPictures pictures = _sheets.CutSpawned(_tree, file[..file.LastIndexOf('.')], spawner,
            lead.Select(beat => beat.ShownFrame).Where(frame => frame.HasValue).Select(frame => EnemySheetDefinitions.ProjectileFrameName(frame!.Value)));
        JsonObject timeline = EngineTimelines.Of(lead, null, frame => pictures.Of(EnemySheetDefinitions.ProjectileFrameName(frame)),
            static (TorizoProjectileCue cue) => cue is TorizoProjectileCue.PlaySound sound ? EngineNames.SoundCue(sound.Sound, sound.Priority) : null);
        timeline["offset"] = new JsonObject { ["x"] = pictures.OffsetX, ["y"] = pictures.OffsetY };
        _tree.WriteData(file, timeline);
        return file;
    }

    /// <summary>
    /// Writes the statue hand that holds the Bombs: the steps of its crumbling, the pieces it throws, and what of it
    /// Samus can stand on.
    /// </summary>
    private void WriteBombTorizoHand()
    {
        const string directory = GameInstallationLayout.RoomObjectBombTorizoHandVisualDirectoryName;
        const string file = "bosses/bomb_torizo_hand.json";
        string folder = file[..file.LastIndexOf('.')];
        static string Frame(BombTorizoHandDraw draw) => RoomObjectBombTorizoHandVisualFiles.VisualId(draw);

        // The picture of one of its two frames; the cleared hand shows nothing at all.
        string Picture(BombTorizoHandDraw draw)
        {
            RoomBlockShape shape = BombTorizoHandDraws.Family.Shape(draw);
            bool blank = shape.Cells.All(cell => Layouts.FrameBlock(directory, Frame(draw), cell.Cell.X, cell.Cell.Y) is null);
            return blank ? "" : RoomObjects.Picture(directory, Frame(draw), folder);
        }

        (int left, int top, int _, int _) = RoomObjects.Bounds(directory, Frame(BombTorizoHandCrumbleDefinitions.RestingImage));

        // What of the intact hand is solid: the old game's blocks of it, row by row, as its fingers above and its palm below.
        RoomBlockShape intact = BombTorizoHandDraws.Family.Shape(BombTorizoHandCrumbleDefinitions.RestingImage);
        if (intact.Cells.Any(cell => cell.Collision != RoomCollisionType.SolidBlock))
            throw new InvalidDataException("The hand that holds the Bombs is not all solid.");
        var runs = new List<(int Left, int Top, int Columns, int Rows)>();
        foreach (IGrouping<int, RoomBlockShapeCell> row in intact.Cells.GroupBy(cell => (int)cell.Cell.Y).OrderBy(row => row.Key))
        {
            int first = row.Min(cell => (int)cell.Cell.X), last = row.Max(cell => (int)cell.Cell.X);
            if (row.Count() != last - first + 1)
                throw new InvalidDataException("A row of the hand that holds the Bombs has a gap in it.");
            if (runs.Count > 0 && runs[^1] is var above && above.Left == first && above.Columns == last - first + 1 && above.Top + above.Rows == row.Key)
                runs[^1] = above with { Rows = above.Rows + 1 };
            else
                runs.Add((first, row.Key, last - first + 1, 1));
        }

        if (runs.Count != 2)
            throw new InvalidDataException($"The hand that holds the Bombs is {runs.Count} solid pieces, where its Godot scene has its fingers and its palm.");

        // Each piece it throws: where it breaks off, and the shot that is the piece.
        EnemyProjectileDefinition piece = EnemyProjectileDefinitionCatalog.Definitions[RoomEnemyProjectileKind.BombTorizoStatueBreaking];
        var fragments = new JsonObject();
        foreach ((BombTorizoStatueFragment fragment, BombTorizoStatueFragmentDefinition thrown) in BombTorizoStatueFragmentDefinitions.Fragments)
        {
            string name = EngineSheets.FileName(fragment.ToString());
            string timeline = TorizoShotTimeline($"{folder}/{name}_timeline.json", EngineSheets.SharedSpawner, TorizoProjectileAnimations.StartingAt[thrown.Animation],
                startsHarmless: piece.Properties.HasFlag(EnemyProjectilePropertyFlags.HarmlessToSamus));
            string shot = $"{folder}/{name}.json";
            _tree.WriteData(shot, new JsonObject
            {
                ["damage"] = piece.Damage,
                ["scene"] = TorizoScenes + "torizo_fragment.tscn",
                ["projectile"] = new JsonObject
                {
                    ["width"] = Math.Max(1, piece.Radius.X * 2),
                    ["height"] = Math.Max(1, piece.Radius.Y * 2),
                    ["meeting_target"] = piece.Properties.HasFlag(EnemyProjectilePropertyFlags.PersistsOnContact) ? "passes_through_and_rehits" : "ends",
                    ["met_by_shot"] = piece.Properties.HasFlag(EnemyProjectilePropertyFlags.BlocksShots) ? "destroyed" : "ignored",
                    ["meeting_terrain"] = "passes_through",
                    ["view_margin"] = -1,
                    ["timeline"] = timeline,
                    ["picture_offset"] = EngineEnemy.NoOffset(),
                },
            });
            fragments[name] = new JsonObject { ["x"] = (int)thrown.Offset.X, ["y"] = (int)thrown.Offset.Y, ["shot"] = shot };
        }

        if (BombTorizoStatueFragmentDefinitions.InitialFallSpeed != SignedEightEight.FromPixels(1))
            throw new InvalidDataException("A piece of the hand starts falling at other than a pixel an update, which the Godot hand throws it at.");

        const int block = EngineRoomObjects.Block;
        new EngineObjectFile(_tree, file, "BombTorizoHand")
            .Value("debris_look", TorizoLookName(TorizoArtworkLookKeys.Installed.Of(TorizoArtworkLook.ChozoCrumbled)))
            .Group("picture_offset", new JsonObject { ["x"] = left * block, ["y"] = top * block })
            .Group("crumble", EngineNames.List(BombTorizoHandCrumbleDefinitions.Crumble.Select(step => (JsonNode)new JsonObject
            {
                ["picture"] = Picture(step.Image),
                ["updates"] = step.Updates,
                ["fragment"] = step.FragmentBefore is { } thrown ? EngineSheets.FileName(thrown.ToString()) : "",
            })))
            .Group("fragments", fragments)
            .Collider("Fingers", runs[0].Left * block, runs[0].Top * block, runs[0].Columns * block, runs[0].Rows * block)
            .Collider("Palm", runs[1].Left * block, runs[1].Top * block, runs[1].Columns * block, runs[1].Rows * block)
            .Write();
    }
}
