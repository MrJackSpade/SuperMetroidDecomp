using Engine.Actors;
using Engine.Actors.Components;
using Engine.Content;
using Engine.Shared;
using Engine.World;
using Game.Compatibility;
using Game.Data;
using Godot;

namespace Game.Objects;

/// <summary>
/// The eye set in the wall near the Morph Ball. Once Samus has the Morph Ball it opens when she comes near, turns to
/// follow her and plays a beam of light on her, and closes again when she goes. It does nothing else.
/// </summary>
/// <remarks>
/// Its timelines are "closed", "opening" and "closing", each ending in "_left" or "_right" for the way it faces; the
/// closing one plays to its end, the eye shut, however soon the eye is ready to open again. Its data also names
/// "aims", the pictures of it looking each way round from the right, clockwise, and "mounts": for each side its
/// socket can be on ("left", "right", "above", "below"), the socket's "picture" and how far "x" across and "y" down
/// from the eye it sits. Its visual "Mount" shows the socket; its "Beam" is a polygon drawn as its light.
/// </remarks>
public partial class MorphBallEye : RoomObject
{
    /// <summary>The sides of the eye its socket can be on.</summary>
    public enum Side
    {
        /// <summary>To its left.</summary>
        Left,

        /// <summary>To its right.</summary>
        Right,

        /// <summary>Above it.</summary>
        Above,

        /// <summary>Below it.</summary>
        Below,
    }

    /// <summary>The way it faces.</summary>
    [Export]
    public Facing Faces { get; set; } = Facing.Left;

    /// <summary>The side its socket is on.</summary>
    [Export]
    public Side MountedOn { get; set; }

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

    private TimelineSet _timelines = TimelineSet.None;
    private TimelinePlayer? _look;
    private readonly Dictionary<string, (string Picture, Vector2I At)> _mounts = new(StringComparer.Ordinal);
    private readonly List<string> _aims = [];
    private Doing _doing;
    private int _timer;
    private int _angle;
    private Beam _beam;
    private int _halfWidth;
    private int _widthGrowth;
    private int _shade;

    /// <summary>Whether it has taken up the glow of its room's face blocks.</summary>
    private bool _glows;
    private float _light;

