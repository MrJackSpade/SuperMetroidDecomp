using Engine.Actors;
using Engine.Collision;
using Engine.Content;
using Engine.Shared;
using Engine.World;
using Godot;

namespace Game.Enemies;

/// <summary>
/// One of Phantoon's flames. Phantoon tells each what to do: ring it at the start of the fight and close in; fall
/// from its mouth, bounce and burn out; spiral or fan out from it; or hang in a row and rain down. Its settings say
/// whether shots put it out and what it may leave.
/// </summary>
/// <remarks>
/// Its data names the timelines it changes to: "falling_timeline", "landing_timeline", "burning_timeline",
/// "resting_timeline" and "rain_impact_timeline". Its settings let it through terrain, which it meets itself.
/// </remarks>
public partial class PhantoonFlame : Projectile, IHasValues
{
    /// <inheritdoc/>
    [Export(PropertyHint.File)]
    public string DataFile { get; set; } = "";

    /// <summary>The downward speed it gains each update falling, as a number of pixels.</summary>
    [Export, DataValue("gravity")]
    public double Gravity { get; set; } = 0.0625;

    /// <summary>The radius of the ring the opening flames keep, in pixels.</summary>
    [Export, DataValue("ring_radius")]
    public int RingRadius { get; set; } = 48;

    /// <summary>How many updates the opening ring keeps its size before it closes in.</summary>
    [Export, DataValue("ring_hold_updates")]
    public int RingHoldUpdates { get; set; } = 181;

    /// <summary>How far a landed flame sinks into the floor, in pixels.</summary>
    [Export, DataValue("landing_drop")]
    public int LandingDrop { get; set; } = 8;

    /// <summary>How many updates a dropped flame rests where it lands before it leaps up.</summary>
    [Export, DataValue("landing_pause")]
    public int LandingPause { get; set; } = 8;

    /// <summary>The upward speed a dropped flame leaps with, and the speed of its next two bounces, in pixels an update.</summary>
    [Export, DataValue("leap_speed")]
    public int LeapSpeed { get; set; } = 3;

    /// <summary>How fast a leaping flame drifts across, as a number of pixels an update.</summary>
    [Export, DataValue("drift_speed")]
    public double DriftSpeed { get; set; } = 0.5;

    /// <summary>The timeline of a flame falling from Phantoon's mouth.</summary>
    [Export, DataValue("falling_timeline")]
    public string FallingTimeline { get; set; } = "";

    /// <summary>The timeline of a dropped flame hitting the ground.</summary>
    [Export, DataValue("landing_timeline")]
    public string LandingTimeline { get; set; } = "";

    /// <summary>The timeline of a flame burning.</summary>
    [Export, DataValue("burning_timeline")]
    public string BurningTimeline { get; set; } = "";

    /// <summary>The timeline of a flame burning out where it came to rest.</summary>
    [Export, DataValue("resting_timeline")]
    public string RestingTimeline { get; set; } = "";

    /// <summary>The timeline of a rained flame bursting on the ground.</summary>
    [Export, DataValue("rain_impact_timeline")]
    public string RainImpactTimeline { get; set; } = "";

    /// <summary>The sound of a rained flame starting to fall, and landing, by name.</summary>
    [Export, DataValue("rain_sound")]
    public string RainSound { get; set; } = "";

    private enum Doing
    {
        Nothing,
        RingWaiting,
        RingClosing,
        Falling,
        Landed,
        Bouncing,
        Raging,
        Spiralling,
        Hanging,
        Raining,
        Still,
    }

    private Doing _doing;
    private Phantoon? _phantoon;
    private int _angle;
    private int _turn;
    private int _radius;
    private int _wait;
    private int _bounces;

    /// <inheritdoc/>
    public void ReadValues(DataNode data)
    {
    }

    /// <inheritdoc/>
    public override void _ValidateProperty(Godot.Collections.Dictionary property) => DataValues.Validate(this, property);

    /// <summary>Sets it in the ring about Phantoon at the start of the fight, at an angle round it.</summary>
    public void OrbitAtStart(Phantoon phantoon, int angle)
    {
        (_phantoon, _angle, _radius, _doing) = (phantoon, angle, RingRadius, Doing.RingWaiting);
        PlaceOnRing();
    }

    /// <summary>Sets it spiralling out from Phantoon, starting at an angle round it.</summary>
    public void Spiral(Phantoon phantoon, int angle) => (_phantoon, _angle, _turn, _radius, _doing) = (phantoon, angle, 2, 0, Doing.Spiralling);

    /// <summary>Sets it flying out from Phantoon in its rage, starting at an angle and turning as it goes.</summary>
    public void Rage(Phantoon phantoon, int angle, int turn) => (_phantoon, _angle, _turn, _radius, _doing) = (phantoon, angle, turn, 0, Doing.Raging);

    /// <summary>Lets it fall from Phantoon's mouth; it cannot hurt until it has landed.</summary>
    public void Fall()
    {
        _doing = Doing.Falling;
        PlayTimeline(FallingTimeline);
    }

