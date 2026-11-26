using Engine.Shared;
using Godot;

namespace Engine.World;

/// <summary>
/// A region that tells one thing to do one thing when the player enters it, optionally only while a condition holds.
/// One can instead fire as the room is entered at a named entry point. It holds no logic of its own.
/// </summary>
[Tool]
public partial class Trigger : Node2D, IPlaced
{
    /// <summary>The region's width and height, in pixels; its top-left is where this node stands.</summary>
    [Export]
    public Vector2I Size { get; set; } = new(16, 16);

    /// <summary>The thing told.</summary>
    [Export]
    public NodePath Target { get; set; } = new();

    /// <summary>What it is told to do.</summary>
    [Export]
    public string What { get; set; } = "";

    /// <summary>
    /// Whether it is the player's own place, their middle, that must come inside the region, and not any part of their
    /// body: for something the cartridge sets off by where the player is.
    /// </summary>
    [Export]
    public bool MeetsMiddle { get; set; }

    /// <summary>The condition that must hold for the trigger to fire; null when it always may.</summary>
    [Export]
    public Condition? OnlyWhen { get; set; }

    /// <summary>Whether it fires only the first time the player enters it after the room is set up.</summary>
    [Export]
    public bool Once { get; set; } = true;

    /// <summary>
    /// The entry point of the room whose use fires it, once, as the room is set up: when everything of the room is
    /// in it and before its first view is settled, so that what it tells holds from the room's first picture, as what
    /// a door of the cartridge does to the room it leads into does. Empty for a trigger the player walks into.
    /// </summary>
    [Export]
    public string AtEntry { get; set; } = "";

    /// <inheritdoc/>
    [Export]
    public Condition? PresentWhen { get; set; }

    /// <summary>Whether the player was inside the region in the update before.</summary>
    private bool _playerWasInside;

    /// <summary>Whether it has fired since the room was set up.</summary>
    private bool _fired;

    /// <summary>The region of the room the trigger covers.</summary>
    public PixelRect Area => Region.Of(this, Size);

    /// <summary>Puts the trigger in its starting state.</summary>
    internal void Setup()
    {
        _playerWasInside = false;
        _fired = false;
    }

    /// <summary>
    /// Fires for the entry point the room was entered by, if it is one that does and it may. Returns whether it fired.
    /// </summary>
    internal bool FireOnArrival(Room room)
    {
        if (AtEntry.Length == 0 || room.Entry?.EntryPoint != AtEntry || (OnlyWhen is not null && !OnlyWhen.IsTrue(room)))
            return false;
        Fire();
        return true;
    }

    /// <summary>Fires if the player has just entered the region and the trigger may fire.</summary>
    internal void Step(Room room, PixelRect playerBody)
    {
        // One that fires for an entry point has fired, or not, as the room was set up.
        if (AtEntry.Length > 0)
            return;
        bool inside = MeetsMiddle ? Area.Contains(room.Player.Position.X.Whole, room.Player.Position.Y.Whole) : Area.Overlaps(playerBody);
        bool entered = inside && !_playerWasInside;
        _playerWasInside = inside;
        if (!entered || (Once && _fired) || (OnlyWhen is not null && !OnlyWhen.IsTrue(room)))
            return;
        Fire();
    }

    /// <summary>Tells its target what it has to tell.</summary>
    private void Fire()
    {
        _fired = true;
        Node target = GetNodeOrNull(Target)
            ?? throw new InvalidOperationException($"The trigger \"{Name}\" names no target that exists.");
        if (target is not ITriggerTarget told)
            throw new InvalidOperationException($"The trigger \"{Name}\" targets \"{target.Name}\", which cannot be told to do anything.");
        told.Triggered(What);
    }

    /// <inheritdoc/>
    public override void _Draw() => Region.DrawInEditor(this, Size, Colors.Cyan);
}
