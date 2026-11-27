using System.Globalization;
using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Bosses.Draygon.Artwork;
using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Earthquakes;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.EffectActors;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>Draygon, its goop, and the cannons in the walls of its chamber.</summary>
public sealed partial class EngineAssetWriter
{
    /// <summary>The scene of a gob of Draygon's goop, which is a projectile of a class of its own.</summary>
    private const string DraygonGoopScene = "res://game/bosses/draygon/draygon_goop.tscn";

    /// <summary>The scene of a wall cannon's shot, which is a projectile of a class of its own.</summary>
    private const string DraygonCannonShotScene = "res://game/bosses/draygon/draygon_cannon_shot.tscn";

    /// <summary>What stands in the names of the pictures of Draygon's body for the band of its health.</summary>
    private const string DraygonHealthPlaceholder = "{health}";

    /// <summary>
    /// Writes Draygon: its body's and parts' timelines, where it can be hit facing each way, its brood, its sounds,
    /// its goop, and its wall cannons with their shot.
    /// </summary>
    private void WriteDraygon()
    {
        const string file = "bosses/draygon.json";
        string folder = file[..file.LastIndexOf('.')];
        BossBody body = CutBossBody("Draygon", folder);
        EngineEnemy draygon = BossWithBody(file, "Draygon", EnemyDefinitionId.DraygonBody, body, EnemyFrameId.DraygonBodyFacingLeftJawShut);
        (int X, int Y) origin = body.Anchor(EnemyFrameId.DraygonBodyFacingLeftJawShut);

        // What it leaves about it, each drawn as its own chamber draws it.
        var effects = new Dictionary<RoomEffectActorKind, string>();
        string Effect(RoomEffectActorKind kind)
        {
            if (!effects.TryGetValue(kind, out string? written))
                effects[kind] = written = draygon.SpawnedEffect(EngineSheets.FileName(kind.ToString()), kind);
            return written;
        }

        string? Cue(DraygonCue cue) => cue switch
        {
            // The Godot Draygon starts its parts itself.
            DraygonCue.StartPartAnimations => null,
            // Its eye follows Samus from here, looking out to the side named, which the eye's blink names as left
            // whichever way Draygon faces.
            DraygonCue.TrackWithEye track => "track_eye:" + (track.Facing == HorizontalDirection.Right ? "right" : "left"),
            DraygonCue.ShiftBodyPicture shift => string.Create(CultureInfo.InvariantCulture, $"shift:{(int)shift.Shift.X}:{(int)shift.Shift.Y}"),
            DraygonCue.QueueSound sound => EngineNames.SoundCue(sound.Sound)
                ?? throw new InvalidDataException($"Draygon's animation asks for the sound {sound.Sound}, which has no name."),
            DraygonCue.SpawnDyingEffect dying => "dying_effect:" + Effect(dying.Kind),
            DraygonCue.ParalyseTailAndArms => "paralyse",
            DraygonCue.SetBodyIntangible => "intangible",
            DraygonCue.ReleaseSamus => "release",
            DraygonCue.TailWhipHit => "whip_hit",
            DraygonCue.SpawnGoop => "goop",
            _ => throw new InvalidDataException($"Draygon's animation raises {cue}, which this does not know."),
        };

        // Its body and its eye are of its body's sheet; a dying Draygon's limp arm is a picture of its whole body.
        void OfBody(string name, Animation<DraygonSegment> animation) =>
            draygon.Timeline(name, BossBodyTimeline(body, DraygonAnimations.Segments, animation, origin, Cue));

        // Its tail and its arms are sheets of their own, each with its own corner, measured from the body's.
        EngineSheetPictures tail = EnemyPictures(folder + "_tail", EnemyDefinitionIdKeys.Installed.Of(EnemyDefinitionId.DraygonTail));
        EngineSheetPictures arms = EnemyPictures(folder + "_arms", EnemyDefinitionIdKeys.Installed.Of(EnemyDefinitionId.DraygonArms));
        void OfPart(string name, EngineSheetPictures pictures, Animation<DraygonSegment> animation)
        {
            JsonObject timeline = EngineTimelines.Of(DraygonAnimations.Segments, animation, frame => PictureOf(pictures, frame), Cue);
            if ((pictures.OffsetX, pictures.OffsetY) != origin)
                timeline["offset"] = new JsonObject { ["x"] = pictures.OffsetX - origin.X, ["y"] = pictures.OffsetY - origin.Y };
            draygon.Timeline(name, timeline);
        }

        static Animation<DraygonSegment> Starting(DraygonSegment segment) => DraygonAnimations.StartingWith(segment);

        OfBody("body_left", Starting(DraygonSegment.BodyFacingLeftIdle));
        OfBody("body_right", Starting(DraygonSegment.BodyFacingRightIdle));
        OfBody("fire_goop_left", Starting(DraygonSegment.BodyFacingLeftFireGoop));
        OfBody("fire_goop_right", Starting(DraygonSegment.BodyFacingRightFireGoop));
        OfBody("roar_left", Starting(DraygonSegment.BodyFacingLeftRoar));
        OfBody("roar_right", Starting(DraygonSegment.BodyFacingRightRoar));
        OfBody("dying_left", Starting(DraygonSegment.BodyFacingLeftDying));
        OfBody("dying_right", Starting(DraygonSegment.BodyFacingRightDying));
        // At rest it shows nothing new: the Godot Draygon stays as it was shown.
        if (Starting(DraygonSegment.Rest).Phases.Count != 0)
            throw new InvalidDataException("Draygon's rest shows something, which the Godot Draygon's does not.");
        draygon.Timeline("rest", NothingShown());

        OfBody("eye_idle_left", Starting(DraygonSegment.EyeFacingLeftIdle));
        OfBody("eye_idle_right", Starting(DraygonSegment.EyeFacingRightIdle));
        OfBody("eye_left_looking_up", Starting(DraygonSegment.EyeFacingLeftLookingUp));
        OfBody("eye_left_looking_down", Starting(DraygonSegment.EyeFacingLeftLookingDown));
        OfBody("eye_left_looking_left", Starting(DraygonSegment.EyeFacingLeftLookingLeft));
        OfBody("eye_left_looking_right", Starting(DraygonSegment.EyeFacingLeftLookingRight));
        OfBody("eye_right_looking_up", Starting(DraygonSegment.EyeFacingRightLookingUp));
        OfBody("eye_right_looking_down", Starting(DraygonSegment.EyeFacingRightLookingDown));
        OfBody("eye_right_looking_left", Starting(DraygonSegment.EyeFacingRightLookingLeft));
        OfBody("eye_right_looking_right", Starting(DraygonSegment.EyeFacingRightLookingRight));
        OfBody("eye_dying_left", Starting(DraygonSegment.EyeFacingLeftDying));
        OfBody("eye_dying_right", Starting(DraygonSegment.EyeFacingRightDying));
        OfBody("eye_dead_left", Starting(DraygonSegment.EyeFacingLeftDead));
        OfBody("eye_dead_right", Starting(DraygonSegment.EyeFacingRightDead));

        OfPart("tail_left", tail, Starting(DraygonSegment.TailFacingLeftIdle));
        OfPart("tail_right", tail, Starting(DraygonSegment.TailFacingRightIdle));
        // Its tail's feint as the fight opens, which the cartridge plays only facing left, before it swings idly.
        OfPart("tail_fake_whip_left", tail, Starting(DraygonSegment.TailFacingLeftInitialFakeWhip));
        OfPart("tail_whip_left", tail, Starting(DraygonSegment.TailFacingLeftWhip));
        OfPart("tail_whip_right", tail, Starting(DraygonSegment.TailFacingRightWhip));
        OfPart("tail_final_whips_left", tail, DraygonAnimations.TailFacingLeftFinalWhips);
        OfPart("tail_final_whips_right", tail, DraygonAnimations.TailFacingRightFinalWhips);
        OfPart("arms_left", arms, Starting(DraygonSegment.ArmsFacingLeftIdle));
        OfPart("arms_right", arms, Starting(DraygonSegment.ArmsFacingRightIdle));
        OfPart("arms_reach_left", arms, Starting(DraygonSegment.ArmsFacingLeftNearSwoopApex));
        OfPart("arms_reach_right", arms, Starting(DraygonSegment.ArmsFacingRightNearSwoopApex));
        OfPart("arms_grab_left", arms, Starting(DraygonSegment.ArmsFacingLeftGrab));
        OfPart("arms_grab_right", arms, Starting(DraygonSegment.ArmsFacingRightGrab));

        // Where it can be hit, facing each way: its belly, which shots hurt and which does not hurt to touch, and the
        // three parts of its shell, which stop shots and hurt to touch.
        JsonObject Boxes(EnemyCollisionHitbox[] boxes, bool sized)
        {
            if (boxes.Length != 4 || !ReferenceEquals(boxes[0].Touch, EnemyReaction.Ignore) || !ReferenceEquals(boxes[0].Shot, DraygonReactions.Shot)
                || boxes.Skip(1).Any(shell => !ReferenceEquals(shell.Touch, DraygonReactions.Touch) || !shell.Shot.IsDud))
                throw new InvalidDataException("Draygon is not a belly that shots hurt and three parts of a shell that stop them, as the Godot Draygon is.");
            var placed = new JsonObject();
            // The old probe meets a shot and Samus's touch on different edges of a box, a pixel apart: a hurtbox is
            // written for the one and a hitbox for the other.
            void Place(string name, EnemyCollisionHitbox box, bool touch)
            {
                JsonObject rect = touch ? EngineEnemy.TouchRect(box) : EngineEnemy.ShotRect(box);
                if (sized)
                {
                    draygon.Collider(
                        name, rect["left"]!.GetValue<int>(), rect["top"]!.GetValue<int>(), rect["width"]!.GetValue<int>(), rect["height"]!.GetValue<int>());
                }

                placed[name] = rect;
            }

            Place("Belly", boxes[0], touch: false);
            for (int shell = 1; shell < boxes.Length; shell++)
            {
                Place($"Shell{shell}", boxes[shell], touch: false);
                Place($"Shell{shell}Touch", boxes[shell], touch: true);
            }

            return placed;
        }

        // Its pictures of each band of its health, as that band colours them.
        const string family = GameInstallationLayout.BossBodySheetDirectoryName;
        string sheet = _sheets.SheetNamed(family, BossBodySheetPrefix + body.Boss);
        IReadOnlyList<DraygonHealthBand> bands = DraygonHealthBandDefinitions.HighestFirst;
        for (int band = 0; band < bands.Count; band++)
            _sheets.CutShade(_tree, family, folder + "_health_" + band.ToString(CultureInfo.InvariantCulture), sheet, DraygonShades.Health(bands[band]).Key);

        // His hurt flash, shown on the updates the engine's flash shows: his tail and his arms in his sprites' white laid
        // over his opening's colours, and his body and eye in his body's white, whichever band of his health they are in.
        draygon.Group("hurt_colours", ShadeChange(DraygonShades.Intro.Key + "+" + DraygonShades.SpriteWhite.Key, GameInstallationLayout.EnemySheetDirectoryName,
            new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal)
            {
                [folder + "_tail"] = [_sheets.EnemySheet(EnemyDefinitionIdKeys.Installed.Of(EnemyDefinitionId.DraygonTail))],
                [folder + "_arms"] = [_sheets.EnemySheet(EnemyDefinitionIdKeys.Installed.Of(EnemyDefinitionId.DraygonArms))],
            }));
        for (int band = 0; band < bands.Count; band++)
        {
            draygon.FlashShaded(folder + "_health_" + band.ToString(CultureInfo.InvariantCulture), family, sheet,
                DraygonShades.Health(bands[band]).Key, DraygonShades.BodyWhite.Key);
        }

