using System.Globalization;
using Engine.Actors;
using Engine.Collision;
using Engine.Content;
using Engine.Shared;
using Engine.Ui;
using Engine.World;
using Godot;

namespace Game.Enemies;

/// <summary>
/// A Work Robot of the Wrecked Ship. With the power off it stands dead, a solid thing to climb on. With the power on
/// it tramps back and forth, turning at walls and ledges, and fires lasers level, up and down as it goes. It cannot be
/// destroyed, but a shot makes it flinch and holds its fire for a while, and Samus can ride on its head.
/// </summary>
/// <remarks>
/// Everything it does is written in its timelines, as cues. "step:&lt;pixels&gt;:&lt;left|right&gt;:&lt;timeline at a
/// wall&gt;" takes a step facing that way, and with ":&lt;left|right&gt;:&lt;timeline at a ledge&gt;:&lt;left|right&gt;"
/// added also feels for floor on that side and, finding none, plays that timeline facing the last way.
/// "laser:&lt;up|level|down&gt;:&lt;timeline&gt;" fires if it may and then plays that timeline. "footstep" sounds its
/// step and "cool" counts its cooldown down. It starts on "standing_left"; a shot plays "shot_left_ahead",
/// "shot_left_behind", "shot_right_ahead" or "shot_right_behind". Its shots are "laser_level" and "laser_diagonal".
/// One without power is read from a file of its own, which holds only the timelines of the poses it stands in and
/// no glow, and stands in its pose wherever it is placed, doing nothing else.
/// </remarks>
public partial class WorkRobot : Enemy
{
    /// <summary>Whether the ship's power is on, so that it walks.</summary>
    [Export]
    public bool Powered { get; set; } = true;

    /// <summary>For one without power, the timeline of the pose it stands in.</summary>
    [Export]
    public string Pose { get; set; } = "no_power_neutral";

    /// <summary>The downward speed a powered one gains each update in the air, as a number of pixels.</summary>
    [Export, DataValue("gravity")]
    public double Gravity { get; set; } = 0.5;

    /// <summary>The fewest updates between lasers; by chance up to 31 more.</summary>
    [Export, DataValue("laser_cooldown")]
    public int LaserCooldown { get; set; } = 16;

    /// <summary>How many updates turning round adds to its cooldown.</summary>
    [Export, DataValue("turn_delay")]
    public int TurnDelay { get; set; } = 8;

    /// <summary>How many updates being shot adds to its cooldown.</summary>
    [Export, DataValue("recoil_delay")]
    public int RecoilDelay { get; set; } = 64;

    /// <summary>How far in front of its middle its lasers leave from, in pixels.</summary>
    [Export, DataValue("muzzle_ahead")]
    public int MuzzleAhead { get; set; } = 4;

    /// <summary>How far above its middle its lasers leave from, in pixels.</summary>
    [Export, DataValue("muzzle_above")]
    public int MuzzleAbove { get; set; } = 16;

    /// <summary>How fast a slanting laser rises or falls, as a number of pixels an update.</summary>
    [Export, DataValue("laser_slant_speed")]
    public double LaserSlantSpeed { get; set; } = 0.5;

    /// <summary>The sound of its step, by name, heard only from where <see cref="FootstepIsHeard"/> says.</summary>
    [Export, DataValue("footstep_sound")]
    public string FootstepSound { get; set; } = "";

    /// <summary>The sound of its laser, by name, heard only from where <see cref="LaserIsHeard"/> says.</summary>
    [Export, DataValue("laser_sound")]
    public string LaserSound { get; set; } = "";

    private int _cooldown;
    private Pixels _fallSpeed;

    /// <summary>The steps of its glow, as its data gives them.</summary>
    private DataNode? _glowSteps;

    /// <summary>The glow a powered robot shows on itself; none for one without power.</summary>
    private ColourSteps? _glow;

    /// <inheritdoc/>
    public override void ReadValues(DataNode data)
    {
        base.ReadValues(data);
        // The file of one without power, which has no glow, gives none.
        _glowSteps = data.Has("glow") ? data.Get("glow") : null;
    }

    /// <summary>Set up, a powered robot begins its glow; one without power shows none.</summary>
    protected override void OnEnterRoom()
    {
        Facing = Facing.Left;
        Play(Powered ? "standing_left" : Pose);
        _glow = null;
        if (!Powered || Visual is not { } visual)
            return;
        _glow = new ColourSteps(_glowSteps ?? throw new InvalidOperationException($"The Work Robot \"{Name}\" has no data for its glow."), "work_robot_glow");
        _glow.ShowOn(visual);
        _glow.Restart();
    }

    /// <summary>Its glow goes on while a hurt holds its behaviour off.</summary>
    protected override void WhileHurt() => _glow?.Step();

    /// <inheritdoc/>
    /// <remarks>One without power does nothing on its turn: it never moves, not even to fall.</remarks>
    protected override void Behave()
    {
        if (!Powered)
            return;
        _glow?.Step();

        // In the air its timeline waits, and it falls faster; landed, it keeps the speed it landed with.
        bool landed = Room.Mover.MoveDown(MovingBody(), _fallSpeed, Moves).Blocked;
        HoldsTimeline = !landed;
        if (!landed)
            _fallSpeed += Pixels.FromNumber(Gravity);
    }

    /// <inheritdoc/>
    protected override void OnHurt(Hit hit, Collider part) => Flinch();

    /// <inheritdoc/>
    protected override void OnAbsorbed(Hit hit, Collider part) => Flinch();

