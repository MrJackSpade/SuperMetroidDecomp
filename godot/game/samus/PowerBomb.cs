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
/// enough is hit twice. An enemy is reached by its middle, as the cartridge's sweep of the enemies reaches it; a
/// door or gate only as the border of the reach passes over it, as the cartridge's react to the blocks the blast
/// bombs; anything else by the rectangle of the reach.
/// </summary>
/// <remarks>
/// Its data gives how far the blast reaches across on each of its updates, one after another, from the update after
/// the fuse runs out; nought is an update on which it reaches nothing but the terrain where it lies, as while it
/// gathers. The blast ends on the update after the last of them.
/// <code>{ "fuse_updates": 60, "blast_reaches": [0, 0, 4, 4, 5, 6], "flash_blast_reaches": [4, 4, 5], "height_parts": 3, "width_parts": 4, "rehit_after": 48 }</code>
/// What is seen of a blast is given beside its reaches, update for update: the oval shown ("blast_ovals", how far
/// across, nought for none) and the colour it adds to everything inside it ("blast_glows", as red, green and blue
/// in one number), and the same for a crystal flash's blast ("flash_blast_ovals", "flash_blast_glows"). The ovals
/// are the cartridge's own, close to the blast's reach but not it, and the glow goes on fading after the blast has
/// done growing.
/// A blast that ends where the Samus who laid it lies may start a crystal flash. The bomb is then kept, holding its
/// place among her power bombs, until the flash's rise is done and the flash's own blast begins: that blast is this
/// bomb's too, from where she has risen to, by "flash_blast_reaches", the first of which it reaches on the update it
/// begins. It strikes no terrain and offers no flash.
/// </remarks>
public partial class PowerBomb : Projectile, IHasValues, IOpeningStep, IClosingStep
{
    /// <inheritdoc/>
    [Export(PropertyHint.File)]
    public string DataFile { get; set; } = "";

    /// <summary>A power bomb is not tried against the enemies' shots as her shots are.</summary>
    protected override bool MeetsShots => false;

    /// <summary>How many updates the bomb lies there before its blast begins.</summary>
    [Export, DataValue("fuse_updates")]
    public int FuseUpdates { get; set; } = 60;

    /// <summary>How far the blast reaches across on each of its updates, in pixels.</summary>
    [Export, DataValue("blast_reaches")]
    public int[] BlastReaches { get; set; } = [];

    /// <summary>How far the blast a crystal flash starts reaches across on each of its updates, in pixels, from the update it begins.</summary>
    [Export, DataValue("flash_blast_reaches")]
    public int[] FlashBlastReaches { get; set; } = [];

    /// <summary>The blast reaches this many parts up and down…</summary>
    [Export, DataValue("height_parts")]
    public int HeightParts { get; set; } = 3;

    /// <summary>…for every this many parts it reaches across: to the nearest pixel for what it hits, rounded down for the terrain.</summary>
    [Export, DataValue("width_parts")]
    public int WidthParts { get; set; } = 4;

    /// <summary>How many updates after the blast's hit has landed on something it can land on it again.</summary>
    [Export, DataValue("rehit_after")]
    public int RehitAfter { get; set; } = 48;

    /// <summary>How far across the oval shown on each update of the blast is, in pixels; nought shows none.</summary>
    [Export, DataValue("blast_ovals")]
    public int[] BlastOvals { get; set; } = [];

    /// <summary>The colour the blast adds inside its oval on each of its updates: red, green and blue in one number.</summary>
    [Export, DataValue("blast_glows")]
    public int[] BlastGlows { get; set; } = [];

    /// <summary>How far across the oval shown on each update of a crystal flash's blast is.</summary>
    [Export, DataValue("flash_blast_ovals")]
    public int[] FlashBlastOvals { get; set; } = [];

    /// <summary>The colour a crystal flash's blast adds inside its oval on each of its updates.</summary>
    [Export, DataValue("flash_blast_glows")]
    public int[] FlashBlastGlows { get; set; } = [];

    /// <summary>How many points the oval's outline is drawn with.</summary>
    private const int OvalPoints = 128;

    /// <summary>
    /// How far the blast reaches across, for the rest of an update's sweep of the enemies, once it has killed one that
    /// comes back: the cartridge's sweep keeps its reach across where a death leaves that enemy's mark of coming back,
    /// which is this much, or nothing for an enemy that stays dead.
    /// </summary>
    private const int ReachAfterRespawningKill = 1 << 14;

