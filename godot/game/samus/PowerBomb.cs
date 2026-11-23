using Engine.Actors;
using Engine.Collision;
using Engine.Content;
using Engine.Shared;
using Engine.World;
using Godot;

namespace Game;

/// <summary>
/// One of Samus's power bombs: it lies where it was laid until its fuse runs out, and then its blast spreads out from
/// it, faster and faster, as an oval wider than it is tall, until it has passed the edges of the view. The blast's hit
/// lands once on everything it reaches, and on the terrain it passes over.
/// </summary>
public partial class PowerBomb : Projectile, IHasValues
{
    /// <inheritdoc/>
    [Export(PropertyHint.File)]
    public string DataFile { get; set; } = "";

    /// <summary>How many updates the bomb lies there before its blast begins.</summary>
    [Export, DataValue("fuse_updates")]
    public int FuseUpdates { get; set; } = 60;

    /// <summary>How far the blast reaches across as it begins, as a number of pixels.</summary>
    [Export, DataValue("first_reach")]
    public double FirstReach { get; set; } = 4;

    /// <summary>How fast the blast's reach grows as it begins.</summary>
    [Export, DataValue("first_spread")]
    public double FirstSpread { get; set; } = 1;

    /// <summary>How much faster the blast spreads each update.</summary>
    [Export, DataValue("spread_gain")]
    public double SpreadGain { get; set; } = 0.1875;

    /// <summary>The reach across at which the blast is over.</summary>
    [Export, DataValue("last_reach")]
    public int LastReach { get; set; } = 320;

    /// <summary>The blast reaches this many parts up and down…</summary>
    [Export, DataValue("height_parts")]
    public int HeightParts { get; set; } = 3;

    /// <summary>…for every this many parts it reaches across.</summary>
    [Export, DataValue("width_parts")]
    public int WidthParts { get; set; } = 4;

    /// <summary>The colour the blast is drawn in.</summary>
    [Export]
    public Color Colour { get; set; } = new(1f, 1f, 0.6f, 0.6f);

    private readonly HashSet<IHittable> _reached = [];
    private Pixels _reach;
    private Pixels _spread;
    private bool _blasting;

    /// <summary>Whether the blast has begun.</summary>
    public bool IsBlasting => _blasting;

    /// <inheritdoc/>
    public void ReadValues(DataNode data)
    {
    }

    /// <inheritdoc/>
    public override void _ValidateProperty(Godot.Collections.Dictionary property) => DataValues.Validate(this, property);

    /// <inheritdoc/>
    protected override void OnLaunched(Room room) => DataValues.Fill(this, room.Content);

    /// <inheritdoc/>
    protected override void BeforeTravel(Room room)
    {
        if (!_blasting)
        {
            if (Age <= FuseUpdates)
                return;
            _blasting = true;
            _reach = Pixels.FromNumber(FirstReach);
            _spread = Pixels.FromNumber(FirstSpread);
            room.Session.Sounds.Request(Data.Sounds.BigExplosion, channel: "explosions");
        }

        int across = _reach.Whole, down = across * HeightParts / Math.Max(1, WidthParts);
        var blast = PixelRect.Around(Position.X.Whole, Position.Y.Whole, across, down);
        _reached.Add(this);
        foreach ((IHittable target, HitResult _) in room.DeliverHit(Hit, blast, from: Launcher, skipping: _reached))
            _reached.Add(target);
        room.Terrain.TakeHit(Hit, blast);

        _reach += _spread;
        _spread += Pixels.FromNumber(SpreadGain);
        QueueRedraw();
        if (_reach.Whole >= LastReach)
            Leave(room);
    }

    /// <inheritdoc/>
    public override void _Draw()
    {
        if (!_blasting)
            return;
        float across = _reach.ToDisplay(), down = across * HeightParts / Math.Max(1, WidthParts);
        var points = new Vector2[32];
        for (int point = 0; point < points.Length; point++)
        {
            float angle = Mathf.Tau * point / points.Length;
            points[point] = new Vector2(Mathf.Cos(angle) * across, Mathf.Sin(angle) * down);
        }

        DrawColoredPolygon(points, Colour);
    }
}
