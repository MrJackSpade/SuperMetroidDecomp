using Engine.Actors;
using Engine.Actors.Components;
using Engine.Collision;
using Engine.Content;
using Engine.Shared;
using Godot;

namespace Game.Enemies;

/// <summary>
/// The Zebetites: the living barriers before Mother Brain. Only one stands at a time. It mends itself a little every
/// update, so it falls only to a quick run of missiles; when it does, the next one along takes its place, and how
/// many have fallen is remembered for good. Some are a single column, some a pair of stubs above and below that
/// share their health.
/// </summary>
/// <remarks>
/// Its data lists its "barriers" in the order they stand, each with where its middle is ("x", "y"), half its height
/// ("half_height") and whether it is a "pair", whose halves sit "pair_offset" pixels above and below the middle; and
/// "health_pictures" for a single one and "pair_health_pictures" for a pair, each with the least "health" a "picture"
/// is shown from, highest first. Its visuals are "Visual" and "Visual2", its hurtboxes "Hurtbox" and "Hurtbox2", and
/// its solids "Solid" and "Solid2"; the second of each is the lower half of a pair.
/// </remarks>
public partial class Zebetite : Enemy
{
    /// <summary>The fact recorded for each one that has fallen, with its number after it.</summary>
    [Export]
    public string FallenFact { get; set; } = "zebetite_destroyed_";

    /// <summary>How far above and below the middle the halves of a pair sit, in pixels.</summary>
    [Export, DataValue("pair_offset")]
    public int PairOffset { get; set; } = 40;

    /// <summary>The effect of one falling, by the data file of its timeline.</summary>
    [Export, DataValue("fall_effect")]
    public string FallEffect { get; set; } = "";

    /// <summary>The sound of a shot striking it, by name.</summary>
    [Export, DataValue("shot_sound")]
    public string ShotSound { get; set; } = "";

    private sealed record Barrier(int X, int Y, int HalfHeight, bool Pair);

    private readonly List<Barrier> _barriers = [];
    private readonly List<(int Health, string Picture)> _pictures = [];
    private readonly List<(int Health, string Picture)> _pairPictures = [];
    private int _standing;

    /// <inheritdoc/>
    public override void ReadValues(DataNode data)
    {
        base.ReadValues(data);
        _barriers.Clear();
        _barriers.AddRange(data.List("barriers").Select(barrier => new Barrier(barrier.Int("x"), barrier.Int("y"), barrier.Int("half_height"), barrier.Flag("pair"))));
        foreach ((List<(int, string)> pictures, string name) in new[] { (_pictures, "health_pictures"), (_pairPictures, "pair_health_pictures") })
        {
            pictures.Clear();
            pictures.AddRange(data.List(name).Select(picture => (picture.Int("health"), picture.Text("picture"))));
            if (pictures.Count == 0)
                throw data.Problem($"must list at least one of its {name}");
        }
    }

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        _standing = 0;
        while (_standing < _barriers.Count && Room.Session.Progress.Has(FallenFact + _standing))
            _standing++;
        Stand();
    }

    /// <inheritdoc/>
    protected override void OnHurt(Hit hit, Collider part) => Sound(ShotSound);

    /// <inheritdoc/>
    protected override bool DiesAtOnce(Hit hit)
    {
        // It falls, and the next one along stands in its place.
        Sound(ShotSound);
        LeaveFallEffects();
        Room.Session.Progress.Record(FallenFact + _standing);
        _standing++;
        if (_standing >= _barriers.Count)
            return true;
        Health.Restore(MaxHealth);
        Stand();
        return false;
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        if (!Health.IsEmpty)
            Health.Restore(1);
        ShowHealth();
    }

    /// <summary>Stands the barrier whose turn it is where it belongs, or is gone if none is left.</summary>
    private void Stand()
    {
        if (_standing >= _barriers.Count)
        {
            Vanish();
            return;
        }

        Barrier barrier = _barriers[_standing];
        Position = PixelPosition.FromWhole(barrier.X, barrier.Y);
        int offset = barrier.Pair ? PairOffset : 0;
        Shape("Hurtbox", "Solid", "Visual", -offset, barrier.HalfHeight, true);
        Shape("Hurtbox2", "Solid2", "Visual2", offset, barrier.HalfHeight, barrier.Pair);
        ShowHealth();
    }

    /// <summary>Sizes and places one column of the barrier, or puts it away.</summary>
    private void Shape(string hurtbox, string solid, string visual, int offset, int halfHeight, bool used)
    {
        foreach (string name in new[] { hurtbox, solid })
        {
            if (GetNodeOrNull<Collider>(name) is not { } collider)
                continue;
            collider.Enabled = used;
            collider.Rect = new PixelRect(collider.Rect.Left, offset - halfHeight, collider.Rect.Width, halfHeight * 2);
        }

        if (GetNodeOrNull<ActorVisual>(visual) is not { } shown)
            return;
        shown.SetShown(used);
        shown.Position = new Vector2(0, offset);
    }

    /// <summary>Shows the picture for its health.</summary>
    private void ShowHealth()
    {
        if (_standing >= _barriers.Count)
            return;
        List<(int Health, string Picture)> pictures = _barriers[_standing].Pair ? _pairPictures : _pictures;
        string picture = pictures[^1].Picture;
        foreach ((int health, string candidate) in pictures)
        {
            if (Health.Current >= health)
            {
                picture = candidate;
                break;
            }
        }

        foreach (string visual in new[] { "Visual", "Visual2" })
            GetNodeOrNull<ActorVisual>(visual)?.Show(picture);
    }

    /// <summary>Leaves the effect of its fall on each column.</summary>
    private void LeaveFallEffects()
    {
        if (FallEffect.Length == 0)
            return;
        if (!_barriers[_standing].Pair)
        {
            LeaveEffect(FallEffect, Position);
            return;
        }

        LeaveEffect(FallEffect, Position - PixelOffset.FromWhole(0, PairOffset));
        LeaveEffect(FallEffect, Position + PixelOffset.FromWhole(0, PairOffset));
    }
}
