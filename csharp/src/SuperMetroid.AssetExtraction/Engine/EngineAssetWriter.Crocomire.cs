using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.World.RoomStates;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>Crocomire: its body and tongue, its fire, the bridge and walls of its room, and its long death.</summary>
public sealed partial class EngineAssetWriter
{
    /// <summary>Writes Crocomire and the patches of its room.</summary>
    private void WriteCrocomire()
    {
        const string file = "bosses/crocomire.json", folder = "bosses/crocomire";
        const string pieceScene = "res://game/bosses/crocomire/crocomire_spike_piece.tscn";
        const string bridgeWall = "bridge_wall", skeletonWall = "skeleton_wall", wallBreaking = "spike_wall_breaking", breakThrough = "break_through_bridge";

        // A frame of it is the old game's sprites with the frame of its body of the same name over them.
        EngineSheetPictures pictures = _sheets.CutWithBossBody(_tree, _installation.ContentDirectory, folder, "Crocomire",
            _sheets.EnemySheet(EnemyDefinitionIdKeys.Installed.Of(EnemyDefinitionId.Crocomire)),
            _sheets.EnemySheet(EnemyDefinitionIdKeys.Installed.Of(EnemyDefinitionId.CrocomireTongue)));
        var crocomire = new EngineEnemy(_tree, _sheets, file, "Crocomire", EnemyDefinitionId.Crocomire, pictures);
        // What a shot that does it no harm leaves is its own class's, part by part.
        crocomire.Data.Remove("dud");

        // What it does to its room. The Godot project's room asks for the bridge's crumbling and collapse and the
        // opened spike wall by name; the walls that hold Samus in while it dies are named by its data.
        List<(RoomDefinition Room, RoomStateSettings State)> states = StatesHolding(EnemyDefinitionId.Crocomire);
        void Patch(string name, params ReadOnlySpan<(Position16<RoomBlocks> Block, CrocomireArenaMutation Change)> changes)
        {
            Position16<RoomBlocks> origin = changes[0].Block;
            var cells = new List<EngineLayouts.PatchCell>();
            foreach ((Position16<RoomBlocks> block, CrocomireArenaMutation change) in changes)
            {
                foreach (EngineLayouts.PatchCell cell in Layouts.PatchCells(GameInstallationLayout.RoomObjectCrocomireVisualDirectoryName,
                             (CrocomireArenaDrawKeys.Installed.Of(change), CrocomireArenaDraws.Family.Shape(change))))
                {
                    cells.Add(cell with { Across = cell.Across + block.X - origin.X, Down = cell.Down + block.Y - origin.Y });
                }
            }

            foreach ((RoomDefinition room, RoomStateSettings state) in states)
                Layouts.Patch(room, state, name, origin, cells);
        }

        IReadOnlyList<CrocomireBridgeApproachStage> approach = CrocomireBridgeCollapseDefinitions.ApproachStages;
        if (approach[0].CrumblingBlocks.Count != 0)
            throw new InvalidDataException("Crocomire's first step onto the bridge crumbles a block, which the Godot Crocomire's first stage does not.");
        for (int stage = 1; stage < approach.Count; stage++)
            Patch($"bridge_crumbles_{stage}", [.. approach[stage].CrumblingBlocks.Select(block => (block, CrocomireArenaMutation.CrumbleBridgeBlock))]);
        Patch("bridge_collapsed", (CrocomireArenaDefinitions.BridgeFirstBlock, CrocomireArenaMutation.ClearBridge));
        Patch(bridgeWall, (CrocomireArenaDefinitions.BridgeWallBlock, CrocomireArenaMutation.CreateInvisibleWall));
        Patch(skeletonWall, (CrocomireArenaDefinitions.SkeletonWallBlock, CrocomireArenaMutation.CreateInvisibleWall));
        // As its skeleton breaks through, the wall of the fight is cleared and the one before the spike wall raised.
        Patch(wallBreaking, (CrocomireArenaDefinitions.FightWallBlock, CrocomireArenaMutation.ClearInvisibleWall),
            (CrocomireArenaDefinitions.SpikeWallBlock, CrocomireArenaMutation.CreateInvisibleWall));
        Patch(breakThrough, (CrocomireArenaDefinitions.BreakThroughBridgeBlock, CrocomireArenaMutation.ClearBridge));
        Patch("spike_wall_open", (CrocomireArenaDefinitions.FightWallBlock, CrocomireArenaMutation.ClearInvisibleWall),
            (CrocomireArenaDefinitions.SpikeWallBlock, CrocomireArenaMutation.ClearInvisibleWall));

        // Its timelines, each named as the old animation is.
        static string? BodyCue(CrocomireBodyCue cue) => cue switch
        {
            CrocomireBodyCue.Step step => $"step:{step.Move.HorizontalPixels}:" + step.Move.Condition switch
            {
                CrocomireStepCondition.Always => "always",
                CrocomireStepCondition.UnlessHitPending => "unless_hit",
                CrocomireStepCondition.WhileOnScreen => "on_screen",
                var condition => throw new InvalidDataException($"Crocomire steps {condition}, which this does not know."),
            } + (step.Move.RaisesFootDust ? ":dust" : ""),
            CrocomireBodyCue.Dust dust => $"dust:{dust.Offset}",
            CrocomireBodyCue.Signal { Kind: CrocomireBodyCueKind.FightAi } => "fight",
            CrocomireBodyCue.Signal { Kind: CrocomireBodyCueKind.MaybeStartProjectileAttack } => "maybe_projectile",
            CrocomireBodyCue.Signal { Kind: CrocomireBodyCueKind.Cry } => EngineNames.SoundCue(CrocomireSoundDefinitions.Cry),
            CrocomireBodyCue.Signal { Kind: CrocomireBodyCueKind.BigExplosionSound } => EngineNames.SoundCue(CrocomireSoundDefinitions.BigExplosion),
            CrocomireBodyCue.Signal { Kind: CrocomireBodyCueKind.SkeletonCollapseSound } => EngineNames.SoundCue(CrocomireSoundDefinitions.SkeletonCollapse),
            CrocomireBodyCue.Signal { Kind: CrocomireBodyCueKind.ShakeScreen } => "shake",
            CrocomireBodyCue.Signal { Kind: CrocomireBodyCueKind.StepTowardSpikeWall } => "toward_spike_wall",
            CrocomireBodyCue.Signal { Kind: CrocomireBodyCueKind.SkeletonCollapsed } => "skeleton_collapsed",
            _ => throw new InvalidDataException($"Crocomire's animation raises {cue}, which this does not know."),
        };
        JsonObject poses = CrocomirePoses();
        foreach ((CrocomireBodyAnimation animation, SuperMetroid.Core.Animation.Animation<CrocomireBodySegment> played) in CrocomireBodyAnimations.StartingAt)
        {
            JsonObject timeline = EngineEnemy.WithPoseCues(EngineTimelines.Of(CrocomireBodyAnimations.Segments, played, crocomire.Picture, BodyCue));
            foreach (JsonNode? step in timeline["steps"]!.AsArray())
            {
                string? pose = step!["cues"]?.AsArray().Select(cue => cue!.GetValue<string>()).FirstOrDefault(cue => cue.StartsWith(EngineEnemy.PoseCue, StringComparison.Ordinal));
                if (pose is not null && !poses.ContainsKey(pose[EngineEnemy.PoseCue.Length..]))
                    throw new InvalidDataException($"Crocomire's animation {animation} shows a frame the old game gives no hitboxes: {pose}.");
            }

            crocomire.Timeline(EngineSheets.FileName(animation.ToString()), timeline);
        }

        crocomire.Timeline("tongue", CrocomireAnimations.TongueSegments, CrocomireAnimations.TongueStartingWith(CrocomireTongueSegment.Fight))
            .Timeline("tongue_melting", CrocomireAnimations.TongueSegments, CrocomireAnimations.TongueStartingWith(CrocomireTongueSegment.Melting));

        // What it does in the acid, a thing at a time, from the old course of its death.
        JsonArray acid = [];
        int waited = 0;
        string body = EngineSheets.FileName(CrocomireBodyInAcid.Whole.ToString());
        CrocomireMeltingIllustration pass = CrocomireMeltingIllustration.Flesh;
        void Flush()
        {
            if (waited > 0)
                acid.Add($"wait:{waited}");
            waited = 0;
        }

        void Shows(CrocomireBodyInAcid shown)
        {
            string name = EngineSheets.FileName(shown.ToString());
            if (name == body)
                return;
            Flush();
            acid.Add("body:" + name);
            body = name;
        }

        void Does(string what)
        {
            Flush();
            acid.Add(what);
        }

        bool inAcid = false, carriedOff = false;
        foreach (CrocomireDeathStage stage in CrocomireDeathStageDefinitions.Stages)
        {
            if (stage is CrocomireDeathStage.Fighting)
                continue;
            if (stage is CrocomireDeathStage.FlowDownRiver)
            {
                carriedOff = true;
                break;
            }

            inAcid = true;
            switch (stage)
            {
                case CrocomireDeathStage.SinkThroughBridge:
                    Does("sink");
                    break;
                case CrocomireDeathStage.Sink sink:
                    Shows(sink.Body);
                    Does("sink");
                    break;
                case CrocomireDeathStage.SubmergedPause pause:
                    Shows(pause.Body);
                    Does("pause");
                    break;
                case CrocomireDeathStage.Rise rise:
                    Shows(rise.Body);
                    Does("rise");
                    break;
                case CrocomireDeathStage.ShowMeltImage image:
                    pass = image.Picture;
                    waited++;
                    break;
                case CrocomireDeathStage.BeginMeltPass or CrocomireDeathStage.PauseOneUpdate or CrocomireDeathStage.ClearMeltImage:
                    waited++;
                    break;
                case CrocomireDeathStage.ShowMeltBands:
                    // One band an update, and done on the update after the last.
                    waited += CrocomireMelting.BandRound(pass) + 1;
                    break;
                case CrocomireDeathStage.StartDissolve dissolve:
                    Does("sound:" + EngineNames.SoundOrStop(dissolve.Cry));
                    waited++;
                    break;
                case CrocomireDeathStage.Dissolve:
                    Does($"melt:{CrocomireMeltingDefinitions.ShakeOffset}:{CrocomireMeltingDefinitions.ShakeHalfPeriod}");
                    break;
                case CrocomireDeathStage.FinalSink:
                    Does("sink");
                    break;
                default:
                    throw new InvalidDataException($"Crocomire's death goes through {stage} in the acid, which this does not know.");
            }
        }

        Flush();
        if (!inAcid || !carriedOff)
            throw new InvalidDataException("Crocomire's death never sinks into the acid, or is never carried off down the river.");

        static JsonArray MeltLooks(params ReadOnlySpan<(int FromY, CrocomireBodyAnimation Shown)> looks)
        {
            JsonArray listed = [];
            foreach ((int fromY, CrocomireBodyAnimation shown) in looks)
                listed.Add(new JsonObject { ["from_y"] = fromY, ["timeline"] = EngineSheets.FileName(shown.ToString()) });
            return listed;
        }

        // Its fire: aimed on a slant, gone on the first wall, and bursting and perhaps leaving something when shot.
        static string? ShotCue(CrocomireProjectileCue cue) => cue is CrocomireProjectileCue.SpawnDrops or CrocomireProjectileCue.Delete
            ? null
            : throw new InvalidDataException($"Something Crocomire launches raises {cue}, which this does not know.");
        IReadOnlyList<CrocomireSpikeWallPiece> wallPieces = CrocomireSpikeMotionDefinitions.Pieces;
        crocomire.Shot("fire", RoomEnemyProjectileKind.CrocomireProjectile, static (croc, shot, projectile) =>
            {
                shot["speed"] = CrocomireProjectileDefinitions.VolleySpeed;
                shot["scene"] = "res://game/bosses/crocomire/crocomire_fire.tscn";
                projectile["timeline"] = croc.SpawnedTimeline("fire", CrocomireAnimations.ProjectileSegments,
                    CrocomireAnimations.ProjectileStartingWith(CrocomireProjectileSegment.Mouth), ShotCue);
                projectile["picture_offset"] = EngineEnemy.NoOffset();
                // It moves itself and meets terrain its own way, carrying on past a block on its first move.
                projectile["meeting_terrain"] = "passes_through";
                projectile["ending_effect"] = croc.SpawnedTimeline("fire_burst", CrocomireAnimations.ProjectileSegments,
                    CrocomireAnimations.ProjectileStartingWith(CrocomireProjectileSegment.Shot), ShotCue);
                if (Drops(croc.Definition.Drops) is { } drops)
                    projectile["drops"] = drops;
            })
            .Shot("bridge_fragment", RoomEnemyProjectileKind.CrocomireBridgeCrumbling, static (croc, _, projectile) =>
            {
                projectile["timeline"] = croc.SpawnedTimeline("bridge_fragment", CrocomireAnimations.ProjectileSegments,
                    CrocomireAnimations.ProjectileStartingWith(CrocomireProjectileSegment.BridgeFragment), ShotCue);
                projectile["picture_offset"] = EngineEnemy.NoOffset();
                projectile["motion"] = "falling";
                projectile["acceleration"] = EngineNames.Number(CrocomireBridgeFragmentDefinitions.Gravity);
            })
            .Shot("spike_piece", RoomEnemyProjectileKind.CrocomireSpikeWallPieces, static (croc, shot, projectile) =>
            {
                shot["scene"] = pieceScene;
                projectile["timeline"] = croc.SpawnedTimeline("spike_piece", CrocomireAnimations.ProjectileSegments,
                    CrocomireAnimations.ProjectileStartingWith(CrocomireProjectileSegment.SpikeWallPiece), ShotCue);
                projectile["picture_offset"] = EngineEnemy.NoOffset();
                // It moves itself and lands at a height of its own; a thing of no size harms nothing.
                projectile["meeting_terrain"] = "passes_through";
                projectile["delivers_hit"] = false;
            });

        if (CrocomireProjectileDefinitions.AuthoredGradients.Length + 1 != CrocomireProjectileDefinitions.VolleyShotCount)
            throw new InvalidDataException("Crocomire's volley is not its authored shots and one more.");
        crocomire.BodyColliders("Body")
            .Value("wake_distance", CrocomireFightDefinitions.WakeDistance)
            .Value("spike_wall_charge_x", CrocomireFightDefinitions.SpikeWallChargeX)
            .Value("bridge_approach_x", CrocomireBridgeCollapseDefinitions.ApproachStartX)
            .Value("bridge_collapse_x", CrocomireBridgeCollapseDefinitions.CollapseStartX)
            .Value("sunken_y", CrocomireBridgeCollapseDefinitions.SunkenY)
            .Value("risen_y", CrocomireBridgeCollapseDefinitions.RisenY)
            .Value("submerged_pause", CrocomireBridgeCollapseDefinitions.SubmergedPauseUpdates)
            .Value("melt_updates", CrocomireMeltUpdates())
            .Value("acid_sound_updates", CrocomireBridgeCollapseDefinitions.AcidSoundInterval)
            .Value("wall_approach_x", CrocomireSkeletonDefinitions.WallApproachX)
            .Value("spiked_wall_updates", CrocomireSkeletonDefinitions.SpikedWallUpdates)
            .Value("fall_delay", CrocomireSkeletonDefinitions.FallDelay)
            .Value("death_drops", CrocomireSkeletonDefinitions.DropCount)
            // Where it keeps Samus in the view, by the way she faces: the boss's place through its fight, the right
            // edge's as its skeleton breaks out.
            .Value("samus_in_view_facing_right", HorizontalCameraTargets.Offset(CameraDistanceMode.BossTracking, HorizontalDirection.Right))
            .Value("samus_in_view_facing_left", HorizontalCameraTargets.Offset(CameraDistanceMode.BossTracking, HorizontalDirection.Left))
            .Value("samus_in_view_at_break_out", HorizontalCameraTargets.Offset(CameraDistanceMode.RightEdge, HorizontalDirection.Right))
            .Value("step_back_screen_reach", CrocomireFightDefinitions.StepBackScreenReach)
            .Value("mouth_shot_screen_reach", CrocomireFightDefinitions.MouthShotScreenReach)
            .Value("mouth_shot_hold", CrocomireFightDefinitions.MouthShotReactionHoldFrames)
            .Value("mouth_shot_flash", CrocomireFightDefinitions.MouthShotFlash)
            .Value("power_bomb_flash", CrocomireFightDefinitions.PowerBombFlash)
            .Value("charged_beam_steps", CrocomireFightDefinitions.ChargedBeamSteps)
            .Value("missile_steps", CrocomireFightDefinitions.MissileSteps)
            .Value("super_missile_steps", CrocomireFightDefinitions.SuperMissileSteps)
            .Value("power_bomb_steps", CrocomireFightDefinitions.PowerBombSteps)
            .Value("claw_push", CrocomireFightDefinitions.ClawPush)
            .Value("tongue_across", CrocomireFightDefinitions.TongueBodyXOffset)
            .Value("dust_effect", DustFile(DustExplosionAnimation.BigDustCloud))
            .Value("dud_effect", DustFile(DustExplosionAnimation.DudShotTinyExplosion))
            .Value("big_dud_effect", DustFile(DustExplosionAnimation.BigExplosion))
            .Value("spit_sound", EngineNames.Sound(CrocomireSoundDefinitions.ProjectileSpit))
            .Value("shake_sound", EngineNames.Sound(CrocomireSoundDefinitions.ScreenShake))
            .Value("bridge_sound", EngineNames.Sound(CrocomireSoundDefinitions.BridgeCollapse))
            .Value("acid_sound", EngineNames.Sound(CrocomireSoundDefinitions.AcidBurn))
            .Value("rumble_sound", EngineNames.Sound(CrocomireSoundDefinitions.WallRumble))
            .Value("explosion_sound", EngineNames.Sound(CrocomireSoundDefinitions.BigExplosion))
            .Value("crumble_sound", EngineNames.Sound(CrocomireSoundDefinitions.Crumble))
            .Value("wall_break_sound", EngineNames.Sound(CrocomireSoundDefinitions.SpikeWallBreak))
            .Value("bridge_wall_patch", bridgeWall)
            .Value("skeleton_wall_patch", skeletonWall)
            .Value("spike_wall_breaking_patch", wallBreaking)
            .Value("break_through_patch", breakThrough)
            .Group("poses", poses)
            // The screens of its room its fight and its death shut and open.
            .Group("screens", new JsonObject
            {
                ["behind_wall"] = EngineNames.List(CrocomireCameraDefinitions.BehindWallScreens.Select(screen => (JsonNode)At(screen.X, screen.Y))),
                ["arena"] = EngineNames.List(CrocomireCameraDefinitions.ArenaScreens.Select(screen => (JsonNode)At(screen.X, screen.Y))),
                ["hidden_wall"] = At(CrocomireCameraDefinitions.HiddenWallScreen.X, CrocomireCameraDefinitions.HiddenWallScreen.Y),
                ["bridge_left"] = At(CrocomireCameraDefinitions.BridgeLeftScreen.X, CrocomireCameraDefinitions.BridgeLeftScreen.Y),
                ["bridge"] = At(CrocomireCameraDefinitions.BridgeScreen.X, CrocomireCameraDefinitions.BridgeScreen.Y),
                ["bridge_left_shut_from_x"] = CrocomireCameraDefinitions.BridgeVisibleSamusX,
            })
            .Group("volley", new JsonObject
            {
                ["reach"] = CrocomireProjectileDefinitions.AimReachLeft,
                // Its last shot is aimed as the installation says the old one read past its list.
                ["gradients"] = EngineNames.List(CrocomireProjectileDefinitions.AuthoredGradients
                    .Append(_installation.LoadBossAndIntroOverreads().CrocomireLastShotGradient.Gradient)),
            })
            .Group("spit", new JsonObject
            {
                ["x"] = CrocomireFightDefinitions.SpitOffset.X, ["y"] = CrocomireFightDefinitions.SpitOffset.Y,
                // Its fire moves across by its first speed once, before it takes the speed it is aimed at.
                ["first_step"] = CrocomireFightDefinitions.SpitVelocity.X.FractionSubpixels == 0
                    ? CrocomireFightDefinitions.SpitVelocity.X.WholePixels
                    : throw new InvalidDataException("Crocomire's fire first moves by part of a pixel, which its data's whole first step cannot say."),
            })
            .Group("rumbles", EngineNames.List(CrocomireRumbleDefinitions.Oscillations.Select(rumble => (JsonNode)new JsonObject
            {
                ["return"] = rumble.ReturnOffset,
                ["extreme"] = rumble.ExtremeOffset,
                ["repeats"] = rumble.Repeats,
                ["speed"] = rumble.Speed,
            })))
            .Group("acid_script", acid)
            .Group("melt_looks", new JsonObject
            {
                // Lowest first: the deeper it lies, the fewer of its rows stand above the acid.
                [EngineSheets.FileName(CrocomireBodyInAcid.MeltingFlesh.ToString())] = MeltLooks(
                    (CrocomireMeltingDefinitions.TopRowAboveAcidY, CrocomireBodyAnimation.MeltingFleshTopRow),
                    (CrocomireMeltingDefinitions.TopTwoRowsAboveAcidY, CrocomireBodyAnimation.MeltingFleshTopTwoRows),
                    (CrocomireMeltingDefinitions.TopThreeRowsAboveAcidY, CrocomireBodyAnimation.MeltingFleshTopThreeRows),
                    (short.MinValue, CrocomireBodyAnimation.MeltingFleshTopFourRows)),
                [EngineSheets.FileName(CrocomireBodyInAcid.MeltingBareSkull.ToString())] = MeltLooks(
                    (CrocomireMeltingDefinitions.TopRowAboveAcidY, CrocomireBodyAnimation.MeltingBareSkullTopRow),
                    (CrocomireMeltingDefinitions.TopTwoRowsAboveAcidY, CrocomireBodyAnimation.MeltingBareSkullTopTwoRows),
                    (CrocomireMeltingDefinitions.TopThreeRowsAboveAcidY, CrocomireBodyAnimation.MeltingBareSkullTopThreeRows),
                    (short.MinValue, CrocomireBodyAnimation.MeltingBareSkullTopFourRows)),
            })
            .Group("bridge_fragments", new JsonObject
            {
                ["y"] = CrocomireBridgeFragmentDefinitions.Y,
                ["slowest_fall"] = EngineNames.Number(CrocomireBridgeFragmentDefinitions.SlowestFall),
                ["xs"] = EngineNames.List(Enumerable.Range(0, CrocomireBridgeFragmentDefinitions.FragmentCount).Select(CrocomireBridgeFragmentDefinitions.XPosition)),
            })
            .Group("spike_pieces", new JsonObject
            {
                ["x"] = CrocomireSkeletonDefinitions.SpikeWallTop.X,
                ["y"] = CrocomireSkeletonDefinitions.SpikeWallTop.Y,
                ["launch_speed"] = EngineNames.Number(CrocomireSpikeMotionDefinitions.WallPieceLaunchSpeed),
                ["gravity"] = EngineNames.Number(CrocomireSpikeMotionDefinitions.PieceGravity),
                ["landing_y"] = CrocomireSpikeMotionDefinitions.LandingY,
                ["pieces"] = EngineNames.List(wallPieces.Select(piece => (JsonNode)new JsonObject
                {
                    ["depth"] = piece.Depth,
                    ["growth"] = EngineNames.Number(piece.AccelerationGrowth),
                    ["most"] = EngineNames.Number(piece.MaximumAcceleration),
                    ["fastest"] = piece.FastestSpeed,
                    ["sound"] = piece.CrumbleSound is { } sound ? EngineNames.SoundOrStop(sound) : "",
                })),
            })
            .Write();
    }

