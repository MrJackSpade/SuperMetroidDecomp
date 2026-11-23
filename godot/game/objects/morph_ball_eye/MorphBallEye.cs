using Engine.Actors;
using Engine.Actors.Components;
using Engine.Content;
using Engine.Shared;
using Engine.World;
using Game.Data;
using Godot;

namespace Game.Objects;

/// <summary>
/// The eye set in the wall near the Morph Ball. Once Samus has the Morph Ball it opens when she comes near, turns to
/// follow her and plays a beam of light on her, and closes again when she goes. It does nothing else.
/// </summary>
/// <remarks>
/// Its data names its pictures: "closed", the steps of its "opening" and "closing", each with a "picture" and how
/// many "updates" it shows for, and "aims", the pictures of it looking each way round from the right, clockwise. Its
/// visual "Mount" is the socket it sits in; its "Beam" is a polygon drawn as its light.
/// </remarks>
public partial class MorphBallEye : RoomObject
{
    /// <summary>How near Samus must come for it to open, in pixels across and down.</summary>
    [Export]
    public Vector2I OpensWithin { get; set; } = new(128, 128);

    /// <summary>How near Samus must stay for it to stay open, in pixels across and down.</summary>
    [Export]
    public Vector2I StaysOpenWithin { get; set; } = new(176, 128);

    /// <summary>How many updates it takes to open or close.</summary>
    [Export, DataValue("transition_updates")]
    public int TransitionUpdates { get; set; } = 32;

    /// <summary>The sound of it opening and its beam coming on, by name.</summary>
    [Export, DataValue("activation_sound")]
    public string ActivationSound { get; set; } = "";

    /// <summary>The sound that takes the place of its beam's when the beam goes off.</summary>
    [Export, DataValue("beam_stop_sound")]
    public string BeamStopSound { get; set; } = "";

    private enum Doing
    {
        WaitingForSamus,
        Opening,
        Open,
        Closing,
    }

    private enum Beam
    {
        Off,
        Widening,
        Full,
        Fading,
    }

    /// <summary>How wide the beam is to each side at its fullest, and how much faster it widens each update, in 256ths of a step of angle.</summary>
    private const int FullHalfWidth = 4 * 256, WidthGrowth = 64;

    /// <summary>How many shades its light goes through, down and back up, and how bright the dimmest is.</summary>
    private const int LightShades = 16;

    private string _closed = "";
    private readonly List<(string Picture, int Updates)> _opening = [];
    private readonly List<(string Picture, int Updates)> _closing = [];
    private readonly List<string> _aims = [];
    private Doing _doing;
    private int _timer;
    private int _step;
    private int _stepLeft;
    private int _angle;
    private Beam _beam;
    private int _halfWidth;
    private int _widthGrowth;
    private int _shade;
    private float _light;