    private readonly Dictionary<IHittable, int> _reachedAt = [];
    private int _blastUpdate;
    private int _reach;
    private bool _blasting;

    /// <summary>Whether its blast is done and it waits to learn whether a crystal flash takes it.</summary>
    private bool _awaiting;

    /// <summary>Whether the blast it plays is a crystal flash's.</summary>
    private bool _flash;

    /// <summary>Whether the last reach of the blast it plays is done.</summary>
    private bool _over;

    /// <summary>
    /// Whether its blast, or the blast of the crystal flash it started, is under way: from the fuse's end, once the
    /// blast's own sound has been asked for, until its last reach is done. While it waits for a flash's rise no blast
    /// is under way.
    /// </summary>
    public bool IsExploding => _blasting && !_awaiting && !_over;

    /// <inheritdoc/>
    public void ReadValues(DataNode data)
    {
    }

    /// <inheritdoc/>
    public override void _ValidateProperty(Godot.Collections.Dictionary property) => DataValues.Validate(this, property);

    /// <inheritdoc/>
    protected override void OnLaunched(Room room)
    {
        DataValues.Fill(this, room.Content);

        // The blast's oval adds its colour to what lies beneath it, over everything of the room.
        Material = new CanvasItemMaterial { BlendMode = CanvasItemMaterial.BlendModeEnum.Add };
        ZIndex = RoomDepth.Overlay;
    }

    /// <inheritdoc/>
    protected override void BeforeTravel(Room room)
    {
        // Blasting, it strikes the terrain along the border of its reach here, in the shots' own step.
        if (_blasting)
        {
            // A crystal flash's blast strikes no terrain, and a bomb that waits strikes none.
            if (_blastUpdate > 0 && !_flash && !_awaiting)
                StrikeBorder(room, Math.Max(0, _reach));
            return;
        }

        Bomb.ThrowBall(this, FuseUpdates - Age);
        if (Age < FuseUpdates)
            return;

        // The fuse is out: the bomb is gone, and its blast takes its first step as the next update opens.
        // Its own sound is asked for before the blast is under way, which turns every other sound away.
        room.Session.Sounds.Request(Data.Sounds.PowerBombExplosion, channel: "explosions", priority: SoundPriority.Essential);
        _blasting = true;
        StopShowing();
    }

    /// <summary>
    /// Plays one update of the blast as the update opens, before anyone acts: it reaches further, and once its
    /// reaches are done the bomb leaves. Its hit lands on the terrain along the border of its reach in the shots'
    /// own step, and on what it reaches as the update ends, as the cartridge's does.
    /// </summary>
    public void StepOpening(Room room)
    {
        if (!_blasting || _awaiting)
            return;

        int[] reaches = _flash ? FlashBlastReaches : BlastReaches;
        if (_blastUpdate >= reaches.Length)
        {
            // The blast is over before anything is asked for again.
            _over = true;
            _reach = 0;
            QueueRedraw();
            if (Launcher is not Samus samus)
            {
                Leave(room);
                return;
            }

            if (_flash)
            {
                samus.CrystalFlashBlastEnded();
                Leave(room);
                return;
            }

            // The blast's end offers a crystal flash to the Samus who laid it, should she lie exactly where it lay.
            // She says on this update whether one begins: the bomb waits for that, and leaves if none does.
            _awaiting = true;
            samus.PowerBombBlastEnded(this);
            return;
        }

        _reach = reaches[_blastUpdate];
        _blastUpdate++;
        QueueRedraw();
    }

    /// <summary>No crystal flash takes the bomb whose blast is done: it leaves.</summary>
    public void LeaveAfterBlast(Room room)
    {
        ArgumentNullException.ThrowIfNull(room);
        if (_awaiting)
            Leave(room);
    }

    /// <summary>
    /// The crystal flash this bomb's blast started has finished its rise: the flash's own blast begins from
    /// <paramref name="at"/>, reaching its first reach on this update.
    /// </summary>
    /// <exception cref="InvalidOperationException">The bomb does not wait for a crystal flash, or its data gives the flash's blast no reach.</exception>
    public void BeginFlashBlast(PixelPosition at)
    {
        if (!_awaiting || FlashBlastReaches.Length == 0)
            throw new InvalidOperationException("A power bomb begins a crystal flash's blast only while it waits for one, with reaches to give it.");
        Position = at;
        (_awaiting, _flash, _over, _reach, _blastUpdate) = (false, true, false, FlashBlastReaches[0], 1);
        QueueRedraw();
    }

