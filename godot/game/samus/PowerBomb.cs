using Engine.Actors;
using Engine.Collision;
using Engine.Content;
using Engine.Core;
using Engine.Shared;
using Engine.World;
using Godot;

namespace Game;

/// <summary>
/// One of Samus's power bombs: it lies where it was laid until its fuse runs out, and then its blast spreads out from
/// it as an oval wider than it is tall. On every update of the blast its hit lands on everything the blast reaches
/// that it has not hit lately, and on the blocks along the border of its reach; so what stays inside a blast long
/// enough is hit twice.
/// </summary>
/// <remarks>
/// Its data gives how far the blast reaches across on each of its updates, one after another, from the update after
/// the fuse runs out; nought is an update on which it reaches nothing but the terrain where it lies, as while it
/// gathers. The blast ends on the update after the last of them.
/// <code>{ "fuse_updates": 60, "blast_reaches": [0, 0, 4, 4, 5, 6], "height_parts": 3, "width_parts": 4, "rehit_after": 48 }</code>
/// </remarks>
public partial class PowerBomb : Projectile, IHasValues, IOpeningStep, IClosingStep
{
    /// <inheritdoc/>
    [Export(PropertyHint.File)]
    public string DataFile { get; set; } = "";

    /// <summary>How many updates the bomb lies there before its blast begins.</summary>
    [Export, DataValue("fuse_updates")]
    public int FuseUpdates { get; set; } = 60;

    /// <summary>How far the blast reaches across on each of its updates, in pixels.</summary>
    [Export, DataValue("blast_reaches")]
    public int[] BlastReaches { get; set; } = [];

    /// <summary>The blast reaches this many parts up and down…</summary>
    [Export, DataValue("height_parts")]
    public int HeightParts { get; set; } = 3;

    /// <summary>…for every this many parts it reaches across: to the nearest pixel for what it hits, rounded down for the terrain.</summary>
    [Export, DataValue("width_parts")]
    public int WidthParts { get; set; } = 4;

    /// <summary>How many updates after the blast's hit has landed on something it can land on it again.</summary>
    [Export, DataValue("rehit_after")]
    public int RehitAfter { get; set; } = 48;

    /// <summary>The colour the blast is drawn in.</summary>
    [Export]
    public Color Colour { get; set; } = new(1f, 1f, 0.6f, 0.6f);

    private readonly Dictionary<IHittable, int> _reachedAt = [];
    private int _blastUpdate;
    private int _reach;
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
        // Blasting, it strikes the terrain along the border of its reach here, in the shots' own step.
        if (_blasting)
        {
            if (_blastUpdate > 0)
                StrikeBorder(room, Math.Max(0, _reach));
            return;
        }

        Bomb.ThrowBall(this, FuseUpdates - Age);
        if (Age < FuseUpdates)
            return;

        // The fuse is out: the bomb is gone, and its blast takes its first step as the next update opens.
        _blasting = true;
        StopShowing();
        room.Session.Sounds.Request(Data.Sounds.PowerBombExplosion, channel: "explosions", priority: SoundPriority.Essential);
    }

    /// <summary>
    /// Plays one update of the blast as the update opens, before anyone acts: it reaches further, and once its
    /// reaches are done the bomb leaves. Its hit lands on the terrain along the border of its reach in the shots'
    /// own step, and on what it reaches as the update ends, as the cartridge's does.
    /// </summary>
    public void StepOpening(Room room)
    {
        if (!_blasting)
            return;

        if (_blastUpdate >= BlastReaches.Length)
        {
            // The blast's end offers a crystal flash to the Samus who laid it, should she lie exactly where it lay.
            if (Launcher is Samus samus)
                samus.PowerBombBlastEnded(Position);
            Leave(room);
            return;
        }

        _reach = BlastReaches[_blastUpdate];
        _blastUpdate++;
        QueueRedraw();
    }

    /// <summary>
    /// As the update ends the blast's hit lands on what it reaches; a blast that has not grown from nothing reaches
    /// nothing.
    /// </summary>
    public void StepClosing(Room room)
    {
        if (!_blasting || _blastUpdate == 0 || _reach <= 0)
            return;
        int parts = Math.Max(1, WidthParts);
        int across = _reach, down = (across * HeightParts + (parts - 1) / 2) / parts;
        var blast = PixelRect.Around(Position.X.Whole, Position.Y.Whole, _reach, down);
        var lately = new HashSet<IHittable> { this };
        foreach ((IHittable reached, int at) in _reachedAt)
        {
            if (_blastUpdate - at < RehitAfter)
                lately.Add(reached);
        }

        // An enemy the blast harms makes no cry, as on the cartridge.
        foreach ((IHittable target, HitResult result) in room.DeliverHit(Hit with { Quiet = true }, blast, from: Launcher, skipping: lately))
        {
            if (result != HitResult.Ignored)
                _reachedAt[target] = _blastUpdate;
        }
    }

    /// <summary>
    /// As the cartridge's blast does on every update it runs, strikes only the blocks on the border of its reach, held
    /// to the room: the top row left to right, the left column top to bottom, the bottom row left to right, the right
    /// column top to bottom, so each corner is struck twice and a reach of nought strikes the block it lies in four
    /// times. Its reach up and down here is its reach across in proportion, rounded down.
    /// </summary>
    private void StrikeBorder(Room room, int across)
    {
        Terrain terrain = room.Terrain;
        int tile = terrain.TileSize, down = across * HeightParts / Math.Max(1, WidthParts);
        int left = Math.Max(0, Position.X.Whole - across) / tile;
        int right = Math.Min(terrain.WidthInPixels / tile - 1, (Position.X.Whole + across) / tile);
        int top = Math.Max(0, Position.Y.Whole - down) / tile;
        int bottom = Math.Min(terrain.HeightInPixels / tile - 1, (Position.Y.Whole + down) / tile);
        for (int column = left; column <= right; column++)
            StrikeTile(terrain, column, top);
        for (int row = top; row <= bottom; row++)
            StrikeTile(terrain, left, row);
        for (int column = left; column <= right; column++)
            StrikeTile(terrain, column, bottom);
        for (int row = top; row <= bottom; row++)
            StrikeTile(terrain, right, row);
    }

    /// <summary>Lands the blast's hit on whatever block covers the tile at <paramref name="column"/>, <paramref name="row"/>.</summary>
    private void StrikeTile(Terrain terrain, int column, int row) =>
        terrain.TakeHit(Hit, new PixelRect(column * terrain.TileSize, row * terrain.TileSize, 1, 1));

    /// <inheritdoc/>
    public override void _Draw()
    {
        if (!_blasting || _reach <= 0)
            return;
        float across = _reach, down = across * HeightParts / Math.Max(1, WidthParts);
        var points = new Vector2[32];
        for (int point = 0; point < points.Length; point++)
        {
            float angle = Mathf.Tau * point / points.Length;
            points[point] = new Vector2(Mathf.Cos(angle) * across, Mathf.Sin(angle) * down);
        }

        DrawColoredPolygon(points, Colour);
    }
}
