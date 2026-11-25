using Engine.Actors.Components;
using Engine.Content;
using Engine.Shared;
using Godot;

namespace Engine.World;

/// <summary>
/// A room-wide condition that may show as pictures laid over the whole view, tiled and drifting: fog, rain, falling
/// spores. Like any room effect it may also do ongoing damage, and one given no look shows nothing: heat. It affects
/// play in no other way.
/// </summary>
/// <remarks>
/// Its look is a data file:
/// <code>
/// { "timeline": { "steps": [ { "picture": "effects/{graphics}/rain_0.png", "updates": 10 } ], "loops_to": 0 },
///   "drift": { "x": 0, "y": 6 }, "winds": [ -6, 6, -4, 4 ], "winds_from": 2, "follows_room": 0, "blend": "add",
///   "behind": false }
/// </code>
/// "timeline" gives its pictures in turn, each tiled across the view. "drift" is how far they move each update, across
/// and down; "winds", when given, are drifts across, a power of two of them, of which one is added to the drift as
/// the room is set up: the one counted by the binary digits of the number the room's chance last drew, starting
/// "winds_from" digits up from its lowest. No number is drawn for it. "follows_room" is how much of the camera's movement moves the pictures on the screen, from none (they
/// stay with the screen) to all (they stay with the room). "blend" is "add" for pictures that brighten what is under
/// them, "subtract" for ones that darken it, and "mix" for ones that cover it. "behind" puts them behind the room's
/// terrain and what stands in it, over only the backdrop.
/// </remarks>
public partial class OverlayEffect : RoomEffect
{
    /// <summary>The data file of its look; empty for an effect that shows nothing.</summary>
    [Export(PropertyHint.File)]
    public string LookFile { get; set; } = "";

    private ContentLibrary? _content;
    private TimelinePlayer? _player;
    private Vector2 _drift;
    private float _followsRoom;
    private Vector2 _drifted;
    private PixelRect _view;

    /// <inheritdoc/>
    protected override void OnSetup(Room room)
    {
        _player = null;
        _drifted = Vector2.Zero;
        if (LookFile.Length == 0)
            return;
        DataNode look = room.Content.ReadData(LookFile);
        _content = room.Content;
        _player = new TimelinePlayer(FrameTimeline.Read(look.Get("timeline")));
        _drift = new Vector2(look.Get("drift").Distance("x").ToDisplay(), look.Get("drift").Distance("y").ToDisplay());
        if (look.Has("winds"))
        {
            IReadOnlyList<DataNode> winds = look.List("winds");
            int digits = System.Numerics.BitOperations.Log2((uint)Math.Max(1, winds.Count));
            if (winds.Count == 0 || 1 << digits != winds.Count)
                throw look.Problem("names winds and lists none, or a number of them that is not a power of two");

            // The wind is read from the number last drawn and draws none: what is only for show never moves the
            // room's chance on.
            _drift.X += winds[room.Session.Random.Last.Part(look.Int("winds_from"), digits)].AsPixels().ToDisplay();
        }

        _followsRoom = look.Distance("follows_room").ToDisplay();
        Material = new CanvasItemMaterial { BlendMode = BlendOf(look, look.Text("blend")) };
        ZIndex = look.Flag("behind") ? RoomDepth.BehindRoom : RoomDepth.Overlay;
    }

    /// <summary>The way of drawing that a look's "blend" names.</summary>
    internal static CanvasItemMaterial.BlendModeEnum BlendOf(DataNode look, string blend) => blend switch
    {
        "add" => CanvasItemMaterial.BlendModeEnum.Add,
        "subtract" => CanvasItemMaterial.BlendModeEnum.Sub,
        "mix" => CanvasItemMaterial.BlendModeEnum.Mix,
        _ => throw look.Problem($"names the blend \"{blend}\", which is none of \"add\", \"subtract\" and \"mix\""),
    };

    /// <inheritdoc/>
    protected override void OnStep(Room room)
    {
        if (_player is null)
            return;
        _player.Step();
        _drifted += _drift;
        _view = room.Session.Camera.View;
        QueueRedraw();
    }

    /// <inheritdoc/>
    public override void _Draw()
    {
        if (_player is null || _content is null || !IsActive || _player.Shown.Picture.Length == 0)
            return;
        Texture2D texture = _content.LoadTexture(_player.Shown.Picture);
        Vector2 size = texture.GetSize();

        // Tiles cover the view from the first one that reaches into it.
        float startX = _view.Left - Wrap(_view.Left * _followsRoom - _drifted.X, size.X);
        float startY = _view.Top - Wrap(_view.Top * _followsRoom - _drifted.Y, size.Y);
        for (float y = startY; y < _view.Top + _view.Height; y += size.Y)
        {
            for (float x = startX; x < _view.Left + _view.Width; x += size.X)
                DrawTexture(texture, ToLocal(new Vector2(x, y)));
        }

        static float Wrap(float value, float period) => ((value % period) + period) % period;
    }
}
