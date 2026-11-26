using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Enemies.SpacePirates;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.World.RoomStates;
using SoundRequestPriority = SuperMetroid.Core.Compatibility.SoundRequestPriority;
using B = SuperMetroid.Core.Bosses.MotherBrain.MotherBrainBodySegment;
using H = SuperMetroid.Core.Bosses.MotherBrain.MotherBrainHeadSegment;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>
/// Mother Brain: her body, her brain and her neck, what she spits, breathes and fires, the pipes of her tank, and
/// what her fight and her death do to her room. Then the giant Metroid that comes to Samus's aid.
/// </summary>
public sealed partial class EngineAssetWriter
{
    /// <summary>The name of the timeline her brain plays for one of the old game's animations of it.</summary>
    private static string MotherBrainHeadName(H segment) => segment switch
    {
        // The Godot Mother Brain knows her last form's volley at Samus by this name.
        H.AttackingSamusFourOnionRingsPhase3 => EngineSheets.FileName(nameof(H.FourOnionRingsPhase3)),
        _ => EngineSheets.FileName(segment.ToString()),
    };

    /// <summary>
    /// Where one of the effects her fight lays on her room's acid has it go: the height, in pixels down from the
    /// room's top, how far it moves an update, and how many updates it waits first.
    /// </summary>
    /// <exception cref="InvalidDataException">The effect gives the acid nowhere to go, or no speed.</exception>
    private static JsonObject AcidMotion(SuperMetroid.Core.Fx.RoomEffectDefinition effect)
    {
        if (effect.LiquidTargetLevel is not int height || effect.LiquidLevelSpeed.Pixels == 0)
            throw new InvalidDataException("An effect of Mother Brain's fight gives her room's acid nowhere to go, or no speed.");
        return new JsonObject { ["height"] = height, ["speed"] = Math.Abs(effect.LiquidLevelSpeed.Pixels), ["delay"] = effect.LiquidMoveDelay };
    }

    /// <summary>Writes Mother Brain, everything of hers, and the giant Metroid of her fight.</summary>
    private void WriteMotherBrain()
    {
        List<(RoomDefinition Room, RoomStateSettings State)> states = StatesHolding(EnemyDefinitionId.MotherBrainBody);
        WriteMotherBrainTurret();
        WriteMotherBrainGlass(states);
        WriteMotherBrainHerself(states);
        WriteMotherBrainBaby();
    }

