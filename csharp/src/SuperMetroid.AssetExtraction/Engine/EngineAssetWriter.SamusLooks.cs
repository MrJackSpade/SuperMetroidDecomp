using SuperMetroid.Core;
using SuperMetroid.Core.Spatial;
using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Colours;
using SuperMetroid.Core.Samus.Death;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using HudItem = SuperMetroid.Core.Samus.HudItem;
using P = SuperMetroid.Core.Samus.Poses.SamusPoseId;
using Seg = SuperMetroid.Core.Samus.Poses.SamusAnimationSegmentName;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>
/// How Samus looks: her pictures, cut once in their sheet's own colours, and a timeline for every look the Godot game gives her,
/// each the old pose's own animation. The Godot game names a timeline for her look, the way she faces and her aim.
/// </summary>
public sealed partial class EngineAssetWriter
{
    /// <summary>The family of sheets Samus, her shots and her effects are on.</summary>
    private const string SpriteFamily = GameInstallationLayout.SpriteSheetDirectoryName;

    /// <summary>Her suits, as her colours are named for them.</summary>
    private static readonly (string Name, SamusSuit Suit)[] SamusSuits =
        [("power", SamusSuit.Power), ("varia", SamusSuit.Varia), ("gravity", SamusSuit.Gravity)];

    /// <summary>The two sides she faces, as her timelines are named for them.</summary>
    private static readonly string[] SamusSides = ["right", "left"];

    private InstalledSamusBody? _samusBody;
    private EngineSheetPictures? _samusPictures;

    /// <summary>What the installation holds of Samus's body: which picture each frame of each pose shows, and how high it sits.</summary>
    private InstalledSamusBody SamusBody => _samusBody ??= _installation.LoadSamusBody();

    /// <summary>The sheet her body's pictures are on.</summary>
    private static string SamusBodySheet => SpriteSheetDefinitions.FileNames[SpriteSheetId.SamusBody];

    /// <summary>
    /// Her body's pictures, cut once, in their sheet's own colours, onto canvases that are all alike, by the frame's
    /// name. Every colouring of her, her suits' own among them, is a change of those colours ("samus/colours.json"),
    /// so each picture some of whose colours are changed by region has its map of regions beside it.
    /// </summary>
    private EngineSheetPictures SamusPictures => _samusPictures ??= _sheets.CutNamed(
        _tree, SpriteFamily, SamusBodyPlace, SamusBodySheet, _sheets.FramesOf(SpriteFamily, SamusBodySheet), shade: null, EngineSheets.FileName,
        regionMaps: true);

    /// <summary>The picture of one frame of her body's sheet; empty for a frame that shows nothing.</summary>
    private string SamusPicture(string frame) =>
        _sheets.ShowsNothing(SpriteFamily, SamusBodySheet, frame) ? "" : SamusPictures.Of(frame);

    /// <summary>One step of a timeline of hers: a frame of a pose's animation shown for so many updates.</summary>
    private readonly partial record struct SamusStep(P Pose, SamusAnimationFrame Frame, int Updates);

    /// <summary>One step as a timeline writes it: the picture the frame shows, lifted as the installation lifts it.</summary>
    private JsonObject SamusStepWritten(SamusStep step)
    {
        if (step.Updates <= 0)
            throw new InvalidDataException($"Samus's pose {step.Pose} shows its frame {step.Frame} for no updates, which a timeline cannot.");
        if (!SamusBody.ShownFrames.TryPictureFrame(step.Pose, step.Frame, out SamusAnimationFrame picture))
            throw new InvalidDataException($"The installation has no picture for Samus's pose {step.Pose} at its frame {step.Frame}.");
        var written = new JsonObject { ["picture"] = SamusPicture(new SamusBodyFrame(step.Pose, picture).Name), ["updates"] = step.Updates };

        // The old game draws the picture so many pixels above her place.
        int rise = SamusBody.Gameplay.OriginRise(step.Pose, picture);
        if (rise != 0)
            written["offset"] = new JsonObject { ["x"] = 0, ["y"] = -rise };
        return written;
    }

    /// <summary>A timeline of her body's pictures.</summary>
    private JsonObject SamusTimeline(IEnumerable<JsonObject> steps, int? loopsTo)
    {
        var written = new JsonArray();
        foreach (JsonObject step in steps)
            written.Add(step);
        if (written.Count == 0)
            throw new InvalidDataException("A timeline of Samus's shows no frame.");
        var timeline = new JsonObject
        {
            ["steps"] = written,
            ["offset"] = new JsonObject { ["x"] = SamusPictures.OffsetX, ["y"] = SamusPictures.OffsetY },
        };
        if (loopsTo is { } to)
            timeline["loops_to"] = to;
        return timeline;
    }

    /// <summary>A timeline of steps of her poses.</summary>
    private JsonObject SamusTimeline((List<SamusStep> Steps, int? LoopsTo) played) =>
        SamusTimeline(played.Steps.Select(SamusStepWritten), played.LoopsTo);

    /// <summary>The frames of one segment of a pose's animation, each for its own length.</summary>
    private static List<SamusStep> SamusSegment(P pose, Seg segment)
    {
        SamusAnimationSegment played = SamusPoseTraits.Of(pose).Animation.Segment(segment);
        var steps = new List<SamusStep>();
        for (int frame = 0; frame < played.Delays.Count; frame++)
            steps.Add(new SamusStep(pose, new SamusAnimationFrame(segment, frame), played.Delays[frame]));
        return steps;
    }