        static JsonObject Field(RandomSampleField field) => new() { ["from"] = field.Shift, ["digits"] = field.Width };

        // Its brood: those that dance before the fight, and those that bury it. The Godot Draygon counts a heading
        // from the right through down, and one of the brood drifts back along the heading the old game counts up
        // from the right.
        string BroodEffect(RoomEffectActorKind kind) => kind is RoomEffectActorKind.EvirFacingLeft or RoomEffectActorKind.EvirFacingRight
            ? Effect(kind)
            : throw new InvalidDataException($"One of Draygon's brood is drawn as {kind}, which is not one of its brood.");
        var buriers = new JsonArray();
        foreach (DraygonBurialEvirDefinition evir in DraygonBurialEvirDefinitions.InSpawnOrder)
        {
            buriers.Add(new JsonObject
            {
                ["x"] = (int)evir.Start.X,
                ["y"] = (int)evir.Start.Y,
                ["heading"] = (TrigonometryAngle.HalfTurn - evir.Heading).RawValue,
                ["effect"] = BroodEffect(evir.Kind),
            });
        }

        // Its cannons' turns, in the order a number drawn picks them.
        var turns = new JsonArray();
        foreach (DraygonTurretTurn turn in EngineNames.Each(DraygonCannonData.FiringCannon))
        {
            if (turn is DraygonTurretTurn.FiresFrom fires)
            {
                Position16<RoomPixels> from = DraygonCannonData.FiringOrigins[fires.Cannon];
                turns.Add(new JsonObject { ["cannon"] = fires.Cannon.ToString(), ["x"] = (int)from.X, ["y"] = (int)from.Y });
            }
            else
            {
                turns.Add(new JsonObject { ["cannon"] = "", ["x"] = 0, ["y"] = 0 });
            }
        }

