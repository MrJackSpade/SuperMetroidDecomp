using Engine.Content;
using Engine.Shared;

namespace Game;

/// <summary>
/// The numbers of the grapple beam's swing, as the cartridge has them. Speeds of turning are in 256ths of an angle
/// step an update, a step being a 256th of a turn; lengths are in pixels.
/// </summary>
/// <remarks>
/// Read from the group "swing" of the grapple's values in Samus's data file:
/// <code>
/// "swing": {
///   "gravity": 24, "push": 12, "correction": 5, "most_turning": 1152, "jump_kick": 768, "kick_decay": 6,
///   "still_kick": 256, "resting_turning": 256,
///   "submerged_share": 2, "submerged_turn_share": 5, "submerged_turn_parts": 8, "upper_share": 4,
///   "bounce_updates": 16, "length_step": 2, "shortest": 8, "longest_let_out": 63, "length_cap": 64,
///   "shortening_probe": 8, "lengthening_probe": 56, "body_point_spacing": 8, "body_points": 6, "stopping_points": 2,
///   "anchor_toward_positive": 8, "anchor_toward_negative": 7,
///   "release_lead": 64, "release_lead_per_step": 3, "retraction": -8, "long_line": 64, "long_line_shortening": 24,
///   "fast_turning": 64, "wall_jump_window": 30, "wall_jump_probe": 16,
///   "stops": [ { "angle": 54912, "kind": "held", "facing": "right", "x": -30, "y": -24 } ],
///   "body_left": [ { "x": 0, "y": -15 } ], "body_right": [ { "x": 0, "y": -14 } ]
/// }
/// </code>
/// "body_left" and "body_right" give, for each of the ways the line can run from her counted round from straight
/// up, where her place lies from the end of the line she holds, facing left and facing right. "stops" lists the
/// exact angles, in 65536ths of a turn, at which a swing stopped by her hand on the shortest line leaves her in a
/// pose of its own: held crouched ("held") or against a wall ("wall"), facing as it says, at the place given from
/// what holds the line.
/// </remarks>
public sealed class GrappleSwingRules
{
    private GrappleSwingRules(DataNode data)
    {
        Gravity = data.Int("gravity");
        Push = data.Int("push");
        Correction = data.Int("correction");
        MostTurning = data.Int("most_turning");
        JumpKick = data.Int("jump_kick");
        KickDecay = data.Int("kick_decay");
        StillKick = data.Int("still_kick");
        RestingTurning = data.Int("resting_turning");
        SubmergedShare = data.Int("submerged_share");
        SubmergedTurnShare = data.Int("submerged_turn_share");
        SubmergedTurnParts = data.Int("submerged_turn_parts");
        UpperShare = data.Int("upper_share");
        BounceUpdates = data.Int("bounce_updates");
        LengthStep = data.Int("length_step");
        Shortest = data.Int("shortest");
        LongestLetOut = data.Int("longest_let_out");
        LengthCap = data.Int("length_cap");
        ShorteningProbe = data.Int("shortening_probe");
        LengtheningProbe = data.Int("lengthening_probe");
        BodyPointSpacing = data.Int("body_point_spacing");
        BodyPoints = data.Int("body_points");
        StoppingPoints = data.Int("stopping_points");
        AnchorTowardPositive = data.Int("anchor_toward_positive");
        AnchorTowardNegative = data.Int("anchor_toward_negative");
        ReleaseLead = data.Int("release_lead");
        ReleaseLeadPerStep = data.Int("release_lead_per_step");
        Retraction = data.Int("retraction");
        LongLine = data.Int("long_line");
        LongLineShortening = data.Int("long_line_shortening");
        FastTurning = data.Int("fast_turning");
        WallJumpWindow = data.Int("wall_jump_window");
        WallJumpProbe = data.Int("wall_jump_probe");
        Stops =
        [
            .. data.List("stops").Select(stop => new Stop(
                stop.Int("angle"),
                stop.Text("kind") switch
                {
                    "held" => false,
                    "wall" => true,
                    string kind => throw new ContentException(data.File, $"gives a stop of the swing the kind \"{kind}\", which is neither held nor wall"),
                },
                stop.Text("facing") == "left",
                PixelOffset.FromWhole(stop.Int("x"), stop.Int("y")))),
        ];
        BodyLeft = [.. data.List("body_left").Select(place => PixelOffset.FromWhole(place.Int("x"), place.Int("y")))];
        BodyRight = [.. data.List("body_right").Select(place => PixelOffset.FromWhole(place.Int("x"), place.Int("y")))];
        if (BodyLeft.Count == 0 || BodyLeft.Count != BodyRight.Count || Turns.PerTurn % BodyLeft.Count != 0)
            throw new ContentException(data.File, "gives the swing's body places for a number of ways that does not divide the turn, or not the same number facing each way");
    }

