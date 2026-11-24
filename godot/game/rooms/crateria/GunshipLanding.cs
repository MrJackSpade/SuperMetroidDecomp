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

    /// <inheritdoc/>
    protected override bool Play(Room room, int elapsed)
    {
        (GetNode(Gunship) as ITriggerTarget
            ?? throw new InvalidOperationException($"The landing \"{Name}\" is to tell \"{Gunship}\" to land, but that is nothing that can be told."))
            .Triggered("land");
        return true;
    }
}
