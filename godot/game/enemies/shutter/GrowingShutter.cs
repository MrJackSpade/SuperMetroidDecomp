using Engine.Actors;
using Engine.Collision;
using Engine.Content;
using Engine.Shared;
using Engine.World;
using Game.Compatibility;
using Godot;

namespace Game.Enemies;

/// <summary>
/// A shutter that grows out of the wall, a section at a time, until it is four sections long. It waits for a set
/// time or for Samus to come near, and then grows up or down. It is solid, and one growing upward lifts Samus.
/// </summary>
/// <remarks>Its timelines are "quarter", "half", "three_quarters" and "full", one for each length.</remarks>
public partial class GrowingShutter : Enemy
{
    /// <summary>How many updates pass before it grows; zero when it waits for Samus instead.</summary>
    [Export]
    public int WaitUpdates { get; set; }

    /// <summary>How near Samus must come, in pixels across, for one that waits for her.</summary>
    [Export]
    public int WaitsForSamusWithin { get; set; }

    /// <summary>Whether it grows upward, not downward.</summary>
    [Export]
    public bool GrowsUp { get; set; }

    /// <summary>How fast it grows, in pixels an update.</summary>
    [Export]
    public double GrowthSpeed { get; set; } = 1;

    /// <summary>The sound of it starting to grow, by name.</summary>
    [Export, DataValue("activation_sound")]
    public string ActivationSound { get; set; } = "";

    private static readonly string[] Lengths = ["quarter", "half", "three_quarters", "full"];

    /// <summary>How long each section is, how far each starts from the one before, and how far back it sits while the next is still to come, in pixels.</summary>
    private const int SectionLength = 16, SectionSpacing = 8, Inset = 7;

    private bool _growing;
    private int _timer;
    private int _home;
    private int _section;

    /// <inheritdoc/>
    public override HitResult TakeHit(Hit hit, Collider part) => HitResult.Absorbed;

    /// <inheritdoc/>
    protected override void Touch(Collider part)
    {
        // It hurts nothing.
    }

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        (_growing, _timer, _home, _section) = (false, WaitUpdates + 1, Position.Y.Whole, 0);
        ShowSection();
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        if (!_growing)
        {
            bool due = WaitsForSamusWithin > 0
                ? Math.Abs(Room.Player.Position.X.Whole - Position.X.Whole) < WaitsForSamusWithin
                : --_timer <= 0;
            if (!due)
                return;
            _growing = true;
            if (EnemyView.Contains(Room, Position.X.Whole, Position.Y.Whole))
                Sound(ActivationSound);
            return;
        }

        if (_section >= Lengths.Length)
            return;
        int way = GrowsUp ? -1 : 1;
        MoveCarrying(new PixelOffset(Pixels.Zero, Pixels.FromNumber(GrowthSpeed * way)));
        int target = _home + (_section * SectionSpacing + SectionLength) * way;
        bool reached = GrowsUp ? Position.Y.Whole <= target : Position.Y.Whole > target;
        if (!reached)
            return;

        // The section is out: it settles, a little back while there is more to come, and the next begins.
        bool last = _section + 1 >= Lengths.Length;
        Position = new PixelPosition(Position.X, Pixels.FromWhole(last ? target : target - Inset * way));
        _section++;
        if (!last)
            ShowSection();
    }

    /// <summary>Shows it at its present length, and makes it as tall as it shows.</summary>
    private void ShowSection()
    {
        Play(Lengths[_section], restart: true);
        int reach = 8 * (_section + 1);
        foreach (Collider part in GetChildren().OfType<Collider>())
        {
            PixelRect rect = part.Rect;
            part.Rect = new PixelRect(rect.Left, -reach, rect.Width, reach * 2);
        }
    }
}
