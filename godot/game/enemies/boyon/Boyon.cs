using Engine.Actors;
using Engine.Shared;
using Godot;

namespace Game.Enemies;

/// <summary>
/// A Boyon: a blob that rests until Samus comes near across, and then bounces straight up and down on the spot for
/// as long as she stays near, always finishing the bounce it is in. It moves by whole pixels only.
/// </summary>
/// <remarks>
/// Its bounce follows a curve of its own: the speed at each step from the top of the arc is the sum of the whole
/// numbers up to that step, in 256ths of a pixel, times its multiplier, and never more than 255 256ths times it. Its
/// timelines are "idle" and "bouncing".
/// </remarks>
public partial class Boyon : Enemy
{
    /// <summary>What its bounce curve is multiplied by.</summary>
    [Export]
    public int SpeedMultiplier { get; set; } = 1;

    /// <summary>How high its bounce carries it, in pixels.</summary>
    [Export]
    public int JumpHeight { get; set; } = 32;

    /// <summary>How near across Samus must come, in pixels, for it to bounce.</summary>
    [Export]
    public int Reach { get; set; } = 80;

    /// <summary>How many steps the curve has before its speed stops growing.</summary>
    private const int CurveSteps = 23;

    private enum Doing
    {
        FindingBaseline,
        Resting,
        Rising,
        Falling,
        Landed,
    }

    private Doing _doing;

    /// <summary>How many steps down from the top of its arc it is; -1 once it has risen past the top.</summary>
    private int _step;

    /// <summary>The step at which a fall has brought it back to where it rests.</summary>
    private int _baseline;

    /// <inheritdoc/>
    protected override void OnEnterRoom() => Play("idle");

    /// <inheritdoc/>
    protected override void Behave()
    {
        // Its first update only works out where on the curve its bounce begins and ends: it neither moves nor looks
        // for Samus.
        if (_doing == Doing.FindingBaseline)
        {
            Pixels fallen = Pixels.Zero;
            _step = 0;
            do
            {
                fallen += SpeedAt(_step);
                _step++;
            }
            while (fallen < Pixels.FromWhole(JumpHeight));
            _baseline = _step;
            _doing = Doing.Resting;
            return;
        }

        if (_doing is Doing.Resting or Doing.Landed)
        {
            if (Math.Abs(Target.X.Whole - Position.X.Whole) >= Reach)
            {
                if (_doing == Doing.Landed)
                {
                    _doing = Doing.Resting;
                    Play("idle");
                }

                return;
            }

            _doing = Doing.Rising;
            Play("bouncing", restart: true);
        }

        if (_doing == Doing.Rising)
        {
            Position -= new PixelOffset(Pixels.Zero, Pixels.FromWhole(SpeedAt(_step).Whole));
            _step--;
            if (_step < 0)
                _doing = Doing.Falling;
            return;
        }

        _step++;
        Position += new PixelOffset(Pixels.Zero, Pixels.FromWhole(SpeedAt(_step).Whole));
        if (_step >= _baseline)
            _doing = Doing.Landed;
    }

    /// <summary>The speed of the bounce at a step down from the top of its arc.</summary>
    private Pixels SpeedAt(int step)
    {
        int parts = step < CurveSteps ? step * (step + 1) / 2 : 255;
        return Pixels.FromParts(0, parts * 256) * SpeedMultiplier;
    }
}
