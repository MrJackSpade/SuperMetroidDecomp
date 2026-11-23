using Engine.Actors;
using Engine.Collision;
using Engine.Content;
using Engine.Shared;
using Engine.World;
using Game.Data;
using Godot;

namespace Game.Enemies;

/// <summary>
/// The glass of Mother Brain's tank. Missiles and super missiles count against it, and so do shots that land on Mother
/// Brain's head inside it. As the count rises it cracks, and at some counts a pane bursts and throws shards; with
/// the last burst it is gone for good, and Mother Brain is open to the room.
/// </summary>
/// <remarks>
/// Its data lists its "stages" in order, each with the "timeline" it shows, the count of "hits" at which it gives way
/// to the next, and, for a stage that bursts, the shards thrown as it bursts ("first") and a few updates after
/// ("second"): each a list of the places shards fly from, across from its own place. It plays those timelines and
/// "shattered". Its shot is "shard".
/// </remarks>
public partial class MotherBrainGlass : Enemy
{
    /// <summary>How far below its place shards fly from, in pixels.</summary>
    [Export, DataValue("shard_drop")]
    public int ShardDrop { get; set; } = 32;

    /// <summary>The sound of a pane bursting, by name.</summary>
    [Export, DataValue("shatter_sound")]
    public string ShatterSound { get; set; } = "";

    private sealed record GlassStage(string Timeline, int Hits, IReadOnlyList<int> First, IReadOnlyList<int> Second);

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
            _stages.Add(new GlassStage(stage.Text("timeline"), stage.Int("hits"), Shards(stage, "first"), Shards(stage, "second")));
        }

        if (_stages.Count == 0)
            throw data.Problem("must list at least one stage");
    }

    /// <inheritdoc/>
    public override HitResult TakeHit(Hit hit, Collider part)
    {
        if (hit.CarriesAny([new HitKind(HitKinds.Missile), new HitKind(HitKinds.SuperMissile)]) && !_shattering)
            _hits++;
        return HitResult.Absorbed;
    }

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        AlwaysActive = true;
        _stage = 0;
        Play(_stages[0].Timeline, restart: true);
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
            Die();
            return;
        }

        if (_bursting)
        {
            // A few updates after a pane bursts, more of it follows.
            _bursting = false;
            Throw(stage.Second);
            Advance();
            return;
        }

        if (_hits < stage.Hits)
            return;
        if (stage.First.Count == 0 && stage.Second.Count == 0)
        {
            Advance();
            return;
        }

        Throw(stage.First);
        _bursting = true;
        _hold = 4;
        Play(_stage + 1 < _stages.Count ? _stages[_stage + 1].Timeline : "shattered", restart: true);
    }

    /// <summary>Moves on to the next stage; past the last, it shatters for good.</summary>
    private void Advance()
    {
        if (++_stage < _stages.Count)
        {
            Play(_stages[_stage].Timeline, restart: true);
            return;
        }

        _stage = _stages.Count - 1;
        _shattering = true;
        _hold = 48;
        Play("shattered", restart: true);
    }

    /// <summary>Throws shards from the places given, each a chance way: mostly up or down, a little across.</summary>
    private void Throw(IReadOnlyList<int> places)
    {
        if (places.Count == 0)
            return;
        Sound(ShatterSound);
        foreach (int across in places)
        {
            int angle = Random.Draw().Part(0, 8);
            var velocity = new PixelVelocity(Turns.Sine(angle, Pixels.FromWhole(1)), -Turns.Cosine(angle, Pixels.FromWhole(4)));
            RandomDraw jitter = Random.Draw();
            Fire("shard", Position + PixelOffset.FromWhole(across + jitter.Part(0, 4) - 8, ShardDrop + jitter.Part(8, 4) - 8), velocity);
        }
    }
}