    /// <summary>Hangs it in the air for some updates, and then lets it fall.</summary>
    public void Rain(int hangUpdates) => (_wait, _doing) = (hangUpdates, Doing.Hanging);

    /// <inheritdoc/>
    public override bool IsHitBy(HitSide side) => _doing != Doing.Falling && base.IsHitBy(side);

    /// <inheritdoc/>
    protected override bool DeliversHitNow => _doing != Doing.Falling && base.DeliversHitNow;

    /// <inheritdoc/>
    protected override void OnLaunched(Room room) => DataValues.Fill(this, room.Content);

    /// <inheritdoc/>
    protected override void OnCue(Room room, string cue)
    {
        if (cue == "end")
            Leave(room);
    }

    /// <inheritdoc/>
    protected override void BeforeTravel(Room room)
    {
        Pixels gravity = Pixels.FromNumber(Gravity);
        switch (_doing)
        {
            case Doing.RingWaiting:
                if (_phantoon is { StartingFlamesClose: true })
                {
                    _doing = Doing.RingClosing;
                    _wait = RingHoldUpdates;
                }

                break;

            case Doing.RingClosing:
                // Once its hold is over the ring draws in a pixel every other update, and at nothing the flame is gone.
                if (_wait > 0)
                    _wait--;
                if (_wait == 0 && room.Session.Clock.Updates % 2 == 1 && --_radius <= 0)
                {
                    Leave(room);
                    return;
                }

                _angle++;
                PlaceOnRing();
                break;

            case Doing.Spiralling or Doing.Raging:
                _radius += _doing == Doing.Raging ? 4 : 2;
                _angle += _turn;
                PlaceOnRing();
                if (Position.X.Whole is < 0 or >= 256 || Position.Y.Whole is < 0 or >= 256)
                    Leave(room);
                break;

            case Doing.Falling:
                Velocity = Velocity.WithY(Velocity.Y + gravity);
                if (!MeetsGround(room))
                    break;
                _doing = Doing.Landed;
                _wait = LandingPause;
                Velocity = PixelVelocity.Zero;
                Position += PixelOffset.FromWhole(0, LandingDrop);
                PlayTimeline(LandingTimeline);
                break;

            case Doing.Landed:
                if (--_wait > 0)
                    break;
                _doing = Doing.Bouncing;
                PlayTimeline(BurningTimeline);
                Position -= PixelOffset.FromWhole(0, LandingDrop);
                Pixels drift = Pixels.FromNumber(DriftSpeed);
                Velocity = new PixelVelocity(room.Session.Clock.Updates % 2 == 1 ? -drift : drift, Pixels.FromWhole(-LeapSpeed));
                break;

            case Doing.Bouncing:
                Velocity = Velocity.WithY(Velocity.Y + gravity);
                if (MeetsGround(room))
                {
                    // Each bounce is lower than the last, and after the third it burns out where it lies.
                    if (++_bounces < 3)
                        Velocity = Velocity.WithY(Pixels.FromWhole(_bounces == 1 ? -(LeapSpeed - 1) : -(LeapSpeed - 2)));
                    else
                        Rest();
                }
                else if (room.Terrain.IsSolidIn(Area.MovedBy(Velocity.X.IsNegative ? -1 : 1, 0), SlopeTreatment.Wall))
                {
                    Rest();
                }

                break;

            case Doing.Hanging:
                if (--_wait > 0)
                    break;
                _doing = Doing.Raining;
                room.Session.Sounds.Request(RainSound, channel: "enemies");
                break;

            case Doing.Raining:
                Velocity = Velocity.WithY(Velocity.Y + gravity);
                if (!MeetsGround(room))
                    break;
                _doing = Doing.Still;
                Velocity = PixelVelocity.Zero;
                Position += PixelOffset.FromWhole(0, LandingDrop);
                PlayTimeline(RainImpactTimeline);
                room.Session.Sounds.Request(RainSound, channel: "enemies");
                break;
        }
    }

    /// <summary>Whether its fall this update would carry it into the ground.</summary>
    private bool MeetsGround(Room room) =>
        !Velocity.Y.IsNegative && room.Terrain.IsSolidIn(Area.MovedBy(0, Velocity.Y.Whole + 1), SlopeTreatment.Wall);

    /// <summary>Leaves it burning out where it lies.</summary>
    private void Rest()
    {
        _doing = Doing.Still;
        Velocity = PixelVelocity.Zero;
        PlayTimeline(RestingTimeline);
    }

    /// <summary>Puts it on its circle about Phantoon: straight up at nothing, turning clockwise.</summary>
    private void PlaceOnRing()
    {
        if (_phantoon is not { } phantoon || !IsInstanceValid(phantoon))
            return;
        Pixels radius = Pixels.FromWhole(_radius);
        Position = phantoon.FlameCentre + PixelOffset.FromWhole(Turns.Sine(_angle, radius).Whole, Turns.Sine(_angle + 3 * Turns.PerTurn / 4, radius).Whole);
    }
}