    /// <summary>
    /// The steps a pose's animation plays, and the step it comes round to where it goes on for good. It is played from
    /// its first segment, or from <paramref name="from"/>: each segment's frames, then on as its end says. A segment
    /// that plays one again comes round to it; breathing comes round while her energy is not low; one that hands on to
    /// another pose ends the timeline, since what she does next is the Godot game's to say.
    /// </summary>
    /// <param name="pose">The pose.</param>
    /// <param name="from">The segment to begin at; the animation's first when not given.</param>
    /// <param name="oneSegment">Whether to play that one segment and no more: it comes round only if it plays itself again.</param>
    private static (List<SamusStep> Steps, int? LoopsTo) SamusPlayed(P pose, Seg? from = null, bool oneSegment = false)
    {
        IReadOnlyList<SamusAnimationSegment> segments = SamusPoseTraits.Of(pose).Animation.Segments;
        int IndexOf(Seg name)
        {
            for (int index = 0; index < segments.Count; index++)
            {
                if (segments[index].Name == name)
                    return index;
            }

            throw new InvalidDataException($"The animation of Samus's pose {pose} has no segment {name}.");
        }

        var steps = new List<SamusStep>();
        var firstStep = new Dictionary<Seg, int>();
        int at = from is { } named ? IndexOf(named) : 0;
        while (true)
        {
            SamusAnimationSegment segment = segments[at];
            if (firstStep.TryGetValue(segment.Name, out int round))
                return (steps, round);
            firstStep[segment.Name] = steps.Count;
            steps.AddRange(SamusSegment(pose, segment.Name));
            switch (segment.End)
            {
                case SamusAnimationRepeat repeat when firstStep.TryGetValue(repeat.Target.Segment, out int first):
                    return (steps, first + repeat.Target.Frame);
                case SamusAnimationRestart when firstStep.TryGetValue(segments[0].Name, out int start):
                    return (steps, start);
                case SamusAnimationLoopUnlessLowEnergy:
                    return (steps, firstStep[segment.Name]);
                case not null when oneSegment:
                    return (steps, null);
                case null when oneSegment:
                    return (steps, null);
                case null:
                    at++;
                    break;
                case SamusAnimationRepeat repeat:
                    if (repeat.Target.Frame != 0)
                        throw new InvalidDataException($"The animation of Samus's pose {pose} goes on into the middle of a segment it has not played.");
                    at = IndexOf(repeat.Target.Segment);
                    break;
                case SamusAnimationRestart:
                    at = 0;
                    break;
                case SamusAnimationCue cue when cue.ContinuesToNextSegment:
                    at++;
                    break;
                default:
                    return (steps, null);
            }
        }
    }

    /// <summary>One frame of a pose held for good.</summary>
    private static (List<SamusStep> Steps, int? LoopsTo) SamusHeld(P pose, SamusAnimationFrame frame) => ([new SamusStep(pose, frame, 1)], 0);

    /// <summary>How far above her middle a pose's shots start; the two sides' poses must agree.</summary>
    private static int SamusShotHeight(P right, P left)
    {
        int height = SamusPoseTraits.Of(right).ProjectileHeight;
        return SamusPoseTraits.Of(left).ProjectileHeight == height
            ? height
            : throw new InvalidDataException($"Samus's poses {right} and {left} start their shots at different heights, which one height for the look cannot say.");
    }

    /// <summary>How far above her middle the shots of all of <paramref name="poses"/> start; they must agree.</summary>
    private static int SamusShotHeight(string named, params P[] poses)
    {
        int height = SamusPoseTraits.Of(poses[0]).ProjectileHeight;
        return poses.All(pose => SamusPoseTraits.Of(pose).ProjectileHeight == height)
            ? height
            : throw new InvalidDataException($"Samus's poses for {named} start their shots at different heights, which one height cannot say.");
    }

    /// <summary>
    /// Where the arm cannon's cover lies at each step of a timeline, or null when none of its steps shows the cover:
    /// for each step an empty group where the pose draws no cover, and otherwise its place from hers, lifted as the pose's
    /// art is, the way it aims and whether it lies in front of her body. The aims used are gathered into
    /// <paramref name="aims"/>.
    /// </summary>
    private JsonArray? ArmCannonPlaces(List<SamusStep> steps, HashSet<SamusProjectileDirection> aims)
    {
        var places = new JsonArray();
        bool shows = false;
        foreach (SamusStep step in steps)
        {
            SamusArmCannonPlacement placement = SamusBody.Gameplay.ArmCannon.Placement(step.Pose);
            if (placement.DrawOrder == SamusArmCannonDrawOrder.Hidden)
            {
                places.Add(new JsonObject());
                continue;
            }

            if (!placement.Offsets.TryGetValue(step.Frame, out Offset16<RoomPixels> offset))
                throw new InvalidDataException($"Samus's pose {step.Pose} draws the arm cannon's cover, but the installation has no place for it at its frame {step.Frame}.");
            SamusProjectileDirection aim = placement.Direction(step.Pose, step.Frame);
            aims.Add(aim);
            places.Add(new JsonObject
            {
                ["x"] = offset.X,
                ["y"] = offset.Y - SamusBody.Gameplay.ArtOrigin(step.Pose),
                ["aim"] = EngineSheets.FileName(aim.ToString()),
                // The old game lists its sprites frontmost first: a cover drawn before her body lies in front of it.
                ["in_front"] = placement.DrawOrder == SamusArmCannonDrawOrder.BeforeBody,
            });
            shows = true;
        }

        return shows ? places : null;
    }

    /// <summary>
    /// The arm cannon's cover: how long what she has selected must stay as it is before the cover opens or closes, what
    /// opens it, its pictures for each way it aims (slightly, half and fully open), where their corner lies from the
    /// cover's place, and its <paramref name="places"/> by timeline.
    /// </summary>
    private JsonObject ArmCannonWritten(JsonObject places, IReadOnlyCollection<SamusProjectileDirection> aims)
    {
        const string prefix = "ArmCannon.";
        SamusArmCannonCoverFrame[] opening = [SamusArmCannonCoverFrame.SlightlyOpen, SamusArmCannonCoverFrame.HalfOpen, SamusArmCannonCoverFrame.FullyOpen];
        string sheet = SpriteSheetDefinitions.FileNames[SpriteSheetId.SamusOverlays];
        EngineSheetPictures cut = _sheets.CutNamed(
            _tree, SpriteFamily, "samus/arm_cannon", sheet,
            aims.SelectMany(aim => opening.Select(frame => SamusOverlaySheetDefinitions.ArmCannonFrame(aim, frame))),
            shade: null, frame => EngineSheets.FileName(frame[prefix.Length..]), regionMaps: true);
        var pictures = new JsonObject();
        foreach (SamusProjectileDirection aim in aims)
            pictures[EngineSheets.FileName(aim.ToString())] = EngineNames.List(opening.Select(frame => (JsonNode)cut.Of(SamusOverlaySheetDefinitions.ArmCannonFrame(aim, frame))));

        // What she has selected, as the Godot game names it.
        static string Selection(HudItem item) => item switch
        {
            HudItem.Beam => "",
            HudItem.Missile => "missiles",
            HudItem.SuperMissile => "super_missiles",
            HudItem.PowerBomb => "power_bombs",
            HudItem.Grapple => "grapple_beam",
            HudItem.XRay => "xray_scope",
            _ => throw new InvalidDataException($"Samus's selection {item} has no name in the Godot game."),
        };

        return new JsonObject
        {
            ["steady_updates"] = SamusArmCannonDefinitions.StableSelectionUpdates,
            ["opened_by"] = EngineNames.List(Enum.GetValues<HudItem>().Where(SamusArmCannonOpening.Opens).Select(item => (JsonNode)Selection(item))),
            ["offset"] = new JsonObject { ["x"] = cut.OffsetX, ["y"] = cut.OffsetY },
            ["pictures"] = pictures,
            ["places"] = places,
        };
    }