    /// <summary>
    /// How many updates one of Crocomire's meltings lasts: the old melt's stretch climbs its source row and steepens
    /// each update, and is done on the update that finds it at its top and steepest.
    /// </summary>
    private static int CrocomireMeltUpdates()
    {
        int row = CrocomireMeltingDefinitions.LowestDistortionSourceRow;
        UnsignedSixteenSixteen slope = CrocomireMeltingDefinitions.FirstDistortionSlope;
        for (int updates = 1; updates < 4096; updates++)
        {
            int next = row - CrocomireMeltingDefinitions.DistortionSourceClimb;
            if (row < CrocomireMeltingDefinitions.DistortionSourceTop + CrocomireMeltingDefinitions.DistortionSourceClimb)
            {
                if (!slope.IsSlowerThan(CrocomireMeltingDefinitions.SteepestDistortionSlope))
                    return updates;
                next = CrocomireMeltingDefinitions.DistortionSourceTop;
            }

            row = next;
            UnsignedSixteenSixteen steeper = slope + CrocomireMeltingDefinitions.DistortionSlopeGrowth;
            slope = steeper.IsSlowerThan(CrocomireMeltingDefinitions.SteepestDistortionSlope) ? steeper : CrocomireMeltingDefinitions.SteepestDistortionSlope;
        }

        throw new InvalidDataException("Crocomire's melt never ends.");
    }

