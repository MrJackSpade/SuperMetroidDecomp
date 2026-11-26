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
using SuperMetroid.Core.Enemies.Drawing;
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

    /// <summary>The shades his eyes' and his body's showing end on, which leave him in his own colours.</summary>
    private static string[] RidleyShownShades =>
        [RidleyShades.Fade(RidleyFadeDefinitions.EyeFade.End).Key, RidleyShades.Fade(RidleyFadeDefinitions.BodyFade.End).Key];

    /// <summary>
    /// The colours one of Ridley's fights shows on his one set of pictures, as one change: his arrival's colours, his
    /// own hidden in black, and <paramref name="laid"/> over those, in the order the old game lays them over every
    /// picture of his.
    /// </summary>
    /// <exception cref="InvalidDataException">
    /// The change holds in one region of his pictures only, and his pictures have no map of regions; or, aboard Ceres,
    /// the Baby's shade, which the old game lays between his arrival's colours and his own hidden, changes his pictures.
    /// </exception>
    private JsonObject RidleyLook(EngineEnemy ridley, bool onCeres, params string[] laid)
    {
        const string family = GameInstallationLayout.EnemySheetDirectoryName;
        string sheet = _sheets.EnemySheet(EnemyDefinitionIdKeys.Installed.Of(onCeres ? EnemyDefinitionId.RidleyCeres : EnemyDefinitionId.Ridley));
        var places = new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal) { [ridley.Folder] = [sheet] };
        string arrival = (onCeres ? RidleyShades.CeresStart : RidleyShades.NorfairStart).Key;
        string over = string.Join('+', laid.Prepend((onCeres ? RidleyShades.CeresHidden : RidleyShades.NorfairHidden).Key));
        string shade = $"{arrival}+{over}";
        JsonObject change = ShadeChange(shade, family, places);
        foreach (KeyValuePair<string, JsonNode?> place in change)
        {
            foreach (KeyValuePair<string, JsonNode?> colour in place.Value!.AsObject())
            {
                if (colour.Key.Contains('/', StringComparison.Ordinal))
                    throw new InvalidDataException($"The shade {shade} changes the colour {colour.Key} of Ridley's pictures in one region of them only, and his pictures have no map of regions.");
            }
        }

        if (onCeres)
        {
            // The Godot Ridley does not follow the Baby's shade, which is sound only while it changes nothing of his.
            foreach (CeresBabyShade baby in Enum.GetValues<CeresBabyShade>())
            {
                if (ShadeChange($"{arrival}+{RidleyShades.Baby(baby).Key}+{over}", family, places).ToJsonString() != change.ToJsonString())
                    throw new InvalidDataException($"The Baby's shade {baby} changes the colours of Ceres Ridley's own pictures under {shade}, and the Godot Ridley does not follow the Baby's shade.");
            }
        }

        return change;
    }

    /// <summary>
    /// Ridley's showing himself as a fade of opacity: unseen, then his eyes alone a step at a time, then the rest of
    /// him. The owner has it so (#3273): the old game brings his colours up from black and moves him among its layers
    /// as it does; here he stands at one depth and is faded in, the same number of steps for his eyes and for the
    /// rest, each step as much more opaque as the last.
    /// </summary>
    /// <param name="unseen">The old game's change that hides him: it names every colour of his pictures.</param>
    /// <param name="firstOfBody">The old change for the first step of his body's showing: the colours it leaves alone are his eyes'.</param>
    /// <param name="shown">The old change for the last step: his own colours, where they are not his pictures'.</param>
    /// <param name="steps">How many steps each of the two fades takes.</param>
    /// <exception cref="InvalidDataException">The old changes are not of one place, or tell no eyes from the rest.</exception>
    private static JsonObject RidleyFadeByOpacity(JsonObject unseen, JsonObject firstOfBody, JsonObject shown, int steps)
    {
        if (unseen.Count != 1 || firstOfBody.Count != 1 || shown.Count > 1)
            throw new InvalidDataException("Ridley's colours change in more places than his one set of pictures.");
        string place = unseen.First().Key;
        JsonObject hidden = unseen[place]!.AsObject(), body = firstOfBody[place]!.AsObject();
        JsonObject? own = shown.Count == 1 ? shown[place]!.AsObject() : null;
        List<string> every = [.. hidden.Select(colour => colour.Key)];
        var eyes = every.Where(colour => !body.ContainsKey(colour)).ToHashSet(StringComparer.Ordinal);
        if (eyes.Count == 0 || eyes.Count == every.Count)
            throw new InvalidDataException("The first step of Ridley's body showing itself does not tell his eyes' colours from the rest.");

        // A colour as he shows it once he is wholly seen, and that colour so many parts of the steps opaque.
        string Own(string colour) => own is not null && own[colour] is { } changed ? changed.GetValue<string>() : colour;
        string Faded(string colour, int parts) => Own(colour) + ((int)Math.Round(255.0 * parts / steps)).ToString("x2", System.Globalization.CultureInfo.InvariantCulture);
        JsonObject Step(Func<string, string?> shownAs)
        {
            var change = new JsonObject();
            foreach (string colour in every)
            {
                if (shownAs(colour) is { } to)
                    change[colour] = to;
            }

            return new JsonObject { [place] = change };
        }

        var eyeSteps = new JsonArray();
        var bodySteps = new JsonArray();
        for (int step = 0; step < steps; step++)
        {
            int parts = step + 1;
            eyeSteps.Add(Step(colour => Faded(colour, eyes.Contains(colour) ? parts : 0)));

            // The body's last step is his own colours, which his fight gives.
            if (step < steps - 1)
                bodySteps.Add(Step(colour => eyes.Contains(colour) ? (Own(colour) == colour ? null : Own(colour)) : Faded(colour, parts)));
        }

        return new JsonObject { ["hidden"] = Step(colour => Faded(colour, 0)), ["eyes"] = eyeSteps, ["body"] = bodySteps };
    }

    /// <summary>Begins the data file of one of Ridley's fights with what both fights share.</summary>
    private EngineEnemy RidleyFight(string file, EnemyDefinitionId id, bool onCeres)
    {
        EngineEnemy ridley = Enemy(file, "Ridley", id);

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
                    // Each box is written as a shot meets it; Samus's touch meets it a pixel down and right of that.
                    boxes.Add(EngineEnemy.ShotRect(
                        (int)component.Offset.X + box.Left, (int)component.Offset.Y + box.Top,
                        (int)component.Offset.X + box.Right, (int)component.Offset.Y + box.Bottom));
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
            poses[ridley.Picture(frame)] = BoxesOf(frame);
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
            string left = ridley.Picture(shown.FacingLeft).Replace("_facing_left_", "_facing_{side}_", StringComparison.Ordinal);
            string right = ridley.Picture(shown.FacingRight).Replace("_facing_right_", "_facing_{side}_", StringComparison.Ordinal);
            if (left != right || !left.Contains("{side}", StringComparison.Ordinal))
                throw new InvalidDataException($"Ridley's wing pictures {left} and {right} are not one picture named for each side.");
            wings.Add(left);
        }

        IEnumerable<EnemyFrameId> links = RidleyTailDefinitions.ShaftLinks.Select(link => link.Frame).Prepend(RidleyTailDefinitions.BaseLink.Frame);
        TurnArcs<EnemyFrameId> tips = RidleySupplementalVisualDefinitions.TailTipFrames;

        // Its colours as it shows itself, each a change of the colours of its one set of pictures: the dark it waits
        // in, its eyes and then the rest of it coming out of that dark. The last step of its body's showing is its own
        // colours, which aboard Ceres it keeps and in Norfair the band of its health gives.
        IReadOnlyList<RidleyFadeImage> eyes = RidleyFadeDefinitions.EyeFade.Images, body = RidleyFadeDefinitions.BodyFade.Images;
        if (eyes.Count != body.Count)
            throw new InvalidDataException("Ridley's eyes and body come out of the dark in different numbers of steps, where the Godot Ridley has one number for both.");
        string eyesShown = RidleyShades.Fade(RidleyFadeDefinitions.EyeFade.End).Key;
        JsonObject colours = RidleyFadeByOpacity(
            RidleyLook(ridley, onCeres),
            RidleyLook(ridley, onCeres, eyesShown, RidleyShades.Fade(body[0]).Key),
            RidleyLook(ridley, onCeres, eyesShown, RidleyShades.Fade(body[^1]).Key),
            eyes.Count);
        if (onCeres)
            colours["own"] = RidleyLook(ridley, onCeres, RidleyShownShades);

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
            .Group("tail_pictures", new JsonArray([.. links.Select(link => (JsonNode)ridley.Picture(link))]))
            .Group("tail_tip_pictures", new JsonArray([.. tips.InTurnOrder.Select(tip => (JsonNode)ridley.Picture(tip))]))
            .Group("colours", colours);
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
        // The old game draws his fireballs and every flame of them in the projectiles' shared colours, not their sheet's own.
        string Timeline(string name, RidleyFireballSegment segment) =>
            ridley.SpawnedTimeline(name, RidleyFireballAnimations.Segments, RidleyFireballAnimations.StartingAt[segment], Cue,
                SuperMetroid.Core.Enemies.Drawing.EnemyShades.ProjectileLook.Key);

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

    /// <summary>Gives Ridley's fight in Norfair what only it has: its health's bands and colours, its death and its pieces.</summary>
    private void WriteNorfairRidley(EngineEnemy ridley)
    {
        const string family = GameInstallationLayout.EnemySheetDirectoryName;

        JsonObject Band(NorfairRidleyHealthBand band, int? below)
        {
            var written = new JsonObject();
            if (below is { } health)
                written["below"] = health;
            written["hover_divisor"] = band.HoverDivisor;
            written["grab_divisor"] = band.GrabDivisor;
            // Its own colours, with the band's look of its health laid over them where the band has one.
            written["colours"] = band.HealthShade is { } shade
                ? RidleyLook(ridley, onCeres: false, [.. RidleyShownShades, RidleyShades.Health(shade).Key])
                : RidleyLook(ridley, onCeres: false, RidleyShownShades);
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
        // Its arena's colours: hidden in black as it arrives, and each step of its coming out of the dark laid over that.
        SortedDictionary<string, string> arena = RoomPlacesOf(EnemyDefinitionId.Ridley);
        string hiddenArena = RidleyShades.NorfairHidden.Key;
        ridley.Group("arena_colours", new JsonObject
        {
            ["hidden"] = ShadeChange(hiddenArena, arena),
            ["reveal"] = EngineNames.List(RidleyFadeDefinitions.ArenaReveal.Images.Select(image => (JsonNode)ShadeChange($"{hiddenArena}+{RidleyShades.Fade(image).Key}", arena))),
        });
        // The room's acid: up for the fight as his reveal completes, and drained as his death roar ends. He gives it
        // where to go, how fast and after how long; the room's own effect gives it no speed.
        static JsonObject LiquidMotion(int height, SignedEightEight velocity, int delay) => new()
        {
            ["height"] = height, ["speed"] = Math.Abs(velocity.Pixels), ["delay"] = delay,
        };
        ridley.Group("liquid", new JsonObject
        {
            ["rise"] = LiquidMotion(RidleyLiquidDefinitions.BattleHeight, RidleyLiquidDefinitions.RiseVelocity, RidleyLiquidDefinitions.RiseDelay),
            ["drain"] = LiquidMotion(RidleyLiquidDefinitions.DrainedHeight, RidleyLiquidDefinitions.DrainVelocity, RidleyLiquidDefinitions.DrainDelay),
        });
        ridley
            .Value("arena_reveal_updates", NorfairRidleyRevealDefinitions.RevealStepCount * NorfairRidleyRevealDefinitions.RevealStepUpdates + NorfairRidleyRevealDefinitions.BeforeFirstStepUpdates)
            .Value("arena_reveal_step_updates", NorfairRidleyRevealDefinitions.RevealStepUpdates)
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
        // The capsule. The old game draws it in the hurt flash's look on every update, never in the shades its colour
        // cues lay over Ridley's own, so it shows one set of pictures in one set of colours.
        string spawner = EnemyDefinitionIdKeys.Installed.Of(EnemyDefinitionId.RidleyCeres);
        string[] frames = [.. Enum.GetValues<CeresBabyPose>().Select(pose => EnemySheetDefinitions.EncounterFrameName(CeresBabyFrameDefinitions.ShownFrames.Of(pose)))];
        EngineSheetPictures capsule = _sheets.CutSpawnedShade(_tree, $"{ridley.Folder}_baby", spawner, frames, EnemyShades.HurtFlash.Key);

        JsonObject Baby(CeresBabySegment segment)
        {
            static string? Cue(CeresBabyCue cue) => cue switch
            {
                CeresBabyCue.ReactsToMotion => "reacts",
                CeresBabyCue.Cries => "cries",
                // Its colour cues change nothing the capsule is drawn in.
                CeresBabyCue.UpdateColors => null,
                _ => throw new InvalidDataException($"The capsule's animation raises {cue}, which this does not know."),
            };

            JsonObject timeline = EngineTimelines.Of(CeresBabyAnimations.Segments, CeresBabyAnimations.StartingWith(segment),
                pose => capsule.Of(EnemySheetDefinitions.EncounterFrameName(CeresBabyFrameDefinitions.ShownFrames.Of(pose))), Cue);
            // Its visual is one of Ridley's, whose pictures lie from another corner.
            timeline["offset"] = new JsonObject { ["x"] = capsule.OffsetX - ridley.Pictures.OffsetX, ["y"] = capsule.OffsetY - ridley.Pictures.OffsetY };
            return timeline;
        }

        // The colours its retreat gives the chamber, on the pictures of each state of a room that places it.
        ridley.Group("retreat_colours", ShadeChange($"{RidleyShades.RetreatRoom.Key}+{RidleyShades.RetreatShared.Key}", RoomPlacesOf(EnemyDefinitionId.RidleyCeres)));

        // The bands of hits taken that lay a look of its health over its own colours. A band with no look leaves the
        // colours as they are, which, as hits only add up, is the last band's look reached below it.
        var bands = new JsonArray();
        foreach (CeresRidleyHealthBand band in CeresRidleyHealthBandDefinitions.HighestFirst)
        {
            if (band.HealthShade is { } shade)
                bands.Add(new JsonObject { ["hits"] = band.MinimumHits, ["colours"] = RidleyLook(ridley, onCeres: true, [.. RidleyShownShades, RidleyShades.Health(shade).Key]) });
        }

        ridley.Group("health_bands", bands);

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
