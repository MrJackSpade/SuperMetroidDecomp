using System.Globalization;
using Engine.Actors;
using Engine.Content;
using Engine.Shared;
using Godot;

namespace Game.Enemies;

/// <summary>
/// A ninja Space Pirate: it holds one of two posts at either end of a room. Roused, it throws its claws at Samus,
/// kicks her if she comes close, somersaults to its other post when she stands between them, and from there dives
/// at her with a kick and runs back. A shot coming near makes it flinch out of the way.
/// </summary>
/// <remarks>
/// Its timelines, each ending "_left" or "_right", are "awaiting", "active", "flinch", "kick", "claw_attack",
/// "spin_jump", "divekick_jump", "divekick_dive", "walk_to_post" and "land". Their cues are
/// "state:&lt;still|awaiting|active|ready&gt;", which sets what it is doing; "state:&lt;spin_rising|dive_jump|dive|
/// walk_to_post&gt;:&lt;left|right&gt;", which sets it moving that way; "start_active", which turns it to face Samus;
/// "reset_spin" and "start_dive", which ready a somersault and a dive; and "claw:&lt;left|right&gt;:&lt;across&gt;:
/// &lt;down&gt;", which throws a claw from that far off its middle. Its shot is "claw".
/// </remarks>
public partial class NinjaPirate : SpacePirate
{
    /// <summary>Whether the post it starts at is the left one.</summary>
    [Export]
    public bool StartsAtLeftPost { get; set; }

    /// <summary>How far apart its two posts are, in pixels.</summary>
    [Export]
    public int PostDistance { get; set; } = 128;

    /// <summary>How near across Samus must come, in pixels, to rouse it.</summary>
    [Export, DataValue("activation_distance")]
    public int ActivationDistance { get; set; } = 128;

    /// <summary>How near a shot must come, in pixels across and down, to make it flinch.</summary>
    [Export, DataValue("flinch_distance")]
    public int FlinchDistance { get; set; } = 32;

    /// <summary>How near the middle between its posts Samus must be, in pixels, for it to somersault or dive.</summary>
    [Export, DataValue("midpoint_distance")]
    public int MidpointDistance { get; set; } = 32;

    /// <summary>How near Samus must be, in pixels across and down, for it to kick.</summary>
    [Export, DataValue("kick_distance")]
    public int KickDistance { get; set; } = 40;

    /// <summary>The speed across its somersault gains, then loses, each update, as a number of pixels.</summary>
    [Export, DataValue("spin_speed_change")]
    public double SpinSpeedChange { get; set; } = 0.125;

    /// <summary>How far its somersault climbs, then falls, each update, in pixels.</summary>
    [Export, DataValue("spin_climb_step")]
    public int SpinClimbStep { get; set; } = 2;

    /// <summary>The speed its leap into a dive starts with, and the dive itself, in pixels an update.</summary>
    [Export, DataValue("dive_start_speed")]
    public int DiveStartSpeed { get; set; } = 6;

    /// <summary>The speed its leap, and its dive, lose each update, as a number of pixels.</summary>
    [Export, DataValue("dive_speed_change")]
    public double DiveSpeedChange { get; set; } = 0.25;

    /// <summary>How far across its dive carries it each update, in pixels.</summary>
    [Export, DataValue("dive_step")]
    public int DiveStep { get; set; } = 5;

    /// <summary>How far it runs each update on its way back to a post, in pixels.</summary>
    [Export, DataValue("return_step")]
    public int ReturnStep { get; set; } = 2;

    /// <summary>How many updates pass between its chances to throw its claws.</summary>
    [Export, DataValue("claw_attack_every")]
    public int ClawAttackEvery { get; set; } = 64;

    /// <summary>The effect of the dust it kicks up landing, by the data file of its timeline.</summary>
    [Export, DataValue("landing_dust_effect")]
    public string LandingDustEffect { get; set; } = "";

    /// <summary>How far to either side of it the dust rises, in pixels.</summary>
    [Export, DataValue("landing_dust_spread")]
    public int LandingDustSpread { get; set; } = 8;

    /// <summary>How far below its middle the dust rises from, in pixels.</summary>
    [Export, DataValue("landing_dust_drop")]
    public int LandingDustDrop { get; set; } = 28;

    /// <summary>How many more drops it scatters about itself as it is destroyed, beside its own; the gold one scatters them.</summary>
    [Export, DataValue("scattered_drops")]
    public int ScatteredDrops { get; set; }