    /// <inheritdoc/>
    public override void ReadValues(DataNode data)
    {
        base.ReadValues(data);
        _closed = data.Text("closed");
        foreach ((List<(string, int)> steps, string name) in new[] { (_opening, "opening"), (_closing, "closing") })
        {
            steps.Clear();
            steps.AddRange(data.List(name).Select(step => (step.Text("picture"), step.Int("updates"))));
        }

        _aims.Clear();
        _aims.AddRange(data.List("aims").Select(picture => picture.AsText()));
        if (_aims.Count == 0)
            throw data.Problem("gives the Morph Ball eye no \"aims\" to look with");
    }

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        (_doing, _beam, _halfWidth, _widthGrowth, _shade, _light) = (Doing.WaitingForSamus, Beam.Off, 0, 0, 0, 0f);
        Show(_closed);
        ShowBeam();
    }

    /// <inheritdoc/>
    protected override void OnStep()
    {
        Player samus = Room.Player;
        StepBeam();
        if (!samus.Inventory.Owns(Equipment.MorphBall))
        {
            ShowBeam();
            return;
        }

        PixelOffset to = samus.Position - Position;
        switch (_doing)
        {
            case Doing.WaitingForSamus:
                if (Math.Abs(to.Y.Whole) >= OpensWithin.Y || Math.Abs(to.X.Whole) >= OpensWithin.X)
                    break;
                (_doing, _timer) = (Doing.Opening, TransitionUpdates);
                BeginSteps(_opening);
                break;

            case Doing.Opening:
                StepSteps(_opening);
                if (--_timer > 0)
                    break;
                Sound(ActivationSound);
                (_beam, _halfWidth, _widthGrowth, _shade, _light) = (Beam.Widening, 0, 0, 0, 1f);
                _doing = Doing.Open;
                LookAt(to);
                break;

            case Doing.Open:
                if (Math.Abs(to.Y.Whole) >= StaysOpenWithin.Y || Math.Abs(to.X.Whole) >= StaysOpenWithin.X)
                {
                    Sound(BeamStopSound);
                    if (_beam != Beam.Off)
                        _beam = Beam.Fading;
                    (_doing, _timer) = (Doing.Closing, TransitionUpdates);
                    BeginSteps(_closing);
                    break;
                }

                LookAt(to);
                break;

            case Doing.Closing:
                StepSteps(_closing);
                if (--_timer <= 0)
                    _doing = Doing.WaitingForSamus;
                break;

            default:
                throw new InvalidOperationException($"The Morph Ball eye has no step {_doing}.");
        }

        ShowBeam();
    }

    /// <summary>Turns it to look along the way to Samus.</summary>
    private void LookAt(PixelOffset to)
    {
        _angle = Turns.AngleOf(to.X.Whole, to.Y.Whole);
        Show(_aims[Turns.Wrap(_angle) * _aims.Count / Turns.PerTurn]);
    }

    /// <summary>Its light widens as it comes on, wavers while it is on, and fades when it goes off.</summary>
    private void StepBeam()
    {
        switch (_beam)
        {
            case Beam.Off:
                break;

            case Beam.Widening:
                _widthGrowth += WidthGrowth;
                _halfWidth += _widthGrowth;
                if (_halfWidth >= FullHalfWidth)
                    (_halfWidth, _beam) = (FullHalfWidth, Beam.Full);
                break;

            case Beam.Full:
                // It dims by steps to half and brightens again.
                int along = _shade % LightShades;
                _light = (LightShades - (along <= LightShades / 2 ? along : LightShades - along)) / (float)LightShades;
                _shade++;
                break;

            case Beam.Fading:
                _light -= 1f / LightShades;
                if (_light <= 0f)
                    (_light, _beam, _halfWidth) = (0f, Beam.Off, 0);
                break;

            default:
                throw new InvalidOperationException($"The Morph Ball eye's light has no step {_beam}.");
        }
    }

    /// <summary>Shows its light as a wedge from it toward Samus.</summary>
    private void ShowBeam()
    {
        if (GetNodeOrNull<Polygon2D>("Beam") is not { } beam)
            return;
        beam.Visible = _beam != Beam.Off;
        if (!beam.Visible)
            return;
        int half = _halfWidth >> 8;
        Pixels reach = Pixels.FromWhole(512);
        Vector2 Edge(int angle) => new(Turns.Cosine(Turns.Wrap(angle), reach).Whole, Turns.Sine(Turns.Wrap(angle), reach).Whole);
        beam.Polygon = [Vector2.Zero, Edge(_angle - half), Edge(_angle + half)];
        beam.Color = new Color(_light, _light, 0f, 0.5f);
    }

    private void BeginSteps(List<(string Picture, int Updates)> steps)
    {
        (_step, _stepLeft) = (0, steps.Count > 0 ? steps[0].Updates : 0);
        if (steps.Count > 0)
            Show(steps[0].Picture);
    }

    private void StepSteps(List<(string Picture, int Updates)> steps)
    {
        if (_step >= steps.Count || --_stepLeft > 0)
            return;
        if (++_step >= steps.Count)
            return;
        _stepLeft = steps[_step].Updates;
        Show(steps[_step].Picture);
    }

    private void Show(string picture) => GetNodeOrNull<ActorVisual>("Visual")?.Show(picture);

    private void Sound(string sound)
    {
        if (sound.Length > 0)
            Room.Session.Sounds.Request(sound, channel: "objects");
    }
}