    /// <summary>
    /// Writes every timeline of Samus's into <paramref name="timelines"/>, into <paramref name="heights"/> how far
    /// above her middle the shots of each look start: by the look's name, and by the look's and the aim's where an aim has
    /// a pose of its own, and into <paramref name="coverPlaces"/> where the arm cannon's cover lies at each step of each
    /// timeline that shows it, gathering the ways it aims into <paramref name="coverAims"/>.
    /// </summary>
    private void WriteSamusLooks(JsonObject timelines, JsonObject heights, JsonObject coverPlaces, HashSet<SamusProjectileDirection> coverAims)
    {
        void Add(string name, (List<SamusStep> Steps, int? LoopsTo) played)
        {
            timelines.Add(name, SamusTimeline(played));
            if (ArmCannonPlaces(played.Steps, coverAims) is { } places)
                coverPlaces.Add(name, places);
        }

        // A look, or a look with one aim, shown by one old pose for each side.
        void Look(string look, string? aim, P right, P left, Seg? from = null, bool oneSegment = false, string suffix = "")
        {
            Add($"{look}_right{(aim is null ? "" : "_" + aim)}{suffix}", SamusPlayed(right, from, oneSegment));
            Add($"{look}_left{(aim is null ? "" : "_" + aim)}{suffix}", SamusPlayed(left, from, oneSegment));
            if (suffix.Length == 0)
                heights[aim is null ? look : $"{look}_{aim}"] = SamusShotHeight(right, left);
        }

        // The pictures shown once as one look gives way to another.
        void Passing(string from, string to, P right, P left, string held = "")
        {
            string suffix = held.Length == 0 ? "" : "_" + held;
            Add($"{from}_to_{to}_right{suffix}", SamusPlayed(right));
            Add($"{from}_to_{to}_left{suffix}", SamusPlayed(left));
        }

        Look("standing", null, P.FacingRightNormalPose, P.FacingLeftNormalPose);
        Look("standing", null, P.FacingRightNormalPose, P.FacingLeftNormalPose, from: Seg.LowEnergyBreathe, suffix: "_low");
        Look("standing", "up", P.StandingAimUpRightPose, P.StandingAimUpLeftPose);
        Look("standing", "diagonal_up", P.StandingAimDiagonalUpRightPose, P.StandingAimDiagonalUpLeftPose);
        Look("standing", "diagonal_down", P.StandingAimDiagonalDownRightPose, P.StandingAimDiagonalDownLeftPose);

        // Stopped against a wall she has run into, level and aiming diagonally up and down.
        Look("wall_stopped", null, P.RanIntoWallRightPose, P.RanIntoWallLeftPose);
        Look("wall_stopped", "diagonal_up", P.RanIntoWallAimUpRightPose, P.RanIntoWallAimUpLeftPose);
        Look("wall_stopped", "diagonal_down", P.RanIntoWallAimDownRightPose, P.RanIntoWallAimDownLeftPose);

        Look("running", null, P.MovingRightNormalPose, P.MovingLeftNormalPose);
        Look("running", "diagonal_up", P.RunningAimDiagonalUpRightPose, P.RunningAimDiagonalUpLeftPose);
        Look("running", "diagonal_down", P.RunningAimDiagonalDownRightPose, P.RunningAimDiagonalDownLeftPose);

        // Running with her arm held out, once the attack has been held in a run aiming level.
        Look("running", "arm_out", P.MovingRightGunExtendedPose, P.MovingLeftGunExtendedPose);

        // The Godot Samus, like the old turning poses, faces the way she turns to from the turn's start.
        Look("turning", null, P.TurningLeftToRightPose, P.TurningRightToLeftPose);
        Look("turning", "up", P.TurningLeftToRightAimUpPose, P.TurningRightToLeftAimUpPose);
        Look("turning", "diagonal_up", P.TurningLeftToRightAimDiagonalUpPose, P.TurningRightToLeftAimDiagonalUpPose);
        Look("turning", "diagonal_down", P.TurningLeftToRightAimDiagonalDownPose, P.TurningRightToLeftAimDiagonalDownPose);

        Look("crouching", null, P.CrouchingRightPose, P.CrouchingLeftPose);
        Look("crouching", null, P.CrouchingRightPose, P.CrouchingLeftPose, from: Seg.LowEnergyBreathe, suffix: "_low");
        Look("crouching", "up", P.CrouchingAimUpRightPose, P.CrouchingAimUpLeftPose);
        Look("crouching", "diagonal_up", P.CrouchingAimDiagonalUpRightPose, P.CrouchingAimDiagonalUpLeftPose);
        Look("crouching", "diagonal_down", P.CrouchingAimDiagonalDownRightPose, P.CrouchingAimDiagonalDownLeftPose);

        Look("jumping", null, P.NeutralJumpRightPose, P.NeutralJumpLeftPose);
        Look("jumping", "up", P.NormalJumpAimUpRightPose, P.NormalJumpAimUpLeftPose);
        Look("jumping", "down", P.NormalJumpAimDownRightPose, P.NormalJumpAimDownLeftPose);
        Look("jumping", "diagonal_up", P.NormalJumpAimDiagonalUpRightPose, P.NormalJumpAimDiagonalUpLeftPose);
        Look("jumping", "diagonal_down", P.NormalJumpAimDiagonalDownRightPose, P.NormalJumpAimDiagonalDownLeftPose);

        // Aiming level in a jump: steering the way she faces, and with her arm held out.
        Look("jumping", "moving", P.NormalJumpForwardRightPose, P.NormalJumpForwardLeftPose);
        Look("jumping", "arm_out", P.NormalJumpGunExtendedRightPose, P.NormalJumpGunExtendedLeftPose);

        Look("falling", null, P.FallingRightPose, P.FallingLeftPose);
        Look("falling", "up", P.FallingAimUpRightPose, P.FallingAimUpLeftPose);
        Look("falling", "down", P.FallingAimDownRightPose, P.FallingAimDownLeftPose);
        Look("falling", "diagonal_up", P.FallingAimDiagonalUpRightPose, P.FallingAimDiagonalUpLeftPose);
        Look("falling", "diagonal_down", P.FallingAimDiagonalDownRightPose, P.FallingAimDiagonalDownLeftPose);
        Look("falling", "arm_out", P.FallingGunExtendedRightPose, P.FallingGunExtendedLeftPose);

        // A plain fall that has reached its fast-fall speed: the fast fall's picture, then its held one over and over.
        Look("falling", "fast", P.FallingRightPose, P.FallingLeftPose, from: Seg.FastFall);

        // Turning in the air, out of a jump and out of a fall. She faces the new way from the turn's start, so the
        // pose that turns to the right is the right-facing look. A turn made aiming diagonally down shows the turn
        // that aims straight down, as the old choice of poses has it, and there is none aiming diagonally down.
        Look("air_turning_jump", null, P.TurningLeftToRightJumpPose, P.TurningRightToLeftJumpPose);
        Look("air_turning_jump", "up", P.TurningLeftToRightJumpAimUpPose, P.TurningRightToLeftJumpAimUpPose);
        Look("air_turning_jump", "down", P.TurningLeftToRightJumpAimDownPose, P.TurningRightToLeftJumpAimDownPose);
        Look("air_turning_jump", "diagonal_up", P.TurningLeftToRightJumpAimDiagonalUpPose, P.TurningRightToLeftJumpAimDiagonalUpPose);
        Look("air_turning_jump", "diagonal_down", P.TurningLeftToRightJumpAimDownPose, P.TurningRightToLeftJumpAimDownPose);
        Look("air_turning_fall", null, P.TurningLeftToRightFallingPose, P.TurningRightToLeftFallingPose);
        Look("air_turning_fall", "up", P.TurningLeftToRightFallingAimUpPose, P.TurningRightToLeftFallingAimUpPose);
        Look("air_turning_fall", "down", P.TurningLeftToRightFallingAimDownPose, P.TurningRightToLeftFallingAimDownPose);
        Look("air_turning_fall", "diagonal_up", P.TurningLeftToRightFallingAimDiagonalUpPose, P.TurningRightToLeftFallingAimDiagonalUpPose);
        Look("air_turning_fall", "diagonal_down", P.TurningLeftToRightFallingAimDownPose, P.TurningRightToLeftFallingAimDownPose);

        // The kick off the wall, and each of the spins it goes into once the kick has been shown: the Godot game
        // chooses among them as the old pose's own cue does.
        Look("wall_jumping", null, P.WallJumpRightPose, P.WallJumpLeftPose, from: Seg.Launch, oneSegment: true);
        Look("wall_jump_spinning", null, P.WallJumpRightPose, P.WallJumpLeftPose, from: Seg.OrdinarySpin, oneSegment: true);
        Look("wall_jump_space", null, P.WallJumpRightPose, P.WallJumpLeftPose, from: Seg.SpaceJumpSpin, oneSegment: true);
        Look("wall_jump_screw", null, P.WallJumpRightPose, P.WallJumpLeftPose, from: Seg.ScrewAttackSpin, oneSegment: true);
        Look("spin_jumping", null, P.SpinJumpRightPose, P.SpinJumpLeftPose);
        Look("space_jumping", null, P.SpaceJumpRightPose, P.SpaceJumpLeftPose);
        Look("screw_attacking", null, P.ScrewAttackRightPose, P.ScrewAttackLeftPose);

        // The braced picture each kind of spin shows for as long as it is ready to jump off a wall it has found.
        Look("spin_jumping_wall_ready", null, P.SpinJumpRightPose, P.SpinJumpLeftPose, from: Seg.WallJumpReady, oneSegment: true);
        Look("space_jumping_wall_ready", null, P.SpaceJumpRightPose, P.SpaceJumpLeftPose, from: Seg.WallJumpReady, oneSegment: true);
        Look("screw_attacking_wall_ready", null, P.ScrewAttackRightPose, P.ScrewAttackLeftPose, from: Seg.WallJumpReady, oneSegment: true);
        Look("morph_ball", null, P.MorphBallGroundRightPose, P.MorphBallGroundLeftPose);
        Look("knocked_back", null, P.KnockbackRightPose, P.KnockbackLeftPose);
        Look("damage_boost", null, P.DamageBoostRightPose, P.DamageBoostLeftPose);

        Look("moonwalking", null, P.MoonwalkFacingRightPose, P.MoonwalkFacingLeftPose);
        Look("moonwalking", "diagonal_up", P.MoonwalkAimUpRightPose, P.MoonwalkAimUpLeftPose);
        Look("moonwalking", "diagonal_down", P.MoonwalkAimDownRightPose, P.MoonwalkAimDownLeftPose);

        Look("grabbed", null, P.DraygonGrabbedNeutralRightPose, P.DraygonGrabbedNeutralLeftPose);
        Look("grabbed", "diagonal_up", P.DraygonGrabbedAimUpRightPose, P.DraygonGrabbedAimUpLeftPose);
        Look("grabbed", "diagonal_down", P.DraygonGrabbedAimDownRightPose, P.DraygonGrabbedAimDownLeftPose);
        Look("grabbed", "firing", P.DraygonGrabbedFiringRightPose, P.DraygonGrabbedFiringLeftPose);
        Look("grabbed", "moving", P.DraygonGrabbedMovingRightPose, P.DraygonGrabbedMovingLeftPose);

        Look("shinespark_windup", null, P.ShinesparkWindupRightPose, P.ShinesparkWindupLeftPose);
        Look("shinespark_horizontal", null, P.ShinesparkHorizontalRightPose, P.ShinesparkHorizontalLeftPose);
        Look("shinespark_vertical", null, P.ShinesparkVerticalRightPose, P.ShinesparkVerticalLeftPose);
        Look("shinespark_diagonal", null, P.ShinesparkDiagonalRightPose, P.ShinesparkDiagonalLeftPose);

        // Held in place by the grapple's line, standing or crouched. A line fired straight down holds her in the
        // down-left pose whichever way she faces, crouched or not, as the old table of connections has it.
        Look("grapple_standing", null, P.GrappleStandingRightPose, P.GrappleStandingLeftPose);
        Look("grapple_standing", "diagonal_down", P.GrappleStandingDownRightPose, P.GrappleStandingDownLeftPose);
        Look("grapple_standing", "down", P.GrappleStandingDownLeftPose, P.GrappleStandingDownLeftPose);
        Look("grapple_crouching", null, P.GrappleCrouchingRightPose, P.GrappleCrouchingLeftPose);
        Look("grapple_crouching", "diagonal_down", P.GrappleCrouchingDownRightPose, P.GrappleCrouchingDownLeftPose);
        Look("grapple_crouching", "down", P.GrappleStandingDownLeftPose, P.GrappleStandingDownLeftPose);

        // Against a wall, holding on by the line.
        Look("grapple_wall_contact", null, P.GrappleWallContactRightPose, P.GrappleWallContactLeftPose);

        // Swinging, she is shown one of so many ways by the way the line runs from her.
        for (int way = 0; way < GrappleSwingDirection.Count; way++)
        {
            SamusAnimationFrame frame = new GrappleSwingPicture(GrappleSwingDirection.FromNumber(way)).AnimationFrame;
            Add($"grappling_{way}_right", SamusHeld(P.GrappleSwingRightPose, frame));
            Add($"grappling_{way}_left", SamusHeld(P.GrappleSwingLeftPose, frame));
        }

        // Looking through the scope, her head is held one of five ways.
        (string Name, SamusAnimationFrame Frame)[] scope =
        [
            ("up", XrayScopeDefinitions.Aims.Up), ("raised", XrayScopeDefinitions.Aims.Raised), ("level", XrayScopeDefinitions.Aims.Level),
            ("lowered", XrayScopeDefinitions.Aims.Lowered), ("down", XrayScopeDefinitions.Aims.Down),
        ];
        foreach ((string name, SamusAnimationFrame frame) in scope)
        {
            Add($"xray_standing_{name}_right", SamusHeld(P.XrayingStandingRightPose, frame));
            Add($"xray_standing_{name}_left", SamusHeld(P.XrayingStandingLeftPose, frame));
            Add($"xray_crouching_{name}_right", SamusHeld(P.XrayingCrouchingRightPose, frame));
            Add($"xray_crouching_{name}_left", SamusHeld(P.XrayingCrouchingLeftPose, frame));
        }

        // Facing out of the screen: in a station, in the gunship, on an elevator, taking a suit.
        SamusAnimationFrame still = new(Seg.Still, 0);
        Add("facing_forward", SamusHeld(P.ForwardFacingPowerSuitPose, still));
        Add("collecting_suit", SamusHeld(P.ForwardFacingPowerSuitPose, still));

        // The old game has one pose facing out of the screen for the power suit and another for the other two.
        Add("facing_forward_suited", SamusHeld(P.ForwardFacingSuitedPose, still));
        Add("collecting_suit_suited", SamusHeld(P.ForwardFacingSuitedPose, still));

        // Appearing where a saved game is taken up: the front view's appearance, played through to the picture it
        // rests on. Its first picture is given as the game loads and counted down from her first update, so it shows
        // for an update less than it is given.
        foreach ((string name, P pose) in new[] { ("appearing", P.ForwardFacingPowerSuitPose), ("appearing_suited", P.ForwardFacingSuitedPose) })
        {
            (List<SamusStep> steps, int? loopsTo) = SamusPlayed(pose, Seg.Appearance);
            steps[0] = steps[0] with { Updates = SuperMetroid.Core.World.SavedGameAppearanceDefinitions.FirstFrameTimer - 1 };
            Add(name, (steps, loopsTo));
        }

        // A crystal flash: she rises, glows while her ammunition is taken, and finishes standing.
        foreach ((string side, P pose, P standing) in new[]
        {
            ("right", P.CrystalFlashRightPose, P.FacingRightNormalPose),
            ("left", P.CrystalFlashLeftPose, P.FacingLeftNormalPose),
        })
        {
            // A picture the flash itself turns to is given the flash's own hold, which is counted down on the update
            // it is given: it shows for an update less.
            static SamusStep TurnedTo(SamusStep first) => first with { Updates = CrystalFlashDefinitions.PhasePictureHold - 1 };

            List<SamusStep> steps = [.. SamusSegment(pose, Seg.Rise)];
            int risen = steps.Sum(step => step.Updates);
            List<SamusStep> rising = SamusSegment(pose, Seg.Rising);
            while (risen < CrystalFlashDefinitions.RaiseUpdates)
            {
                steps.AddRange(rising);
                risen += rising.Sum(step => step.Updates);
            }

            // The glow's first picture is turned to as the rise ends, and the glow comes round from its second.
            List<SamusStep> glowing = SamusSegment(pose, Seg.Glow);
            steps.Add(TurnedTo(glowing[0]));
            int glow = steps.Count;
            steps.AddRange(glowing.Skip(1));
            steps.Add(glowing[0]);
            Add($"crystal_flash_{side}", (steps, glow));

            // The finish is turned to as the last of her ammunition is taken, and ends with her standing.
            List<SamusStep> finish = SamusSegment(pose, Seg.Finish);
            finish[0] = TurnedTo(finish[0]);
            finish.Add(SamusPlayed(standing).Steps[0] with { Updates = 1 });
            Add($"crystal_flash_finish_{side}", (finish, finish.Count - 1));
        }

        // Drained. The old game shows the left-facing crouch whichever way she faces for all but kneeling and rising,
        // so those looks are written once, for either side.
        Add("drained_pinned", SamusPlayed(SamusDrainedAnimationDefinitions.RainbowBeamKnockbackPose));
        Add("drained_right", SamusPlayed(P.DrainedCrouchingRightPose, Seg.Held));
        Add("drained_left", SamusPlayed(P.DrainedCrouchingLeftPose, Seg.Held));
        Add("drained_struggling", SamusPlayed(SamusDrainedAnimationDefinitions.GetUpPose, Seg.FailedStand, oneSegment: true));
        Add("drained_getting_up_right", SamusPlayed(P.DrainedCrouchingRightPose, Seg.StandUp, oneSegment: true));
        Add("drained_getting_up_left", SamusPlayed(P.DrainedCrouchingLeftPose, Seg.StandUp, oneSegment: true));
        Add("drained_standing_right", SamusPlayed(P.DrainedStandingRightPose));
        Add("drained_standing_left", SamusPlayed(P.DrainedStandingLeftPose));
        Add("drained_frozen", SamusPlayed(SamusDrainedAnimationDefinitions.GetUpPose, Seg.Frozen));

        WriteSamusDeath(timelines);

        // Landing, crouching and curling up each pass through a picture or two of their own.
        // A landing out of a jump or a fall is by the way she aims, as the old table of landings has it: its own
        // pictures aiming up and diagonally, the firing landing aiming level with the attack held, and the plain one
        // aiming level or straight down.
        // Her shots leave from a height of their own while she settles from any landing, and while a jump gathers.
        heights["landing"] = SamusShotHeight(
            "landing",
            P.NormalLandingRightPose, P.NormalLandingLeftPose, P.LandingAimUpRightPose, P.LandingAimUpLeftPose,
            P.LandingAimDiagonalUpRightPose, P.LandingAimDiagonalUpLeftPose, P.LandingAimDiagonalDownRightPose, P.LandingAimDiagonalDownLeftPose,
            P.FiringLandingRightPose, P.FiringLandingLeftPose, P.SpinLandingRightPose, P.SpinLandingLeftPose);
        heights["jump_start"] = SamusShotHeight(
            "a jump's gathering",
            P.NeutralJumpTransitionRightPose, P.NeutralJumpTransitionLeftPose,
            P.NormalJumpTransitionAimUpRightPose, P.NormalJumpTransitionAimUpLeftPose,
            P.NormalJumpTransitionAimDiagonalUpRightPose, P.NormalJumpTransitionAimDiagonalUpLeftPose,
            P.NormalJumpTransitionAimDiagonalDownRightPose, P.NormalJumpTransitionAimDiagonalDownLeftPose);

        // She can fire while she crouches down and while she stands up, from heights those poses have of their own,
        // by the way she aims.
        heights["changing_to_crouch"] = SamusShotHeight(P.CrouchingTransitionRightPose, P.CrouchingTransitionLeftPose);
        heights["changing_to_stand"] = SamusShotHeight(P.StandingTransitionRightPose, P.StandingTransitionLeftPose);
        heights["changing_to_crouch_up"] = SamusShotHeight(P.CrouchingTransitionAimUpRightPose, P.CrouchingTransitionAimUpLeftPose);
        heights["changing_to_stand_up"] = SamusShotHeight(P.StandingTransitionAimUpRightPose, P.StandingTransitionAimUpLeftPose);
        heights["changing_to_crouch_diagonal_up"] = SamusShotHeight(P.CrouchingTransitionAimDiagonalUpRightPose, P.CrouchingTransitionAimDiagonalUpLeftPose);
        heights["changing_to_stand_diagonal_up"] = SamusShotHeight(P.StandingTransitionAimDiagonalUpRightPose, P.StandingTransitionAimDiagonalUpLeftPose);
        heights["changing_to_crouch_diagonal_down"] = SamusShotHeight(P.CrouchingTransitionAimDiagonalDownRightPose, P.CrouchingTransitionAimDiagonalDownLeftPose);
        heights["changing_to_stand_diagonal_down"] = SamusShotHeight(P.StandingTransitionAimDiagonalDownRightPose, P.StandingTransitionAimDiagonalDownLeftPose);

        // The turn a moonwalk makes with the jump fires the shot the moonwalk hands it from heights of its own, by the
        // moonwalk's aim. She faces the way she turns to, so the pose that turns to the right is the right-facing one.
        heights["moonwalk_jump_turn"] = SamusShotHeight(P.MoonwalkTurnToSpinJumpRightPose, P.MoonwalkTurnToSpinJumpLeftPose);
        heights["moonwalk_jump_turn_diagonal_up"] = SamusShotHeight(P.MoonwalkTurnToSpinJumpAimUpRightPose, P.MoonwalkTurnToSpinJumpAimUpLeftPose);
        heights["moonwalk_jump_turn_diagonal_down"] = SamusShotHeight(P.MoonwalkTurnToSpinJumpAimDownRightPose, P.MoonwalkTurnToSpinJumpAimDownLeftPose);

        foreach (string from in new[] { "falling", "jumping" })
        {
            Passing(from, "standing", P.NormalLandingRightPose, P.NormalLandingLeftPose);
            Passing(from, "standing", P.LandingAimUpRightPose, P.LandingAimUpLeftPose, held: "up");
            Passing(from, "standing", P.LandingAimDiagonalUpRightPose, P.LandingAimDiagonalUpLeftPose, held: "diagonal_up");
            Passing(from, "standing", P.LandingAimDiagonalDownRightPose, P.LandingAimDiagonalDownLeftPose, held: "diagonal_down");
            Passing(from, "standing", P.FiringLandingRightPose, P.FiringLandingLeftPose, held: "firing");
        }
        foreach (string spin in new[]
                 {
                     "spin_jumping", "space_jumping", "screw_attacking",
                     "spin_jumping_wall_ready", "space_jumping_wall_ready", "screw_attacking_wall_ready",
                 })
        {
            Passing(spin, "standing", P.SpinLandingRightPose, P.SpinLandingLeftPose);
            Passing(spin, "morph_ball", P.MorphingTransitionRightPose, P.MorphingTransitionLeftPose);
        }

        // A wall jump lands as a spin does.
        Passing("wall_jumping", "standing", P.SpinLandingRightPose, P.SpinLandingLeftPose);
        Passing("wall_jump_spinning", "standing", P.SpinLandingRightPose, P.SpinLandingLeftPose);
        Passing("wall_jump_space", "standing", P.SpinLandingRightPose, P.SpinLandingLeftPose);
        Passing("wall_jump_screw", "standing", P.SpinLandingRightPose, P.SpinLandingLeftPose);

        Passing("standing", "crouching", P.CrouchingTransitionRightPose, P.CrouchingTransitionLeftPose);
        Passing("wall_stopped", "crouching", P.CrouchingTransitionRightPose, P.CrouchingTransitionLeftPose);
        Passing("crouching", "standing", P.StandingTransitionRightPose, P.StandingTransitionLeftPose);
        foreach (string from in new[] { "crouching", "falling", "jumping" })
            Passing(from, "morph_ball", P.MorphingTransitionRightPose, P.MorphingTransitionLeftPose);
        foreach (string to in new[] { "crouching", "falling" })
            Passing("morph_ball", to, P.UnmorphingTransitionRightPose, P.UnmorphingTransitionLeftPose);
    }