        JsonObject left = Boxes(DraygonCollisionDefinitions.FacingLeftBodyHitboxes, sized: true);
        JsonObject right = Boxes(DraygonCollisionDefinitions.FacingRightBodyHitboxes, sized: false);
        draygon
            .Value("swim_line_y", DraygonArenaDefinitions.SwimLineY)
            .Value("right_side_distance", DraygonArenaDefinitions.RightSideResetDistance)
            .Value("intro_updates", DraygonArenaDefinitions.IntroUpdates)
            .Value("intro_dance_updates", DraygonIntroDanceDefinitions.DurationFrames)
            .Value("swoop_quickening", EngineNames.Number(DraygonSwoopDefinitions.InitialAcceleration))
            .Value("swoop_quickening_per_hit", EngineNames.Number(DraygonSwoopDefinitions.ShotAccelerationStep))
            .Value("swoop_quickening_limit", EngineNames.Number(DraygonSwoopDefinitions.AccelerationLimit))
            .Value("goop_range", DraygonArenaDefinitions.GoopTailRange)
            .Value("goops_per_pass", DraygonArenaDefinitions.GoopsPerPass)
            .Value("chase_speed", DraygonGrabDefinitions.ChaseSpeed)
            .Value("escape_presses", SamusDraygonHoldDefinitions.EscapePresses)
            .Value("grapple_shock_damage", DraygonArenaDefinitions.GrappleShockDamage)
            // The Godot game counts the strength of a shake from one, the lightest.
            .Value("whip_shake_strength", (int)EarthquakeStrength.Strong - (int)EarthquakeStrength.Light + 1)
            .Value("death_drops", DraygonArenaDefinitions.DeathDropCount)
            .Value("dancer_effect", BroodEffect(RoomEffectActorKind.EvirFacingLeft))
            .Value("foam_effect", Effect(RoomEffectActorKind.DraygonFoam))
            .Value("bubble_effect", Effect(RoomEffectActorKind.DraygonBreathBubbles))
            .Value("eye_bubble_effect", DustFile(DraygonEyeEffects.DustVariant))
            .Value("dust_effect", Effect(RoomEffectActorKind.BigDustCloud))
            .Value("smoke_effect", Effect(RoomEffectActorKind.BigDustCloud))
            .Group("facing_colliders", new JsonObject { ["left"] = left, ["right"] = right })
            .Group("health_bands", EngineNames.List(bands.Select(band => band.MinimumHealth)))
            .Group("intro_dance", EngineNames.List(DraygonIntroDanceDefinitions.TracedPath.Select(step => (JsonNode)new JsonObject { ["x"] = (int)step.X, ["y"] = (int)step.Y })))
            .Group("intro_dancer_delays", EngineNames.List(DraygonIntroDanceDefinitions.DancerStartDelays))
            .Group("burial_brood", buriers)
            .Group("cannon_choice", Field(DraygonRandomDefinitions.TurretChoice))
            .Group("cannon_turns", turns)
            .Group("scattered_drops", Drops(draygon.Definition.Drops) ?? throw new InvalidDataException("Draygon scatters nothing when it is gone."))
            .Group("drop_scatter", new JsonObject
            {
                ["x"] = (int)DraygonArenaDefinitions.DeathDropOrigin.X,
                ["y"] = (int)DraygonArenaDefinitions.DeathDropOrigin.Y,
                ["across"] = Field(EnemyDropScatterDefinitions.WideArenaHorizontalScatter),
                ["down"] = Field(EnemyDropScatterDefinitions.ArenaBandVerticalScatter),
            })
            // What its dying leaves lies as far either way of it as the scatter reaches.
            .Group("dying_scatter", new JsonObject
            {
                ["x"] = -DraygonArenaDefinitions.DyingScatterReach,
                ["y"] = -DraygonArenaDefinitions.DyingScatterReach,
                ["across"] = Field(DraygonRandomDefinitions.DyingScatterX),
                ["down"] = Field(DraygonRandomDefinitions.DyingScatterY),
            })
            .PictureSets(DraygonHealthPlaceholder);
        // It leaves nothing of its own as it goes: what it scatters is all.
        draygon.Data.Remove("drops");
        draygon.Data.Remove("death");

