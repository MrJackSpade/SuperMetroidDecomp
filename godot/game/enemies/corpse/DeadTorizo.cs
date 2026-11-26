using Engine.Actors;
using Engine.Actors.Components;
using Engine.Collision;
using Engine.Content;
using Engine.Shared;
using Engine.World;
using Godot;

namespace Game.Enemies;

/// <summary>
/// The husk of the Torizo that lies before Mother Brain. It is solid, and a great one: Samus brushing any part of it
/// is pushed off and sets it crumbling, as a shot or a bomb does, and as her coming to rest against it does after a
/// moment. As it crumbles it sinks, and a heap of sand gathers where it lay.
/// </summary>
/// <remarks>
/// Its data gives "touch_areas", the parts of it Samus can brush, each with "left", "top", "right" and "bottom" from
/// its middle. Its visual "Sand" is the heap.
/// </remarks>
public partial class DeadTorizo : Enemy
{
    /// <summary>The timeline it lies in.</summary>
    [Export, DataValue("look")]
    public string Look { get; set; } = "corpse";

    /// <summary>How many rows of it there are to crumble: the height of its picture.</summary>
    [Export, DataValue("rows")]
    public int Rows { get; set; } = 96;

    /// <summary>How many updates after Samus comes to rest against it that it starts to crumble.</summary>
    [Export, DataValue("disturbed_updates")]
    public int DisturbedUpdates { get; set; } = 16;

    /// <summary>The effect of the dust it crumbles into, by the data file of its timeline.</summary>
    [Export, DataValue("dust_effect")]
    public string DustEffect { get; set; } = "";

    /// <summary>The sound of it crumbling, by name.</summary>
    [Export, DataValue("dust_sound")]
    public string DustSound { get; set; } = "";

    /// <summary>Where its dust rises from, in the room's pixels; each puff a little further along.</summary>
    [Export]
    public Vector2I DustPlace { get; set; } = new(272, 188);

    /// <summary>How many rows the heap of sand has.</summary>
    [Export, DataValue("sand_rows")]
    public int SandRows { get; set; } = 16;

    /// <summary>How many of them gather.</summary>
    [Export, DataValue("sand_lines")]
    public int SandLines { get; set; } = 15;

    /// <summary>How many updates each takes to gather.</summary>
    [Export, DataValue("sand_line_updates")]
    public int SandLineUpdates { get; set; } = 15;

    private sealed record Area(int Left, int Top, int Right, int Bottom);

    private static readonly Pixels SinkGain = Pixels.FromNumber(1 / 256.0);

    private readonly List<Area> _touchAreas = [];
    private RowCrumble? _crumble;
    private Pixels _sink;
    private int _sandUpdates;
    private float? _sandRest;
    private int _restY;

    /// <inheritdoc/>
    public override void ReadValues(DataNode data)
    {
        base.ReadValues(data);
        _touchAreas.Clear();
        _touchAreas.AddRange(data.List("touch_areas").Select(area => new Area(area.Int("left"), area.Int("top"), area.Int("right"), area.Int("bottom"))));
    }

    /// <inheritdoc/>
    public override HitResult TakeHit(Hit hit, Collider part)
    {
        Crumble();
        return HitResult.Absorbed;
    }

    /// <inheritdoc/>
    protected override void Touch(Collider part) => Crumble();

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        _crumble = null;
        _sink = Pixels.FromNumber(1 / 32.0);
        _sandUpdates = 0;
        _restY = Position.Y.Whole;
        Play(Look);
        ShowSand();
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        if (_crumble is not { HasStarted: true } && BrushedBySamus())
            Crumble();
        if (_crumble is null)
        {
            if (PlayerRestsAgainstIt())
                _crumble = new RowCrumble(Rows, DisturbedUpdates);
            return;
        }

        if (!_crumble.Step(LeaveDust))
            return;
        AlwaysActive = true;
        foreach (Collider part in GetChildren().OfType<Collider>())
            part.Enabled = false;

        // It sinks as it crumbles, and its sand gathers.
        _sink += SinkGain;
        Position += new PixelOffset(Pixels.Zero, _sink);
        if (_sandUpdates < SandLines * SandLineUpdates)
            _sandUpdates++;
        ShowSand();
        Visual?.ShowRows(_crumble.Rows);
        if (_crumble.IsOver)
            Vanish();
    }

    /// <summary>Starts it crumbling at once.</summary>
    private void Crumble()
    {
        _crumble ??= new RowCrumble(Rows);
        _crumble.StartNow();
    }

    /// <summary>
    /// Leaves the puff of dust of a row reaching the ground: one of the enemies' shots on the cartridge, which holds one
    /// of the room's places while it plays and is not made with none free.
    /// </summary>
    private void LeaveDust()
    {
        if (DustEffect.Length > 0)
            Room.Spawn(Effect.Of(FrameTimeline.Read(Room.Content.ReadData(DustEffect)), PixelPosition.FromWhole(DustPlace.X + Random.Last.Part(0, 5), DustPlace.Y), holdsPlace: true));
        if (Room.Session.EnemyPasses % 8 == 0)
            Sound(DustSound);
    }

    /// <summary>Whether Samus is against any part of it; if she is, she is pushed off it.</summary>
    private bool BrushedBySamus()
    {
        Player samus = Room.Player;
        PixelRect body = samus.BodyRect;
        PixelOffset from = samus.Position - Position;
        foreach (Area area in _touchAreas)
        {
            int downOverlap = body.Height / 2 + Math.Abs(from.Y.Whole >= 0 ? area.Bottom : area.Top) - Math.Abs(from.Y.Whole);
            if (downOverlap < 0)
                continue;
            int acrossOverlap = body.Width / 2 + Math.Abs(from.X.Whole >= 0 ? area.Right : area.Left) - Math.Abs(from.X.Whole);
            if (acrossOverlap < 0)
                continue;
            ShiftPlayer(PixelOffset.FromWhole(Math.Max(acrossOverlap, 4), 4));
            return true;
        }

        return false;
    }

    /// <summary>Shows as much of the heap of sand as has gathered, from the bottom up, where the husk lay.</summary>
    private void ShowSand()
    {
        if (GetNodeOrNull<ActorVisual>("Sand") is not { } sand)
            return;
        int lines = _sandUpdates / Math.Max(1, SandLineUpdates);
        sand.ShowRows([.. Enumerable.Range(0, SandRows).Select(row => row >= SandRows - lines ? row : (int?)null)]);
        _sandRest ??= sand.Position.Y;
        sand.Position = new Vector2(sand.Position.X, _sandRest.Value - (Position.Y.Whole - _restY));
    }
}