    /// <summary>
    /// Her death: its pose (a ball's picture and then the upright one, which holds), the pictures of her suit breaking
    /// up, which are on a sheet of their own and each name the step of her death's colours they are shown in, and
    /// how the death is played.
    /// </summary>
    private void WriteSamusDeath(JsonObject timelines)
    {
        string sheet = SpriteSheetDefinitions.FileNames[SpriteSheetId.SamusDeath];
        SamusDeathSteps deathSteps = SamusBody.DeathSteps;
        IReadOnlyList<IAnimationBeat<SamusDeathExplosionFrame, NoAnimationCue>> explosion =
            [.. SamusDeathAnimations.ExplosionSegments.Beats(SamusDeathExplosionSegment.Explosion)];
        // Cut once, in their sheet's own colours, as her body's are: her colours are changed where they are drawn.
        EngineSheetPictures pictures = _sheets.CutNamed(
            _tree, SpriteFamily, SamusDeathPlace, sheet, _sheets.FramesOf(SpriteFamily, sheet), shade: null, EngineSheets.FileName, regionMaps: true);
        foreach ((string side, P pose, HorizontalDirection facing) in new[]
            { ("right", P.DeathSequenceRightPose, HorizontalDirection.Right), ("left", P.DeathSequenceLeftPose, HorizontalDirection.Left) })
        {
            // The pose: the picture a ball begins it on, then the upright one, which every other death begins on and
            // which holds. Every picture of the pose lasts as long as the others.
            timelines.Add($"dying_{side}", SamusTimeline(
                [
                    SamusStepWritten(new SamusStep(pose, SamusDeathSequenceDefinitions.BallStart, SamusDeathSequenceDefinitions.InitialFrameHold)),
                    SamusStepWritten(new SamusStep(pose, SamusDeathSequenceDefinitions.UprightStart, SamusDeathSequenceDefinitions.InitialFrameHold)),
                ],
                loopsTo: 1));
            var steps = new List<JsonObject>();
            foreach (IAnimationBeat<SamusDeathExplosionFrame, NoAnimationCue> beat in explosion)
            {
                if (beat is not AnimationBeat<SamusDeathExplosionFrame, NoAnimationCue>.Frame shown)
                    throw new InvalidDataException($"The breaking up of Samus's suit has a beat this does not know: {beat.GetType().Name}.");
                string path = pictures.Of(SamusDeathSheetDefinitions.ExplosionFrame(facing, shown.Shown));
                steps.Add(new JsonObject
                {
                    ["picture"] = path,
                    ["updates"] = shown.Duration,

                    // These pictures lie on a canvas of their own; the step puts them where that canvas lies.
                    ["offset"] = new JsonObject { ["x"] = pictures.OffsetX - SamusPictures.OffsetX, ["y"] = pictures.OffsetY - SamusPictures.OffsetY },

                    // The step of her death's colours the cartridge's table shows the picture in.
                    ["cues"] = new JsonArray($"shade:{deathSteps.ExplosionShade(shown.Shown).Step}"),
                });
            }

            timelines.Add($"death_explosion_{side}", SamusTimeline(steps, null));
        }

        var flash = new JsonArray();
        foreach (IAnimationBeat<SamusDeathShade, NoAnimationCue> beat in SamusDeathAnimations.FlashSegments.Beats(SamusDeathFlashSegment.Flash))
        {
            if (beat is not AnimationBeat<SamusDeathShade, NoAnimationCue>.Frame shown)
                throw new InvalidDataException($"The flash of Samus's death has a beat this does not know: {beat.GetType().Name}.");
            flash.Add(new JsonObject { ["shade"] = shown.Shown.Step, ["updates"] = shown.Duration });
        }

        var whiteout = new JsonArray();
        foreach (SamusWhiteoutShade shade in SamusWhiteoutShade.All)
            whiteout.Add(Colour(deathSteps.WhiteoutColour(shade)));
        _tree.WriteData("samus/death.json", new JsonObject
        {
            ["fade_parts"] = SuperMetroid.Core.Frontend.SamusDeathDefinitions.FadeParts,
            ["music_set"] = MusicSetName(SuperMetroid.Core.Audio.MusicDataSet.Death),
            ["music"] = MusicName(SuperMetroid.Core.Frontend.SamusDeathDefinitions.FanfareTrack),
            ["music_wait"] = SuperMetroid.Core.Frontend.SamusDeathDefinitions.FanfareDelay.Updates,
            ["pose_first_updates"] = SamusDeathSequenceDefinitions.InitialFrameHold,
            ["before_flash_updates"] = SamusDeathSequenceDefinitions.PreFlashFrameCount,
            ["flash_updates"] = SamusDeathSequenceDefinitions.FlashFrameCount,
            ["flash_first_wait"] = SamusDeathSequenceDefinitions.FirstFlashShadeUpdates,
            ["flash"] = flash,
            ["flash_last_shade"] = SamusDeathShade.First.Step,
            ["whiteout"] = whiteout,
            ["whiteout_resting"] = SamusWhiteoutShade.Brightest.Step,
            ["whiteout_last"] = SamusWhiteoutShade.Final.Step,

            // The old last black-out waits one update before each level; it is written as a number there.
            ["final_fade_wait"] = 1,
        });
    }

