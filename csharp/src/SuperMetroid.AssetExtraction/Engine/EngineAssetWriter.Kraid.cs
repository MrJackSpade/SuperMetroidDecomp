using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.Kraid.Artwork;
using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.EffectActors;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.World.RoomStates;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>Kraid: its body, head, arm and foot, the spikes of its belly, its nails and rocks, and what it does to its room.</summary>
public sealed partial class EngineAssetWriter
{
    /// <summary>The names of the patches a room where Kraid has been beaten begins with: its ceiling and its spikes gone.</summary>
    private const string KraidCeilingClearPatch = "ceiling_clear", KraidSpikesClearPatch = "spikes_clear";

    /// <summary>
    /// The colours a shade of Kraid's shows changed on the pictures of the places given, each with the sheets its
    /// pictures were cut from, as one change.
    /// </summary>
    /// <exception cref="InvalidDataException">The change holds in one region of the pictures only, and the pictures cut from the sheets have no map of regions.</exception>
    private JsonObject KraidShadeChange(string shade, string family, IReadOnlyDictionary<string, IReadOnlyList<string>> places)
    {
        JsonObject change = ShadeChange(shade, family, places);
        foreach (KeyValuePair<string, JsonNode?> place in change)
        {
            foreach (KeyValuePair<string, JsonNode?> colour in place.Value!.AsObject())
            {
                if (colour.Key.Contains('/', StringComparison.Ordinal))
                    throw new InvalidDataException($"The shade {shade} changes the colour {colour.Key} of Kraid's pictures in {place.Key} in one region of them only, and its pictures have no map of regions.");
            }
        }

        return change;
    }

    /// <summary>The installed frame, and the kinds of block it makes, of something the old Kraid draws into its room.</summary>
    private static (string Frame, RoomBlockShape Shape) KraidDrawn(RoomObjectDraw draw) => draw switch
    {
        RoomObjectFrameDraw<KraidRoomDraw> framed => (RoomObjectKraidVisualFiles.VisualId(framed.Frame), KraidRoomObjectDraws.Family.Shape(framed.Frame)),
        RoomObjectFrameDraw<KraidRoomObjectDraws.CrumbleDraw> crumbling => (crumbling.Frame.Stage switch
        {
            BlockBreakupStage.Rock => "crumble-rock",
            BlockBreakupStage.Pieces => "crumble-pieces",
            BlockBreakupStage.Dust => "crumble-dust",
            var stage => throw new InvalidDataException($"A block of Kraid's room crumbles through {stage}, which is not drawn."),
        }, KraidRoomObjectDraws.CrumbleFamily.Shape(crumbling.Frame)),
        _ => throw new InvalidDataException("Kraid draws something into its room that is not one of its room's frames."),
    };