        // Its goop. A gob flies, sticks to Samus, and a shot bursts it, which may leave something.
        static string? GoopCue(DraygonProjectileCue cue) => cue switch
        {
            // The Godot gob sticks by itself, and what a burst one leaves is in its shot.
            DraygonProjectileCue.StickToSamus or DraygonProjectileCue.SpawnDrops => null,
            DraygonProjectileCue.Delete => "end",
            DraygonProjectileCue.Fire => "fire",
            _ => throw new InvalidDataException($"Something Draygon launches raises {cue}, which this does not know."),
        };
        string Launched(string name, DraygonProjectileSegment segment) =>
            draygon.SpawnedTimeline(name, DraygonProjectileAnimations.Segments, DraygonProjectileAnimations.StartingAt[segment], GoopCue);

        string goop = Launched("goop", DraygonProjectileSegment.Goop);
        draygon.ProjectileData("bosses/draygon_goop.json", "DraygonGoop",
            ("reach_across", (int)DraygonArenaDefinitions.GoopTouchReach.X),
            ("reach_down", (int)DraygonArenaDefinitions.GoopTouchReach.Y),
            // One more than this would slow her past the old game's limit, and does not stick.
            ("most_stuck", DraygonArenaDefinitions.GoopSlowdownLimit - 1),
            ("stuck_updates", DraygonArenaDefinitions.GoopAttachedLifetime),
            ("stack_top", DraygonArenaDefinitions.GoopStackTop),
            ("stack_spacing", DraygonArenaDefinitions.GoopStackSpacing),
            ("chamber_size", DraygonArenaDefinitions.ArenaSize),
            ("stuck_timeline", Launched("goop_stuck", DraygonProjectileSegment.GoopTouch)));
        draygon.Shot("goop", RoomEnemyProjectileKind.DraygonGoop, (_, shot, projectile) =>
            {
                shot["scene"] = DraygonGoopScene;
                shot["speed"] = DraygonProjectileSpeeds.Goop;
                projectile["timeline"] = goop;
                projectile["picture_offset"] = EngineEnemy.NoOffset();
                projectile["meeting_terrain"] = "passes_through";
                // A shot one stops where it is and bursts, harming to the touch while it does; touching Samus it is
                // only taken away.
                projectile["shot_timeline"] = Launched("goop_burst", DraygonProjectileSegment.GoopShot);
                // What a burst gob leaves, as its burst ends, is what the old game lists for Draygon's eye.
                if (Drops(RoomEnemyDefinitionCatalog.ById[EnemyDefinitionId.DraygonEye].Drops) is { } drops)
                    projectile["drops"] = drops;
            });