    /// <summary>The place of the grapple beam's own pictures.</summary>
    private const string SamusGrapplePlace = "samus/grapple";

    /// <summary>
    /// The grapple beam's own pictures and how they are shown: the line's, a set for each way it can lie and be
    /// mirrored with the set each angle step shows, in the order its pattern steps through them; its end's; how long
    /// each shows; how many pixels of the line each of the line's pictures stands for; and where a picture's corner
    /// lies from its middle.
    /// </summary>
    private JsonObject SamusGrappleLine()
    {
        string sheet = SpriteSheetDefinitions.FileNames[SpriteSheetId.Grapple];
        EngineSheetPictures pictures = _sheets.CutNamed(
            _tree, SpriteFamily, SamusGrapplePlace, sheet, _sheets.FramesOf(SpriteFamily, sheet), shade: null, EngineSheets.FileName, regionMaps: true);

        static AnimationBeat<T, NoAnimationCue>.Frame Shown<T>(IAnimationBeat<T, NoAnimationCue> beat)
            where T : struct => beat as AnimationBeat<T, NoAnimationCue>.Frame
                ?? throw new InvalidDataException($"The grapple beam's pictures have a beat this does not know: {beat.GetType().Name}.");
        var rope = GrappleAnimations.RopeSegments.Beats(GrappleRopeSegment.Travelling).Select(Shown).ToList();
        var end = GrappleAnimations.EndpointSegments.Beats(GrappleEndpointSegment.Cycling).Select(Shown).ToList();
        if (rope.Select(beat => beat.Duration).Distinct().Count() != 1 || end.Select(beat => beat.Duration).Distinct().Count() != 1)
            throw new InvalidDataException("The grapple beam's pictures do not each show for the same time, which its data gives as one number.");

        // The sets of the line's pictures, and the set each angle step shows: by the way it lies and how it is mirrored there.
        var sets = new JsonArray();
        var setOfAngle = new JsonArray();
        var known = new Dictionary<(GrappleRopeOrientation, SpriteMirror), int>();
        for (int step = 0; step < RotationAngle.StepsPerTurn; step++)
        {
            RotationAngle angle = RotationAngle.FromSteps(step);
            (GrappleRopeOrientation Lies, SpriteMirror Mirrored) key = (GrappleRopeOrientations.For(angle), SamusGrappleBeamDrawing.RopeMirror(angle));
            if (!known.TryGetValue(key, out int set))
            {
                set = known.Count;
                known.Add(key, set);
                var paths = new JsonArray();
                foreach (AnimationBeat<GrappleRopePicture, NoAnimationCue>.Frame beat in rope)
                    paths.Add(pictures.Of(GrappleSheets.RopeFrame(key.Lies, beat.Shown, key.Mirrored)));
                sets.Add(paths);
            }

            setOfAngle.Add(set);
        }

        var ends = new JsonArray();
        foreach (AnimationBeat<GrappleEndpointFrame, NoAnimationCue>.Frame beat in end)
            ends.Add(pictures.Of(GrappleSheets.EndpointFrame(beat.Shown)));
        return new JsonObject
        {
            ["spacing"] = GrappleBeamDefinitions.Rendering.SegmentSpacing,
            ["picture_updates"] = rope[0].Duration,
            ["first_picture_updates"] = GrappleBeamDefinitions.Rendering.FirstSegmentAnimationDelay,
            ["rope"] = sets,
            ["rope_of_angle"] = setOfAngle,
            ["end"] = ends,
            ["end_updates"] = end[0].Duration,
            ["offset"] = new JsonObject { ["x"] = pictures.OffsetX, ["y"] = pictures.OffsetY },
        };
    }

