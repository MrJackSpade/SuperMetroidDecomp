using System.Globalization;
using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Bosses.Ridley.Artwork;
using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>Ridley, in Norfair and aboard Ceres station, and its fire.</summary>
public sealed partial class EngineAssetWriter
{
    /// <summary>
    /// The scene of a piece of one fight's fire: "fireball", "flare" or "flame". Each fight has its own, whose data
    /// names that fight's pictures.
    /// </summary>
    private static string RidleyFireScene(EngineEnemy ridley, string what) =>
        $"res://game/bosses/ridley/{ridley.Folder[(ridley.Folder.LastIndexOf('/') + 1)..]}_{what}.tscn";

    /// <summary>The data file of the scene of a piece of one fight's fire.</summary>
    private static string RidleyFireData(EngineEnemy ridley, string what) => $"{ridley.Folder}_{what}.json";

    /// <summary>What stands in the names of Ridley's pictures for the colours it is in.</summary>
    private const string RidleyLookPlaceholder = "{look}";

    /// <summary>Writes Ridley's two fights, each with its fire as the rooms of that fight draw it.</summary>
    private void WriteRidley()
    {
        EngineEnemy norfair = RidleyFight("bosses/ridley.json", EnemyDefinitionId.Ridley, onCeres: false);
        RidleyFireballShot(norfair, WriteRidleyFire(norfair), AreaId.Norfair);
        WriteNorfairRidley(norfair);
        norfair.Write();

        EngineEnemy ceres = RidleyFight("bosses/ceres_ridley.json", EnemyDefinitionId.RidleyCeres, onCeres: true);
        RidleyFireballShot(ceres, WriteRidleyFire(ceres), AreaId.Ceres);
        WriteCeresRidley(ceres);
        ceres.Write();
    }

    /// <summary>A picture of Ridley's as its timelines name it: one of the sets of its pictures, by the colours it is in.</summary>
    private static string RidleySets(EngineEnemy ridley, string picture) => picture.StartsWith(ridley.Folder + "/", StringComparison.Ordinal)
        ? ridley.Folder + RidleyLookPlaceholder + picture[ridley.Folder.Length..]
        : throw new InvalidDataException($"Ridley's picture {picture} is not one of its own.");

