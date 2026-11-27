using Engine.Actors;
using Engine.Collision;
using Engine.Content;
using Engine.Shared;
using Engine.World;
using Godot;

namespace Game.Enemies;

/// <summary>
/// A Spark: a crackle of electricity on broken machinery. It hurts to touch and cannot be destroyed. One kind is
/// always live; one flickers on and off; one only drips falling sparks from where it sits.
/// </summary>
/// <remarks>
/// Its timelines are "active", "flicker_on", "flicker_out" and "emitter". The cues "tangible" and "intangible" of the
/// flickering ones switch its touch on and off. Its shot "falling_spark" is the spark it drips, and "spark_drifts" in
/// its data lists the eight speeds across one can fall with.
/// </remarks>
public partial class Spark : Enemy, IShocksGrapple
{
    /// <summary>Her grapple's line touching it takes its own touch's damage from her.</summary>
    public int GrappleShock => ContactDamage;

    /// <summary>What a spark does.</summary>
    public enum Kind
    {
        /// <summary>It is always live.</summary>
        AlwaysLive,

        /// <summary>It flickers on and off.</summary>
        Flickering,

        /// <summary>It drips falling sparks.</summary>
        Dripping,
    }

    /// <summary>What this spark does.</summary>
    [Export]
    public Kind Does { get; set; }

    /// <summary>How many updates each spell of being on, of being off, or between drips lasts.</summary>
    [Export]
    public int SpellUpdates { get; set; } = 64;

    /// <summary>How many more updates a spell of being off lasts than one of being on.</summary>
    [Export, DataValue("extra_off_updates")]
    public int ExtraOffUpdates { get; set; } = 8;

    /// <summary>How far below itself it lets a falling spark go, in pixels.</summary>
    [Export, DataValue("drip_below")]
    public int DripBelow { get; set; } = 8;

    private readonly List<Pixels> _drifts = [];
    private Countdown _spell;
    private bool _on = true;

    /// <inheritdoc/>
    public override void ReadValues(DataNode data)
    {
        base.ReadValues(data);
        _drifts.Clear();
        if (data.Has("spark_drifts"))
            _drifts.AddRange(data.List("spark_drifts").Select(drift => drift.AsPixels()));
    }

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        Play(Does == Kind.Dripping ? "emitter" : "active");
        _spell = Countdown.Of(SpellUpdates);
    }

    /// <inheritdoc/>
    protected override void OnCue(string cue)
    {
        if (cue is not ("tangible" or "intangible"))
            return;
        foreach (Collider collider in EntityNode.CollidersOf(this))
            collider.Enabled = cue == "tangible";
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        if (Does == Kind.AlwaysLive)
            return;
        _spell = _spell.Stepped();
        if (_spell.IsRunning)
            return;
        if (Does == Kind.Dripping)
        {
            // The spark falls with one of eight speeds across, picked by three digits of the number drawn. With no place
            // free in the room no spark falls, and no number is drawn for it.
            if (_drifts.Count != 8)
                throw new InvalidOperationException($"The spark \"{Name}\" needs eight speeds across for its falling sparks, not {_drifts.Count}.");
            if (Room.HasFreePlace)
            {
                Pixels drift = _drifts[Random.Draw().Part(2, 3)];
                Fire("falling_spark", Position + PixelOffset.FromWhole(0, DripBelow), new PixelVelocity(drift, Pixels.Zero));
            }

            _spell = Countdown.Of(SpellUpdates);
            return;
        }

        _on = !_on;
        Play(_on ? "flicker_on" : "flicker_out", restart: true);
        _spell = Countdown.Of(_on ? SpellUpdates : SpellUpdates + ExtraOffUpdates);
    }
}