        // The shot of its wall cannons gathers at the muzzle, and flies at the cue.
        EnemyProjectileDefinition turret = EnemyProjectileDefinitionCatalog.Definitions[RoomEnemyProjectileKind.DraygonWallTurret];
        var cannonShot = new JsonObject
        {
            ["damage"] = turret.Damage,
            ["scene"] = DraygonCannonShotScene,
            ["speed"] = DraygonProjectileSpeeds.WallTurret,
            ["projectile"] = new JsonObject
            {
                // Something of no size still lies somewhere: one pixel.
                ["width"] = Math.Max(1, turret.Radius.X * 2),
                ["height"] = Math.Max(1, turret.Radius.Y * 2),
                ["meeting_terrain"] = "passes_through",
                ["meeting_target"] = turret.Properties.HasFlag(EnemyProjectilePropertyFlags.PersistsOnContact) ? "passes_through_and_rehits" : "ends",
                ["met_by_shot"] = turret.Properties.HasFlag(EnemyProjectilePropertyFlags.BlocksShots) ? "destroyed" : "ignored",
                ["standing"] = EngineEnemy.StandingOf(turret),
                // It is taken away once out of the chamber, wherever the view is, which its own data says.
                ["view_margin"] = -1,
                ["timeline"] = Launched("cannon_shot", DraygonProjectileSegment.WallTurretBloom),
                ["picture_offset"] = EngineEnemy.NoOffset(),
                // One that touches her is gone with nothing left. Her shots pass through it, so the burst the old
                // game names for one they meet never shows.
            },
        };
        draygon.ProjectileData("bosses/draygon_cannon_shot.json", "DraygonCannonShot", ("chamber_size", DraygonArenaDefinitions.ArenaSize));
        draygon.Write();
        WriteDraygonCannon(cannonShot);
    }

    /// <summary>
    /// Writes a cannon of Draygon's chamber. The old game made it of the room's own blocks, so its pictures are
    /// written for every set of graphics. Whole, only missiles harm it: as many as the old game counts, or one super
    /// missile. Broken, it is the old game's spikes: it hurts to touch, and drains whoever's grapple beam holds it.
    /// </summary>
    private void WriteDraygonCannon(JsonObject shot)
    {
        const string directory = GameInstallationLayout.RoomObjectDraygonCannonVisualDirectoryName;
        const string file = "bosses/draygon_cannon.json";
        string folder = file[..file.LastIndexOf('.')];
        string Picture(DraygonCannonDraw draw) => RoomObjects.Picture(directory, DraygonCannonDrawKeys.Installed.Of(draw), folder);

        // One of its animations; one that stops says so as it does, for the cannon to put its shield back.
        JsonObject Timeline(Animation<DraygonCannonSegment> animation)
        {
            JsonObject timeline = EngineTimelines.Of(DraygonCannonAnimationDefinitions.Segments, animation, Picture, static (NoAnimationCue _) => null);
            if (animation.End is AnimationEnd.Stop)
            {
                JsonArray steps = timeline["steps"]!.AsArray();
                steps.Add(new JsonObject { ["picture"] = steps[^1]!["picture"]!.GetValue<string>(), ["updates"] = 1, ["cues"] = new JsonArray("shield") });
            }

            return timeline;
        }

        // Where the corner of its pictures lies from its block, facing each way, and the blocks of it that are solid.
        // Its pictures are of the room's blocks, and lie a row up with the room's tiles.
        JsonObject Corner(DraygonCannonDraw shield)
        {
            (int column, int row, int _, int _) = RoomObjects.Bounds(directory, DraygonCannonDrawKeys.Installed.Of(shield));
            return EngineRoomObjects.PictureCorner(column * EngineRoomObjects.Block, row * EngineRoomObjects.Block);
        }

        (int Left, int Top, int Width, int Height) Core(DraygonCannonDraw draw)
        {
            var solid = DraygonCannonObjectDrawDefinitions.Shapes[draw].Cells.Where(cell => cell.Collision != RoomCollisionType.Air).ToList();
            int left = solid.Min(cell => (int)cell.Cell.X), top = solid.Min(cell => (int)cell.Cell.Y);
            return (left * EngineRoomObjects.Block, top * EngineRoomObjects.Block,
                (solid.Max(cell => (int)cell.Cell.X) - left + 1) * EngineRoomObjects.Block, (solid.Max(cell => (int)cell.Cell.Y) - top + 1) * EngineRoomObjects.Block);
        }

        (int Left, int Top, int Width, int Height) core = Core(DraygonCannonDraw.RightShield);
        if (Enum.GetValues<DraygonCannonDraw>().Any(draw => Core(draw) != core))
            throw new InvalidDataException("A cannon of Draygon's chamber is not solid in the same blocks whichever way it faces and whatever it shows.");

        // A missile's hit, whole, as many times as the old game counts breaks it; a super missile's is as many at once.
        if (SamusProjectileDamageDefinitions.StartingDamage[SamusProjectileKind.Missile] is not SamusProjectileStartingDamage.Own missile)
            throw new InvalidDataException("A missile's damage is not its own, which a cannon's health is counted in.");
        static JsonObject Reaction(JsonNode damage, params string[] kinds) => new() { ["kinds"] = new JsonArray([.. kinds.Select(kind => (JsonNode)kind)]), ["damage"] = damage };
        var reactions = new JsonArray(
            Reaction("none", "beam"), Reaction("none", "bomb"), Reaction("none", "power_bomb"), Reaction("none", "speed_boost"),
            Reaction("none", "shinespark"), Reaction("none", "screw_attack"), Reaction("none", "charged_spin"), Reaction("none", "grapple"),
            Reaction("full", "missile"), Reaction(DraygonCannonDefinitions.HitsToDestroy, "super_missile"));

        EnergyAmount drain = SpikeHazardDefinitions.GrappledTurretSpikeDamage;
        if (drain.FractionParts != 0)
            throw new InvalidDataException("A broken cannon drains a part of a unit of energy, which this does not write.");
        if (DraygonCannonDefinitions.DestroyedCannon.Behavior.Spike is not { } spike
            || !SpikeHazardDefinitions.DamagingSpikeBlocks.TryGetValue(spike, out SpikeBlockBehavior harm) || harm != SpikeBlockBehavior.Light)
            throw new InvalidDataException("A broken cannon is not a light spike, whose harm the Godot cannon is given.");

        EngineObjectFile cannon = new EngineObjectFile(_tree, file, "DraygonCannon")
            .Value("health", DraygonCannonDefinitions.HitsToDestroy * missile.Value)
            .Value("contact_damage", SpikeHazardDefinitions.LightSpikeDamage)
            .Value("grapple_drain", drain.WholeEnergy * 100)
            .Group("reactions", reactions)
            .Group("picture_offsets", new JsonObject { ["left"] = Corner(DraygonCannonDraw.LeftShield), ["right"] = Corner(DraygonCannonDraw.RightShield) })
            .Group("shots", new JsonObject { ["shot"] = shot })
            .Collider("Solid", core.Left, core.Top, core.Width, core.Height)
            .Collider("Hurtbox", core.Left, core.Top, core.Width, core.Height);
        foreach ((DraygonCannonOrientation way, string side) in new[] { (DraygonCannonOrientation.Left, "left"), (DraygonCannonOrientation.Right, "right") })
        {
            DraygonCannonLook look = DraygonCannonAnimationDefinitions.Looks[way];
            cannon
                .Timeline("shield_" + side, Timeline(look.Shield))
                .Timeline("hit_flash_" + side, Timeline(look.HitFlash))
                .Timeline("sparks_" + side, Timeline(look.Sparks));
        }

        cannon.Write();
    }
}