    /// <summary>Reads the swing's numbers.</summary>
    public static GrappleSwingRules Read(DataNode data)
    {
        ArgumentNullException.ThrowIfNull(data);
        return new GrappleSwingRules(data);
    }

    /// <summary>What swings her back toward hanging straight down each update, in the lower half of the turn.</summary>
    public int Gravity { get; }

    /// <summary>What a held direction adds to the swing each update.</summary>
    public int Push { get; }

    /// <summary>What is added besides when the swing turns against the side she hangs on.</summary>
    public int Correction { get; }

    /// <summary>The fastest the swing turns, either way.</summary>
    public int MostTurning { get; }

    /// <summary>The kick a fresh press of the jump gives the swing while a bounce off the ground lasts.</summary>
    public int JumpKick { get; }

    /// <summary>How much of that kick each update that meets nothing takes away.</summary>
    public int KickDecay { get; }

    /// <summary>The turning a held direction kicks a still swing hanging straight down to.</summary>
    public int StillKick { get; }

    /// <summary>A swing hanging straight down that turns slower than this stops.</summary>
    public int RestingTurning { get; }

    /// <summary>The share of gravity, of the push and of the jump's kick a swing under a liquid takes: one part in so many.</summary>
    public int SubmergedShare { get; }

    /// <summary>The parts of its turning, out of <see cref="SubmergedTurnParts"/>, a swing under a liquid turns through each update.</summary>
    public int SubmergedTurnShare { get; }

    /// <summary>The parts a swing's turning under a liquid divides into.</summary>
    public int SubmergedTurnParts { get; }

    /// <summary>The share of gravity and of the correction the swing takes above the level of what holds it: one part in so many.</summary>
    public int UpperShare { get; }

    /// <summary>The updates after a bounce off the ground in which the jump adds its kick.</summary>
    public int BounceUpdates { get; }

    /// <summary>How far a press of up or down draws the line in or lets it out each update.</summary>
    public int LengthStep { get; }

    /// <summary>The shortest the line is drawn in to.</summary>
    public int Shortest { get; }

    /// <summary>The longest down lets the line out to.</summary>
    public int LongestLetOut { get; }

    /// <summary>Down cuts a line this long or longer to this length instead of letting it out.</summary>
    public int LengthCap { get; }

    /// <summary>How far beyond the line's end the point a shortening line tests lies: her hand's end.</summary>
    public int ShorteningProbe { get; }

    /// <summary>How far beyond the line's end the point a lengthening line tests lies: her feet's end.</summary>
    public int LengtheningProbe { get; }

    /// <summary>How far apart the points along her body a swing tests are, the first this far beyond her hand.</summary>
    public int BodyPointSpacing { get; }

    /// <summary>How many points along her body a swing tests, from her hand toward her feet.</summary>
    public int BodyPoints { get; }

    /// <summary>The points nearest her hand whose strike, on the shortest line, stops the swing instead of bouncing it.</summary>
    public int StoppingPoints { get; }

    /// <summary>How far into its block what holds the line sits on an axis the line does not run back along.</summary>
    public int AnchorTowardPositive { get; }

    /// <summary>How far into its block what holds the line sits on an axis the line runs back along.</summary>
    public int AnchorTowardNegative { get; }

    /// <summary>The angle steps a still release's speed across leads the line by.</summary>
    public int ReleaseLead { get; }

    /// <summary>The angle steps that lead loses for each whole step of the swing's turning.</summary>
    public int ReleaseLeadPerStep { get; }

    /// <summary>How the line's length changes each update from the moment it takes hold: it draws itself in.</summary>
    public int Retraction { get; }

    /// <summary>A line this long or longer as it takes hold is shortened by <see cref="LongLineShortening"/>.</summary>
    public int LongLine { get; }

    /// <summary>How much a long line is shortened as it takes hold.</summary>
    public int LongLineShortening { get; }

    /// <summary>While the swing turns this fast or faster, either way, the view follows her slowly.</summary>
    public int FastTurning { get; }

    /// <summary>The updates after a wall held by the line is let go in which the jump still jumps off it.</summary>
    public int WallJumpWindow { get; }

    /// <summary>How far, in pixels, a wall is felt for on the side she faces in that window.</summary>
    public int WallJumpProbe { get; }

    /// <summary>The angles at which a swing stopped by her hand leaves her in a pose of its own.</summary>
    public IReadOnlyList<Stop> Stops { get; }

    /// <summary>One such angle: the line's angle in parts of a step, whether she is left against a wall or held crouched, the way she faces, and her place from what holds the line.</summary>
    public sealed record Stop(int Angle, bool GrabsWall, bool FacesLeft, PixelOffset Place);

    /// <summary>Where her place lies from the end of the line she holds, facing left, for each way the line can run.</summary>
    public IReadOnlyList<PixelOffset> BodyLeft { get; }

    /// <summary>Where her place lies from the end of the line she holds, facing right, for each way the line can run.</summary>
    public IReadOnlyList<PixelOffset> BodyRight { get; }
}