    /// <summary>Begins the data file of one of Ridley's fights with what both fights share.</summary>
    private EngineEnemy RidleyFight(string file, EnemyDefinitionId id, bool onCeres)
    {
        EngineEnemy ridley = Enemy(file, "Ridley", id);
        const string family = GameInstallationLayout.EnemySheetDirectoryName;
        string sheet = _sheets.EnemySheet(EnemyDefinitionIdKeys.Installed.Of(id));

        static string? Cue(RidleyBodyCue cue) => cue switch
        {
            RidleyBodyCue.Move move => string.Create(CultureInfo.InvariantCulture, $"move:{(int)move.Displacement.X}:{(int)move.Displacement.Y}"),
            RidleyBodyCue.SetClawHeight claws => claws.Height switch
            {
                RidleyClawHeight.Standing => "claws:standing",
                RidleyClawHeight.LegsHalfExtended => "claws:half",
                RidleyClawHeight.LegsExtended => "claws:extended",
                _ => throw new InvalidDataException($"Ridley's claws hang at {claws.Height}, which this does not know."),
            },
            RidleyBodyCue.Flip flip => "flip:" + EngineNames.Side(flip.Facing),
            RidleyBodyCue.Signal signal => signal.Kind switch
            {
                RidleyBodyCueKind.MayCutFireballingShort => "cut_short",
                RidleyBodyCueKind.Roar => "roar",
                RidleyBodyCueKind.StopRoaring => "stop_roaring",
                RidleyBodyCueKind.FaceForward => "face_forward",
                RidleyBodyCueKind.AimFireball => "aim",
                RidleyBodyCueKind.SpawnFireballWithAfterburn => "fireball:afterburn",
                RidleyBodyCueKind.SpawnFireballWithoutAfterburn => "fireball:plain",
                RidleyBodyCueKind.LiftOff => "lift_off",
                _ => throw new InvalidDataException($"Ridley's animation raises {signal.Kind}, which this does not know."),
            },
            _ => throw new InvalidDataException($"Ridley's animation raises {cue}, which this does not know."),
        };

        void Body(string name, RidleyBodySegment segment) =>
            ridley.Timeline(name, RidleyAnimations.Segments, RidleyAnimations.StartingWith(segment), Cue);
        void Faced(string name, RidleyFacedAnimation animation)
        {
            Body(name + "_left", animation.FacingLeft);
            Body(name + "_right", animation.FacingRight);
        }

        Faced("standing", RidleyAnimations.Standing);
        Faced("opening_roar", RidleyAnimations.OpeningRoar);
        Faced("fireballing", RidleyAnimations.Fireballing);
        Faced("transition_to_flying", RidleyAnimations.TransitionToFlying);
        if (onCeres)
        {
            Faced("ceres_lunge", RidleyAnimations.CeresLunge);
            Faced("retrieve_baby", RidleyAnimations.RetrieveBabyMetroid);
            Faced("fireballing_end", RidleyAnimations.FireballingEnd);
        }
        else
        {
            // Only in Norfair does it turn about and die: aboard Ceres it has no picture facing out of the screen.
            Body("turn_left_to_right", RidleyBodySegment.TurnFromLeftToRight);
            Body("turn_right_to_left", RidleyBodySegment.TurnFromRightToLeft);
            Faced("death_roar", RidleyAnimations.DeathRoar);
        }

        // The parts of it that can be touched and shot, by the picture its body shows.
        JsonArray BoxesOf(EnemyFrameId frame)
        {
            var boxes = new JsonArray();
            foreach (RidleyCollisionComponent component in RidleyCollision.ComponentsAt(frame))
            {
                foreach (EnemyHitboxRectangle box in component.Hitboxes)
                {
                    boxes.Add(new JsonObject
                    {
                        ["left"] = (int)component.Offset.X + box.Left,
                        ["top"] = (int)component.Offset.Y + box.Top,
                        ["width"] = box.Right - box.Left,
                        ["height"] = box.Bottom - box.Top,
                    });
                }
            }

            return boxes.Count is > 0 and <= RidleyBodyParts
                ? boxes
                : throw new InvalidDataException($"Ridley's frame {frame} has {boxes.Count} parts to touch and shoot, where the Godot Ridley has {RidleyBodyParts} colliders for them.");
        }

        var poses = new JsonObject();
        foreach (EnemyFrameId frame in RidleyCollisionDefinitions.BodyFrames)
        {
            if (onCeres && frame == EnemyFrameId.RidleyBodyForward)
                continue;
            poses[RidleySets(ridley, ridley.Picture(frame))] = BoxesOf(frame);
        }

        // Until its first pose places them, its parts lie where its standing pose has them.
        JsonArray standing = BoxesOf(onCeres ? EnemyFrameId.RidleyBodyLeft : EnemyFrameId.RidleyBodyRight);
        for (int part = 0; part < RidleyBodyParts; part++)
        {
            JsonNode box = standing[Math.Min(part, standing.Count - 1)]!;
            foreach (string name in new[] { $"Part{part + 1}", $"Part{part + 1}Touch" })
                ridley.Collider(name, box["left"]!.GetValue<int>(), box["top"]!.GetValue<int>(), box["width"]!.GetValue<int>(), box["height"]!.GetValue<int>());
        }

        ridley.BodyColliders("Body")
            .Collider("TailTip", -RidleyTailDefinitions.TipArmourReach, -RidleyTailDefinitions.TipArmourReach, RidleyTailDefinitions.TipArmourReach * 2, RidleyTailDefinitions.TipArmourReach * 2)
            .Collider("TailJoint", -RidleyTailDefinitions.InnerJointArmourReach, -RidleyTailDefinitions.InnerJointArmourReach,
                RidleyTailDefinitions.InnerJointArmourReach * 2, RidleyTailDefinitions.InnerJointArmourReach * 2);

        // Its wings beat through their poses in turn from the pose its fight begins in; one path names both sides.
        IReadOnlyList<RidleyWingPose> beat = RidleySupplementalVisualDefinitions.WingBeat;
        RidleyWingPose firstPose = onCeres ? RidleyWingDefinitions.CeresFirstPose : RidleyWingDefinitions.NorfairFirstPose;
        int first = beat.ToList().IndexOf(firstPose);
        if (first < 0)
            throw new InvalidDataException("The pose Ridley's wings begin in is not one of their beat.");
        var wings = new JsonArray();
        for (int pose = 0; pose < beat.Count; pose++)
        {
            RidleyWingPose shown = beat[(first + pose) % beat.Count];
            string left = RidleySets(ridley, ridley.Picture(shown.FacingLeft)).Replace("_facing_left_", "_facing_{side}_", StringComparison.Ordinal);
            string right = RidleySets(ridley, ridley.Picture(shown.FacingRight)).Replace("_facing_right_", "_facing_{side}_", StringComparison.Ordinal);
            if (left != right || !left.Contains("{side}", StringComparison.Ordinal))
                throw new InvalidDataException($"Ridley's wing pictures {left} and {right} are not one picture named for each side.");
            wings.Add(left);
        }

        IEnumerable<EnemyFrameId> links = RidleyTailDefinitions.ShaftLinks.Select(link => link.Frame).Prepend(RidleyTailDefinitions.BaseLink.Frame);
        TurnArcs<EnemyFrameId> tips = RidleySupplementalVisualDefinitions.TailTipFrames;

        // Its pictures as each of its colours shows them: the dark it waits in, its eyes and then the rest of it
        // coming out of that dark.
        string hidden = (onCeres ? RidleyShades.CeresHidden : RidleyShades.NorfairHidden).Key;
        IReadOnlyList<RidleyFadeImage> eyes = RidleyFadeDefinitions.EyeFade.Images, body = RidleyFadeDefinitions.BodyFade.Images;
        if (eyes.Count != body.Count)
            throw new InvalidDataException("Ridley's eyes and body come out of the dark in different numbers of steps, where the Godot Ridley has one number for both.");
        _sheets.CutShades(_tree, family, ridley.Folder + "_hidden", sheet, hidden);
        for (int step = 0; step < eyes.Count; step++)
        {
            string number = step.ToString(CultureInfo.InvariantCulture);
            _sheets.CutShades(_tree, family, ridley.Folder + "_eyes_" + number, sheet, hidden, RidleyShades.Fade(eyes[step]).Key);
            // The last step of its body's showing is its own colours.
            if (step < body.Count - 1)
                _sheets.CutShades(_tree, family, ridley.Folder + "_body_" + number, sheet, hidden, RidleyShades.Fade(eyes[^1]).Key, RidleyShades.Fade(body[step]).Key);
        }

        (Offset16<RoomPixels> least, Offset16<RoomPixels> greatest) = onCeres
            ? (CeresRidleyArenaDefinitions.MovementMinimum, CeresRidleyArenaDefinitions.MovementMaximum)
            : (NorfairRidleyArenaDefinitions.MovementMinimum, NorfairRidleyArenaDefinitions.MovementMaximum);
        ridley
            .Value("reveal_delay", onCeres ? CeresRidleyFightDefinitions.RevealDelayUpdates : NorfairRidleyRevealDefinitions.RevealDelayUpdates)
            .Value("fade_steps", eyes.Count)
            .Value("eye_fade_step_updates", RidleyEyeFadeDefinitions.ShadeUpdates)
            .Value("eye_hold_updates", RidleyEyeFadeDefinitions.HoldUpdates)
            .Value("body_fade_step_updates", RidleyShadeDefinitions.BodyFadeShadeUpdates)
            .Value("before_roar_updates", RidleyShadeDefinitions.BeforeRoarUpdates)
            .Value("tail_damage", onCeres ? RidleyTailDefinitions.CeresTipDamage : RidleyTailDefinitions.NorfairTipDamage)
            .Value("roar_sound", EngineNames.Sound(RidleySoundDefinitions.Roar))
            // The old game counts the angle as a part of a turn; the first picture's arc begins a little before straight down.
            .Value("tail_tip_arc_start", (int)(sbyte)tips.Start.RawValue)
            .Group("pose_boxes", poses)
            .Group("flight_bounds", new JsonObject { ["left"] = (int)least.X, ["top"] = (int)least.Y, ["right"] = (int)greatest.X, ["bottom"] = (int)greatest.Y })
            .Group("wing_pictures", wings)
            .Group("tail_pictures", new JsonArray([.. links.Select(link => (JsonNode)RidleySets(ridley, ridley.Picture(link)))]))
            .Group("tail_tip_pictures", new JsonArray([.. tips.InTurnOrder.Select(tip => (JsonNode)RidleySets(ridley, ridley.Picture(tip)))]))
            .PictureSets(RidleyLookPlaceholder);
        return ridley;
    }