    /// <inheritdoc/>
    protected override void OnCue(string cue)
    {
        if (!Powered)
            return;
        string[] parts = cue.Split(':');
        switch (parts[0])
        {
            case "footstep":
                if (FootstepIsHeard())
                    Sound(FootstepSound);
                break;
            case "cool":
                Cool();
                break;
            case "step":
                Step(parts);
                break;
            case "laser":
                if (_cooldown != 0)
                {
                    Cool();
                    break;
                }

                // How long until the next is read from the number last drawn.
                _cooldown = Random.Last.Part(0, 5) + LaserCooldown;
                Pixels slant = Pixels.FromNumber(LaserSlantSpeed);
                Pixels down = parts[1] == "up" ? -slant : parts[1] == "down" ? slant : Pixels.Zero;
                string shot = parts[1] == "level" ? "laser_level" : "laser_diagonal";
                PixelPosition muzzle = PixelPosition.FromWhole(Position.X.Whole + (Facing == Facing.Left ? -MuzzleAhead : MuzzleAhead), Position.Y.Whole - MuzzleAbove);
                ShotType type = Shot(shot);
                Projectile laser = type.Launch(muzzle, new PixelVelocity(Facing.Toward(type.Speed), down), HitSide.Enemies, this, this);
                if (Room.Spawn(laser) && LaserIsHeard())
                    Sound(LaserSound);
                Play(parts[2], restart: true);
                break;
        }
    }

    /// <summary>
    /// Whether a laser it fires is heard. The cartridge asks for the sound only when its body reaches into the view's
    /// columns, its bottom reaches the view's top or below, and its top lies less than a view's height below the top
    /// of the room, wherever the view stands down the room: one lower in its room fires silently even in view.
    /// </summary>
    private bool LaserIsHeard()
    {
        PixelRect body = (Body ?? throw new InvalidOperationException($"The Work Robot \"{Name}\" has no body.")).RoomRect;
        Camera camera = Room.Session.Camera;
        return body.Right - camera.Left >= 0 &&
            body.Left - (camera.Width + 1) - camera.Left < 0 &&
            body.Bottom - camera.Top >= 0 &&
            body.Top - camera.Height < 0;
    }

    /// <summary>
    /// Whether its step is heard. The cartridge asks for it only when its place lies past the view's left and top edges
    /// and no further than a view's width and height beyond them: a pixel to the right of and below the view itself.
    /// </summary>
    private bool FootstepIsHeard()
    {
        Camera camera = Room.Session.Camera;
        int x = Position.X.Whole, y = Position.Y.Whole;
        return x > camera.Left && x <= camera.Left + camera.Width && y > camera.Top && y <= camera.Top + camera.Height;
    }

    /// <summary>Counts its cooldown down by one.</summary>
    private void Cool()
    {
        if (_cooldown > 0)
            _cooldown--;
    }

    /// <summary>Takes the step a cue describes: a wall or a ledge turns it round.</summary>
    private void Step(string[] parts)
    {
        Cool();
        int pixels = int.Parse(parts[1], CultureInfo.InvariantCulture);
        Facing = parts[2] == "left" ? Facing.Left : Facing.Right;
        bool ridden = IsRiddenByPlayer(Riding.FeetAbove);
        if (Room.Mover.MoveAcross(MovingBody(), Pixels.FromWhole(pixels), MovesAcross).Blocked)
        {
            _cooldown += TurnDelay;
            Play(parts[3], restart: true);
            return;
        }

        if (ridden)
            ShiftPlayer(PixelOffset.FromWhole(pixels, 0));
        if (parts.Length < 7 || FeelsFloor(parts[4] == "left" ? Facing.Left : Facing.Right))
            return;
        _cooldown += TurnDelay;
        Facing = parts[6] == "left" ? Facing.Left : Facing.Right;
        Play(parts[5], restart: true);
    }

    /// <summary>Flinches from a shot, toward or away from Samus, and holds its fire a while longer.</summary>
    private void Flinch()
    {
        if (!Powered)
            return;
        bool ahead = Facing == Facing.Left ? Target.X.Whole < Position.X.Whole : Target.X.Whole >= Position.X.Whole;
        Play($"shot_{(Facing == Facing.Left ? "left" : "right")}_{(ahead ? "ahead" : "behind")}", restart: true);
        _cooldown += RecoilDelay;
    }

    /// <summary>
    /// Whether there is floor a body's width to one side of it, felt as the cartridge feels it: by moving a pixel down
    /// from there. It is left at its own whole pixels, with the part of a pixel that move left it at.
    /// </summary>
    private bool FeelsFloor(Facing side)
    {
        Collider body = MovingBody();
        PixelPosition standing = Position;
        Position = new PixelPosition(standing.X + side.Toward(Pixels.FromWhole(2 * (body.Size.X / 2))), standing.Y);
        bool floor = Room.Mover.MoveDown(body, Pixels.FromWhole(1), Moves).Blocked;
        Position = new PixelPosition(standing.X, Position.Y.WithWhole(standing.Y.Whole));
        return floor;
    }

    /// <summary>
    /// How its moves are tried, as the cartridge moves an enemy: only where each would end, and sloped ground that holds
    /// things up by their middle met under its middle alone.
    /// </summary>
    private static readonly MoveOptions Moves =
        MoveOptions.AmongBlocks with { Tries = WayTried.WhereItEnds, Slopes = SlopeTreatment.ClimbUnderMiddle, RestsUnderSlopes = true };

    /// <summary>How a step across is tried: as its other moves, and such sloped ground is no part of it.</summary>
    private static readonly MoveOptions MovesAcross = Moves with { Slopes = SlopeTreatment.IgnoreMiddleHeld };

    /// <summary>The collider it moves by.</summary>
    private Collider MovingBody() => Body ?? throw new InvalidOperationException($"The Work Robot \"{Name}\" has no body collider.");
}
