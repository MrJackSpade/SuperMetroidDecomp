using Engine.Actors;
using Engine.Actors.Movement;
using Engine.Content;
using Engine.Shared;
using Engine.World;
using Godot;

namespace Game.Enemies;

/// <summary>
/// A rock lobbed by a Polyp: it rises, slowing, from the place on the speed curve the Polyp gave it, and falls back
/// faster and faster, drifting across all the while. Its settings carry it across and let it through terrain.
/// </summary>
public partial class PolypRock : Projectile, IHasValues
{
    /// <inheritdoc/>
    [Export(PropertyHint.File)]
    public string DataFile { get; set; } = "";

    /// <summary>How many marks of the curve it moves along each update.</summary>
    [Export, DataValue("curve_marks")]
    public int CurveMarks { get; set; } = 2;

    /// <summary>The mark of the curve its fall speeds up to and stays at.</summary>
    [Export, DataValue("fastest_fall_mark")]
    public int FastestFallMark { get; set; } = 64;

    /// <summary>The data file of the speed curve it rises and falls by.</summary>
    [Export, DataValue("speed_curve")]
    public string SpeedCurveFile { get; set; } = "";

    private SpeedCurve? _curve;
    private int _place;
    private bool _falling;
    private Pixels _across;
    private bool _acrossHeld;

    /// <inheritdoc/>
    public void ReadValues(DataNode data)
    {
    }

    /// <inheritdoc/>
    public override void _ValidateProperty(Godot.Collections.Dictionary property) => DataValues.Validate(this, property);

    /// <summary>Sets the mark of the curve its rise starts from: the further on, the higher it goes.</summary>
    public void StartAt(int mark) => _place = mark * SpeedCurve.PartsPerMark;

    /// <inheritdoc/>
    protected override void OnLaunched(Room room)
    {
        DataValues.Fill(this, room.Content);
        _curve = SpeedCurve.Read(room.Content.ReadData(SpeedCurveFile));
    }

    /// <inheritdoc/>
    protected override void BeforeTravel(Room room)
    {
        SpeedCurve curve = _curve ?? throw new InvalidOperationException("The Polyp rock is not in a room.");
        int step = CurveMarks * SpeedCurve.PartsPerMark;
        // Each update it moves by two neighbouring speeds of the curve, one after the other.
        if (!_falling)
        {
            _place -= step;
            if (_place < 0)
            {
                // At the top of its arc it hangs for one update, not even drifting.
                _place = 0;
                _falling = true;
                _across = Velocity.X;
                Velocity = Velocity.WithX(Pixels.Zero);
                _acrossHeld = true;
                return;
            }

            Position -= new PixelOffset(Pixels.Zero, curve.SpeedAt(_place + SpeedCurve.PartsPerMark) + curve.SpeedAt(_place));
            return;
        }

        if (_acrossHeld)
        {
            Velocity = Velocity.WithX(_across);
            _acrossHeld = false;
        }

        _place = Math.Min(_place + step, FastestFallMark * SpeedCurve.PartsPerMark);
        Position += new PixelOffset(Pixels.Zero, curve.SpeedAt(_place - SpeedCurve.PartsPerMark) + curve.SpeedAt(_place));
    }
}
