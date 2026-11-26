using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.EyeDoor;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>The eye that guards an eye door, the growth round its door, what it spits and what it sweats.</summary>
public sealed partial class EngineAssetWriter
{
    /// <summary>The scene of the eye's spit, which is a projectile of a class of its own.</summary>
    private const string EyeDoorSpitScene = "res://game/enemies/eye_door/eye_door_spit.tscn";

    /// <summary>The scene of a drop of the eye's sweat.</summary>
    private const string EyeDoorSweatScene = "res://game/enemies/eye_door/eye_door_sweat.tscn";

    /// <summary>
    /// Writes the eye of an eye door. The old game made it of the room's own blocks, so its pictures are written for
    /// every set of graphics; it faces left or right, and one path with the side's name left open names both.
    /// </summary>
    private void WriteEyeDoorEye()
    {
        const string directory = GameInstallationLayout.RoomObjectEyeDoorVisualDirectoryName;
        const string file = "enemies/eye_door_eye.json";
        string folder = file[..file.LastIndexOf('.')];
        EyeDoorOrientation[] sides = [EyeDoorOrientation.Left, EyeDoorOrientation.Right];
        static string SideName(EyeDoorOrientation side) => side == EyeDoorOrientation.Left ? "left" : "right";

        // The picture of one of the old game's draws, as one path for both sides: either both sides show the one
        // picture, or the two sides' pictures are named alike but for the side.
        string BothSides(Func<EyeDoorOrientation, RoomObjectDraw> draw)
        {
            string[] frames = [.. sides.Select(side => RoomObjectEyeDoorVisualFiles.VisualId(draw(side)))];
            if (frames.Distinct(StringComparer.Ordinal).Count() == 1)
                return RoomObjects.Picture(directory, frames[0], folder);
            string? shared = null;
            for (int index = 0; index < sides.Length; index++)
            {
                string side = SideName(sides[index]);
                if (!frames[index].StartsWith(side + "-", StringComparison.Ordinal))
                    throw new InvalidDataException($"The eye door's frame {frames[index]} is not named for its side, {side}.");
                string named = RoomObjects.Picture(directory, frames[index], folder).Replace($"/{side}_", "/{side}_", StringComparison.Ordinal);
                if (shared is not null && shared != named)
                    throw new InvalidDataException($"The eye door's pictures {shared} and {named} are not one picture named for each side.");
                shared = named;
            }

            return shared!;
        }

        // The closed blue cap it flashes to as it goes. The eye draws it itself, over the door behind it, as the old
        // eye draws it over the door's own blocks: a cap opened meanwhile does not show between the flashes.
        string BlueCapPicture()
        {
            string? shared = null;
            foreach (EyeDoorOrientation each in sides)
            {
                string side = SideName(each);
                if (EyeDoorDraws.BlueCapFacing(each).ToString().ToLowerInvariant() != side)
                    throw new InvalidDataException($"The eye door facing {side} flashes to a blue cap that faces another way.");
                string frame = $"{side}-closed";
                (int capLeft, int capTop, _, _) = RoomObjects.Bounds(GameInstallationLayout.RoomObjectBlueDoorVisualDirectoryName, frame);
                (int eyeLeft, int eyeTop, _, _) = RoomObjects.Bounds(directory, RoomObjectEyeDoorVisualFiles.VisualId(EyeDoorDraws.Of(each, EyeDoorObjectFrame.ClosedEye)));
                if ((capLeft, capTop) != (eyeLeft, eyeTop))
                    throw new InvalidDataException($"The blue cap the eye door facing {side} flashes to does not start where the eye's pictures do.");
                string named = RoomObjects.Picture(GameInstallationLayout.RoomObjectBlueDoorVisualDirectoryName, frame, DoorCapFolder)
                    .Replace($"/{side}_", "/{side}_", StringComparison.Ordinal);
                if (shared is not null && shared != named)
                    throw new InvalidDataException($"The blue cap pictures {shared} and {named} are not one picture named for each side.");
                shared = named;
            }

            return shared!.Contains("{side}", StringComparison.Ordinal)
                ? shared
                : throw new InvalidDataException($"The blue cap picture {shared} is not named for its side.");
        }

        // The eye's own frames, and the blue cap it flashes to.
        string EyePicture(EyeDoorObjectFrame frame) => frame == EyeDoorObjectFrame.BlueCap ? BlueCapPicture() : BothSides(side => EyeDoorDraws.Of(side, frame));

        // Whether a shot can land on the eye while it shows a frame: only where the old game makes its block one that shots trigger.
        static bool LidUp(EyeDoorObjectFrame frame)
        {
            if (frame == EyeDoorObjectFrame.BlueCap)
                return false;
            bool? up = null;
            foreach (EyeDoorOrientation side in new[] { EyeDoorOrientation.Left, EyeDoorOrientation.Right })
            {
                if (!RoomObjectDrawIdentity.TryFrame(EyeDoorDraws.Of(side, frame), out EyeDoorDraws.Draw draw))
                    throw new InvalidDataException($"The eye door's frame {frame} is not one of its own draws.");
                bool shootable = EyeDoorObjectDrawDefinitions.Collisions[draw].Contains(EyeDoorDefinitions.EyeCollision);
                if (up is not null && up != shootable)
                    throw new InvalidDataException($"The eye door's frame {frame} can be shot from one side and not the other.");
                up = shootable;
            }

            return up!.Value;
        }

        static string? Cue(EyeDoorAction action) => action switch
        {
            EyeDoorAction.Shoot => "shoot",
            EyeDoorAction.SpawnTwoSmoke => "smoke",
            EyeDoorAction.SpawnSweat => "sweat",
            _ => throw new InvalidDataException($"The eye door's animation raises {action}, which this does not know."),
        };

        // As it goes, the old eye has moved to the block above, the door's topmost, and draws from there.
        int destroyedRise = Offset16<RoomBlocks>.OneUp.Y * EngineRoomObjects.Block;

        // One of the eye's sequences: each frame's picture, whether its lid is up, and "done" as it runs out.
        JsonObject Sequence(Animation<EyeDoorSegment> animation, int rise)
        {
            (List<IAnimationBeat<EyeDoorObjectFrame, EyeDoorAction>> lead, List<IAnimationBeat<EyeDoorObjectFrame, EyeDoorAction>>? cycle) =
                EngineTimelines.Played(EyeDoorSequenceDefinitions.Segments, animation);
            if (cycle is not null)
                throw new InvalidDataException("One of the eye door's sequences never ends.");
            var steps = new JsonArray();
            bool? lidUp = null;
            var pending = new List<string>();
            string shown = "";
            foreach (IAnimationBeat<EyeDoorObjectFrame, EyeDoorAction> beat in lead)
            {
                switch (beat)
                {
                    case AnimationBeat<EyeDoorObjectFrame, EyeDoorAction>.Event raised:
                        if (Cue(raised.Raised) is { } heard)
                            pending.Add(heard);
                        break;
                    case AnimationBeat<EyeDoorObjectFrame, EyeDoorAction>.Frame frame:
                        var cues = new JsonArray();
                        bool up = LidUp(frame.Shown);
                        if (lidUp != up)
                            cues.Add(up ? "open" : "shut");
                        lidUp = up;
                        foreach (string raised in pending)
                            cues.Add(raised);
                        pending.Clear();
                        shown = EyePicture(frame.Shown);
                        var step = new JsonObject { ["picture"] = shown, ["updates"] = frame.Duration };
                        if (cues.Count > 0)
                            step["cues"] = cues;
                        if (rise != 0 && shown.Length > 0)
                            step["offset"] = new JsonObject { ["x"] = 0, ["y"] = rise };
                        steps.Add(step);
                        break;
                    default:
                        throw new InvalidDataException($"The eye door's sequence has a beat this does not know: {beat.GetType().Name}.");
                }
            }

            if (pending.Count > 0)
                throw new InvalidDataException("One of the eye door's sequences raises something after its last frame.");
            // The step it never rests on: reaching it is the sequence running out, and the eye begins its next at once.
            steps.Add(new JsonObject { ["picture"] = shown, ["updates"] = 1, ["cues"] = new JsonArray("done") });
            return new JsonObject { ["steps"] = steps };
        }

        // The growth round the door: one path for both sides and both parts.
        string PartPicture(EyeDoorPartPose pose)
        {
            string? shared = null;
            foreach ((EyeDoorBodyPart part, string partName) in new[] { (EyeDoorBodyPart.Middle, "middle"), (EyeDoorBodyPart.Bottom, "bottom") })
            {
                string named = BothSides(side => EyeDoorDraws.Of(new EyeDoorPart(part, side), pose))
                    .Replace($"_{partName}_", "_{part}_", StringComparison.Ordinal);
                if (shared is not null && shared != named)
                    throw new InvalidDataException($"The eye door's pictures {shared} and {named} are not one picture named for each part.");
                shared = named;
            }

            return shared!.Contains("{part}", StringComparison.Ordinal) && shared.Contains("{side}", StringComparison.Ordinal)
                ? shared
                : throw new InvalidDataException($"The eye door's picture {shared} is not named for its part and side.");
        }

        JsonObject PartTimeline(Animation<EyeDoorPartSegment> animation) =>
            EngineTimelines.Of(EyeDoorSequenceDefinitions.PartSegments, animation, PartPicture, static (NoAnimationCue _) => null);

        // The middle part lies on the block above the eye and the bottom part two below it, where its scene has them:
        // the eye is as tall as the old game makes it, its own block and those below that answer for it.
        (int left, int top, int columns, int rows) = RoomObjects.Bounds(directory, RoomObjectEyeDoorVisualFiles.VisualId(EyeDoorDraws.Of(EyeDoorOrientation.Left, EyeDoorObjectFrame.ClosedEye)));
        if (rows != 1 + EyeDoorDefinitions.EyeExtensionRows)
            throw new InvalidDataException("The eye door's eye is not drawn as tall as the old game makes it.");

        DustExplosionPlacement scattered = EyeDoorEffectDefinitions.ScatteredSmokePlacement, centred = EyeDoorEffectDefinitions.CenteredSmokePlacement;
        if (centred.XScatter.Width != 0 || centred.YScatter.Width != 0)
            throw new InvalidDataException("The puff of the eye door's destruction is scattered, which the Godot eye's is not.");
        static JsonObject At(int x, int y) => new() { ["x"] = x, ["y"] = y };
        static JsonObject Speed(PixelVelocity<RoomPixels> speed) => new() { ["x"] = EngineNames.Number(speed.X), ["y"] = EngineNames.Number(speed.Y) };
        static JsonObject Field(RandomSampleField field) => new() { ["from"] = field.Shift, ["digits"] = field.Width };
        JsonObject smoke = At(EyeDoorEffectDefinitions.SmokeCentre.X + scattered.Base.X, EyeDoorEffectDefinitions.SmokeCentre.Y + scattered.Base.Y);
        smoke["across"] = Field(scattered.XScatter);
        smoke["down"] = Field(scattered.YScatter);

        // What it spits forms, takes aim and flies; it bursts on terrain, and a shot destroys it.
        static string? SpitCue(EyeDoorProjectileCue cue) => cue.Kind switch
        {
            EyeDoorProjectileCueKind.AimAtSamus => "aim",
            EyeDoorProjectileCueKind.StartFlight => "fly",
            EyeDoorProjectileCueKind.StopMoving => "stop",
            EyeDoorProjectileCueKind.Delete => "end",
            _ => throw new InvalidDataException($"The eye door's spit raises {cue.Kind}, which this does not know."),
        };

        // Left as an effect, or played as a shot one's end, it only stops and takes itself away, which it does by ending.
        static string? EffectCue(EyeDoorProjectileCue cue) => cue.Kind is EyeDoorProjectileCueKind.StopMoving or EyeDoorProjectileCueKind.Delete
            ? null
            : throw new InvalidDataException($"An effect of the eye door raises {cue.Kind}, which this does not know.");

        string Shared<TSegment>(string timelineFile, UnpausedAnimationSegments<TSegment, EnemyProjectileFrameId, EyeDoorProjectileCue> segments, Animation<TSegment> animation,
            Func<EyeDoorProjectileCue, string?> cue, int rise = 0)
            where TSegment : struct
        {
            (List<IAnimationBeat<EnemyProjectileFrameId, EyeDoorProjectileCue>> lead, List<IAnimationBeat<EnemyProjectileFrameId, EyeDoorProjectileCue>>? cycle) =
                EngineTimelines.Played(segments, animation);
            JsonObject timeline = SharedTimeline(timelineFile, lead, cycle, EnemySheetDefinitions.ProjectileFrameName, cue);
            timeline["offset"]!["y"] = timeline["offset"]!["y"]!.GetValue<int>() + rise;
            _tree.WriteData(timelineFile, timeline);
            return timelineFile;
        }

        const string spitFile = "enemies/eye_door_spit.json";
        string spitFolder = spitFile[..spitFile.LastIndexOf('.')];
        string fired = Shared($"{spitFolder}/fired.json", EyeDoorProjectileAnimations.Segments, EyeDoorProjectileAnimations.StartingAt[EyeDoorProjectileSegment.Fired], SpitCue);
        string burst = Shared($"{spitFolder}/burst.json", EyeDoorProjectileAnimations.Segments, EyeDoorProjectileAnimations.StartingAt[EyeDoorProjectileSegment.Impact], SpitCue);
        string shot = Shared($"{spitFolder}/shot.json", EyeDoorProjectileAnimations.Segments, EyeDoorProjectileAnimations.StartingAt[EyeDoorProjectileSegment.Shot], EffectCue);
        new EngineObjectFile(_tree, spitFile, "EyeDoorSpit").Value("burst_timeline", burst).Write();

        EnemyProjectileDefinition spit = EnemyProjectileDefinitionCatalog.Definitions[RoomEnemyProjectileKind.EyeDoorProjectile];
        EnemyProjectileDefinition sweat = EnemyProjectileDefinitionCatalog.Definitions[RoomEnemyProjectileKind.EyeDoorSweat];
        // Its sweat falls, and where it meets the floor it leaves a puff a little above.
        string falling = Shared($"{folder}/sweat.json", EyeDoorSweatAnimations.Segments, EyeDoorSweatAnimations.StartingAt[EyeDoorSweatSegment.Falling], SpitCue);
        string splash = Shared($"{folder}/sweat_splash.json", EyeDoorSweatAnimations.Segments, EyeDoorSweatAnimations.StartingAt[EyeDoorSweatSegment.HitFloor], EffectCue,
            -EyeDoorEffectDefinitions.SweatSplashRise);

        static JsonObject Projectile(EnemyProjectileDefinition old, string timeline) => new()
        {
            // Something of no size still lies somewhere: one pixel.
            ["width"] = Math.Max(1, old.Radius.X * 2),
            ["height"] = Math.Max(1, old.Radius.Y * 2),
            ["meeting_target"] = old.Properties.HasFlag(EnemyProjectilePropertyFlags.PersistsOnContact) ? "passes_through_and_rehits" : "ends",
            ["met_by_shot"] = old.Properties.HasFlag(EnemyProjectilePropertyFlags.BlocksShots) ? "destroyed" : "ignored",
            ["view_margin"] = -1,
            ["timeline"] = timeline,
            ["picture_offset"] = EngineEnemy.NoOffset(),
        };

        JsonObject spitProjectile = Projectile(spit, fired);
        // It moves itself, and meets terrain its own way.
        spitProjectile["meeting_terrain"] = "passes_through";
        spitProjectile["standing"] = EngineEnemy.StandingOf(spit);
        // A shot one stops where it is in a cloud of fire, harming to the touch while that plays; touching Samus it is
        // only taken away.
        spitProjectile["shot_timeline"] = shot;
        // Its sweat moves itself too: a wall only stops its way across, and it ends, with its splash, only where it
        // comes down onto terrain. Ending on Samus it leaves nothing.
        JsonObject sweatProjectile = Projectile(sweat, falling);
        sweatProjectile["meeting_terrain"] = "passes_through";
        sweatProjectile["standing"] = EngineEnemy.StandingOf(sweat);
        new EngineObjectFile(_tree, "enemies/eye_door_sweat.json", "EyeDoorSweat")
            .Value("gravity", EngineNames.Number(EyeDoorEffectDefinitions.SweatGravity))
            .Value("splash_effect", splash)
            .Write();

        if (EyeDoorEffectDefinitions.LeftSweatLaunch.Y != EyeDoorEffectDefinitions.RightSweatLaunch.Y)
            throw new InvalidDataException("The eye door's sweat falls at one speed to the left and another to the right.");
        new EngineObjectFile(_tree, file, "EyeDoorEye")
            .Value("hits_to_destroy", EyeDoorSequenceDefinitions.HitThreshold)
            .Value("hit_sound", EngineNames.Sound(EyeDoorSequenceDefinitions.HitSound))
            .Value("dud_sound", EngineNames.Sound(EyeDoorDefinitions.RejectedShotSound))
            .Value("spit_sound", EngineNames.Sound(EyeDoorDefinitions.ProjectileSound))
            .Value("smoke_effect", DustFile(EyeDoorEffectDefinitions.ScatteredSmokeAnimation))
            .Value("puff_effect", DustFile(EyeDoorEffectDefinitions.CenteredSmokeAnimation))
            .Group("picture_offset", At(left * EngineRoomObjects.Block, top * EngineRoomObjects.Block))
            .Group("wake_reach", At(EyeDoorSequenceDefinitions.WakeReach.X, EyeDoorSequenceDefinitions.WakeReach.Y))
            .Group("in_front_reach", At(EyeDoorSequenceDefinitions.InFrontReach.X, EyeDoorSequenceDefinitions.InFrontReach.Y))
            .Group("pulse_reach", At(EyeDoorSequenceDefinitions.PulseReach.X, EyeDoorSequenceDefinitions.PulseReach.Y))
            .Group("spit_from", new JsonObject
            {
                ["left"] = At(EyeDoorEffectDefinitions.LeftShotOrigin.X, EyeDoorEffectDefinitions.LeftShotOrigin.Y),
                ["right"] = At(EyeDoorEffectDefinitions.RightShotOrigin.X, EyeDoorEffectDefinitions.RightShotOrigin.Y),
            })
            // Its sweat leaves from the same place whichever way it faces.
            .Group("sweat_from", At(EyeDoorEffectDefinitions.SweatOrigin.X, EyeDoorEffectDefinitions.SweatOrigin.Y))
            .Group("sweat_launch", new JsonObject
            {
                ["left"] = Speed(EyeDoorEffectDefinitions.LeftSweatLaunch),
                ["right"] = Speed(EyeDoorEffectDefinitions.RightSweatLaunch),
            })
            .Group("smoke", smoke)
            .Group("puff_at", At(EyeDoorEffectDefinitions.SmokeCentre.X + centred.Base.X, EyeDoorEffectDefinitions.SmokeCentre.Y + centred.Base.Y))
            .Group("shots", new JsonObject
            {
                ["spit"] = new JsonObject { ["damage"] = spit.Damage, ["scene"] = EyeDoorSpitScene, ["projectile"] = spitProjectile },
                ["sweat"] = new JsonObject { ["damage"] = sweat.Damage, ["scene"] = EyeDoorSweatScene, ["projectile"] = sweatProjectile },
            })
            .Collider("Hurtbox", left * EngineRoomObjects.Block, top * EngineRoomObjects.Block, columns * EngineRoomObjects.Block, rows * EngineRoomObjects.Block)
            .Timeline("closed", Sequence(EyeDoorSequenceDefinitions.Closed, 0))
            .Timeline("opening", Sequence(EyeDoorSequenceDefinitions.Opening, 0))
            .Timeline("watching", Sequence(EyeDoorSequenceDefinitions.Watching, 0))
            .Timeline("blinking", Sequence(EyeDoorSequenceDefinitions.Blinking, 0))
            .Timeline("flinching", Sequence(EyeDoorSequenceDefinitions.Flinching, 0))
            .Timeline("destroyed", Sequence(EyeDoorSequenceDefinitions.Destroyed, destroyedRise))
            .Timeline("part_still", PartTimeline(EyeDoorSequenceDefinitions.Still))
            .Timeline("part_pulse_left", PartTimeline(EyeDoorSequenceDefinitions.Pulse[EyeDoorOrientation.Left]))
            .Timeline("part_pulse_right", PartTimeline(EyeDoorSequenceDefinitions.Pulse[EyeDoorOrientation.Right]))
            .Write();
    }
}
