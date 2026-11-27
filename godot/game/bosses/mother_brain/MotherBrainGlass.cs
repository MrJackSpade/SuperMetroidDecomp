using Engine.Actors;
using Engine.Collision;
using Engine.Content;
using Engine.Shared;
using Engine.World;
using Game.Compatibility;
using Game.Data;
using Godot;

namespace Game.Enemies;

/// <summary>
/// The glass of Mother Brain's tank. Missiles and super missiles that strike it count against it, and so do shots
/// that land on Mother Brain's head inside it. As the count rises it cracks, and at some counts a pane bursts and
/// throws shards; with the last burst it is gone for good, and Mother Brain is open to the room.
/// </summary>
/// <remarks>
/// It stands at the one block of the tank that shots strike, and is that block while it lasts. The rest of the tank
/// is the room's own blocks, which it changes as it breaks: its data lists its "stages" in order, each with the
/// terrain "patch" that shows it, the count of "hits" at which it gives way to the next, and, for a stage that bursts,
/// the shards thrown as it bursts ("first") and a few updates after ("second"), each a list of the places shards fly
/// from, across from its own place. Its shots are "shard_0" and on, one for each arc of the turn a shard may be thrown
/// into, counted clockwise from straight up.
/// </remarks>
public partial class MotherBrainGlass : Enemy
{
    /// <summary>What is recorded once it is gone for good, so that the room is without it from then on.</summary>
    [Export]
    public string RecordsFact { get; set; } = "";

    /// <summary>How far below its place shards fly from, in pixels.</summary>
    [Export, DataValue("shard_drop")]
    public int ShardDrop { get; set; } = 32;

    /// <summary>The sound of a pane bursting, by name.</summary>
    [Export, DataValue("shatter_sound")]
    public string ShatterSound { get; set; } = "";

    /// <summary>How many updates pass between the two throws of shards of one burst.</summary>
    [Export, DataValue("burst_updates")]
    public int BurstUpdates { get; set; } = 4;

    /// <summary>How many updates the shattered tank shows before the glass is gone for good.</summary>
    [Export, DataValue("shatter_updates")]
    public int ShatterUpdates { get; set; } = 48;

    /// <summary>The terrain patch that is the tank with its glass gone.</summary>
    [Export, DataValue("shattered_patch")]
    public string ShatteredPatch { get; set; } = "";

    /// <summary>How many arcs of the turn it has a shard's shot for.</summary>
    [Export, DataValue("shard_arcs")]
    public int ShardArcs { get; set; } = 16;

    private sealed record GlassStage(string Patch, int Hits, IReadOnlyList<int> First, IReadOnlyList<int> Second);

    private readonly List<GlassStage> _stages = [];
    private int _stage;
    private int _hits;
    private int _hold;
    private bool _bursting;
    private bool _shattering;

    /// <summary>Counts a shot that landed on Mother Brain's head inside the tank.</summary>
    public void CountHeadShot() => _hits++;

    /// <inheritdoc/>
    public override void ReadValues(DataNode data)
    {
        base.ReadValues(data);
        _stages.Clear();
        foreach (DataNode stage in data.List("stages"))
        {
            static IReadOnlyList<int> Shards(DataNode stage, string name) =>
                stage.Has(name) ? [.. stage.List(name).Select(shard => shard.AsInt())] : [];
            _stages.Add(new GlassStage(stage.Text("patch"), stage.Int("hits"), Shards(stage, "first"), Shards(stage, "second")));
        }

        if (_stages.Count == 0)
            throw data.Problem("must list at least one stage");
        if (_stages[^1].First.Count == 0 && _stages[^1].Second.Count == 0)
            throw data.Problem("must end with a stage that bursts");
    }

    /// <inheritdoc/>
    public override HitResult TakeHit(Hit hit, Collider part)
    {
        if (hit.CarriesAny([new HitKind(HitKinds.Missile), new HitKind(HitKinds.SuperMissile)]))
            _hits++;
        return HitResult.Absorbed;
    }

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        AlwaysActive = true;
        (_stage, _hits, _hold, _bursting, _shattering) = (0, 0, 0, false, false);
        Room.Terrain.ApplyPatch(_stages[0].Patch);
    }

    /// <inheritdoc/>
    protected override void Touch(Collider part)
    {
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        if (_hold > 0 && --_hold > 0)
            return;
        GlassStage stage = _stages[_stage];
        if (_shattering)
        {
            if (RecordsFact.Length > 0)
                Room.Session.Progress.Record(RecordsFact);
            Vanish();
            return;
        }

        if (_bursting)
        {
            // A few updates after a pane bursts, more of it follows.
            _bursting = false;
            Throw(stage.Second);
            if (++_stage < _stages.Count)
                return;
            _stage = _stages.Count - 1;
            (_shattering, _hold) = (true, ShatterUpdates);
            return;
        }

        if (_hits < stage.Hits)
            return;
        if (stage.First.Count == 0 && stage.Second.Count == 0)
        {
            Room.Terrain.ApplyPatch(_stages[++_stage].Patch);
            return;
        }

        // The pane bursts: what is left of it shows at once, between the two throws of its shards.
        Throw(stage.First);
        (_bursting, _hold) = (true, BurstUpdates);
        Room.Terrain.ApplyPatch(_stage + 1 < _stages.Count ? _stages[_stage + 1].Patch : ShatteredPatch);
    }

    /// <summary>Throws shards from the places given, each a chance way: mostly up or down, a little across.</summary>
    private void Throw(IReadOnlyList<int> places)
    {
        Sound(ShatterSound);
        foreach (int across in places)
        {
            // A shard with no place free in the room is lost, and no number is drawn for it.
            if (!Room.HasFreePlace)
                continue;
            int angle = Random.Draw().Part(0, 8);
            // A pixel an update across by the angle's coarse sine, and four down by its sine a quarter turn back, in
            // 256ths of a pixel.
            var velocity = new PixelVelocity(
                From256ths(CartridgeAngles.SpeedAlong(256, angle)),
                From256ths(CartridgeAngles.SpeedAlong(4 * 256, angle - Turns.PerTurn / 4)));
            int right = Random.Draw().Part(0, 4) - 8, down = Random.Draw().Part(0, 4) - 8;
            Fire($"shard_{angle * ShardArcs / Turns.PerTurn}", Position + PixelOffset.FromWhole(across + right, ShardDrop + down), velocity);
        }
    }

    /// <summary>A distance from a count of 256ths of a pixel.</summary>
    private static Pixels From256ths(int count) => Pixels.FromParts(count >> 8, (count & 255) * 256);
}