    /// <summary>The data file of the timeline of one of the puffs and splashes Samus raises.</summary>
    internal static string SamusParticleFile(SamusParticleKind kind) => $"samus/effects/{EngineSheets.FileName(kind.ToString())}.json";

    /// <summary>Writes the puffs and splashes Samus raises: her landing's dust, and what she throws up crossing a liquid's surface.</summary>
    private void WriteSamusParticles()
    {
        string sheet = SpriteSheetDefinitions.FileNames[SpriteSheetId.SamusAtmosphere];
        foreach ((SamusParticleKind kind, Animation<SamusParticleKind> animation) in SamusParticleAnimations.Animations)
        {
            (List<IAnimationBeat<SamusAtmosphereFrame, NoAnimationCue>> lead, List<IAnimationBeat<SamusAtmosphereFrame, NoAnimationCue>>? cycle) =
                EngineTimelines.Played(SamusParticleAnimations.Segments, animation);
            if (cycle is not null)
                throw new InvalidDataException($"Samus's {kind} never ends, which an effect left in a room must.");
            string file = SamusParticleFile(kind);
            IEnumerable<string> frames = lead.Select(beat => beat.ShownFrame).Where(frame => frame.HasValue)
                .Select(frame => SamusAtmosphereSheetDefinitions.FrameName(frame!.Value)).Distinct(StringComparer.Ordinal);
            EngineSheetPictures pictures = _sheets.CutNamed(_tree, SpriteFamily, file[..file.LastIndexOf('.')], sheet, frames, null, EngineSheets.FileName);
            JsonObject timeline = EngineTimelines.Of<SamusAtmosphereFrame, NoAnimationCue>(
                lead, null, frame => pictures.Of(SamusAtmosphereSheetDefinitions.FrameName(frame)), static _ => null);
            timeline["offset"] = new JsonObject { ["x"] = pictures.OffsetX, ["y"] = pictures.OffsetY };

            // One that drifts the same way wherever it is raised, as her dust rises, moves on each update it is shown.
            // Lava spray drifts to the side it is thrown, which is not known here.
            if (SteadyParticles.SingleOrDefault(steady => steady.Kind == kind) is { } drifting && drifting.Drift != default)
                timeline["steps"] = Drifted(kind, timeline, drifting.Drift.X, drifting.Drift.Y);
            _tree.WriteData(file, timeline);
        }
    }