    /// <summary>
    /// As the update ends the blast's hit lands on what it reaches; a blast that has not grown from nothing reaches
    /// nothing.
    /// </summary>
    public void StepClosing(Room room)
    {
        if (!_blasting || _awaiting || _blastUpdate == 0 || _reach <= 0)
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
        Hit hit = Hit with { Quiet = true };

        // The enemies are swept in the reverse of the order they take their turns in. One is reached when its middle
        // lies nearer than the reach both across and up and down, and from then on takes its turns however far from
        // the view it is. A kill changes the reach across for the rest of this update's sweep.
        foreach (Enemy enemy in room.Enemies.Reverse().ToList())
        {
            if (enemy.IsDead || lately.Contains(enemy) || !enemy.IsHitBy(hit.Side))
                continue;
            if (Math.Abs(Position.X.Whole - enemy.Position.X.Whole) >= across || Math.Abs(Position.Y.Whole - enemy.Position.Y.Whole) >= down)
                continue;
            Collider? hurtbox = EntityNode.CollidersOf(enemy).FirstOrDefault(collider => collider.Enabled && collider.Role == ColliderRole.Hurtbox);
            if (hurtbox is null)
                continue;
            HitResult result = enemy.TakeHit(hit, hurtbox);
            if (result == HitResult.Ignored)
                continue;
            _reachedAt[enemy] = _blastUpdate;
            enemy.AlwaysActive = true;
            if (result == HitResult.Destroyed)
                across = enemy.RespawnsWhenKilled ? ReachAfterRespawningKill : 0;
        }

        // What is neither an enemy nor a door or gate is reached by the rectangle of the reach.
        foreach ((IHittable target, HitResult result) in room.DeliverHit(
            hit, blast, from: Launcher, skipping: lately, landsOn: target => target is not (Enemy or Engine.World.Objects.Barrier)))
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
            StrikeTile(room, column, top);
        for (int row = top; row <= bottom; row++)
            StrikeTile(room, left, row);
        for (int column = left; column <= right; column++)
            StrikeTile(room, column, bottom);
        for (int row = top; row <= bottom; row++)
            StrikeTile(room, right, row);
    }

    /// <summary>
    /// Lands the blast's hit on whatever block covers the tile at <paramref name="column"/>, <paramref name="row"/>, and
    /// on a door or gate that stands in that tile, which the cartridge's blast reaches only so.
    /// </summary>
    private void StrikeTile(Room room, int column, int row)
    {
        int tile = room.Terrain.TileSize;
        room.Terrain.TakeHit(Hit, new PixelRect(column * tile, row * tile, 1, 1));
        room.DeliverHit(Hit, new PixelRect(column * tile, row * tile, tile, tile), from: Launcher, landsOn: target => target is Engine.World.Objects.Barrier);
    }

    /// <inheritdoc/>
    public override void _Draw()
    {
        // What is shown is the oval and the colour of the update the blast last played; a bomb that waits through a
        // crystal flash's rise shows nothing.
        int shown = _blastUpdate - 1;
        int[] ovals = _flash ? FlashBlastOvals : BlastOvals, glows = _flash ? FlashBlastGlows : BlastGlows;
        if (!_blasting || _awaiting || _over || shown < 0 || shown >= ovals.Length || shown >= glows.Length || ovals[shown] <= 0)
            return;
        int glow = glows[shown];
        var colour = new Color(((glow >> 16) & 255) / 255f, ((glow >> 8) & 255) / 255f, (glow & 255) / 255f);
        float across = ovals[shown], down = ovals[shown] * HeightParts / Math.Max(1, WidthParts);
        var points = new Vector2[OvalPoints];
        for (int point = 0; point < points.Length; point++)
        {
            float angle = Mathf.Tau * point / points.Length;
            points[point] = new Vector2(Mathf.Cos(angle) * across, Mathf.Sin(angle) * down);
        }

        DrawColoredPolygon(points, colour);
    }
}
