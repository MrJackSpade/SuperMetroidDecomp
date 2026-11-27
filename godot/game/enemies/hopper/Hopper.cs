using Engine.Actors;
using Engine.Actors.Movement;
using Engine.Content;
using Engine.Shared;
using Godot;

namespace Game.Enemies;

/// <summary>
/// A hopper: it crouches on a floor or hangs from a ceiling, then hops toward Samus, a small hop or a big one by
/// chance, and settles before the next. The Sidehoppers and the Dessgeegas, large and small, are all this scene with
/// their own data files.
/// </summary>
/// <remarks>
/// Its timelines are "landed", whose cue "ready" lets the next hop begin, and "jumping"; one placed on a ceiling uses
/// "landed_ceiling" and "jumping_ceiling".
/// </remarks>
public partial class Hopper : Enemy
{
    /// <summary>Whether it hangs from a ceiling and hops downward.</summary>
    [Export]
    public bool OnCeiling { get; set; }

    /// <summary>The data file of the speed curve its hops follow.</summary>
    [Export, DataValue("speed_curve")]
    public string SpeedCurveFile { get; set; } = "";

    /// <summary>How far it travels across each update of a hop, in pixels.</summary>
    [Export, DataValue("across_speed")]
    public int AcrossSpeed { get; set; } = 3;

    /// <summary>How far from its surface a small hop carries it, in pixels.</summary>
    [Export, DataValue("small_hop_height")]
    public int SmallHopHeight { get; set; } = 16;

    /// <summary>How far along the curve a small hop moves each update, in 256ths of a mark.</summary>
    [Export, DataValue("small_hop_step")]
    public int SmallHopStep { get; set; } = 768;

    /// <summary>How far from its surface a big hop carries it, in pixels.</summary>
    [Export, DataValue("big_hop_height")]
    public int BigHopHeight { get; set; } = 48;

    /// <summary>How far along the curve a big hop moves each update, in 256ths of a mark.</summary>
    [Export, DataValue("big_hop_step")]
    public int BigHopStep { get; set; } = 1024;

    /// <summary>The furthest place along the curve its fall back reaches, in marks.</summary>
    [Export, DataValue("fastest_mark")]
    public int FastestMark { get; set; } = 64;

    private enum Doing
    {
        ChoosingHop,
        Preparing,
        ChoosingDirection,
        Starting,
        Hopping,
        Landed,
        Waiting,
    }

    private readonly Hop _hop = new() { WaitUpdates = 0 };
    private Doing _doing;
    private bool _big;
    private bool _ready;

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        // Each hopper placed starts the random sequence again from the same number and draws one it does not use, as
        // the original's does.
        Random.Restart(37);
        Random.Next();
        _hop.Curve = SpeedCurve.Read(Room.Content.ReadData(SpeedCurveFile));
        _hop.AcrossSpeed = Pixels.FromWhole(AcrossSpeed);
        _hop.FastestPlace = FastestMark * SpeedCurve.PartsPerMark;
        _hop.FromCeiling = OnCeiling;
        Play(OnCeiling ? "landed_ceiling" : "landed");
    }

    /// <inheritdoc/>
    protected override void OnCue(string cue)
    {
        if (cue == "ready")
            _ready = true;
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        // Each step of settling on a hop takes an update of its own.
        switch (_doing)
        {
            case Doing.ChoosingHop:
                _big = Random.Draw().Part(0, 1) == 1;
                _doing = Doing.Preparing;
                return;
            case Doing.Preparing:
                _hop.CurveStep = _big ? BigHopStep : SmallHopStep;
                _doing = Doing.ChoosingDirection;
                return;
            case Doing.ChoosingDirection:
                // By whole pixels: in its own pixel column Samus counts as on its right.
                Facing = Target.X.Whole < Position.X.Whole ? Facing.Left : Facing.Right;
                _doing = Doing.Starting;
                return;
            case Doing.Starting:
                Play(OnCeiling ? "jumping_ceiling" : "jumping", restart: true);
                _hop.Launch(this, Pixels.FromWhole(_big ? BigHopHeight : SmallHopHeight));
                _doing = Doing.Hopping;
                return;
            case Doing.Hopping:
                if (_hop.Step(this).Finished)
                    _doing = Doing.Landed;
                return;
            case Doing.Landed:
                _ready = false;
                Play(OnCeiling ? "landed_ceiling" : "landed", restart: true);
                _doing = Doing.Waiting;
                return;
            case Doing.Waiting:
                if (!_ready)
                    return;
                _ready = false;
                _doing = Doing.ChoosingHop;
                return;
            default:
                throw new InvalidOperationException($"Undefined hopper stage {_doing}.");
        }
    }
}