    /// <summary>
    /// Where the pieces of Crocomire that a shot or Samus meets are for each of its frames, in the order they are met,
    /// with what each does and how far its mouth is open.
    /// </summary>
    private static JsonObject CrocomirePoses()
    {
        static string Shot(EnemyReaction reaction) =>
            reaction == CrocomireReactions.MouthShot ? "mouth"
            : reaction == CrocomireReactions.DustShot ? "dust"
            : reaction == CrocomireReactions.AbsorbedShot ? "absorbed"
            : reaction == EnemyReaction.OrdinaryShot ? "plain"
            : throw new InvalidDataException($"A piece of Crocomire meets a shot with {reaction}, which this does not know.");
        static string Touch(EnemyReaction reaction) =>
            reaction == CrocomireReactions.ClawTouch ? "claw"
            : reaction == EnemyReaction.OrdinaryTouch ? "plain"
            : reaction == EnemyReaction.Ignore ? "none"
            : throw new InvalidDataException($"A piece of Crocomire meets Samus with {reaction}, which this does not know.");

        var poses = new JsonObject();
        IEnumerable<KeyValuePair<EnemyFrameId, EnemyCollisionComponent[]>> frames =
            CrocomireBodyCollisionDefinitions.Frames.Concat(CrocomireCorpseCollisionDefinitions.Frames);
        foreach ((EnemyFrameId frame, EnemyCollisionComponent[] components) in frames)
        {
            JsonObject colliders = [], shots = [], touches = [];
            foreach (EnemyCollisionComponent component in components)
            {
                foreach (EnemyCollisionHitbox hitbox in component.Hitboxes)
                {
                    string part = $"Part{colliders.Count + 1}";
                    colliders[part] = Rect(component.Offset.X + hitbox.Left, component.Offset.Y + hitbox.Top, component.Offset.X + hitbox.Right, component.Offset.Y + hitbox.Bottom);
                    shots[part] = Shot(hitbox.Shot);
                    touches[part] = Touch(hitbox.Touch);
                }
            }

            // How far its mouth is open picks how it answers a power bomb; a frame with no mouth answers as shut.
            CrocomireBodyAnimation answer = CrocomireMouthDefinitions.Mouths.ContainsKey(frame)
                ? CrocomirePowerBombReactions.ForFrame(frame)
                : CrocomireBodyAnimation.PowerBombReactionMouthNotOpen;
            string reaction = EngineSheets.FileName(answer.ToString());
            const string before = "power_bomb_reaction_mouth_";
            if (!reaction.StartsWith(before, StringComparison.Ordinal))
                throw new InvalidDataException($"Crocomire answers a power bomb with {answer}, which is not named for its mouth.");
            poses[EngineEnemy.PoseName(frame)] = new JsonObject
            {
                ["mouth"] = reaction[before.Length..],
                ["colliders"] = colliders,
                ["shots"] = shots,
                ["touches"] = touches,
            };
        }

        return poses;
    }
}
