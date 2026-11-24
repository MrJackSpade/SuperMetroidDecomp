using Engine.Actors;
using Engine.Shared;
using Engine.World;
using Engine.World.Objects;
using Godot;

namespace Game.Enemies;

/// <summary>
/// A source of Rinkas that shares a set of places with the other sources in its room, as those in Mother Brain's room
/// do. Each Rinka forms at one of the places: the source's own if it is in view and no other source is using it,
/// otherwise the first in view that is free, otherwise the first that is free at all. Told to "stop", it puts its
/// Rinka out and makes no more.
/// </summary>
public partial class RinkaSpawner : Spawner, ITriggerTarget
{
    /// <summary>The places Rinkas may form at, in the room's pixels, in the order they are looked through.</summary>
    [Export]
    public Godot.Collections.Array<Vector2I> Places { get; set; } = [];

    /// <summary>Which of the places is this source's own.</summary>
    [Export]
    public int OwnPlace { get; set; }

    private Rinka? _rinka;
    private bool _stopped;

    /// <summary>The place its Rinka formed at and still holds; below nothing when it holds none.</summary>
    public int HeldPlace { get; private set; } = -1;

    /// <inheritdoc/>
    public void Triggered(string what)
    {
        if (what != "stop")
            throw new ArgumentException($"A Rinka source can be told to \"stop\", not \"{what}\".", nameof(what));
        _stopped = true;
        if (_rinka is { } rinka && IsInstanceValid(rinka))
            rinka.GoOut();
        HeldPlace = -1;
    }

    /// <inheritdoc/>
    protected override void OnStep()
    {
        if (_stopped)
            return;
        base.OnStep();
        if (AliveCount == 0)
            HeldPlace = -1;
    }

    /// <inheritdoc/>
    protected override Enemy CreateEnemy()
    {
        if (Places.Count == 0)
            throw new InvalidOperationException($"The Rinka source \"{Name}\" has no places for its Rinkas to form at.");
        HashSet<int> taken = [.. Room.GetChildren().OfType<RinkaSpawner>().Where(other => other != this).Select(other => other.HeldPlace)];
        PixelRect view = Room.Session.Camera.View;
        bool InView(int place) => view.Contains(Places[place].X, Places[place].Y);
        int chosen = OwnPlace >= 0 && OwnPlace < Places.Count && InView(OwnPlace) && !taken.Contains(OwnPlace) ? OwnPlace : -1;
        if (chosen < 0)
            chosen = Enumerable.Range(0, Places.Count).Where(place => InView(place) && !taken.Contains(place)).DefaultIfEmpty(-1).First();
        if (chosen < 0)
            chosen = Enumerable.Range(0, Places.Count).Where(place => !taken.Contains(place)).DefaultIfEmpty(-1).First();
        if (chosen < 0)
            throw new InvalidOperationException($"The Rinka source \"{Name}\" found every place taken; the room has more sources than places.");
        HeldPlace = chosen;
        Position = PixelPosition.FromWhole(Places[chosen].X, Places[chosen].Y);
        var rinka = base.CreateEnemy() as Rinka
            ?? throw new InvalidOperationException($"The Rinka source \"{Name}\" makes something that is not a Rinka.");
        rinka.Quiet = true;
        _rinka = rinka;
        return rinka;
    }
}
