using System.Globalization;
using Engine.Actors;
using Engine.Collision;
using Engine.Content;
using Engine.Shared;
using Godot;

namespace Game.Enemies;

/// <summary>
/// A walking Space Pirate: it patrols a stretch of floor, turning at walls, ledges and the ends of its beat. When
/// Samus is level with it, it stops and fires three lasers at her, high, middle and low. Some flinch when a shot comes
/// near.
/// </summary>
/// <remarks>
/// Its timelines are "walking_left", "walking_right", "looking_left", "looking_right", "fire_lasers_left",
/// "fire_lasers_right", "flinch_left" and "flinch_right". Their cues are "walk:&lt;left|right&gt;", which sets it
/// walking that way; "still", which stops it; "fire:&lt;left|right&gt;:&lt;height&gt;", which fires a laser that far
/// above its middle; and "choose", which has it fire again if Samus is still level, or else walk on. Its shot is
/// "laser".
/// </remarks>
public partial class WalkingPirate : SpacePirate
{
    /// <summary>How far to either side of where it is placed its beat reaches, in pixels.</summary>
    [Export]
    public int PatrolReach { get; set; } = 64;

    /// <summary>How fast its lasers fly, in pixels an update.</summary>
    [Export]
    public int LaserSpeed { get; set; } = 4;

    /// <summary>Whether it flinches when one of Samus's shots comes near.</summary>
    [Export]
    public bool FlinchesFromShots { get; set; }

    /// <summary>How far it walks each update to the left, as a number of pixels.</summary>
    [Export, DataValue("walk_speed_left")]
    public double WalkSpeedLeft { get; set; } = 0.2187652587890625;

    /// <summary>How far it walks each update to the right, as a number of pixels.</summary>
    [Export, DataValue("walk_speed_right")]
    public double WalkSpeedRight { get; set; } = 0.21875;

    /// <summary>How near Samus's height must be to its own, in pixels, for it to fire.</summary>
    [Export, DataValue("firing_band")]
    public int FiringBand { get; set; } = 16;

    /// <summary>How near a shot must come, in pixels across and down, to make it flinch.</summary>
    [Export, DataValue("flinch_reach")]
    public int FlinchReach { get; set; } = 32;

    /// <summary>How far ahead of itself it feels for floor walking left, in pixels.</summary>
    [Export, DataValue("ledge_reach_left")]
    public int LedgeReachLeft { get; set; } = 17;

    /// <summary>How far ahead of itself it feels for floor walking right, in pixels.</summary>
    [Export, DataValue("ledge_reach_right")]
    public int LedgeReachRight { get; set; } = 16;

    /// <summary>How far in front of its middle its lasers leave from, in pixels.</summary>
    [Export, DataValue("muzzle_ahead")]
    public int MuzzleAhead { get; set; } = 24;