    /// <summary>How many colliders the Godot Ridley has for the parts of its body, each a pair: one shots meet and one that hurts.</summary>
    private const int RidleyBodyParts = 5;

    /// <summary>Gives one of Ridley's fights its fireball, which does the harm of the fight's area.</summary>
    private static void RidleyFireballShot(EngineEnemy ridley, string timeline, AreaId area) =>
        ridley.Shot("fireball", RoomEnemyProjectileKind.RidleyFireball, (_, shot, projectile) =>
        {
            shot["damage"] = RidleyProjectileDamage.ForArea(area);
            shot["scene"] = RidleyFireScene(ridley, "fireball");
            shot["speed"] = EngineNames.Number(RidleyFireballDefinitions.FireballSpeed);
            projectile["timeline"] = timeline;
            projectile["picture_offset"] = EngineEnemy.NoOffset();
            // It moves itself, and meets terrain its own way.
            projectile["meeting_terrain"] = "passes_through";
        });

    /// <summary>
    /// Writes one fight's fire, its pictures drawn as that fight's rooms draw them: the fireball's timeline, which is
    /// returned, and the flare a fireball leaves where it strikes and the flames a flare sends along the surface, each
    /// a shot of its own with its data.
    /// </summary>
    private string WriteRidleyFire(EngineEnemy ridley)
    {
        static string? Cue(RidleyFireballCue cue) => cue switch
        {
            RidleyFireballCue.StopMoving => "stop",
            RidleyFireballCue.BeginFlight => "fly",
            RidleyFireballCue.SpawnHorizontalAfterburns or RidleyFireballCue.SpawnVerticalAfterburns => "flames",
            RidleyFireballCue.SpawnNextAfterburn => "next",
            RidleyFireballCue.Delete => "end",
            _ => throw new InvalidDataException($"Ridley's fire raises {cue}, which this does not know."),
        };
        string Timeline(string name, RidleyFireballSegment segment) =>
            ridley.SpawnedTimeline(name, RidleyFireballAnimations.Segments, RidleyFireballAnimations.StartingAt[segment], Cue);

        // A shot of its fire, as a file of its own: a flare's is launched by the fireball, and a flame's by the flare.
        string Shot(string name, RoomEnemyProjectileKind kind, string scene, string timeline)
        {
            EnemyProjectileDefinition old = EnemyProjectileDefinitionCatalog.Definitions[kind];
            string file = $"{ridley.Folder}/{name}_shot.json";
            _tree.WriteData(file, new JsonObject
            {
                ["damage"] = old.Damage,
                ["scene"] = scene,
                ["projectile"] = new JsonObject
                {
                    // Something of no size still lies somewhere: one pixel.
                    ["width"] = Math.Max(1, old.Radius.X * 2),
                    ["height"] = Math.Max(1, old.Radius.Y * 2),
                    // It moves itself, and meets terrain its own way.
                    ["meeting_terrain"] = "passes_through",
                    ["meeting_target"] = old.Properties.HasFlag(EnemyProjectilePropertyFlags.PersistsOnContact) ? "passes_through_and_rehits" : "ends",
                    ["met_by_shot"] = old.Properties.HasFlag(EnemyProjectilePropertyFlags.BlocksShots) ? "destroyed" : "ignored",
                    ["view_margin"] = -1,
                    ["timeline"] = timeline,
                    ["picture_offset"] = EngineEnemy.NoOffset(),
                },
            });
            return file;
        }

        EnemyProjectileDefinition across = EnemyProjectileDefinitionCatalog.Definitions[RoomEnemyProjectileKind.RidleyHorizontalAfterburn];
        if (across != EnemyProjectileDefinitionCatalog.Definitions[RoomEnemyProjectileKind.RidleyVerticalAfterburn])
            throw new InvalidDataException("Ridley's flames that run across and those that run up and down differ, where the Godot game has one flame for both.");
        PixelVelocity<RoomPixels> right = RidleyAfterburnDefinitions.Right, down = RidleyAfterburnDefinitions.Down;
        if (right.X != down.Y || right.X != -RidleyAfterburnDefinitions.Left.X || down.Y != -RidleyAfterburnDefinitions.Up.Y)
            throw new InvalidDataException("Ridley's flames race at different speeds each way, where the Godot game has one speed for all.");

        string flame = Shot("flame", RoomEnemyProjectileKind.RidleyHorizontalAfterburn, RidleyFireScene(ridley, "flame"), Timeline("flame", RidleyFireballSegment.DirectionalAfterburn));
        string flareAcross = Shot("flare_across", RoomEnemyProjectileKind.RidleyAfterburnCenter, RidleyFireScene(ridley, "flare"), Timeline("flare_across", RidleyFireballSegment.HorizontalCenter));
        string flareDown = Shot("flare_down", RoomEnemyProjectileKind.RidleyAfterburnCenter, RidleyFireScene(ridley, "flare"), Timeline("flare_down", RidleyFireballSegment.VerticalCenter));
        string final = Timeline("flame_final", RidleyFireballSegment.AfterburnFinal);
        foreach (string what in new[] { "fireball", "flare", "flame" })
        {
            ridley.ProjectileData(RidleyFireData(ridley, what), "RidleyFireball",
                ("is", what),
                ("flare_across", flareAcross),
                ("flare_down", flareDown),
                ("flame", flame),
                ("final_timeline", final),
                ("flame_speed", (int)right.X.Pixels));
        }

        return Timeline("fireball", RidleyFireballSegment.Fireball);
    }

