using Engine.Content;
using Engine.Tests.Core;
using Engine.World;
using Engine.World.Objects;

namespace Engine.Tests.World;

/// <summary>A stand-in game for checks in which every room holds an item remembered by the same name.</summary>
public partial class SharedNameDefinition : StandInDefinition
{
    /// <inheritdoc/>
    public override Room CreateRoom(string scenePath, ContentLibrary content)
    {
        var room = new Room { Name = scenePath };
        room.AddChild(new Item { Name = "Tank", RememberedAs = "the same name" });
        return room;
    }
}