    /// <summary>Writes Kraid, its spikes, nails and rocks, and the patches of its room.</summary>
    private void WriteKraid()
    {
        const string rockScene = "res://game/bosses/kraid/kraid_rock.tscn", ceilingRockScene = "res://game/bosses/kraid/kraid_ceiling_rock.tscn";
        const string directory = GameInstallationLayout.RoomObjectKraidVisualDirectoryName;
        string armKey = EnemyDefinitionIdKeys.Installed.Of(EnemyDefinitionId.KraidArm), footKey = EnemyDefinitionIdKeys.Installed.Of(EnemyDefinitionId.KraidFoot);
        EngineEnemy kraid = Enemy("bosses/kraid.json", "Kraid", EnemyDefinitionId.Kraid, armKey, footKey);
        // What a shot that does it no harm leaves is its own class's, part by part.
        kraid.Data.Remove("dud");
        List<(RoomDefinition Room, RoomStateSettings State)> states = StatesHolding(EnemyDefinitionId.Kraid);

        // What it does to its room: the ceiling's blocks it breaks as it grows, each with the rock that falls from
        // it; the platforms that crumble as it sinks; the spikes that crumble as it dies; and the room as it is left.
        JsonArray ceilingRocks = [];
        foreach (KraidCeilingRock rock in KraidCeilingRockDefinitions.Rocks)
        {
            ceilingRocks.Add(new JsonObject
            {
                ["x"] = rock.X,
                ["crumble"] = TerrainChangeAt(states, $"ceiling_{ceilingRocks.Count + 1}", rock.Crumble.Block,
                    KraidRoomMutationDefinitions.Changes[rock.Crumble.RoomObject], directory, KraidDrawn),
            });
        }

        JsonArray sinkCrumbles = [];
        foreach ((int height, KraidSinkStep step) in KraidArenaDefinitions.SinkCrumbles.OrderBy(crumble => crumble.Key))
        {
            RoomObjectSpawnRequest<KraidRoomMutation> request = KraidObjects.ForSinkStep(step);
            sinkCrumbles.Add(new JsonObject
            {
                ["y"] = height,
                ["x"] = request.Block.ToRoomPixels().X,
                ["crumble"] = TerrainChangeAt(states, $"platform_{sinkCrumbles.Count + 1}", request.Block,
                    KraidRoomMutationDefinitions.Changes[request.RoomObject], directory, KraidDrawn),
            });
        }

        RoomObjectSpawnRequest<KraidRoomMutation> spikes = KraidObjectDefinitions.LiveDeathSpikes;
        JsonObject deathSpikes = TerrainChangeAt(states, "death_spikes", spikes.Block, KraidRoomMutationDefinitions.Changes[spikes.RoomObject], directory, KraidDrawn);
        foreach (RoomObjectSpawnRequest<KraidRoomMutation> cleared in KraidObjectDefinitions.DefeatedRoom)
        {
            string name = cleared.RoomObject switch
            {
                KraidRoomMutation.ClearCeiling => KraidCeilingClearPatch,
                KraidRoomMutation.ClearSpikes => KraidSpikesClearPatch,
                var other => throw new InvalidDataException($"A room where Kraid has been beaten is given {other}, which this does not know."),
            };
            TerrainChangeAt(states, name, cleared.Block, KraidRoomMutationDefinitions.Changes[cleared.RoomObject], directory, KraidDrawn, leaves: name);
        }

        // The spikes of its belly and its nails. Both nails are drawn from the first one's sheet.
        string lintKey = EnemyDefinitionIdKeys.Installed.Of(EnemyDefinitionId.KraidLintTop);
        string nailKey = EnemyDefinitionIdKeys.Installed.Of(EnemyDefinitionId.KraidFirstNail);
        EngineEnemy lint = Enemy("bosses/kraid_lint.json", "KraidLint", EnemyDefinitionId.KraidLintTop);
        (string File, EnemyDefinitionId Id)[] nailFiles =
        [
            ("bosses/kraid_nail.json", EnemyDefinitionId.KraidFirstNail), ("bosses/kraid_second_nail.json", EnemyDefinitionId.KraidSecondNail),
        ];
        (EngineEnemy Nail, EnemyDefinitionId Id)[] nails = [.. nailFiles.Select(file => (Enemy(file.File, "KraidNail", file.Id, nailKey), file.Id))];

        // Its body and head, from the sheet of its body.
        const string bodyFolder = "body";
        IReadOnlyDictionary<string, EngineBodyFrame> body = _sheets.CutBossBody(_tree, _installation.ContentDirectory, $"{kraid.Folder}/{bodyFolder}", "Kraid");
        JsonObject Offset(EngineBodyFrame frame) => new() { ["x"] = frame.OffsetX - kraid.Pictures.OffsetX, ["y"] = frame.OffsetY - kraid.Pictures.OffsetY };

        EngineBodyFrame trunk = body[KraidBodyFrameNames.Body];
        kraid.Timeline("body", new JsonObject
        {
            ["steps"] = new JsonArray(new JsonObject { ["picture"] = trunk.Picture, ["updates"] = 1 }),
            ["loops_to"] = 0,
            ["offset"] = Offset(trunk),
        });

        // Its head: how far its mouth is open goes with each picture, and with it where it can be hurt.
        (string Name, KraidHeadFrame Frame)[] mouths =
        [
            ("closed", KraidHeadFrame.Closed), ("opening", KraidHeadFrame.Opening), ("wider", KraidHeadFrame.Wider), ("open", KraidHeadFrame.Open),
        ];
        foreach ((string name, KraidHeadSegment segment) in new[]
                 {
                     ("head_roar", KraidHeadSegment.Roar), ("head_eye_glow", KraidHeadSegment.EyeGlow), ("head_death", KraidHeadSegment.Death),
                 })
        {
            JsonObject timeline = EngineTimelines.Of(KraidHeadAnimations.Segments, KraidHeadAnimations.StartingWith(segment),
                frame => body[frame.FrameName].Picture,
                static (KraidHeadCue cue) => cue switch
                {
                    KraidHeadCue.Roar => EngineNames.SoundCue(KraidSoundDefinitions.HeadRoar),
                    KraidHeadCue.DyingCry => EngineNames.SoundCue(KraidSoundDefinitions.HeadDyingCry),
                    _ => throw new InvalidDataException($"Kraid's head raises {cue}, which this does not know."),
                });
            foreach (JsonNode? step in timeline["steps"]!.AsArray())
            {
                string picture = step!["picture"]!.GetValue<string>();
                string mouth = mouths.Single(candidate => body[candidate.Frame.FrameName].Picture == picture).Name;
                step["cues"] = Prepended("mouth:" + mouth, step["cues"]);
            }

            EngineBodyFrame[] shown = [.. mouths.Select(mouth => body[mouth.Frame.FrameName])];
            if (shown.Any(frame => frame.OffsetX != shown[0].OffsetX || frame.OffsetY != shown[0].OffsetY))
                throw new InvalidDataException("The pictures of Kraid's head do not all lie in one place, which a timeline's one offset cannot say.");
            timeline["offset"] = Offset(shown[0]);
            kraid.Timeline(name, timeline);
        }

        // Its arm and its foot. The foot's pictures move about as it walks, which its timelines say with a cue.
        static string? PartCue(KraidPartCue cue) => cue switch
        {
            KraidPartCue.RaiseBody => "raise_body",
            KraidPartCue.LowerBodyAndShake => "lower_body",
            KraidPartCue.StepSound => EngineNames.SoundCue(KraidSoundDefinitions.Footstep),
            KraidPartCue.StepLeft => "step_left",
            KraidPartCue.StepRight => "step_right",
            KraidPartCue.SlowArmBelowHalfHealth => "slow_check",
            KraidPartCue.ArmSwingBegins => "swing_begins",
            KraidPartCue.ArmReachesPause => "reaches_pause",
            KraidPartCue.WalkReachesEnd => "walk_end",
            KraidPartCue.WalkPlaysOn => "walk_on",
            _ => throw new InvalidDataException($"A part of Kraid raises {cue}, which this does not know."),
        };
        foreach ((string name, KraidPartSegment segment) in new[]
                 {
                     ("arm_normal", KraidPartSegment.ArmNormal), ("arm_slow", KraidPartSegment.ArmSlow),
                     ("arm_rising_or_sinking", KraidPartSegment.ArmRisingOrSinking),
                     ("arm_dying_or_preparing_to_lunge", KraidPartSegment.ArmDyingOrPreparingToLunge),
                 })
        {
            kraid.Timeline(name, EngineEnemy.WithPoseCues(EngineTimelines.Of(
                KraidAnimations.Segments, KraidAnimations.StartingWith(segment), pose => ArmPicture(kraid, pose), PartCue)));
        }

        // A step's picture carries where the foot is drawn until the step is given the cue that says it, ahead of the
        // cue of the pose its picture has, whose pieces move with it.
        const char placed = '#';
        foreach ((string name, KraidPartSegment segment) in new[]
                 {
                     ("foot_initial", KraidPartSegment.FootInitial), ("foot_neutral", KraidPartSegment.FootNeutral),
                     ("foot_walking_forward", KraidPartSegment.FootWalkingForward), ("foot_lunge_forward", KraidPartSegment.FootLungeForward),
                     ("foot_walking_backward", KraidPartSegment.FootWalkingBackward),
                 })
        {
            JsonObject timeline = EngineTimelines.Of(KraidAnimations.Segments, KraidAnimations.StartingWith(segment),
                pose => $"{kraid.Picture(pose.Picture)}{placed}{pose.Placement.X}:{pose.Placement.Y}", PartCue);
            var placements = new List<string>();
            foreach (JsonNode? step in timeline["steps"]!.AsArray())
            {
                string[] parts = step!["picture"]!.GetValue<string>().Split(placed);
                step["picture"] = parts[0];
                placements.Add(parts[1]);
            }

            EngineEnemy.WithPoseCues(timeline);
            JsonArray steps = timeline["steps"]!.AsArray();
            for (int step = 0; step < steps.Count; step++)
                steps[step]!["cues"] = Prepended("place:" + placements[step], steps[step]!["cues"]);

            kraid.Timeline(name, timeline);
        }

        // Its rocks. One from its mouth breaks up where a shot meets it; the others are only taken away.
        static string? RockCue(KraidRockCue cue) => cue == KraidRockCue.StopFlyingFromNextPass
            ? null
            : throw new InvalidDataException($"A rock of Kraid's raises {cue}, which this does not know.");
        string largeRock = kraid.SpawnedTimeline("rock", KraidRockAnimations.Segments, KraidRockAnimations.StartingWith(KraidRockSegment.SharedPose), RockCue);
        string smallRock = kraid.SpawnedTimeline("rock_small", KraidRockAnimations.Segments, KraidRockAnimations.StartingWith(KraidRockSegment.RisingRightPose), RockCue);
        // A shot one stops where it is and breaks up, which the effect it leaves does.
        string rockBurst = kraid.SpawnedTimeline("rock_burst", KraidRockAnimations.Segments, KraidRockAnimations.StartingWith(KraidRockSegment.SpitRockShot), RockCue);
        void Rock(JsonObject shot, JsonObject projectile, string scene, string timeline)
        {
            shot["scene"] = scene;
            projectile["timeline"] = timeline;
            projectile["picture_offset"] = EngineEnemy.NoOffset();
            // It meets terrain its own way, as it moves itself.
            projectile["meeting_terrain"] = "passes_through";
        }

        kraid.Shot("spit_rock", RoomEnemyProjectileKind.KraidSpitRock, (_, shot, projectile) =>
            {
                Rock(shot, projectile, rockScene, largeRock);
                shot["sound"] = EngineNames.Sound(KraidSoundDefinitions.RockSpit);
                projectile["ending_effect"] = rockBurst;
            })
            .Shot("rising_rock", RoomEnemyProjectileKind.KraidRisingRock, (_, shot, projectile) => Rock(shot, projectile, rockScene, largeRock))
            .Shot("rising_rock_small", RoomEnemyProjectileKind.KraidRisingRock, (_, shot, projectile) => Rock(shot, projectile, rockScene, smallRock))
            .Shot("ceiling_rock", RoomEnemyProjectileKind.KraidCeilingRock, (_, shot, projectile) => Rock(shot, projectile, ceilingRockScene, largeRock));

        // Only what goes into its mouth hurts it: of the beams, only a charged one, which the Godot Kraid sees to itself.
        RoomEnemyDefinition hurtInMouth = kraid.Definition with { Shot = EnemyReaction.OrdinaryShot };
        KraidEyeColours eyes = _installation.LoadEnemyTiles().KraidEyes;
        if (KraidCadenceDefinitions.MouthSpitRock.Phase != 0)
            throw new InvalidDataException("Kraid's rocks do not leave its mouth from the update it opens, as its data's one cadence says.");

        // The values a channel of a colour moves through as its eyes glow and dim, a step an update, darkest first.
        var channelSteps = new JsonArray();
        for (int step = 0; step <= FadeLevelColour.FullLevel; step++)
            channelSteps.Add(FadeLevelColour.Grey(step).ToColour().Red);

        // Its other colours, how hurt it is, the flash of a hit in its mouth and its dying, are changes of the colours
        // of its one set of pictures: of its body and head, cut from the sheet of its body, and of its parts, its arm,
        // its foot, its spikes and its nails, each cut from sheets of their own.
        const string bodyFamily = GameInstallationLayout.BossBodySheetDirectoryName, partFamily = GameInstallationLayout.EnemySheetDirectoryName;
        var bodyPlaces = new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal)
        {
            [$"{kraid.Folder}/{bodyFolder}"] = [_sheets.SheetNamed(bodyFamily, BossBodySheetPrefix + "Kraid")],
        };
        var partPlaces = new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal)
        {
            [kraid.Folder] = [_sheets.EnemySheet(armKey), _sheets.EnemySheet(footKey)],
            [lint.Folder] = [_sheets.EnemySheet(lintKey)],
        };
        foreach ((EngineEnemy nail, EnemyDefinitionId _) in nails)
            partPlaces[nail.Folder] = [_sheets.EnemySheet(nailKey)];
        JsonObject BodyColours(string shade) => KraidShadeChange(shade, bodyFamily, bodyPlaces);
        JsonObject PartColours(string shade) => KraidShadeChange(shade, partFamily, partPlaces);

        kraid.Group("reactions", EngineReactions.Of(hurtInMouth))
            .Value("rise_wait_updates", KraidArenaDefinitions.RiseWaitUpdates)
            .Value("lunge_delay_updates", KraidArenaDefinitions.LungeDelayUpdates)
            .Value("thinking_step_updates", KraidArenaDefinitions.ThinkingDelayStepUpdates)
            .Value("spit_cadence_updates", KraidCadenceDefinitions.MouthSpitRock.Period)
            // Its backdrop's fade takes a step an update while its count stands at or below its last step, and is done on the update after.
            .Value("backdrop_fade_steps", KraidBackgroundDefinitions.BackdropFadeLastStep + 1)
            .Value("death_drops", KraidArenaDefinitions.DeathDropCount)
            // Where its fight keeps Samus in the view, by the way she faces: the boss's place.
            .Value("samus_in_view_facing_right", HorizontalCameraTargets.Offset(CameraDistanceMode.BossTracking, HorizontalDirection.Right))
            .Value("samus_in_view_facing_left", HorizontalCameraTargets.Offset(CameraDistanceMode.BossTracking, HorizontalDirection.Left))
            .Value("dud_effect", DustFile(DustExplosionAnimation.DudShotTinyExplosion))
            .Value("big_dud_effect", DustFile(DustExplosionAnimation.BigExplosion))
            .Value("mouth_dud_effect", EffectFile(RoomEffectActorKind.DudShot))
            .Value("dud_sound", EngineNames.Sound(SamusSound.DudShot))
            .Value("rock_sound", EngineNames.Sound(KraidSoundDefinitions.RockSpit))
            .Group("head_poses", KraidHeadPoses(mouths, kraid.Definition.Radius))
            .Group("arm_poses", KraidArmPoses())
            .Group("foot_poses", KraidFootPoses())
            .Group("body_edge", EngineNames.List(KraidBodyLedges(kraid.Definition.Radius.Y).AsEnumerable().Reverse().Select(ledge => (JsonNode)new JsonObject
            {
                ["from"] = ledge.Top,
                ["left"] = ledge.Left,
            })))
            .Group("ceiling_rocks", ceilingRocks)
            .Group("sink_crumbles", sinkCrumbles)
            .Group("death_spikes", deathSpikes)
            .Group("walks", EngineNames.List(KraidMovementDefinitions.Rows.Select(row => (JsonNode)new JsonObject
            {
                ["from"] = row.X,
                // Each of the roll's eight numbers, in turn, is one walk.
                ["to"] = EngineNames.List(EngineNames.Each(row.Walks).Select(walk => (JsonNode)new JsonObject
                {
                    ["weight"] = 1,
                    ["to"] = walk.TargetX,
                    ["updates"] = walk.ThinkUpdates,
                })),
            })))
            .Group("spit", new JsonObject
            {
                ["x"] = KraidArenaDefinitions.SpitRockOffset.X,
                ["y"] = KraidArenaDefinitions.SpitRockOffset.Y,
                ["rise"] = EngineNames.Number(KraidArenaDefinitions.SpitRockRise),
                ["speeds"] = EngineNames.List(EngineNames.Each(KraidRockLaunchDefinitions.SpitLaunch).Select(speed => EngineNames.Number(speed))),
            })
            .Group("health_shades", EngineNames.List(KraidHealthBandDefinitions.HighestFirst.Select(band => (JsonNode)new JsonObject
            {
                ["health"] = band.MinimumEighths is { } eighths ? KraidHealthThresholds.Eighths(kraid.Definition.Health, eighths) : 0,
                ["body"] = BodyColours(KraidShades.Health(band).Key),
                ["parts"] = PartColours(KraidShades.Health(band).Key),
                ["eyes"] = KraidEyes(eyes.Of(band)),
            })))
            .Group("hurt_colours", new JsonObject
            {
                ["body"] = BodyColours(KraidShades.HurtFlash.Key),
                ["parts"] = PartColours(KraidShades.HurtFlash.Key),
            })
            .Group("death_colours", BodyColours(KraidShades.DeathArm.Key))
            // Every shade of its body shows its eyes in the colours of its most hurt band; its eyes' colours are shown
            // in their place.
            .Group("eye_colours", new JsonObject
            {
                ["pictured"] = KraidEyes(eyes.Shaded),
                ["hurt"] = KraidEyes(eyes.Hurt),
                ["steps"] = channelSteps,
            })
            .Group("hurt_flash", new JsonObject
            {
                ["wait"] = KraidArenaDefinitions.HurtFlashStepUpdates,
                ["steps"] = EngineNames.List(KraidHurtFlashAnimations.Segments.Beats(KraidHurtFlashSegment.Flashing)
                    .OfType<AnimationBeat<KraidHurtFlashShade, NoAnimationCue>.Frame>()
                    .Select(shown => (JsonNode)new JsonObject { ["hurt"] = shown.Shown == KraidHurtFlashShade.HurtColours, ["updates"] = shown.Duration })),
            })
            .Write();

        // Its rocks: those from its mouth and the floor drift across as they fall, and those from the ceiling fall straight.
        JsonObject RockData(string type, SignedEightEight gravity, SignedEightEight drift, SignedEightEight driftLimit)
        {
            var data = new JsonObject();
            EngineDefaults.Fill(data, type);
            if (!data.ContainsKey("gravity") || !data.ContainsKey("drift") || !data.ContainsKey("drift_limit"))
                throw new InvalidDataException($"A rock of Kraid's is given values its type {type} does not read by those names.");
            data["gravity"] = EngineNames.Number(gravity);
            data["drift"] = EngineNames.Number(drift);
            data["drift_limit"] = EngineNames.Number(driftLimit);
            return data;
        }

        _tree.WriteData("bosses/kraid_rock.json", RockData("KraidRock", KraidRockLaunchDefinitions.RockGravity, KraidRockLaunchDefinitions.RockDrift, KraidRockLaunchDefinitions.RockDriftRestart));
        _tree.WriteData("bosses/kraid_ceiling_rock.json", RockData("KraidRock", KraidRockLaunchDefinitions.CeilingRockGravity, SignedEightEight.Zero, KraidRockLaunchDefinitions.RockDriftRestart));

        // The spikes of its belly, which show the colours of its parts.
        lint.BodyColliders("Body", "Solid", "Hurtbox")
            .Value("emerge_pixels", KraidArenaDefinitions.LintEmergePixels)
            .Value("charge_updates", KraidArenaDefinitions.LintChargeUpdates)
            .Value("fire_speed", EngineNames.Number(-KraidPlatformMovement.FiringDisplacement))
            .Value("harmless_x", KraidArenaDefinitions.LintIntangibleX)
            .Value("return_x", KraidArenaDefinitions.LintReturnX)
            .Value("reload_updates", KraidArenaDefinitions.LintReloadUpdates)
            .Value("fire_sound", EngineNames.Sound(KraidSoundDefinitions.LintFire))
            .Value("point_x", KraidContactDefinitions.LintLeadingEdge);
        lint.Timeline("budding", EngineTimelines.Of(KraidAnimations.Segments, KraidAnimations.StartingWith(KraidPartSegment.LintInitial), pose => lint.Picture(pose.Picture), PartCue))
            .Timeline("grown", EngineTimelines.Of(KraidAnimations.Segments, KraidAnimations.StartingWith(KraidPartSegment.LintPostGrowth), pose => lint.Picture(pose.Picture), PartCue));
        lint.Write();

        // Its nails, which show the colours of its parts; only the first leaves anything when it is shot.
        int pastLedges = _installation.LoadBossAndIntroOverreads().KraidNailPastLedges.LeftOffset;
        foreach ((EngineEnemy nail, EnemyDefinitionId id) in nails)
        {
            nail.BodyColliders("Body", "Hurtbox")
                // The second death its touch runs on a place her body's blow already emptied names the small explosion.
                .Value("second_death_effect", DeathFile(EnemyDeathExplosion.Small))
                .Value("launch_delay", KraidArenaDefinitions.NailLaunchDelayUpdates)
                .Value("below_arm", KraidArenaDefinitions.NailLaunchBelowArm)
                // It sets off as far left of Kraid's place as Kraid's half-width and its own.
                .Value("launch_reach", kraid.Definition.Radius.X + RoomEnemyDefinitionCatalog.ById[id].Radius.X)
                .Value("past_ledges_left", pastLedges)
                .Group("ledges", EngineNames.List(KraidNailContourDefinitions.Ledges.Select(ledge => (JsonNode)new JsonObject
                {
                    ["top"] = ledge.Top,
                    ["left"] = ledge.Left,
                })));
            nail.Timeline("spinning", EngineTimelines.Of(KraidAnimations.Segments, KraidAnimations.StartingWith(KraidPartSegment.NailLoop), pose => nail.Picture(pose.Picture), PartCue));
            nail.Write();
        }
    }

    /// <summary>A step's cues with one more put ahead of them.</summary>
    private static JsonArray Prepended(string cue, JsonNode? cues)
    {
        var all = new JsonArray { cue };
        if (cues is JsonArray heard)
        {
            foreach (JsonNode? one in heard)
                all.Add(one!.GetValue<string>());
        }

        return all;
    }

    /// <summary>The picture of a pose of Kraid's arm, which no step of its timelines moves about.</summary>
    private static string ArmPicture(EngineEnemy kraid, KraidPartPose pose) => pose.Placement == Offset16<RoomPixels>.Zero
        ? kraid.Picture(pose.Picture)
        : throw new InvalidDataException("A pose of Kraid's arm is drawn away from its place, which its timelines do not say.");

    /// <summary>
    /// The ledges of Kraid's body: the left edge it has, from its place, from each height down to the next, found by
    /// asking the old game's outline at every height as far above and below its place as it is tall. Top first.
    /// </summary>
    private static List<(int Top, int Bottom, int Left)> KraidBodyLedges(int reach)
    {
        var ledges = new List<(int Top, int Bottom, int Left)>();
        for (int height = -reach; height < reach; height++)
        {
            int left = KraidBodyContour.LeftEdge(height);
            if (ledges.Count > 0 && ledges[^1].Left == left)
                ledges[^1] = (ledges[^1].Top, height + 1, left);
            else
                ledges.Add((height, height + 1, left));
        }

        return ledges;
    }

    /// <summary>
    /// Where the pieces of Kraid that shots meet are for each way its mouth is open. A shot is hurt by the inner shape
    /// of its mouth alone. Above the bottom of the mouth's outer shape its body is that shape, reaching back to its
    /// right side; below it, its body is its ledges.
    /// </summary>
    private static JsonObject KraidHeadPoses((string Name, KraidHeadFrame Frame)[] mouths, Extent16<RoomPixels> radius)
    {
        List<(int Top, int Bottom, int Left)> ledges = KraidBodyLedges(radius.Y);
        var poses = new JsonObject();
        foreach ((string name, KraidHeadFrame frame) in mouths)
        {
            var colliders = new JsonObject();
            if (frame.InvulnerableShape is { } innerShape)
            {
                EnemyHitboxRectangle inner = KraidMouthHitboxes.Resolve(innerShape);
                colliders["Mouth"] = Rect(inner.Left, inner.Top, inner.Right, inner.Bottom);
            }

            EnemyHitboxRectangle outer = KraidMouthHitboxes.Resolve(frame.VulnerableShape);
            colliders["HeadShell"] = Rect(outer.Left, outer.Top, radius.X, outer.Bottom);
            for (int ledge = 0; ledge < ledges.Count; ledge++)
            {
                int top = Math.Max(ledges[ledge].Top, outer.Bottom);
                if (top < ledges[ledge].Bottom)
                    colliders[$"Body{ledge + 1}"] = Rect(ledges[ledge].Left, top, radius.X, ledges[ledge].Bottom);
            }

            poses[name] = new JsonObject { ["colliders"] = colliders };
        }

        return poses;
    }

    /// <summary>
    /// Where the pieces of Kraid's arm are for each of its pictures, from Kraid's place, in the order Samus's touch
    /// meets them: "Arm" and a number for her touch, and "ArmShield" and the same number for her shots, which every
    /// piece stops with the arm's explosion.
    /// </summary>
    private static JsonObject KraidArmPoses()
    {
        var poses = new JsonObject();
        foreach ((EnemyFrameId frame, KraidArmComponentDefinition[] components) in KraidArmCollisionDefinitions.FrameComponents)
        {
            var colliders = new JsonObject();
            int pieces = 0;
            foreach (KraidArmComponentDefinition component in components)
            {
                Offset16<RoomPixels> from = KraidArenaDefinitions.ArmOffset + component.Offset;
                foreach (EnemyCollisionHitbox piece in KraidArmCollision.HitboxesAt(component.Hitboxes))
                {
                    if (piece.Shot != KraidReactions.ArmShot)
                        throw new InvalidDataException($"A piece of Kraid's arm in its frame {frame} meets shots otherwise than with the arm's explosion, which its poses do not say.");
                    pieces++;
                    colliders[$"Arm{pieces}"] = Rect(from.X + piece.Left, from.Y + piece.Top, from.X + piece.Right, from.Y + piece.Bottom);
                    colliders[$"ArmShield{pieces}"] = Rect(from.X + piece.Left, from.Y + piece.Top, from.X + piece.Right, from.Y + piece.Bottom);
                }
            }

            poses[EngineEnemy.PoseName(frame)] = new JsonObject { ["colliders"] = colliders };
        }

        return poses;
    }

    /// <summary>
    /// Where the pieces of Kraid's foot are for each of its pictures, from Kraid's place where its walk first shows the
    /// picture: "FootShield" and a number, in the order shots meet them. Every piece stops shots and does nothing else.
    /// </summary>
    private static JsonObject KraidFootPoses()
    {
        var poses = new JsonObject();
        foreach ((EnemyFrameId frame, Offset16<RoomPixels>[] offsets) in KraidFootCollisionDefinitions.ComponentOffsets)
        {
            var colliders = new JsonObject();
            int pieces = 0;
            foreach (Offset16<RoomPixels> offset in offsets)
            {
                Offset16<RoomPixels> from = KraidArenaDefinitions.FootOffset + offset;
                foreach (EnemyCollisionHitbox piece in KraidFootCollisionDefinitions.SharedHitboxes)
                {
                    if (piece.Shot != EnemyReaction.Ignore)
                        throw new InvalidDataException($"A piece of Kraid's foot in its frame {frame} meets shots otherwise than by stopping them, which its poses do not say.");
                    pieces++;
                    colliders[$"FootShield{pieces}"] = Rect(from.X + piece.Left, from.Y + piece.Top, from.X + piece.Right, from.Y + piece.Bottom);
                }
            }

            poses[EngineEnemy.PoseName(frame)] = new JsonObject { ["colliders"] = colliders };
        }

        return poses;
    }

    /// <summary>
    /// A collider's rectangle from its edges. One the old game gives no width or no height is a line a shot or Samus
    /// still crosses: it is a pixel across.
    /// </summary>
    private static JsonObject Rect(int left, int top, int right, int bottom) => new()
    {
        ["left"] = left, ["top"] = top, ["width"] = Math.Max(1, right - left), ["height"] = Math.Max(1, bottom - top),
    };

    /// <summary>The colours of Kraid's eyes, one for each eye, each written as six hexadecimal digits.</summary>
    private static JsonArray KraidEyes(ReadOnlySpan<SuperMetroid.Core.Assets.RgbColor> colours)
    {
        var written = new JsonArray();
        foreach (SuperMetroid.Core.Assets.RgbColor colour in colours)
            written.Add($"{colour.Red:x2}{colour.Green:x2}{colour.Blue:x2}");
        return written;
    }
}