    /// <summary>Gives Ridley's fight in Norfair what only it has: its health's bands and looks, its death and its pieces.</summary>
    private void WriteNorfairRidley(EngineEnemy ridley)
    {
        const string family = GameInstallationLayout.EnemySheetDirectoryName;
        string sheet = _sheets.EnemySheet(EnemyDefinitionIdKeys.Installed.Of(EnemyDefinitionId.Ridley));
        static string LookOf(RidleyHealthShade? shade) => shade is null ? "" : "_" + shade.Key.Replace('-', '_');
        foreach (RidleyHealthShade shade in RidleyHealthShadeDefinitions.LeastHurtFirst)
            _sheets.CutShades(_tree, family, ridley.Folder + LookOf(shade), sheet, RidleyShades.Health(shade).Key);

        JsonObject Band(NorfairRidleyHealthBand band, int? below)
        {
            var written = new JsonObject();
            if (below is { } health)
                written["below"] = health;
            written["hover_divisor"] = band.HoverDivisor;
            written["grab_divisor"] = band.GrabDivisor;
            written["look"] = LookOf(band.HealthShade);
            written["pogo"] = EngineNames.List(EngineNames.Each(band.PogoLaunch).Select(pogo => (JsonNode)new JsonObject
            {
                ["across"] = EngineNames.Number(pogo.Velocity.X),
                ["up"] = EngineNames.Number(-pogo.Velocity.Y),
                ["rise_gain"] = EngineNames.Number(pogo.UpwardAcceleration),
                ["fall_gain"] = EngineNames.Number(pogo.DownwardAcceleration),
            }));
            return written;
        }

        var bands = new JsonArray();
        foreach (NorfairRidleyWoundBand wound in NorfairRidleyHealthBandDefinitions.WoundedLowestFirst)
            bands.Add(Band(wound.Band, wound.HealthBelow));
        bands.Add(Band(NorfairRidleyHealthBandDefinitions.Unhurt, null));

        // The pieces it breaks into. Each is one picture, in the colours of a Ridley whose health is gone.
        string breakupFolder = ridley.Folder + "/breakup";
        EngineSheetPictures pieces = _sheets.CutShades(_tree, family, breakupFolder,
            _sheets.EnemySheet(EnemyDefinitionIdKeys.Installed.Of(EnemyDefinitionId.RidleyBreakupPiece)), RidleyShades.Health(RidleyHealthShadeDefinitions.LeastHurtFirst[^1]).Key);
        string Piece(string name, EnemyFrameId frame)
        {
            string file = $"{breakupFolder}/{name}.json";
            _tree.WriteData(file, new JsonObject
            {
                ["steps"] = new JsonArray(new JsonObject { ["picture"] = pieces.Of(EnemySheetDefinitions.FrameName(frame)), ["updates"] = RidleyBreakupFrameDefinitions.PictureShowingUpdates }),
                ["loops_to"] = 0,
                ["offset"] = new JsonObject { ["x"] = pieces.OffsetX, ["y"] = pieces.OffsetY },
            });
            return file;
        }

        // The links of the Godot Ridley's tail, from its base to its tip.
        RidleyExplosionPart[] tail =
        [
            RidleyExplosionPart.TailBase, RidleyExplosionPart.TailFirstShaftLink, RidleyExplosionPart.TailSecondShaftLink, RidleyExplosionPart.TailThirdShaftLink,
            RidleyExplosionPart.TailFourthShaftLink, RidleyExplosionPart.TailInnerJoint, RidleyExplosionPart.TailTip,
        ];
        IReadOnlyList<EnemyFrameId> tips = RidleySupplementalVisualDefinitions.TailTipFrames.InTurnOrder;
        for (int tip = 0; tip < tips.Count; tip++)
            Piece("tail_tip_" + tip.ToString(CultureInfo.InvariantCulture), tips[tip]);
        var breakup = new JsonArray();
        foreach (RidleyExplosionPart part in RidleyExplosionDefinitions.BreakupOrder)
        {
            string name = EngineSheets.FileName(part.ToString());
            var piece = new JsonObject();
            int link = Array.IndexOf(tail, part);
            if (part == RidleyExplosionPart.TailTip)
            {
                piece["effect"] = $"{breakupFolder}/tail_tip_{{tip}}.json";
                piece["tail"] = link;
            }
            else if (link >= 0)
            {
                piece["effect"] = Piece(name, RidleyExplosionDefinitions.TailFrames[part]);
                piece["tail"] = link;
            }
            else
            {
                RidleyExplosionBodyPartDefinition placed = RidleyExplosionDefinitions.BodyParts[part];
                Piece(name + "_left", placed.FacingLeftFrame);
                Piece(name + "_right", placed.FacingRightFrame);
                piece["effect"] = $"{breakupFolder}/{name}_{{side}}.json";
                piece["x"] = (int)placed.FacingLeftOffset.X;
                piece["y"] = (int)placed.FacingLeftOffset.Y;
            }

            piece["updates"] = RidleyExplosionDefinitions.LifetimeUpdates[part];
            breakup.Add(piece);
        }

        static JsonObject Field(RandomSampleField field) => new() { ["from"] = field.Shift, ["digits"] = field.Width };
        ridley
            .Value("arena_reveal_updates", NorfairRidleyRevealDefinitions.RevealStepCount * NorfairRidleyRevealDefinitions.RevealStepUpdates + NorfairRidleyRevealDefinitions.BeforeFirstStepUpdates)
            .Value("wounded_health", NorfairRidleyMotionDefinitions.WoundedAttackHealth)
            .Value("death_lunges", RidleyLungeDefinitions.DeathLungeLimit)
            .Value("dust_effect", DustFile(DustExplosionAnimation.SmallDustCloud))
            .Value("explosion_effect", DustFile(DustExplosionAnimation.SmallExplosion))
            .Value("piece_end_effect", DeathFile(EnemyDeathExplosion.Small))
            .Value("bounce_sound", EngineNames.Sound(EnemySound.BigExplosion))
            // The blasts about him as he dies join only a queue holding at most two.
            .Value("explosion_sound", EngineNames.Sound(RidleySoundDefinitions.DeathExplosion, SoundRequestPriority.Low))
            .Value("death_drops", NorfairRidleyArenaDefinitions.DeathDrops)
            .Group("health_bands", bands)
            .Group("death_explosions", EngineNames.List(DeathExplosionScatterDefinitions.Offsets.Select(at => (JsonNode)new JsonObject { ["x"] = (int)at.X, ["y"] = (int)at.Y })))
            .Group("breakup", breakup)
            .Group("scattered_drops", Drops(ridley.Definition.Drops) ?? throw new InvalidDataException("Ridley scatters nothing when it is gone."))
            .Group("drop_scatter", new JsonObject
            {
                ["x"] = (int)NorfairRidleyArenaDefinitions.DeathDropCorner.X,
                ["y"] = (int)NorfairRidleyArenaDefinitions.DeathDropCorner.Y,
                ["across"] = Field(EnemyDropScatterDefinitions.NarrowArenaHorizontalScatter),
                ["down"] = Field(EnemyDropScatterDefinitions.ArenaBandVerticalScatter),
            });
        if (Drops(RoomEnemyDefinitionCatalog.ById[EnemyDefinitionId.RidleyBreakupPiece].Drops) is { } pieceDrops)
            ridley.Group("piece_drops", pieceDrops);
        // It leaves nothing of its own as it goes: what it scatters is all.
        ridley.Data.Remove("drops");
        ridley.Data.Remove("death");
    }