    /// <summary>Samus's particles that move the same way, or not at all, wherever they are raised.</summary>
    private static readonly SamusSteadyParticle[] SteadyParticles =
    [
        SamusParticleKindDefinitions.SurfaceSplash, SamusParticleKindDefinitions.DivingSplash, SamusParticleKindDefinitions.Bubbles,
        SamusParticleKindDefinitions.Dust,
    ];

    /// <summary>
    /// The steps of a particle's timeline that moves by (<paramref name="x"/>, <paramref name="y"/>) on each update it is
    /// shown, its first among them, as the cartridge moves it before it draws it: each update a step of its own, placed so
    /// much further than the one before.
    /// </summary>
    /// <exception cref="InvalidDataException">The timeline loops or rests, or a step shows nothing or raises a cue.</exception>
    private static JsonArray Drifted(SamusParticleKind kind, JsonObject timeline, int x, int y)
    {
        if (timeline["loops_to"] is not null)
            throw new InvalidDataException($"Samus's {kind} comes round to a step, which splitting its steps by update would not keep.");
        var drifted = new JsonArray();
        int shown = 0;
        foreach (JsonNode? step in timeline["steps"]!.AsArray())
        {
            string picture = step!["picture"]!.GetValue<string>();
            if (picture.Length == 0 || step["cues"] is not null || step["offset"] is not null)
                throw new InvalidDataException($"A step of Samus's {kind} shows nothing, raises a cue or has a place of its own, which splitting it by update would not keep.");
            for (int update = 0; update < step["updates"]!.GetValue<int>(); update++)
            {
                shown++;
                drifted.Add(new JsonObject
                {
                    ["picture"] = picture,
                    ["updates"] = 1,
                    ["offset"] = new JsonObject { ["x"] = x * shown, ["y"] = y * shown },
                });
            }
        }

        return drifted;
    }
}
