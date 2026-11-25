using Engine.World;
using Godot;

namespace Game.Rooms.Crateria;

/// <summary>
/// Samus arriving on Zebes in her gunship: as the room is entered that way, the ship is told to come down and land.
/// </summary>
public partial class GunshipLanding : RoomSequence
{
    /// <summary>The gunship.</summary>
    [Export]
    public NodePath Gunship { get; set; } = new();

    /// <summary>The ship is told as the room is set up, so Samus is aboard it before any update of hers is played.</summary>
    protected override void OnStarted() =>
        (GetNode(Gunship) as ITriggerTarget
            ?? throw new InvalidOperationException($"The landing \"{Name}\" is to tell \"{Gunship}\" to land, but that is nothing that can be told."))
            .Triggered("land");

    /// <inheritdoc/>
    protected override bool Play(Room room, int elapsed) => true;
}