    /// <summary>Gives Ridley's fight aboard Ceres what only it has: how it is driven off, and the capsule in its claws.</summary>
    private void WriteCeresRidley(EngineEnemy ridley)
    {
        // The capsule. Each of its poses is drawn in the colours the old game gives that pose as it cries, and in its
        // resting colours as it only sways. Its pictures are kept apart from Ridley's own, which come in sets.
        string spawner = EnemyDefinitionIdKeys.Installed.Of(EnemyDefinitionId.RidleyCeres);
        string[] frames = [.. Enum.GetValues<CeresBabyPose>().Select(pose => EnemySheetDefinitions.EncounterFrameName(CeresBabyFrameDefinitions.ShownFrames.Of(pose)))];
        var shaded = new Dictionary<CeresBabyShade, EngineSheetPictures>();
        foreach (CeresBabyShade shade in Enum.GetValues<CeresBabyShade>())
            shaded[shade] = _sheets.CutSpawnedShade(_tree, $"{ridley.Folder}_baby/{EngineSheets.FileName(shade.ToString())}", spawner, frames, RidleyShades.Baby(shade).Key);
        EngineSheetPictures resting = shaded[CeresBabyShade.Resting];
        if (shaded.Values.Any(cut => cut.OffsetX != resting.OffsetX || cut.OffsetY != resting.OffsetY))
            throw new InvalidDataException("The capsule's pictures lie in different places in its different colours.");

        JsonObject Baby(CeresBabySegment segment)
        {
            CeresBabyShade shade = CeresBabyShade.Resting;
            string? Cue(CeresBabyCue cue)
            {
                switch (cue)
                {
                    case CeresBabyCue.ReactsToMotion:
                        return "reacts";
                    case CeresBabyCue.Cries:
                        return "cries";
                    case CeresBabyCue.UpdateColors colours:
                        // Its colours are in its pictures: every frame after this is drawn in them.
                        shade = colours.Shade;
                        return null;
                    default:
                        throw new InvalidDataException($"The capsule's animation raises {cue}, which this does not know.");
                }
            }

            JsonObject timeline = EngineTimelines.Of(CeresBabyAnimations.Segments, CeresBabyAnimations.StartingWith(segment),
                pose => shaded[shade].Of(EnemySheetDefinitions.EncounterFrameName(CeresBabyFrameDefinitions.ShownFrames.Of(pose))), Cue);
            // Its visual is one of Ridley's, whose pictures lie from another corner.
            timeline["offset"] = new JsonObject { ["x"] = resting.OffsetX - ridley.Pictures.OffsetX, ["y"] = resting.OffsetY - ridley.Pictures.OffsetY };
            return timeline;
        }

        ridley
            .Timeline("baby_idle", Baby(CeresRidleyBabyScene.SilentAnimation))
            .Timeline("baby_expressive", Baby(CeresRidleyBabyScene.MovingAnimation))
            .Value("hits_to_drive_off", CeresRidleyFightDefinitions.HitsToDriveOff)
            .Value("samus_low_energy", CeresRidleyFightDefinitions.SamusLowEnergy)
            .Value("roar_hold_updates", CeresRidleyFightDefinitions.RoarHoldUpdates)
            .Value("hit_flash_updates", CeresRidleyFightDefinitions.HurtFlash)
            .Value("baby_cry_sound", EngineNames.Sound(RoomSound.CeresBabyMetroidCry));
    }
}