    private bool _walking;
    private int _leftPost;
    private int _rightPost;

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        _leftPost = Position.X.Whole - PatrolReach;
        _rightPost = Position.X.Whole + PatrolReach;
        Play(Facing == Facing.Right ? "walking_right" : "walking_left");
    }

    /// <inheritdoc/>
    protected override void OnCue(string cue)
    {
        if (TakePose(cue))
            return;
        string[] parts = cue.Split(':');
        switch (parts[0])
        {
            case "still":
                _walking = false;
                break;
            case "walk":
                _walking = true;
                Facing = parts[1] == "left" ? Facing.Left : Facing.Right;
                break;
            case "fire":
                Facing way = parts[1] == "left" ? Facing.Left : Facing.Right;
                int height = int.Parse(parts[2], CultureInfo.InvariantCulture);
                PixelPosition muzzle = PixelPosition.FromWhole(Position.X.Whole + (way == Facing.Left ? -MuzzleAhead : MuzzleAhead), Position.Y.Whole - height);
                Fire("laser", muzzle, new PixelVelocity(way.Toward(Pixels.FromWhole(LaserSpeed)), Pixels.Zero));
                break;
            case "choose":
                bool samusRight = Target.X.Whole >= Position.X.Whole;
                if (IsLevelWithSamus)
                    Play(samusRight ? "fire_lasers_right" : "fire_lasers_left", restart: true);
                else
                    Play(samusRight ? "walking_left" : "walking_right", restart: true);
                break;
        }
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        if (_walking)
            Walk();
        if (!FlinchesFromShots)
            return;
        // It watches one thing: what holds the highest of her shots' places that holds anything, a shot or what a
        // shot left, and nothing else when that is far off.
        if (((Samus)Room.Player).HighestShotStands(flyingOnly: false) is not { } shot
            || Math.Abs(shot.X.Whole - Position.X.Whole) >= FlinchReach || Math.Abs(shot.Y.Whole - Position.Y.Whole) >= FlinchReach)
        {
            return;
        }

        Play(Position.X.Whole < Target.X.Whole ? "flinch_right" : "flinch_left", restart: true);
    }

    /// <summary>Whether Samus is near enough its own height to be fired on.</summary>
    private bool IsLevelWithSamus => Math.Abs(Target.Y.Whole - Position.Y.Whole) < FiringBand;

    /// <summary>Walks one update the way it faces; it stops to fire, and stops to look about where its beat ends.</summary>
    private void Walk()
    {
        if (IsLevelWithSamus)
        {
            Play(Target.X.Whole >= Position.X.Whole ? "fire_lasers_right" : "fire_lasers_left", restart: true);
            return;
        }

        // It settles a pixel toward the floor each update, and walks only once it stands on it.
        if (!Drop(Pixels.FromWhole(1)))
            return;

        // Left and right are not mirrors in the old game: each way has its own reach for a ledge and its own step. It
        // feels for the floor ahead by moving a pixel down where it would stand there, and back across: with no floor
        // there, it keeps that pixel, as on the cartridge.
        bool left = Facing == Facing.Left;
        Pixels here = Position.X;
        Position = new PixelPosition(here + Pixels.FromWhole(left ? -LedgeReachLeft : LedgeReachRight), Position.Y);
        bool floorAhead = Drop(Pixels.FromWhole(1));
        Position = new PixelPosition(here, Position.Y);
        if (floorAhead && !Stride(left ? -Pixels.FromNumber(WalkSpeedLeft) : Pixels.FromNumber(WalkSpeedRight)))
        {
            bool withinBeat = left ? Position.X.Whole >= _leftPost : Position.X.Whole < _rightPost;
            if (withinBeat)
                return;
        }

        Play(left ? "looking_left" : "looking_right", restart: true);
    }

    /// <summary>
    /// How its moves are tried, as the cartridge moves an enemy: only where each would end, and sloped ground that holds
    /// things up by their middle met under its middle alone.
    /// </summary>
    private static readonly MoveOptions Moves =
        MoveOptions.AmongBlocks with { Tries = WayTried.WhereItEnds, Slopes = SlopeTreatment.ClimbUnderMiddle, RestsUnderSlopes = true };

    /// <summary>How a move across is tried: as its other moves, and such sloped ground is no part of it.</summary>
    private static readonly MoveOptions MovesAcross = Moves with { Slopes = SlopeTreatment.IgnoreMiddleHeld };

    /// <summary>Moves it down by <paramref name="distance"/>; returns whether something stopped it.</summary>
    private bool Drop(Pixels distance) => Room.Mover.MoveDown(MovingBody(), distance, Moves).Blocked;

    /// <summary>Moves it across by <paramref name="distance"/>, sloped ground no part of the move; returns whether a wall stopped it.</summary>
    private bool Stride(Pixels distance) => Room.Mover.MoveAcross(MovingBody(), distance, MovesAcross).Blocked;

    /// <summary>The collider it moves by.</summary>
    private Collider MovingBody() => Body ?? throw new InvalidOperationException($"The walking Space Pirate \"{Name}\" has no body collider.");
}