    /// <inheritdoc/>
    public override void ReadValues(DataNode data)
    {
        base.ReadValues(data);
        _timelines = TimelineSet.ReadFrom(data);
        _mounts.Clear();
        foreach ((string side, DataNode mount) in data.Get("mounts").Entries())
            _mounts[side] = (mount.Text("picture"), new Vector2I(mount.Int("x"), mount.Int("y")));
        _aims.Clear();
        _aims.AddRange(data.List("aims").Select(picture => picture.AsText()));
        if (_aims.Count == 0)
            throw data.Problem("gives the Morph Ball eye no \"aims\" to look with");
    }

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        (_doing, _beam, _halfWidth, _widthGrowth, _shade, _light) = (Doing.WaitingForSamus, Beam.Off, 0, 0, 0, 0f);
        _glows = false;
        Play("closed");
        string side = MountedOn switch
        {
            Side.Left => "left",
            Side.Right => "right",
            Side.Above => "above",
            Side.Below => "below",
            _ => throw new InvalidOperationException($"The Morph Ball eye has no side {MountedOn}."),
        };
        if (!_mounts.TryGetValue(side, out (string Picture, Vector2I At) socket))
            throw new InvalidOperationException($"The Morph Ball eye's data gives no socket for its {side} side.");
        if (GetNodeOrNull<ActorVisual>("Mount") is { } mount)
        {
            mount.Position = socket.At;
            mount.Show(socket.Picture);
        }
        ShowBeam();
    }

    /// <inheritdoc/>
    protected override void OnStep()
    {
        Player samus = Room.Player;
        StepBeam();

        // It shares the colours of its room's face blocks, and so shows their glow from the update after that starts.
        if (!_glows && Room.Objects.OfType<Enemies.FaceBlockGlow>().FirstOrDefault() is { HasStarted: true } glow)
        {
            foreach (ActorVisual visual in GetChildren().OfType<ActorVisual>())
                glow.StartOn(visual);
            _glows = true;
        }

        if (!samus.Inventory.Owns(Equipment.MorphBall))
        {
            ShowBeam();
            return;
        }

        PixelOffset to = samus.Position - Position;
        switch (_doing)
        {
            case Doing.WaitingForSamus:
                StepLook();
                if (Math.Abs(to.Y.Whole) >= OpensWithin.Y || Math.Abs(to.X.Whole) >= OpensWithin.X)
                    break;
                (_doing, _timer) = (Doing.Opening, TransitionUpdates);
                Play("opening");
                break;

            case Doing.Opening:
                StepLook();
                if (--_timer > 0)
                    break;
                Sound(ActivationSound);
                (_beam, _halfWidth, _widthGrowth, _shade, _light) = (Beam.Widening, 0, 0, 0, 1f);
                _doing = Doing.Open;
                LookAt();
                break;

            case Doing.Open:
                if (Math.Abs(to.Y.Whole) >= StaysOpenWithin.Y || Math.Abs(to.X.Whole) >= StaysOpenWithin.X)
                {
                    Sound(BeamStopSound);
                    if (_beam != Beam.Off)
                        _beam = Beam.Fading;
                    (_doing, _timer) = (Doing.Closing, TransitionUpdates);
                    Play("closing");
                    break;
                }

                LookAt();
                break;

            case Doing.Closing:
                StepLook();
                if (--_timer <= 0)
                    _doing = Doing.WaitingForSamus;
                break;

            default:
                throw new InvalidOperationException($"The Morph Ball eye has no step {_doing}.");
        }

        ShowBeam();
    }

    /// <summary>
    /// Turns it to look along the way to Samus: the cartridge's aim between whole pixels, which counts up from right,
    /// taken backward to count clockwise from right as its angle does.
    /// </summary>
    private void LookAt()
    {
        PixelPosition samus = Room.Player.Position;
        _angle = Turns.Wrap(-CartridgeAngles.AimAngle(samus.X.Whole - Position.X.Whole, samus.Y.Whole - Position.Y.Whole));
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

    /// <summary>How much red and green the light adds at its brightest: level 16 of the cartridge's 31.</summary>
    private const float FullLight = 16f / 31f;

    /// <summary>Shows its light as a wedge from it toward Samus.</summary>
    private void ShowBeam()
    {
        if (GetNodeOrNull<Polygon2D>("Beam") is not { } beam)
            return;
        beam.Visible = _beam != Beam.Off;
        if (!beam.Visible)
            return;

        // The light is added to everything already drawn, the room's front tiles and Samus too, from a line above
        // the eye.
        beam.ZAsRelative = false;
        beam.ZIndex = RoomDepth.Overlay;
        beam.Position = new Vector2(0, -1);
        beam.Material ??= new CanvasItemMaterial { BlendMode = CanvasItemMaterial.BlendModeEnum.Add };
        int half = _halfWidth >> 8;
        Pixels reach = Pixels.FromWhole(512);
        Vector2 Edge(int angle) => new(Turns.Cosine(Turns.Wrap(angle), reach).Whole, Turns.Sine(Turns.Wrap(angle), reach).Whole);
        beam.Polygon = [Vector2.Zero, Edge(_angle - half), Edge(_angle + half)];
        beam.Color = new Color(_light * FullLight, _light * FullLight, 0f);
    }

    /// <summary>Starts the timeline of that name for the way it faces.</summary>
    private void Play(string name)
    {
        _look = new TimelinePlayer(_timelines.Get($"{name}_{(Faces == Facing.Right ? "right" : "left")}"));
        GetNodeOrNull<ActorVisual>("Visual")?.Show(_look.Current);
    }

    /// <summary>Moves the timeline it is showing on by one update.</summary>
    private void StepLook()
    {
        if (_look is null)
            return;
        _look.Step();
        GetNodeOrNull<ActorVisual>("Visual")?.Show(_look.Current);
    }

    private void Show(string picture) => GetNodeOrNull<ActorVisual>("Visual")?.Show(picture);

    private void Sound(string sound)
    {
        if (sound.Length > 0)
            Room.Session.Sounds.Request(sound, channel: "objects");
    }
}