    /// <summary>Writes Mother Brain's own data file, the files of what she launches, and the patches of her room.</summary>
    private void WriteMotherBrainHerself(IReadOnlyList<(RoomDefinition Room, RoomStateSettings State)> states)
    {
        const string file = "bosses/mother_brain.json", folder = "bosses/mother_brain";
        const string scenes = "res://game/bosses/mother_brain/";
        const string family = GameInstallationLayout.EnemySheetDirectoryName;

        // A frame of her body is her far leg, which the installed sheet keeps apart, the rest of the old game's
        // sprites over it, and the frame of her body's sheet of the same name over both. She is hurt as her head is,
        // which is the part of her that can be.
        string standing = EnemySheetDefinitions.FrameName(EnemyFrameId.MotherBrainBodyStanding);
        string farLegOfStanding = MotherBrainBodyVisualDefinitions.SecondPictureName(EnemyFrameId.MotherBrainBodyStanding);
        if (!farLegOfStanding.EndsWith(standing, StringComparison.Ordinal) || farLegOfStanding.Length == standing.Length)
            throw new InvalidDataException($"Mother Brain's far leg is kept as {farLegOfStanding}, which is not her frame's name with something before it.");
        EngineSheetPictures pictures = _sheets.CutWithBossBodyAndBehind(_tree, _installation.ContentDirectory, folder, "MotherBrain",
            farLegOfStanding[..^standing.Length], _sheets.EnemySheet(EnemyDefinitionIdKeys.Installed.Of(EnemyDefinitionId.MotherBrainBody)));
        var mother = new EngineEnemy(_tree, _sheets, file, "MotherBrain", EnemyDefinitionId.MotherBrainHead, pictures);
        EngineSheetPictures head = _sheets.Cut(_tree, family, $"{folder}/head",
            _sheets.EnemySheet(EnemyDefinitionIdKeys.Installed.Of(EnemyDefinitionId.MotherBrainHead)));
        JsonObject HeadOffset() => new() { ["x"] = head.OffsetX - pictures.OffsetX, ["y"] = head.OffsetY - pictures.OffsetY };

        // ---- Her body ----
        static string? BodyCue(MotherBrainBodyCue cue) => cue switch
        {
            MotherBrainBodyCue.SetPose pose => "pose:" + pose.Pose switch
            {
                MotherBrainBodyPose.Standing => "standing",
                MotherBrainBodyPose.Walking => "walking",
                MotherBrainBodyPose.CrouchingTransition => "crouching",
                MotherBrainBodyPose.Crouched => "crouched",
                MotherBrainBodyPose.DeathBeam => "death_beam",
                MotherBrainBodyPose.LeaningDown => "leaning",
                _ => throw new InvalidDataException($"Mother Brain's body takes the pose {pose.Pose}, which this does not know."),
            },
            MotherBrainBodyCue.Move move => $"move:{move.Offset.X}:{move.Offset.Y}",
            MotherBrainBodyCue.StepAndMove step => $"step:{step.Offset.X}:{step.Offset.Y}",
            MotherBrainBodyCue.SpawnHandDust dust => $"hand_dust:{dust.Offset.X}:{dust.Offset.Y}:{DustFile(dust.Dust)}",
            MotherBrainBodyCue.Signal { Kind: MotherBrainBodyCueKind.FireHandBeam } => "fire_hand_beam",
            MotherBrainBodyCue.Signal { Kind: MotherBrainBodyCueKind.AdvanceHandBeamPhase } => "hand_beam_done",
            _ => throw new InvalidDataException($"Mother Brain's body raises {cue}, which this does not know."),
        };
        foreach ((B segment, Animation<B> animation) in MotherBrainAnimations.BodyStartingAt)
        {
            mother.Timeline(EngineSheets.FileName(segment.ToString()),
                WithShapeCues(EngineTimelines.Of(MotherBrainAnimations.BodySegments, animation, mother.Picture, BodyCue)));
        }

        // Where a shot meets her body, for each picture of it: the old game's rectangles of her legs, her trunk and
        // what is above it, each set where that picture has it.
        var shapes = new JsonObject();
        foreach ((EnemyFrameId frame, EnemyCollisionComponent[] components) in MotherBrainCollisionDefinitions.FrameComponents)
        {
            if (frame == EnemyFrameId.MotherBrainBrainHitArea)
                continue;
            string[][] parts = [["RightLeg"], ["Trunk", "Chest"], ["NeckPart", "Arm"], ["LeftLeg"]];
            if (components.Length != parts.Length)
                throw new InvalidDataException($"Mother Brain's body in {frame} is {components.Length} parts, where this knows {parts.Length}.");
            var colliders = new JsonObject();
            for (int part = 0; part < parts.Length; part++)
            {
                EnemyCollisionComponent component = components[part];
                if (component.Hitboxes.Count != parts[part].Length)
                    throw new InvalidDataException($"A part of Mother Brain's body in {frame} is {component.Hitboxes.Count} rectangles, where this knows {parts[part].Length}.");
                for (int box = 0; box < parts[part].Length; box++)
                {
                    EnemyCollisionHitbox hitbox = component.Hitboxes[box];
                    // These take shots alone (her touch goes by her own rule), so each is written as a shot meets it.
                    colliders[parts[part][box]] = EngineEnemy.ShotRect(component.Offset.X + hitbox.Left, component.Offset.Y + hitbox.Top,
                        component.Offset.X + hitbox.Right, component.Offset.Y + hitbox.Bottom);
                }
            }

            shapes[EngineEnemy.PoseName(frame)] = new JsonObject { ["colliders"] = colliders };
        }

        // ---- Her brain and her neck ----
        static string? HeadCue(MotherBrainHeadCue cue) => cue switch
        {
            MotherBrainHeadCue.PlaySound sound => EngineNames.SoundCue(sound.Effect),
            MotherBrainHeadCue.SpawnBomb bomb => $"bomb:{bomb.Afterburns}",
            MotherBrainHeadCue.Signal { Kind: var kind } => kind switch
            {
                MotherBrainHeadCueKind.ShakeBrain => "shake",
                MotherBrainHeadCueKind.SpawnDrool => "drool",
                MotherBrainHeadCueKind.SpawnBigPurpleBreath => "big_breath",
                MotherBrainHeadCueKind.DisableNeckMovement => "neck:off",
                MotherBrainHeadCueKind.EnableNeckMovement => "neck:on",
                MotherBrainHeadCueKind.AimOnionRingsAtSamus => "aim_rings",
                MotherBrainHeadCueKind.AimOnionRingsAtBabyMetroid => "aim_rings_at_baby",
                MotherBrainHeadCueKind.SpawnOnionRing => "ring",
                MotherBrainHeadCueKind.SpawnLaser => "laser",
                MotherBrainHeadCueKind.CountBabyMetroidAttack => "count_baby_attack",
                MotherBrainHeadCueKind.ResetBabyMetroidAttacks => "reset_baby_attacks",
                MotherBrainHeadCueKind.QueueBabyMetroidAttackSound => "baby_attack_sound",
                MotherBrainHeadCueKind.PrepareRainbowBeamCharge => "charge",
                MotherBrainHeadCueKind.SpawnRainbowBeamChargeParticle => "charge_particle",
                MotherBrainHeadCueKind.MaybeHoldNeutralPhase2 => "maybe_hold:" + MotherBrainHeadName(H.NeutralPhase2Hold),
                MotherBrainHeadCueKind.MaybeHoldNeutralPhase3 => "maybe_hold_late:" + MotherBrainHeadName(H.NeutralPhase3Hold),
                MotherBrainHeadCueKind.MaybeDroolAgain => "maybe_drool:" + MotherBrainHeadName(H.DyingDroolBurst),
                _ => throw new InvalidDataException($"Mother Brain's brain raises {kind}, which this does not know."),
            },
            _ => throw new InvalidDataException($"Mother Brain's brain raises {cue}, which this does not know."),
        };
        string HeadPicture(EnemyFrameId frame) => head.Of(EnemySheetDefinitions.FrameName(frame));
        foreach ((H segment, Animation<H> animation) in MotherBrainAnimations.HeadStartingAt)
        {
            JsonObject timeline = EngineTimelines.Of(MotherBrainAnimations.HeadSegments, animation, HeadPicture, HeadCue);
            timeline["offset"] = HeadOffset();
            mother.Timeline(MotherBrainHeadName(segment), timeline);
        }

        JsonObject neck = EngineTimelines.Of<EnemyFrameId, NoAnimationCue>(
            [new AnimationBeat<EnemyFrameId, NoAnimationCue>.HeldFrame(MotherBrainVisualDefinitions.NeckJointFrame)], null, HeadPicture, static _ => null);
        neck["offset"] = HeadOffset();
        mother.Timeline("neck", neck);

        // ---- What she launches ----
        // A ring grows as it forms: each step gives it its size, twice the old game's reach each way.
        static string? RingCue(MotherBrainEffectCue cue) => cue is MotherBrainEffectCue.SetRadius { Radius: var radius }
            ? $"size:{radius.X * 2}:{radius.Y * 2}"
            : throw new InvalidDataException($"A ring of Mother Brain's raises {cue}, which this does not know.");
        static string? NoEffectCue(MotherBrainEffectCue cue) =>
            throw new InvalidDataException($"Something of Mother Brain's raises {cue}, which this does not know.");
        static string? BreathCue(MotherBrainEffectCue cue) => cue is MotherBrainEffectCue.Signal
        {
            Kind: MotherBrainEffectCueKind.StopMoving or MotherBrainEffectCueKind.EndSmallPurpleBreath or MotherBrainEffectCueKind.Delete,
        }
            // A breath is an effect left where it was; it neither moves nor needs taking away, and the Godot
            // Mother Brain watches for her little breath's end herself.
            ? null
            : throw new InvalidDataException($"A breath of Mother Brain's raises {cue}, which this does not know.");
        static string? HangingCue(MotherBrainEffectCue cue) => cue switch
        {
            MotherBrainEffectCue.Signal { Kind: MotherBrainEffectCueKind.ReleaseDrool } => "fall",
            MotherBrainEffectCue.Signal { Kind: MotherBrainEffectCueKind.DropDrool } => "drop",
            // Its splash is where it landed already.
            MotherBrainEffectCue.Signal { Kind: MotherBrainEffectCueKind.StopMoving } => null,
            MotherBrainEffectCue.Signal { Kind: MotherBrainEffectCueKind.Delete } => "end",
            _ => throw new InvalidDataException($"Something that hangs from Mother Brain raises {cue}, which this does not know."),
        };
        IReadOnlyDictionary<MotherBrainEffectSegment, Animation<MotherBrainEffectSegment>> effects = MotherBrainEffectAnimations.StartingAt;
        string Effect(string name, MotherBrainEffectSegment segment, Func<MotherBrainEffectCue, string?> cue, bool rests = false) =>
            mother.SpawnedTimelineFrom(mother.Spawner, name, MotherBrainEffectAnimations.Segments, effects[segment], cue, rests);
        string explosionSound = EngineNames.SoundOrStop(MotherBrainSoundDefinitions.Explosion.SoundEffect);
        // The burst of a ring or a bomb joins only a queue holding at most two.
        string burstSound = EngineNames.Sound(MotherBrainSoundDefinitions.Explosion);

        // Her rings sit at her mouth, then fly straight; one bursts on Samus and where it leaves the fighting floor.
        mother.Shot("ring", RoomEnemyProjectileKind.MotherBrainOnionRing, (_, shot, projectile) =>
        {
            shot["speed"] = EngineNames.Number(MotherBrainOnionRingDefinitions.Speed);
            shot["scene"] = scenes + "mother_brain_ring.tscn";
            projectile["timeline"] = Effect("ring", MotherBrainEffectSegment.OnionRing, RingCue, rests: true);
            projectile["picture_offset"] = EngineEnemy.NoOffset();
            // The old ring is harmless to the common touch because Mother Brain's own code makes it hurt.
            projectile.Remove("delivers_hit");
            projectile["meeting_terrain"] = "passes_through";
            projectile["meeting_target"] = "ends";
            projectile["ending_effect"] = DustFile(DustExplosionAnimation.SmallExplosion);
            projectile["ending_sound"] = burstSound;
        });
        mother.ProjectileData("bosses/mother_brain_ring.json", "MotherBrainRing",
            ("mouth_updates", MotherBrainOnionRingDefinitions.MouthWaitUpdates), ("top", MotherBrainOnionRingDefinitions.ArenaTopY),
            ("floor", MotherBrainOnionRingDefinitions.ArenaFloorY), ("view_right", MotherBrainOnionRingDefinitions.ScreenRightEdgeX));

        // The fire a bomb leaves: the middle of it, from which flames race away both ways along the floor.
        static string? FireCue(RidleyFireballCue cue) => cue switch
        {
            RidleyFireballCue.StopMoving => "stop",
            RidleyFireballCue.SpawnHorizontalAfterburns => "flames",
            RidleyFireballCue.Delete => "end",
            _ => throw new InvalidDataException($"The fire a bomb of Mother Brain's leaves raises {cue}, which this does not know."),
        };
        mother.Shot("bomb_flare", RoomEnemyProjectileKind.RidleyAfterburnCenter, (owner, shot, projectile) =>
        {
            shot["scene"] = "res://game/bosses/ridley/ridley_flare.tscn";
            // The shot is written to a file named for it; its timeline has a name of its own.
            projectile["timeline"] = owner.SpawnedTimeline("bomb_fire", RidleyFireballAnimations.Segments,
                RidleyFireballAnimations.StartingAt[RidleyFireballSegment.HorizontalCenter], FireCue);
            projectile["picture_offset"] = EngineEnemy.NoOffset();
            projectile["meeting_terrain"] = "passes_through";
        });
        string flare = mother.ShotFile("bomb_flare");

        // Her bombs bounce along the floor; only the burst of one of Samus's bombs destroys one, and then it may
        // leave what her head would.
        mother.Shot("bomb", RoomEnemyProjectileKind.MotherBrainBomb, (_, shot, projectile) =>
        {
            shot["scene"] = scenes + "mother_brain_bomb.tscn";
            projectile["timeline"] = Effect("bomb", MotherBrainEffectSegment.Bomb, NoEffectCue);
            projectile["picture_offset"] = EngineEnemy.NoOffset();
            projectile["meeting_terrain"] = "passes_through";
            projectile["met_by_shot"] = "destroyed";
            if (Drops(RoomEnemyDefinitionCatalog.ById[EnemyDefinitionId.MotherBrainHead].Drops) is { } drops)
                projectile["drops"] = drops;
        });
        mother.ProjectileData("bosses/mother_brain_bomb.json", "MotherBrainBomb",
            ("flare", flare), ("dust_effect", DustFile(DustExplosionAnimation.SmallDustCloud)),
            ("explosion_effect", DustFile(DustExplosionAnimation.SmallExplosion)), ("explosion_sound", burstSound),
            ("floor", MotherBrainBombBounceDefinitions.FloorY), ("right_wall", MotherBrainBombBounceDefinitions.RightWallX));

        // The laser from her eye is a Space Pirate's, and hurts as much as she does.
        mother.Shot("laser", RoomEnemyProjectileKind.PirateMotherBrainLaser, (owner, shot, projectile) =>
        {
            PirateLaser(owner, shot, projectile);
            // The Godot Mother Brain sounds it herself.
            shot.Remove("sound");
            shot["speed"] = SpacePirateMotionDefinitions.WalkingFastLaserStep;
        });

        // Her drool hangs from her mouth, lets go, falls and splashes; the sparks of the beam she gathers hang at
        // her head until they are spent.
        string hanging = Effect("drool", MotherBrainEffectSegment.Drool, HangingCue, rests: true);
        string splash = Effect("drool_splash", MotherBrainEffectSegment.DroolHitFloor, HangingCue);
        foreach ((string name, RoomEnemyProjectileKind kind) in new[]
                 {
                     ("drool", RoomEnemyProjectileKind.MotherBrainDrool), ("dying_drool", RoomEnemyProjectileKind.MotherBrainDyingDrool),
                 })
        {
            mother.Shot(name, kind, (_, shot, projectile) =>
            {
                shot["scene"] = $"{scenes}mother_brain_{name}.tscn";
                projectile["timeline"] = hanging;
                projectile["picture_offset"] = EngineEnemy.NoOffset();
                projectile["meeting_terrain"] = "passes_through";
            });
        }

        mother.Shot("charge_particle", RoomEnemyProjectileKind.MotherBrainRainbowBeamCharging, (_, shot, projectile) =>
        {
            shot["scene"] = scenes + "mother_brain_charge_particle.tscn";
            projectile["timeline"] = Effect("charge_particle", MotherBrainEffectSegment.RainbowBeamCharging, HangingCue);
            projectile["picture_offset"] = EngineEnemy.NoOffset();
            projectile["meeting_terrain"] = "passes_through";
        });
        foreach (string hangs in new[] { "bosses/mother_brain_drool.json", "bosses/mother_brain_dying_drool.json", "bosses/mother_brain_charge_particle.json" })
        {
            mother.ProjectileData(hangs, "MotherBrainDrool",
                ("floor", MotherBrainFightDefinitions.DroolFloorY), ("release_drop", MotherBrainFightDefinitions.DroolReleaseDrop),
                ("splash_rise", MotherBrainFightDefinitions.DroolSplashRise), ("splash_timeline", splash));
        }

        // The beam from her hand: the glow at her hand and every piece of the beam lays down the next piece, three
        // times over, as its timeline goes.
        static string? HandBeamCue(MotherBrainHandBeamCue cue) => cue switch
        {
            MotherBrainHandBeamCue.SpawnNextBeam => "piece",
            MotherBrainHandBeamCue.Delete => "end",
            _ => throw new InvalidDataException($"The beam from Mother Brain's hand raises {cue}, which this does not know."),
        };
        string handBeam = mother.SpawnedTimeline("hand_beam", MotherBrainHandBeamAnimations.Segments,
            MotherBrainHandBeamAnimations.StartingAt[MotherBrainHandBeamSegment.Stage], HandBeamCue);
        foreach ((string name, RoomEnemyProjectileKind kind) in new[]
                 {
                     ("hand_beam_charge", RoomEnemyProjectileKind.MotherBrainHandBeamCharging), ("hand_beam", RoomEnemyProjectileKind.MotherBrainHandBeamFired),
                 })
        {
            mother.Shot(name, kind, (_, shot, projectile) =>
            {
                shot["speed"] = EngineNames.Number(MotherBrainHandBeamDefinitions.Speed);
                shot["scene"] = scenes + "mother_brain_hand_beam.tscn";
                projectile["timeline"] = handBeam;
                projectile["picture_offset"] = EngineEnemy.NoOffset();
                projectile["meeting_terrain"] = "passes_through";
            });
        }

        // ---- What her fight and her death do to her room ----
        const string fakeDeath = GameInstallationLayout.RoomObjectMotherBrainFakeDeathVisualDirectoryName;
        static (string Frame, RoomBlockShape Shape) Drawn(RoomObjectDraw draw) => draw switch
        {
            RoomObjectFrameDraw<MotherBrainFakeDeathDraw> change => (
                MotherBrainFakeDeathDrawKeys.Installed.Of(change.Frame), MotherBrainFakeDeathObjectDrawDefinitions.Shapes[change.Frame]),
            RoomObjectFrameDraw<MotherBrainFakeDeathDraws.MuralRowDraw> row => (
                $"mural-row-{row.Frame.Row + 1}",
                row.Frame.Shape == MotherBrainMuralRowShape.Border
                    ? MotherBrainFakeDeathObjectDrawDefinitions.MuralBorderRow
                    : MotherBrainFakeDeathObjectDrawDefinitions.MuralInteriorRow),
            _ => throw new InvalidDataException("Mother Brain's room is changed by something that is not one of her own frames."),
        };
        string Change(string name, RoomObjectSpawnRequest<MotherBrainRoomMutation> change)
        {
            TerrainChangeAt(states, name, change.Block, MotherBrainRoomMutationDefinitions.Changes[change.RoomObject], fakeDeath, Drawn, leaves: name);
            return name;
        }

        var sealedWall = new JsonArray();
        for (int wall = 0; wall < MotherBrainFightDefinitions.SealedWall.Count; wall++)
            sealedWall.Add(Change($"sealed_wall_{wall + 1}", MotherBrainFightDefinitions.SealedWall[wall]));
        string ceiling = Change("ceiling_clear", MotherBrainTubeCollapseDefinitions.CeilingBlock);
        var mural = new JsonArray();
        foreach (MotherBrainMuralRow row in MotherBrainMuralRow.All)
        {
            string name = $"mural_row_{mural.Count + 1}";
            TerrainChangeAt(states, name, row.Block, row.Reveal, fakeDeath, Drawn, leaves: name);
            mural.Add(name);
        }

        string escapeDoor = Change("escape_door", new(MotherBrainDeathDefinitions.EscapeDoorBlock, MotherBrainRoomMutation.EscapeDoor));

        // The pipes of her tank, which fall in turn: those that stand about it are the old game's falling pieces, and
        // those that hang above it the old game's thrown things.
        EngineSheetPictures tubes = _sheets.Cut(_tree, family, $"{folder}/tubes",
            _sheets.EnemySheet(EnemyDefinitionIdKeys.Installed.Of(EnemyDefinitionId.MotherBrainTubes)));
        string Piece(MotherBrainTubePiece piece)
        {
            string path = $"{folder}/tube_{EngineSheets.FileName(piece.ToString())}.json";
            JsonObject timeline = EngineTimelines.Of(MotherBrainTubeAnimations.Segments, MotherBrainTubeAnimations.StartingAt[piece],
                frame => tubes.Of(EnemySheetDefinitions.FrameName(frame)), static (NoAnimationCue _) => null);
            timeline["offset"] = new JsonObject { ["x"] = tubes.OffsetX, ["y"] = tubes.OffsetY };
            _tree.WriteData(path, timeline);
            return path;
        }

        var drops = new JsonArray();
        for (int index = 0; index < MotherBrainTubeCollapseDefinitions.Drops.Count; index++)
        {
            MotherBrainTubeDrop drop = MotherBrainTubeCollapseDefinitions.Drops[index];
            var listed = new JsonObject
            {
                ["patch"] = Change($"tube_gone_{index + 1}", drop.Clears),
                ["wait"] = drop is MotherBrainFollowedTubeDrop followed ? followed.WaitAfterUpdates : 0,
            };
            switch (drop)
            {
                case MotherBrainTopTubeDrop top:
                    listed["effect"] = mother.SpawnedTimeline($"tube_{EngineSheets.FileName(top.Segment.ToString())}", MotherBrainTopTubeAnimations.Segments,
                        MotherBrainTopTubeAnimations.StartingAt[top.Segment], static (NoAnimationCue _) => null);
                    (listed["x"], listed["y"]) = (top.Start.X, top.Start.Y);
                    listed["floor"] = MotherBrainTubeCollapseDefinitions.CeilingTubeFloorY;
                    listed["top"] = true;
                    break;
                case MotherBrainBottomTubeDrop bottom:
                    Fallen(listed, bottom.Piece);
                    break;
                case MotherBrainMainTubeDrop:
                    Fallen(listed, MotherBrainTubePiece.Main);
                    listed["main"] = true;
                    break;
                default:
                    throw new InvalidDataException("A pipe of Mother Brain's tank falls in a way this does not know.");
            }

            drops.Add(listed);
        }

        void Fallen(JsonObject listed, MotherBrainTubePiece piece)
        {
            Position16<RoomPixels> at = MotherBrainFallingTubePopulationDefinitions.Positions[piece];
            MotherBrainFallingTubeDefinition body = MotherBrainFallingTubePopulationDefinitions.Bodies[piece];
            listed["effect"] = Piece(piece);
            (listed["x"], listed["y"]) = (at.X, at.Y);
            listed["floor"] = body.Floor;
            if (body.FallWaitUpdates is { } wait)
                listed["delay"] = wait;
        }

        static JsonArray Places(IEnumerable<Position16<RoomPixels>> places) =>
            EngineNames.List(places.Select(place => (JsonNode)new JsonObject { ["x"] = place.X, ["y"] = place.Y }));
        static JsonArray Offsets(IEnumerable<Offset16<RoomPixels>> places) =>
            EngineNames.List(places.Select(place => (JsonNode)new JsonObject { ["x"] = place.X, ["y"] = place.Y }));
        static JsonArray Rects(IReadOnlyList<EnemyHitboxRectangle> rects) => EngineNames.List(rects.Select(rect => (JsonNode)new JsonObject
        {
            ["left"] = rect.Left, ["top"] = rect.Top, ["right"] = rect.Right, ["bottom"] = rect.Bottom,
        }));

        EnemyCollisionHitbox brain = MotherBrainCollisionDefinitions.BrainHitboxes.Single();
        mother.BodyColliders("Body")
            // Her brain's hurtbox, written as a shot meets the old box: a pixel more at its left and top.
            .Collider("Brain", brain.Left - 1, brain.Top - 1, brain.Right - brain.Left + 1, brain.Bottom - brain.Top + 1)
            .Value("first_phase_health", MotherBrainFightDefinitions.FirstPhaseHealth)
            .Value("second_phase_health", MotherBrainFightDefinitions.SecondPhaseHealth)
            .Value("third_phase_health", MotherBrainFightDefinitions.ThirdPhaseHealth)
            .Value("full_strength_health", MotherBrainFightDefinitions.FullStrengthHealth)
            .Value("hurt_sound", EngineNames.Sound(EnemySound.ShotMotherBrain))
            .Value("explosion_sound", explosionSound)
            .Value("dust_effect", DustFile(DustExplosionAnimation.SmallDustCloud))
            .Value("big_dust_effect", DustFile(DustExplosionAnimation.ShortBigDustCloud))
            .Value("explosion_effect", DustFile(DustExplosionAnimation.SmallExplosion))
            .Value("smoke_effect", DustFile(DustExplosionAnimation.Smoke))
            .Value("big_explosion_effect", DustFile(DustExplosionAnimation.BigExplosion))
            .Value("fake_death_grey_step_updates", MotherBrainWaitDefinitions.FakeDeathGreyStepUpdates)
            .Value("grey_steps", MotherBrainFakeDeathFadeDefinitions.FrameCount)
            .Value("from_grey_step_updates", MotherBrainWaitDefinitions.FromGreyStepUpdates)
            .Value("drained_grey_step_updates", MotherBrainFightDefinitions.GreyStepUpdates)
            .Value("ascent_dust_sound", EngineNames.Sound(MotherBrainSoundDefinitions.AscentDust))
            .Value("footstep_sound", EngineNames.Sound(RoomSound.MotherBrainFootstep))
            .Value("hand_beam_sound", EngineNames.Sound(MotherBrainSoundDefinitions.HandBeam))
            .Value("laser_sound", EngineNames.Sound(EnemySoundEffectDefinitions.PirateMotherBrainLaserSound))
            .Value("charge_sound", EngineNames.Sound(MotherBrainSoundDefinitions.RainbowBeamCharge))
            .Value("charge_stop_sound", EngineNames.SoundOrStop(EnemySoundStop.Any))
            .Value("rainbow_beam_sound", EngineNames.Sound(MotherBrainSoundDefinitions.StartRainbowDrain))
            .Value("rainbow_beam_stop_sound", EngineNames.SoundOrStop(MotherBrainSoundDefinitions.StopRainbowDrain))
            .Value("attack_cry_sound", EngineNames.Sound(MotherBrainSoundDefinitions.AttackCry))
            .Value("small_breath_effect", Effect("breath_small", MotherBrainEffectSegment.PurpleBreathSmall, BreathCue))
            .Value("big_breath_effect", Effect("breath_big", MotherBrainEffectSegment.PurpleBreathBig, BreathCue))
            .Value("samus_stands_energy", MotherBrainLateFightDefinitions.SamusStandsInBeamHealth)
            .Value("finish_off_energy", MotherBrainLateFightDefinitions.FinishOffSamusHealth)
            .Value("corpse_dust_effect", DustFile(DustExplosionAnimation.CorpseDustCloud))
            // The dust of her rotting corpse joins only a queue holding at most two.
            .Value("corpse_dust_sound", EngineNames.Sound(EnemySound.LiquidDamage, SoundRequestPriority.Low))
            .Value("door_fragment_effect", Effect("door_fragment", MotherBrainEffectSegment.EscapeDoorParticle, NoEffectCue))
            .Group("shapes", shapes)
            .Group("contacts", new JsonObject
            {
                ["body"] = Rects(MotherBrainContactHitboxDefinitions.Parts[MotherBrainContactPart.Body]),
                ["brain"] = Rects(MotherBrainContactHitboxDefinitions.Parts[MotherBrainContactPart.Brain]),
                ["neck"] = Rects(MotherBrainContactHitboxDefinitions.Parts[MotherBrainContactPart.Neck]),
            })
            .Group("tube_drops", drops)
            .Group("fake_death_explosions", Places(MotherBrainTubeCollapseDefinitions.FakeDeathExplosionPositions))
            .Group("ascent_dust", Places(MotherBrainAscentDustDefinitions.DustPositions))
            .Group("sealed_wall_dust", Places(MotherBrainFightDefinitions.SealedWallDust))
            .Group("sealed_wall_patches", sealedWall)
            .Group("mural_patches", mural)
            .Group("ceiling_patch", JsonValue.Create(ceiling))
            .Group("rainbow_explosions", Offsets(MotherBrainBeamDefinitions.ExplosionOffsets))
            .Group("death_explosions", EngineNames.List(MotherBrainDeathExplosionDefinitions.Groups.Select(group => (JsonNode)Offsets(group.Mixed))))
            .Group("escape_door_dust", Places(MotherBrainDeathDefinitions.EscapeDoorDustPositions))
            .Group("escape_door_fragments", EngineNames.List(MotherBrainDoorFragmentDefinitions.Fragments.Select(piece => (JsonNode)new JsonObject
            {
                ["down"] = piece.Offset.Y,
                ["rise"] = EngineNames.Number(piece.Velocity.Y),
            })))
            .Group("escape_door_patch", JsonValue.Create(escapeDoor))

            // Her room's acid: up as her body is set up, and drained as her false death starts the room's flash. Her
            // fight gives it where to go, how fast and after how long; her room's own effect only has it at rest.
            .Group("liquid", new JsonObject
            {
                ["rise"] = AcidMotion(MotherBrainRoomFxDefinitions.AcidRises),
                ["drain"] = AcidMotion(MotherBrainRoomFxDefinitions.AcidDrains),
            })

            // Her room's lights: out as the giant Metroid is struck its last, and each step of their coming back,
            // the last of which stays. The changes are of her room's own pictures, its blocks and what lies behind.
            .Group("room_lights", new JsonObject
            {
                ["out"] = ShadeChange(MotherBrainShades.LightsOut.Key, RoomPlacesOf(EnemyDefinitionId.MotherBrainBody)),
                ["back"] = EngineNames.List(MotherBrainFadeDefinitions.RecoveryLights.Images.Select(
                    image => (JsonNode)ShadeChange(MotherBrainShades.Fade(image).Key, RoomPlacesOf(EnemyDefinitionId.MotherBrainBody)))),
            })
            .Write();
    }

