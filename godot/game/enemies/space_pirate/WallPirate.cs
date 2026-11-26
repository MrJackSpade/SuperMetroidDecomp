using System.Globalization;
using Engine.Actors;
using Engine.Content;
using Engine.Shared;
using Game.Compatibility;
using Godot;

namespace Game.Enemies;

/// <summary>
/// A wall Space Pirate: it clings to one wall of a shaft, climbing up and down by chance. When Samus is level with it,
/// it fires a laser across at her and leaps in an arc to the wall opposite.
/// </summary>
/// <remarks>
/// Its timelines are "moving_up_left_wall", "moving_down_left_wall", "moving_up_right_wall" and
/// "moving_down_right_wall", "fire_and_jump_left" and "fire_and_jump_right", and "landed_on_left_wall" and
/// "landed_on_right_wall". Their cues are "on_wall:&lt;left|right&gt;", which sets it climbing that wall;
/// "climb:&lt;down&gt;", which climbs that many pixels and turns it back where its way is blocked; "choose_climb",
/// which picks up or down by chance; "attacking", which stops it; "fire:&lt;left|right&gt;", which fires across;
/// "prepare_jump:&lt;left|right&gt;", which sets up its leap; and "jump:&lt;left|right&gt;", which launches it toward
/// that wall. Its shot is "laser".
/// </remarks>
public partial class WallPirate : Enemy
{
    /// <summary>The wall it starts on.</summary>
    [Export]
    public Facing Wall { get; set; } = Facing.Left;

    /// <summary>How fast its lasers fly, in pixels an update.</summary>
    [Export]
    public int LaserSpeed { get; set; } = 4;

    /// <summary>Whether its leap is the slower of the two.</summary>
    [Export]
    public bool SlowJump { get; set; }

    /// <summary>How far across its leap carries it, in pixels: the width of its shaft.</summary>
    [Export]
    public int JumpSpan { get; set; } = 96;

    /// <summary>How near Samus's height must be to its own, in pixels, for it to attack.</summary>
    [Export, DataValue("detection_band")]
    public int DetectionBand { get; set; } = 32;

    /// <summary>How far above its middle its laser leaves from, in pixels.</summary>
    [Export, DataValue("muzzle_above")]
    public int MuzzleAbove { get; set; } = 16;

    /// <summary>How far in front of its middle its laser leaves from, in pixels.</summary>
    [Export, DataValue("muzzle_ahead")]
    public int MuzzleAhead { get; set; } = 24;

    /// <summary>How many angle steps its fast leap turns each update.</summary>
    [Export, DataValue("fast_jump_turn")]
    public int FastJumpTurn { get; set; } = 4;

    /// <summary>How many angle steps its slow leap turns each update.</summary>
    [Export, DataValue("slow_jump_turn")]
    public int SlowJumpTurn { get; set; } = 2;

    /// <summary>How many angle steps short of the far wall its slow leap ends.</summary>
    [Export, DataValue("slow_jump_short_by")]
    public int SlowJumpShortBy { get; set; } = 2;

    /// <summary>The width of a block, in pixels; it hangs half a block out from the wall.</summary>
    [Export, DataValue("block_size")]
    public int BlockSize { get; set; } = 16;

    private enum Doing
    {
        Climbing,
        Attacking,
        Leaping,
    }

    private Doing _doing;
    private bool _climbsUp;
    private (int X, int Y) _arcCentre;
    private int _arcAngle;

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        Play(Wall == Facing.Right ? "moving_down_right_wall" : "moving_down_left_wall");
        SnapToWall();
    }

    /// <inheritdoc/>
    protected override void OnCue(string cue)
    {
        string[] parts = cue.Split(':');
        Facing side = parts.Length > 1 && parts[1] == "right" ? Facing.Right : Facing.Left;
        switch (parts[0])
        {
            case "on_wall":
                _doing = Doing.Climbing;
                Wall = side;
                break;
            case "climb":
                if (MoveDownBy(Pixels.FromWhole(int.Parse(parts[1], CultureInfo.InvariantCulture))))
                {
                    _climbsUp = !_climbsUp;
                    Play(ClimbTimeline, restart: true);
                }

                break;
            case "choose_climb":
                _climbsUp = Random.Draw().Part(0, 1) == 1;
                Play(ClimbTimeline, restart: true);
                break;
            case "attacking":
                _doing = Doing.Attacking;
                break;
            case "fire":
                PixelPosition muzzle = PixelPosition.FromWhole(Position.X.Whole + (side == Facing.Left ? -MuzzleAhead : MuzzleAhead), Position.Y.Whole - MuzzleAbove);
                Fire("laser", muzzle, new PixelVelocity(side.Toward(Pixels.FromWhole(LaserSpeed)), Pixels.Zero));
                break;
            case "prepare_jump":
                _arcCentre = (Position.X.Whole + (side == Facing.Left ? -JumpSpan / 2 : JumpSpan / 2), Position.Y.Whole);
                _arcAngle = side == Facing.Right ? Turns.PerTurn / 4 : 3 * Turns.PerTurn / 4;
                break;
            case "jump":
                _doing = Doing.Leaping;
                Wall = side;
                break;
        }
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        if (_doing == Doing.Climbing)
        {
            if (Math.Abs(Target.Y.Whole - Position.Y.Whole) < DetectionBand)
                Play(Wall == Facing.Left ? "fire_and_jump_right" : "fire_and_jump_left", restart: true);
            return;
        }

        if (_doing != Doing.Leaping)
            return;
        // The leap is half an oval, from one wall up and over to the other, its points the cartridge's short products.
        Position = PixelPosition.FromWhole(
            _arcCentre.X + CartridgeAngles.CircleSine(_arcAngle + Turns.PerTurn / 2, JumpSpan / 2).Whole,
            _arcCentre.Y - CartridgeAngles.CircleCosine(_arcAngle, JumpSpan / 4).Whole);
        int turn = SlowJump ? SlowJumpTurn : FastJumpTurn;
        int shortBy = SlowJump ? SlowJumpShortBy : 0;
        bool right = Wall == Facing.Right;
        _arcAngle = Turns.Wrap(_arcAngle + (right ? -turn : turn));
        if (_arcAngle != (right ? 3 * Turns.PerTurn / 4 - shortBy : Turns.PerTurn / 4 + shortBy))
            return;
        Play(right ? "landed_on_right_wall" : "landed_on_left_wall", restart: true);
        SnapToWall();
    }

    /// <summary>The name of its climbing timeline for its wall and the way it climbs.</summary>
    private string ClimbTimeline => $"moving_{(_climbsUp ? "up" : "down")}_{(Wall == Facing.Left ? "left" : "right")}_wall";

    /// <summary>
    /// Puts it against the wall it has reached: less than eleven pixels into its block it goes back to the half
    /// block it is in, the block's start or its middle, and further in it goes on to the next block's start.
    /// </summary>
    private void SnapToWall()
    {
        int x = Position.X.Whole;
        int within = ((x % BlockSize) + BlockSize) % BlockSize;
        int start = x - within;
        int half = BlockSize / 2;
        Position = new PixelPosition(Position.X.WithWhole(within < BlockSize * 11 / 16 ? start + within / half * half : start + BlockSize), Position.Y);
    }
}