    /// <inheritdoc/>
    protected override void OnDied()
    {
        // Each is placed by a fresh random number, as far as sixteen pixels off either way.
        for (int drop = 0; drop < ScatteredDrops; drop++)
        {
            RandomDraw draw = Random.Draw();
            Drop(Drops, PixelPosition.FromWhole(Position.X.Whole, Position.Y.Whole) + PixelOffset.FromWhole(draw.Part(0, 5) - 16, draw.Part(8, 5) - 16));
        }
    }

    private enum Doing
    {
        Still,
        Awaiting,
        Active,
        SpinRising,
        SpinFalling,
        Ready,
        DiveJump,
        Dive,
        WalkToPost,
    }

    private Doing _doing;
    private Facing _heading;
    private int _leftPost;
    private int _rightPost;
    private int _midpoint;
    private int _floorY;
    private Pixels _spinSpeed;
    private Pixels _diveSpeed;

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        Play(StartsAtLeftPost ? "awaiting_right" : "awaiting_left");
        int left = StartsAtLeftPost ? Position.X.Whole : Position.X.Whole - PostDistance;
        _midpoint = left + PostDistance / 2;
        // Its posts are moved to where a somersault, gaining speed to the middle, really carries it.
        Pixels wanted = Pixels.FromWhole(PostDistance / 2), speed = Pixels.Zero, covered = Pixels.Zero, change = Pixels.FromNumber(SpinSpeedChange);
        do
        {
            speed += change;
            covered += speed;
        }
        while (covered < wanted);
        _rightPost = _midpoint + covered.Whole;
        _leftPost = _midpoint - covered.Whole;
        Position = PixelPosition.FromWhole(StartsAtLeftPost ? _leftPost : _rightPost, Position.Y.Whole);
        _floorY = Position.Y.Whole;
    }

    /// <inheritdoc/>
    protected override void OnCue(string cue)
    {
        if (TakePose(cue))
            return;
        string[] parts = cue.Split(':');
        switch (parts[0])
        {
            case "state":
                _doing = parts[1] switch
                {
                    "still" => Doing.Still,
                    "awaiting" => Doing.Awaiting,
                    "active" => Doing.Active,
                    "ready" => Doing.Ready,
                    "spin_rising" => Doing.SpinRising,
                    "dive_jump" => Doing.DiveJump,
                    "dive" => Doing.Dive,
                    "walk_to_post" => Doing.WalkToPost,
                    _ => throw new InvalidOperationException($"The ninja pirate \"{Name}\" has no state named \"{parts[1]}\"."),
                };
                if (parts.Length > 2)
                    _heading = parts[2] == "left" ? Facing.Left : Facing.Right;
                break;
            case "start_active":
                Play(Sided("active", TowardPlayer), restart: true);
                break;
            case "reset_spin":
                _spinSpeed = Pixels.Zero;
                break;
            case "start_dive":
                _diveSpeed = Pixels.FromWhole(DiveStartSpeed);
                break;
            case "claw":
                Facing thrown = parts[1] == "left" ? Facing.Left : Facing.Right;
                PixelPosition from = PixelPosition.FromWhole(Position.X.Whole, Position.Y.Whole)
                    + PixelOffset.FromWhole(int.Parse(parts[2], CultureInfo.InvariantCulture), int.Parse(parts[3], CultureInfo.InvariantCulture));
                Fire("claw", from, new PixelVelocity(thrown.Toward(Shot("claw").Speed), Pixels.Zero));
                break;
        }
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        Pixels spinChange = Pixels.FromNumber(SpinSpeedChange), diveChange = Pixels.FromNumber(DiveSpeedChange);
        bool right = _heading == Facing.Right;
        switch (_doing)
        {
            case Doing.Still:
                return;

            case Doing.Awaiting:
                if (Math.Abs(Position.X.Whole - Target.X.Whole) >= ActivationDistance)
                    Flinches();
                else
                    Play(Sided("active", TowardPlayer), restart: true);
                return;

            case Doing.Active:
                if (Flinches() || Kicks())
                    return;
                if (Math.Abs(_midpoint - Target.X.Whole) < MidpointDistance)
                {
                    Play(Position.X.Whole == _leftPost ? "spin_jump_right" : "spin_jump_left", restart: true);
                    return;
                }

                // Every so often it throws its claws, if Samus is on the side its post faces.
                if (Updates % ClawAttackEvery != 0)
                    return;
                if (Position.X.Whole == _leftPost)
                {
                    if (Position.X.Whole >= Target.X.Whole)
                        Play("claw_attack_left", restart: true);
                }
                else if (Position.X.Whole < Target.X.Whole)
                {
                    Play("claw_attack_right", restart: true);
                }

                return;

            case Doing.SpinRising:
                Position += PixelOffset.FromWhole(right ? _spinSpeed.Whole : -_spinSpeed.Whole, -SpinClimbStep);
                _spinSpeed += spinChange;
                if (right ? Position.X.Whole >= _midpoint : Position.X.Whole < _midpoint)
                    _doing = Doing.SpinFalling;
                return;

            case Doing.SpinFalling:
                Position += PixelOffset.FromWhole(right ? _spinSpeed.Whole : -_spinSpeed.Whole, SpinClimbStep);
                _spinSpeed -= spinChange;
                if (!_spinSpeed.IsZero)
                    return;
                _doing = Doing.Still;
                Position = new PixelPosition(Position.X.WithWhole(right ? _rightPost : _leftPost), Position.Y);
                Play(Sided("land", _heading), restart: true);
                LeaveDust();
                return;

            case Doing.Ready:
                if (Flinches() || Kicks() || Math.Abs(_midpoint - Target.X.Whole) >= MidpointDistance)
                    return;
                // It draws until a number lets it go: one time in four it draws again.
                while (Random.Draw().Part(0, 2) == 0)
                {
                }

                Play(Position.X.Whole == _leftPost ? "divekick_jump_right" : "divekick_jump_left", restart: true);
                return;

            case Doing.DiveJump:
                Position -= PixelOffset.FromWhole(0, _diveSpeed.Whole);
                _diveSpeed -= diveChange;
                if (!_diveSpeed.IsNegative)
                    return;
                _doing = Doing.Dive;
                Play(Sided("divekick_dive", _heading), restart: true);
                _diveSpeed = Pixels.FromWhole(DiveStartSpeed);
                return;

            case Doing.Dive:
                Position += PixelOffset.FromWhole(right ? DiveStep : -DiveStep, 0);
                // The cartridge falls by the speed's whole pixels and counts its 256ths of a pixel only as 65536ths.
                bool landed = MoveDownBy(Pixels.FromParts(_diveSpeed.Whole, _diveSpeed.Fraction / 256));
                if (!landed)
                {
                    _diveSpeed -= diveChange;
                    landed = _diveSpeed.IsNegative || _diveSpeed.Whole == 0;
                }

                if (!landed)
                    return;
                _doing = Doing.WalkToPost;
                Play(Sided("walk_to_post", _heading), restart: true);
                Position = new PixelPosition(Position.X, Position.Y.WithWhole(_floorY));
                LeaveDust();
                return;

            case Doing.WalkToPost:
                Position += PixelOffset.FromWhole(right ? ReturnStep : -ReturnStep, 0);
                if (right ? Position.X.Whole < _rightPost : Position.X.Whole >= _leftPost)
                    return;
                Position = new PixelPosition(Position.X.WithWhole(right ? _rightPost : _leftPost), Position.Y);
                Play(Sided("land", _heading), restart: true);
                _doing = Doing.Still;
                return;

            default:
                throw new InvalidOperationException($"Undefined ninja pirate stage {_doing}.");
        }
    }

    /// <summary>The name of one of its timelines for a side.</summary>
    private static string Sided(string timeline, Facing side) => side == Facing.Left ? $"{timeline}_left" : $"{timeline}_right";

    /// <summary>
    /// Flinches, facing Samus, if the shot it watches has come near; returns whether it did. It watches one shot: the
    /// one still flying in the highest of her shots' places that holds one, and no other when that one is far off.
    /// </summary>
    private bool Flinches()
    {
        if (((Samus)Room.Player).HighestShotStands(flyingOnly: true) is not { } shot
            || Math.Abs(shot.X.Whole - Position.X.Whole) >= FlinchDistance || Math.Abs(shot.Y.Whole - Position.Y.Whole) >= FlinchDistance)
        {
            return false;
        }

        Play(Sided("flinch", TowardPlayer), restart: true);
        return true;
    }

    /// <summary>Kicks, facing Samus, if she is close; returns whether it did.</summary>
    private bool Kicks()
    {
        if (!PlayerIsWithin(KickDistance, KickDistance))
            return false;
        Play(Sided("kick", TowardPlayer), restart: true);
        return true;
    }

    /// <summary>Leaves a puff of dust to either side of its feet.</summary>
    private void LeaveDust()
    {
        if (LandingDustEffect.Length == 0)
            return;
        LeaveEffect(LandingDustEffect, PixelPosition.FromWhole(Position.X.Whole - LandingDustSpread, Position.Y.Whole + LandingDustDrop));
        LeaveEffect(LandingDustEffect, PixelPosition.FromWhole(Position.X.Whole + LandingDustSpread, Position.Y.Whole + LandingDustDrop));
    }
}