    /// <summary>
    /// Gives every step of one of the timelines of Mother Brain's body that shows a picture the cue that puts the
    /// rectangles of her body where that picture has them, ahead of the step's other cues.
    /// </summary>
    private static JsonObject WithShapeCues(JsonObject timeline)
    {
        foreach (JsonNode? step in timeline["steps"]!.AsArray())
        {
            string picture = step!["picture"]!.GetValue<string>();
            if (picture.Length == 0)
                continue;
            string name = picture[(picture.LastIndexOf('/') + 1)..picture.LastIndexOf('.')];
            step["cues"] = Prepended("shape:" + name, step["cues"] as JsonArray);
        }

        return timeline;
    }

    /// <summary>Writes the giant Metroid that comes to Samus's aid against Mother Brain.</summary>
    private void WriteMotherBrainBaby()
    {
        EngineEnemy baby = Enemy("bosses/mother_brain_baby.json", "MotherBrainBaby", EnemyDefinitionId.BabyMetroidCutscene);
        string Picture(BabyMetroidFrame frame) => baby.Picture(BabyMetroidVisualDefinitions.ShownFrames.Of(frame));
        foreach ((MotherBrainBabySegment segment, Animation<MotherBrainBabySegment> animation) in MotherBrainBabyAnimations.StartingAt)
        {
            baby.Timeline(EngineSheets.FileName(segment.ToString()),
                EngineTimelines.Of(MotherBrainBabyAnimations.Segments, animation, Picture, static (NoAnimationCue _) => null));
        }

        // It is as big, for what Mother Brain's rings and Samus meet, as the old one's own measure of it.
        int across = BabyMetroidCutsceneState.XHitboxRadius, down = BabyMetroidCutsceneState.YHitboxRadius;
        baby.Collider("Body", -across, -down, across * 2, down * 2)
            .Collider("Hurtbox", -across, -down, across * 2, down * 2)
            .Value("cry_sound", EngineNames.Sound(BabyMetroidSoundDefinitions.DrainCry))
            .Value("remorse_cry_sound", EngineNames.Sound(BabyMetroidSoundDefinitions.RemorseCry))
            .Value("death_sound", EngineNames.Sound(RoomSound.BabyMetroidDeath))
            .Value("heal_sound", EngineNames.Sound(RoomSound.IncrementalEnergy))
            .Value("explosion_sound", EngineNames.Sound(MotherBrainSoundDefinitions.Explosion))
            .Value("dust_effect", DustFile(DustExplosionAnimation.SmallDustCloud))
            .Value("explosion_effect", DustFile(DustExplosionAnimation.SmallExplosion))
            .Value("shielding_health", BabyMetroidCutsceneDefinitions.ArrivalHealth)
            .Write();
    }
}
