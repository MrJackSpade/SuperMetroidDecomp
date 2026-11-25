using Engine.Actors;
using Engine.Content;
using Engine.Shared;
using Game.Compatibility;
using Godot;

namespace Game.Enemies;

/// <summary>
/// A fly: it wheels in a figure of eight until Samus comes near across, then dives at where she is, pulls out as it
/// passes her height, climbs away for as long as it dived, and after a short rest may dive again. The Mellow, the
/// Mella and the Maridia fly are all this scene with their own data files. Nothing in the room stops it.
/// </summary>
/// <remarks>Its timeline is "flying".</remarks>
public partial class Fly : Enemy
{
    private const int QuarterTurn = 64, HalfTurn = 128;

    /// <summary>How near across Samus must come, in pixels, for it to dive.</summary>
    [Export, DataValue("reach")]
    public int Reach { get; set; } = 112;

    /// <summary>How far round its wheeling it turns each update, in 256ths of a turn.</summary>
    [Export, DataValue("wheel_step")]
    public int WheelStep { get; set; } = 16;

    /// <summary>How many updates after a dive before it may dive again.</summary>
    [Export, DataValue("rest_updates")]
    public int RestUpdates { get; set; } = 24;

    /// <summary>The most speed across of a dive, in pixels.</summary>
    [Export, DataValue("dive_across_speed")]
    public int DiveAcrossSpeed { get; set; } = 2;

    /// <summary>The most speed up or down of a dive, in pixels.</summary>
    [Export, DataValue("dive_down_speed")]
    public int DiveDownSpeed { get; set; } = 4;

    private enum Doing
    {
        Wheeling,
        Diving,
        Climbing,
    }

    private Doing _doing;
    private int _angle;
    private bool _anticlockwise;
    private Countdown _rest = Countdown.Over;
    private Countdown _climb;
    private int _divedFor;
    private int _targetRow;

    /// <inheritdoc/>
    protected override void OnEnterRoom() => Play("flying");

    /// <inheritdoc/>
    protected override void Behave()
    {
        // Every fly draws a number every update and uses none of them, as the original's do.
        Random.Next();
        switch (_doing)
        {
            case Doing.Wheeling:
                if (_rest.IsRunning)
                {
                    _rest = _rest.Stepped();
                }
                else if (Math.Abs(Position.X.Whole - Target.X.Whole) < Reach)
                {
                    // As the cartridge has it: the aim between whole pixels, counting up from right, and each speed
                    // along it in 256ths of a pixel, across by its cosine and down by its sine a half turn on.
                    int aim = CartridgeAngles.AimAngle(Target.X.Whole - Position.X.Whole, Target.Y.Whole - Position.Y.Whole);
                    Velocity = new PixelVelocity(
                        FromTwoFiftySixths(CartridgeAngles.SpeedAlong(DiveAcrossSpeed * 256, aim + QuarterTurn)),
                        FromTwoFiftySixths(CartridgeAngles.SpeedAlong(DiveDownSpeed * 256, aim + HalfTurn)));
                    _targetRow = Target.Y.Whole;
                    _divedFor = 0;
                    _doing = Doing.Diving;
                    return;
                }

                // Its wheeling turns one way round a circle and then the other way round the next, which draws the eight.
                // A pixel along the sine across and along the sine a quarter turn back down, in 256ths of a pixel.
                Position += new PixelOffset(
                    FromTwoFiftySixths(CartridgeAngles.SpeedAlong(256, _angle)),
                    FromTwoFiftySixths(CartridgeAngles.SpeedAlong(256, _angle - QuarterTurn)));
                _angle = Turns.Wrap(_angle + (_anticlockwise ? -WheelStep : WheelStep));
                if (_angle == 0)
                    _anticlockwise = !_anticlockwise;
                return;

            case Doing.Diving:
                Position += Velocity.OverOneUpdate;
                _divedFor++;
                bool passed = Velocity.Y.IsNegative ? Position.Y.Whole < _targetRow : Position.Y.Whole >= _targetRow;
                if (!passed)
                    return;
                Velocity = Velocity.WithY(-Velocity.Y);
                _climb = Countdown.Of(_divedFor + 1);
                _doing = Doing.Climbing;
                return;

            case Doing.Climbing:
                Position += Velocity.OverOneUpdate;
                _climb = _climb.Stepped();
                if (_climb.IsRunning)
                    return;
                _rest = Countdown.Of(RestUpdates);
                _doing = Doing.Wheeling;
                _anticlockwise = false;
                return;

            default:
                throw new InvalidOperationException($"Undefined fly stage {_doing}.");
        }
    }

    /// <summary>A distance given in 256ths of a pixel.</summary>
    private static Pixels FromTwoFiftySixths(int twoFiftySixths) => Pixels.FromParts(twoFiftySixths >> 8, (twoFiftySixths & 0xFF) << 8);
}
